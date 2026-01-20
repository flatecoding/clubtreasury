using TTCCashRegister.Data.Notification;
using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.CashRegister;

public interface ICashRegisterService
{
    Task<List<CashRegisterModel>> GetAllCashRegisters();
    Task<CashRegisterModel?> GetCashRegisterById(int id);
    Task<CashRegisterModel?> GetFirstEntry();
    Task<IOperationResult> AddCashRegister(CashRegisterModel cashRegisterModel);
    Task<IOperationResult> UpdateCashRegister(CashRegisterModel cashRegisterModel);
    Task<IOperationResult> DeleteCashRegisterAsync(int id);
}