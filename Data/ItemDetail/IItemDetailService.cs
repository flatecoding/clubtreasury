using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.ItemDetail;

public interface IItemDetailService
{
    Task<List<ItemDetailModel>> GetAllItemDetailsAsync(CancellationToken ct = default);
    Task<ItemDetailModel?> GetItemDetailByIdAsync(int id, CancellationToken ct = default);
    Task<ItemDetailModel?> GetItemDetailByNameAsync(string name, CancellationToken ct = default);
    Task<List<ItemDetailModel>> GetItemDetailByCategoryIdAsync(int categoryId, CancellationToken ct = default);
    Task<Result> AddItemDetailAsync(ItemDetailModel itemDetail, CancellationToken ct = default);
    Task<Result> UpdateItemDetailAsync(ItemDetailModel itemDetail, CancellationToken ct = default);
    Task<Result> DeleteItemDetailAsync(int id, CancellationToken ct = default);
}