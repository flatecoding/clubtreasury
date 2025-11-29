namespace TTCCashRegister.Data.CostCenter;

public interface ICostCenterService
{
    Task<List<CostCenterModel>> GetAllCostCentersAsync();
    Task<CostCenterModel?> GetCostCenterByIdAsync(int id);
    Task<bool> AddCostCenterAsync(CostCenterModel costCenter);
    Task<bool> UpdateCostCenterAsync(CostCenterModel costCenter);
    Task<bool> DeleteCostCenterAsync(int id);
}