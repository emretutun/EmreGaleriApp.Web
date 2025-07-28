using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmreGaleriApp.Repository.Models
{
    public class CarReview
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int CarId { get; set; }
        public Car Car { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;

        public int Rating { get; set; }
        public string Comment { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

    }


}
