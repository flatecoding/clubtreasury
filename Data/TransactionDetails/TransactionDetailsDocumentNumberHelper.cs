namespace ClubTreasury.Data.TransactionDetails;

public static class TransactionDetailsDocumentNumberHelper
{
    public static int GetNextDetailDocumentNumber(int parentDocumentNumber, List<TransactionDetailsModel>? details)
    {
        var baseNumber = parentDocumentNumber * 100;
        if (details is null || details.Count == 0)
            return baseNumber + 1;

        var maxExisting = details
            .Where(d => d.DocumentNumber.HasValue
                        && d.DocumentNumber.Value >= baseNumber + 1
                        && d.DocumentNumber.Value <= baseNumber + 99)
            .Select(d => d.DocumentNumber!.Value)
            .DefaultIfEmpty(baseNumber)
            .Max();

        return maxExisting + 1;
    }
}