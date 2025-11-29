namespace TTCCashRegister.Data.SpecialItem;

public interface ISpecialItemService
{
    Task<List<SpecialItemModel>> GetAllSpecialItems();
    Task<SpecialItemModel?> GetSpecialPositionById(int id);
    Task AddSpecialPosition(SpecialItemModel specialPosition);
    Task UpdateSpecialPosition(SpecialItemModel specialPosition);
    Task<bool> DeleteSpecialPosition(int id);
}