using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.CashRegister;

public interface ICashRegisterService
{
    Task<List<CashRegisterModel>> GetAllCashRegisters();
    Task<CashRegisterModel?> GetCashRegisterById(int id);
    Task<CashRegisterModel?> GetFirstCashRegisterAsync();
    Task<IOperationResult> AddCashRegister(CashRegisterModel cashRegisterModel);
    Task<IOperationResult> UpdateCashRegister(CashRegisterModel cashRegisterModel);
    Task<IOperationResult> DeleteCashRegisterAsync(int id);
    Task<(byte[] Data, string ContentType)?> GetLogoAsync(int cashRegisterId);
    Task<IOperationResult> UploadLogoAsync(int cashRegisterId, byte[] data, string contentType);
    Task<IOperationResult> DeleteLogoAsync(int cashRegisterId);
}