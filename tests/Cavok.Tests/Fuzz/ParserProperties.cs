using System.Diagnostics;
using FsCheck;
using FsCheck.Fluent;

namespace Cavok.Tests.Fuzz;

// The "never crashes" guarantee: for any input, parsing, describing and formatting never throw (except
// CavokParseException from ParseStrict), never report an internal error, are deterministic, and every
// diagnostic points inside the input.
public class ParserProperties
{
    private static int Cases =>
        int.TryParse(Environment.GetEnvironmentVariable("CAVOK_FUZZ_CASES"), out int cases) && cases > 0 ? cases : 1000;

    private static Config Config => Config.QuickThrowOnFailure.WithMaxTest(Cases);

    [Fact]
    public void ArbitraryStringsNeverBreakTheParser() =>
        Prop.ForAll(Generators.AnyString().ToArbitrary(), IsSafe).Check(Config);

    [Fact]
    public void TokenSoupNeverBreaksTheParser() =>
        Prop.ForAll(Generators.TokenSoup().ToArbitrary(), IsSafe).Check(Config);

    [Fact]
    public void MutatedRealReportsNeverBreakTheParser() =>
        Prop.ForAll(Generators.MutatedReport().ToArbitrary(), IsSafe).Check(Config);

    [Theory]
    [InlineData("\0")]
    [InlineData("R/")]
    [InlineData("WS")]
    [InlineData("WS ALL")]
    [InlineData("1")]
    [InlineData("1 1/")]
    [InlineData("PROB")]
    [InlineData("FM")]
    [InlineData("TX")]
    [InlineData("M/")]
    [InlineData("/")]
    [InlineData("Q")]
    [InlineData("RE")]
    [InlineData("BLACK")]
    [InlineData("TEMPO FM")]
    [InlineData("METAR")]
    [InlineData("TAF")]
    [InlineData("TAF EHAM 210440Z 2106/2212 BECMG")]
    [InlineData("METAR EHAM 211125Z RMK")]
    [InlineData("RMK")]
    [InlineData("= = =")]
    [InlineData("R24/")]
    [InlineData("R24L/")]
    [InlineData("W/S")]
    [InlineData("W15/")]
    [InlineData("VV")]
    [InlineData("TEMPO TEMPO TEMPO")]
    [InlineData("PROB30 TEMPO")]
    [InlineData("1 1/2SM")]
    [InlineData("TXM2/2114Z")]
    [InlineData("TX15/")]
    [InlineData("QNH3043INS")]
    public void KnownTrickyInputs(string input) => Assert.True(IsSafe(input));

    // xUnit v3 cannot discover/serialize a lone UTF-16 surrogate as [InlineData], so it is covered separately.
    [Fact]
    public void LoneSurrogateNeverBreaksTheParser() => Assert.True(IsSafe("\uD800"));

    [Fact]
    public void LongInputIsParsedQuickly()
    {
        string input = string.Join(" ", Enumerable.Repeat("FEW012", 9000));
        Stopwatch stopwatch = Stopwatch.StartNew();

        Assert.True(IsSafe(input));

        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5), $"took {stopwatch.Elapsed}");
    }

    [Fact]
    public void MutationIsDeterministic() =>
        Assert.Equal(Generators.Mutate("METAR EHAM 211125Z 24012KT 9999 Q1013", 42), Generators.Mutate("METAR EHAM 211125Z 24012KT 9999 Q1013", 42));

    private static bool IsSafe(string input)
    {
        Metar metar = Metar.Parse(input);
        Taf taf = Taf.Parse(input);
        CheckDiagnostics(input, metar.Diagnostics);
        CheckDiagnostics(input, taf.Diagnostics);
        Assert.Equal(metar.Diagnostics, Metar.Parse(input).Diagnostics);
        Assert.Equal(taf.Diagnostics, Taf.Parse(input).Diagnostics);

        foreach (Language language in new[] { Language.English, Language.Dutch })
        {
            Assert.NotNull(metar.Describe(language));
            Assert.NotNull(taf.Describe(language));
            foreach (Diagnostic diagnostic in metar.Diagnostics.Concat(taf.Diagnostics))
            {
                Assert.NotNull(diagnostic.ToString());
                Assert.NotNull(diagnostic.Describe(language));
            }
        }

        StrictThrowsOnlyParseExceptions(() => Metar.ParseStrict(input));
        StrictThrowsOnlyParseExceptions(() => Taf.ParseStrict(input));
        return true;
    }

    private static void CheckDiagnostics(string input, IReadOnlyList<Diagnostic> diagnostics)
    {
        foreach (Diagnostic diagnostic in diagnostics)
        {
            Assert.NotEqual(DiagnosticCode.InternalError, diagnostic.Code);
            Assert.InRange(diagnostic.Position, 0, input.Length);
            Assert.InRange(diagnostic.Length, 0, input.Length - diagnostic.Position);
        }
    }

    private static void StrictThrowsOnlyParseExceptions(Action parse)
    {
        try
        {
            parse();
        }
        catch (CavokParseException)
        {
            // Expected for reports with errors.
        }
    }
}
