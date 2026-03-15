using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Category
{
    public class CategoryService(CashDataContext context, ILogger<CategoryService> logger,
    IOperationResultFactory operationResultFactory,
        IStringLocalizer<Translation> localizer): ICategoryService
    {
        private string EntityName => localizer["Category"];

        public async Task<List<CategoryModel>> GetAllCategoriesAsync(CancellationToken ct = default)
        {
            return await context.Categories
                .Include(b => b.Allocations)
                    .ThenInclude(a => a.CostCenter)
                .Include(b => b.Allocations)
                    .ThenInclude(a => a.ItemDetail)
                .ToListAsync(ct);
        }

        public async Task<CategoryModel?> GetCategoryByIdAsync(int id, CancellationToken ct = default)
        {
            return await context.Categories
                .Include(b => b.Allocations)
                    .ThenInclude(a => a.CostCenter)
                .Include(b => b.Allocations)
                    .ThenInclude(a => a.ItemDetail)
                .FirstOrDefaultAsync(b => b.Id == id, ct);
        }

        public async Task<CategoryModel?> GetCategoryByNameAsync(string name, CancellationToken ct = default)
        {
            return await  context.Categories.FirstOrDefaultAsync(b => b.Name == name, ct);
        }

        public async Task<IEnumerable<CategoryModel>> GetCategoriesByCostCenterIdAsync(int costUnitId, CancellationToken ct = default)
        {
            var categories = await context.Categories
                .Where(b => b.Allocations.Any(a => a.CostCenterId == costUnitId))
                .OrderBy(c => c.Name)
                .ToListAsync(ct);
            if (categories.Count == 0) return new List<CategoryModel>();
            logger.LogInformation("Available categories found");
            return categories;
        }

        public async Task<IOperationResult> AddCategoryAsync(CategoryModel category, CancellationToken ct = default)
        {
            try
            {
                context.Categories.Add(category);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("Category added: {@unit}", category.Name);

                return operationResultFactory.SuccessAdded($"{EntityName}: '{category.Name}", category.Id);

            }
            catch (Exception e)
            {
                logger.LogError(e, "Error adding category: {@category}", category);
                return operationResultFactory.FailedToAdd(EntityName, localizer["Exception"]);
            }

        }

        public async Task<IOperationResult> UpdateCategoryAsync(CategoryModel category, CancellationToken ct = default)
        {
            try
            {
                context.Categories.Update(category);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("Category updated: {@unit}", category.Name);

                return operationResultFactory.SuccessUpdated($"{EntityName}: '{category.Name}'", category.Id);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error updating category: {@category}", category);
                return operationResultFactory.FailedToUpdate(EntityName, localizer["Exception"]);
            }
        }

        public async Task<IOperationResult> DeleteCategoryAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var category = await context.Categories.FindAsync([id], ct);
                if (category == null)
                {
                    logger.LogError("Category not found");
                    return operationResultFactory.NotFound(EntityName, $"Id: '{id}' not found");
                }
                context.Categories.Remove(category);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("Category deleted: {@unit}", category.Name);

                return operationResultFactory.SuccessDeleted($"{EntityName}: '{category.Name}'", category.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting category with id: {@id}", id);
                return operationResultFactory.FailedToDelete(EntityName, localizer["Exception"]);
            }
        }
    }
}