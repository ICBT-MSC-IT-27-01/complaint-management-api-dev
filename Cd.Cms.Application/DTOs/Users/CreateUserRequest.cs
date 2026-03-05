namespace Cd.Cms.Application.DTOs.Users
{
    public class CreateUserRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Agent";
        public string? Department { get; set; }
        public long? ReportingManagerId { get; set; }
    }
}
