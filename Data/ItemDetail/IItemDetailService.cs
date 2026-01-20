using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.ItemDetail;

public interface IItemDetailService
{
    Task<List<ItemDetailModel>> GetAllItemDetailsAsync();
    Task<ItemDetailModel?> GetItemDetailByIdAsync(int id);
    Task<List<ItemDetailModel>> GetItemDetailByCategoryIdAsync(int categoryId);
    Task<IOperationResult> AddItemDetailAsync(ItemDetailModel itemDetail);
    Task<IOperationResult> UpdateItemDetailAsync(ItemDetailModel itemDetail);
    Task<IOperationResult> DeleteItemDetailAsync(int id);
}