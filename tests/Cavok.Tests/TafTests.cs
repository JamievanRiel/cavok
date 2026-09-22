namespace Cavok.Tests;

public class TafTests
{
    [Fact]
    public void ParsesATypicalEuropeanForecast()
    {
        Taf taf = Taf.Parse("TAF EHAM 210440Z 2106/2212 27005KT 9999 FEW035 BECMG 2106/2109 31011KT PROB30 TEMPO 2114/2118 TSRA");

        Assert.Empty(taf.Diagnostics);
        Assert.Equal("EHAM", taf.Station);
        Assert.Equal(new DayTime(21, 4, 40), taf.IssueTime);
        Assert.Equal(new ValidityPeriod(new DayHour(21, 6), new DayHour(22, 12)), taf.Validity);
        Assert.Equal(270, taf.Base.Wind!.Direction);
        Assert.True(taf.Base.Visibility!.IsTenKmOrMore);
        Assert.Equal(3500, Assert.Single(taf.Base.Clouds).HeightFeet);
        Assert.Equal(FlightCategory.Vfr, taf.Base.FlightCategory);

        Assert.Equal(2, taf.Changes.Count);
        TafChange becoming = taf.Changes[0];
        Assert.Equal(TafChangeKind.Becoming, becoming.Kind);
        Assert.Equal(new ValidityPeriod(new DayHour(21, 6), new DayHour(21, 9)), becoming.Period);
        Assert.Equal(310, becoming.Conditions.Wind!.Direction);
        Assert.Null(becoming.Conditions.FlightCategory);

        TafChange tempo = taf.Changes[1];
        Assert.Equal(TafChangeKind.Temporary, tempo.Kind);
        Assert.Equal(30, tempo.Probability);
        Assert.Equal(new ValidityPeriod(new DayHour(21, 14), new DayHour(21, 18)), tempo.Period);
        WeatherPhenomenon weather = Assert.Single(tempo.Conditions.Weather);
        Assert.Equal(WeatherDescriptor.Thunderstorm, weather.Descriptor);
        Assert.Equal(new[] { WeatherType.Rain }, weather.Types);
    }

    [Fact]
    public void ProbabilityWithoutTempo()
    {
        Taf taf = Taf.Parse("TAF AMD EFKI 210745Z 2107/2115 27005KT 9999 FEW007 PROB30 2107/2109 BKN009");

        Assert.Empty(taf.Diagnostics);
        Assert.True(taf.IsAmended);
        TafChange change = Assert.Single(taf.Changes);
        Assert.Equal(TafChangeKind.Probability, change.Kind);
        Assert.Equal(30, change.Probability);
        Assert.Equal(FlightCategory.Ifr, change.Conditions.FlightCategory);
    }

    [Fact]
    public void FromGroups()
    {
        Taf taf = Taf.Parse("TAF KCON 210808Z 2108/2206 35002KT P6SM BKN015 TEMPO 2108/2109 BKN004 FM211100 35002KT P6SM VCFG SCT002 BKN050 FM211400 35007KT P6SM SCT050");

        Assert.Empty(taf.Diagnostics);
        Assert.Equal(3, taf.Changes.Count);
        Assert.Null(taf.Changes[0].Probability);
        Assert.Equal(TafChangeKind.From, taf.Changes[1].Kind);
        Assert.Equal(new DayTime(21, 11, 0), taf.Changes[1].From);
        Assert.True(taf.Changes[1].Conditions.Visibility!.IsMoreThan);
        Assert.Equal(WeatherIntensity.InVicinity, Assert.Single(taf.Changes[1].Conditions.Weather).Intensity);
        Assert.Equal(new DayTime(21, 14, 0), taf.Changes[2].From);
    }

    [Fact]
    public void TemperatureForecastsBetweenGroups()
    {
        Taf taf = Taf.Parse("TAF BIKF 210750Z 2109/2209 11025G40KT 6000 -RADZ BKN014 OVC022 TX12/2113Z TN08/2202Z TEMPO 2109/2111 2000 DZRA BR BKN005 OVC012 BECMG 2110/2112 19018KT 9999 -SHRA BKN020 TEMPO 2112/2209 19022G32KT 4000 SHRA BKN012CB");

        Assert.Empty(taf.Diagnostics);
        Assert.Equal(
            new[]
            {
                new TemperatureForecast(TemperatureKind.Maximum, 12, new DayHour(21, 13)),
                new TemperatureForecast(TemperatureKind.Minimum, 8, new DayHour(22, 2)),
            },
            taf.Temperatures);
        Assert.Equal(3, taf.Changes.Count);
    }

    [Fact]
    public void UkMilitaryForecastQnh()
    {
        Taf taf = Taf.Parse("TAF EGUN 210800Z 2108/2214 24008KT 9999 SCT200 QNH3043INS BECMG 2116/2117 VRB06KT 9999 SCT080 QNH3042INS TX19/2114Z TN10/2204Z");

        Assert.Empty(taf.Diagnostics);
        Assert.Equal(30.43, taf.Base.Pressure!.Value.Value, 2);
        Assert.Equal(30.42, taf.Changes[0].Conditions.Pressure!.Value.Value, 2);
        Assert.Equal(2, taf.Temperatures.Count);
    }

    [Fact]
    public void MilitaryIcingAndTurbulenceGroups()
    {
        Taf taf = Taf.Parse("TAF AMD EGUL 181807Z 1818/1919 23010G15KT 8000 -RA BKN030 OVC080 651109 QNH2988INS BECMG 1820/1821 23012G25KT 8000 -RA BKN025 OVC050 651109 510005 QNH2988INS");

        Assert.Empty(taf.Diagnostics);
        Assert.Equal(new IcingLayer(5, 11000, 9000), Assert.Single(taf.Base.Icing));
        Assert.Empty(taf.Base.Turbulence);
        Assert.Equal(29.88, taf.Base.Pressure!.Value.Value, 2);
        ForecastConditions becoming = Assert.Single(taf.Changes).Conditions;
        Assert.Equal(new IcingLayer(5, 11000, 9000), Assert.Single(becoming.Icing));
        Assert.Equal(new TurbulenceLayer(1, 0, 5000), Assert.Single(becoming.Turbulence));
        Assert.Equal(29.88, becoming.Pressure!.Value.Value, 2);
    }

    [Fact]
    public void CorrectedForecastWithCavok()
    {
        Taf taf = Taf.Parse("TAF COR LRTC 210750Z 2106/2115 VRB04KT CAVOK BECMG 2109/2111 27010KT SCT040CB TEMPO 2111/2115 VRB15G25KT 5000 TSRA");

        Assert.Empty(taf.Diagnostics);
        Assert.True(taf.IsCorrected);
        Assert.True(taf.Base.IsCavok);
        Assert.Equal(2, taf.Changes.Count);
    }

    [Fact]
    public void NilForecast()
    {
        Taf taf = Taf.Parse("TAF EHAM 210440Z NIL");

        Assert.Empty(taf.Diagnostics);
        Assert.True(taf.IsNil);
        Assert.Null(taf.Validity);
    }

    [Fact]
    public void CancelledForecast()
    {
        Taf taf = Taf.Parse("TAF AMD EHAM 210600Z 2106/2212 CNL");

        Assert.Empty(taf.Diagnostics);
        Assert.True(taf.IsAmended);
        Assert.True(taf.IsCancelled);
    }

    [Fact]
    public void MultiLineReportWithEndMarker()
    {
        Taf taf = Taf.Parse("TAF EHAM 210440Z 2106/2212 27005KT 9999 FEW035\n  BECMG 2106/2109 31011KT=");

        Assert.Empty(taf.Diagnostics);
        Assert.Single(taf.Changes);
    }

    [Fact]
    public void Remarks()
    {
        Taf taf = Taf.Parse("TAF ESDF 210530Z 2106/2115 28007KT CAVOK BECMG 2108/2111 31010G20KT RMK MIL");

        Assert.Empty(taf.Diagnostics);
        Assert.Equal("MIL", taf.Remarks);
    }

    [Fact]
    public void UsMilitaryAmendmentStatementIsKeptAsRemarks()
    {
        Taf taf = Taf.Parse("TAF ETAD 201945Z 2020/2202 30006KT 9999 FEW200 QNH3036INS BECMG 2103/2104 24003KT 9999 BKN120 OVC200 QNH3041INS BECMG 2108/2109 24006KT 9999 SCT200 QNH3043INS TX20/2114Z TN04/2104Z LAST NO AMDS AFT 2020 NEXT 2104");

        Assert.Empty(taf.Diagnostics);
        Assert.Equal("LAST NO AMDS AFT 2020 NEXT 2104", taf.Remarks);
        Assert.Null(taf.Changes[1].Conditions.Visibility!.Minimum);
        Assert.Equal(2, taf.Temperatures.Count);
    }

    [Fact]
    public void ProbabilityTempoWithFog()
    {
        Taf taf = Taf.Parse("TAF AMD LFRB 210804Z 2108/2212 VRB05KT 9999 BKN007 BKN012 TEMPO 2108/2109 BKN003 BECMG 2110/2112 SCT020 BKN025 BECMG 2114/2116 02010KT TEMPO 2117/2208 2000 BR BKN002 PROB40 TEMPO 2204/2207 0700 FG VV///");

        Assert.Empty(taf.Diagnostics);
        TafChange last = taf.Changes[taf.Changes.Count - 1];
        Assert.Equal(TafChangeKind.Temporary, last.Kind);
        Assert.Equal(40, last.Probability);
        Assert.Equal(FlightCategory.Lifr, last.Conditions.FlightCategory);
    }
}
