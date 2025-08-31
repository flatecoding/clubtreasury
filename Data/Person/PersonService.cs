using Microsoft.EntityFrameworkCore;

namespace TTCCashRegister.Data.Person
{
    public class PersonService(CashDataContext context)
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
        }

        public async Task UpdatePerson(PersonModel personModel)
        {
            try
            {
                context.Persons.Update(personModel);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
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
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                return false;
            }
        }
    }
}
