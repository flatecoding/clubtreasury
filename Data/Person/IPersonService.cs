using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Person;

public interface IPersonService
{
    Task<List<PersonModel>> GetAllPersonsAsync(CancellationToken ct = default);
    Task<PersonModel?> GetPersonById(int id, CancellationToken ct = default);
    Task<PersonModel?> GetFirstEntry(CancellationToken ct = default);
    Task<IOperationResult> AddPersonAsync(PersonModel person, CancellationToken ct = default);
    Task<IOperationResult> UpdatePersonAsync(PersonModel person, CancellationToken ct = default);
    Task<IOperationResult> DeletePersonAsync(int id, CancellationToken ct = default);
}