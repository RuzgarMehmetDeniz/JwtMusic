using JwtMusic.WebApi.Dtos;
using JwtMusic.WebApi.Entities;
using Microsoft.AspNetCore.Identity;

namespace JwtMusic.WebApi.Services.RegisterServices
{
    public class RegisterService : IRegisterService
    {
        private readonly UserManager<AppUser> _userManager;

        public RegisterService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            AppUser appUser = new AppUser
            {
                Name = dto.Name,
                Surname = dto.Surname,
                UserName = dto.UserName,
                Email = dto.Email
            };
            var result =await _userManager.CreateAsync(appUser, dto.Password);
            return result.Succeeded;
        }
    }
}
