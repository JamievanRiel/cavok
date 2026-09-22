using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class IcingTurbulenceParserTests
{
    [Theory]
    [InlineData("651109", 5, 11000, 9000)]
    [InlineData("651103", 5, 11000, 3000)]
    [InlineData("620304", 2, 3000, 4000)]
    [InlineData("690000", 9, 0, 0)]
    public void ParsesIcing(string text, int type, int baseFeet, int thicknessFeet) =>
        Assert.Equal(new IcingLayer(type, baseFeet, thicknessFeet), IcingTurbulenceParser.ParseIcing(text));

    [Theory]
    [InlineData("510005", 1, 0, 5000)]
    [InlineData("520002", 2, 0, 2000)]
    [InlineData("530005", 3, 0, 5000)]
    [InlineData("591209", 9, 12000, 9000)]
    public void ParsesTurbulence(string text, int type, int baseFeet, int thicknessFeet) =>
        Assert.Equal(new TurbulenceLayer(type, baseFeet, thicknessFeet), IcingTurbulenceParser.ParseTurbulence(text));

    [Theory]
    [InlineData("510005")]
    [InlineData("65110")]
    [InlineData("6511090")]
    [InlineData("65110X")]
    [InlineData("6/1109")]
    [InlineData("211824")]
    public void RejectsWhatIsNotAnIcingGroup(string text) => Assert.Null(IcingTurbulenceParser.ParseIcing(text));

    [Theory]
    [InlineData("651109")]
    [InlineData("51000")]
    [InlineData("5100050")]
    [InlineData("5X0005")]
    [InlineData("51000/")]
    [InlineData("211824")]
    public void RejectsWhatIsNotATurbulenceGroup(string text) => Assert.Null(IcingTurbulenceParser.ParseTurbulence(text));
}
