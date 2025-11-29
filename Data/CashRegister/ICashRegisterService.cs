namespace TTCCashRegister.Data.CashRegister;

public interface ICashRegisterService
{
    Task<List<CashRegisterModel>> GetAllCashRegisters();
    Task<CashRegisterModel?> GetCashRegisterById(int id);
    Task<CashRegisterModel?> GetFirstEntry();
    Task AddCashRegister(CashRegisterModel cashRegisterModel);
    Task UpdateCashRegister(CashRegisterModel cashRegisterModel);
    Task<bool> DeleteCashRegister(int id);
}