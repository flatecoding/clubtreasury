using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.Import;

public interface IImportCostCenterService
{
    Task<IOperationResult> ImportCostCentersAndPositions(Stream? fileStream, string fileName);
}