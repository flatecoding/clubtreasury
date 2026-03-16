using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Import;

public interface IImportCostCenterService
{
    Task<Result> ImportCostCentersAndPositions(Stream? fileStream, string fileName, CancellationToken ct = default);
}