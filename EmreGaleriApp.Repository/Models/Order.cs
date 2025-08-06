using EmreGaleriApp.Core.Enums;
using System;
using System.Collections.Generic;

namespace EmreGaleriApp.Repository.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string AppUserId { get; set; } = null!;
        public AppUser AppUser { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Beklemede";

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.None;


    }


}
