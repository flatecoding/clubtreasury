using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.SpecialItem;

public interface ISpecialItemService
{
    Task<List<SpecialItemModel>> GetAllSpecialItems(CancellationToken ct = default);
    Task<SpecialItemModel?> GetSpecialPositionById(int id, CancellationToken ct = default);
    Task<IOperationResult> AddSpecialPositionAsync(SpecialItemModel specialPosition, CancellationToken ct = default);
    Task<IOperationResult> UpdateSpecialPositionAsync(SpecialItemModel specialPosition, CancellationToken ct = default);
    Task<IOperationResult> DeleteSpecialPositionAsync(int id, CancellationToken ct = default);
}