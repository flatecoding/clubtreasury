using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TTCCashRegister.Data.Category;
using TTCCashRegister.Data.CostCenter;
using TTCCashRegister.Data.ItemDetail;
using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.Allocation;

public class AllocationService(
    CashDataContext context, 
    ILogger<AllocationService> logger, 
    IOperationResultFactory operationResultFactory,
    IStringLocalizer<Translation> localizer,
    ICostCenterService costCenterService,
    ICategoryService categoryService,
    IItemDetailService itemDetailService) : IAllocationService
{
    private string EntityName => localizer["Allocation"];

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
                logger.LogInformation("Use existing allocation. CostCenter: {@CostCenter}, Category: {@Category}, " +
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

            context.Allocations.Add(created);
            logger.LogInformation("Add allocation. CostCenter: {@CostCenter}, Category: {@Category}, " +
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

    public async Task<IOperationResult> AddAllocationAsync(AllocationModel allocation)
    {
        if (await AllocationExistsAsync(allocation))
        {
            logger.LogWarning(
                "Allocation with CostCenter: {CostCenterId}, Category: {CategoryId}, ItemDetail: {ItemDetailId} already exists.",
                allocation.CostCenterId, allocation.CategoryId, allocation.ItemDetailId);

            return operationResultFactory.AlreadyExists(
                EntityName);
        }

        try
        {
            context.Allocations.Add(allocation);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "AddAllocationAsync: CostCenter: {CostCenterId}, Category: {CategoryId}, ItemDetail: {ItemDetailId}",
                allocation.CostCenterId, allocation.CategoryId, allocation.ItemDetailId);
            
            return operationResultFactory.SuccessAdded(EntityName, allocation.Id); 
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to add allocation: CostCenter: {CostCenterId}, Category: {CategoryId}, ItemDetail: {ItemDetailId}",
                allocation.CostCenterId, allocation.CategoryId, allocation.ItemDetailId);

            return operationResultFactory.FailedToAdd(EntityName, localizer["Exception"]);
        }
    }

    private async Task<bool> AllocationExistsAsync(AllocationModel allocation)
    {
        return await context.Allocations.AnyAsync(a =>
            a.CostCenterId == allocation.CostCenterId &&
            a.CategoryId == allocation.CategoryId &&
            a.ItemDetailId == allocation.ItemDetailId);
    }

    public async Task<IOperationResult> UpdateAllocationAsync(AllocationModel updatedAllocation)
    {
        var existing = await context.Allocations.FindAsync(updatedAllocation.Id);
        if (existing == null)
        {
            logger.LogWarning("Allocation with Id {AllocationId} not found.", updatedAllocation.Id);
            return operationResultFactory.NotFound(EntityName, updatedAllocation.Id);
        }

        try
        {
            existing.CostCenterId = updatedAllocation.CostCenterId;
            existing.CategoryId = updatedAllocation.CategoryId;
            existing.ItemDetailId = updatedAllocation.ItemDetailId;

            await context.SaveChangesAsync();

            logger.LogInformation(
                "Updated allocation Id {AllocationId}. CostCenter: {CostCenter}, Category: {Category}, ItemDetail: {ItemDetail}",
                updatedAllocation.Id,
                updatedAllocation.CostCenter.CostUnitName,
                updatedAllocation.Category.Name,
                updatedAllocation.ItemDetail?.CostDetails ?? "null");

            return operationResultFactory.SuccessUpdated(EntityName, updatedAllocation.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to update allocation Id {AllocationId}. CostCenter: {CostCenter}, Category: {Category}, ItemDetail: {ItemDetail}",
                updatedAllocation.Id,
                updatedAllocation.CostCenter.CostUnitName,
                updatedAllocation.Category.Name,
                updatedAllocation.ItemDetail?.CostDetails ?? "null");

            return operationResultFactory.FailedToUpdate(EntityName, localizer["Exception"]);
        }
    }

    public async Task<IOperationResult> DeleteAllocationAsync(int id)
    {
        var allocation = await context.Allocations.FindAsync(id);
        if (allocation == null)
        {
            logger.LogWarning("Allocation with Id {AllocationId} not found.", id);
            return operationResultFactory.NotFound(EntityName, id);
        }
        try
        {
            context.Allocations.Remove(allocation);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "Deleted allocation Id {AllocationId}. CostCenter: {CostCenter}, Category: {Category}, ItemDetail: {ItemDetail}",
                allocation.Id,
                allocation.CostCenterId,
                allocation.CategoryId,
                allocation.ItemDetailId);

            return operationResultFactory.SuccessDeleted(EntityName, id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to delete allocation Id {AllocationId}. CostCenter: {CostCenter}, Category: {Category}, ItemDetail: {ItemDetail}",
                allocation.Id,
                allocation.CostCenterId,
                allocation.CategoryId,
                allocation.ItemDetailId);

            return operationResultFactory.FailedToDelete(EntityName, localizer["Exception"]);
        }
    }
    
    public async Task<AllocationModel> GetOrCreateAllocationAsync(
    string costCenterName,
    string categoryName,
    string? itemDetailName = null)
{
    var costCenter = await costCenterService.GetCostCenterByNameAsync(costCenterName);
    if (costCenter == null)
    {
        costCenter = new CostCenterModel { CostUnitName = costCenterName };
        await costCenterService.AddCostCenterAsync(costCenter);
    }
    
    var category = await categoryService.GetCategoryByNameAsync(categoryName); 
    if (category == null)
    {
        category = new CategoryModel { Name = categoryName };
        await categoryService.AddCategoryAsync(category);
    }

    ItemDetailModel? itemDetail = null;
    if (!string.IsNullOrEmpty(itemDetailName))
    {
        itemDetail = await itemDetailService.GetItemDetailByNameAsync(itemDetailName); 
        if (itemDetail == null)
        {
            itemDetail = new ItemDetailModel { CostDetails = itemDetailName };
            await itemDetailService.AddItemDetailAsync(itemDetail);
        }
    }
    
    var allocation = await context.Allocations
        .FirstOrDefaultAsync(a =>
            a.CostCenterId == costCenter.Id &&
            a.CategoryId == category.Id &&
            a.ItemDetailId == (itemDetail != null ? itemDetail.Id : null));

    if (allocation != null) return allocation;
    allocation = new AllocationModel
    {
        CostCenter = costCenter,
        Category = category,
        ItemDetail = itemDetail
    };
    context.Allocations.Add(allocation);
    await context.SaveChangesAsync();
    logger.LogInformation("Created new allocation: {CostCenter}/{Category}", 
        costCenterName, categoryName);

    return allocation;
}
}