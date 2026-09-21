using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class WindParserTests
{
    [Fact]
    public void ParsesDirectionSpeedAndGust()
    {
        Wind wind = WindParser.Parse("24012G25KT")!;

        Assert.Equal(240, wind.Direction);
        Assert.Equal(new Speed(12, SpeedUnit.Knots), wind.Speed);
        Assert.Equal(new Speed(25, SpeedUnit.Knots), wind.Gust);
        Assert.False(wind.IsVariable);
        Assert.False(wind.IsCalm);
        Assert.False(wind.IsMissing);
    }

    [Theory]
    [InlineData("VRB03KT", null, true, 3)]
    [InlineData("00000KT", 0, false, 0)]
    [InlineData("240120KT", 240, false, 120)]
    [InlineData("///05KT", null, false, 5)]
    public void ParsesVariants(string text, int? direction, bool variable, int speed)
    {
        Wind wind = WindParser.Parse(text)!;

        Assert.Equal(direction, wind.Direction);
        Assert.Equal(variable, wind.IsVariable);
        Assert.Equal(speed, wind.Speed?.Value);
    }

    [Fact]
    public void CalmWind() => Assert.True(WindParser.Parse("00000KT")!.IsCalm);

    [Fact]
    public void MissingWindFromAnAutomaticStation()
    {
        Wind wind = WindParser.Parse("/////KT")!;

        Assert.True(wind.IsMissing);
        Assert.Null(wind.Direction);
        Assert.Null(wind.Speed);
    }

    [Theory]
    [InlineData("27015MPS", SpeedUnit.MetersPerSecond)]
    [InlineData("27015KMH", SpeedUnit.KilometersPerHour)]
    [InlineData("27015KT", SpeedUnit.Knots)]
    public void ParsesUnits(string text, SpeedUnit unit) =>
        Assert.Equal(new Speed(15, unit), WindParser.Parse(text)!.Speed);

    [Fact]
    public void AboveMarker()
    {
        Wind wind = WindParser.Parse("270P49MPS")!;

        Assert.Equal(new Speed(49, SpeedUnit.MetersPerSecond, IsAbove: true), wind.Speed);
    }

    [Theory]
    [InlineData("2401KT")]
    [InlineData("24012G5KT")]
    [InlineData("37012KT")]
    [InlineData("24012")]
    [InlineData("24012KTS")]
    [InlineData("ABC12KT")]
    [InlineData("KT")]
    [InlineData("2401234KT")]
    [InlineData("24012X25KT")]
    public void RejectsInvalidGroups(string text) => Assert.Null(WindParser.Parse(text));

    [Fact]
    public void ParsesVariation()
    {
        WindVariation variation = WindParser.ParseVariation("200V280")!.Value;

        Assert.Equal(200, variation.From);
        Assert.Equal(280, variation.To);
    }

    [Theory]
    [InlineData("200V28")]
    [InlineData("400V280")]
    [InlineData("200X280")]
    [InlineData("2O0V280")]
    public void RejectsInvalidVariation(string text) => Assert.Null(WindParser.ParseVariation(text));

    [Fact]
    public void ConvertsToKnots()
    {
        Assert.Equal(19.44, new Speed(10, SpeedUnit.MetersPerSecond).ToKnots(), 2);
        Assert.Equal(10.8, new Speed(20, SpeedUnit.KilometersPerHour).ToKnots(), 1);
        Assert.Equal(12, new Speed(12, SpeedUnit.Knots).ToKnots());
    }
}
