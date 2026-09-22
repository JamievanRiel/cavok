using System.Globalization;

namespace Cavok.Tests.Model;

public class DayTimeTests
{
    [Theory]
    [InlineData("2026-09-21T12:00:00Z", 21, 11, 25, "2026-09-21T11:25:00Z")]
    [InlineData("2026-10-01T00:10:00Z", 30, 23, 50, "2026-09-30T23:50:00Z")]
    [InlineData("2026-01-01T00:20:00Z", 31, 23, 50, "2025-12-31T23:50:00Z")]
    [InlineData("2026-03-01T00:30:00Z", 28, 23, 50, "2026-02-28T23:50:00Z")]
    [InlineData("2026-09-30T22:00:00Z", 1, 0, 0, "2026-10-01T00:00:00Z")]
    public void ResolvesToTheClosestMonth(string reference, int day, int hour, int minute, string expected)
    {
        DateTimeOffset actual = new DayTime(day, hour, minute).ToDateTimeOffset(Parse(reference));

        Assert.Equal(Parse(expected), actual);
    }

    [Fact]
    public void Day31AfterAThirtyDayMonthResolvesToThePreviousMonthWithThatDay()
    {
        // September has no 31st, so a report from "day 31" received on 1 October is from 31 August.
        DateTimeOffset actual = new DayTime(31, 23, 0).ToDateTimeOffset(Parse("2026-10-01T00:00:00Z"));

        Assert.Equal(Parse("2026-08-31T23:00:00Z"), actual);
    }

    [Fact]
    public void HourTwentyFourIsMidnightAtTheEndOfTheDay()
    {
        DateTimeOffset actual = new DayHour(21, 24).ToDateTimeOffset(Parse("2026-09-21T12:00:00Z"));

        Assert.Equal(Parse("2026-09-22T00:00:00Z"), actual);
    }

    [Fact]
    public void InvalidDayThrows() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new DayTime(32, 0, 0).ToDateTimeOffset(DateTimeOffset.UtcNow));

    [Theory]
    [InlineData("0001-01-15T00:00:00Z")]
    [InlineData("0001-02-28T00:00:00Z")]
    [InlineData("9999-12-15T00:00:00Z")]
    public void ReferenceAtTheEdgeOfTheDateRangeThrows(string reference)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DayTime(15, 12, 0).ToDateTimeOffset(Parse(reference)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DayHour(15, 12).ToDateTimeOffset(Parse(reference)));
    }

    [Fact]
    public void ReferenceJustInsideTheDateRangeWorks()
    {
        Assert.Equal(Parse("0001-03-15T12:00:00Z"), new DayTime(15, 12, 0).ToDateTimeOffset(Parse("0001-03-15T00:00:00Z")));
        Assert.Equal(Parse("9999-11-15T12:00:00Z"), new DayTime(15, 12, 0).ToDateTimeOffset(Parse("9999-11-15T00:00:00Z")));
    }

    private static DateTimeOffset Parse(string text) => DateTimeOffset.Parse(text, CultureInfo.InvariantCulture);
}
