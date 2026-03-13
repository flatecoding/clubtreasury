using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.CashRegister
{
    public class CashRegisterService(
        CashDataContext context,
        ILogger<CashRegisterService> logger,
        IOperationResultFactory operationResultFactory,
        IStringLocalizer<Translation> localizer)
        : ICashRegisterService
    {
        private string EntityName => localizer["CashRegister"];

        public async Task<List<CashRegisterModel>> GetAllCashRegisters(CancellationToken ct = default)
        {
            return await context.CashRegisters
                .Include(t => t.Transactions)
                .ToListAsync(ct);
        }

        public async Task<CashRegisterModel?> GetCashRegisterById(int id, CancellationToken ct = default)
        {
            var cashRegister = await context.CashRegisters.FindAsync([id], ct);
            if (cashRegister is not null)
            {
                logger.LogInformation("Cash register found: {@CashRegister}", cashRegister.Name);
                return cashRegister;
            }

            logger.LogError("Cash register not found with id '{CashRegisterId}'", id);
            return null;
        }

        public async Task<CashRegisterModel?> GetFirstCashRegisterAsync(CancellationToken ct = default)
        {
            var cashRegister = await context.CashRegisters.FirstOrDefaultAsync(ct);
            if (cashRegister is not null)
            {
                logger.LogInformation("First cash register found: {@CashRegister}", cashRegister.Name);
                return cashRegister;
            }

            logger.LogError("No cash register data available");
            return null;
        }

        public async Task<IOperationResult> AddCashRegister(CashRegisterModel cashRegisterModel, CancellationToken ct = default)
        {
            try
            {
                await context.CashRegisters.AddAsync(cashRegisterModel, ct);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("Cash register added: {@CashRegister}", cashRegisterModel.Name);
                return operationResultFactory.SuccessAdded($"{EntityName}: '{cashRegisterModel.Name}'", cashRegisterModel.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while adding cash register entry to database.");
                return operationResultFactory.FailedToAdd(EntityName, localizer["Exception"]);
            }
        }

        public async Task<IOperationResult> UpdateCashRegister(CashRegisterModel cashRegisterModel, CancellationToken ct = default)
        {
            try
            {
                context.CashRegisters.Update(cashRegisterModel);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("Cash register updated: {@CashRegister}", cashRegisterModel.Name);
                return operationResultFactory.SuccessUpdated($"{EntityName}: '{cashRegisterModel.Name}'", cashRegisterModel.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during update of cash register: {@CashRegister}",
                    cashRegisterModel);
                return operationResultFactory.FailedToUpdate(EntityName, localizer["Exception"]);
            }
        }


        public async Task<IOperationResult> DeleteCashRegisterAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var cashRegister = await context.CashRegisters.FindAsync([id], ct);
                if (cashRegister is null)
                {
                    logger.LogWarning("Cash register with Id {CashRegisterId} not found.", id);
                    return operationResultFactory.NotFound(EntityName, id);
                }

                context.CashRegisters.Remove(cashRegister);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("Cash register deleted: {@CashRegister}", cashRegister.Name);
                return operationResultFactory.SuccessDeleted(EntityName, id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during delete of cash register: {Id}", id);
                return operationResultFactory.FailedToDelete(EntityName, localizer["Exception"]);
            }
        }

        public async Task<(byte[] Data, string ContentType)?> GetLogoAsync(int cashRegisterId, CancellationToken ct = default)
        {
            var logo = await context.CashRegisterLogos
                .FirstOrDefaultAsync(l => l.CashRegisterId == cashRegisterId, ct);
            return logo is not null ? (logo.Data, logo.ContentType) : null;
        }

        public async Task<IOperationResult> UploadLogoAsync(int cashRegisterId, byte[] data, string contentType, CancellationToken ct = default)
        {
            try
            {
                var logo = await context.CashRegisterLogos
                    .FirstOrDefaultAsync(l => l.CashRegisterId == cashRegisterId, ct);

                if (logo is not null)
                {
                    logo.Data = data;
                    logo.ContentType = contentType;
                }
                else
                {
                    logo = new CashRegisterLogoModel
                    {
                        CashRegisterId = cashRegisterId,
                        Data = data,
                        ContentType = contentType
                    };
                    context.CashRegisterLogos.Add(logo);
                }

                await context.SaveChangesAsync(ct);
                logger.LogInformation("Logo uploaded for cash register {CashRegisterId}", cashRegisterId);
                return operationResultFactory.SuccessUpdated(EntityName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading logo for cash register {CashRegisterId}", cashRegisterId);
                return operationResultFactory.FailedToUpdate(EntityName, localizer["Exception"]);
            }
        }

        public async Task<IOperationResult> DeleteLogoAsync(int cashRegisterId, CancellationToken ct = default)
        {
            try
            {
                var logo = await context.CashRegisterLogos
                    .FirstOrDefaultAsync(l => l.CashRegisterId == cashRegisterId, ct);

                if (logo is not null)
                {
                    context.CashRegisterLogos.Remove(logo);
                    await context.SaveChangesAsync(ct);
                    logger.LogInformation("Logo deleted for cash register {CashRegisterId}", cashRegisterId);
                }

                return operationResultFactory.SuccessDeleted(EntityName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting logo for cash register {CashRegisterId}", cashRegisterId);
                return operationResultFactory.FailedToDelete(EntityName, localizer["Exception"]);
            }
        }
    }
}