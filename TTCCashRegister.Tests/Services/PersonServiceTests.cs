using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using TTCCashRegister.Data;
using TTCCashRegister.Data.OperationResult;
using TTCCashRegister.Data.Person;

namespace TTCCashRegister.Tests.Services;

[TestFixture]
public class PersonServiceTests
{
    private CashDataContext _context = null!;
    private ILogger<PersonService> _logger = null!;
    private IOperationResultFactory _operationResultFactory = null!;
    private IStringLocalizer<Translation> _localizer = null!;
    private PersonService _sut = null!;
    private bool _contextDisposed;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CashDataContext(options);
        _contextDisposed = false;
        _logger = A.Fake<ILogger<PersonService>>();
        _operationResultFactory = A.Fake<IOperationResultFactory>();
        _localizer = A.Fake<IStringLocalizer<Translation>>();

        A.CallTo(() => _localizer["Person"])
            .Returns(new LocalizedString("Person", "Person"));
        A.CallTo(() => _localizer["Exception"])
            .Returns(new LocalizedString("Exception", "An error occurred"));

        _sut = new PersonService(_context, _logger, _localizer, _operationResultFactory);
    }

    [TearDown]
    public void TearDown()
    {
        if (_contextDisposed) return;

        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetAllPersonsAsync Tests

    [Test]
    public async Task GetAllPersonsAsync_WhenNoPersonsExist_ShouldReturnEmptyList()
    {
        // Act
        var result = await _sut.GetAllPersonsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllPersonsAsync_WhenPersonsExist_ShouldReturnAllPersons()
    {
        // Arrange
        var persons = new List<PersonModel>
        {
            new() { Name = "John Doe" },
            new() { Name = "Jane Smith" },
            new() { Name = "Bob Wilson" }
        };
        await _context.Persons.AddRangeAsync(persons);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllPersonsAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Select(p => p.Name).Should().BeEquivalentTo(["John Doe", "Jane Smith", "Bob Wilson"]);
    }

    #endregion

    #region GetPersonById Tests

    [Test]
    public async Task GetPersonById_WhenPersonExists_ShouldReturnPerson()
    {
        // Arrange
        var person = new PersonModel { Name = "Test Person" };
        await _context.Persons.AddAsync(person);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetPersonById(person.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Person");
    }

    [Test]
    public async Task GetPersonById_WhenPersonDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetPersonById(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetFirstEntry Tests

    [Test]
    public async Task GetFirstEntry_WhenPersonsExist_ShouldReturnFirstPerson()
    {
        // Arrange
        var persons = new List<PersonModel>
        {
            new() { Name = "First Person" },
            new() { Name = "Second Person" }
        };
        await _context.Persons.AddRangeAsync(persons);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetFirstEntry();

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("First Person");
    }

    [Test]
    public async Task GetFirstEntry_WhenNoPersonsExist_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetFirstEntry();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region AddPersonAsync Tests

    [Test]
    public async Task AddPersonAsync_WhenValidPerson_ShouldAddAndReturnSuccess()
    {
        // Arrange
        var person = new PersonModel { Name = "New Person" };
        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully added"
        };
        A.CallTo(() => _operationResultFactory.SuccessAdded(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.AddPersonAsync(person);

        // Assert
        result.Should().Be(expectedResult);
        var addedPerson = await _context.Persons.FirstOrDefaultAsync(p => p.Name == "New Person");
        addedPerson.Should().NotBeNull();
        A.CallTo(() => _operationResultFactory.SuccessAdded(
            A<string>.That.Contains("New Person"),
            A<object?>._)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task AddPersonAsync_WhenExceptionOccurs_ShouldReturnFailure()
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
        _context.Dispose();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        disposedContext.Dispose();

        _sut = new PersonService(disposedContext, _logger, _localizer, _operationResultFactory);

        var person = new PersonModel { Name = "New Person" };

        // Act
        var result = await _sut.AddPersonAsync(person);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region UpdatePersonAsync Tests

    [Test]
    public async Task UpdatePersonAsync_WhenValidPerson_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var person = new PersonModel { Name = "Original Name" };
        await _context.Persons.AddAsync(person);
        await _context.SaveChangesAsync();

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully updated"
        };
        A.CallTo(() => _operationResultFactory.SuccessUpdated(A<string>._, A<object?>._))
            .Returns(expectedResult);

        person.Name = "Updated Name";

        // Act
        var result = await _sut.UpdatePersonAsync(person);

        // Assert
        result.Should().Be(expectedResult);
        var updatedPerson = await _context.Persons.FindAsync(person.Id);
        updatedPerson!.Name.Should().Be("Updated Name");
    }

    [Test]
    public async Task UpdatePersonAsync_WhenExceptionOccurs_ShouldReturnFailure()
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
        _context.Dispose();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        disposedContext.Dispose();

        _sut = new PersonService(disposedContext, _logger, _localizer, _operationResultFactory);

        var person = new PersonModel { Name = "Test" };

        // Act
        var result = await _sut.UpdatePersonAsync(person);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region DeletePersonAsync Tests

    [Test]
    public async Task DeletePersonAsync_WhenPersonExists_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var person = new PersonModel { Name = "To Be Deleted" };
        await _context.Persons.AddAsync(person);
        await _context.SaveChangesAsync();
        var id = person.Id;

        var expectedResult = new OperationResult
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully deleted"
        };
        A.CallTo(() => _operationResultFactory.SuccessDeleted(A<string>._, A<object?>._))
            .Returns(expectedResult);

        // Act
        var result = await _sut.DeletePersonAsync(id);

        // Assert
        result.Should().Be(expectedResult);
        var deletedPerson = await _context.Persons.FindAsync(id);
        deletedPerson.Should().BeNull();
    }

    [Test]
    public async Task DeletePersonAsync_WhenPersonDoesNotExist_ShouldReturnNotFound()
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
        var result = await _sut.DeletePersonAsync(999);

        // Assert
        result.Should().Be(expectedResult);
        A.CallTo(() => _operationResultFactory.NotFound(A<string>._, 999))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task DeletePersonAsync_WhenExceptionOccurs_ShouldReturnFailure()
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
        _context.Dispose();
        _contextDisposed = true;

        var options = new DbContextOptionsBuilder<CashDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new CashDataContext(options);
        disposedContext.Dispose();

        _sut = new PersonService(disposedContext, _logger, _localizer, _operationResultFactory);

        // Act
        var result = await _sut.DeletePersonAsync(1);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion
}