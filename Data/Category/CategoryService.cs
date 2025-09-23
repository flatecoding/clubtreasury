using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.Category
{
    public class CategoryService
    {
        private readonly CashDataContext _context;

        public CategoryService(CashDataContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        
        public async Task<List<CategoryModel>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(b => b.Accounts)
                    .ThenInclude(a => a.CostCenter)
                .Include(b => b.Accounts)
                    .ThenInclude(a => a.UnitDetails)
                .ToListAsync();
        }

        public async Task<CategoryModel?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .Include(b => b.Accounts)
                    .ThenInclude(a => a.CostCenter)
                .Include(b => b.Accounts)
                    .ThenInclude(a => a.UnitDetails)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<CategoryModel>> GetCategoriesByCostCenterIdAsync(int costUnitId)
        {
            return await _context.Categories
                .Where(b => b.Accounts.Any(a => a.CostCenterId == costUnitId))
                .ToListAsync();
        }

        public async Task AddCategoryAsync(CategoryModel unit)
        {
            _context.Categories.Add(unit);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(CategoryModel unit)
        {
            _context.Categories.Update(unit);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var unit = await _context.Categories.FindAsync(id);
            if (unit == null) return false;
            _context.Categories.Remove(unit);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
