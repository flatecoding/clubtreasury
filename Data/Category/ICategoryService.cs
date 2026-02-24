using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Category;

public interface ICategoryService
{
    Task<List<CategoryModel>> GetAllCategoriesAsync();
    Task<CategoryModel?> GetCategoryByIdAsync(int id);
    Task<CategoryModel?> GetCategoryByNameAsync(string name);
    Task<IEnumerable<CategoryModel>> GetCategoriesByCostCenterIdAsync(int costUnitId);
    Task<IOperationResult> AddCategoryAsync(CategoryModel unit);
    Task<IOperationResult> UpdateCategoryAsync(CategoryModel unit);
    Task<IOperationResult> DeleteCategoryAsync(int id);
}