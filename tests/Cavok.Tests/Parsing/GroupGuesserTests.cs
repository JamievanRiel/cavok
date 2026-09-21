using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class GroupGuesserTests
{
    [Theory]
    [InlineData("2400O", DiagnosticCode.InvalidVisibility)]
    [InlineData("24O12KT", DiagnosticCode.InvalidWind)]
    [InlineData("200V28O", DiagnosticCode.InvalidWindVariation)]
    [InlineData("R24/P200N", DiagnosticCode.InvalidRvr)]
    [InlineData("R24/29005X", DiagnosticCode.InvalidRunwayState)]
    [InlineData("BKN03O", DiagnosticCode.InvalidCloud)]
    [InlineData("VV0O2", DiagnosticCode.InvalidCloud)]
    [InlineData("TX15/2114", DiagnosticCode.InvalidTemperatureForecast)]
    [InlineData("PROB50", DiagnosticCode.InvalidChangeGroup)]
    [InlineData("FM2114OO", DiagnosticCode.InvalidChangeGroup)]
    [InlineData("FM1O30", DiagnosticCode.InvalidTrend)]
    [InlineData("Q1O13", DiagnosticCode.InvalidPressure)]
    [InlineData("21112OZ", DiagnosticCode.InvalidTime)]
    [InlineData("211125", DiagnosticCode.InvalidTime)]
    [InlineData("2106/22O2", DiagnosticCode.InvalidValidity)]
    [InlineData("12/O9", DiagnosticCode.InvalidTemperature)]
    [InlineData("-XX", DiagnosticCode.InvalidWeather)]
    [InlineData("VCXX", DiagnosticCode.InvalidWeather)]
    [InlineData("HELLO", DiagnosticCode.UnknownGroup)]
    [InlineData("OCV030", DiagnosticCode.UnknownGroup)]
    public void GuessesTheIntendedGroup(string token, DiagnosticCode expected) => Assert.Equal(expected, GroupGuesser.Guess(token));

    [Fact]
    public void SixDigitsAreAnOldValidityPeriodInATaf() =>
        Assert.Equal(DiagnosticCode.InvalidValidity, GroupGuesser.Guess("211824", taf: true));
}
