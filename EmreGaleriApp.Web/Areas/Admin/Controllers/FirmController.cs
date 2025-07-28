using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EmreGaleriApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Yonetici,Yetkili")]
    public class FirmController : Controller
    {
        private readonly AppDbContext _context;

        public FirmController(AppDbContext context)
        {
            _context = context;
        }

        // Firma Listesi
        public async Task<IActionResult> Index()
        {


            var firms = await _context.Firms.ToListAsync();
            return View(firms);
        }

        // Detay
        public async Task<IActionResult> Details(int id)
        {

            var firm = await _context.Firms.FindAsync(id);
            if (firm == null) return NotFound();
            return View(firm);
        }

        // Yeni Firma Formu
        public IActionResult Create()
        {
            return View();
        }

        // Yeni Firma Kaydet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Firm firm)
        {
            if (ModelState.IsValid)
            {
                _context.Firms.Add(firm);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(firm);
        }

        // Firma Düzenle Formu
        public async Task<IActionResult> Edit(int id)
        {


            var firm = await _context.Firms.FindAsync(id);
            if (firm == null) return NotFound();
            return View(firm);
        }

        // Firma Güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Firm firm)
        {
            if (id != firm.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                _context.Update(firm);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(firm);
        }

        // Silme Onay Sayfası
        public async Task<IActionResult> Delete(int id)
        {
            var firm = await _context.Firms.FindAsync(id);
            if (firm == null) return NotFound();
            return View(firm);
        }

        // Silme İşlemi
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var firm = await _context.Firms.FindAsync(id);
            if (firm != null)
            {
                _context.Firms.Remove(firm);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
