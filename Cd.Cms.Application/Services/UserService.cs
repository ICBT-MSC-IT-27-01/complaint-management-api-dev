using Cd.Cms.Application.Contracts.Repositories;
using Cd.Cms.Application.Contracts.Services;
using Cd.Cms.Application.DTOs.Users;
using Cd.Cms.Domain;
using Cd.Cms.Shared;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace Cd.Cms.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly PasswordHasher<User> _hasher = new();

        public UserService(IUserRepository repo) => _repo = repo;

        public Task<UserDto?> GetByIdAsync(long id) => _repo.GetByIdAsync(id);
        public Task<PagedResult<UserDto>> SearchAsync(UserSearchRequest request) => _repo.SearchAsync(request);
        public Task<List<UserDto>> GetAgentsAsync() => _repo.GetAgentsAsync();

        public Task<UserDto> CreateAsync(CreateUserRequest request, long actorUserId)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Name is required.");
            if (string.IsNullOrWhiteSpace(request.Email)) throw new ArgumentException("Email is required.");
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters.");
            EnsurePasswordComplexity(request.Password);

            // Hash password before storing
            var tempUser = new User();
            request.Password = _hasher.HashPassword(tempUser, request.Password);
            return _repo.CreateAsync(request, actorUserId);
        }

        public Task UpdateAsync(long id, UpdateUserRequest request, long actorUserId) => _repo.UpdateAsync(id, request, actorUserId);
        public Task DeleteAsync(long id, long actorUserId) => _repo.DeleteAsync(id, actorUserId);

        public Task ChangePasswordAsync(long id, ChangePasswordRequest request, long actorUserId)
        {
            if (request.NewPassword != request.ConfirmPassword)
                throw new ArgumentException("Passwords do not match.");
            if (request.NewPassword.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters.");
            EnsurePasswordComplexity(request.NewPassword);

            var tempUser = new User();
            var hash = _hasher.HashPassword(tempUser, request.NewPassword);
            return _repo.ChangePasswordAsync(id, hash, actorUserId);
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
    }
}
