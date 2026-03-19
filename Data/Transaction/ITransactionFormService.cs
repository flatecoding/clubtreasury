using ClubTreasury.Data.Category;
using ClubTreasury.Data.ItemDetail;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Transaction;

public interface ITransactionFormService
{
    Task<TransactionReferenceData> LoadReferenceDataAsync(CancellationToken ct = default);
    Task<TransactionModel?> LoadTransactionAsync(int id, CancellationToken ct = default);
    Task<List<CategoryModel>> GetCategoriesForCostCenterAsync(int costCenterId, CancellationToken ct = default);
    Task<List<ItemDetailModel>> GetItemDetailsForCategoryAsync(int categoryId, CancellationToken ct = default);
    Task<int> GetNextDocumentNumberAsync(int cashRegisterId, CancellationToken ct = default);
    Task<Result> SaveTransactionAsync(TransactionModel model, TransactionFormSelections selections, bool isEditMode, CancellationToken ct = default);
}
