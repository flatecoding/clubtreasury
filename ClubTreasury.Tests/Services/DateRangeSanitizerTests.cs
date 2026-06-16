using AwesomeAssertions;
using MudBlazor;
using ClubTreasury.Data.Transaction.Dialogs;

namespace ClubTreasury.Tests.Services;

[TestFixture]
public class DateRangeSanitizerTests
{
    [Test]
    public void Sanitize_WithValidDates_ReturnsSameDates()
    {
        var start = new DateTime(2025, 6, 1);
        var end = new DateTime(2025, 6, 30);
        var dateRange = new DateRange(start, end);

        var result = DateRangeSanitizer.Sanitize(dateRange);

        result.Start.Should().Be(start);
        result.End.Should().Be(end);
    }

    [Test]
    public void Sanitize_WithNullDates_ReturnsNullDates()
    {
        var dateRange = new DateRange(null, null);

        var result = DateRangeSanitizer.Sanitize(dateRange);

        result.Start.Should().BeNull();
        result.End.Should().BeNull();
    }

    [Test]
    public void Sanitize_WithInvalidStartDate_ReturnsNullStart()
    {
        var invalidStart = DateTime.MinValue;
        var validEnd = new DateTime(2025, 6, 30);
        var dateRange = new DateRange(invalidStart, validEnd);

        var result = DateRangeSanitizer.Sanitize(dateRange);

        result.Start.Should().BeNull();
        result.End.Should().Be(validEnd);
    }

    [Test]
    public void Sanitize_WithInvalidEndDate_ReturnsNullEnd()
    {
        var validStart = new DateTime(2025, 6, 1);
        var invalidEnd = new DateTime(9999, 12, 31);
        var dateRange = new DateRange(validStart, invalidEnd);

        var result = DateRangeSanitizer.Sanitize(dateRange);

        result.Start.Should().Be(validStart);
        result.End.Should().BeNull();
    }

    [Test]
    public void Sanitize_WithBothDatesInvalid_ReturnsBothNull()
    {
        var dateRange = new DateRange(DateTime.MinValue, DateTime.MaxValue);

        var result = DateRangeSanitizer.Sanitize(dateRange);

        result.Start.Should().BeNull();
        result.End.Should().BeNull();
    }

    [Test]
    public void Sanitize_WithYearBelowMinimum_ReturnsNull()
    {
        var invalidDate = new DateTime(1899, 12, 31);
        var dateRange = new DateRange(invalidDate, null);

        var result = DateRangeSanitizer.Sanitize(dateRange);

        result.Start.Should().BeNull();
    }

    [Test]
    public void Sanitize_WithYearAboveMaximum_ReturnsNull()
    {
        var invalidDate = new DateTime(2101, 1, 1);
        var dateRange = new DateRange(invalidDate, null);

        var result = DateRangeSanitizer.Sanitize(dateRange);

        result.Start.Should().BeNull();
    }

    [Test]
    public void Sanitize_WithBoundaryYear1900_ReturnsValidDate()
    {
        var boundaryDate = new DateTime(1900, 1, 1);
        var dateRange = new DateRange(boundaryDate, null);

        var result = DateRangeSanitizer.Sanitize(dateRange);

        result.Start.Should().Be(boundaryDate);
    }

    [Test]
    public void Sanitize_WithBoundaryYear2100_ReturnsValidDate()
    {
        var boundaryDate = new DateTime(2100, 12, 31);
        var dateRange = new DateRange(boundaryDate, null);

        var result = DateRangeSanitizer.Sanitize(dateRange);

        result.Start.Should().Be(boundaryDate);
    }
}