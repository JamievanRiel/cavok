using Cavok.Parsing;
using Cavok.Text;

namespace Cavok.Tests.Text;

public class EnglishDescriptionTests
{
    [Fact]
    public void DescribesATypicalMetar()
    {
        Metar metar = Metar.Parse("METAR EHAM 211125Z 24012G25KT 200V280 9999 -SHRA FEW012 BKN030CB 12/09 Q1013 TEMPO 4000 SHRA");

        string expected = Lines(
            "METAR EHAM, observed day 21 at 11:25 UTC",
            L("Wind", "240° at 12 kt, gusting 25 kt, varying between 200° and 280°"),
            L("Visibility", "10 km or more"),
            L("Weather", "light rain showers"),
            L("Clouds", "few (1–2/8) at 1,200 ft, broken (5–7/8) at 3,000 ft with cumulonimbus"),
            L("Temperature", "12 °C, dew point 9 °C"),
            L("QNH", "1013 hPa"),
            L("Category", "MVFR"),
            L("Trend", "temporarily: visibility 4,000 m; rain showers"));
        Assert.Equal(expected, metar.Describe(Language.English));
        Assert.Equal(expected, metar.Describe());
    }

    [Fact]
    public void CavokAndNosig()
    {
        Metar metar = Metar.Parse("METAR EDDF 210750Z AUTO 25006KT CAVOK 14/09 Q1031 NOSIG");

        Assert.Equal(
            Lines(
                "METAR EDDF, observed day 21 at 07:50 UTC (automated)",
                L("Wind", "250° at 6 kt"),
                L("Visibility", "CAVOK: 10 km or more, no cloud below 5,000 ft and no significant weather"),
                L("Temperature", "14 °C, dew point 9 °C"),
                L("QNH", "1031 hPa"),
                L("Category", "VFR"),
                L("Trend", "no significant change")),
            metar.Describe());
    }

    [Fact]
    public void MissingValuesAreShownAsNotAvailable()
    {
        Metar metar = Metar.Parse("METAR LIBQ 210755Z AUTO /////KT //// // ///////// 15/10 Q1027");

        Assert.Equal(
            Lines(
                "METAR LIBQ, observed day 21 at 07:55 UTC (automated)",
                L("Wind", "not available"),
                L("Visibility", "not available"),
                L("Weather", "not observable"),
                L("Clouds", "not available"),
                L("Temperature", "15 °C, dew point 10 °C"),
                L("QNH", "1027 hPa")),
            metar.Describe());
    }

    [Fact]
    public void UnitedStatesUnits()
    {
        Metar metar = Metar.Parse("SPECI KRUT 210809Z AUTO 00000KT 2 1/2SM BR OVC002 11/11 A3009 RMK AO2");

        Assert.Equal(
            Lines(
                "SPECI KRUT, observed day 21 at 08:09 UTC (automated)",
                L("Wind", "calm"),
                L("Visibility", "2.5 SM"),
                L("Weather", "mist"),
                L("Clouds", "overcast (8/8) at 200 ft"),
                L("Temperature", "11 °C, dew point 11 °C"),
                L("QNH", "30.09 inHg"),
                L("Category", "LIFR"),
                L("Remarks", "AO2")),
            metar.Describe());
    }

    [Fact]
    public void RunwayVisualRangeAndVerticalVisibility()
    {
        Metar metar = Metar.Parse("METAR EGLL 210550Z 00000KT 0300 R27L/0550V0800U R27R/P1500N FG VV001 09/09 Q1020");

        Assert.Equal(
            Lines(
                "METAR EGLL, observed day 21 at 05:50 UTC",
                L("Wind", "calm"),
                L("Visibility", "300 m"),
                L("RVR", "runway 27L: between 550 m and 800 m, increasing"),
                L("RVR", "runway 27R: more than 1,500 m, no change"),
                L("Weather", "fog"),
                L("Clouds", "sky obscured, vertical visibility 100 ft"),
                L("Temperature", "9 °C, dew point 9 °C"),
                L("QNH", "1020 hPa"),
                L("Category", "LIFR")),
            metar.Describe());
    }

    [Fact]
    public void SupplementaryInformation()
    {
        Metar metar = Metar.Parse("METAR ULLI 211130Z 36005MPS 9999 -SN OVC010 M02/M04 Q1003 RESN WS R28L R28L/590160 NOSIG");

        Assert.Equal(
            Lines(
                "METAR ULLI, observed day 21 at 11:30 UTC",
                L("Wind", "360° at 5 m/s"),
                L("Visibility", "10 km or more"),
                L("Weather", "light snow"),
                L("Clouds", "overcast (8/8) at 1,000 ft"),
                L("Temperature", "-2 °C, dew point -4 °C"),
                L("QNH", "1003 hPa"),
                L("Recent", "snow"),
                L("Wind shear", "runway 28L"),
                L("Runway state", "runway 28L: wet snow, covering 51–100%, depth 1 mm, friction coefficient 0.60"),
                L("Category", "MVFR"),
                L("Trend", "no significant change")),
            metar.Describe());
    }

    [Fact]
    public void MilitaryColorStates()
    {
        Metar metar = Metar.Parse("METAR ETMN 210720Z 30019KT 9999 VCSH SCT021 SCT040 BKN320 14/10 Q1025 BLU+BLU+ TEMPO WHT");

        Assert.Equal(
            Lines(
                "METAR ETMN, observed day 21 at 07:20 UTC",
                L("Wind", "300° at 19 kt"),
                L("Visibility", "10 km or more"),
                L("Weather", "showers in the vicinity"),
                L("Clouds", "scattered (3–4/8) at 2,100 ft, scattered (3–4/8) at 4,000 ft, broken (5–7/8) at 32,000 ft"),
                L("Temperature", "14 °C, dew point 10 °C"),
                L("QNH", "1025 hPa"),
                L("Color state", "blue plus, blue plus"),
                L("Category", "VFR"),
                L("Trend", "temporarily: color state white")),
            metar.Describe());
    }

    [Fact]
    public void DescribesATaf()
    {
        Taf taf = Taf.Parse("TAF EHAM 210440Z 2106/2212 27005KT 9999 FEW035 BECMG 2106/2109 31011KT PROB30 TEMPO 2114/2118 TSRA TX19/2114Z");

        Assert.Equal(
            Lines(
                "TAF EHAM, issued day 21 at 04:40 UTC, valid from day 21 06:00 to day 22 12:00 UTC",
                L("Wind", "270° at 5 kt"),
                L("Visibility", "10 km or more"),
                L("Clouds", "few (1–2/8) at 3,500 ft"),
                L("Category", "VFR"),
                L("Max temp", "19 °C on day 21 at 14:00 UTC"),
                "Becoming between day 21 06:00 and day 21 09:00 UTC:",
                "  " + L("Wind", "310° at 11 kt"),
                "30% probability, temporarily between day 21 14:00 and day 21 18:00 UTC:",
                "  " + L("Weather", "thunderstorm with rain")),
            taf.Describe());
    }

    [Fact]
    public void DescribesFromGroups()
    {
        Taf taf = Taf.Parse("TAF KCON 210808Z 2108/2206 35002KT P6SM BKN015 FM211100 35002KT P6SM VCFG SCT002 BKN050");

        Assert.Equal(
            Lines(
                "TAF KCON, issued day 21 at 08:08 UTC, valid from day 21 08:00 to day 22 06:00 UTC",
                L("Wind", "350° at 2 kt"),
                L("Visibility", "more than 6 SM"),
                L("Clouds", "broken (5–7/8) at 1,500 ft"),
                L("Category", "MVFR"),
                "From day 21 11:00 UTC:",
                "  " + L("Wind", "350° at 2 kt"),
                "  " + L("Visibility", "more than 6 SM"),
                "  " + L("Weather", "fog in the vicinity"),
                "  " + L("Clouds", "scattered (3–4/8) at 200 ft, broken (5–7/8) at 5,000 ft"),
                "  " + L("Category", "VFR")),
            taf.Describe());
    }

    [Fact]
    public void MilitaryTurbulenceGroup()
    {
        Taf taf = Taf.Parse("TAF LXGB 201325Z 2015/2022 09015KT 9999 FEW020 PROB30 TEMPO 2015/2016 10018G28KT 520002 PROB30 TEMPO 2019/2022 SCT020");

        Assert.Equal(
            Lines(
                "TAF LXGB, issued day 20 at 13:25 UTC, valid from day 20 15:00 to day 20 22:00 UTC",
                L("Wind", "090° at 15 kt"),
                L("Visibility", "10 km or more"),
                L("Clouds", "few (1–2/8) at 2,000 ft"),
                L("Category", "VFR"),
                "30% probability, temporarily between day 20 15:00 and day 20 16:00 UTC:",
                "  " + L("Wind", "100° at 18 kt, gusting 28 kt"),
                "  " + L("Turbulence", "occasional moderate turbulence in clear air up to 2,000 ft"),
                "30% probability, temporarily between day 20 19:00 and day 20 22:00 UTC:",
                "  " + L("Clouds", "scattered (3–4/8) at 2,000 ft"),
                "  " + L("Category", "VFR")),
            taf.Describe());
    }

    [Theory]
    [InlineData("651109", "moderate icing in cloud from 11,000 to 20,000 ft")]
    [InlineData("600003", "trace icing up to 3,000 ft")]
    [InlineData("690500", "severe icing in precipitation from 5,000 ft")]
    [InlineData("510005", "light turbulence up to 5,000 ft")]
    [InlineData("591209", "frequent severe turbulence in cloud from 12,000 to 21,000 ft")]
    public void IcingAndTurbulencePhrases(string code, string expected) =>
        Assert.Equal(expected, IcingTurbulenceParser.ParseIcing(code) is IcingLayer icing
            ? EnglishPhrasebook.Instance.IcingText(icing)
            : EnglishPhrasebook.Instance.TurbulenceText(IcingTurbulenceParser.ParseTurbulence(code)!));

    [Fact]
    public void NilAndCancelledForecasts()
    {
        Assert.Equal(
            Lines("TAF EHAM, issued day 21 at 04:40 UTC", "No report available"),
            Taf.Parse("TAF EHAM 210440Z NIL").Describe());
        Assert.Equal(
            Lines("TAF EHAM, issued day 21 at 06:00 UTC, valid from day 21 06:00 to day 22 12:00 UTC (amended)", "Forecast cancelled"),
            Taf.Parse("TAF AMD EHAM 210600Z 2106/2212 CNL").Describe());
    }

    [Fact]
    public void RejectedProbabilityHasNoPercentage()
    {
        Taf taf = Taf.Parse("TAF EHAM 210440Z 2106/2212 27005KT PROB50 2112/2114 BKN005");

        Assert.Contains("Probability between day 21 12:00 and day 21 14:00 UTC:", taf.Describe());
    }

    [Theory]
    [InlineData("-SHRA", "light rain showers")]
    [InlineData("+TSRA", "heavy thunderstorm with rain")]
    [InlineData("VCSH", "showers in the vicinity")]
    [InlineData("VCTS", "thunderstorm in the vicinity")]
    [InlineData("FZFG", "freezing fog")]
    [InlineData("BCFG", "patches of fog")]
    [InlineData("MIFG", "shallow fog")]
    [InlineData("PRFG", "partial fog")]
    [InlineData("-RADZ", "light rain and drizzle")]
    [InlineData("+FC", "tornado or waterspout")]
    [InlineData("DRSN", "low drifting snow")]
    [InlineData("BLSN", "blowing snow")]
    [InlineData("DU", "widespread dust")]
    [InlineData("BLDU", "blowing dust")]
    [InlineData("DRDU", "low drifting dust")]
    [InlineData("TSRAGS", "thunderstorm with rain and small hail")]
    [InlineData("SHRASNGR", "rain, snow and hail showers")]
    [InlineData("BR", "mist")]
    [InlineData("UP", "unknown precipitation")]
    [InlineData("//", "not observable")]
    public void WeatherPhrases(string code, string expected) =>
        Assert.Equal(expected, EnglishPhrasebook.Instance.WeatherText(WeatherParser.Parse(code)!));

    [Theory]
    [InlineData("VRB03KT", "variable at 3 kt")]
    [InlineData("00000KT", "calm")]
    [InlineData("27015MPS", "270° at 15 m/s")]
    [InlineData("270P49MPS", "270° at more than 49 m/s")]
    [InlineData("05010KT", "050° at 10 kt")]
    [InlineData("///05KT", "unknown direction at 5 kt")]
    [InlineData("/////KT", "not available")]
    [InlineData("24010KMH", "240° at 10 km/h")]
    public void WindPhrases(string code, string expected) =>
        Assert.Equal(expected, EnglishPhrasebook.Instance.WindText(WindParser.Parse(code)!));

    [Theory]
    [InlineData("9999NDV", "10 km or more (no directional variation)")]
    [InlineData("0800", "800 m")]
    [InlineData("M1/4SM", "less than 0.25 SM")]
    [InlineData("1/2SM", "0.5 SM")]
    [InlineData("1/16SM", "0.0625 SM")]
    [InlineData("5/8SM", "0.625 SM")]
    public void VisibilityPhrases(string code, string expected) =>
        Assert.Equal(expected, EnglishPhrasebook.Instance.VisibilityText(
            VisibilityParser.ParseMetric(code) ?? VisibilityParser.ParseStatuteMiles(code)!));

    [Fact]
    public void MinimumVisibilityPhrase()
    {
        var visibility = new Visibility
        {
            Meters = 10000,
            IsTenKmOrMore = true,
            Minimum = new MinimumVisibility(4900, CompassDirection.SouthEast),
        };

        Assert.Equal("10 km or more, minimum 4,900 m to the southeast", EnglishPhrasebook.Instance.VisibilityText(visibility));
    }

    private static string L(string label, string value) => (label + ":").PadRight(14) + value;

    private static string Lines(params string[] lines) => string.Join("\n", lines);
}
