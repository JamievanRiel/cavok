using System.Diagnostics;
using Cavok.Parsing;

namespace Cavok.Tests;

public class TafDiagnosticsTests
{
    [Fact]
    public void MissingValidity()
    {
        Taf taf = Taf.Parse("TAF EHAM 210440Z 27005KT 9999");

        Assert.Equal(DiagnosticCode.MissingValidity, Assert.Single(taf.Diagnostics).Code);
        Assert.Equal(270, taf.Base.Wind!.Direction);
    }

    [Fact]
    public void ObsoleteValidityFormat() =>
        Assert.Equal(DiagnosticCode.InvalidValidity, Assert.Single(Taf.Parse("TAF EHAM 210440Z 211824 27005KT").Diagnostics).Code);

    [Fact]
    public void MissingIssueTime()
    {
        Taf taf = Taf.Parse("TAF EHAM 2106/2212 27005KT 9999 FEW035");

        Assert.Equal(DiagnosticCode.MissingTime, Assert.Single(taf.Diagnostics).Code);
        Assert.NotNull(taf.Validity);
    }

    [Fact]
    public void UnknownGroupInTheBaseDoesNotEndIt()
    {
        Taf taf = Taf.Parse("TAF EGXX 210500Z 2106/2212 27005KT 9999 OCV030 BKN050");

        Diagnostic diagnostic = Assert.Single(taf.Diagnostics);
        Assert.Equal(DiagnosticCode.UnknownGroup, diagnostic.Code);
        Assert.Equal("OCV030", diagnostic.Token);
        Assert.Equal(5000, Assert.Single(taf.Base.Clouds).HeightFeet);
    }

    [Fact]
    public void ChangeGroupWithoutPeriodIsAnErrorButIsKept()
    {
        Taf taf = Taf.Parse("TAF EHAM 210440Z 2106/2212 27005KT BECMG 31011KT");

        Assert.Equal(DiagnosticCode.InvalidChangeGroup, Assert.Single(taf.Diagnostics).Code);
        Assert.Equal(310, Assert.Single(taf.Changes).Conditions.Wind!.Direction);
    }

    [Fact]
    public void TruncatedChangePeriodIsASingleInvalidValidity()
    {
        Taf taf = Taf.Parse("TAF LIBN 210800Z 2109/2118 34008KT 9999 FEW020TCU SCT025 PROB40 TEMPO 211/2115 TS FEW020CB BKN025");

        Diagnostic diagnostic = Assert.Single(taf.Diagnostics);
        Assert.Equal(DiagnosticCode.InvalidValidity, diagnostic.Code);
        Assert.Equal("211/2115", diagnostic.Token);
        TafChange change = Assert.Single(taf.Changes);
        Assert.Equal(TafChangeKind.Temporary, change.Kind);
        Assert.Equal(40, change.Probability);
        Assert.Null(change.Period);
    }

    [Theory]
    [InlineData("TAF EHAM 210440Z 2106/2212 27005KT PROB50 2112/2114 BKN005", DiagnosticCode.InvalidChangeGroup)]
    [InlineData("TAF EHAM 210440Z 2106/2212 27005KT FM21140O 30010KT", DiagnosticCode.InvalidChangeGroup)]
    [InlineData("TAF EHAM 210440Z 2106/2212 27005KT 12/09", DiagnosticCode.UnknownGroup)]
    [InlineData("TAF EHAM 210440Z 2106/2212 27005KT TX15/2114", DiagnosticCode.InvalidTemperatureForecast)]
    [InlineData("TAF EHAM 210440Z 2106/2212 27005KT 30010KT", DiagnosticCode.Duplicate)]
    [InlineData("METAR EHAM 210440Z 2106/2212 27005KT", DiagnosticCode.WrongReportType)]
    public void ReportsSingleProblems(string raw, DiagnosticCode code) =>
        Assert.Equal(code, Assert.Single(Taf.Parse(raw).Diagnostics).Code);

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void EmptyInput(string raw) => Assert.Equal(DiagnosticCode.EmptyInput, Assert.Single(Taf.Parse(raw).Diagnostics).Code);

    [Fact]
    public void InputTooLong() =>
        Assert.Equal(DiagnosticCode.InputTooLong, Assert.Single(Taf.Parse(new string(' ', MetarParser.MaxLength + 1)).Diagnostics).Code);

    [Fact]
    public void NullThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Taf.Parse(null!));
        Assert.Throws<ArgumentNullException>(() => Taf.ParseStrict(null!));
    }

    [Fact]
    public void ParseStrictThrowsOnErrors() =>
        Assert.Throws<CavokParseException>(() => Taf.ParseStrict("TAF EHAM 210440Z 27005KT"));

    [Fact]
    public void InterBlockIsReportedOnceAndNotApplied()
    {
        Taf taf = Taf.Parse("TAF YSSY 210500Z 2106/2212 27010KT 9999 FEW012 INTER 2112/2114 4000 SHRA");

        Diagnostic diagnostic = Assert.Single(taf.Diagnostics);
        Assert.Equal(DiagnosticCode.InvalidChangeGroup, diagnostic.Code);
        Assert.Equal("INTER", diagnostic.Token);
        Assert.Empty(taf.Changes);
        Assert.True(taf.Base.Visibility!.IsTenKmOrMore);
        Assert.Null(taf.Base.Visibility.Minimum);
        Assert.Empty(taf.Base.Weather);
        Assert.Equal(1200, Assert.Single(taf.Base.Clouds).HeightFeet);
    }

    [Fact]
    public void InterBlockEndsAtTheNextChangeGroup()
    {
        Taf taf = Taf.Parse(
            "TAF YSSY 210500Z 2106/2212 27010KT 9999 FEW012 TEMPO 2108/2110 5000 RA INTER 2112/2114 4000 SHRA BECMG 2114/2116 30015KT");

        Assert.Equal(DiagnosticCode.InvalidChangeGroup, Assert.Single(taf.Diagnostics).Code);
        Assert.Equal(2, taf.Changes.Count);
        TafChange tempo = taf.Changes[0];
        Assert.Equal(TafChangeKind.Temporary, tempo.Kind);
        Assert.Equal(5000, tempo.Conditions.Visibility!.Meters);
        Assert.Null(tempo.Conditions.Visibility.Minimum);
        Assert.Equal(WeatherType.Rain, Assert.Single(Assert.Single(tempo.Conditions.Weather).Types));
        TafChange becoming = taf.Changes[1];
        Assert.Equal(TafChangeKind.Becoming, becoming.Kind);
        Assert.Equal(300, becoming.Conditions.Wind!.Direction);
        Assert.Empty(becoming.Conditions.Weather);
    }

    [Fact]
    public void InterWithoutPeriodIsReportedOnce()
    {
        Taf taf = Taf.Parse("TAF YSSY 210500Z 2106/2212 27010KT 9999 FEW012 INTER 4000 SHRA");

        Assert.Equal("INTER", Assert.Single(taf.Diagnostics).Token);
        Assert.Empty(taf.Base.Weather);
    }

    [Theory]
    [InlineData("TAF EHAM 210440Z 2106/2212 27005KT OCV030 BECMG 31011KT PROB50 XX RMK A B")]
    [InlineData("METAR TAF 2106/2212 CNL 12/09 FM21140O")]
    [InlineData("TAF YSSY 210500Z 2106/2212 27010KT 9999 FEW012 INTER 2112/2114 4000 SHRA XX 520002 TX15/2114Z")]
    [InlineData("TAF YSSY 210500Z 2106/2212 INTER 211/2114 INTER")]
    public void EveryTokenIsConsumedOrReported(string raw)
    {
        Taf taf = TafParser.Parse(raw, out TokenCursor cursor);

        for (int i = 0; i < cursor.Tokens.Count; i++)
        {
            Token token = cursor.Tokens[i];
            bool reported = taf.Diagnostics.Any(d => d.Length > 0 && d.Position == token.Position);
            Assert.True(cursor.IsConsumed(i) || reported, $"token '{token.Text}' was silently dropped");
        }
    }

    [Fact]
    public void ChangeGroupParsingIsLinear()
    {
        string raw = "TAF EHAM 210440Z 2106/2212 " + string.Join(" ", Enumerable.Repeat("BECMG", 10000));

        var stopwatch = Stopwatch.StartNew();
        Taf taf = Taf.Parse(raw);
        stopwatch.Stop();

        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2), $"took {stopwatch.Elapsed}");
        Assert.Equal(10000, taf.Changes.Count);
    }

    [Fact]
    public void RejectedProbabilityIsNotKept()
    {
        Taf taf = Taf.Parse("TAF EHAM 210440Z 2106/2212 27005KT PROB50 2112/2114 BKN005");

        Assert.Equal(DiagnosticCode.InvalidChangeGroup, Assert.Single(taf.Diagnostics).Code);
        TafChange change = Assert.Single(taf.Changes);
        Assert.Equal(TafChangeKind.Probability, change.Kind);
        Assert.Null(change.Probability);
        Assert.NotNull(change.Period);
    }

    [Fact]
    public void MalformedFromGroupStillOpensANewChangeBlock()
    {
        Taf taf = Taf.Parse("TAF EHAM 210440Z 2106/2212 27005KT 9999 BKN010 FM21140 30010KT 3000 SHRA BKN005");

        Diagnostic diagnostic = Assert.Single(taf.Diagnostics);
        Assert.Equal(DiagnosticCode.InvalidChangeGroup, diagnostic.Code);
        Assert.Equal("FM21140", diagnostic.Token);

        TafChange change = Assert.Single(taf.Changes);
        Assert.Equal(TafChangeKind.From, change.Kind);
        Assert.Null(change.From);
        Assert.Equal(300, change.Conditions.Wind!.Direction);
        Assert.Equal(3000, change.Conditions.Visibility!.Meters);
        Assert.Equal(WeatherType.Rain, Assert.Single(Assert.Single(change.Conditions.Weather).Types));
        Assert.Equal(500, Assert.Single(change.Conditions.Clouds).HeightFeet);

        Assert.Equal(1000, Assert.Single(taf.Base.Clouds).HeightFeet);
        Assert.Empty(taf.Base.Weather);
    }

    [Fact]
    public void MalformedProbabilityGroupStillOpensANewChangeBlock()
    {
        Taf taf = Taf.Parse("TAF EHAM 210440Z 2106/2212 27005KT 9999 PROB3 2112/2114 BKN005");

        Diagnostic diagnostic = Assert.Single(taf.Diagnostics);
        Assert.Equal(DiagnosticCode.InvalidChangeGroup, diagnostic.Code);
        Assert.Equal("PROB3", diagnostic.Token);

        TafChange change = Assert.Single(taf.Changes);
        Assert.Null(change.Probability);
        Assert.Equal(500, Assert.Single(change.Conditions.Clouds).HeightFeet);

        Assert.Empty(taf.Base.Clouds);
    }
}
