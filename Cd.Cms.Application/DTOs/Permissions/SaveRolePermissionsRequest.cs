namespace Cd.Cms.Application.DTOs.Permissions
{
    public sealed class SaveRolePermissionsRequest
    {
        public string Role { get; set; } = string.Empty;
        public List<RolePermissionDto> Permissions { get; set; } = new();
    }
}
