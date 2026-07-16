namespace Application.DTOs;

public record CreatePortfolioDto(string Name);
public record CreateHoldingDto(string Symbol, string Currency);