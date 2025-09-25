using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.Allocation;

public class AllocationService
{
    private readonly CashDataContext _context;

    public AllocationService(CashDataContext context)
    {
        _context = context;
    }

    public async Task<AllocationModel?> GetAllocationsByIdAsync(int id)
    {
        return await _context.Allocations
            .Include(a => a.CostCenter)
            .Include(a => a.Category)
            .Include(a => a.ItemDetail)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<AllocationModel>> GetAllAllocationsAsync()
    {
        return await _context.Allocations
            .Include(a => a.CostCenter)
            .Include(a => a.Category)
            .Include(a => a.ItemDetail)
            .ToListAsync();
    }

    
    public async Task<AllocationModel> EnsureAllocationExistsAsync(AllocationModel allocation, CancellationToken ct = default)
    {
        var existing = await _context.Allocations.FirstOrDefaultAsync(a =>
            a.CostCenterId == allocation.CostCenterId &&
            a.CategoryId   == allocation.CategoryId &&
            a.ItemDetailId == allocation.ItemDetailId, ct);

        if (existing != null)
            return existing; // tracked
        
        var created = new AllocationModel
        {
            CostCenterId = allocation.CostCenterId,
            CategoryId   = allocation.CategoryId,
            ItemDetailId = allocation.ItemDetailId
        };
        
        _context.Allocations.Add(created); // State = Added
        return created;
    }

    public async Task<bool> AddAllocationAsync(AllocationModel allocation)
    {
        
        if (await AllocationExistsAsync(allocation))
        {
            // Optional: Loggen oder Rückgabe null
            return false;
        }
        _context.Allocations.Add(allocation);
        await _context.SaveChangesAsync();
        return true;
    }
    
    
    public async Task<bool> AllocationExistsAsync(AllocationModel allocation)
    {
        return await _context.Allocations.AnyAsync(a =>
            a.CostCenterId == allocation.CostCenterId &&
            a.CategoryId == allocation.CategoryId &&
            a.ItemDetailId == allocation.ItemDetailId);
    }

    
    
    public async Task<bool> UpdateAllocationAsync(AllocationModel updatedAllocation)
    {
        var existing = await _context.Allocations.FindAsync(updatedAllocation.Id);
        if (existing == null)
            return false;

        existing.CostCenterId = updatedAllocation.CostCenterId;
        existing.CategoryId = updatedAllocation.CategoryId;
        existing.ItemDetailId = updatedAllocation.ItemDetailId;

        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<bool> DeleteAllocationAsync(int id)
    {
        var allocation = await _context.Allocations.FindAsync(id);
        if (allocation == null) return false;

        _context.Allocations.Remove(allocation);
        await _context.SaveChangesAsync();
        return true;
    }
}