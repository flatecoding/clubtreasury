namespace ClubTreasury.Data.Transaction;

public record PagedRequest
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public string? SortLabel { get; init; }
    public SortDirection SortDirection { get; init; }
    public DateTime? DateStart { get; init; }
    public DateTime? DateEnd { get; init; }
    public string? SearchText { get; init; }
    public int? PersonId { get; init; }
}

public enum SortDirection
{
    None,
    Ascending,
    Descending
}