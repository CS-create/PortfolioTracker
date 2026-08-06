using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Moq;
using PortfolioTracker.Domain.Entities;
using Xunit;

namespace Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_userRepository.Object, _jwtTokenGenerator.Object);
    }

    [Fact]
    public async Task RegisterAsync_NewEmail_CreatesUserAndReturnsToken()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("new@test.dk"))
            .ReturnsAsync((User?)null);
        _jwtTokenGenerator.Setup(g => g.GenerateToken(It.IsAny<User>()))
            .Returns("fake-jwt-token");

        var dto = new AuthDtos.RegisterDto("new@test.dk", "Password123!");

        var result = await _sut.RegisterAsync(dto);

        Assert.Equal("fake-jwt-token", result.Token);
        Assert.Equal("new@test.dk", result.Email);

        _userRepository.Verify(r => r.AddAsync(It.Is<User>(
            u => u.Email == "new@test.dk" && u.PasswordHash != "Password123!")), Times.Once);
        _userRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_PasswordGetsHashed_NotStoredInPlainText()
    {
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _jwtTokenGenerator.Setup(g => g.GenerateToken(It.IsAny<User>()))
            .Returns("token");

        User? capturedUser = null;
        _userRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .Returns(Task.CompletedTask);

        var dto = new AuthDtos.RegisterDto("test@test.dk", "MySecretPassword");

        await _sut.RegisterAsync(dto);

        Assert.NotNull(capturedUser);
        Assert.NotEqual("MySecretPassword", capturedUser!.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("MySecretPassword", capturedUser.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_ThrowsInvalidOperation()
    {
        var existingUser = new User { Id = Guid.NewGuid(), Email = "taken@test.dk", PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
        _userRepository.Setup(r => r.GetByEmailAsync("taken@test.dk"))
            .ReturnsAsync(existingUser);

        var dto = new AuthDtos.RegisterDto("taken@test.dk", "Password123!");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.RegisterAsync(dto));

        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_CorrectPassword_ReturnsToken()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.dk", PasswordHash = hashedPassword, CreatedAt = DateTime.UtcNow };

        _userRepository.Setup(r => r.GetByEmailAsync("test@test.dk")).ReturnsAsync(user);
        _jwtTokenGenerator.Setup(g => g.GenerateToken(user)).Returns("valid-token");

        var dto = new AuthDtos.LoginDto("test@test.dk", "CorrectPassword");

        var result = await _sut.LoginAsync(dto);

        Assert.Equal("valid-token", result.Token);
        Assert.Equal("test@test.dk", result.Email);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");
        var user = new User { Id = Guid.NewGuid(), Email = "test@test.dk", PasswordHash = hashedPassword, CreatedAt = DateTime.UtcNow };

        _userRepository.Setup(r => r.GetByEmailAsync("test@test.dk")).ReturnsAsync(user);

        var dto = new AuthDtos.LoginDto("test@test.dk", "WrongPassword");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.LoginAsync(dto));

        _jwtTokenGenerator.Verify(g => g.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_EmailDoesNotExist_ThrowsUnauthorized()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("nobody@test.dk"))
            .ReturnsAsync((User?)null);

        var dto = new AuthDtos.LoginDto("nobody@test.dk", "AnyPassword");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.LoginAsync(dto));
    }
}