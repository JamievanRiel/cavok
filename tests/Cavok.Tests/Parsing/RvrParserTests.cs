using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class RvrParserTests
{
    [Fact]
    public void ParsesValueWithQualifierAndTendency()
    {
        RunwayVisualRange rvr = RvrParser.Parse("R24/P2000N")!;

        Assert.Equal("24", rvr.Runway);
        Assert.Equal(2000, rvr.Value);
        Assert.Equal(RvrQualifier.Above, rvr.Qualifier);
        Assert.Equal(RvrTendency.NoChange, rvr.Tendency);
        Assert.False(rvr.IsMissing);
    }

    [Fact]
    public void ParsesAVariableRange()
    {
        RunwayVisualRange rvr = RvrParser.Parse("R06L/0800VP1500U")!;

        Assert.Equal("06L", rvr.Runway);
        Assert.Equal(800, rvr.Value);
        Assert.Equal(1500, rvr.VariableMax);
        Assert.Equal(RvrQualifier.Above, rvr.VariableMaxQualifier);
        Assert.Equal(RvrTendency.Up, rvr.Tendency);
    }

    [Fact]
    public void ParsesFeetAsUsedInTheUnitedStates()
    {
        RunwayVisualRange rvr = RvrParser.Parse("R01L/0600V1000FT")!;

        Assert.True(rvr.IsFeet);
        Assert.Equal(600, rvr.Value);
        Assert.Equal(1000, rvr.VariableMax);
    }

    [Fact]
    public void ParsesBelowQualifier() => Assert.Equal(RvrQualifier.Below, RvrParser.Parse("R24/M0050")!.Qualifier);

    [Fact]
    public void TendencyMayFollowASlash() => Assert.Equal(RvrTendency.Down, RvrParser.Parse("R24/1100/D")!.Tendency);

    [Theory]
    [InlineData("R24/////")]
    [InlineData("R27/////N")]
    public void MissingValue(string text)
    {
        RunwayVisualRange rvr = RvrParser.Parse(text)!;

        Assert.True(rvr.IsMissing);
        Assert.Null(rvr.Value);
    }

    [Theory]
    [InlineData("R24/2000X")]
    [InlineData("R2/2000")]
    [InlineData("R24/200")]
    [InlineData("R24")]
    [InlineData("R24/P2000N5")]
    [InlineData("RA")]
    [InlineData("R2X/2000")]
    public void RejectsInvalidGroups(string text) => Assert.Null(RvrParser.Parse(text));
}
