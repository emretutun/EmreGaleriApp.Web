namespace EmreGaleriApp.Web.ApiDto
{
    public class CreateOrderRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public CartItemDto[]? CartItems { get; set; }
    }

    public class CartItemDto
    {
        public int CarId { get; set; }
        public decimal DailyPrice { get; set; }
    }
}
