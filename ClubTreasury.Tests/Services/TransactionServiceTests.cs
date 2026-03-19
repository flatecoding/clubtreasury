using FakeItEasy;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ClubTreasury.Data;
using ClubTreasury.Data.Allocation;
using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.Category;
using ClubTreasury.Data.CostCenter;
using ClubTreasury.Data.OperationResult;
using ClubTreasury.Data.Transaction;

namespace ClubTreasury.Tests.Services;

[TestFixture]
public class TransactionServiceTests
{
    private CashDataContext _context = null!;
    private IAllocationService _allocationService = null!;
    private ILogger<TransactionService> _logger = null!;
    private IResultFactory _resultFactory = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private TransactionService _sut = null!;
    private bool _contextDisposed;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CashDataContext(options);
        _contextDisposed = false;
        _allocationService = A.Fake<IAllocationService>();
        _logger = A.Fake<ILogger<TransactionService>>();
        _resultFactory = A.Fake<IResultFactory>();
        _localizer = A.Fake<IStringLocalizer<Translation>>();

        A.CallTo(() => _localizer["Transaction"])
            .Returns(new LocalizedString("Transaction", "Transaction"));
        A.CallTo(() => _localizer["DocumentNumber"])
            .Returns(new LocalizedString("DocumentNumber", "Document Number"));
        A.CallTo(() => _localizer["Exception"])
            .Returns(new LocalizedString("Exception", "An error occurred"));

        _sut = new TransactionService(_context, _allocationService, _logger, _localizer, _resultFactory);
    }

    [TearDown]
    public void TearDown()
    {
        if (_contextDisposed) return;

        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<(CashRegisterModel CashRegister, AllocationModel Allocation)> CreateTestDataAsync()
    {
        var cashRegister = new CashRegisterModel { Name = "Test Register" };
        var costCenter = new CostCenterModel { CostUnitName = "Test Cost Center" };
        var category = new CategoryModel { Name = "Test Category" };

        await _context.CashRegisters.AddAsync(cashRegister);
        await _context.CostCenters.AddAsync(costCenter);
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var allocation = new AllocationModel
        {
            CostCenterId = costCenter.Id,
            CategoryId = category.Id
        };
        await _context.Allocations.AddAsync(allocation);
        await _context.SaveChangesAsync();

        return (cashRegister, allocation);
    }

    #region GetTransactionByIdAsync Tests

    [Test]
    public async Task GetTransactionByIdAsync_WhenTransactionExists_ShouldReturnTransaction()
    {
        // Arrange
        var (cashRegister, allocation) = await CreateTestDataAsync();
        var transaction = new TransactionModel
        {
            Documentnumber = 100,
            Description = "Test Transaction",
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id,
            Date = DateOnly.FromDateTime(DateTime.Now)
        };
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetTransactionByIdAsync(transaction.Id);

        // Assert
        result.Should().NotBeNull();
        result.Documentnumber.Should().Be(100);
        result.Description.Should().Be("Test Transaction");
    }

    [Test]
    public async Task GetTransactionByIdAsync_WhenTransactionDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetTransactionByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllDocumentNumbersAsync Tests

    [Test]
    public async Task GetAllDocumentNumbersAsync_WhenTransactionsExist_ShouldReturnAllDocumentNumbers()
    {
        // Arrange
        var (cashRegister, allocation) = await CreateTestDataAsync();
        var transactions = new List<TransactionModel>
        {
            new() { Documentnumber = 100, CashRegisterId = cashRegister.Id, AllocationId = allocation.Id },
            new() { Documentnumber = 200, CashRegisterId = cashRegister.Id, AllocationId = allocation.Id },
            new() { Documentnumber = 300, CashRegisterId = cashRegister.Id, AllocationId = allocation.Id }
        };
        await _context.Transactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllDocumentNumbersAsync(1);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(new[] { 100, 200, 300 });
    }

    [Test]
    public async Task GetAllDocumentNumbersAsync_WhenNoTransactionsExist_ShouldReturnEmptySet()
    {
        // Act
        var result = await _sut.GetAllDocumentNumbersAsync(1);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetLatestDocumentNumberAsync Tests

    [Test]
    public async Task GetLatestDocumentNumberAsync_WhenTransactionsExist_ShouldReturnDocumentNumberOfLatestByDate()
    {
        // Arrange
        var (cashRegister, allocation) = await CreateTestDataAsync();
        var transactions = new List<TransactionModel>
        {
            new() { Documentnumber = 2385, Date = new DateOnly(2024, 3, 10), CashRegisterId = cashRegister.Id, AllocationId = allocation.Id },
            new() { Documentnumber = 2386, Date = new DateOnly(2024, 3, 11), CashRegisterId = cashRegister.Id, AllocationId = allocation.Id },
            new() { Documentnumber = 2387, Date = new DateOnly(2024, 3, 12), CashRegisterId = cashRegister.Id, AllocationId = allocation.Id },
            new() { Documentnumber = 9001, Date = new DateOnly(2024, 1, 5), CashRegisterId = cashRegister.Id, AllocationId = allocation.Id },
            new() { Documentnumber = 9002, Date = new DateOnly(2024, 1, 6), CashRegisterId = cashRegister.Id, AllocationId = allocation.Id }
        };
        await _context.Transactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetLatestDocumentNumberAsync(cashRegister.Id);

        // Assert
        result.Should().Be(2387);
    }

    [Test]
    public async Task GetLatestDocumentNumberAsync_WhenSameDateMultipleTransactions_ShouldReturnHighestDocumentNumber()
    {
        // Arrange
        var (cashRegister, allocation) = await CreateTestDataAsync();
        var transactions = new List<TransactionModel>
        {
            new() { Documentnumber = 100, Date = new DateOnly(2024, 3, 12), CashRegisterId = cashRegister.Id, AllocationId = allocation.Id },
            new() { Documentnumber = 200, Date = new DateOnly(2024, 3, 12), CashRegisterId = cashRegister.Id, AllocationId = allocation.Id },
            new() { Documentnumber = 150, Date = new DateOnly(2024, 3, 12), CashRegisterId = cashRegister.Id, AllocationId = allocation.Id }
        };
        await _context.Transactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetLatestDocumentNumberAsync(cashRegister.Id);

        // Assert
        result.Should().Be(200);
    }

    [Test]
    public async Task GetLatestDocumentNumberAsync_WhenNoTransactionsExist_ShouldReturnZero()
    {
        // Act
        var result = await _sut.GetLatestDocumentNumberAsync(1);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public async Task GetLatestDocumentNumberAsync_ShouldOnlyConsiderSpecifiedRegister()
    {
        // Arrange
        var (cashRegister1, allocation) = await CreateTestDataAsync();
        var cashRegister2 = new CashRegisterModel { Name = "Other Register" };
        await _context.CashRegisters.AddAsync(cashRegister2);
        await _context.SaveChangesAsync();

        var transactions = new List<TransactionModel>
        {
            new() { Documentnumber = 500, Date = new DateOnly(2024, 3, 12), CashRegisterId = cashRegister1.Id, AllocationId = allocation.Id },
            new() { Documentnumber = 9999, Date = new DateOnly(2024, 6, 1), CashRegisterId = cashRegister2.Id, AllocationId = allocation.Id }
        };
        await _context.Transactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetLatestDocumentNumberAsync(cashRegister1.Id);

        // Assert
        result.Should().Be(500);
    }

    #endregion

    #region AddTransactionAsync Tests

    [Test]
    public async Task AddTransactionAsync_WhenValidTransaction_ShouldAddAndReturnSuccess()
    {
        // Arrange
        var (cashRegister, allocation) = await CreateTestDataAsync();

        var transaction = new TransactionModel
        {
            Documentnumber = 100,
            Description = "New Transaction",
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id,
            Sum = 50.00m,
            AccountMovement = 50.00m,
            Date = DateOnly.FromDateTime(DateTime.Now)
        };

        var expectedResult = Result.Success("Successfully added");
        A.CallTo(() => _resultFactory.SuccessAdded(A<string>._, A<object?>._))
            .Returns(expectedResult);
        A.CallTo(() => _allocationService.GetRequiredAllocationAsync(allocation.Id, A<CancellationToken>._))
            .Returns(allocation);

        // Act
        var result = await _sut.AddTransactionAsync(transaction);

        // Assert
        result.Should().Be(expectedResult);
        var addedTransaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Documentnumber == 100);
        addedTransaction.Should().NotBeNull();
    }

    [Test]
    public async Task AddTransactionAsync_WhenCashRegisterDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var (_, allocation) = await CreateTestDataAsync();

        var transaction = new TransactionModel
        {
            Documentnumber = 100,
            CashRegisterId = 999, // Non-existent
            AllocationId = allocation.Id
        };

        var expectedResult = Result.Failure(new Error("Test.Error", "Not found"));
        A.CallTo(() => _resultFactory.NotFound(A<string>._, A<object>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddTransactionAsync(transaction);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task AddTransactionAsync_WhenDocumentNumberAlreadyExists_ShouldReturnAlreadyExists()
    {
        // Arrange
        var (cashRegister, allocation) = await CreateTestDataAsync();

        var existingTransaction = new TransactionModel
        {
            Documentnumber = 100,
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id
        };
        await _context.Transactions.AddAsync(existingTransaction);
        await _context.SaveChangesAsync();

        var newTransaction = new TransactionModel
        {
            Documentnumber = 100, // Same document number
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id
        };

        var expectedResult = Result.Failure(new Error("Test.Error", "Already exists"));
        A.CallTo(() => _resultFactory.AlreadyExists(A<string>._, A<string?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddTransactionAsync(newTransaction);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task AddTransactionAsync_WhenDocumentNumberExistsInDifferentCashRegister_ShouldAddSuccessfully()
    {
        // Arrange
        var (cashRegister1, allocation) = await CreateTestDataAsync();
        var cashRegister2 = new CashRegisterModel { Name = "Second Register" };
        await _context.CashRegisters.AddAsync(cashRegister2);
        await _context.SaveChangesAsync();

        var existingTransaction = new TransactionModel
        {
            Documentnumber = 2002,
            CashRegisterId = cashRegister1.Id,
            AllocationId = allocation.Id
        };
        await _context.Transactions.AddAsync(existingTransaction);
        await _context.SaveChangesAsync();

        var newTransaction = new TransactionModel
        {
            Documentnumber = 2002, // Same document number, different cash register
            CashRegisterId = cashRegister2.Id,
            AllocationId = allocation.Id,
            Sum = 10.00m,
            AccountMovement = 10.00m,
            Date = DateOnly.FromDateTime(DateTime.Now)
        };

        var expectedResult = Result.Success("Successfully added");
        A.CallTo(() => _resultFactory.SuccessAdded(A<string>._, A<object?>._))
            .Returns(expectedResult);
        A.CallTo(() => _allocationService.GetRequiredAllocationAsync(allocation.Id, A<CancellationToken>._))
            .Returns(allocation);

        // Act
        var result = await _sut.AddTransactionAsync(newTransaction);

        // Assert
        result.Should().Be(expectedResult);
        var transactions = await _context.Transactions.Where(t => t.Documentnumber == 2002).ToListAsync();
        transactions.Should().HaveCount(2);
    }

    [Test]
    public async Task AddTransactionAsync_WhenAllocationIdIsMissing_ShouldReturnFailure()
    {
        // Arrange
        var (cashRegister, _) = await CreateTestDataAsync();

        var transaction = new TransactionModel
        {
            Documentnumber = 100,
            CashRegisterId = cashRegister.Id,
            AllocationId = 0 // Missing
        };

        var expectedResult = Result.Failure(new Error("Test.Error", "Failed to add"));
        A.CallTo(() => _resultFactory.FailedToAdd(A<string>._, A<string?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddTransactionAsync(transaction);

        // Assert - Exception is caught inside try-catch and returns failure
        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task AddTransactionAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = Result.Failure(new Error("Test.Error", "Failed to add"));
        A.CallTo(() => _resultFactory.FailedToAdd(A<string>._, A<string?>._))
            .Returns(expectedResult);

        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new TransactionService(disposedContext, _allocationService, _logger, _localizer, _resultFactory);

        var transaction = new TransactionModel
        {
            Documentnumber = 100,
            CashRegisterId = 1,
            AllocationId = 1
        };

        // Act
        var result = await _sut.AddTransactionAsync(transaction);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region UpdateTransactionAsync Tests

    [Test]
    public async Task UpdateTransactionAsync_WhenValidTransaction_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var (cashRegister, allocation) = await CreateTestDataAsync();

        var transaction = new TransactionModel
        {
            Documentnumber = 100,
            Description = "Original",
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id
        };
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        var expectedResult = Result.Success("Successfully updated");
        A.CallTo(() => _resultFactory.SuccessUpdated(A<string>._, A<object?>._))
            .Returns(expectedResult);
        A.CallTo(() => _allocationService.GetRequiredAllocationAsync(allocation.Id, A<CancellationToken>._))
            .Returns(allocation);

        var updatedTransaction = new TransactionModel
        {
            Id = transaction.Id,
            Documentnumber = 100,
            Description = "Updated",
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id
        };

        // Act
        var result = await _sut.UpdateTransactionAsync(updatedTransaction);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task UpdateTransactionAsync_WhenTransactionDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var expectedResult = Result.Failure(new Error("Test.Error", "Not found"));
        A.CallTo(() => _resultFactory.NotFound(A<string>._, A<object>._))
            .Returns(expectedResult);

        var transaction = new TransactionModel
        {
            Id = 999,
            Documentnumber = 100
        };

        // Act
        var result = await _sut.UpdateTransactionAsync(transaction);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task UpdateTransactionAsync_WhenDocumentNumberAlreadyExistsOnOtherTransaction_ShouldReturnAlreadyExists()
    {
        // Arrange
        var (cashRegister, allocation) = await CreateTestDataAsync();

        var transaction1 = new TransactionModel
        {
            Documentnumber = 100,
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id
        };
        var transaction2 = new TransactionModel
        {
            Documentnumber = 200,
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id
        };
        await _context.Transactions.AddRangeAsync(transaction1, transaction2);
        await _context.SaveChangesAsync();

        var expectedResult = Result.Failure(new Error("Test.Error", "Already exists"));
        A.CallTo(() => _resultFactory.AlreadyExists(A<string>._, A<string?>._))
            .Returns(expectedResult);

        var updatedTransaction = new TransactionModel
        {
            Id = transaction2.Id,
            Documentnumber = 100, // Same as transaction1
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id
        };

        // Act
        var result = await _sut.UpdateTransactionAsync(updatedTransaction);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task UpdateTransactionAsync_WhenDocumentNumberExistsInDifferentCashRegister_ShouldUpdateSuccessfully()
    {
        // Arrange
        var (cashRegister1, allocation) = await CreateTestDataAsync();
        var cashRegister2 = new CashRegisterModel { Name = "Second Register" };
        await _context.CashRegisters.AddAsync(cashRegister2);
        await _context.SaveChangesAsync();

        var transaction1 = new TransactionModel
        {
            Documentnumber = 2002,
            CashRegisterId = cashRegister1.Id,
            AllocationId = allocation.Id
        };
        var transaction2 = new TransactionModel
        {
            Documentnumber = 200,
            CashRegisterId = cashRegister2.Id,
            AllocationId = allocation.Id
        };
        await _context.Transactions.AddRangeAsync(transaction1, transaction2);
        await _context.SaveChangesAsync();

        var expectedResult = Result.Success("Successfully updated");
        A.CallTo(() => _resultFactory.SuccessUpdated(A<string>._, A<object?>._))
            .Returns(expectedResult);
        A.CallTo(() => _allocationService.GetRequiredAllocationAsync(allocation.Id, A<CancellationToken>._))
            .Returns(allocation);

        var updatedTransaction = new TransactionModel
        {
            Id = transaction2.Id,
            Documentnumber = 2002, // Same as transaction1, but different cash register
            CashRegisterId = cashRegister2.Id,
            AllocationId = allocation.Id
        };

        // Act
        var result = await _sut.UpdateTransactionAsync(updatedTransaction);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task UpdateTransactionAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = Result.Failure(new Error("Test.Error", "Failed to update"));
        A.CallTo(() => _resultFactory.FailedToUpdate(A<string>._, A<string?>._))
            .Returns(expectedResult);

        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new TransactionService(disposedContext, _allocationService, _logger, _localizer, _resultFactory);

        var transaction = new TransactionModel { Id = 1, Documentnumber = 100 };

        // Act
        var result = await _sut.UpdateTransactionAsync(transaction);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task UpdateTransactionAsync_WhenChangingAllocation_ShouldUpdateSuccessfully()
    {
        // Arrange
        var (cashRegister, allocation1) = await CreateTestDataAsync();

        // Create a second allocation
        var allocation2 = new AllocationModel
        {
            CostCenterId = allocation1.CostCenterId,
            CategoryId = allocation1.CategoryId
        };
        await _context.Allocations.AddAsync(allocation2);
        await _context.SaveChangesAsync();

        var transaction = new TransactionModel
        {
            Documentnumber = 100,
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation1.Id
        };
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear(); // Simulate fresh context like real app

        // Load with Include (simulating GetTransactionByIdAsync)
        var loaded = await _context.Transactions
            .Include(t => t.Allocation)
            .FirstAsync(t => t.Id == transaction.Id);

        // Change allocation
        loaded.AllocationId = allocation2.Id;

        // Re-load allocation2 so it's tracked by the context
        var trackedAllocation2 = await _context.Allocations.FindAsync(allocation2.Id);

        var expectedResult = Result.Success("Successfully updated");
        A.CallTo(() => _allocationService.GetRequiredAllocationAsync(allocation2.Id, A<CancellationToken>._))
            .Returns(trackedAllocation2!);
        A.CallTo(() => _resultFactory.SuccessUpdated(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.UpdateTransactionAsync(loaded);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region DeleteTransactionAsync Tests

    [Test]
    public async Task DeleteTransactionAsync_WhenTransactionExists_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var (cashRegister, allocation) = await CreateTestDataAsync();
        var transaction = new TransactionModel
        {
            Documentnumber = 100,
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id
        };
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
        var id = transaction.Id;

        var expectedResult = Result.Success("Successfully deleted");
        A.CallTo(() => _resultFactory.SuccessDeleted(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteTransactionAsync(id);

        // Assert
        result.Should().Be(expectedResult);
        var deletedTransaction = await _context.Transactions.FindAsync(id);
        deletedTransaction.Should().BeNull();
    }

    [Test]
    public async Task DeleteTransactionAsync_WhenTransactionDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var expectedResult = Result.Failure(new Error("Test.Error", "Not found"));
        A.CallTo(() => _resultFactory.NotFound(A<string>._, A<object>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteTransactionAsync(999);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task DeleteTransactionAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = Result.Failure(new Error("Test.Error", "Failed to delete"));
        A.CallTo(() => _resultFactory.FailedToDelete(A<string>._, A<string?>._))
            .Returns(expectedResult);

        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new TransactionService(disposedContext, _allocationService, _logger, _localizer, _resultFactory);

        // Act
        var result = await _sut.DeleteTransactionAsync(1);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region GetTransactionsForExport Tests

    [Test]
    public async Task GetTransactionsForExport_WhenTransactionsInDateRange_ShouldReturnTransactions()
    {
        // Arrange
        var (cashRegister, allocation) = await CreateTestDataAsync();
        var transactions = new List<TransactionModel>
        {
            new() { Documentnumber = 1, Date = new DateOnly(2024, 1, 15), CashRegisterId = cashRegister.Id, AllocationId = allocation.Id },
            new() { Documentnumber = 2, Date = new DateOnly(2024, 1, 20), CashRegisterId = cashRegister.Id, AllocationId = allocation.Id },
            new() { Documentnumber = 3, Date = new DateOnly(2024, 2, 15), CashRegisterId = cashRegister.Id, AllocationId = allocation.Id }
        };
        await _context.Transactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetTransactionsForExportAsync(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 31),
            cashRegister.Id);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList.Select(t => t.Documentnumber).Should().BeEquivalentTo([1, 2]);
    }

    [Test]
    public async Task GetTransactionsForExport_WhenNoTransactionsInDateRange_ShouldReturnEmpty()
    {
        // Arrange
        var (cashRegister, allocation) = await CreateTestDataAsync();
        var transaction = new TransactionModel
        {
            Documentnumber = 1,
            Date = new DateOnly(2024, 6, 15),
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id
        };
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetTransactionsForExportAsync(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 31),
            cashRegister.Id);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetTransactionsForBudgetExport Tests

    [Test]
    public async Task GetTransactionsForBudgetExport_WhenTransactionsInDateRange_ShouldReturnTransactionsWithIncludes()
    {
        // Arrange
        var (cashRegister, allocation) = await CreateTestDataAsync();
        var transactions = new List<TransactionModel>
        {
            new() { Documentnumber = 1, Date = new DateOnly(2024, 1, 15), CashRegisterId = cashRegister.Id, AllocationId = allocation.Id },
            new() { Documentnumber = 2, Date = new DateOnly(2024, 1, 20), CashRegisterId = cashRegister.Id, AllocationId = allocation.Id }
        };
        await _context.Transactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetTransactionsForBudgetExportAsync(
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 31),
            cashRegister.Id);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(2);
        resultList.All(t => t.Allocation != null).Should().BeTrue();
    }

    #endregion
}