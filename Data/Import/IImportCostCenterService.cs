using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Import;

public interface IImportCostCenterService
{
    Task<Result> ImportCostCentersAndPositionsAsync(Stream? fileStream, string fileName, CancellationToken ct = default);
}