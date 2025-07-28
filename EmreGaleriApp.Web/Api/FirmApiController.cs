using EmreGaleriApp.Repository;
using EmreGaleriApp.Repository.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EmreGaleriApp.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Yonetici,Yetkili")]
    public class FirmApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FirmApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/FirmApi
        [HttpGet]
        public async Task<IActionResult> GetFirms()
        {
            var firms = await _context.Firms.ToListAsync();
            return Ok(firms);
        }

        // GET: api/FirmApi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFirm(int id)
        {
            var firm = await _context.Firms.FindAsync(id);
            if (firm == null) return NotFound();
            return Ok(firm);
        }

        // POST: api/FirmApi
        [HttpPost]
        public async Task<IActionResult> CreateFirm(Firm firm)
        {
            _context.Firms.Add(firm);
            await _context.SaveChangesAsync();
            return Ok(firm);
        }

        // PUT: api/FirmApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFirm(int id, Firm firm)
        {
            if (id != firm.Id) return BadRequest();

            _context.Entry(firm).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(firm);
        }

        // DELETE: api/FirmApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFirm(int id)
        {
            var firm = await _context.Firms.FindAsync(id);
            if (firm == null) return NotFound();

            _context.Firms.Remove(firm);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
