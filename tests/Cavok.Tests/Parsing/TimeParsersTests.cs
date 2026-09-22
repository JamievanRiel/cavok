using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class TimeParsersTests
{
    [Fact]
    public void ParsesDayTime() => Assert.Equal(new DayTime(21, 11, 25), TimeParsers.ParseDayTime("211125Z"));

    [Theory]
    [InlineData("211125")]
    [InlineData("321125Z")]
    [InlineData("002500Z")]
    [InlineData("212500Z")]
    [InlineData("211160Z")]
    [InlineData("21112Z")]
    [InlineData("2111255Z")]
    [InlineData("212400Z")]
    public void RejectsInvalidDayTime(string text) => Assert.Null(TimeParsers.ParseDayTime(text));

    [Theory]
    [InlineData("21112Z", true)]
    [InlineData("21112OZ", true)]
    [InlineData("24012KT", false)]
    [InlineData("Z", false)]
    public void LooksLikeDayTime(string text, bool expected) => Assert.Equal(expected, TimeParsers.LooksLikeDayTime(text));

    [Fact]
    public void ParsesFromGroup() => Assert.Equal(new DayTime(21, 14, 0), TimeParsers.ParseFromGroup("FM211400"));

    [Theory]
    [InlineData("FM2114")]
    [InlineData("FM211460")]
    [InlineData("TL211400")]
    [InlineData("FM212400")]
    public void RejectsInvalidFromGroup(string text) => Assert.Null(TimeParsers.ParseFromGroup(text));

    [Fact]
    public void ParsesPeriod() =>
        Assert.Equal(new ValidityPeriod(new DayHour(21, 6), new DayHour(22, 12)), TimeParsers.ParsePeriod("2106/2212"));

    [Fact]
    public void PeriodMayEndAtHourTwentyFour() =>
        Assert.Equal(new DayHour(21, 24), TimeParsers.ParsePeriod("2118/2124")!.Value.To);

    [Theory]
    [InlineData("2106/2225")]
    [InlineData("3206/0112")]
    [InlineData("2106-2212")]
    [InlineData("210/2212")]
    [InlineData("2106/22O2")]
    public void RejectsInvalidPeriod(string text) => Assert.Null(TimeParsers.ParsePeriod(text));

    [Theory]
    [InlineData("211824", true)]
    [InlineData("2106/22O2", true)]
    [InlineData("211/2115", true)]
    [InlineData("2111/215", true)]
    [InlineData("R24/0600", false)]
    [InlineData("9999", false)]
    [InlineData("12/09", false)]
    public void LooksLikePeriod(string text, bool expected) => Assert.Equal(expected, TimeParsers.LooksLikePeriod(text));

    [Theory]
    [InlineData("FM1030", 10, 30)]
    [InlineData("TL2400", 24, 0)]
    [InlineData("AT0000", 0, 0)]
    public void ParsesTrendTimes(string text, int hour, int minute)
    {
        Assert.True(TimeParsers.IsTrendTimeShape(text));
        Assert.Equal(new TimeOfDay(hour, minute), TimeParsers.ParseTimeOfDay(text, 2));
    }

    [Theory]
    [InlineData("FM2460")]
    [InlineData("FM1060")]
    [InlineData("FM10A0")]
    [InlineData("TL2430")]
    public void RejectsInvalidTrendTimes(string text) => Assert.Null(TimeParsers.ParseTimeOfDay(text, 2));

    [Theory]
    [InlineData("TL11000")]
    [InlineData("FM211400")]
    [InlineData("XX1030")]
    public void TrendTimeShapeNeedsTwoLettersAndFourCharacters(string text) =>
        Assert.False(TimeParsers.IsTrendTimeShape(text));

    [Theory]
    [InlineData("TX15/2114Z", TemperatureKind.Maximum, 15, 21, 14)]
    [InlineData("TNM02/2205Z", TemperatureKind.Minimum, -2, 22, 5)]
    public void ParsesTemperatureForecast(string text, TemperatureKind kind, int celsius, int day, int hour) =>
        Assert.Equal(new TemperatureForecast(kind, celsius, new DayHour(day, hour)), TimeParsers.ParseTemperatureForecast(text));

    [Theory]
    [InlineData("TX15/2114")]
    [InlineData("TX1/2114Z")]
    [InlineData("TY15/2114Z")]
    [InlineData("TX15/2125Z")]
    [InlineData("TXM2/2114Z")]
    [InlineData("TX")]
    public void RejectsInvalidTemperatureForecast(string text) => Assert.Null(TimeParsers.ParseTemperatureForecast(text));
}
