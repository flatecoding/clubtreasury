using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Allocation;

public interface IAllocationService
{
    Task<AllocationModel?> GetAllocationsByIdAsync(int id, CancellationToken ct = default);
    Task<List<AllocationModel>> GetAllAllocationsAsync(CancellationToken ct = default);

    Task<AllocationModel> GetRequiredAllocationAsync(
        int allocationId,
        CancellationToken ct = default);
    Task<AllocationModel> GetOrCreateAllocationAsync(
        string costCenterName,
        string categoryName,
        string? itemDetailName = null,
        CancellationToken ct = default);
    Task<Result> AddAllocationAsync(AllocationModel allocation, CancellationToken ct = default);
    Task<Result> UpdateAllocationAsync(AllocationModel updatedAllocation, CancellationToken ct = default);
    Task<Result> DeleteAllocationAsync(int id, CancellationToken ct = default);
}