using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.Allocation;

public class AllocationService(CashDataContext context, ILogger<AllocationService> logger) :IAllocationService
{
    public async Task<AllocationModel?> GetAllocationsByIdAsync(int id)
    {
        return await context.Allocations
            .Include(a => a.CostCenter)
            .Include(a => a.Category)
            .Include(a => a.ItemDetail)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<AllocationModel>> GetAllAllocationsAsync()
    {
        return await context.Allocations
            .Include(a => a.CostCenter)
            .Include(a => a.Category)
            .Include(a => a.ItemDetail)
            .ToListAsync();
    }

    
    public async Task<AllocationModel> EnsureAllocationExistsAsync(AllocationModel allocation, CancellationToken ct = default)
    {
        try
        {
            var existing = await context.Allocations.FirstOrDefaultAsync(a =>
                a.CostCenterId == allocation.CostCenterId &&
                a.CategoryId   == allocation.CategoryId &&
                a.ItemDetailId == allocation.ItemDetailId, ct);

            if (existing != null)
            {
                logger.LogInformation("Use existing allocation. CostCenter: {@CostCenter},  Category: {@Category}, " +
                                      "ItemDetail: {@ItemDetail}", allocation.CostCenter.CostUnitName, 
                    allocation.Category.Name,
                    allocation.ItemDetail?.CostDetails ?? "null");
                return existing;
            }
            
        
            var created = new AllocationModel
            {
                CostCenterId = allocation.CostCenterId,
                CategoryId   = allocation.CategoryId,
                ItemDetailId = allocation.ItemDetailId
            };
        
            context.Allocations.Add(created); // State = Added
            logger.LogInformation("Add allocation. CostCenter: {@CostCenter},  Category: {@Category}, " +
                                  "ItemDetail: {@ItemDetail}", created.CostCenter.CostUnitName,
                created.Category.Name, created.ItemDetail?.CostDetails ?? "null");
            return created;

        }
        catch(Exception e)
        {
            logger.LogCritical(e, "An exception occurred while adding allocation: CostCenter: {@CostCenter} " +
                                     "Category: {@Category} ItemDetails: {@ItemDetails}", allocation.CostCenter.CostUnitName, 
                                      allocation.Category.Name, allocation.ItemDetail?.CostDetails ?? "N/A");
            
        }
        logger.LogCritical("Existing allocation not found and new allocation could not be created.");
        throw new DbUpdateException();
    }

    public async Task<bool> AddAllocationAsync(AllocationModel allocation)
    {
        if (await AllocationExistsAsync(allocation))
        {
            logger.LogWarning("Allocation wit CostCenter: {CostCenterId}, Category: {CategoryId} " +
                              " ItemDetail: {ItemDetailId} already exists.", allocation.CostCenterId,
                allocation.CategoryId, allocation.ItemDetailId);
            return false;
        }
        context.Allocations.Add(allocation);
        await context.SaveChangesAsync();
        logger.LogInformation("AddAllocationAsync: CostCenter: {@CostCenter},  Category: {@Category}, " +
                              "ItemDetail: {@ItemDetail}", allocation.CostCenterId, 
                               allocation.CategoryId, allocation.ItemDetailId);
        return true;
    }


    private async Task<bool> AllocationExistsAsync(AllocationModel allocation)
    {
        return await context.Allocations.AnyAsync(a =>
            a.CostCenterId == allocation.CostCenterId &&
            a.CategoryId == allocation.CategoryId &&
            a.ItemDetailId == allocation.ItemDetailId);
    }

    
    
    public async Task<bool> UpdateAllocationAsync(AllocationModel updatedAllocation)
    {
        var existing = await context.Allocations.FindAsync(updatedAllocation.Id);
        if (existing == null)
            return false;

        existing.CostCenterId = updatedAllocation.CostCenterId;
        existing.CategoryId = updatedAllocation.CategoryId;
        existing.ItemDetailId = updatedAllocation.ItemDetailId;
        
        logger.LogInformation("Update allocation. CostCenter: {@CostCenter},  Category: {@Category}, " +
        "ItemDetail: {@ItemDetail}", updatedAllocation.CostCenter.CostUnitName, 
            updatedAllocation.Category.Name, updatedAllocation.ItemDetail?.CostDetails ?? "null");

        await context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> DeleteAllocationAsync(int id)
    {
        var allocation = await context.Allocations.FindAsync(id);
        if (allocation == null) return false;

        context.Allocations.Remove(allocation);
        await context.SaveChangesAsync();
        return true;
    }
}