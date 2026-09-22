using Cavok.Parsing;

namespace Cavok.Tests.Corpus;

public class CorpusTests(ITestOutputHelper output)
{
    [Fact]
    public void CorpusIsLargeEnough()
    {
        Assert.True(CorpusFiles.Lines("metar-eu.txt").Count >= 3000, "metar-eu.txt needs at least 3000 reports; run the corpus tool");
        Assert.True(CorpusFiles.Lines("taf-eu.txt").Count >= 1500, "taf-eu.txt needs at least 1500 reports; run the corpus tool");
        Assert.True(CorpusFiles.Lines("metar-world.txt").Count >= 200, "metar-world.txt needs at least 200 reports; run the corpus tool");
    }

    [Fact]
    public void EuropeanMetarsParseWithoutErrors() => AssertNoErrors("metar-eu.txt", raw => Metar.Parse(raw).Diagnostics);

    [Fact]
    public void EuropeanTafsParseWithoutErrors() => AssertNoErrors("taf-eu.txt", raw => Taf.Parse(raw).Diagnostics);

    [Fact]
    public void WorldMetarsNeverFailInternally()
    {
        int withErrors = 0;
        var codes = new Dictionary<DiagnosticCode, int>();
        IReadOnlyList<string> reports = CorpusFiles.Lines("metar-world.txt");
        foreach (string raw in reports)
        {
            IReadOnlyList<Diagnostic> diagnostics = Metar.Parse(raw).Diagnostics;
            Assert.DoesNotContain(diagnostics, d => d.Code == DiagnosticCode.InternalError);
            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                withErrors++;
            }

            foreach (Diagnostic diagnostic in diagnostics)
            {
                codes[diagnostic.Code] = codes.TryGetValue(diagnostic.Code, out int count) ? count + 1 : 1;
            }
        }

        output.WriteLine($"{withErrors} of {reports.Count} world METARs have errors: "
            + string.Join(", ", codes.OrderByDescending(c => c.Value).Select(c => $"{c.Key} x{c.Value}")));
    }

    [Fact]
    public void EveryTokenIsConsumedOrReported()
    {
        var dropped = new List<string>();
        foreach (string raw in CorpusFiles.Lines("metar-eu.txt").Concat(CorpusFiles.Lines("metar-world.txt")))
        {
            Metar metar = MetarParser.Parse(raw, out TokenCursor cursor);
            dropped.AddRange(Dropped(raw, cursor, metar.Diagnostics));
        }

        foreach (string raw in CorpusFiles.Lines("taf-eu.txt"))
        {
            Taf taf = TafParser.Parse(raw, out TokenCursor cursor);
            dropped.AddRange(Dropped(raw, cursor, taf.Diagnostics));
        }

        Assert.True(dropped.Count == 0, "Silently dropped tokens:\n" + string.Join("\n", dropped.Take(25)));
    }

    [Fact]
    public void ParsingTwiceGivesEqualReports()
    {
        foreach (string raw in CorpusFiles.Lines("metar-eu.txt").Concat(CorpusFiles.Lines("metar-world.txt")))
        {
            Metar metar = Metar.Parse(raw);
            Metar again = Metar.Parse(raw);
            Assert.True(metar == again && metar.GetHashCode() == again.GetHashCode(), "Not equal when parsed twice: " + raw);
        }

        foreach (string raw in CorpusFiles.Lines("taf-eu.txt"))
        {
            Taf taf = Taf.Parse(raw);
            Taf again = Taf.Parse(raw);
            Assert.True(taf == again && taf.GetHashCode() == again.GetHashCode(), "Not equal when parsed twice: " + raw);
        }
    }

    [Fact]
    public void KnownIssuesStillHaveErrors()
    {
        foreach (string raw in CorpusFiles.KnownIssues())
        {
            bool hasErrors = raw.StartsWith("TAF", StringComparison.Ordinal) ? Taf.Parse(raw).HasErrors : Metar.Parse(raw).HasErrors;
            Assert.True(hasErrors, "This known issue parses cleanly now; remove it from known-issues.txt:\n" + raw);
        }
    }

    private static void AssertNoErrors(string file, Func<string, IReadOnlyList<Diagnostic>> parse)
    {
        HashSet<string> known = CorpusFiles.KnownIssues();
        var failures = new List<string>();
        IReadOnlyList<string> reports = CorpusFiles.Lines(file);
        foreach (string raw in reports)
        {
            IReadOnlyList<Diagnostic> diagnostics = parse(raw);
            bool internalError = diagnostics.Any(d => d.Code == DiagnosticCode.InternalError);
            bool hasErrors = diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
            if (internalError || (hasErrors && !known.Contains(raw)))
            {
                failures.Add(string.Join("\n", diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.ToString())));
            }
        }

        Assert.True(
            failures.Count == 0,
            $"{failures.Count} of {reports.Count} reports in {file} have errors. First {Math.Min(25, failures.Count)}:\n\n"
            + string.Join("\n\n", failures.Take(25)));
    }

    private static IEnumerable<string> Dropped(string raw, TokenCursor cursor, IReadOnlyList<Diagnostic> diagnostics)
    {
        for (int i = 0; i < cursor.Tokens.Count; i++)
        {
            Token token = cursor.Tokens[i];
            if (!cursor.IsConsumed(i) && !diagnostics.Any(d => d.Length > 0 && d.Position == token.Position))
            {
                yield return $"'{token.Text}' in: {raw}";
            }
        }
    }
}
