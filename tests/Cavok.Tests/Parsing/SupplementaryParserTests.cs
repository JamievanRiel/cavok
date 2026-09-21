using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class SupplementaryParserTests
{
    [Theory]
    [InlineData("WS R24L", "24L", false, 2)]
    [InlineData("WS RWY24", "24", false, 2)]
    [InlineData("WS ALL RWY", null, true, 3)]
    public void ParsesWindShear(string text, string? runway, bool allRunways, int tokens)
    {
        WindShear windShear = WindShearParser.Read(new TokenCursor(Tokenizer.Tokenize(text)), out int tokenCount)!;

        Assert.Equal(runway, windShear.Runway);
        Assert.Equal(allRunways, windShear.AllRunways);
        Assert.Equal(tokens, tokenCount);
    }

    [Theory]
    [InlineData("WS")]
    [InlineData("WS R2")]
    [InlineData("WS XYZ")]
    [InlineData("WS ALL")]
    public void RejectsIncompleteWindShear(string text) =>
        Assert.Null(WindShearParser.Read(new TokenCursor(Tokenizer.Tokenize(text)), out _));

    [Theory]
    [InlineData("W15/S4", 15, 4, null)]
    [InlineData("W///S5", null, 5, null)]
    [InlineData("W18/H14", 18, null, 14)]
    [InlineData("W///H///", null, null, null)]
    [InlineData("WM01/S2", -1, 2, null)]
    public void ParsesSeaCondition(string text, int? temperature, int? state, int? waveHeight)
    {
        SeaCondition sea = SeaParser.Parse(text)!;

        Assert.Equal(temperature, sea.SeaTemperature);
        Assert.Equal(state, sea.StateOfSea);
        Assert.Equal(waveHeight, sea.WaveHeightDecimeters);
    }

    [Theory]
    [InlineData("W15/X4")]
    [InlineData("W15S4")]
    [InlineData("W15/S45")]
    [InlineData("W1/S4")]
    [InlineData("WHT")]
    [InlineData("W15/H1234")]
    public void RejectsInvalidSeaCondition(string text) => Assert.Null(SeaParser.Parse(text));

    [Fact]
    public void ParsesRunwayState()
    {
        RunwayState state = RunwayStateParser.Parse("R24L/451293")!;

        Assert.Equal("24L", state.Runway);
        Assert.Equal(4, state.Deposit);
        Assert.Equal(5, state.Extent);
        Assert.Equal(12, state.Depth);
        Assert.Equal(93, state.Friction);
        Assert.Equal("R24L/451293", state.RawGroup);
    }

    [Theory]
    [InlineData("R88/290050", true, false)]
    [InlineData("R99/290050", false, true)]
    public void SpecialRunwayDesignators(string text, bool allRunways, bool repeated)
    {
        RunwayState state = RunwayStateParser.Parse(text)!;

        Assert.Equal(allRunways, state.AllRunways);
        Assert.Equal(repeated, state.IsRepeated);
        Assert.Null(state.Runway);
    }

    [Theory]
    [InlineData("R24/CLRD62", 62)]
    [InlineData("R24/CLRD//", null)]
    public void ClearedRunway(string text, int? friction)
    {
        RunwayState state = RunwayStateParser.Parse(text)!;

        Assert.True(state.Cleared);
        Assert.Equal(friction, state.Friction);
        Assert.Equal("24", state.Runway);
    }

    [Theory]
    [InlineData("SNOCLO", null, true)]
    [InlineData("R/SNOCLO", null, true)]
    [InlineData("R24/SNOCLO", "24", false)]
    public void ClosedDueToSnow(string text, string? runway, bool allRunways)
    {
        RunwayState state = RunwayStateParser.Parse(text)!;

        Assert.True(state.SnowClosed);
        Assert.Equal(runway, state.Runway);
        Assert.Equal(allRunways, state.AllRunways);
    }

    [Fact]
    public void LegacyEightDigitFormat()
    {
        RunwayState state = RunwayStateParser.Parse("24290050")!;

        Assert.Equal("24", state.Runway);
        Assert.Equal(2, state.Deposit);
        Assert.Equal(50, state.Friction);
    }

    [Fact]
    public void MissingValues()
    {
        // Separator plus six missing characters. "R24//////" (one slash fewer) is a missing RVR instead.
        RunwayState state = RunwayStateParser.Parse("R24/" + "//////")!;

        Assert.Equal("24", state.Runway);
        Assert.Null(state.Deposit);
        Assert.Null(state.Friction);
    }

    [Theory]
    [InlineData("R24/2900")]
    [InlineData("R24/CLRD6")]
    [InlineData("R24/P2000N")]
    [InlineData("2429005")]
    [InlineData("R24/29005A")]
    public void RejectsOtherGroups(string text) => Assert.Null(RunwayStateParser.Parse(text));

    [Theory]
    [InlineData("BLU", new[] { ColorState.Blue })]
    [InlineData("BLU+BLU+", new[] { ColorState.BluePlus, ColorState.BluePlus })]
    [InlineData("WHT", new[] { ColorState.White })]
    [InlineData("YLO1", new[] { ColorState.Yellow1 })]
    [InlineData("GRNAMB", new[] { ColorState.Green, ColorState.Amber })]
    [InlineData("RED", new[] { ColorState.Red })]
    public void ParsesColorCodes(string text, ColorState[] states) =>
        Assert.Equal(states, ColorCodeParser.Parse(text)!.Select(c => c.State));

    [Fact]
    public void BlackPrefix()
    {
        ColorCode code = Assert.Single(ColorCodeParser.Parse("BLACKBLU+")!);

        Assert.Equal(new ColorCode(ColorState.BluePlus, IsBlack: true), code);
    }

    [Theory]
    [InlineData("BLUE")]
    [InlineData("BLACK")]
    [InlineData("XYZ")]
    [InlineData("BL")]
    [InlineData("")]
    public void RejectsInvalidColorCodes(string text) => Assert.Null(ColorCodeParser.Parse(text));
}
