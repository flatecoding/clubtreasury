using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using ClubTreasury.Data.Allocation;
using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.Category;
using ClubTreasury.Data.CostCenter;
using ClubTreasury.Data.OperationResult;
using ClubTreasury.Data.Transaction;

namespace ClubTreasury.IntegrationTests.Integration;

[TestFixture]
public class TransactionServiceIntegrationTests : IntegrationTestBase
{
    private ITransactionService _transactionService = null!;

    [SetUp]
    public new async Task SetUp()
    {
        await base.SetUp();
        _transactionService = GetService<ITransactionService>();
    }

    private async Task<(CashRegisterModel CashRegister, AllocationModel Allocation)> CreateTestDataAsync()
    {
        var cashRegister = new CashRegisterModel { Name = "Test Register" };
        var costCenter = new CostCenterModel { CostUnitName = "Test Cost Center" };
        var category = new CategoryModel { Name = "Test Category" };

        await GetDbContext().CashRegisters.AddAsync(cashRegister);
        await GetDbContext().CostCenters.AddAsync(costCenter);
        await GetDbContext().Categories.AddAsync(category);
        await GetDbContext().SaveChangesAsync();

        var allocation = new AllocationModel
        {
            CostCenterId = costCenter.Id,
            CategoryId = category.Id
        };
        await GetDbContext().Allocations.AddAsync(allocation);
        await GetDbContext().SaveChangesAsync();

        return (cashRegister, allocation);
    }

    [Test]
    public async Task AddTransaction_WithValidData_ShouldPersistToDatabase()
    {
        // Arrange
        var (cashRegister, allocation) = await CreateTestDataAsync();

        var transaction = new TransactionModel
        {
            Documentnumber = 1001,
            Description = "Integration Test Transaction",
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id,
            Sum = 100.50m,
            AccountMovement = 100.50m,
            Date = DateOnly.FromDateTime(DateTime.Now)
        };

        // Act
        var result = await _transactionService.AddTransactionAsync(transaction);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var savedTransaction = await GetDbContext().Transactions
            .Include(t => t.Allocation)
            .ThenInclude(a => a.CostCenter)
            .FirstOrDefaultAsync(t => t.Documentnumber == 1001);

        savedTransaction.Should().NotBeNull();
        savedTransaction.Description.Should().Be("Integration Test Transaction");
        savedTransaction.Sum.Should().Be(100.50m);
        savedTransaction.Allocation.CostCenter.CostUnitName.Should().Be("Test Cost Center");
    }

    [Test]
    public async Task UpdateTransaction_WhenChangingAllocation_ShouldSucceed()
    {
        // Arrange - Create initial data
        var (cashRegister, allocation) = await CreateTestDataAsync();

        // Create a second allocation with different cost center
        var secondCostCenter = new CostCenterModel { CostUnitName = "Second Cost Center" };
        var secondCategory = new CategoryModel { Name = "Second Category" };
        await GetDbContext().CostCenters.AddAsync(secondCostCenter);
        await GetDbContext().Categories.AddAsync(secondCategory);
        await GetDbContext().SaveChangesAsync();

        var secondAllocation = new AllocationModel
        {
            CostCenterId = secondCostCenter.Id,
            CategoryId = secondCategory.Id
        };
        await GetDbContext().Allocations.AddAsync(secondAllocation);
        await GetDbContext().SaveChangesAsync();

        // Create a transaction with initial allocation
        var originalTransaction = new TransactionModel
        {
            Documentnumber = 2001,
            Description = "Transaction to update",
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id,
            Sum = 50.00m,
            AccountMovement = 50.00m,
            Date = DateOnly.FromDateTime(DateTime.Now)
        };
        await GetDbContext().Transactions.AddAsync(originalTransaction);
        await GetDbContext().SaveChangesAsync();

        // Clear the change tracker to simulate fresh context like in real app
        GetDbContext().ChangeTracker.Clear();

        // Load transaction with Include (simulating GetTransactionByIdAsync behavior)
        var loadedTransaction = await GetDbContext().Transactions
            .Include(t => t.Allocation)
            .FirstAsync(t => t.Id == originalTransaction.Id);

        // Change allocation to secondAllocation
        loadedTransaction.AllocationId = secondAllocation.Id;
        loadedTransaction.Description = "Updated allocation";

        // Act
        var result = await _transactionService.UpdateTransactionAsync(loadedTransaction);

        // Assert
        result.IsSuccess.Should().BeTrue();

        GetDbContext().ChangeTracker.Clear();
        var updatedTransaction = await GetDbContext().Transactions
            .Include(t => t.Allocation)
            .ThenInclude(a => a.CostCenter)
            .FirstAsync(t => t.Id == originalTransaction.Id);

        updatedTransaction.AllocationId.Should().Be(secondAllocation.Id);
        updatedTransaction.Allocation.CostCenter.CostUnitName.Should().Be("Second Cost Center");
        updatedTransaction.Description.Should().Be("Updated allocation");
    }

    [Test]
    public async Task UpdateTransaction_WhenChangingOnlyDate_ShouldSucceed()
    {
        // Arrange - This tests the user's scenario: editing just the date
        // when the transaction was loaded with Include(Allocation)
        var (cashRegister, allocation) = await CreateTestDataAsync();

        var transaction = new TransactionModel
        {
            Documentnumber = 2002,
            Description = "Transaction to update date",
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id,
            Sum = 50.00m,
            AccountMovement = 50.00m,
            Date = new DateOnly(2024, 1, 15)
        };
        await GetDbContext().Transactions.AddAsync(transaction);
        await GetDbContext().SaveChangesAsync();

        GetDbContext().ChangeTracker.Clear();

        // Load transaction with all Includes (simulating UI load via GetTransactionByIdAsync)
        var loadedTransaction = await GetDbContext().Transactions
            .Include(t => t.Allocation)
            .ThenInclude(a => a.CostCenter)
            .Include(t => t.Allocation)
            .ThenInclude(a => a.Category)
            .FirstAsync(t => t.Id == transaction.Id);

        // Only change the date - do NOT change allocation
        loadedTransaction.Date = new DateOnly(2024, 2, 20);

        // Act
        var result = await _transactionService.UpdateTransactionAsync(loadedTransaction);

        // Assert
        result.IsSuccess.Should().BeTrue();

        GetDbContext().ChangeTracker.Clear();
        var updatedTransaction = await GetDbContext().Transactions.FirstAsync(t => t.Id == transaction.Id);
        updatedTransaction.Date.Should().Be(new DateOnly(2024, 2, 20));
    }

    [Test]
    public async Task DeleteTransaction_ShouldRemoveFromDatabase()
    {
        // Arrange
        var (cashRegister, allocation) = await CreateTestDataAsync();

        var transaction = new TransactionModel
        {
            Documentnumber = 4001,
            Description = "Transaction to delete",
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id,
            Sum = 25.00m,
            AccountMovement = 25.00m,
            Date = DateOnly.FromDateTime(DateTime.Now)
        };
        await GetDbContext().Transactions.AddAsync(transaction);
        await GetDbContext().SaveChangesAsync();
        var transactionId = transaction.Id;

        // Act
        var result = await _transactionService.DeleteTransactionAsync(transactionId);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deletedTransaction = await GetDbContext().Transactions.FindAsync(transactionId);
        deletedTransaction.Should().BeNull();
    }

    [Test]
    public async Task AddTransaction_WithDuplicateDocumentNumber_ShouldReturnWarning()
    {
        // Arrange
        var (cashRegister, allocation) = await CreateTestDataAsync();

        var existingTransaction = new TransactionModel
        {
            Documentnumber = 5001,
            Description = "First transaction",
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id,
            Sum = 10.00m,
            AccountMovement = 10.00m,
            Date = DateOnly.FromDateTime(DateTime.Now)
        };
        await GetDbContext().Transactions.AddAsync(existingTransaction);
        await GetDbContext().SaveChangesAsync();

        var duplicateTransaction = new TransactionModel
        {
            Documentnumber = 5001, // Same document number
            Description = "Second transaction",
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id,
            Sum = 20.00m,
            AccountMovement = 20.00m,
            Date = DateOnly.FromDateTime(DateTime.Now)
        };

        // Act
        var result = await _transactionService.AddTransactionAsync(duplicateTransaction);

        // Assert - AlreadyExists returns failure per ResultFactory
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task AddTransaction_WithSameDocumentNumberInDifferentCashRegister_ShouldSucceed()
    {
        // Arrange
        var (firstCashRegister, allocation) = await CreateTestDataAsync();

        var secondCashRegister = new CashRegisterModel { Name = "Second Register" };
        await GetDbContext().CashRegisters.AddAsync(secondCashRegister);
        await GetDbContext().SaveChangesAsync();

        var existingTransaction = new TransactionModel
        {
            Documentnumber = 7001,
            Description = "Transaction in first register",
            CashRegisterId = firstCashRegister.Id,
            AllocationId = allocation.Id,
            Sum = 10.00m,
            AccountMovement = 10.00m,
            Date = DateOnly.FromDateTime(DateTime.Now)
        };
        await GetDbContext().Transactions.AddAsync(existingTransaction);
        await GetDbContext().SaveChangesAsync();

        var transactionInOtherRegister = new TransactionModel
        {
            Documentnumber = 7001, // Same document number, different cash register
            Description = "Transaction in second register",
            CashRegisterId = secondCashRegister.Id,
            AllocationId = allocation.Id,
            Sum = 30.00m,
            AccountMovement = 30.00m,
            Date = DateOnly.FromDateTime(DateTime.Now)
        };

        // Act
        var result = await _transactionService.AddTransactionAsync(transactionInOtherRegister);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task GetTransactionsForExport_ShouldFilterByDateRangeAndCashRegister()
    {
        // Arrange
        var (cashRegister, allocation) = await CreateTestDataAsync();

        var januaryTransaction = new TransactionModel
        {
            Documentnumber = 6001,
            Description = "January transaction",
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id,
            Sum = 100.00m,
            AccountMovement = 100.00m,
            Date = new DateOnly(2024, 1, 15)
        };
        var februaryTransaction = new TransactionModel
        {
            Documentnumber = 6002,
            Description = "February transaction",
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id,
            Sum = 200.00m,
            AccountMovement = 200.00m,
            Date = new DateOnly(2024, 2, 15)
        };
        var marchTransaction = new TransactionModel
        {
            Documentnumber = 6003,
            Description = "March transaction",
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id,
            Sum = 300.00m,
            AccountMovement = 300.00m,
            Date = new DateOnly(2024, 3, 15)
        };

        await GetDbContext().Transactions.AddRangeAsync(januaryTransaction, februaryTransaction, marchTransaction);
        await GetDbContext().SaveChangesAsync();

        // Act
        var result = await _transactionService.GetTransactionsForExportAsync(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 2, 28),
            cashRegister.Id);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList.Select(t => t.Documentnumber).Should().Contain([6001, 6002]);
        resultList.Select(t => t.Documentnumber).Should().NotContain(6003);
    }
}
