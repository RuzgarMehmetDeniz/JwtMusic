using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JwtMusic.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        // Tüm rolleri listele
        [HttpGet]
        public IActionResult GetAllRoles()
        {
            var roles = _roleManager.Roles
                .Select(r => new { r.Id, r.Name })
                .ToList();
            return Ok(roles);
        }

        // ID'ye gore tekil rol getir
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
                return NotFound("Rol bulunamadi.");

            return Ok(new { role.Id, role.Name });
        }

        // Yeni rol ekle
        [HttpPost]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest("Rol adi bos olamaz.");

            var exists = await _roleManager.RoleExistsAsync(roleName);
            if (exists)
                return BadRequest("Bu rol adi zaten mevcut.");

            var identityRole = new IdentityRole
            {
                Name = roleName
            };

            var result = await _roleManager.CreateAsync(identityRole);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Basarili");
        }

        // Rol guncelle
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(string id, string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest("Rol adi bos olamaz.");

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound("Rol bulunamadi.");

            role.Name = roleName;

            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Rol guncellendi.");
        }

        // Rol sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound("Rol bulunamadi.");

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Rol silindi.");
        }
    }
}