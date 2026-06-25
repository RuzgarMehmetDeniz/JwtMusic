using JwtMusic.WebApi.Dtos;
using JwtMusic.WebApi.Services.LoginServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JwtMusic.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }
        [HttpPost]
        public async Task<IActionResult> UserLogin(LoginDto loginDto)
        {
            var token = await _loginService.LoginAsync(loginDto);

            // Eğer servis "hata" dönüyorsa, kullanıcıya 401 (Yetkisiz) dönelim
            if (token == "hata" || string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı!" });
            }

            return Ok(new { token });
        }
    }
}