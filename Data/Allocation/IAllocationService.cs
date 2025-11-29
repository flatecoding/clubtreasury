namespace TTCCashRegister.Data.Allocation;

public interface IAllocationService
{
    Task<AllocationModel?> GetAllocationsByIdAsync(int id);
    Task<List<AllocationModel>> GetAllAllocationsAsync();
    Task<AllocationModel> EnsureAllocationExistsAsync(AllocationModel allocation, CancellationToken ct = default);
    Task<bool> AddAllocationAsync(AllocationModel allocation);
    Task<bool> UpdateAllocationAsync(AllocationModel updatedAllocation);
    Task<bool> DeleteAllocationAsync(int id);
}