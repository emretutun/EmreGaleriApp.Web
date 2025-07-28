using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using EmreGaleriApp.Web.ApiDto;

namespace EmreGaleriApp.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Yonetici", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CashRegisterApiController : ControllerBase
    {
        private readonly ICashRegisterService _cashService;
        private readonly UserManager<AppUser> _userManager;

        public CashRegisterApiController(ICashRegisterService cashService, UserManager<AppUser> userManager)
        {
            _cashService = cashService;
            _userManager = userManager;
        }



        // GET: api/cashregisterapi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _cashService.GetAllTransactionsAsync();
            var balance = await _cashService.GetCurrentBalanceAsync();

            var transactionDtos = new List<CashRegisterDto>();
            foreach (var t in transactions)
            {
                var user = await _userManager.FindByIdAsync(t.CreatedByUserId!);
                transactionDtos.Add(new CashRegisterDto
                {
                    Id = t.Id,
                    Amount = (double)t.Amount,       // Burada cast yapıldı
                    Type = t.Type,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt,
                    CreatedByUserId = t.CreatedByUserId!,
                    CreatedByUserName = user != null ? user.UserName! : "Bilinmiyor"
                });
            }

            return Ok(new
            {
                balance,
                transactions = transactionDtos
            });
        }

        // GET: api/cashregisterapi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var transaction = await _cashService.GetByIdAsync(id);
            if (transaction == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(transaction.CreatedByUserId!);

            var dto = new CashRegisterDto
            {
                Id = transaction.Id,
                Amount = (double)transaction.Amount,
                Type = transaction.Type,
                Description = transaction.Description,
                CreatedAt = transaction.CreatedAt,
                CreatedByUserId = transaction.CreatedByUserId,
                CreatedByUserName = user != null ? user.UserName! : "Bilinmiyor"
            };

            return Ok(dto);
        }

        // POST: api/cashregisterapi
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CashRegister model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kullanıcı ID'sini önce NameIdentifier'dan al, yoksa sub'dan dene
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(adminId))
                return Unauthorized("Kullanıcı bilgisi alınamadı.");

            var user = await _userManager.FindByIdAsync(adminId);
            if (user == null)
                return Unauthorized("Kullanıcı bulunamadı.");

            if (model.Type == "Gider" && model.Amount > 0)
                model.Amount *= -1;

            model.CreatedByUserId = adminId;
            model.CreatedAt = DateTime.Now;

            await _cashService.AddTransactionAsync(model);

            var result = new
            {
                model.Id,
                Amount = (double)model.Amount,
                model.Type,
                model.Description,
                model.CreatedAt,
                model.CreatedByUserId,
                CreatedByUserName = user.UserName
            };

            return Ok(result);
        }


        // PUT: api/cashregisterapi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CashRegister model)
        {
            var existing = await _cashService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            existing.Amount = model.Amount;
            existing.Type = model.Type;
            existing.Description = model.Description;
            existing.CreatedAt = model.CreatedAt;

            await _cashService.UpdateAsync(existing);
            return Ok(existing);
        }

        // DELETE: api/cashregisterapi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _cashService.GetByIdAsync(id);
            if (transaction == null)
                return NotFound();

            await _cashService.DeleteAsync(transaction);
            return Ok();
        }
    }
}
