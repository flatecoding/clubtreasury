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
}