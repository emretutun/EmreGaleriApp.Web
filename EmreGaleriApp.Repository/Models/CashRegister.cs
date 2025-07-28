using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmreGaleriApp.Repository.Models
{
    public class CashRegister
    {
        public int Id { get; set; }

        [Required]
        public decimal Amount { get; set; } // + gelir, - gider

        [Required]
        public string Type { get; set; } = null!; // "Gelir", "Gider"

        [Required]
        [StringLength(300)]
        public string Description { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Yapan kullanıcı
        public string? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public AppUser? CreatedByUser { get; set; }

        // İlgili işlem tipi ve ID (Order, Stock vs.)
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
    }
}
