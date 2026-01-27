namespace TTCCashRegister.Data.Import;

public record BookingJournalRowDto()
{
    public DateOnly Date { get; init; }
    public int DocumentNumber { get; init; }
    public string? Description { get; init; }
    public decimal Sum { get; init; }
    public decimal AccountMovement { get; init; }
    public string CostCenterName { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
}