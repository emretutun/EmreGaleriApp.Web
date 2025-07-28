using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmreGaleriApp.Repository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmreGaleriApp.Service.Services
{
    public interface ICashRegisterService
    {
        Task AddTransactionAsync(CashRegister transaction);
        Task<List<CashRegister>> GetAllTransactionsAsync();
        Task<decimal> GetCurrentBalanceAsync();

        Task UpdateAsync(CashRegister entity);
        Task DeleteAsync(CashRegister entity);
        Task<CashRegister?> GetByIdAsync(int id);



    }
}
