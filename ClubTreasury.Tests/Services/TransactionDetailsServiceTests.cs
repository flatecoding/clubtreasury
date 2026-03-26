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
using ClubTreasury.Data.Person;
using ClubTreasury.Data.Transaction;
using ClubTreasury.Data.TransactionDetails;

namespace ClubTreasury.Tests.Services;

[TestFixture]
public class TransactionDetailsServiceTests
{
    private CashDataContext _context = null!;
    private ILogger<TransactionDetailsService> _logger = null!;
    private IResultFactory _resultFactory = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private TransactionDetailsService _sut = null!;
    private bool _contextDisposed;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CashDataContext(options);
        _contextDisposed = false;
        _logger = A.Fake<ILogger<TransactionDetailsService>>();
        _resultFactory = A.Fake<IResultFactory>();
        _localizer = A.Fake<IStringLocalizer<Translation>>();

        A.CallTo(() => _localizer["TransactionDetails"])
            .Returns(new LocalizedString("TransactionDetails", "Transaction Details"));
        A.CallTo(() => _localizer["Exception"])
            .Returns(new LocalizedString("Exception", "An error occurred"));

        _sut = new TransactionDetailsService(_context, _logger, _localizer, _resultFactory);
    }

    [TearDown]
    public void TearDown()
    {
        if (_contextDisposed) return;

        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<TransactionModel> CreateTestTransactionAsync()
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

        var transaction = new TransactionModel
        {
            Documentnumber = 100,
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id,
            Date = DateOnly.FromDateTime(DateTime.Now)
        };
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        return transaction;
    }

    #region GetAllTransactionDetailsAsync Tests

    [Test]
    public async Task GetAllTransactionDetailsAsync_WhenNoDetailsExist_ShouldReturnEmptyList()
    {
        // Act
        var result = await _sut.GetAllTransactionDetailsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllTransactionDetailsAsync_WhenDetailsExist_ShouldReturnAllDetails()
    {
        // Arrange
        var transaction = await CreateTestTransactionAsync();

        var details = new List<TransactionDetailsModel>
        {
            new() { TransactionId = transaction.Id, Description = "Detail 1", Sum = 10.00m },
            new() { TransactionId = transaction.Id, Description = "Detail 2", Sum = 20.00m },
            new() { TransactionId = transaction.Id, Description = "Detail 3", Sum = 30.00m }
        };
        await _context.TransactionDetails.AddRangeAsync(details);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllTransactionDetailsAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Select(d => d.Description).Should().BeEquivalentTo(["Detail 1", "Detail 2", "Detail 3"]);
    }

    #endregion

    #region GetTransactionDetailsByIdAsync Tests

    [Test]
    public async Task GetTransactionDetailsByIdAsync_WhenDetailExists_ShouldReturnDetail()
    {
        // Arrange
        var transaction = await CreateTestTransactionAsync();
        var detail = new TransactionDetailsModel
        {
            TransactionId = transaction.Id,
            Description = "Test Detail",
            Sum = 50.00m
        };
        await _context.TransactionDetails.AddAsync(detail);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetTransactionDetailsByIdAsync(detail.Id);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be("Test Detail");
        result.Sum.Should().Be(50.00m);
    }

    [Test]
    public async Task GetTransactionDetailsByIdAsync_WhenDetailDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetTransactionDetailsByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetTransactionDetailsByTransactionIdAsync Tests

    [Test]
    public async Task GetTransactionDetailsByTransactionIdAsync_WhenDetailsExist_ShouldReturnDetailsForTransaction()
    {
        // Arrange
        var transaction1 = await CreateTestTransactionAsync();

        var cashRegister = await _context.CashRegisters.FirstAsync();
        var allocation = await _context.Allocations.FirstAsync();
        var transaction2 = new TransactionModel
        {
            Documentnumber = 200,
            CashRegisterId = cashRegister.Id,
            AllocationId = allocation.Id
        };
        await _context.Transactions.AddAsync(transaction2);
        await _context.SaveChangesAsync();

        var details = new List<TransactionDetailsModel>
        {
            new() { TransactionId = transaction1.Id, Description = "Detail for T1", Sum = 10.00m },
            new() { TransactionId = transaction1.Id, Description = "Another Detail for T1", Sum = 20.00m },
            new() { TransactionId = transaction2.Id, Description = "Detail for T2", Sum = 30.00m }
        };
        await _context.TransactionDetails.AddRangeAsync(details);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetTransactionDetailsByTransactionIdAsync(transaction1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.All(d => d.TransactionId == transaction1.Id).Should().BeTrue();
    }

    [Test]
    public async Task GetTransactionDetailsByTransactionIdAsync_WhenNoDetailsForTransaction_ShouldReturnEmptyList()
    {
        // Arrange
        var transaction = await CreateTestTransactionAsync();

        // Act
        var result = await _sut.GetTransactionDetailsByTransactionIdAsync(transaction.Id);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddTransactionDetailsAsync Tests

    [Test]
    public async Task AddTransactionDetailsAsync_WhenValidDetail_ShouldAddAndReturnSuccess()
    {
        // Arrange
        var transaction = await CreateTestTransactionAsync();
        var detail = new TransactionDetailsModel
        {
            TransactionId = transaction.Id,
            Description = "New Detail",
            Sum = 100.00m
        };

        var expectedResult = Result.Success("Successfully added");
        A.CallTo(() => _resultFactory.SuccessAdded(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddTransactionDetailsAsync(detail);

        // Assert
        result.Should().Be(expectedResult);
        var addedDetail = await _context.TransactionDetails.FirstOrDefaultAsync(d => d.Description == "New Detail");
        addedDetail.Should().NotBeNull();
    }

    [Test]
    public async Task AddTransactionDetailsAsync_WithPerson_ShouldAddAndReturnSuccess()
    {
        // Arrange
        var transaction = await CreateTestTransactionAsync();
        var person = new PersonModel { Name = "Test Person" };
        await _context.Persons.AddAsync(person);
        await _context.SaveChangesAsync();

        var detail = new TransactionDetailsModel
        {
            TransactionId = transaction.Id,
            Description = "Detail with Person",
            Sum = 75.00m,
            PersonId = person.Id
        };

        var expectedResult = Result.Success("Successfully added");
        A.CallTo(() => _resultFactory.SuccessAdded(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddTransactionDetailsAsync(detail);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task AddTransactionDetailsAsync_WhenExceptionOccurs_ShouldReturnFailure()
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

        _sut = new TransactionDetailsService(disposedContext, _logger, _localizer, _resultFactory);

        var detail = new TransactionDetailsModel
        {
            TransactionId = 1,
            Description = "Test",
            Sum = 10.00m
        };

        // Act
        var result = await _sut.AddTransactionDetailsAsync(detail);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region UpdateTransactionDetailsAsync Tests

    [Test]
    public async Task UpdateTransactionDetailsAsync_WhenValidDetail_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var transaction = await CreateTestTransactionAsync();
        var detail = new TransactionDetailsModel
        {
            TransactionId = transaction.Id,
            Description = "Original",
            Sum = 50.00m
        };
        await _context.TransactionDetails.AddAsync(detail);
        await _context.SaveChangesAsync();

        var expectedResult = Result.Success("Successfully updated");
        A.CallTo(() => _resultFactory.SuccessUpdated(A<string>._, A<object?>._))
            .Returns(expectedResult);

        detail.Description = "Updated";
        detail.Sum = 75.00m;

        // Act
        var result = await _sut.UpdateTransactionDetailsAsync(detail);

        // Assert
        result.Should().Be(expectedResult);
        var updatedDetail = await _context.TransactionDetails.FindAsync(detail.Id);
        updatedDetail!.Description.Should().Be("Updated");
        updatedDetail.Sum.Should().Be(75.00m);
    }

    [Test]
    public async Task UpdateTransactionDetailsAsync_WhenDetailDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var expectedResult = Result.Failure(new Error("Entity.NotFound", "Not found"));
        A.CallTo(() => _resultFactory.NotFound(A<string>._, A<object>._))
            .Returns(expectedResult);

        var detail = new TransactionDetailsModel
        {
            TransactionId = 1,
            Description = "NonExistent",
            Sum = 10.00m
        };

        // Act
        var result = await _sut.UpdateTransactionDetailsAsync(detail);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _resultFactory.NotFound(A<string>._, 0))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task UpdateTransactionDetailsAsync_WhenExceptionOccurs_ShouldReturnFailure()
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

        _sut = new TransactionDetailsService(disposedContext, _logger, _localizer, _resultFactory);

        var detail = new TransactionDetailsModel
        {
            TransactionId = 1,
            Description = "Test",
            Sum = 10.00m
        };

        // Act
        var result = await _sut.UpdateTransactionDetailsAsync(detail);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region DeleteTransactionDetailsAsync Tests

    [Test]
    public async Task DeleteTransactionDetailsAsync_WhenDetailExists_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var transaction = await CreateTestTransactionAsync();
        var detail = new TransactionDetailsModel
        {
            TransactionId = transaction.Id,
            Description = "To Be Deleted",
            Sum = 25.00m
        };
        await _context.TransactionDetails.AddAsync(detail);
        await _context.SaveChangesAsync();
        var id = detail.Id;

        var expectedResult = Result.Success("Successfully deleted");
        A.CallTo(() => _resultFactory.SuccessDeleted(A<string>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteTransactionDetailsAsync(id);

        // Assert
        result.Should().Be(expectedResult);
        var deletedDetail = await _context.TransactionDetails.FindAsync(id);
        deletedDetail.Should().BeNull();
    }

    [Test]
    public async Task DeleteTransactionDetailsAsync_WhenDetailDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var expectedResult = Result.Failure(new Error("Test.Error", "Not found"));
        A.CallTo(() => _resultFactory.NotFound(A<string>._, A<object>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteTransactionDetailsAsync(999);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _resultFactory.NotFound(A<string>._, 999))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task DeleteTransactionDetailsAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var transaction = await CreateTestTransactionAsync();
        var detail = new TransactionDetailsModel
        {
            TransactionId = transaction.Id,
            Description = "Test",
            Sum = 10.00m
        };
        await _context.TransactionDetails.AddAsync(detail);
        await _context.SaveChangesAsync();

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

        _sut = new TransactionDetailsService(disposedContext, _logger, _localizer, _resultFactory);

        // Act
        var result = await _sut.DeleteTransactionDetailsAsync(1);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion
}