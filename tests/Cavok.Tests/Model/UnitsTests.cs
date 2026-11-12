namespace Cavok.Tests.Model;

public class UnitsTests
{
    [Theory]
    [InlineData(10, SpeedUnit.Knots, 5.14444, 18.52)]
    [InlineData(10, SpeedUnit.MetersPerSecond, 10.0, 36.0)]
    [InlineData(36, SpeedUnit.KilometersPerHour, 10.0, 36.0)]
    public void SpeedConvertsToMetricUnits(int value, SpeedUnit unit, double expectedMps, double expectedKmh)
    {
        Speed speed = new(value, unit);

        Assert.Equal(expectedMps, speed.ToMetersPerSecond(), 5);
        Assert.Equal(expectedKmh, speed.ToKilometersPerHour(), 5);
    }

    [Theory]
    [InlineData(10, SpeedUnit.Knots, 10.0)]
    [InlineData(10, SpeedUnit.MetersPerSecond, 19.43844)]
    [InlineData(36, SpeedUnit.KilometersPerHour, 19.43844)]
    public void SpeedStillConvertsToKnots(int value, SpeedUnit unit, double expectedKnots)
    {
        Assert.Equal(expectedKnots, new Speed(value, unit).ToKnots(), 5);
    }

    [Fact]
    public void CloudHeightConvertsToMeters()
    {
        CloudLayer layer = new() { Cover = CloudCover.Broken, HeightFeet = 1000 };

        Assert.Equal(304.8, layer.HeightMeters!.Value, 5);
    }

    [Fact]
    public void CloudHeightMetersIsNullWhenNotObservable() =>
        Assert.Null(new CloudLayer { Cover = CloudCover.Broken }.HeightMeters);

    [Fact]
    public void RvrInMetersStaysInMeters()
    {
        RunwayVisualRange rvr = new() { Runway = "24", Value = 800, VariableMax = 1200 };

        Assert.Equal(800, rvr.ValueMeters);
        Assert.Equal(1200, rvr.VariableMaxMeters);
    }

    [Fact]
    public void RvrInFeetConvertsToMeters()
    {
        RunwayVisualRange rvr = new() { Runway = "24", Value = 2000, VariableMax = 6000, IsFeet = true };

        Assert.Equal(609.6, rvr.ValueMeters!.Value, 5);
        Assert.Equal(1828.8, rvr.VariableMaxMeters!.Value, 5);
    }

    [Fact]
    public void RvrMetersIsNullWhenMissing() =>
        Assert.Null(new RunwayVisualRange { Runway = "24", IsMissing = true }.ValueMeters);

    [Fact]
    public void IcingLayerConvertsToMeters()
    {
        IcingLayer layer = new(Type: 5, BaseFeet: 11000, ThicknessFeet: 9000);

        Assert.Equal(3352.8, layer.BaseMeters, 5);
        Assert.Equal(2743.2, layer.ThicknessMeters, 5);
    }

    [Fact]
    public void TurbulenceLayerConvertsToMeters()
    {
        TurbulenceLayer layer = new(Type: 2, BaseFeet: 0, ThicknessFeet: 2000);

        Assert.Equal(0, layer.BaseMeters);
        Assert.Equal(609.6, layer.ThicknessMeters, 5);
    }

    [Fact]
    public void WaveHeightConvertsToMeters()
    {
        SeaCondition sea = new() { SeaTemperature = 18, WaveHeightDecimeters = 14 };

        Assert.Equal(1.4, sea.WaveHeightMeters!.Value, 5);
    }

    [Fact]
    public void WaveHeightMetersIsNullWhenMissing() =>
        Assert.Null(new SeaCondition { SeaTemperature = 15 }.WaveHeightMeters);
}
