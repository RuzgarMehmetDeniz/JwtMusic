using JwtMusic.WebApi.Dtos;

namespace JwtMusic.WebApi.Services.RegisterServices
{
    public interface IRegisterService
    {
        Task<bool> RegisterAsync(RegisterDto dto);
    }
}
