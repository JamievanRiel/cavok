namespace Cavok.Corpus;

// Prints the diagnostics the parser reports on the corpus, grouped by code and token shape (digits → 9),
// with the number of stations involved and one example, so parser gaps and broken reports can be told apart.
internal static class CorpusTriage
{
    public static void Run(string dir)
    {
        Report(Path.Combine(dir, "metar-eu.txt"), raw => Metar.Parse(raw).Diagnostics);
        Report(Path.Combine(dir, "taf-eu.txt"), raw => Taf.Parse(raw).Diagnostics);
        Report(Path.Combine(dir, "metar-world.txt"), raw => Metar.Parse(raw).Diagnostics);
    }

    private static void Report(string path, Func<string, IReadOnlyList<Diagnostic>> parse)
    {
        List<string> reports = CorpusFetcher.ReadLines(path);
        var buckets = new Dictionary<string, Bucket>(StringComparer.Ordinal);
        int withErrors = 0;
        foreach (string raw in reports)
        {
            IReadOnlyList<Diagnostic> diagnostics = parse(raw);
            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                withErrors++;
            }

            foreach (Diagnostic diagnostic in diagnostics)
            {
                string key = $"{diagnostic.Id} {diagnostic.Severity} {diagnostic.Code} '{Shape(diagnostic.Token)}'";
                if (!buckets.TryGetValue(key, out Bucket? bucket))
                {
                    bucket = new Bucket(diagnostic.ToString());
                    buckets.Add(key, bucket);
                }

                bucket.Count++;
                bucket.Stations.Add(Station(raw));
            }
        }

        Console.WriteLine($"{Path.GetFileName(path)}: {reports.Count} reports, {withErrors} with errors");
        foreach (KeyValuePair<string, Bucket> entry in buckets.OrderByDescending(e => e.Value.Count))
        {
            Console.WriteLine($"  {entry.Value.Count,5}x at {entry.Value.Stations.Count,3} stations  {entry.Key}");
            Console.WriteLine("      " + entry.Value.Example.Replace("\n", "\n      "));
        }

        Console.WriteLine();
    }

    private static string Shape(string token) =>
        new string(token.Select(c => char.IsAsciiDigit(c) ? '9' : c).ToArray());

    private static string Station(string raw)
    {
        foreach (string token in raw.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (token is not ("METAR" or "SPECI" or "TAF" or "AMD" or "COR"))
            {
                return token;
            }
        }

        return "";
    }

    private sealed class Bucket
    {
        public Bucket(string example)
        {
            Example = example;
        }

        public int Count { get; set; }

        public HashSet<string> Stations { get; } = new HashSet<string>(StringComparer.Ordinal);

        public string Example { get; }
    }
}
