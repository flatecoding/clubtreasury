using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.CashRegister;

public interface ICashRegisterService
{
    Task<List<CashRegisterModel>> GetAllCashRegisters(CancellationToken ct = default);
    Task<CashRegisterModel?> GetCashRegisterById(int id, CancellationToken ct = default);
    Task<CashRegisterModel?> GetFirstCashRegisterAsync(CancellationToken ct = default);
    Task<IOperationResult> AddCashRegister(CashRegisterModel cashRegisterModel, CancellationToken ct = default);
    Task<IOperationResult> UpdateCashRegister(CashRegisterModel cashRegisterModel, CancellationToken ct = default);
    Task<IOperationResult> DeleteCashRegisterAsync(int id, CancellationToken ct = default);
    Task<(byte[] Data, string ContentType)?> GetLogoAsync(int cashRegisterId, CancellationToken ct = default);
    Task<IOperationResult> UploadLogoAsync(int cashRegisterId, byte[] data, string contentType, CancellationToken ct = default);
    Task<IOperationResult> DeleteLogoAsync(int cashRegisterId, CancellationToken ct = default);
}