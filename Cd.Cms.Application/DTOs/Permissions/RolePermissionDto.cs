namespace Cd.Cms.Application.DTOs.Permissions
{
    public sealed class RolePermissionDto
    {
        public string Role { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string PermissionLevel { get; set; } = "None";
    }
}
