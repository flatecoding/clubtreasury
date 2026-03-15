namespace ClubTreasury.Data.TransactionDetails;

public static class TransactionDetailsDocumentNumberHelper
{
    private const int DetailNumberMultiplier = 100;
    private const int MinDetailOffset = 1;
    private const int MaxDetailOffset = 99;

    public static int GetNextDetailDocumentNumber(int parentDocumentNumber, List<TransactionDetailsModel>? details)
    {
        var baseNumber = parentDocumentNumber * DetailNumberMultiplier;
        if (details is null || details.Count == 0)
            return baseNumber + MinDetailOffset;

        var maxExisting = details
            .Where(d => d.DocumentNumber.HasValue
                        && d.DocumentNumber.Value >= baseNumber + MinDetailOffset
                        && d.DocumentNumber.Value <= baseNumber + MaxDetailOffset)
            .Select(d => d.DocumentNumber!.Value)
            .DefaultIfEmpty(baseNumber)
            .Max();

        return maxExisting + 1;
    }
}