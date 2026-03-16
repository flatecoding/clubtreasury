using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.CashRegister;

public interface ICashRegisterLogoService
{
    Task<(byte[] Data, string ContentType)?> GetLogoAsync(int cashRegisterId, CancellationToken ct = default);
    Task<Result> UploadLogoAsync(int cashRegisterId, byte[] data, string contentType, CancellationToken ct = default);
    Task<Result> DeleteLogoAsync(int cashRegisterId, CancellationToken ct = default);
}
