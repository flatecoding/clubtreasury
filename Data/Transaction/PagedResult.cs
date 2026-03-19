namespace ClubTreasury.Data.Transaction;

public record PagedResult<T>
{
    public required IEnumerable<T> Items { get; init; }
    public int TotalItems { get; init; }
}