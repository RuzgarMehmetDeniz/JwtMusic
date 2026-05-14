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
        [HttpPost]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            IdentityRole identityRole = new IdentityRole()
            {
                Name = roleName
            };

            await _roleManager.CreateAsync(identityRole);

            return Ok("Başarılı");
        }

    }
}
