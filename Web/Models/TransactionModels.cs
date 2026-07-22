namespace Web.Models;

public record CreateTransactionForm(
    Guid HoldingId,
    int Type,
    decimal Quantity,
    decimal PricePerUnit,
    DateTime ExecutedAt);