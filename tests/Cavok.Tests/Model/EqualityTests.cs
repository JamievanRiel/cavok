namespace Cavok.Tests.Model;

public class EqualityTests
{
    private const string MetarReport =
        "METAR LUKK 211330Z 33020G31KT 9999 -SHRA SCT013 BKN017CB OVC024 15/13 Q1013 TEMPO 33020G30KT 3000 SHRA BKN012 SCT035CB";

    private const string TafReport =
        "TAF EDHK 200800Z 2009/2018 24010KT 9999 SCT030 BECMG 2009/2012 27015G30KT TEMPO 2009/2012 4000 SHRA BKN014TCU "
        + "TEMPO 2012/2016 27020G35KT 4000 SHRA BKN014CB PROB30 TEMPO 2012/2016 2500 TSRA BECMG 2016/2018 29011KT "
        + "TEMPO 2016/2018 4000 SHRA BKN014CB";

    [Fact]
    public void ParsingAMetarTwiceGivesEqualRecords()
    {
        Metar first = Metar.Parse(MetarReport);
        Metar second = Metar.Parse(MetarReport);

        Assert.True(first == second);
        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.Equal(first.Trends[0], second.Trends[0]);
    }

    [Fact]
    public void ParsingATafTwiceGivesEqualRecords()
    {
        Taf first = Taf.Parse(TafReport);
        Taf second = Taf.Parse(TafReport);

        Assert.True(first == second);
        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.Equal(first.Changes[3].Conditions, second.Changes[3].Conditions);
    }

    [Fact]
    public void DifferentReportsAreNotEqual()
    {
        Metar metar = Metar.Parse(MetarReport);

        Assert.NotEqual(metar, Metar.Parse(MetarReport.Replace("-SHRA", "+SHRA")));
        Assert.NotEqual(metar, Metar.Parse(MetarReport.Replace("TEMPO 33020G30KT 3000 SHRA", "TEMPO 33020G30KT 3000 TSRA")));
    }

    [Fact]
    public void ParsedListsContainValuesBuiltByHand()
    {
        Metar metar = Metar.Parse(MetarReport);
        var lightShowers = new WeatherPhenomenon
        {
            Intensity = WeatherIntensity.Light,
            Descriptor = WeatherDescriptor.Showers,
            Types = new[] { WeatherType.Rain },
        };

        bool found = metar.Weather.Contains(lightShowers);

        Assert.True(found);
        Assert.Contains(lightShowers, metar.Weather);
        Assert.Contains(lightShowers, new HashSet<WeatherPhenomenon>(metar.Weather));
        Assert.Equal(new CloudLayer { Cover = CloudCover.Broken, HeightFeet = 1700, Type = CloudType.Cumulonimbus }, metar.Clouds[1]);
    }

    [Fact]
    public void ARecordBuiltByHandEqualsTheParsedOne()
    {
        var expected = new WeatherPhenomenon { Descriptor = WeatherDescriptor.Thunderstorm, Types = new List<WeatherType> { WeatherType.Rain } };
        Taf taf = Taf.Parse(TafReport);

        Assert.Equal(expected, taf.Changes[3].Conditions.Weather[0]);
        Assert.Equal(expected.GetHashCode(), taf.Changes[3].Conditions.Weather[0].GetHashCode());
    }

    [Fact]
    public void EmptyListsAreEqualHoweverTheyWereGiven()
    {
        Assert.Equal(new ForecastConditions(), new ForecastConditions { Weather = Array.Empty<WeatherPhenomenon>(), Clouds = new List<CloudLayer>() });
        Assert.Equal(new Metar(), new Metar { Diagnostics = Array.Empty<Diagnostic>() });
        Assert.Equal(new Taf(), new Taf { Changes = new List<TafChange>() });
    }

    [Fact]
    public void ListsCannotBeChangedByDowncasting()
    {
        Metar metar = Metar.Parse(MetarReport);

        Assert.False(metar.Weather is WeatherPhenomenon[]);
        Assert.False(metar.Weather is IList<WeatherPhenomenon>);
        Assert.False(metar.Clouds is ICollection<CloudLayer>);
        Assert.False(metar.Trends is IList<Trend>);
        Assert.False(metar.Weather[0].Types is IList<WeatherType>);
        Assert.False(metar.Diagnostics is IList<Diagnostic>);
    }

    [Fact]
    public void ListsGivenByTheCallerAreCopied()
    {
        var types = new[] { WeatherType.Rain };
        var phenomenon = new WeatherPhenomenon { Types = types };

        types[0] = WeatherType.Snow;

        Assert.Equal(WeatherType.Rain, phenomenon.Types[0]);
    }

    [Fact]
    public void ToStringShowsTheListItems()
    {
        var phenomenon = new WeatherPhenomenon { Intensity = WeatherIntensity.Light, Types = new[] { WeatherType.Rain, WeatherType.Snow } };

        Assert.Contains("Types = [Rain, Snow]", phenomenon.ToString(), StringComparison.Ordinal);
    }
}
