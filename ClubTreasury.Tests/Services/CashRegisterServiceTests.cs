using FakeItEasy;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using TTCCashRegister.Data;
using TTCCashRegister.Data.CashRegister;
using TTCCashRegister.Data.OperationResult;

namespace TTCCashRegister.Tests.Services;

[TestFixture]
public class CashRegisterServiceTests
{
    private CashDataContext _context = null!;
    private ILogger<CashRegisterService> _logger = null!;
    private IOperationResultFactory _operationResultFactory = null!;
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
        _operationResultFactory = A.Fake<IOperationResultFactory>();
        _localizer = A.Fake<IStringLocalizer<Translation>>();

        A.CallTo(() => _localizer["CashRegister"])
            .Returns(new LocalizedString("CashRegister", "Cash Register"));
        A.CallTo(() => _localizer["Exception"])
            .Returns(new LocalizedString("Exception", "An error occurred"));

        _sut = new CashRegisterService(_context, _logger, _operationResultFactory, _localizer);
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
        var result = await _sut.GetAllCashRegisters();

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
        var result = await _sut.GetAllCashRegisters();

        // Assert
        result.Should().HaveCount(3);
        result.Select(r => r.Name).Should().BeEquivalentTo(["Register 1", "Register 2", "Register 3"]);
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
        var result = await _sut.GetCashRegisterById(cashRegister.Id);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Register");
    }

    [Test]
    public async Task GetCashRegisterById_WhenCashRegisterDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetCashRegisterById(999);

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
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully added"
        };
        A.CallTo(() => _operationResultFactory.SuccessAdded(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddCashRegister(cashRegister);

        // Assert
        result.Should().Be(expectedResult);
        var addedRegister = await _context.CashRegisters.FirstOrDefaultAsync(r => r.Name == "New Register");
        addedRegister.Should().NotBeNull();
        A.CallTo(() => _operationResultFactory.SuccessAdded(
            A<string>.That.Contains("New Register"),
            A<object?>._)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task AddCashRegister_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Failed to add"
        };
        A.CallTo(() => _operationResultFactory.FailedToAdd(A<string>._, A<string?>._))
            .Returns(expectedResult);

        // Dispose context to simulate an error
        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new CashRegisterService(disposedContext, _logger, _operationResultFactory, _localizer);

        var cashRegister = new CashRegisterModel { Name = "New Register" };

        // Act
        var result = await _sut.AddCashRegister(cashRegister);

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

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully updated"
        };
        A.CallTo(() => _operationResultFactory.SuccessUpdated(A<string>._, A<object?>._))
            .Returns(expectedResult);

        cashRegister.Name = "Updated Name";

        // Act
        var result = await _sut.UpdateCashRegister(cashRegister);

        // Assert
        result.Should().Be(expectedResult);
        var updatedRegister = await _context.CashRegisters.FindAsync(cashRegister.Id);
        updatedRegister!.Name.Should().Be("Updated Name");
    }

    [Test]
    public async Task UpdateCashRegister_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Failed to update"
        };
        A.CallTo(() => _operationResultFactory.FailedToUpdate(A<string>._, A<string?>._))
            .Returns(expectedResult);

        // Dispose context to simulate error
        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new CashRegisterService(disposedContext, _logger, _operationResultFactory, _localizer);

        var cashRegister = new CashRegisterModel { Name = "Test" };

        // Act
        var result = await _sut.UpdateCashRegister(cashRegister);

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

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully deleted"
        };
        A.CallTo(() => _operationResultFactory.SuccessDeleted(A<string>._, A<object?>._))
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
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Not found"
        };
        A.CallTo(() => _operationResultFactory.NotFound(A<string>._, A<object>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteCashRegisterAsync(999);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _operationResultFactory.NotFound(A<string>._, 999))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task DeleteCashRegisterAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Failed to delete"
        };
        A.CallTo(() => _operationResultFactory.FailedToDelete(A<string>._, A<string?>._))
            .Returns(expectedResult);

        // Dispose context to simulate error during delete
        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new CashRegisterService(disposedContext, _logger, _operationResultFactory, _localizer);

        // Act
        var result = await _sut.DeleteCashRegisterAsync(1);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region GetLogoAsync Tests

    [Test]
    public async Task GetLogoAsync_WhenLogoExists_ShouldReturnDataAndContentType()
    {
        // Arrange
        var cashRegister = new CashRegisterModel { Name = "Test Register" };
        await _context.CashRegisters.AddAsync(cashRegister);
        await _context.SaveChangesAsync();

        var logoData = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        _context.CashRegisterLogos.Add(new CashRegisterLogoModel
        {
            CashRegisterId = cashRegister.Id,
            Data = logoData,
            ContentType = "image/png"
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetLogoAsync(cashRegister.Id);

        // Assert
        result.Should().NotBeNull();
        result.Value.Data.Should().BeEquivalentTo(logoData);
        result.Value.ContentType.Should().Be("image/png");
    }

    [Test]
    public async Task GetLogoAsync_WhenNoLogoExists_ShouldReturnNull()
    {
        // Arrange
        var cashRegister = new CashRegisterModel { Name = "Test Register" };
        await _context.CashRegisters.AddAsync(cashRegister);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetLogoAsync(cashRegister.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region UploadLogoAsync Tests

    [Test]
    public async Task UploadLogoAsync_WhenNoLogoExists_ShouldCreateNewLogo()
    {
        // Arrange
        var cashRegister = new CashRegisterModel { Name = "Test Register" };
        await _context.CashRegisters.AddAsync(cashRegister);
        await _context.SaveChangesAsync();

        var logoData = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully updated"
        };
        A.CallTo(() => _operationResultFactory.SuccessUpdated(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.UploadLogoAsync(cashRegister.Id, logoData, "image/png");

        // Assert
        result.Should().Be(expectedResult);
        var logo = await _context.CashRegisterLogos.FirstOrDefaultAsync(l => l.CashRegisterId == cashRegister.Id);
        logo.Should().NotBeNull();
        logo.Data.Should().BeEquivalentTo(logoData);
        logo.ContentType.Should().Be("image/png");
    }

    [Test]
    public async Task UploadLogoAsync_WhenLogoAlreadyExists_ShouldUpdateExistingLogo()
    {
        // Arrange
        var cashRegister = new CashRegisterModel { Name = "Test Register" };
        await _context.CashRegisters.AddAsync(cashRegister);
        await _context.SaveChangesAsync();

        _context.CashRegisterLogos.Add(new CashRegisterLogoModel
        {
            CashRegisterId = cashRegister.Id,
            Data = [0x01, 0x02],
            ContentType = "image/png"
        });
        await _context.SaveChangesAsync();

        var newLogoData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully updated"
        };
        A.CallTo(() => _operationResultFactory.SuccessUpdated(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.UploadLogoAsync(cashRegister.Id, newLogoData, "image/jpeg");

        // Assert
        result.Should().Be(expectedResult);
        var logos = await _context.CashRegisterLogos.Where(l => l.CashRegisterId == cashRegister.Id).ToListAsync();
        logos.Should().HaveCount(1);
        logos[0].Data.Should().BeEquivalentTo(newLogoData);
        logos[0].ContentType.Should().Be("image/jpeg");
    }

    [Test]
    public async Task UploadLogoAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Failed to update"
        };
        A.CallTo(() => _operationResultFactory.FailedToUpdate(A<string>._, A<string?>._))
            .Returns(expectedResult);

        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new CashRegisterService(disposedContext, _logger, _operationResultFactory, _localizer);

        // Act
        var result = await _sut.UploadLogoAsync(1, [0x01], "image/png");

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region DeleteLogoAsync Tests

    [Test]
    public async Task DeleteLogoAsync_WhenLogoExists_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var cashRegister = new CashRegisterModel { Name = "Test Register" };
        await _context.CashRegisters.AddAsync(cashRegister);
        await _context.SaveChangesAsync();

        _context.CashRegisterLogos.Add(new CashRegisterLogoModel
        {
            CashRegisterId = cashRegister.Id,
            Data = [0x01, 0x02],
            ContentType = "image/png"
        });
        await _context.SaveChangesAsync();

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully deleted"
        };
        A.CallTo(() => _operationResultFactory.SuccessDeleted(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteLogoAsync(cashRegister.Id);

        // Assert
        result.Should().Be(expectedResult);
        var logo = await _context.CashRegisterLogos.FirstOrDefaultAsync(l => l.CashRegisterId == cashRegister.Id);
        logo.Should().BeNull();
    }

    [Test]
    public async Task DeleteLogoAsync_WhenNoLogoExists_ShouldReturnSuccessWithoutError()
    {
        // Arrange
        var cashRegister = new CashRegisterModel { Name = "Test Register" };
        await _context.CashRegisters.AddAsync(cashRegister);
        await _context.SaveChangesAsync();

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully deleted"
        };
        A.CallTo(() => _operationResultFactory.SuccessDeleted(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeleteLogoAsync(cashRegister.Id);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Test]
    public async Task DeleteLogoAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Failed,
            Message = "Failed to delete"
        };
        A.CallTo(() => _operationResultFactory.FailedToDelete(A<string>._, A<string?>._))
            .Returns(expectedResult);

        await _context.DisposeAsync();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        await disposedContext.DisposeAsync();

        _sut = new CashRegisterService(disposedContext, _logger, _operationResultFactory, _localizer);

        // Act
        var result = await _sut.DeleteLogoAsync(1);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion
}