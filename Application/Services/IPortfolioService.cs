using Application.DTOs;

namespace Application.Services;

public interface IPortfolioService
{
    Task<PortfolioDto> GetPortfolioOverviewAsync(Guid portfolioId, Guid userId);
    Task<Guid> CreatePortfolioAsync(Guid userId, CreatePortfolioDto dto);
    Task<Guid> AddHoldingAsync(Guid portfolioId, Guid userId, CreateHoldingDto dto);
    Task AddTransactionAsync(Guid portfolioId, Guid userId, CreateTransactionDto dto);
}