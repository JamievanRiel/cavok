using Cavok.Parsing;

namespace Cavok.Tests;

public class MetarDiagnosticsTests
{
    [Fact]
    public void UnknownGroupIsReportedAndTheRestIsParsed()
    {
        Metar metar = Metar.Parse("METAR EHAM 211125Z 24012KT 2400O -RA FEW012 12/09 Q1013");

        Diagnostic diagnostic = Assert.Single(metar.Diagnostics);
        Assert.Equal(DiagnosticCode.InvalidVisibility, diagnostic.Code);
        Assert.Equal(27, diagnostic.Position);
        Assert.Equal("2400O", diagnostic.Token);
        Assert.Null(metar.Visibility);
        Assert.Single(metar.Weather);
        Assert.Single(metar.Clouds);
        Assert.Equal(12, metar.Temperature);
        Assert.NotNull(metar.Pressure);
        Assert.True(metar.HasErrors);
    }

    [Fact]
    public void GroupOutOfOrderIsAWarning()
    {
        Metar metar = Metar.Parse("METAR EHAM 211125Z 24012KT 9999 12/09 FEW012 Q1013");

        Diagnostic diagnostic = Assert.Single(metar.Diagnostics);
        Assert.Equal(DiagnosticCode.OutOfOrder, diagnostic.Code);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("FEW012", diagnostic.Token);
        Assert.Single(metar.Clouds);
        Assert.False(metar.HasErrors);
    }

    [Fact]
    public void DuplicateGroupKeepsTheFirst()
    {
        Metar metar = Metar.Parse("METAR EHAM 211125Z 24012KT 25015KT 9999 Q1013");

        Assert.Equal(DiagnosticCode.Duplicate, Assert.Single(metar.Diagnostics).Code);
        Assert.Equal(240, metar.Wind!.Direction);
    }

    [Theory]
    [InlineData("METAR EHAM 211125Z 24012G10KT 9999 Q1013", DiagnosticCode.GustNotAboveSpeed)]
    [InlineData("METAR EHAM 211125Z 24012KT 9999 10/12 Q1013", DiagnosticCode.DewPointAboveTemperature)]
    [InlineData("METAR EHAM 211125Z 24012KT 9999 NSW Q1013", DiagnosticCode.InvalidWeather)]
    [InlineData("METAR EH4 211125Z 24012KT CAVOK Q1013", DiagnosticCode.InvalidStation)]
    [InlineData("METAR EHAM 24012KT CAVOK Q1013", DiagnosticCode.MissingTime)]
    [InlineData("METAR EHAM 21112OZ 24012KT CAVOK Q1013", DiagnosticCode.InvalidTime)]
    public void ReportsSingleProblems(string raw, DiagnosticCode code) =>
        Assert.Equal(code, Assert.Single(Metar.Parse(raw).Diagnostics).Code);

    [Fact]
    public void MinimumVisibilityAfterCavokIsNotApplied()
    {
        Metar metar = Metar.Parse("METAR EHAM 211125Z 24005KT CAVOK 4000W 12/09 Q1013 TEMPO CAVOK 1500N");

        Assert.True(metar.IsCavok);
        Assert.Null(metar.Visibility);
        ForecastConditions trend = Assert.Single(metar.Trends).Conditions!;
        Assert.True(trend.IsCavok);
        Assert.Null(trend.Visibility);
        Assert.Equal(new[] { "4000W", "1500N" }, metar.Diagnostics.Select(d => d.Token));
        Assert.All(metar.Diagnostics, d => Assert.Equal(DiagnosticCode.Duplicate, d.Code));
    }

    [Fact]
    public void MissingStationPointsToWhereItWasExpected()
    {
        Metar metar = Metar.Parse("METAR 211125Z 24012KT CAVOK Q1013");

        Diagnostic diagnostic = Assert.Single(metar.Diagnostics);
        Assert.Equal(DiagnosticCode.MissingStation, diagnostic.Code);
        Assert.Equal(6, diagnostic.Position);
        Assert.Equal(new DayTime(21, 11, 25), metar.Time);
    }

    [Fact]
    public void TafGivenToTheMetarParser() =>
        Assert.Equal(DiagnosticCode.WrongReportType, Metar.Parse("TAF EHAM 210440Z 2106/2212 27005KT 9999 FEW035").Diagnostics[0].Code);

    [Theory]
    [InlineData("")]
    [InlineData("  \n\t")]
    public void EmptyInput(string raw) => Assert.Equal(DiagnosticCode.EmptyInput, Assert.Single(Metar.Parse(raw).Diagnostics).Code);

    [Fact]
    public void InputTooLong() =>
        Assert.Equal(DiagnosticCode.InputTooLong, Assert.Single(Metar.Parse(new string('A', MetarParser.MaxLength + 1)).Diagnostics).Code);

    [Fact]
    public void NullThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Metar.Parse(null!));
        Assert.Throws<ArgumentNullException>(() => Metar.ParseStrict(null!));
    }

    [Fact]
    public void ParseStrictThrowsOnErrors()
    {
        var exception = Assert.Throws<CavokParseException>(() => Metar.ParseStrict("METAR EHAM 211125Z 24012KT 2400O Q1013"));

        Assert.Contains("CAV004", exception.Message, StringComparison.Ordinal);
        Assert.Single(exception.Diagnostics);
    }

    [Fact]
    public void ParseStrictAllowsWarnings() =>
        Assert.Single(Metar.ParseStrict("METAR EHAM 211125Z 24012G10KT 9999 Q1013").Diagnostics);

    [Fact]
    public void InvalidTrendTimeKeepsTheTrend()
    {
        Metar metar = Metar.Parse("METAR EHAM 211125Z 24012KT 9999 Q1013 TEMPO FM1O30 4000");

        Assert.Equal(DiagnosticCode.InvalidTrend, Assert.Single(metar.Diagnostics).Code);
        Assert.Equal(4000, Assert.Single(metar.Trends).Conditions!.Visibility!.Meters);
    }

    [Fact]
    public void GroupsThatCannotBelongToATrendGoBackToTheReport()
    {
        Metar metar = Metar.Parse("METAR EHAM 211125Z 24012KT 9999 Q1013 TEMPO 4000 RERA");

        Assert.Equal(DiagnosticCode.OutOfOrder, Assert.Single(metar.Diagnostics).Code);
        Assert.Single(metar.RecentWeather);
        Assert.Equal(4000, Assert.Single(metar.Trends).Conditions!.Visibility!.Meters);
    }

    [Fact]
    public void AnUnknownTokenInsideATrendDoesNotEndIt()
    {
        Metar metar = Metar.Parse("METAR EHAM 211125Z 24012KT 9999 Q1013 TEMPO 4000 XX SHRA");

        Assert.Equal(DiagnosticCode.UnknownGroup, Assert.Single(metar.Diagnostics).Code);
        Assert.Empty(metar.Weather);
        Assert.Single(Assert.Single(metar.Trends).Conditions!.Weather);
    }

    [Theory]
    [InlineData("METAR EHAM 211125Z 24012KT 2400O -RA XX FEW012 12/09 Q1013 TEMPO FM1O30 4000 RMK TEST")]
    [InlineData("METAR 211125Z 24012KT 25015KT CAVOK 9999 NSW Q1013 TAF")]
    [InlineData("TAF EHAM 210440Z 2106/2212 27005KT")]
    public void EveryTokenIsConsumedOrReported(string raw)
    {
        Metar metar = MetarParser.Parse(raw, out TokenCursor cursor);

        for (int i = 0; i < cursor.Tokens.Count; i++)
        {
            Token token = cursor.Tokens[i];
            bool reported = metar.Diagnostics.Any(d => d.Length > 0 && d.Position == token.Position);
            Assert.True(cursor.IsConsumed(i) || reported, $"token '{token.Text}' was silently dropped");
        }
    }
}
