using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class VisibilityParserTests
{
    [Theory]
    [InlineData("0800", 800)]
    [InlineData("0000", 0)]
    [InlineData("4000", 4000)]
    public void ParsesMeters(string text, int meters)
    {
        Visibility visibility = VisibilityParser.ParseMetric(text)!;

        Assert.Equal(meters, visibility.Meters);
        Assert.False(visibility.IsTenKmOrMore);
        Assert.Equal(meters, visibility.ToMeters());
    }

    [Fact]
    public void NineNineNineNineMeansTenKilometresOrMore()
    {
        Visibility visibility = VisibilityParser.ParseMetric("9999")!;

        Assert.True(visibility.IsTenKmOrMore);
        Assert.Equal(10000, visibility.Meters);
    }

    [Fact]
    public void NoDirectionalVariation()
    {
        Visibility visibility = VisibilityParser.ParseMetric("9999NDV")!;

        Assert.True(visibility.IsTenKmOrMore);
        Assert.True(visibility.NoDirectionalVariation);
    }

    [Fact]
    public void MissingVisibility()
    {
        Visibility visibility = VisibilityParser.ParseMetric("////")!;

        Assert.True(visibility.IsMissing);
        Assert.Null(visibility.Meters);
        Assert.Null(visibility.ToMeters());
    }

    [Theory]
    [InlineData("1500SW", 1500, CompassDirection.SouthWest)]
    [InlineData("4900SE", 4900, CompassDirection.SouthEast)]
    [InlineData("3000N", 3000, CompassDirection.North)]
    public void ParsesMinimumVisibility(string text, int meters, CompassDirection direction)
    {
        MinimumVisibility minimum = VisibilityParser.ParseMinimum(text)!;

        Assert.Equal(meters, minimum.Meters);
        Assert.Equal(direction, minimum.Direction);
    }

    [Theory]
    [InlineData("1500XY")]
    [InlineData("150SW")]
    [InlineData("1500")]
    public void RejectsInvalidMinimumVisibility(string text) => Assert.Null(VisibilityParser.ParseMinimum(text));

    [Theory]
    [InlineData("10SM", 10.0, false, false)]
    [InlineData("P6SM", 6.0, false, true)]
    [InlineData("1/2SM", 0.5, false, false)]
    [InlineData("M1/4SM", 0.25, true, false)]
    [InlineData("3/4SM", 0.75, false, false)]
    public void ParsesStatuteMiles(string text, double miles, bool lessThan, bool moreThan)
    {
        Visibility visibility = VisibilityParser.ParseStatuteMiles(text)!;

        Assert.Equal(miles, visibility.StatuteMiles);
        Assert.Equal(lessThan, visibility.IsLessThan);
        Assert.Equal(moreThan, visibility.IsMoreThan);
        Assert.Null(visibility.Meters);
    }

    [Fact]
    public void ReadsWholeAndFractionalMilesAcrossTwoTokens()
    {
        var cursor = new TokenCursor(Tokenizer.Tokenize("1 1/2SM BR"));

        Visibility visibility = VisibilityParser.Read(cursor, out int tokenCount)!;

        Assert.Equal(1.5, visibility.StatuteMiles);
        Assert.Equal(2, tokenCount);
    }

    [Fact]
    public void ALoneDigitIsNotVisibility()
    {
        var cursor = new TokenCursor(Tokenizer.Tokenize("1 BR"));

        Assert.Null(VisibilityParser.Read(cursor, out _));
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("99A9")]
    [InlineData("SM")]
    [InlineData("1/0SM")]
    [InlineData("PSM")]
    [InlineData("123SM")]
    public void RejectsInvalidGroups(string text) =>
        Assert.Null(VisibilityParser.ParseMetric(text) ?? VisibilityParser.ParseStatuteMiles(text));

    [Fact]
    public void ToMetersConvertsStatuteMiles() =>
        Assert.Equal(1609.344, VisibilityParser.ParseStatuteMiles("1SM")!.ToMeters()!.Value, 3);
}
