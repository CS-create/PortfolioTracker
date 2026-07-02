using PortfolioTracker.Domain.Entities;

namespace Application.DTOs;

public record CreateTransactionDto(
    Guid HoldingId,
    TransactionType Type,
    decimal Quantity,
    decimal PricePerUnit,
    DateTime ExecutedAt
);