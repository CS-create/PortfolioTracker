using Application.DTOs;

namespace Application.Services;

public interface IPortfolioService
{
    Task<PortfolioDto> GetPortfolioOverviewAsync(Guid portfolioId);
    Task AddTransactionAsync(CreateTransactionDto dto);
}