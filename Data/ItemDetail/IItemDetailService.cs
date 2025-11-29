namespace TTCCashRegister.Data.ItemDetail;

public interface IItemDetailService
{
    Task<List<ItemDetailModel>> GetAllItemDetailsAsync();
    Task<ItemDetailModel?> GetItemDetailByIdAsync(int id);
    Task<List<ItemDetailModel>> GetItemDetailByCategoryIdAsync(int categoryId);
    Task<ItemDetailModel?> AddItemDetailAsync(ItemDetailModel itemDetail);
    Task<bool> UpdateItemDetailAsync(ItemDetailModel itemDetail);
    Task<bool> DeleteItemDetailAsync(int id);
}