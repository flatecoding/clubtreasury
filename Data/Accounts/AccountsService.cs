using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.Accounts;

public class AccountsService
{
    private readonly CashDataContext _context;

    public AccountsService(CashDataContext context)
    {
        _context = context;
    }

    public async Task<AccountsModel?> GetByIdAsync(int id)
    {
        return await _context.Accounts
            .Include(a => a.CostCenter)
            .Include(a => a.BasicUnit)
            .Include(a => a.UnitDetails)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<AccountsModel>> GetAllAccountsAsync()
    {
        return await _context.Accounts
            .Include(a => a.CostCenter)
            .Include(a => a.BasicUnit)
            .Include(a => a.UnitDetails)
            .ToListAsync();
    }

    public async Task<AccountsModel> EnsureAccountExistsAsync(AccountsModel account)
    {
        var existing = await _context.Accounts.FirstOrDefaultAsync(a =>
            a.CostCenterId == account.CostCenterId &&
            a.BasicUnitId == account.BasicUnitId &&
            a.UnitDetailsId == account.UnitDetailsId);

        if (existing != null)
        {
            return existing;
        }

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<AccountsModel> AddAsync(AccountsModel account)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null) return false;

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
        return true;
    }
}