using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Import;

public interface IImportCostCenterService
{
    Task<IOperationResult> ImportCostCentersAndPositions(Stream? fileStream, string fileName);
}