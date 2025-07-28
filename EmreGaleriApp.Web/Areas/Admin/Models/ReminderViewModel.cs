using System.ComponentModel.DataAnnotations;

namespace EmreGaleriApp.Web.Areas.Admin.Models
{
    public class ReminderViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur.")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Tarih seçilmelidir.")]
        [DataType(DataType.DateTime)]
        public DateTime ReminderDate { get; set; }

        public bool IsCompleted { get; set; }
    }
}
