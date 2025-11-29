using Microsoft.EntityFrameworkCore;
using Serilog.Data;

namespace TTCCashRegister.Data.Category
{
    public class CategoryService(CashDataContext context, ILogger<CategoryService> logger): ICategoryService
    {
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

        public async Task AddCategoryAsync(CategoryModel unit)
        {
            _context.Categories.Add(unit);
            await _context.SaveChangesAsync();
            logger.LogInformation("Category added: {@unit}", unit.Name);
        }

        public async Task UpdateCategoryAsync(CategoryModel unit)
        {
            _context.Categories.Update(unit);
            await _context.SaveChangesAsync();
            logger.LogInformation("Category updated: {@unit}", unit.Name);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var unit = await _context.Categories.FindAsync(id);
            if (unit == null)
            {
                logger.LogError("Category not found");
                return false;
            }
            _context.Categories.Remove(unit);
            await _context.SaveChangesAsync();
            logger.LogInformation("Category deleted: {@unit}", unit.Name);
            return true;
        }
    }
}
