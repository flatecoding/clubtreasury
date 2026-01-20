using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.SpecialItem
{
    public class SpecialItemService(CashDataContext context, ILogger<SpecialItemService> logger,
        IStringLocalizer<Translation> localizer, IOperationResultFactory operationResultFactory) : ISpecialItemService
    {
        private string EntityName => localizer["SpecialPosition"];
        public async Task<List<SpecialItemModel>> GetAllSpecialItems()
        {
            return await context.SpecialItems
                .Include(s => s.Transactions)  
                .ToListAsync();
        }

        public async Task<SpecialItemModel?> GetSpecialPositionById(int id)
        {
            return await context.SpecialItems
                .Include(s => s.Transactions)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IOperationResult> AddSpecialPositionAsync(SpecialItemModel specialPosition)
        {
            try
            {
                await context.SpecialItems.AddAsync(specialPosition);
                await context.SaveChangesAsync();

                logger.LogInformation("SpecialItem added: {@SpecialItem}", specialPosition);
                return operationResultFactory.SuccessAdded(
                    $"{EntityName}: '{specialPosition.Name}'",
                    specialPosition.Id);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to add SpecialItem");
                return operationResultFactory.FailedToAdd(EntityName, localizer["Exception"]);
            }
        }

        public async Task<IOperationResult> UpdateSpecialPositionAsync(SpecialItemModel specialPosition)
        {
            try
            {
                context.SpecialItems.Update(specialPosition);
                await context.SaveChangesAsync();

                logger.LogInformation("SpecialItem updated: {@SpecialItem}", specialPosition);
                return operationResultFactory.SuccessUpdated(
                    $"{EntityName}: '{specialPosition.Name}'",
                    specialPosition);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update SpecialItem");
                return operationResultFactory.FailedToUpdate(EntityName, localizer["Exception"]);
            }
        }

        public async Task<IOperationResult> DeleteSpecialPositionAsync(int id)
        {
            try
            {
                var entity = await context.SpecialItems.FindAsync(id);
                if (entity is null)
                {
                    logger.LogInformation("SpecialItem not found: {Id}", id);
                    return operationResultFactory.NotFound(
                        $"{EntityName} with Id '{id}' not found",
                        id);
                }

                context.SpecialItems.Remove(entity);
                await context.SaveChangesAsync();

                logger.LogInformation("SpecialItem deleted: {@SpecialItem}", entity);
                return operationResultFactory.SuccessDeleted(
                    $"{EntityName}: '{entity.Name}'",
                    id);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to delete SpecialItem");
                return operationResultFactory.FailedToDelete(EntityName, localizer["Exception"]);
            }
        }
    }
}
