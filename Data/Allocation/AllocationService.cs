using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.Category;
using ClubTreasury.Data.CostCenter;
using ClubTreasury.Data.ItemDetail;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Allocation;

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

    public async Task<AllocationModel?> GetAllocationsByIdAsync(int id, CancellationToken ct = default)
    {
        return await context.Allocations
            .Include(a => a.CostCenter)
            .Include(a => a.Category)
            .Include(a => a.ItemDetail)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<List<AllocationModel>> GetAllAllocationsAsync(CancellationToken ct = default)
    {
        return await context.Allocations
            .Include(a => a.CostCenter)
            .Include(a => a.Category)
            .Include(a => a.ItemDetail)
            .ToListAsync(ct);
    }

    public async Task<IOperationResult> AddAllocationAsync(AllocationModel allocation, CancellationToken ct = default)
    {
        try
        {
            if (allocation.CategoryId == 0 || allocation.CostCenterId == 0)
                return operationResultFactory.DialogIsEmpty(EntityName, $"{localizer["CostCenter"]} Id '{allocation.CostCenterId}' " +
                                                                        $" - {localizer["Category"]} Id '{allocation.CategoryId}'");
            if (await AllocationExistsAsync(allocation, ct))
            {
                logger.LogWarning(
                    "Allocation with CostCenter: {CostCenterId}, Category: {CategoryId}, ItemDetail: {ItemDetailId} already exists.",
                    allocation.CostCenterId, allocation.CategoryId, allocation.ItemDetailId);

                return operationResultFactory.AlreadyExists(EntityName);
            }

            context.Allocations.Add(allocation);
            await context.SaveChangesAsync(ct);

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

    private async Task<bool> AllocationExistsAsync(AllocationModel allocation, CancellationToken ct = default)
    {
        return await context.Allocations.AnyAsync(a =>
            a.CostCenterId == allocation.CostCenterId &&
            a.CategoryId == allocation.CategoryId &&
            a.ItemDetailId == allocation.ItemDetailId, ct);
    }

    public async Task<IOperationResult> UpdateAllocationAsync(AllocationModel updatedAllocation, CancellationToken ct = default)
    {
        try
        {
            var existing = await context.Allocations.FindAsync([updatedAllocation.Id], ct);
            if (existing == null)
            {
                logger.LogWarning("Allocation with Id {AllocationId} not found.", updatedAllocation.Id);
                return operationResultFactory.NotFound(EntityName, updatedAllocation.Id);
            }

            existing.CostCenterId = updatedAllocation.CostCenterId;
            existing.CategoryId = updatedAllocation.CategoryId;
            existing.ItemDetailId = updatedAllocation.ItemDetailId;

            await context.SaveChangesAsync(ct);

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

    public async Task<IOperationResult> DeleteAllocationAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var allocation = await context.Allocations.FindAsync([id], ct);
            if (allocation == null)
            {
                logger.LogWarning("Allocation with Id {AllocationId} not found.", id);
                return operationResultFactory.NotFound(EntityName, id);
            }

            context.Allocations.Remove(allocation);
            await context.SaveChangesAsync(ct);

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
                "Failed to delete allocation Id {AllocationId}.",
                id);

            return operationResultFactory.FailedToDelete(EntityName, localizer["Exception"]);
        }
    }

    public async Task<AllocationModel> GetOrCreateAllocationAsync(
    string costCenterName,
    string categoryName,
    string? itemDetailName = null,
    CancellationToken ct = default)
    {
        var costCenter = await costCenterService.GetCostCenterByNameAsync(costCenterName, ct);
        if (costCenter == null)
        {
            costCenter = new CostCenterModel { CostUnitName = costCenterName };
            await costCenterService.AddCostCenterAsync(costCenter, ct);
        }

        var category = await categoryService.GetCategoryByNameAsync(categoryName, ct);
        if (category == null)
        {
            category = new CategoryModel { Name = categoryName };
            await categoryService.AddCategoryAsync(category, ct);
        }

        ItemDetailModel? itemDetail = null;
        if (!string.IsNullOrEmpty(itemDetailName))
        {
            itemDetail = await itemDetailService.GetItemDetailByNameAsync(itemDetailName, ct);
            if (itemDetail == null)
            {
                itemDetail = new ItemDetailModel { CostDetails = itemDetailName };
                await itemDetailService.AddItemDetailAsync(itemDetail, ct);
            }
        }

        var allocation = await FindAllocationAsync(
            costCenter.Id,
            category.Id,
            itemDetail?.Id,
            ct);

        if (allocation != null)
            return allocation;

        allocation = new AllocationModel
        {
            CostCenterId = costCenter.Id,
            CategoryId   = category.Id,
            ItemDetailId = itemDetail?.Id
        };

        context.Allocations.Add(allocation);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Created new allocation: {CostCenter}/{Category}",
            costCenterName, categoryName);

        return allocation;
    }

    private Task<AllocationModel?> FindAllocationAsync(
        int costCenterId,
        int categoryId,
        int? itemDetailId,
        CancellationToken ct = default)
    {
        return context.Allocations.FirstOrDefaultAsync(a =>
            a.CostCenterId == costCenterId &&
            a.CategoryId   == categoryId &&
            a.ItemDetailId == itemDetailId, ct);
    }

    public async Task<AllocationModel> GetRequiredAllocationAsync(
        int allocationId,
        CancellationToken ct = default)
    {
        var allocation = await context.Allocations.FindAsync([ allocationId ], ct);
        return allocation ?? throw new InvalidOperationException($"Allocation {allocationId} not found.");
    }
}