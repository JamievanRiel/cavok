using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class TemperatureParserTests
{
    [Theory]
    [InlineData("12/09", 12, 9)]
    [InlineData("M05/M07", -5, -7)]
    [InlineData("00/M01", 0, -1)]
    [InlineData("17///", 17, null)]
    [InlineData("/////", null, null)]
    [InlineData("12/", 12, null)]
    public void ParsesTemperatureAndDewPoint(string text, int? temperature, int? dewPoint)
    {
        TemperaturePair pair = TemperatureParser.Parse(text)!.Value;

        Assert.Equal(temperature, pair.Temperature);
        Assert.Equal(dewPoint, pair.DewPoint);
    }

    [Theory]
    [InlineData("1/2SM")]
    [InlineData("2106/2115")]
    [InlineData("12/9")]
    [InlineData("M5/M7")]
    [InlineData("////")]
    [InlineData("//")]
    [InlineData("R24/1200")]
    [InlineData("12-09")]
    [InlineData("W15/S4")]
    public void RejectsOtherGroups(string text) => Assert.Null(TemperatureParser.Parse(text));
}
