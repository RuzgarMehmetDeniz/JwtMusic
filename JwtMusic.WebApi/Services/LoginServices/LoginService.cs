using JwtMusic.WebApi.Dtos;
using JwtMusic.WebApi.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtMusic.WebApi.Services.LoginServices
{
    public class LoginService : ILoginService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;

        public LoginService(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<string> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);

            var result = await _signInManager.PasswordSignInAsync(loginDto.UserName, loginDto.Password, false, false);
            if (result.Succeeded)
            {
                return await GenerateToken(user);
            }
            else
            {
                return "hata";
            }
        }

        private async Task<string> GenerateToken(AppUser appuser)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var roles = await _userManager.GetRolesAsync(appuser);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, appuser.Id),
                new Claim(ClaimTypes.Email, appuser.Email),
                new Claim(ClaimTypes.Name, appuser.Name),
                new Claim(ClaimTypes.Surname, appuser.Surname)
            };

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpireMinutes"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
