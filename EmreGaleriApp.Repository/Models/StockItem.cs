using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmreGaleriApp.Repository.Models
{
    public class StockItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(150)]
        public string ProductName { get; set; } = null!;  // Ürün adı

        [Range(0, double.MaxValue, ErrorMessage = "Alış fiyatı sıfır veya pozitif olmalı.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; }  // Alış fiyatı

        [Range(0, double.MaxValue, ErrorMessage = "Satış fiyatı sıfır veya pozitif olmalı.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalePrice { get; set; }  // Satış fiyatı

        [Range(0, int.MaxValue, ErrorMessage = "Adet sıfır veya pozitif olmalı.")]
        public int Quantity { get; set; }  // Stoktaki adet

        [Required(ErrorMessage = "Firma seçimi zorunludur.")]
        public int FirmId { get; set; }  // Firma foreign key

        public Firm? Firm { get; set; }  // Navigation Property
    }
}
