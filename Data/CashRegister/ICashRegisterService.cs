using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.CashRegister;

public interface ICashRegisterService
{
    Task<List<CashRegisterModel>> GetAllCashRegisters(CancellationToken ct = default);
    Task<CashRegisterModel?> GetCashRegisterById(int id, CancellationToken ct = default);
    Task<CashRegisterModel?> GetFirstCashRegisterAsync(CancellationToken ct = default);
    Task<Result> AddCashRegister(CashRegisterModel cashRegisterModel, CancellationToken ct = default);
    Task<Result> UpdateCashRegister(CashRegisterModel cashRegisterModel, CancellationToken ct = default);
    Task<Result> DeleteCashRegisterAsync(int id, CancellationToken ct = default);
    Task<(byte[] Data, string ContentType)?> GetLogoAsync(int cashRegisterId, CancellationToken ct = default);
    Task<Result> UploadLogoAsync(int cashRegisterId, byte[] data, string contentType, CancellationToken ct = default);
    Task<Result> DeleteLogoAsync(int cashRegisterId, CancellationToken ct = default);
}