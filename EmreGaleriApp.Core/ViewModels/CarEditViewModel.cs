using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EmreGaleriApp.Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmreGaleriApp.Core.ViewModels
{
    public class CarEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public string? Brand { get; set; }

        [Required]
        public string? Model { get; set; }

        [Required]
        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal DailyPrice { get; set; }

        [Required]
        public string? FuelType { get; set; }

        [Required]
        public string? Color { get; set; }

        [Required]
        [Range(1900, 2100)]
        public int ModelYear { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Mileage { get; set; }

        public string? ImageUrl { get; set; }

        // Yeni fotoğraf yüklenecekse buraya gelir
        public IFormFile? ImageFile { get; set; }

        // ----------------- EKLENDİ --------------------

        // Seçilen ehliyet türlerinin Id’leri
        public List<int> SelectedLicenseTypeIds { get; set; } = new();

        // Checkbox listesini doldurmak için kullanılacak
        public List<SelectListItem> LicenseTypeList { get; set; } = new();

        public GearType? GearType { get; set; }
    }
}
