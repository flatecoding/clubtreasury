using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.CostCenter;

public interface ICostCenterService
{
    Task<List<CostCenterModel>> GetAllCostCentersAsync(CancellationToken ct = default);
    Task<CostCenterModel?> GetCostCenterByIdAsync(int id, CancellationToken ct = default);
    Task<CostCenterModel?> GetCostCenterByNameAsync(string name, CancellationToken ct = default);
    Task<IOperationResult> AddCostCenterAsync(CostCenterModel costCenter, CancellationToken ct = default);
    Task<IOperationResult> UpdateCostCenterAsync(CostCenterModel costCenter, CancellationToken ct = default);
    Task<IOperationResult> DeleteCostCenterAsync(int id, CancellationToken ct = default);
}