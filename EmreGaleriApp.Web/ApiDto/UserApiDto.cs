namespace EmreGaleriApp.Web.ApiDto
{
    public class AssignRolesDto
    {
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class UserUpdateDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public int? Experience { get; set; }
        public string? Picture { get; set; }
    }
}
