using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Category;

public interface ICategoryService
{
    Task<List<CategoryModel>> GetAllCategoriesAsync(CancellationToken ct = default);
    Task<CategoryModel?> GetCategoryByIdAsync(int id, CancellationToken ct = default);
    Task<CategoryModel?> GetCategoryByNameAsync(string name, CancellationToken ct = default);
    Task<IEnumerable<CategoryModel>> GetCategoriesByCostCenterIdAsync(int costUnitId, CancellationToken ct = default);
    Task<Result> AddCategoryAsync(CategoryModel unit, CancellationToken ct = default);
    Task<Result> UpdateCategoryAsync(CategoryModel unit, CancellationToken ct = default);
    Task<Result> DeleteCategoryAsync(int id, CancellationToken ct = default);
}