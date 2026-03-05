namespace Cd.Cms.Application.DTOs.Users
{
    public class UserSearchRequest
    {
        public string? Keyword { get; set; }
        public string? Role { get; set; }
        public string? Department { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
