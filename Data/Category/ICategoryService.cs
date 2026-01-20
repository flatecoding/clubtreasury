using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.Category;

public interface ICategoryService
{
    Task<List<CategoryModel>> GetAllCategoriesAsync();
    Task<CategoryModel?> GetCategoryByIdAsync(int id);
    Task<IEnumerable<CategoryModel>> GetCategoriesByCostCenterIdAsync(int costUnitId);
    Task<IOperationResult> AddCategoryAsync(CategoryModel unit);
    Task<IOperationResult> UpdateCategoryAsync(CategoryModel unit);
    Task<IOperationResult> DeleteCategoryAsync(int id);
}