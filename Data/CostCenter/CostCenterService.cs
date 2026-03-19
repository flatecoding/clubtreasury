using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.CostCenter
{
    public class CostCenterService(CashDataContext context, ILogger<CostCenterService> logger,
        IStringLocalizer<Translation> localizer, IResultFactory operationResultFactory) : ICostCenterService
    {
        private string EntityName => localizer["CostCenter"];

        public async Task<List<CostCenterModel>> GetAllCostCentersAsync(CancellationToken ct = default)
        {
            return await context.CostCenters
                .OrderBy(c => c.Id)
                .ToListAsync(ct);
        }

        public async Task<CostCenterModel?> GetCostCenterByIdAsync(int id, CancellationToken ct = default)
        {
            return await context.CostCenters
                .FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task<CostCenterModel?> GetCostCenterByNameAsync(string name, CancellationToken ct = default)
        {
            return await  context.CostCenters.FirstOrDefaultAsync(c => c.CostUnitName == name, ct);
        }

        public async Task<Result> AddCostCenterAsync(CostCenterModel costCenter, CancellationToken ct = default)
        {
            try
            {
                await context.CostCenters.AddAsync(costCenter, ct);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("Cost center added: {@costCenter}", costCenter.CostUnitName);
                return operationResultFactory.SuccessAdded($"{EntityName}: '{costCenter.CostUnitName}'", costCenter.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured during add cost center: {@costCenter}", costCenter.CostUnitName);
                return operationResultFactory.FailedToAdd(EntityName, localizer["Exception"]);
            }
        }

        public async Task<Result> UpdateCostCenterAsync(CostCenterModel costCenter, CancellationToken ct = default)
        {
            try
            {
                context.CostCenters.Update(costCenter);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("Cost center updated: {@costCenter}", costCenter.CostUnitName);
                return operationResultFactory.SuccessUpdated(EntityName, costCenter.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured during update of cost center: {@costCenter}", costCenter.CostUnitName);
                return operationResultFactory.FailedToUpdate(EntityName, localizer["Exception"]);
            }
        }

        public async Task<Result> DeleteCostCenterAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var costUnit = await context.CostCenters.FindAsync([id], ct);
                if (costUnit == null)
                {
                    logger.LogError("Cost center Id '{ID}' not found", id);
                    return operationResultFactory.NotFound(EntityName, $"Id: '{id}' not found");
                }
                context.CostCenters.Remove(costUnit);
                await context.SaveChangesAsync(ct);
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