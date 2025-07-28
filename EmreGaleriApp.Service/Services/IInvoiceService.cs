using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmreGaleriApp.Repository.Models;

namespace EmreGaleriApp.Service.Services
{
    public interface IInvoiceService
    {
        byte[] GenerateInvoicePdf(Order order);
    }
}