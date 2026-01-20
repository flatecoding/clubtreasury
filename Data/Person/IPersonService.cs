using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Data.Person;

public interface IPersonService
{
    Task<List<PersonModel>> GetAllPersonsAsync();
    Task<PersonModel?> GetPersonById(int id);
    Task<PersonModel?> GetFirstEntry();
    Task<IOperationResult> AddPersonAsync(PersonModel person);
    Task<IOperationResult> UpdatePersonAsync(PersonModel person);
    Task<IOperationResult> DeletePersonAsync(int id);
}