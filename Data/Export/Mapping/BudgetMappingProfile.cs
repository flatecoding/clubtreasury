using AutoMapper;
using TTCCashRegister.Data.Category;
using TTCCashRegister.Data.CostCenter;
using TTCCashRegister.Data.Export.DTOs;
using TTCCashRegister.Data.ItemDetail;
using TTCCashRegister.Data.Person;
using TTCCashRegister.Data.Transaction;
using TTCCashRegister.Data.TransactionDetails;

namespace TTCCashRegister.Data.Export.Mapping;

public class BudgetMappingProfile : Profile
{
   public BudgetMappingProfile()
{
    //
    // FLAT DTO benötigt:
    // CostCenterId
    // CostCenterName
    // CategoryId
    // CategoryName
    // ItemDetailId
    // ItemDetailName
    // Amount
    // PersonId
    // PersonName
    //

    // TRANSACTION (ohne Person)
    CreateMap<TransactionModel, BudgetFlatEntryDto>()
        .ForMember(d => d.CostCenterId,
            o => o.MapFrom(s => s.Allocation.CostCenter.Id))
        .ForMember(d => d.CostCenterName,
            o => o.MapFrom(s => s.Allocation.CostCenter.CostUnitName))

        .ForMember(d => d.CategoryId,
            o => o.MapFrom(s => s.Allocation.Category.Id))
        .ForMember(d => d.CategoryName,
            o => o.MapFrom(s => s.Allocation.Category.Name))

        .ForMember(d => d.ItemDetailId,
            o => o.MapFrom(s => s.Allocation.ItemDetail.Id))
        .ForMember(d => d.ItemDetailName,
            o => o.MapFrom(s => s.Allocation.ItemDetail.CostDetails))

        .ForMember(d => d.Amount,
            o => o.MapFrom(s => s.AccountMovement))

        .ForMember(d => d.PersonId, o => o.Ignore())
        .ForMember(d => d.PersonName, o => o.Ignore());


    // TRANSACTION DETAILS (mit Person)
    CreateMap<TransactionDetailsModel, BudgetFlatEntryDto>()
        .ForMember(d => d.CostCenterId,
            o => o.MapFrom(s => s.Transaction.Allocation.CostCenter.Id))
        .ForMember(d => d.CostCenterName,
            o => o.MapFrom(s => s.Transaction.Allocation.CostCenter.CostUnitName))

        .ForMember(d => d.CategoryId,
            o => o.MapFrom(s => s.Transaction.Allocation.Category.Id))
        .ForMember(d => d.CategoryName,
            o => o.MapFrom(s => s.Transaction.Allocation.Category.Name))

        .ForMember(d => d.ItemDetailId,
            o => o.MapFrom(s => s.Transaction.Allocation.ItemDetail.Id))
        .ForMember(d => d.ItemDetailName,
            o => o.MapFrom(s => s.Transaction.Allocation.ItemDetail.CostDetails))

        .ForMember(d => d.Amount,
            o => o.MapFrom(s => s.Sum))

        .ForMember(d => d.PersonId,
            o => o.MapFrom(s => s.Person.Id))
        .ForMember(d => d.PersonName,
            o => o.MapFrom(s => s.Person.Name));
}

}