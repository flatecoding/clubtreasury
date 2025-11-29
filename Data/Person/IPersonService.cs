namespace TTCCashRegister.Data.Person;

public interface IPersonService
{
    Task<List<PersonModel>> GetAllPersonsAsync();
    Task<PersonModel?> GetPersonById(int id);
    Task<PersonModel?> GetFirstEntry();
    Task AddPerson(PersonModel personModel);
    Task UpdatePerson(PersonModel personModel);
    Task<bool> DeletePerson(int id);
}