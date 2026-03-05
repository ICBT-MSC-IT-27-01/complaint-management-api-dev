namespace Cd.Cms.Application.DTOs.Users
{
    public class UserDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public string Department { get; set; } = string.Empty;
        public long? ReportingManagerId { get; set; }
        public string ReportingManagerName { get; set; } = string.Empty;
        public DateTime CreatedDateTime { get; set; }
        public DateTime? LastLoginDateTime { get; set; }
    }
}
