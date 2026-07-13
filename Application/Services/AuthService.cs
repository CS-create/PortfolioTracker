using Application.DTOs;
using Application.Interfaces;
using PortfolioTracker.Domain.Entities;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthDtos.AuthReponseDto> RegisterAsync(AuthDtos.RegisterDto dto)
    {
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Email already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var token = _jwtTokenGenerator.GenerateToken(user);
        return new AuthDtos.AuthReponseDto(token, user.Email);
    }

    public async Task<AuthDtos.AuthReponseDto> LoginAsync(AuthDtos.LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email)
                   ?? throw new UnauthorizedAccessException("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        var token = _jwtTokenGenerator.GenerateToken(user);
        return new AuthDtos.AuthReponseDto(token, user.Email);
    }
}