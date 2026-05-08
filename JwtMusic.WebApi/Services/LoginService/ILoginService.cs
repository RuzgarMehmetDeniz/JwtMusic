using JwtMusic.WebApi.Dtos;

namespace JwtMusic.WebApi.Services.LoginService
{
    public interface ILoginService
    {
        Task<string> LoginAsync(LoginDto loginDto);
    }
}
