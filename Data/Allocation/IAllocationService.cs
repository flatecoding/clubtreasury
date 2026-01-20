using TTCCashRegister.Data.Notification;
using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.Allocation;

public interface IAllocationService
{
    Task<AllocationModel?> GetAllocationsByIdAsync(int id);
    Task<List<AllocationModel>> GetAllAllocationsAsync();
    Task<AllocationModel> EnsureAllocationExistsAsync(AllocationModel allocation, CancellationToken ct = default);
    Task<IOperationResult> AddAllocationAsync(AllocationModel allocation);
    Task<IOperationResult> UpdateAllocationAsync(AllocationModel updatedAllocation);
    Task<IOperationResult> DeleteAllocationAsync(int id);
}