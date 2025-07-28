using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EmreGaleriApp.Repository.Models
{
    public class Firm
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;  // Firma adı

        [StringLength(250)]
        public string Address { get; set; } = null!;  // Adres

        [StringLength(100)]
        public string ContactPerson { get; set; } = null!;  // Yetkili kişi

        [StringLength(50)]
        public string Phone { get; set; } = null!;  // Telefon

        [StringLength(100)]
        public string Email { get; set; } = null!;  // E-posta

        public string? Notes { get; set; }  // Ekstra notlar

        // Firma ile ilişkili stoklar
        public ICollection<StockItem>? StockItems { get; set; }
    }
}
