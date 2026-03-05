using Cd.Cms.Application.Common.Auth;
using Cd.Cms.Application.Contracts.Repositories;
using Cd.Cms.Application.Contracts.Services;
using Cd.Cms.Application.DTOs.Users;
using Cd.Cms.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Cd.Cms.Application.Services
{
    public sealed class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly JwtSettings _jwt;
        private readonly PasswordHasher<User> _hasher = new();
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        private static readonly ConcurrentDictionary<long, FailedAttemptState> FailedAttempts = new();
        private static readonly ConcurrentDictionary<string, PasswordResetState> PasswordResets = new(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, SessionState> Sessions = new(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<long, TwoFactorState> TwoFactorStates = new();

        public AuthService(IUserRepository users, IOptions<JwtSettings> jwtOptions)
        {
            _users = users;
            _jwt = jwtOptions.Value;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
        {
            if (request == null)
                throw new ArgumentException("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.EmailOrUsername))
                throw new ArgumentException("Email or username is required.");
            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password is required.");

            var user = await _users.GetAuthUserByEmailOrUsernameAsync(request.EmailOrUsername)
                ?? throw new UnauthorizedAccessException("Invalid credentials.");

            if (!user.IsActive)
                throw new InvalidOperationException("Account is inactive.");
            if (user.IsLocked)
                throw new InvalidOperationException("Account is locked.");

            if (IsTemporarilyLocked(user.Id, out var lockoutEndsAtUtc))
                throw new InvalidOperationException($"Account is temporarily locked until {lockoutEndsAtUtc:O}.");

            if (!IsPasswordValid(user.PasswordHash, request.Password))
            {
                RegisterFailedAttempt(user.Id);
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            ClearFailedAttempts(user.Id);

            if (IsTwoFactorEnabled(user.Id))
            {
                if (string.IsNullOrWhiteSpace(request.TwoFactorCode))
                    throw new InvalidOperationException("Two-factor code is required.");

                if (!ValidateTwoFactorCode(user.Id, request.TwoFactorCode))
                    throw new UnauthorizedAccessException("Invalid two-factor code.");
            }

            var session = CreateSession(user.Id, request.DeviceId);
            return GenerateAuthResponse(user, session.SessionId);
        }

        public async Task<ClientEmailCheckResponseDto> CheckClientEmailAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");

            var normalizedEmail = email.Trim();
            var user = await _users.GetAuthUserByEmailOrUsernameAsync(normalizedEmail);
            var exists = user != null && user.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase);

            return new ClientEmailCheckResponseDto
            {
                Email = normalizedEmail,
                EmailExists = exists,
                RequiresPassword = exists,
                RequiresRegistration = !exists
            };
        }

        public async Task<AuthResponseDto> RegisterClientAsync(ClientRegisterRequestDto request, CancellationToken ct = default)
        {
            if (request == null)
                throw new ArgumentException("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name is required.");
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required.");
            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password is required.");
            if (request.Password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters.");
            if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
                throw new ArgumentException("Passwords do not match.");

            EnsurePasswordComplexity(request.Password);

            var normalizedEmail = request.Email.Trim();
            var existing = await _users.GetAuthUserByEmailOrUsernameAsync(normalizedEmail);
            if (existing != null && existing.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Email is already registered.");

            var username = await GenerateUniqueClientUsernameAsync(normalizedEmail);
            var hashedPassword = _hasher.HashPassword(new User(), request.Password);

            UserDto created;
            try
            {
                created = await _users.CreateAsync(new CreateUserRequest
                {
                    Name = request.Name.Trim(),
                    Email = normalizedEmail,
                    Username = username,
                    PhoneNumber = request.PhoneNumber?.Trim() ?? string.Empty,
                    Password = hashedPassword,
                    Role = "Client"
                }, actorUserId: 0);
            }
            catch (Exception ex) when (ex.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            var session = CreateSession(created.Id, "client-registration");
            return GenerateAuthResponse(new AuthUserDto
            {
                Id = created.Id,
                Name = created.Name,
                Email = created.Email,
                Username = created.Username,
                Role = created.Role,
                IsActive = created.IsActive,
                IsLocked = created.IsLocked,
                PasswordHash = hashedPassword
            }, session.SessionId);
        }

        public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken ct = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required.");

            var normalizedEmail = request.Email.Trim();
            var user = await _users.GetAuthUserByEmailOrUsernameAsync(normalizedEmail);
            if (user == null || !string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                return new ForgotPasswordResponseDto
                {
                    RequestAccepted = true,
                    Message = "If the account exists, reset instructions were generated."
                };
            }

            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
            var expiresAt = DateTime.UtcNow.AddMinutes(20);
            PasswordResets[normalizedEmail] = new PasswordResetState
            {
                UserId = user.Id,
                Token = token,
                ExpiresAtUtc = expiresAt
            };

            return new ForgotPasswordResponseDto
            {
                RequestAccepted = true,
                Message = "Reset token generated.",
                DemoResetToken = token,
                ExpiresAtUtc = expiresAt
            };
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken ct = default)
        {
            if (request == null) throw new ArgumentException("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Email is required.");
            if (string.IsNullOrWhiteSpace(request.ResetToken)) throw new ArgumentException("Reset token is required.");
            if (string.IsNullOrWhiteSpace(request.NewPassword)) throw new ArgumentException("New password is required.");
            if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
                throw new ArgumentException("Passwords do not match.");
            if (request.NewPassword.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters.");

            EnsurePasswordComplexity(request.NewPassword);

            var normalizedEmail = request.Email.Trim();
            if (!PasswordResets.TryGetValue(normalizedEmail, out var resetState))
                throw new InvalidOperationException("Reset token is invalid or expired.");
            if (!string.Equals(resetState.Token, request.ResetToken, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Reset token is invalid.");
            if (resetState.ExpiresAtUtc < DateTime.UtcNow)
                throw new InvalidOperationException("Reset token has expired.");

            var hash = _hasher.HashPassword(new User(), request.NewPassword);
            await _users.ChangePasswordAsync(resetState.UserId, hash, resetState.UserId);
            PasswordResets.TryRemove(normalizedEmail, out _);
            ClearFailedAttempts(resetState.UserId);
        }

        public Task<TwoFactorSetupResponseDto> BeginTwoFactorSetupAsync(long userId, CancellationToken ct = default)
        {
            var secret = Convert.ToHexString(RandomNumberGenerator.GetBytes(10));
            TwoFactorStates[userId] = new TwoFactorState { Secret = secret, Enabled = false };
            var demoCode = BuildTwoFactorCode(secret);
            return Task.FromResult(new TwoFactorSetupResponseDto
            {
                Secret = secret,
                QrCodeUri = $"otpauth://totp/CMS:{userId}?secret={secret}&issuer=CMS",
                DemoVerificationCode = demoCode
            });
        }

        public Task EnableTwoFactorAsync(long userId, EnableTwoFactorRequestDto request, CancellationToken ct = default)
        {
            if (!TwoFactorStates.TryGetValue(userId, out var state))
                throw new InvalidOperationException("Two-factor setup not initialized.");
            if (string.IsNullOrWhiteSpace(request.VerificationCode))
                throw new ArgumentException("Verification code is required.");

            var expected = BuildTwoFactorCode(state.Secret);
            if (!string.Equals(expected, request.VerificationCode, StringComparison.Ordinal))
                throw new UnauthorizedAccessException("Invalid verification code.");

            state.Enabled = true;
            TwoFactorStates[userId] = state;
            return Task.CompletedTask;
        }

        public Task<List<UserSessionDto>> GetSessionsAsync(long userId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var sessions = Sessions.Values
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.LastSeenAtUtc)
                .Select(s => new UserSessionDto
                {
                    SessionId = s.SessionId,
                    DeviceId = s.DeviceId,
                    IssuedAtUtc = s.IssuedAtUtc,
                    LastSeenAtUtc = s.LastSeenAtUtc,
                    ExpiresAtUtc = s.ExpiresAtUtc,
                    IsActive = s.RevokedAtUtc == null && s.ExpiresAtUtc > now
                })
                .ToList();
            return Task.FromResult(sessions);
        }

        public Task RevokeSessionAsync(long userId, string sessionId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session id is required.");

            if (!Sessions.TryGetValue(sessionId, out var state) || state.UserId != userId)
                throw new InvalidOperationException("Session not found.");

            state.RevokedAtUtc = DateTime.UtcNow;
            Sessions[sessionId] = state;
            return Task.CompletedTask;
        }

        private static string BuildTwoFactorCode(string secret)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
            var number = BitConverter.ToInt32(bytes, 0);
            var positive = Math.Abs(number % 1000000);
            return positive.ToString("D6");
        }

        private bool IsPasswordValid(string storedPasswordHash, string providedPassword)
        {
            if (string.IsNullOrWhiteSpace(storedPasswordHash)) return false;

            try
            {
                var passwordCheck = _hasher.VerifyHashedPassword(new User(), storedPasswordHash, providedPassword);
                return passwordCheck != PasswordVerificationResult.Failed;
            }
            catch (FormatException)
            {
                return string.Equals(storedPasswordHash, providedPassword, StringComparison.Ordinal);
            }
        }

        private static void EnsurePasswordComplexity(string password)
        {
            if (!Regex.IsMatch(password, "[A-Z]") ||
                !Regex.IsMatch(password, "[a-z]") ||
                !Regex.IsMatch(password, "[0-9]"))
            {
                throw new ArgumentException("Password must include uppercase, lowercase, and number.");
            }
        }

        private bool IsTemporarilyLocked(long userId, out DateTime lockedUntil)
        {
            lockedUntil = DateTime.MinValue;
            if (!FailedAttempts.TryGetValue(userId, out var state)) return false;
            if (state.LockedUntilUtc == null || state.LockedUntilUtc.Value <= DateTime.UtcNow) return false;
            lockedUntil = state.LockedUntilUtc.Value;
            return true;
        }

        private static void RegisterFailedAttempt(long userId)
        {
            var state = FailedAttempts.GetOrAdd(userId, _ => new FailedAttemptState());
            state.Count += 1;
            state.LastAttemptUtc = DateTime.UtcNow;
            if (state.Count >= MaxFailedAttempts)
                state.LockedUntilUtc = DateTime.UtcNow.Add(LockoutDuration);
            FailedAttempts[userId] = state;
        }

        private static void ClearFailedAttempts(long userId)
        {
            FailedAttempts.TryRemove(userId, out _);
        }

        private static bool IsTwoFactorEnabled(long userId)
        {
            return TwoFactorStates.TryGetValue(userId, out var state) && state.Enabled;
        }

        private static bool ValidateTwoFactorCode(long userId, string code)
        {
            if (!TwoFactorStates.TryGetValue(userId, out var state)) return false;
            return string.Equals(BuildTwoFactorCode(state.Secret), code, StringComparison.Ordinal);
        }

        private SessionState CreateSession(long userId, string? deviceId)
        {
            var now = DateTime.UtcNow;
            var session = new SessionState
            {
                SessionId = Guid.NewGuid().ToString("N"),
                UserId = userId,
                DeviceId = string.IsNullOrWhiteSpace(deviceId) ? "unknown-device" : deviceId.Trim(),
                IssuedAtUtc = now,
                LastSeenAtUtc = now,
                ExpiresAtUtc = now.AddMinutes(Math.Max(_jwt.AccessTokenMinutes, 30))
            };
            Sessions[session.SessionId] = session;
            return session;
        }

        private async Task<string> GenerateUniqueClientUsernameAsync(string email)
        {
            var baseUsername = email.Split('@')[0].Trim();
            if (string.IsNullOrWhiteSpace(baseUsername)) baseUsername = "client";

            var cleaned = new string(baseUsername.Where(c => char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '-').ToArray());
            if (string.IsNullOrWhiteSpace(cleaned)) cleaned = "client";

            var candidate = cleaned;
            var suffix = 1;
            while (await _users.GetByEmailOrUsernameAsync(candidate) != null)
            {
                candidate = $"{cleaned}{suffix}";
                suffix++;
            }

            return candidate;
        }

        private AuthResponseDto GenerateAuthResponse(AuthUserDto user, string sessionId)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new("username", user.Username),
                new(ClaimTypes.Role, user.Role),
                new("uid", user.Id.ToString()),
                new("sid", sessionId),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds);

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Username = user.Username,
                FullName = user.Name,
                Role = user.Role,
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAtUtc = expiresAt,
                SessionId = sessionId,
                TwoFactorEnabled = IsTwoFactorEnabled(user.Id)
            };
        }

        private sealed class FailedAttemptState
        {
            public int Count { get; set; }
            public DateTime LastAttemptUtc { get; set; }
            public DateTime? LockedUntilUtc { get; set; }
        }

        private sealed class PasswordResetState
        {
            public long UserId { get; set; }
            public string Token { get; set; } = string.Empty;
            public DateTime ExpiresAtUtc { get; set; }
        }

        private sealed class TwoFactorState
        {
            public string Secret { get; set; } = string.Empty;
            public bool Enabled { get; set; }
        }

        private sealed class SessionState
        {
            public string SessionId { get; set; } = string.Empty;
            public long UserId { get; set; }
            public string DeviceId { get; set; } = string.Empty;
            public DateTime IssuedAtUtc { get; set; }
            public DateTime LastSeenAtUtc { get; set; }
            public DateTime ExpiresAtUtc { get; set; }
            public DateTime? RevokedAtUtc { get; set; }
        }
    }
}
