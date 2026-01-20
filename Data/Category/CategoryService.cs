using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Serilog.Data;
using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.Category
{
    public class CategoryService(CashDataContext context, ILogger<CategoryService> logger,
    IOperationResultFactory operationResultFactory,
        IStringLocalizer<Translation> localizer): ICategoryService
    {
        private string EntityName => localizer["Category"];
        private readonly CashDataContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<List<CategoryModel>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(b => b.Allocations)
                    .ThenInclude(a => a.CostCenter)
                .Include(b => b.Allocations)
                    .ThenInclude(a => a.ItemDetail)
                .ToListAsync();
        }

        public async Task<CategoryModel?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .Include(b => b.Allocations)
                    .ThenInclude(a => a.CostCenter)
                .Include(b => b.Allocations)
                    .ThenInclude(a => a.ItemDetail)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<CategoryModel>> GetCategoriesByCostCenterIdAsync(int costUnitId)
        {
            var categories = await _context.Categories
                .Where(b => b.Allocations.Any(a => a.CostCenterId == costUnitId))
                .OrderBy(c => c.Name)
                .ToListAsync();
            if (categories.Count == 0) return new List<CategoryModel>();
            logger.LogInformation("Available categories found");
            return categories;
        }

        public async Task<IOperationResult> AddCategoryAsync(CategoryModel category)
        {
            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                logger.LogInformation("Category added: {@unit}", category.Name);

                return operationResultFactory.SuccessAdded($"{EntityName}: '{category.Name}", category.Id);

            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Error adding category: {@category}", category);
                return operationResultFactory.FailedToAdd(EntityName, localizer["Exception"]);
            }
            
        }

        public async Task<IOperationResult> UpdateCategoryAsync(CategoryModel category)
        {
            try
            {
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                logger.LogInformation("Category updated: {@unit}", category.Name);
                
                return operationResultFactory.SuccessUpdated($"{EntityName}: '{category.Name}'", category.Id);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
           
        }

        public async Task<IOperationResult> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    logger.LogError("Category not found");
                    return operationResultFactory.NotFound(EntityName, $"Id: '{id}' not found");
                }
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                logger.LogInformation("Category deleted: {@unit}", category.Name);
                
                return operationResultFactory.SuccessDeleted($"{EntityName}: '{category.Name}'", category.Id);
            }
            catch (Exception e)
            {
                logger.LogCritical("Error deleting category with id: {@id}", id);
                return operationResultFactory.FailedToDelete(EntityName, localizer["Exception"]);
            }
        }
    }
}
