namespace TTCCashRegister.Data.Category;

public interface ICategoryService
{
    Task<List<CategoryModel>> GetAllCategoriesAsync();
    Task<CategoryModel?> GetCategoryByIdAsync(int id);
    Task<IEnumerable<CategoryModel>> GetCategoriesByCostCenterIdAsync(int costUnitId);
    Task AddCategoryAsync(CategoryModel unit);
    Task UpdateCategoryAsync(CategoryModel unit);
    Task<bool> DeleteCategoryAsync(int id);
}