using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.CostCenter;
using ClubTreasury.Data.SpecialItem;

namespace ClubTreasury.Data.Transaction;

public record TransactionReferenceData(
    List<CashRegisterModel> CashRegisters,
    List<CostCenterModel> CostCenters,
    List<SpecialItemModel> SpecialItems);
