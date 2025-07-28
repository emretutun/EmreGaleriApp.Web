using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace EmreGaleriApp.Web.Areas.Admin.Models
{
    public class UserRolesViewModel
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }

        // Sistemdeki tüm roller
        public List<IdentityRole>? Roles { get; set; }

        // Kullanıcının sahip olduğu roller
        public List<string>? UserRoles { get; set; }
    }
}
