using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.CashRegister;

public interface ICashRegisterService
{
    Task<List<CashRegisterModel>> GetAllCashRegistersAsync(CancellationToken ct = default);
    Task<Dictionary<int, decimal>> GetCashRegisterBalancesAsync(CancellationToken ct = default);
    Task<CashRegisterModel?> GetCashRegisterByIdAsync(int id, CancellationToken ct = default);
    Task<CashRegisterModel?> GetFirstCashRegisterAsync(CancellationToken ct = default);
    Task<Result> AddCashRegisterAsync(CashRegisterModel cashRegisterModel, CancellationToken ct = default);
    Task<Result> UpdateCashRegisterAsync(CashRegisterModel cashRegisterModel, CancellationToken ct = default);
    Task<Result> DeleteCashRegisterAsync(int id, CancellationToken ct = default);
}