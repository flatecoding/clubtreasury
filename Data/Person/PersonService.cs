using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.Person
{
    public class PersonService(CashDataContext context, ILogger<PersonService> logger)
    {
        public async Task<List<PersonModel>> GetAllPersonsAsync()
        {
            return await context.Persons
                .ToListAsync();
        }

        public async Task<PersonModel?> GetPersonById(int id)
        {
            return await context.Persons.FindAsync(id);
        }

        public async Task<PersonModel?> GetFirstEntry()
        {
            return await context.Persons.FirstOrDefaultAsync();
        }

        public async Task AddPerson(PersonModel personModel)
        {
            try
            {
                await context.Persons.AddAsync(personModel);
                await context.SaveChangesAsync();
                logger.LogInformation("Person added: {@PersonModel}", personModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                logger.LogError($"Error: {ex}");
            }
        }

        public async Task UpdatePerson(PersonModel personModel)
        {
            try
            {
                context.Persons.Update(personModel);
                await context.SaveChangesAsync();
                logger.LogInformation("Person updated: {@PersonModel}", personModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                logger.LogError($"Error: {ex}");
            }
        }

        public async Task<bool> DeletePerson(int id)
        {
            try
            {
                var person = await context.Persons.FindAsync(id);
                if (person == null)
                {
                    return false;
                }

                context.Persons.Remove(person);
                await context.SaveChangesAsync();
                logger.LogInformation("Person deleted: {@PersonModel}", person);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                logger.LogError($"Error: {ex}");
                return false;
            }
        }
    }
}
