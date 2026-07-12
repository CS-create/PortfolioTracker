using Application.DTOs;

namespace Application.Services;

public interface IAuthService
{
    Task<AuthDtos.AuthReponseDto> LoginAsync(AuthDtos.LoginDto dto);
    Task<AuthDtos.AuthReponseDto> RegisterAsync(AuthDtos.RegisterDto dto);
}