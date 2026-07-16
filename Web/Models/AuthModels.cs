namespace Web.Models;

public record LoginViewModel(string Email, string Password);
public record RegisterViewModel(string Email, string Password);

public record AuthApiResponse(string Token, string Email);