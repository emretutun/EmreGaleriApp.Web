using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EmreGaleriApp.Repository.Models
{
    public class Reminder
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime ReminderDate { get; set; }

        public bool IsCompleted { get; set; } = false;

        [BindNever]
        [Required]
        public string AppUserId { get; set; } = null!;

        [ForeignKey("AppUserId")]
        public AppUser? AppUser { get; set; }
    }
}
