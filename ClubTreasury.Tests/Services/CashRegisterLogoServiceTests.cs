using FakeItEasy;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ClubTreasury.Data;
using ClubTreasury.Data.CashRegister;
using ClubTreasury.Data.OperationResult;

namespace ClubTreasury.Tests.Services;

[TestFixture]
public class CashRegisterLogoServiceTests
{
    private CashDataContext _context = null!;
    private ILogger<CashRegisterLogoService> _logger = null!;
    private IResultFactory _resultFactory = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private CashRegisterLogoService _sut = null!;
    private bool _contextDisposed;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CashDataContext(options);
        _contextDisposed = false;
        _logger = A.Fake<ILogger<CashRegisterLogoService>>();
        _resultFactory = A.Fake<IResultFactory>();
        _localizer = A.Fake<IStringLocalizer<Translation>>();

        A.CallTo(() => _localizer["CashRegister"])
            .Returns(new LocalizedString("CashRegister", "Cash Register"));
        A.CallTo(() => _localizer["Exception"])
            .Returns(new LocalizedString("Exception", "An error occurred"));

        _sut = new CashRegisterLogoService(_context, _logger, _resultFactory, _localizer);
    }

    [TearDown]
    public void TearDown()
    {
        if (_contextDisposed) return;

        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

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
        var expectedResult = Result.Success("Successfully updated");
        A.CallTo(() => _resultFactory.SuccessUpdated(A<string>._, A<object?>._))
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
        var expectedResult = Result.Success("Successfully updated");
        A.CallTo(() => _resultFactory.SuccessUpdated(A<string>._, A<object?>._))
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

        _sut = new CashRegisterLogoService(disposedContext, _logger, _resultFactory, _localizer);

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

        var expectedResult = Result.Success("Successfully deleted");
        A.CallTo(() => _resultFactory.SuccessDeleted(A<string>._, A<object?>._))
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

        var expectedResult = Result.Success("Successfully deleted");
        A.CallTo(() => _resultFactory.SuccessDeleted(A<string>._, A<object?>._))
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

        _sut = new CashRegisterLogoService(disposedContext, _logger, _resultFactory, _localizer);

        // Act
        var result = await _sut.DeleteLogoAsync(1);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion
}
