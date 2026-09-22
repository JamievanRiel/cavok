using Cavok.Parsing;
using Cavok.Text;

namespace Cavok.Tests.Text;

public class DutchDescriptionTests
{
    [Fact]
    public void DescribesATypicalMetar()
    {
        Metar metar = Metar.Parse("METAR EHAM 211125Z 24012G25KT 200V280 9999 -SHRA FEW012 BKN030CB 12/09 Q1013 TEMPO 4000 SHRA");

        Assert.Equal(
            Lines(
                "METAR EHAM, waarneming dag 21 om 11:25 UTC",
                L("Wind", "240° met 12 kt, windstoten tot 25 kt, variërend tussen 200° en 280°"),
                L("Zicht", "10 km of meer"),
                L("Weer", "lichte regenbuien"),
                L("Bewolking", "enkele wolken (1–2/8) op 1.200 ft, gebroken (5–7/8) op 3.000 ft met cumulonimbus"),
                L("Temperatuur", "12 °C, dauwpunt 9 °C"),
                L("QNH", "1013 hPa"),
                L("Categorie", "MVFR"),
                L("Trend", "tijdelijk: zicht 4.000 m; regenbuien")),
            metar.Describe(Language.Dutch));
    }

    [Fact]
    public void CavokAndNosig()
    {
        Metar metar = Metar.Parse("METAR EDDF 210750Z AUTO 25006KT CAVOK 14/09 Q1031 NOSIG");

        Assert.Equal(
            Lines(
                "METAR EDDF, waarneming dag 21 om 07:50 UTC (automatisch)",
                L("Wind", "250° met 6 kt"),
                L("Zicht", "CAVOK: 10 km of meer, geen bewolking onder 5.000 ft en geen significant weer"),
                L("Temperatuur", "14 °C, dauwpunt 9 °C"),
                L("QNH", "1031 hPa"),
                L("Categorie", "VFR"),
                L("Trend", "geen significante verandering")),
            metar.Describe(Language.Dutch));
    }

    [Fact]
    public void UnitedStatesUnits()
    {
        Metar metar = Metar.Parse("SPECI KRUT 210809Z AUTO 00000KT 2 1/2SM BR OVC002 11/11 A3009 RMK AO2");

        Assert.Equal(
            Lines(
                "SPECI KRUT, waarneming dag 21 om 08:09 UTC (automatisch)",
                L("Wind", "windstil"),
                L("Zicht", "2,5 SM"),
                L("Weer", "nevel"),
                L("Bewolking", "geheel bewolkt (8/8) op 200 ft"),
                L("Temperatuur", "11 °C, dauwpunt 11 °C"),
                L("QNH", "30,09 inHg"),
                L("Categorie", "LIFR"),
                L("Opmerkingen", "AO2")),
            metar.Describe(Language.Dutch));
    }

    [Fact]
    public void SupplementaryInformation()
    {
        Metar metar = Metar.Parse("METAR ULLI 211130Z 36005MPS 9999 -SN OVC010 M02/M04 Q1003 RESN WS R28L R28L/590160 NOSIG");

        Assert.Equal(
            Lines(
                "METAR ULLI, waarneming dag 21 om 11:30 UTC",
                L("Wind", "360° met 5 m/s"),
                L("Zicht", "10 km of meer"),
                L("Weer", "lichte sneeuw"),
                L("Bewolking", "geheel bewolkt (8/8) op 1.000 ft"),
                L("Temperatuur", "-2 °C, dauwpunt -4 °C"),
                L("QNH", "1003 hPa"),
                L("Recent weer", "sneeuw"),
                L("Windschering", "baan 28L"),
                L("Baantoestand", "baan 28L: natte sneeuw, bedekking 51–100%, diepte 1 mm, wrijvingscoëfficiënt 0,60"),
                L("Categorie", "MVFR"),
                L("Trend", "geen significante verandering")),
            metar.Describe(Language.Dutch));
    }

    [Fact]
    public void RunwayVisualRange()
    {
        Metar metar = Metar.Parse("METAR EGLL 210550Z 00000KT 0300 R27L/0550V0800U R27R/P1500N FG VV001 09/09 Q1020");

        string description = metar.Describe(Language.Dutch);

        Assert.Contains(L("Baanzicht", "baan 27L: tussen 550 m en 800 m, toenemend"), description, StringComparison.Ordinal);
        Assert.Contains(L("Baanzicht", "baan 27R: meer dan 1.500 m, geen verandering"), description, StringComparison.Ordinal);
        Assert.Contains(L("Bewolking", "lucht onzichtbaar, verticaal zicht 100 ft"), description, StringComparison.Ordinal);
    }

    [Fact]
    public void DescribesATaf()
    {
        Taf taf = Taf.Parse("TAF EHAM 210440Z 2106/2212 27005KT 9999 FEW035 BECMG 2106/2109 31011KT PROB30 TEMPO 2114/2118 TSRA TX19/2114Z");

        Assert.Equal(
            Lines(
                "TAF EHAM, uitgegeven dag 21 om 04:40 UTC, geldig van dag 21 06:00 tot dag 22 12:00 UTC",
                L("Wind", "270° met 5 kt"),
                L("Zicht", "10 km of meer"),
                L("Bewolking", "enkele wolken (1–2/8) op 3.500 ft"),
                L("Categorie", "VFR"),
                L("Max. temp.", "19 °C op dag 21 om 14:00 UTC"),
                "Geleidelijk tussen dag 21 06:00 en dag 21 09:00 UTC:",
                "  " + L("Wind", "310° met 11 kt"),
                "30% kans, tijdelijk tussen dag 21 14:00 en dag 21 18:00 UTC:",
                "  " + L("Weer", "onweersbuien met regen")),
            taf.Describe(Language.Dutch));
    }

    [Fact]
    public void FromGroupAndCancelledForecast()
    {
        Assert.Contains(
            "\nVanaf dag 21 11:00 UTC:\n",
            Taf.Parse("TAF KCON 210808Z 2108/2206 35002KT P6SM BKN015 FM211100 35002KT P6SM").Describe(Language.Dutch),
            StringComparison.Ordinal);
        Assert.Equal(
            Lines("TAF EHAM, uitgegeven dag 21 om 06:00 UTC, geldig van dag 21 06:00 tot dag 22 12:00 UTC (gewijzigd)", "Verwachting geannuleerd"),
            Taf.Parse("TAF AMD EHAM 210600Z 2106/2212 CNL").Describe(Language.Dutch));
    }

    [Fact]
    public void MilitaryTurbulenceGroup()
    {
        Taf taf = Taf.Parse("TAF LXGB 201325Z 2015/2022 09015KT 9999 FEW020 PROB30 TEMPO 2015/2016 10018G28KT 520002 PROB30 TEMPO 2019/2022 SCT020");

        Assert.Equal(
            Lines(
                "TAF LXGB, uitgegeven dag 20 om 13:25 UTC, geldig van dag 20 15:00 tot dag 20 22:00 UTC",
                L("Wind", "090° met 15 kt"),
                L("Zicht", "10 km of meer"),
                L("Bewolking", "enkele wolken (1–2/8) op 2.000 ft"),
                L("Categorie", "VFR"),
                "30% kans, tijdelijk tussen dag 20 15:00 en dag 20 16:00 UTC:",
                "  " + L("Wind", "100° met 18 kt, windstoten tot 28 kt"),
                "  " + L("Turbulentie", "af en toe matige turbulentie in heldere lucht tot 2.000 ft"),
                "30% kans, tijdelijk tussen dag 20 19:00 en dag 20 22:00 UTC:",
                "  " + L("Bewolking", "verspreid (3–4/8) op 2.000 ft"),
                "  " + L("Categorie", "VFR")),
            taf.Describe(Language.Dutch));
    }

    [Theory]
    [InlineData("651109", "matige ijsafzetting in wolken van 11.000 tot 20.000 ft")]
    [InlineData("600003", "sporen van ijsafzetting tot 3.000 ft")]
    [InlineData("690500", "zware ijsafzetting in neerslag vanaf 5.000 ft tot de wolkentoppen")]
    [InlineData("620000", "lichte ijsafzetting in wolken vanaf het oppervlak tot de wolkentoppen")]
    [InlineData("510005", "lichte turbulentie tot 5.000 ft")]
    [InlineData("591209", "vaak zware turbulentie in wolken van 12.000 tot 21.000 ft")]
    public void IcingAndTurbulencePhrases(string code, string expected) =>
        Assert.Equal(expected, IcingTurbulenceParser.ParseIcing(code) is IcingLayer icing
            ? DutchPhrasebook.Instance.IcingText(icing)
            : DutchPhrasebook.Instance.TurbulenceText(IcingTurbulenceParser.ParseTurbulence(code)!));

    [Fact]
    public void RejectedProbabilityHasNoPercentage()
    {
        Taf taf = Taf.Parse("TAF EHAM 210440Z 2106/2212 27005KT PROB50 2112/2114 BKN005");

        Assert.Equal(
            Lines(
                "TAF EHAM, uitgegeven dag 21 om 04:40 UTC, geldig van dag 21 06:00 tot dag 22 12:00 UTC",
                L("Wind", "270° met 5 kt"),
                "Kans tussen dag 21 12:00 en dag 21 14:00 UTC:",
                "  " + L("Bewolking", "gebroken (5–7/8) op 500 ft"),
                "  " + L("Categorie", "IFR")),
            taf.Describe(Language.Dutch));
    }

    [Theory]
    [InlineData("-SHRA", "lichte regenbuien")]
    [InlineData("+TSRA", "zware onweersbuien met regen")]
    [InlineData("-TS", "licht onweer")]
    [InlineData("FZDZ", "onderkoelde motregen")]
    [InlineData("-FZRA", "lichte onderkoelde regen")]
    [InlineData("FZFG", "aanvriezende mist")]
    [InlineData("VCFG", "mist in de omgeving")]
    [InlineData("VCSH", "buien in de omgeving")]
    [InlineData("VCTS", "onweer in de omgeving")]
    [InlineData("BLSN", "hoog opwaaiende sneeuw")]
    [InlineData("DRSN", "laag opwaaiende sneeuw")]
    [InlineData("DRSA", "laag opwaaiend zand")]
    [InlineData("MIFG", "ondiepe mist")]
    [InlineData("PRFG", "gedeeltelijke mist")]
    [InlineData("BCFG", "mistbanken")]
    [InlineData("SHGS", "buien met korrelhagel")]
    [InlineData("+SHSN", "zware sneeuwbuien")]
    [InlineData("SHRASN", "buien met regen en sneeuw")]
    [InlineData("-RADZ", "lichte regen en motregen")]
    [InlineData("+FC", "tornado of waterhoos")]
    [InlineData("+DS", "zware stofstorm")]
    [InlineData("HZ", "heiigheid")]
    [InlineData("BR", "nevel")]
    [InlineData("//", "niet waarneembaar")]
    public void WeatherPhrases(string code, string expected) =>
        Assert.Equal(expected, DutchPhrasebook.Instance.WeatherText(WeatherParser.Parse(code)!));

    [Theory]
    [InlineData("VRB03KT", "variabel met 3 kt")]
    [InlineData("00000KT", "windstil")]
    [InlineData("270P49MPS", "270° met meer dan 49 m/s")]
    [InlineData("///05KT", "onbekende richting met 5 kt")]
    [InlineData("/////KT", "niet beschikbaar")]
    public void WindPhrases(string code, string expected) =>
        Assert.Equal(expected, DutchPhrasebook.Instance.WindText(WindParser.Parse(code)!));

    [Theory]
    [InlineData("M1/4SM", "minder dan 0,25 SM")]
    [InlineData("1/16SM", "0,0625 SM")]
    [InlineData("5/8SM", "0,625 SM")]
    public void StatuteMileVisibilityPhrases(string code, string expected) =>
        Assert.Equal(expected, DutchPhrasebook.Instance.VisibilityText(VisibilityParser.ParseStatuteMiles(code)!));

    [Theory]
    [InlineData("W18/H14", "zeewatertemperatuur 18 °C, golfhoogte 1,4 m")]
    [InlineData("W///H25", "zeewatertemperatuur niet beschikbaar, golfhoogte 2,5 m")]
    [InlineData("W///S5", "zeewatertemperatuur niet beschikbaar, zeegang 5")]
    [InlineData("W///H///", "niet beschikbaar")]
    public void SeaPhrases(string code, string expected) =>
        Assert.Equal(expected, DutchPhrasebook.Instance.SeaText(SeaParser.Parse(code)!));

    [Fact]
    public void MinimumVisibilityPhrase()
    {
        var visibility = new Visibility
        {
            Meters = 10000,
            IsTenKmOrMore = true,
            Minimum = new MinimumVisibility(4900, CompassDirection.SouthEast),
        };

        Assert.Equal("10 km of meer, minimaal 4.900 m in het zuidoosten", DutchPhrasebook.Instance.VisibilityText(visibility));
    }

    private static string L(string label, string value) => (label + ":").PadRight(14) + value;

    private static string Lines(params string[] lines) => string.Join("\n", lines);
}
