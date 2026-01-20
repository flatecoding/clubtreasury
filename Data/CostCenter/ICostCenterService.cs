using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.CostCenter;

public interface ICostCenterService
{
    Task<List<CostCenterModel>> GetAllCostCentersAsync();
    Task<CostCenterModel?> GetCostCenterByIdAsync(int id);
    Task<IOperationResult> AddCostCenterAsync(CostCenterModel costCenter);
    Task<IOperationResult> UpdateCostCenterAsync(CostCenterModel costCenter);
    Task<IOperationResult> DeleteCostCenterAsync(int id);
}