using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.CashRegister;

public class CashRegisterLogoService(
    CashDataContext context,
    ILogger<CashRegisterLogoService> logger,
    IResultFactory resultFactory,
    IStringLocalizer<Translation> localizer)
    : ICashRegisterLogoService
{
    private string EntityName => localizer["CashRegister"];

    public async Task<(byte[] Data, string ContentType)?> GetLogoAsync(int cashRegisterId, CancellationToken ct = default)
    {
        var logo = await context.CashRegisterLogos
            .FirstOrDefaultAsync(l => l.CashRegisterId == cashRegisterId, ct);
        return logo is not null ? (logo.Data, logo.ContentType) : null;
    }

    public async Task<Result> UploadLogoAsync(int cashRegisterId, byte[] data, string contentType, CancellationToken ct = default)
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
            return resultFactory.SuccessUpdated(EntityName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading logo for cash register {CashRegisterId}", cashRegisterId);
            return resultFactory.FailedToUpdate(EntityName, localizer["Exception"]);
        }
    }

    public async Task<Result> DeleteLogoAsync(int cashRegisterId, CancellationToken ct = default)
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

            return resultFactory.SuccessDeleted(EntityName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting logo for cash register {CashRegisterId}", cashRegisterId);
            return resultFactory.FailedToDelete(EntityName, localizer["Exception"]);
        }
    }
}
