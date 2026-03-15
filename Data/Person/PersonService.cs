using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Data.Person
{
    public class PersonService(CashDataContext context, ILogger<PersonService> logger,
        IStringLocalizer<Translation> localizer, IOperationResultFactory operationResultFactory) : IPersonService
    {
        private string EntityName => localizer["Person"];
        public async Task<List<PersonModel>> GetAllPersonsAsync(CancellationToken ct = default)
        {
            return await context.Persons
                .ToListAsync(ct);
        }

        public async Task<PersonModel?> GetPersonById(int id, CancellationToken ct = default)
        {
            return await context.Persons.FindAsync([id], ct);
        }

        public async Task<PersonModel?> GetFirstEntry(CancellationToken ct = default)
        {
            return await context.Persons.FirstOrDefaultAsync(ct);
        }

        public async Task<IOperationResult> AddPersonAsync(PersonModel personModel, CancellationToken ct = default)
        {
            try
            {
                await context.Persons.AddAsync(personModel, ct);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("Person added: {@PersonModel}", personModel);
                return operationResultFactory.SuccessAdded($"{EntityName}: '{personModel.Name}'", personModel.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while adding person: {PersonName}", personModel.Name);
                return operationResultFactory.FailedToAdd(EntityName, localizer["Exception"]);
            }
        }

        public async Task<IOperationResult> UpdatePersonAsync(PersonModel personModel, CancellationToken ct = default)
        {
            try
            {
                context.Persons.Update(personModel);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("Person updated: {@PersonModel}", personModel);
                return operationResultFactory.SuccessUpdated($"{EntityName}: '{personModel.Name}'", personModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while updating person");
                return operationResultFactory.FailedToUpdate(EntityName, localizer["Exception"]);
            }
        }

        public async Task<IOperationResult> DeletePersonAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var person = await context.Persons.FindAsync([id], ct);
                if (person is null)
                {
                    logger.LogInformation("Person not found: Id: '{ID}'", id);
                    return operationResultFactory.NotFound($"Person with Id '{id} not found", id);
                }
                context.Persons.Remove(person);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("Person deleted: {@PersonModel}", person);
                return operationResultFactory.SuccessDeleted($"{EntityName}: '{person.Name}'", id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while deleting person");
                return operationResultFactory.FailedToDelete(EntityName, localizer["Exception"]);
            }
        }
    }
}