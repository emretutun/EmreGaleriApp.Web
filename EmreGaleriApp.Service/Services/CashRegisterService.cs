using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmreGaleriApp.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmreGaleriApp.Service.Services
{
    public class CashRegisterService : ICashRegisterService
    {
        private readonly AppDbContext _context;

        public CashRegisterService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddTransactionAsync(CashRegister transaction)
        {
            _context.CashRegisters.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CashRegister>> GetAllTransactionsAsync()
        {
            return await _context.CashRegisters
                .Include(x => x.CreatedByUser)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetCurrentBalanceAsync()
        {
            return await _context.CashRegisters.SumAsync(x => x.Amount);
        }

        public async Task UpdateAsync(CashRegister entity)
        {
            _context.CashRegisters.Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(CashRegister entity)
        {
            _context.CashRegisters.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<CashRegister?> GetByIdAsync(int id)
        {
            return await _context.CashRegisters.FindAsync(id);
        }


    }
}
