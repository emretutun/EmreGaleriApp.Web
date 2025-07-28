namespace EmreGaleriApp.Web.ApiDto
{
    // DTO tanımı
    public class CashRegisterDto
    {
        public int Id { get; set; }
        public double Amount { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public string? CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
    }
}
