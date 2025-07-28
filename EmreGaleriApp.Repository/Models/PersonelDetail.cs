using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EmreGaleriApp.Repository.Models
{
    public class PersonelDetail
    {
        public int Id { get; set; }

        [Required]
        public string? UserId { get; set; }

        [Required]
        public string? Position { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Salary { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [ValidateNever]
        public AppUser? User { get; set; }

    }
}
