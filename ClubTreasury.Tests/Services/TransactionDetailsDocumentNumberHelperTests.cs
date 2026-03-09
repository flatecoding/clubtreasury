using AwesomeAssertions;
using ClubTreasury.Data.TransactionDetails;

namespace ClubTreasury.Tests.Services;

[TestFixture]
public class TransactionDetailsDocumentNumberHelperTests
{
    [Test]
    public void GetNextDetailDocumentNumber_WhenNoDetailsExist_ShouldReturnBaseNumberPlusOne()
    {
        var result = TransactionDetailsDocumentNumberHelper.GetNextDetailDocumentNumber(2400, []);

        result.Should().Be(240001);
    }

    [Test]
    public void GetNextDetailDocumentNumber_WhenDetailsAreNull_ShouldReturnBaseNumberPlusOne()
    {
        var result = TransactionDetailsDocumentNumberHelper.GetNextDetailDocumentNumber(2400, null);

        result.Should().Be(240001);
    }

    [Test]
    public void GetNextDetailDocumentNumber_WhenOneDetailExists_ShouldReturnNextNumber()
    {
        var details = new List<TransactionDetailsModel>
        {
            new() { DocumentNumber = 240001 }
        };

        var result = TransactionDetailsDocumentNumberHelper.GetNextDetailDocumentNumber(2400, details);

        result.Should().Be(240002);
    }

    [Test]
    public void GetNextDetailDocumentNumber_WhenMultipleDetailsExist_ShouldReturnNextAfterMax()
    {
        var details = new List<TransactionDetailsModel>
        {
            new() { DocumentNumber = 240001 },
            new() { DocumentNumber = 240003 },
            new() { DocumentNumber = 240002 }
        };

        var result = TransactionDetailsDocumentNumberHelper.GetNextDetailDocumentNumber(2400, details);

        result.Should().Be(240004);
    }

    [Test]
    public void GetNextDetailDocumentNumber_WhenDetailsHaveNoDocumentNumber_ShouldReturnBaseNumberPlusOne()
    {
        var details = new List<TransactionDetailsModel>
        {
            new() { DocumentNumber = null },
            new() { DocumentNumber = null }
        };

        var result = TransactionDetailsDocumentNumberHelper.GetNextDetailDocumentNumber(2400, details);

        result.Should().Be(240001);
    }

    [Test]
    public void GetNextDetailDocumentNumber_WhenDetailsHaveDocumentNumbersOutsideRange_ShouldReturnBaseNumberPlusOne()
    {
        var details = new List<TransactionDetailsModel>
        {
            new() { DocumentNumber = 999999 },
            new() { DocumentNumber = 100001 }
        };

        var result = TransactionDetailsDocumentNumberHelper.GetNextDetailDocumentNumber(2400, details);

        result.Should().Be(240001);
    }

    [Test]
    public void GetNextDetailDocumentNumber_WhenMixOfValidAndInvalidDocumentNumbers_ShouldOnlyConsiderValidRange()
    {
        var details = new List<TransactionDetailsModel>
        {
            new() { DocumentNumber = 240002 },
            new() { DocumentNumber = null },
            new() { DocumentNumber = 999999 }
        };

        var result = TransactionDetailsDocumentNumberHelper.GetNextDetailDocumentNumber(2400, details);

        result.Should().Be(240003);
    }

    [Test]
    public void GetNextDetailDocumentNumber_WithDifferentParentNumber_ShouldUseCorrectBase()
    {
        var details = new List<TransactionDetailsModel>
        {
            new() { DocumentNumber = 100 * 100 + 1 }
        };

        var result = TransactionDetailsDocumentNumberHelper.GetNextDetailDocumentNumber(100, details);

        result.Should().Be(10002);
    }

    [Test]
    public void GetNextDetailDocumentNumber_WithParentNumberOne_ShouldWork()
    {
        var result = TransactionDetailsDocumentNumberHelper.GetNextDetailDocumentNumber(1, []);

        result.Should().Be(101);
    }
}