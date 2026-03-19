using ClubTreasury.Data.Allocation;
using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.Category;
using ClubTreasury.Data.CostCenter;
using ClubTreasury.Data.ItemDetail;
using ClubTreasury.Data.OperationResult;
using ClubTreasury.Data.SpecialItem;

namespace ClubTreasury.Data.Transaction;

public class TransactionFormService(
    ITransactionService transactionService,
    ICashRegisterService cashRegisterService,
    ICostCenterService costCenterService,
    ICategoryService categoryService,
    IItemDetailService itemDetailService,
    ISpecialItemService specialItemService,
    IAllocationService allocationService) : ITransactionFormService
{
    public async Task<TransactionReferenceData> LoadReferenceDataAsync(CancellationToken ct = default)
    {
        var cashRegisters = await cashRegisterService.GetAllCashRegistersAsync(ct);
        var costCenters = await costCenterService.GetAllCostCentersAsync(ct);
        var specialItems = await specialItemService.GetAllSpecialItemsAsync(ct);

        return new TransactionReferenceData(cashRegisters, costCenters, specialItems);
    }

    public Task<TransactionModel?> LoadTransactionAsync(int id, CancellationToken ct = default)
        => transactionService.GetTransactionByIdAsync(id, ct);

    public async Task<List<CategoryModel>> GetCategoriesForCostCenterAsync(int costCenterId, CancellationToken ct = default)
        => (await categoryService.GetCategoriesByCostCenterIdAsync(costCenterId, ct)).ToList();

    public Task<List<ItemDetailModel>> GetItemDetailsForCategoryAsync(int categoryId, CancellationToken ct = default)
        => itemDetailService.GetItemDetailByCategoryIdAsync(categoryId, ct);

    public async Task<int> GetNextDocumentNumberAsync(int cashRegisterId, CancellationToken ct = default)
    {
        var latest = await transactionService.GetLatestDocumentNumberAsync(cashRegisterId, ct);
        return latest + 1;
    }

    public async Task<Result> SaveTransactionAsync(
        TransactionModel model,
        TransactionFormSelections selections,
        bool isEditMode,
        CancellationToken ct = default)
    {
        model.CashRegister = selections.CashRegister;
        model.CashRegisterId = selections.CashRegister.Id;

        var allocation = await allocationService.GetOrCreateAllocationAsync(
            selections.CostCenter.CostUnitName,
            selections.Category.Name,
            selections.ItemDetail?.CostDetails,
            ct);

        model.AllocationId = allocation.Id;
        model.Allocation = null!;

        model.SpecialItem = selections.SpecialItem;
        model.SpecialItemId = selections.SpecialItem?.Id;

        model.Date = DateOnly.FromDateTime(selections.Date);

        return isEditMode
            ? await transactionService.UpdateTransactionAsync(model, ct)
            : await transactionService.AddTransactionAsync(model, ct);
    }
}
