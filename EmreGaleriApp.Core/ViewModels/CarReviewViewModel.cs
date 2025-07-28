using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmreGaleriApp.Core.ViewModels
{
    public class CarReviewViewModel
    {
        public int OrderId { get; set; }
        public int CarId { get; set; }

        [Required(ErrorMessage = "Puan zorunludur.")]
        [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Yorum maksimum 1000 karakter olabilir.")]
        public string? Comment { get; set; }
    }

}
