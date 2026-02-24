using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.CostCenter
{
    public class CostCenterService(CashDataContext context, ILogger<CostCenterService> logger, 
        IStringLocalizer<Translation> localizer, IOperationResultFactory operationResultFactory) : ICostCenterService
    {
        private string EntityName => localizer["CostCenter"];
        private readonly CashDataContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<List<CostCenterModel>> GetAllCostCentersAsync()
        {
            return await _context.CostCenters
                .Include(c => c.Allocations)
                    .ThenInclude(a => a.Category)
                .Include(c => c.Allocations)
                    .ThenInclude(a => a.ItemDetail)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }
        
        public async Task<CostCenterModel?> GetCostCenterByIdAsync(int id)
        {
            return await _context.CostCenters
                .Include(c => c.Allocations)
                    .ThenInclude(a => a.Category)
                .Include(c => c.Allocations)
                    .ThenInclude(a => a.ItemDetail)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CostCenterModel?> GetCostCenterByNameAsync(string name)
        {
            return await  _context.CostCenters.FirstOrDefaultAsync(c => c.CostUnitName == name);
        }

        public async Task<IOperationResult> AddCostCenterAsync(CostCenterModel costCenter)
        {
            try
            {
                await _context.CostCenters.AddAsync(costCenter);
                await _context.SaveChangesAsync();
                logger.LogInformation("Cost center added: {@costCenter}", costCenter.CostUnitName);
                return operationResultFactory.SuccessAdded($"{EntityName}: '{costCenter.CostUnitName}'", costCenter.Id);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An error occured during add cost center: {@costCenter}", costCenter.CostUnitName);
                return operationResultFactory.FailedToAdd(EntityName, localizer["Exception"]);
            }
        }

        public async Task<IOperationResult> UpdateCostCenterAsync(CostCenterModel costCenter)
        {
            try
            {
                _context.CostCenters.Update(costCenter);
                await _context.SaveChangesAsync();
                logger.LogInformation("Cost center updated: {@costCenter}", costCenter.CostUnitName);
                return operationResultFactory.SuccessUpdated(EntityName, costCenter.Id);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An error occured during update of cost center: {@costCenter}", costCenter.CostUnitName);
                return operationResultFactory.FailedToUpdate(EntityName, localizer["Exception"]);
            }
        }

        public async Task<IOperationResult> DeleteCostCenterAsync(int id)
        {
            try
            {
                var costUnit = await _context.CostCenters.FindAsync(id);
                if (costUnit == null)
                {
                    logger.LogError("Cost center Id '{ID}' not found", id);
                    return operationResultFactory.NotFound(EntityName, $"Id: '{id}' not found");
                }
                _context.CostCenters.Remove(costUnit);
                await _context.SaveChangesAsync();
                logger.LogInformation("Cost center deleted: {@costUnit}", costUnit.CostUnitName);
                return operationResultFactory.SuccessDeleted(EntityName, id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured during delete of costunit with Id: {Id}", id );
                return operationResultFactory.FailedToDelete(EntityName, localizer["Exception"]);
            }
        }
    }
}
