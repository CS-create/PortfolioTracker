namespace Application.DTOs;

public class AuthDtos
{
    public record RegisterDto(string Email, string Password);
    public record LoginDto(string Email, string Password);
    public record AuthReponseDto(string Token, string Email);
}