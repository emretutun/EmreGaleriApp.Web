namespace EmreGaleriApp.Web.ApiDto
{
    public class PersonelUpdateDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string Position { get; set; } = null!;
        public decimal Salary { get; set; }
        public DateTime StartDate { get; set; }
    }
}
