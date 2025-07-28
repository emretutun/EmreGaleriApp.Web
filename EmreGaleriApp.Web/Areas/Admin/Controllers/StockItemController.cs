using EmreGaleriApp.Repository;
using EmreGaleriApp.Repository.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using EmreGaleriApp.Service.Services;
using System.Security.Claims;
using System;

namespace EmreGaleriApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Yonetici,Yetkili")]
    public class StockItemController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ICashRegisterService _cashRegisterService;

        public StockItemController(AppDbContext context, ICashRegisterService cashRegisterService)
        {
            _context = context;
            _cashRegisterService = cashRegisterService;
        }

        // Listeleme
        public async Task<IActionResult> Index()
        {
            var stockItems = await _context.StockItems.Include(s => s.Firm).ToListAsync();
            return View(stockItems);
        }

        // Detay
        public async Task<IActionResult> Details(int id)
        {
            var stockItem = await _context.StockItems
                .Include(s => s.Firm)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stockItem == null) return NotFound();

            return View(stockItem);
        }

        // Create GET
        public IActionResult Create()
        {
            ViewData["FirmId"] = new SelectList(_context.Firms, "Id", "Name");
            return View();
        }

        // Create POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StockItem stockItem)
        {
            if (ModelState.IsValid)
            {
                _context.StockItems.Add(stockItem);
                await _context.SaveChangesAsync();

                // Kasa hareketi oluştur (adet ile fiyat çarpımı eklendi)
                var transaction = new CashRegister
                {
                    Amount = -stockItem.PurchasePrice * stockItem.Quantity,
                    Type = "Gider",
                    Description = $"Stok alımı - Ürün: {stockItem.ProductName}, Adet: {stockItem.Quantity}",
                    CreatedAt = DateTime.Now,
                    CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    RelatedEntityType = "Stock",
                    RelatedEntityId = stockItem.Id
                };

                await _cashRegisterService.AddTransactionAsync(transaction);

                return RedirectToAction(nameof(Index));
            }
            ViewData["FirmId"] = new SelectList(_context.Firms, "Id", "Name", stockItem.FirmId);
            return View(stockItem);
        }

        // Edit GET
        public async Task<IActionResult> Edit(int id)
        {
            var stockItem = await _context.StockItems.FindAsync(id);
            if (stockItem == null) return NotFound();

            ViewData["FirmId"] = new SelectList(_context.Firms, "Id", "Name", stockItem.FirmId);
            return View(stockItem);
        }

        // Edit POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StockItem stockItem)
        {
            if (id != stockItem.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                _context.Update(stockItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FirmId"] = new SelectList(_context.Firms, "Id", "Name", stockItem.FirmId);
            return View(stockItem);
        }

        // Delete GET
        public async Task<IActionResult> Delete(int id)
        {
            var stockItem = await _context.StockItems.Include(s => s.Firm).FirstOrDefaultAsync(s => s.Id == id);
            if (stockItem == null) return NotFound();

            return View(stockItem);
        }

        // Delete POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stockItem = await _context.StockItems.FindAsync(id);
            if (stockItem != null)
            {
                _context.StockItems.Remove(stockItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
