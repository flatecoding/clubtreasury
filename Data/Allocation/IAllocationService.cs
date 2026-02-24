using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Allocation;

public interface IAllocationService
{
    Task<AllocationModel?> GetAllocationsByIdAsync(int id);
    Task<List<AllocationModel>> GetAllAllocationsAsync();

    Task<AllocationModel> GetRequiredAllocationAsync(
        int allocationId,
        CancellationToken ct = default);
    Task<AllocationModel> GetOrCreateAllocationAsync(
        string costCenterName,
        string categoryName,
        string? itemDetailName = null);
    Task<IOperationResult> AddAllocationAsync(AllocationModel allocation);
    Task<IOperationResult> UpdateAllocationAsync(AllocationModel updatedAllocation);
    Task<IOperationResult> DeleteAllocationAsync(int id);
}