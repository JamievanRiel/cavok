using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class CloudParserTests
{
    [Theory]
    [InlineData("FEW012", CloudCover.Few, 1200, null)]
    [InlineData("BKN030CB", CloudCover.Broken, 3000, CloudType.Cumulonimbus)]
    [InlineData("SCT025TCU", CloudCover.Scattered, 2500, CloudType.ToweringCumulus)]
    [InlineData("OVC000", CloudCover.Overcast, 0, null)]
    [InlineData("VV002", CloudCover.VerticalVisibility, 200, null)]
    [InlineData("VV///", CloudCover.VerticalVisibility, null, null)]
    [InlineData("///015", null, 1500, null)]
    [InlineData("//////CB", null, null, CloudType.Cumulonimbus)]
    [InlineData("///CB", null, null, CloudType.Cumulonimbus)]
    [InlineData("///TCU", null, null, CloudType.ToweringCumulus)]
    public void ParsesLayers(string text, CloudCover? cover, int? height, CloudType? type)
    {
        CloudLayer layer = CloudParser.ParseLayer(text)!;

        Assert.Equal(cover, layer.Cover);
        Assert.Equal(height, layer.HeightFeet);
        Assert.Equal(type, layer.Type);
    }

    [Theory]
    [InlineData("BKN018///")]
    [InlineData("/////////")]
    public void TypeNotObservable(string text) => Assert.True(CloudParser.ParseLayer(text)!.IsTypeNotObservable);

    [Fact]
    public void CompletelyMissingLayer()
    {
        CloudLayer layer = CloudParser.ParseLayer("//////")!;

        Assert.Null(layer.Cover);
        Assert.Null(layer.HeightFeet);
        Assert.Null(layer.Type);
    }

    [Theory]
    [InlineData("NSC", CloudCondition.NoSignificantCloud)]
    [InlineData("NCD", CloudCondition.NoCloudDetected)]
    [InlineData("SKC", CloudCondition.SkyClear)]
    [InlineData("CLR", CloudCondition.Clear)]
    public void ParsesConditions(string text, CloudCondition condition) =>
        Assert.Equal(condition, CloudParser.ParseCondition(text));

    [Theory]
    [InlineData("FEW12")]
    [InlineData("FEW012XX")]
    [InlineData("ABC012")]
    [InlineData("VV002CB")]
    [InlineData("FEW")]
    [InlineData("BKN0O5")]
    [InlineData("VV")]
    [InlineData("NSW")]
    [InlineData("////CB")]
    [InlineData("///")]
    public void RejectsInvalidGroups(string text)
    {
        Assert.Null(CloudParser.ParseLayer(text));
        Assert.Null(CloudParser.ParseCondition(text));
    }
}
