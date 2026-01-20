using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.SpecialItem;

public interface ISpecialItemService
{
    Task<List<SpecialItemModel>> GetAllSpecialItems();
    Task<SpecialItemModel?> GetSpecialPositionById(int id);
    Task<IOperationResult> AddSpecialPositionAsync(SpecialItemModel specialPosition);
    Task<IOperationResult> UpdateSpecialPositionAsync(SpecialItemModel specialPosition);
    Task<IOperationResult> DeleteSpecialPositionAsync(int id);
}