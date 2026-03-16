using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.Category;
using ClubTreasury.Data.CostCenter;
using ClubTreasury.Data.ItemDetail;
using ClubTreasury.Data.SpecialItem;

namespace ClubTreasury.Data.Transaction;

public record TransactionFormSelections(
    CashRegisterModel CashRegister,
    CostCenterModel CostCenter,
    CategoryModel Category,
    ItemDetailModel? ItemDetail,
    SpecialItemModel? SpecialItem,
    DateTime Date);
