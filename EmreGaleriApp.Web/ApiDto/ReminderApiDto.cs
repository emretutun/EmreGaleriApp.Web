namespace EmreGaleriApp.Web.ApiDto
{
    public class ReminderDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime ReminderDate { get; set; }

        public bool IsCompleted { get; set; } = false;
    }
}
