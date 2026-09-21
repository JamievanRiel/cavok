using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class WeatherParserTests
{
    [Theory]
    [InlineData("-RA", WeatherIntensity.Light, null, new[] { WeatherType.Rain })]
    [InlineData("+TSRA", WeatherIntensity.Heavy, WeatherDescriptor.Thunderstorm, new[] { WeatherType.Rain })]
    [InlineData("VCSH", WeatherIntensity.InVicinity, WeatherDescriptor.Showers, new WeatherType[0])]
    [InlineData("SHRASN", WeatherIntensity.Moderate, WeatherDescriptor.Showers, new[] { WeatherType.Rain, WeatherType.Snow })]
    [InlineData("FZFG", WeatherIntensity.Moderate, WeatherDescriptor.Freezing, new[] { WeatherType.Fog })]
    [InlineData("BR", WeatherIntensity.Moderate, null, new[] { WeatherType.Mist })]
    [InlineData("TS", WeatherIntensity.Moderate, WeatherDescriptor.Thunderstorm, new WeatherType[0])]
    [InlineData("+FC", WeatherIntensity.Heavy, null, new[] { WeatherType.FunnelCloud })]
    [InlineData("-RADZ", WeatherIntensity.Light, null, new[] { WeatherType.Rain, WeatherType.Drizzle })]
    [InlineData("BLSN", WeatherIntensity.Moderate, WeatherDescriptor.Blowing, new[] { WeatherType.Snow })]
    [InlineData("TSRAGS", WeatherIntensity.Moderate, WeatherDescriptor.Thunderstorm, new[] { WeatherType.Rain, WeatherType.SmallHail })]
    [InlineData("VCFG", WeatherIntensity.InVicinity, null, new[] { WeatherType.Fog })]
    [InlineData("MIFG", WeatherIntensity.Moderate, WeatherDescriptor.Shallow, new[] { WeatherType.Fog })]
    [InlineData("BCFG", WeatherIntensity.Moderate, WeatherDescriptor.Patches, new[] { WeatherType.Fog })]
    [InlineData("UP", WeatherIntensity.Moderate, null, new[] { WeatherType.UnknownPrecipitation })]
    public void ParsesWeather(string text, WeatherIntensity intensity, WeatherDescriptor? descriptor, WeatherType[] types)
    {
        WeatherPhenomenon weather = WeatherParser.Parse(text)!;

        Assert.Equal(intensity, weather.Intensity);
        Assert.Equal(descriptor, weather.Descriptor);
        Assert.Equal(types, weather.Types);
        Assert.False(weather.IsNotObservable);
    }

    [Fact]
    public void NotObservable() => Assert.True(WeatherParser.Parse("//")!.IsNotObservable);

    [Theory]
    [InlineData("-")]
    [InlineData("+")]
    [InlineData("VC")]
    [InlineData("SH")]
    [InlineData("RAX")]
    [InlineData("XX")]
    [InlineData("NSW")]
    [InlineData("RE")]
    [InlineData("TSX")]
    [InlineData("-SHRAZZ")]
    [InlineData("///")]
    public void RejectsInvalidGroups(string text) => Assert.Null(WeatherParser.Parse(text));
}
