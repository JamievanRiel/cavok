using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class GroupReaderTests
{
    [Theory]
    [InlineData("AUTO", "Auto")]
    [InlineData("COR", "Corrected")]
    [InlineData("24012KT", "Wind")]
    [InlineData("/////KT", "Wind")]
    [InlineData("200V280", "WindVariation")]
    [InlineData("CAVOK", "Cavok")]
    [InlineData("9999", "Visibility")]
    [InlineData("////", "Visibility")]
    [InlineData("P6SM", "Visibility")]
    [InlineData("1500SW", "MinimumVisibility")]
    [InlineData("R24/P2000N", "RunwayVisualRange")]
    [InlineData("R24//////", "RunwayVisualRange")]
    [InlineData("R24/290050", "RunwayState")]
    [InlineData("24290050", "RunwayState")]
    [InlineData("SNOCLO", "RunwayState")]
    [InlineData("-SHRA", "Weather")]
    [InlineData("//", "Weather")]
    [InlineData("NSW", "NoSignificantWeather")]
    [InlineData("RERA", "RecentWeather")]
    [InlineData("RE//", "RecentWeather")]
    [InlineData("BKN030CB", "Cloud")]
    [InlineData("//////", "Cloud")]
    [InlineData("/////////", "Cloud")]
    [InlineData("VV///", "Cloud")]
    [InlineData("NCD", "CloudCondition")]
    [InlineData("12/09", "Temperature")]
    [InlineData("17///", "Temperature")]
    [InlineData("/////", "Temperature")]
    [InlineData("Q1013", "Pressure")]
    [InlineData("Q////", "Pressure")]
    [InlineData("QNH3043INS", "Pressure")]
    [InlineData("W15/S4", "Sea")]
    [InlineData("BLU+BLU+", "ColorCode")]
    [InlineData("RED", "ColorCode")]
    public void RecognisesSingleTokenGroups(string text, string kind)
    {
        Group group = GroupReader.Read(Cursor(text))!;

        Assert.Equal(kind, group.Kind.ToString());
        Assert.Equal(1, group.TokenCount);
    }

    [Theory]
    [InlineData("1 1/2SM", "Visibility", 2)]
    [InlineData("WS R24", "WindShear", 2)]
    [InlineData("WS ALL RWY", "WindShear", 3)]
    public void RecognisesMultiTokenGroups(string text, string kind, int tokenCount)
    {
        Group group = GroupReader.Read(Cursor(text))!;

        Assert.Equal(kind, group.Kind.ToString());
        Assert.Equal(tokenCount, group.TokenCount);
    }

    [Fact]
    public void MissingPressureHasNoValue() => Assert.Null(GroupReader.Read(Cursor("Q////"))!.Value);

    [Theory]
    [InlineData("NOSIG")]
    [InlineData("TEMPO")]
    [InlineData("BECMG")]
    [InlineData("RMK")]
    [InlineData("FM211400")]
    [InlineData("PROB30")]
    [InlineData("TX15/2114Z")]
    [InlineData("2106/2212")]
    [InlineData("2400O")]
    [InlineData("EHAM")]
    public void ReturnsNullForKeywordsAndUnknownTokens(string text) => Assert.Null(GroupReader.Read(Cursor(text)));

    [Fact]
    public void DoesNotConsume()
    {
        TokenCursor cursor = Cursor("WS ALL RWY");

        GroupReader.Read(cursor);

        Assert.Equal(0, cursor.Index);
    }

    private static TokenCursor Cursor(string text) => new TokenCursor(Tokenizer.Tokenize(text));
}
