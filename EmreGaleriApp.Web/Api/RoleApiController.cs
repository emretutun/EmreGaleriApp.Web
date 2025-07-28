using EmreGaleriApp.Repository.Models;
using EmreGaleriApp.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EmreGaleriApp.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Yonetici", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RoleApiController : ControllerBase
    {
        private readonly RoleManager<AppRole> _roleManager;

        public RoleApiController(RoleManager<AppRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles.Select(r => new { r.Id, r.Name }).ToList();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] RoleAddViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = await _roleManager.RoleExistsAsync(model.Name!);
            if (exists)
                return BadRequest("Bu rol zaten var.");

            var result = await _roleManager.CreateAsync(new AppRole { Name = model.Name });

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Rol başarıyla eklendi.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] RoleUpdateViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            role.Name = model.Name;
            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Rol güncellendi.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Rol silindi.");
        }
    }
}
