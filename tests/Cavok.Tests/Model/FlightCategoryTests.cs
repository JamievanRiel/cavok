using System.Globalization;
using Cavok.Parsing;

namespace Cavok.Tests.Model;

public class FlightCategoryTests
{
    [Theory]
    [InlineData(9999, 3500, FlightCategory.Vfr)]
    [InlineData(9999, 3000, FlightCategory.Mvfr)]
    [InlineData(9999, 1000, FlightCategory.Mvfr)]
    [InlineData(9999, 900, FlightCategory.Ifr)]
    [InlineData(9999, 500, FlightCategory.Ifr)]
    [InlineData(9999, 400, FlightCategory.Lifr)]
    [InlineData(8000, 5000, FlightCategory.Mvfr)] // 8 km is just under 5 SM (8047 m)
    [InlineData(9000, 5000, FlightCategory.Vfr)]
    [InlineData(4800, 5000, FlightCategory.Ifr)] // below 3 SM (4828 m)
    [InlineData(1700, 5000, FlightCategory.Ifr)]
    [InlineData(1609, 5000, FlightCategory.Lifr)] // below 1 SM (1609.344 m)
    [InlineData(5000, 800, FlightCategory.Ifr)] // the worse of MVFR (visibility) and IFR (ceiling)
    public void UsesTheWorseOfVisibilityAndCeiling(int visibilityMeters, int brokenBaseFeet, FlightCategory expected)
    {
        Visibility visibility = VisibilityParser.ParseMetric(visibilityMeters.ToString("0000", CultureInfo.InvariantCulture))!;
        CloudLayer[] clouds = { new CloudLayer { Cover = CloudCover.Broken, HeightFeet = brokenBaseFeet } };

        Assert.Equal(expected, FlightCategoryCalculator.Compute(visibility, false, clouds, null));
    }

    [Fact]
    public void CavokIsVfr() => Assert.Equal(FlightCategory.Vfr, FlightCategoryCalculator.Compute(null, true, Array.Empty<CloudLayer>(), null));

    [Fact]
    public void FewAndScatteredLayersAreNotACeiling()
    {
        CloudLayer[] clouds =
        {
            new CloudLayer { Cover = CloudCover.Few, HeightFeet = 300 },
            new CloudLayer { Cover = CloudCover.Scattered, HeightFeet = 800 },
        };

        Assert.Equal(FlightCategory.Vfr, FlightCategoryCalculator.Compute(VisibilityParser.ParseMetric("9999"), false, clouds, null));
    }

    [Fact]
    public void VerticalVisibilityIsACeiling()
    {
        CloudLayer[] clouds = { new CloudLayer { Cover = CloudCover.VerticalVisibility, HeightFeet = 100 } };

        Assert.Equal(FlightCategory.Lifr, FlightCategoryCalculator.Compute(null, false, clouds, null));
    }

    [Fact]
    public void CeilingWithUnknownHeightIsIgnored()
    {
        CloudLayer[] clouds = { new CloudLayer { Cover = CloudCover.VerticalVisibility } };

        Assert.Equal(FlightCategory.Lifr, FlightCategoryCalculator.Compute(VisibilityParser.ParseMetric("0800"), false, clouds, null));
    }

    [Theory]
    [InlineData("P6SM", FlightCategory.Vfr)]
    [InlineData("2SM", FlightCategory.Ifr)]
    [InlineData("M1/4SM", FlightCategory.Lifr)]
    public void StatuteMiles(string text, FlightCategory expected) =>
        Assert.Equal(expected, FlightCategoryCalculator.Compute(VisibilityParser.ParseStatuteMiles(text), false, Array.Empty<CloudLayer>(), CloudCondition.Clear));

    [Fact]
    public void UnknownWhenNeitherVisibilityNorCloudIsKnown() =>
        Assert.Null(FlightCategoryCalculator.Compute(VisibilityParser.ParseMetric("////"), false, Array.Empty<CloudLayer>(), null));

    [Fact]
    public void CloudConditionAloneMeansNoCeiling() =>
        Assert.Equal(FlightCategory.Vfr, FlightCategoryCalculator.Compute(null, false, Array.Empty<CloudLayer>(), CloudCondition.NoCloudDetected));
}
