using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Person;

public interface IPersonService
{
    Task<List<PersonModel>> GetAllPersonsAsync(CancellationToken ct = default);
    Task<PersonModel?> GetPersonByIdAsync(int id, CancellationToken ct = default);
    Task<PersonModel?> GetFirstEntryAsync(CancellationToken ct = default);
    Task<Result> AddPersonAsync(PersonModel peson, CancellationToken ct = default);
    Task<Result> UpdatePersonAsync(PersonModel person, CancellationToken ct = default);
    Task<Result> DeletePersonAsync(int id, CancellationToken ct = default);
}