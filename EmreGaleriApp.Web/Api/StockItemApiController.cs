using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EmreGaleriApp.Web.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Yonetici,Yetkili")]
    public class StockItemApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICashRegisterService _cashRegisterService;
        private readonly UserManager<AppUser> _userManager;

        public StockItemApiController(AppDbContext context, ICashRegisterService cashRegisterService, UserManager<AppUser> userManager)
        {
            _context = context;
            _cashRegisterService = cashRegisterService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var stockItems = await _context.StockItems
                .Include(s => s.Firm)
                .Select(s => new
                {
                    s.Id,
                    s.ProductName,
                    s.Quantity,
                    s.PurchasePrice,
                    s.SalePrice,
                    Firm = s.Firm == null ? null : new { s.Firm.Id, s.Firm.Name }
                })
                .ToListAsync();

            return Ok(stockItems);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var stockItem = await _context.StockItems
                .Include(s => s.Firm)
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.ProductName,
                    s.Quantity,
                    s.PurchasePrice,
                    s.SalePrice,
                    Firm = s.Firm == null ? null : new { s.Firm.Id, s.Firm.Name }
                })
                .FirstOrDefaultAsync();

            if (stockItem == null)
                return NotFound();

            return Ok(stockItem);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StockItem stockItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.StockItems.Add(stockItem);
            await _context.SaveChangesAsync();

            try
            {
                // 🔴 sub üzerinden kullanıcı ID alınıyor
                var userId = User.FindFirstValue("sub");

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Kullanıcı ID bulunamadı (sub).");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Unauthorized("Kullanıcı bulunamadı.");

                var userName = user.UserName ?? "Bilinmeyen";

                var amount = stockItem.PurchasePrice * stockItem.Quantity;

                var cashRegisterEntry = new CashRegister
                {
                    Amount = -amount,
                    Type = "Gider",
                    Description = $"Stok alımı: {stockItem.ProductName} ({stockItem.Quantity} adet) | Yapan: {userName}",
                    CreatedAt = System.DateTime.Now,
                    CreatedByUserId = userId,
                    RelatedEntityType = "StockItem",
                    RelatedEntityId = stockItem.Id
                };

                await _cashRegisterService.AddTransactionAsync(cashRegisterEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kasa kaydı sırasında hata oluştu: " + ex.Message);
            }

            return CreatedAtAction(nameof(GetById), new { id = stockItem.Id }, stockItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] StockItem stockItem)
        {
            if (id != stockItem.Id)
                return BadRequest("ID eşleşmiyor.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingItem = await _context.StockItems.FindAsync(id);
            if (existingItem == null)
                return NotFound();

            existingItem.ProductName = stockItem.ProductName;
            existingItem.PurchasePrice = stockItem.PurchasePrice;
            existingItem.SalePrice = stockItem.SalePrice;
            existingItem.Quantity = stockItem.Quantity;
            existingItem.FirmId = stockItem.FirmId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var stockItem = await _context.StockItems.FindAsync(id);
            if (stockItem == null)
                return NotFound();

            _context.StockItems.Remove(stockItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
