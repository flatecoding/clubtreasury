using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.Import;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.IntegrationTests.Integration;

[TestFixture]
public class ImportBookingJournalIntegrationTests : IntegrationTestBase
{
    private IImportBookingJournalService _importService = null!;

    private static string TestResourcesPath =>
        Path.Combine(TestContext.CurrentContext.TestDirectory, "TestResources");

    [SetUp]
    public new async Task SetUp()
    {
        await base.SetUp();
        _importService = GetService<IImportBookingJournalService>();
    }

    private async Task<CashRegisterModel> CreateCashRegisterAsync(string name = "Import Test Register")
    {
        var cashRegister = new CashRegisterModel { Name = name };
        await GetDbContext().CashRegisters.AddAsync(cashRegister);
        await GetDbContext().SaveChangesAsync();
        return cashRegister;
    }

    [Test]
    public async Task ImportTransactions_WithValidFile_ShouldCreateTransactionsAndAllocations()
    {
        // Arrange
        var cashRegister = await CreateCashRegisterAsync();
        var filePath = Path.Combine(TestResourcesPath, "ValidBookingJournal.xlsx");
        await using var fileStream = File.OpenRead(filePath);

        // Act
        var result = await _importService.ImportTransactionsAsync(fileStream, "ValidBookingJournal.xlsx", cashRegister.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var transactions = await GetDbContext().Transactions
            .Include(t => t.Allocation)
            .ThenInclude(a => a.CostCenter)
            .Include(t => t.Allocation)
            .ThenInclude(a => a.Category)
            .Where(t => t.CashRegisterId == cashRegister.Id)
            .OrderBy(t => t.Documentnumber)
            .ToListAsync();

        transactions.Should().HaveCount(3);

        transactions[0].Documentnumber.Should().Be(1001);
        transactions[0].Description.Should().Be("Membership fee payment");
        transactions[0].Sum.Should().Be(50.00m);
        transactions[0].Allocation.CostCenter.CostUnitName.Should().Be("Membership");
        transactions[0].Allocation.Category.Name.Should().Be("Fees");

        transactions[1].Documentnumber.Should().Be(1002);
        transactions[1].Description.Should().Be("Office supplies");
        transactions[1].Sum.Should().Be(30.00m);
        transactions[1].Allocation.CostCenter.CostUnitName.Should().Be("Administration");
        transactions[1].Allocation.Category.Name.Should().Be("Supplies");

        transactions[2].Documentnumber.Should().Be(1003);
        transactions[2].Description.Should().Be("Event ticket sales");
        transactions[2].Sum.Should().Be(200.00m);
        transactions[2].Allocation.CostCenter.CostUnitName.Should().Be("Events");
        transactions[2].Allocation.Category.Name.Should().Be("Tickets");
    }

    [Test]
    public async Task ImportTransactions_WithValidFile_ShouldCreateCostCentersAndCategories()
    {
        // Arrange
        var cashRegister = await CreateCashRegisterAsync();
        var filePath = Path.Combine(TestResourcesPath, "ValidBookingJournal.xlsx");
        await using var fileStream = File.OpenRead(filePath);

        // Act
        await _importService.ImportTransactionsAsync(fileStream, "ValidBookingJournal.xlsx", cashRegister.Id);

        // Assert - 3 distinct cost centers should be created
        var costCenters = await GetDbContext().CostCenters.ToListAsync();
        costCenters.Should().HaveCount(3);
        costCenters.Select(c => c.CostUnitName).Should()
            .Contain(["Membership", "Administration", "Events"]);

        // 3 distinct categories
        var categories = await GetDbContext().Categories.ToListAsync();
        categories.Should().HaveCount(3);
        categories.Select(c => c.Name).Should()
            .Contain(["Fees", "Supplies", "Tickets"]);
    }

    [Test]
    public async Task ImportTransactions_WhenImportedTwice_ShouldSkipExistingDocumentNumbers()
    {
        // Arrange
        var cashRegister = await CreateCashRegisterAsync();
        var filePath = Path.Combine(TestResourcesPath, "ValidBookingJournal.xlsx");

        // First import
        await using (var firstStream = File.OpenRead(filePath))
        {
            await _importService.ImportTransactionsAsync(firstStream, "ValidBookingJournal.xlsx", cashRegister.Id);
        }

        // Act - Second import of same file
        await using var secondStream = File.OpenRead(filePath);
        var result = await _importService.ImportTransactionsAsync(secondStream, "ValidBookingJournal.xlsx", cashRegister.Id);

        // Assert - should succeed but not create duplicates
        result.IsSuccess.Should().BeTrue();

        var transactionCount = await GetDbContext().Transactions
            .CountAsync(t => t.CashRegisterId == cashRegister.Id);
        transactionCount.Should().Be(3);
    }

    [Test]
    public async Task ImportTransactions_SameFileIntoDifferentCashRegisters_ShouldImportIntoBoth()
    {
        // Arrange
        var firstCashRegister = await CreateCashRegisterAsync("First Register");
        var secondCashRegister = await CreateCashRegisterAsync("Second Register");
        var filePath = Path.Combine(TestResourcesPath, "ValidBookingJournal.xlsx");

        // Import into first register
        await using (var firstStream = File.OpenRead(filePath))
        {
            await _importService.ImportTransactionsAsync(firstStream, "ValidBookingJournal.xlsx", firstCashRegister.Id);
        }

        // Act - Import into second register
        await using var secondStream = File.OpenRead(filePath);
        var result = await _importService.ImportTransactionsAsync(secondStream, "ValidBookingJournal.xlsx", secondCashRegister.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var firstRegisterCount = await GetDbContext().Transactions
            .CountAsync(t => t.CashRegisterId == firstCashRegister.Id);
        var secondRegisterCount = await GetDbContext().Transactions
            .CountAsync(t => t.CashRegisterId == secondCashRegister.Id);

        firstRegisterCount.Should().Be(3);
        secondRegisterCount.Should().Be(3);
    }

    [Test]
    public async Task ImportTransactions_WithDuplicateRows_ShouldFailValidation()
    {
        // Arrange
        var cashRegister = await CreateCashRegisterAsync();
        var filePath = Path.Combine(TestResourcesPath, "DuplicateRowBookingJournal.xlsx");
        await using var fileStream = File.OpenRead(filePath);

        // Act
        var result = await _importService.ImportTransactionsAsync(fileStream, "DuplicateRowBookingJournal.xlsx", cashRegister.Id);

        // Assert
        result.IsFailure.Should().BeTrue();

        var transactionCount = await GetDbContext().Transactions
            .CountAsync(t => t.CashRegisterId == cashRegister.Id);
        transactionCount.Should().Be(0);
    }

    [Test]
    public async Task ImportTransactions_WithNullStream_ShouldFail()
    {
        // Arrange
        var cashRegister = await CreateCashRegisterAsync();

        // Act
        var result = await _importService.ImportTransactionsAsync(null, "nonexistent.xlsx", cashRegister.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public async Task ImportTransactions_WithInvalidCashRegisterId_ShouldFail()
    {
        // Arrange
        var filePath = Path.Combine(TestResourcesPath, "ValidBookingJournal.xlsx");
        await using var fileStream = File.OpenRead(filePath);

        // Act
        var result = await _importService.ImportTransactionsAsync(fileStream, "ValidBookingJournal.xlsx", 99999);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
