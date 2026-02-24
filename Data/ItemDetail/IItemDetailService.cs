using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.ItemDetail;

public interface IItemDetailService
{
    Task<List<ItemDetailModel>> GetAllItemDetailsAsync();
    Task<ItemDetailModel?> GetItemDetailByIdAsync(int id);
    Task<ItemDetailModel?> GetItemDetailByNameAsync(string name);
    Task<List<ItemDetailModel>> GetItemDetailByCategoryIdAsync(int categoryId);
    Task<IOperationResult> AddItemDetailAsync(ItemDetailModel itemDetail);
    Task<IOperationResult> UpdateItemDetailAsync(ItemDetailModel itemDetail);
    Task<IOperationResult> DeleteItemDetailAsync(int id);
}