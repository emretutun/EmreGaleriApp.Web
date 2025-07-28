namespace EmreGaleriApp.Web.Areas.Admin.Models
{
    public class AssignRolesViewModel
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }

        // Tüm roller
        public List<RoleCheckboxItem>? Roles { get; set; }
    }

    public class RoleCheckboxItem
    {
        public string? RoleId { get; set; }
        public string? RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}
