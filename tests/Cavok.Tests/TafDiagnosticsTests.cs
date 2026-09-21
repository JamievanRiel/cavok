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

    [Theory]
    [InlineData("TAF EHAM 210440Z 2106/2212 27005KT OCV030 BECMG 31011KT PROB50 XX RMK A B")]
    [InlineData("METAR TAF 2106/2212 CNL 12/09 FM21140O")]
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
}
