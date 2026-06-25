using JwtMusic.WebApi.Dtos;

namespace JwtMusic.WebApi.Services.LoginServices
{
    public interface ILoginService
    {
        Task<string> LoginAsync(LoginDto loginDto);
    }
}