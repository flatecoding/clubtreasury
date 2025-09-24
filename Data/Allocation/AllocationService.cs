using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.Allocation;

public class AllocationService
{
    private readonly CashDataContext _context;

    public AllocationService(CashDataContext context)
    {
        _context = context;
    }

    public async Task<AllocationModel?> GetByIdAsync(int id)
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

    public async Task<AllocationModel> EnsureAllocationExistsAsync(AllocationModel allocation)
    {
        var existing = await _context.Allocations.FirstOrDefaultAsync(a =>
            a.CostCenterId == allocation.CostCenterId &&
            a.CategoryId == allocation.CategoryId &&
            a.ItemDetailId == allocation.ItemDetailId);

        if (existing != null)
        {
            return existing;
        }

        _context.Allocations.Add(allocation);
        await _context.SaveChangesAsync();
        return allocation;
    }

    public async Task<AllocationModel> AddAsync(AllocationModel allocation)
    {
        _context.Allocations.Add(allocation);
        await _context.SaveChangesAsync();
        return allocation;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var allocation = await _context.Allocations.FindAsync(id);
        if (allocation == null) return false;

        _context.Allocations.Remove(allocation);
        await _context.SaveChangesAsync();
        return true;
    }
}