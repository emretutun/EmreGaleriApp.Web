using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class UserEditViewModel
{
    [Display(Name = "Kimlik No")]
    public string? NationalId { get; set; }

    [Display(Name = "Cinsiyet")]
    public string? Gender { get; set; }

    [Display(Name = "Doğum Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    [Display(Name = "Sürüş Deneyimi (Yıl)")]
    public int? DrivingExperienceYears { get; set; }

    public string? PictureUrl { get; set; }

    [Display(Name = "Fotoğraf")]
    public IFormFile? Picture { get; set; }

    public List<SelectListItem> GenderOptions { get; } = new List<SelectListItem>
    {
        new SelectListItem("Belirtilmemiş", ""),
        new SelectListItem("Erkek", "Male"),
        new SelectListItem("Kadın", "Female"),
        new SelectListItem("Diğer", "Other")
    };

    // Yeni eklenenler:
    public List<LicenseTypeViewModel> LicenseTypes { get; set; } = new List<LicenseTypeViewModel>();

    // Kullanıcının seçtiği ehliyet türleri için Id listesi
    [Display(Name = "Ehliyet Türleri")]
    public List<int> SelectedLicenseTypeIds { get; set; } = new List<int>();
    public string? Email { get; internal set; }
}

public class LicenseTypeViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
