using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EmreGaleriApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Yonetici")]
    public class CashRegisterController : Controller
    {
        private readonly ICashRegisterService _cashService;

        public CashRegisterController(ICashRegisterService cashService)
        {
            _cashService = cashService;
        }

        public async Task<IActionResult> Index()
        {
            var transactions = await _cashService.GetAllTransactionsAsync();
            var balance = await _cashService.GetCurrentBalanceAsync();

            ViewBag.Balance = balance;
            return View(transactions);
        }

        [HttpGet]
        public IActionResult AddTransaction()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddTransaction(CashRegister model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Gider seçildiyse miktarı negatif yap
            if (model.Type == "Gider" && model.Amount > 0)
            {
                model.Amount *= -1;
            }

            model.CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.CreatedAt = DateTime.Now;

            await _cashService.AddTransactionAsync(model);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var transaction = await _cashService.GetByIdAsync(id);
            if (transaction == null)
                return NotFound();

            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CashRegister model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existing = await _cashService.GetByIdAsync(model.Id);
            if (existing == null)
                return NotFound();

            existing.Amount = model.Amount;
            existing.Type = model.Type;
            existing.Description = model.Description;
            existing.CreatedAt = model.CreatedAt;

            await _cashService.UpdateAsync(existing);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _cashService.GetByIdAsync(id);
            if (transaction == null)
                return NotFound();

            return View(transaction);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _cashService.GetByIdAsync(id);
            if (transaction == null)
                return NotFound();

            await _cashService.DeleteAsync(transaction);

            return RedirectToAction(nameof(Index));
        }


    }
}
