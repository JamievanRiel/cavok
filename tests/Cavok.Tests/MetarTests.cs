namespace Cavok.Tests;

public class MetarTests
{
    [Fact]
    public void ParsesATypicalEuropeanReport()
    {
        Metar metar = Metar.Parse("METAR EHAM 211125Z 24012G25KT 200V280 9999 -SHRA FEW012 BKN030CB 12/09 Q1013 TEMPO 4000 SHRA");

        Assert.Empty(metar.Diagnostics);
        Assert.Equal(ReportType.Metar, metar.Type);
        Assert.Equal("EHAM", metar.Station);
        Assert.Equal(new DayTime(21, 11, 25), metar.Time);
        Assert.Equal(240, metar.Wind!.Direction);
        Assert.Equal(new Speed(25, SpeedUnit.Knots), metar.Wind.Gust);
        Assert.Equal(200, metar.Wind.VariableFrom);
        Assert.Equal(280, metar.Wind.VariableTo);
        Assert.True(metar.Visibility!.IsTenKmOrMore);
        Assert.Equal(WeatherDescriptor.Showers, Assert.Single(metar.Weather).Descriptor);
        Assert.Equal(2, metar.Clouds.Count);
        Assert.Equal(CloudType.Cumulonimbus, metar.Clouds[1].Type);
        Assert.Equal(12, metar.Temperature);
        Assert.Equal(9, metar.DewPoint);
        Assert.Equal(1013, metar.Pressure?.Hectopascals);
        Trend trend = Assert.Single(metar.Trends);
        Assert.Equal(TrendKind.Temporary, trend.Kind);
        Assert.Equal(4000, trend.Conditions!.Visibility!.Meters);
        Assert.Single(trend.Conditions.Weather);
        Assert.Equal(FlightCategory.Mvfr, metar.FlightCategory); // BKN030 is a 3,000 ft ceiling
        Assert.False(metar.HasErrors);
    }

    [Fact]
    public void CavokNosigAndAutomaticStation()
    {
        Metar metar = Metar.Parse("METAR EDDF 210750Z AUTO 25006KT CAVOK 14/09 Q1031 NOSIG");

        Assert.Empty(metar.Diagnostics);
        Assert.True(metar.IsAuto);
        Assert.True(metar.IsCavok);
        Assert.Null(metar.Visibility);
        Trend trend = Assert.Single(metar.Trends);
        Assert.Equal(TrendKind.NoSignificantChange, trend.Kind);
        Assert.Null(trend.Conditions);
        Assert.Equal(FlightCategory.Vfr, metar.FlightCategory);
    }

    [Theory]
    [InlineData("SPECI LKKB 210800Z COR 24013KT 9999 FEW021 SCT060 14/08 Q1026 NOSIG", ReportType.Speci)]
    [InlineData("METAR COR EHAM 211125Z 24005KT CAVOK 12/09 Q1013", ReportType.Metar)]
    public void CorrectedReports(string raw, ReportType type)
    {
        Metar metar = Metar.Parse(raw);

        Assert.Empty(metar.Diagnostics);
        Assert.Equal(type, metar.Type);
        Assert.True(metar.IsCorrected);
    }

    [Fact]
    public void MissingValuesFromAutomaticStationsAreNotErrors()
    {
        Metar metar = Metar.Parse("METAR LIBQ 210755Z AUTO VRB01KT //// // ///////// 15/10 Q1027 RMK ///");

        Assert.Empty(metar.Diagnostics);
        Assert.True(metar.Visibility!.IsMissing);
        Assert.True(Assert.Single(metar.Weather).IsNotObservable);
        CloudLayer layer = Assert.Single(metar.Clouds);
        Assert.Null(layer.Cover);
        Assert.True(layer.IsTypeNotObservable);
        Assert.Equal("///", metar.Remarks);
    }

    [Fact]
    public void MissingStatuteMileVisibilityFromCanadianAutomaticStationsIsNotAnError()
    {
        Metar metar = Metar.Parse("METAR CBAR 210900Z AUTO 36009KT ////SM ////// 04/04 A2929 RMK VIS MISG CLD MISG T00400039 SLP925");

        Assert.Empty(metar.Diagnostics);
        Assert.True(metar.Visibility!.IsMissing);
        Assert.Null(metar.Visibility.StatuteMiles);
    }

    [Theory]
    [InlineData("METAR LFKS 201430Z AUTO 12005KT 050V160 9999 ///CB 28/20 Q1022", CloudType.Cumulonimbus)]
    [InlineData("METAR LFKC 201330Z AUTO 35010KT 320V020 9999 FEW027/// ///TCU 28/21 Q1023 BECMG NSC", CloudType.ToweringCumulus)]
    public void ConvectiveCloudWithoutAmountOrHeightFromFrenchAutomaticStations(string raw, CloudType type)
    {
        Metar metar = Metar.Parse(raw);

        Assert.Empty(metar.Diagnostics);
        CloudLayer layer = metar.Clouds[metar.Clouds.Count - 1];
        Assert.Null(layer.Cover);
        Assert.Null(layer.HeightFeet);
        Assert.Equal(type, layer.Type);
    }

    [Fact]
    public void RecentWeatherAndSeaState()
    {
        Metar metar = Metar.Parse("METAR EHJR 210755Z AUTO 31011KT //// // ///////// 16/11 Q1030 RE// W18/H14");

        Assert.Empty(metar.Diagnostics);
        Assert.True(Assert.Single(metar.RecentWeather).IsNotObservable);
        Assert.Equal(18, metar.Sea!.SeaTemperature);
        Assert.Equal(14, metar.Sea.WaveHeightDecimeters);
    }

    [Fact]
    public void MilitaryColorStatesBeforeAndInsideTrends()
    {
        Metar metar = Metar.Parse("METAR ETNS 210720Z 30011G21KT 9999 -SHRA FEW020 SCT040 15/11 Q1023 BLU+BLU+ TEMPO BLU");

        Assert.Empty(metar.Diagnostics);
        Assert.Equal(new[] { ColorState.BluePlus, ColorState.BluePlus }, metar.ColorCodes.Select(c => c.State));
        Assert.Equal(ColorState.Blue, Assert.Single(Assert.Single(metar.Trends).Conditions!.ColorCodes).State);
    }

    [Theory]
    [InlineData("METAR ETGG 210820Z AUTO 30026KT 9999 // ////// 15/09 Q1023 ///")]
    [InlineData("METAR ETNS 200920Z AUTO 28017KT //// // ////// 16/13 Q1013 ///")]
    public void MissingColorStateFromGermanMilitaryAutomaticStations(string raw)
    {
        Metar metar = Metar.Parse(raw);

        Assert.Empty(metar.Diagnostics);
        Assert.Empty(metar.ColorCodes);
        Assert.NotNull(metar.Temperature);
        Assert.NotNull(metar.DewPoint);
        Assert.NotNull(metar.Pressure);
    }

    [Fact]
    public void LoneSlashesOutsideTheColorStatePositionAreUnchanged()
    {
        Metar metar = Metar.Parse("METAR ENSE 201220Z AUTO 30035KT 9999 /// ///// Q//// W///S6");

        Diagnostic diagnostic = Assert.Single(metar.Diagnostics);
        Assert.Equal(DiagnosticCode.Duplicate, diagnostic.Code);
        Assert.Equal("/////", diagnostic.Token);
        Assert.Empty(metar.ColorCodes);
    }

    [Fact]
    public void DutchMilitaryAutomaticReport() =>
        Assert.Empty(Metar.Parse("METAR EHKD 210755Z AUTO 30010KT 260V330 9999 SCT030 SCT036 17/10 Q1029 BLU NOSIG").Diagnostics);

    [Fact]
    public void MinimumVisibilityWithDirection()
    {
        Metar metar = Metar.Parse("METAR LEBB 210800Z 11003KT 070V150 9999 4900SE FEW005 16/16 Q1029 NOSIG");

        Assert.Empty(metar.Diagnostics);
        Assert.Equal(new MinimumVisibility(4900, CompassDirection.SouthEast), metar.Visibility!.Minimum);
    }

    [Fact]
    public void RunwayVisualRangeAndVerticalVisibility()
    {
        Metar metar = Metar.Parse("METAR EGLL 210550Z 00000KT 0300 R27L/0550V0800U R27R/P1500N FG VV001 09/09 Q1020 BECMG 1500 BR");

        Assert.Empty(metar.Diagnostics);
        Assert.Equal(2, metar.RunwayVisualRanges.Count);
        Assert.Equal(CloudCover.VerticalVisibility, Assert.Single(metar.Clouds).Cover);
        Assert.Equal(FlightCategory.Lifr, metar.FlightCategory);
        Assert.True(metar.Wind!.IsCalm);
    }

    [Theory]
    [InlineData("METAR EDDB 210750Z AUTO 28016KT 9999 BKN024 BKN033 15/09 Q1023 TEMPO FM0820 29020G30KT", "FM", 8, 20)]
    [InlineData("METAR LTAI 210750Z 07003KT 030V130 9999 FEW030 SCT180 31/14 Q1012 BECMG TL0900 16012KT", "TL", 9, 0)]
    [InlineData("METAR EHAM 211125Z 24005KT 9999 FEW030 12/09 Q1013 BECMG AT1200 30010KT", "AT", 12, 0)]
    public void TrendTimes(string raw, string prefix, int hour, int minute)
    {
        Metar metar = Metar.Parse(raw);

        Assert.Empty(metar.Diagnostics);
        Trend trend = Assert.Single(metar.Trends);
        TimeOfDay? time = prefix switch { "FM" => trend.From, "TL" => trend.Until, _ => trend.At };
        Assert.Equal(new TimeOfDay(hour, minute), time);
        Assert.NotNull(trend.Conditions!.Wind);
    }

    [Fact]
    public void Remarks()
    {
        Metar metar = Metar.Parse("METAR BIHN 210805Z 10010G20KT 7000 +RA BR BKN016 BKN023 OVC029 10/07 Q1006 RMK VIS NW-N +10 KM");

        Assert.Empty(metar.Diagnostics);
        Assert.Equal("VIS NW-N +10 KM", metar.Remarks);
        Assert.Equal(2, metar.Weather.Count);
    }

    [Fact]
    public void UnitedStatesFormat()
    {
        Metar metar = Metar.Parse("SPECI KRUT 210809Z AUTO 00000KT 2 1/2SM BR OVC002 11/11 A3009 RMK AO2 VIS 1 1/4V5 $");

        Assert.Empty(metar.Diagnostics);
        Assert.Equal(2.5, metar.Visibility!.StatuteMiles);
        Assert.Equal(30.09, metar.Pressure!.Value.Value, 2);
        Assert.Equal(PressureUnit.InchesOfMercury, metar.Pressure.Value.Unit);
        Assert.Equal(FlightCategory.Lifr, metar.FlightCategory);
    }

    [Fact]
    public void NilReport()
    {
        Metar metar = Metar.Parse("METAR EHAM 211125Z NIL");

        Assert.Empty(metar.Diagnostics);
        Assert.True(metar.IsNil);
    }

    [Fact]
    public void WindShearRunwayStateAndRecentWeather()
    {
        Metar metar = Metar.Parse("METAR ULLI 211130Z 36005MPS 9999 -SN OVC010 M02/M04 Q1003 RESN WS R28L R28L/590160 NOSIG");

        Assert.Empty(metar.Diagnostics);
        Assert.Equal(WeatherType.Snow, Assert.Single(Assert.Single(metar.RecentWeather).Types));
        Assert.Equal("28L", Assert.Single(metar.WindShear).Runway);
        RunwayState state = Assert.Single(metar.RunwayStates);
        Assert.Equal(5, state.Deposit);
        Assert.Equal(9, state.Extent);
        Assert.Equal(1, state.Depth);
        Assert.Equal(60, state.Friction);
        Assert.Equal(-2, metar.Temperature);
        Assert.Equal(SpeedUnit.MetersPerSecond, metar.Wind!.Speed!.Value.Unit);
    }

    [Fact]
    public void MissingPressureIsNotAnError()
    {
        Metar metar = Metar.Parse("METAR ENQC 210750Z AUTO 33017KT 9999 BKN018/// 12/// Q//// W///S5");

        Assert.Empty(metar.Diagnostics);
        Assert.Null(metar.Pressure);
        Assert.Null(metar.DewPoint);
        Assert.Equal(5, metar.Sea!.StateOfSea);
    }

    [Theory]
    [InlineData("METAR MGMM 210900Z 00000KT CAVOK 24/23 Q1011 A2985 RMK BKN090", 1011)]
    [InlineData("METAR MSLP 210900Z 08004KT 8000 RA FEW027 BKN060 24/23 Q1009 A2980 NOSIG RMK DSTN CB S SW", 1009)]
    [InlineData("METAR OMRK 220700Z 23006KT 120V280 CAVOK 39/14 Q1009 A2982", 1009)]
    public void PressureInBothUnitsKeepsTheFirst(string raw, int hectopascals)
    {
        Metar metar = Metar.Parse(raw);

        Assert.Empty(metar.Diagnostics);
        Assert.Equal(new Pressure(hectopascals, PressureUnit.Hectopascals), metar.Pressure);
    }

    [Fact]
    public void PressureInInchesThenHectopascals()
    {
        Metar metar = Metar.Parse("METAR MGMM 210900Z 00000KT CAVOK 24/23 A2985 Q1011");

        Assert.Empty(metar.Diagnostics);
        Assert.Equal(new Pressure(29.85, PressureUnit.InchesOfMercury), metar.Pressure);
    }

    [Fact]
    public void MissingHectopascalsThenInches()
    {
        Metar metar = Metar.Parse("METAR MGMM 210900Z 00000KT CAVOK 24/23 Q//// A2985");

        Assert.Empty(metar.Diagnostics);
        Assert.Equal(new Pressure(29.85, PressureUnit.InchesOfMercury), metar.Pressure);
    }

    [Fact]
    public void MissingInchesThenHectopascals()
    {
        Metar metar = Metar.Parse("METAR MGMM 210900Z 00000KT CAVOK 24/23 A//// Q1011");

        Assert.Empty(metar.Diagnostics);
        Assert.Equal(new Pressure(1011, PressureUnit.Hectopascals), metar.Pressure);
    }

    [Fact]
    public void HectopascalsThenMissingInches()
    {
        Metar metar = Metar.Parse("METAR MGMM 210900Z 00000KT CAVOK 24/23 Q1011 A////");

        Assert.Empty(metar.Diagnostics);
        Assert.Equal(new Pressure(1011, PressureUnit.Hectopascals), metar.Pressure);
    }

    [Theory]
    [InlineData("METAR MGMM 210900Z 00000KT CAVOK 24/23 Q1011 Q1012", "Q1012")]
    [InlineData("METAR MGMM 210900Z 00000KT CAVOK 24/23 A2985 A2986", "A2986")]
    [InlineData("METAR MGMM 210900Z 00000KT CAVOK 24/23 Q1011 A2985 Q1012", "Q1012")]
    [InlineData("METAR MGMM 210900Z 00000KT CAVOK 24/23 Q1011 A2985 A2986", "A2986")]
    [InlineData("METAR MGMM 210900Z 00000KT CAVOK 24/23 Q1011 RERA A2985", "A2985")]
    public void SecondPressureInTheSameUnitOrNotDirectlyAfterIsADuplicate(string raw, string duplicate)
    {
        Metar metar = Metar.Parse(raw);

        Diagnostic diagnostic = Assert.Single(metar.Diagnostics);
        Assert.Equal(DiagnosticCode.Duplicate, diagnostic.Code);
        Assert.Equal(duplicate, diagnostic.Token);
    }

    [Theory]
    [InlineData("METAR EHAM 211125Z 24005KT CAVOK 12/09 Q1013 NOSIG=")]
    [InlineData("metar eham 211125z 24005kt cavok 12/09 q1013")]
    [InlineData("  METAR EHAM 211125Z\n 24005KT CAVOK\t12/09 Q1013  ")]
    [InlineData("EHAM 211125Z 24005KT CAVOK 12/09 Q1013")]
    public void NormalisesFormatting(string raw)
    {
        Metar metar = Metar.Parse(raw);

        Assert.Empty(metar.Diagnostics);
        Assert.Equal("EHAM", metar.Station);
        Assert.Equal(raw, metar.Raw);
    }
}
