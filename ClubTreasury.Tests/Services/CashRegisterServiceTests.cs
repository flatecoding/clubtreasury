using FakeItEasy;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ClubTreasury.Data;
using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.OperationResult;
using ClubTreasury.Data.Person;

namespace ClubTreasury.Tests.Services;

[TestFixture]
public class CashRegisterServiceTests
{
    private CashDataContext _context = null!;
    private ILogger<CashRegisterService> _logger = null!;
    private IResultFactory _resultFactory = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private CashRegisterService _sut = null!;
    private bool _contextDisposed;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CashDataContext(options);
        _contextDisposed = false;
        _logger = A.Fake<ILogger<CashRegisterService>>();
        _resultFactory = A.Fake<IResultFactory>();
        _localizer = A.Fake<IStringLocalizer<Translation>>();

        A.CallTo(() => _localizer["CashRegister"])
            .Returns(new LocalizedString("CashRegister", "Cash Register"));
        A.CallTo(() => _localizer["Exception"])
            .Returns(new LocalizedString("Exception", "An error occurred"));

        _sut = new CashRegisterService(_context, _logger, _resultFactory, _localizer);
    }

    [TearDown]
    public void TearDown()
    {
        if (_contextDisposed) return;

        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetAllCashRegisters Tests

    [Test]
    public async Task GetAllCashRegisters_WhenNoCashRegistersExist_ShouldReturnEmptyList()
    {
        // Act
        var result = await _sut.GetAllCashRegistersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllCashRegisters_WhenCashRegistersExist_ShouldReturnAllCashRegisters()
    {
        // Arrange
        var cashRegisters = new List<CashRegisterModel>
        {
            new() { Name = "Register 1" },
            new() { Name = "Register 2" },
            new() { Name = "Register 3" }
        };
        await _context.CashRegisters.AddRangeAsync(cashRegisters);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllCashRegistersAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Select(r => r.Name).Should().BeEquivalentTo(["Register 1", "Register 2", "Register 3"]);
    }

    #endregion

    #region GetCashRegisterBalancesAsync Tests

    [Test]
    public async Task GetCashRegisterBalancesAsync_WhenNoTransactions_ShouldReturnEmptyDictionary()
    {
        // Arrange
        await _context.CashRegisters.AddAsync(new CashRegisterModel { Name = "Empty Register" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetCashRegisterBalancesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetCashRegisterBalancesAsync_ShouldReturnCorrectSumPerRegister()
    {
        // Arrange
        var register1 = new CashRegisterModel { Name = "Register 1" };
        var register2 = new CashRegisterModel { Name = "Register 2" };
        await _context.CashRegisters.AddRangeAsync(register1, register2);

        var allocation = new Data.Allocation.AllocationModel { CostCenterId = 1, CategoryId = 1 };
        await _context.Allocations.AddAsync(allocation);
        await _context.SaveChangesAsync();

        await _context.Transactions.AddRangeAsync(
            new Data.Transaction.TransactionModel
            {
                CashRegisterId = register1.Id, AllocationId = allocation.Id,
                Documentnumber = 1, Sum = 100m, AccountMovement = 100m
            },
            new Data.Transaction.TransactionModel
            {
                CashRegisterId = register1.Id, AllocationId = allocation.Id,
                Documentnumber = 2, Sum = 50m, AccountMovement = -50m
            },
            new Data.Transaction.TransactionModel
            {
                CashRegisterId = register2.Id, AllocationId = allocation.Id,
                Documentnumber = 1, Sum = 200m, AccountMovement = 200m
            });
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetCashRegisterBalancesAsync();

        // Assert
        result.Should().HaveCount(2);
        result[register1.Id].Should().Be(50m);
        result[register2.Id].Should().Be(200m);
    }

    [Test]
    public async Task GetCashRegisterBalancesAsync_ShouldNotIncludeRegistersWithoutTransactions()
    {
        // Arrange
        var registerWithTx = new CashRegisterModel { Name = "With Transactions" };
        var registerEmpty = new CashRegisterModel { Name = "Empty" };
        await _context.CashRegisters.AddRangeAsync(registerWithTx, registerEmpty);

        var allocation = new Data.Allocation.AllocationModel { CostCenterId = 1, CategoryId = 1 };
        await _context.Allocations.AddAsync(allocation);
        await _context.SaveChangesAsync();

        await _context.Transactions.AddAsync(new Data.Transaction.TransactionModel
        {
            CashRegisterId = registerWithTx.Id, AllocationId = allocation.Id,
            Documentnumber = 1, Sum = 100m, AccountMovement = 100m
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetCashRegisterBalancesAsync();

        // Assert
        result.Should().ContainKey(registerWithTx.Id);
        result.Should().NotContainKey(registerEmpty.Id);
    }

    #endregion

    #region GetCashRegisterWithTreasurerAsync Tests

    [Test]
    public async Task GetCashRegisterWithTreasurer_WhenExists_ShouldReturnWithTreasurer()
    {
        // Arrange
        var person = new PersonModel { Name = "John Doe" };
        await _context.Persons.AddAsync(person);
        await _context.SaveChangesAsync();

        var cashRegister = new CashRegisterModel { Name = "Test Register", TreasurerId = person.Id };
        await _context.CashRegisters.AddAsync(cashRegister);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetCashRegisterWithTreasurerAsync(cashRegister.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Register");
        result.Treasurer.Should().NotBeNull();
        result.Treasurer!.Name.Should().Be("John Doe");
    }

    [Test]
    public async Task GetCashRegisterWithTreasurer_WhenNoTreasurer_ShouldReturnWithNullTreasurer()
    {
        // Arrange
        var cashRegister = new CashRegisterModel { Name = "No Treasurer Register" };
        await _context.CashRegisters.AddAsync(cashRegister);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetCashRegisterWithTreasurerAsync(cashRegister.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Treasurer.Should().BeNull();
    }

    [Test]
    public async Task GetCashRegisterWithTreasurer_WhenNotFound_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetCashRegisterWithTreasurerAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetCashRegisterById Tests

    [Test]
    public async Task GetCashRegisterById_WhenCashRegisterExists_ShouldReturnCashRegister()
    {
        // Arrange
        var cashRegister = new CashRegisterModel { Name = "Test Register" };
        await _context.CashRegisters.AddAsync(cashRegister);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetCashRegisterByIdAsync(cashRegister.Id);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Register");
    }

    [Test]
    public async Task GetCashRegisterById_WhenCashRegisterDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetCashRegisterByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetFirstCashRegisterAsync Tests

    [Test]
    public async Task GetFirstCashRegisterAsync_WhenCashRegistersExist_ShouldReturnFirstCashRegister()
    {
        // Arrange
        var cashRegisters = new List<CashRegisterModel>
        {
            new() { Name = "First Register" },
            new() { Name = "Second Register" }
        };
        await _context.CashRegisters.AddRangeAsync(cashRegisters);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetFirstCashRegisterAsync();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("First Register");
    }

    [Test]
    public async Task GetFirstCashRegisterAsync_WhenNoCashRegistersExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetFirstCashRegisterAsync();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region AddCashRegister Tests

    [Test]
    public async Task AddCashRegister_WhenValidCashRegister_ShouldAddAndReturnSuccess()
    {
        // Arrange
        var cashRegister = new CashRegisterModel { Name = "New Register" };
        var expectedResult = Result.Success("Successfully added");
        A.CallTo(() => _resultFactory.SuccessAdded(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddCashRegisterAsync(cashRegister);

        // Assert
        result.Should().Be(expectedResult);
        var addedRegister = await _context.CashRegisters.FirstOrDefaultAsync(r => r.Name == "New Register");
        addedRegister.Should().NotBeNull();
        A.CallTo(() => _resultFactory.SuccessAdded(
            A<string>.That.Contains("New Register"),
            A<object?>._)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task AddCashRegister_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = Result.Failure(new Error("Test.Error", "Failed to add"));
        A.CallTo(() => _resultFactory.FailedToAdd(A<string>._, A<string?>._))
            .Returns(expectedResult);

        // Dispose context to simulate an error
        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new CashRegisterService(disposedContext, _logger, _resultFactory, _localizer);

        var cashRegister = new CashRegisterModel { Name = "New Register" };

        // Act
        var result = await _sut.AddCashRegisterAsync(cashRegister);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region UpdateCashRegister Tests

    [Test]
    public async Task UpdateCashRegister_WhenValidCashRegister_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var cashRegister = new CashRegisterModel { Name = "Original Name" };
        await _context.CashRegisters.AddAsync(cashRegister);
        await _context.SaveChangesAsync();

        var expectedResult = Result.Success("Successfully updated");
        A.CallTo(() => _resultFactory.SuccessUpdated(A<string>._, A<object?>._))
            .Returns(expectedResult);

        cashRegister.Name = "Updated Name";

        // Act
        var result = await _sut.UpdateCashRegisterAsync(cashRegister);

        // Assert
        result.Should().Be(expectedResult);
        var updatedRegister = await _context.CashRegisters.FindAsync(cashRegister.Id);
        updatedRegister!.Name.Should().Be("Updated Name");
    }

    [Test]
    public async Task UpdateCashRegister_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = Result.Failure(new Error("Test.Error", "Failed to update"));
        A.CallTo(() => _resultFactory.FailedToUpdate(A<string>._, A<string?>._))
            .Returns(expectedResult);

        // Dispose context to simulate error
        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new CashRegisterService(disposedContext, _logger, _resultFactory, _localizer);

        var cashRegister = new CashRegisterModel { Name = "Test" };

        // Act
        var result = await _sut.UpdateCashRegisterAsync(cashRegister);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region DeleteCashRegisterAsync Tests

    [Test]
    public async Task DeleteCashRegisterAsync_WhenCashRegisterExists_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var cashRegister = new CashRegisterModel { Name = "To Be Deleted" };
        await _context.CashRegisters.AddAsync(cashRegister);
        await _context.SaveChangesAsync();
        var id = cashRegister.Id;

        var expectedResult = Result.Success("Successfully deleted");
        A.CallTo(() => _resultFactory.SuccessDeleted(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteCashRegisterAsync(id);

        // Assert
        result.Should().Be(expectedResult);
        var deletedRegister = await _context.CashRegisters.FindAsync(id);
        deletedRegister.Should().BeNull();
    }

    [Test]
    public async Task DeleteCashRegisterAsync_WhenCashRegisterDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var expectedResult = Result.Failure(new Error("Test.Error", "Not found"));
        A.CallTo(() => _resultFactory.NotFound(A<string>._, A<object>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteCashRegisterAsync(999);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _resultFactory.NotFound(A<string>._, 999))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task DeleteCashRegisterAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = Result.Failure(new Error("Test.Error", "Failed to delete"));
        A.CallTo(() => _resultFactory.FailedToDelete(A<string>._, A<string?>._))
            .Returns(expectedResult);

        // Dispose context to simulate error during delete
        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new CashRegisterService(disposedContext, _logger, _resultFactory, _localizer);

        // Act
        var result = await _sut.DeleteCashRegisterAsync(1);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion
}