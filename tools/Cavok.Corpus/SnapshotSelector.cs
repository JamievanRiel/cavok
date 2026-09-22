namespace Cavok.Corpus;

// Picks a small, diverse subset of the corpus for snapshot tests: repeatedly the report that adds the most
// token shapes (digits replaced by 9) not covered yet. Deterministic for a given corpus.
internal static class SnapshotSelector
{
    private const int MetarCount = 120;
    private const int TafCount = 80;
    private static readonly HashSet<string> HeaderKeywords = new(StringComparer.Ordinal) { "METAR", "SPECI", "TAF", "AMD", "COR" };

    public static void Select(string dir)
    {
        IEnumerable<string> metars = CorpusFetcher.ReadLines(Path.Combine(dir, "metar-eu.txt"))
            .Concat(CorpusFetcher.ReadLines(Path.Combine(dir, "metar-world.txt")));
        List<string> pickedMetars = Pick(metars, MetarCount);
        List<string> pickedTafs = Pick(CorpusFetcher.ReadLines(Path.Combine(dir, "taf-eu.txt")), TafCount);
        File.WriteAllText(Path.Combine(dir, "snapshots-metar.txt"), string.Join("\n", pickedMetars) + "\n");
        File.WriteAllText(Path.Combine(dir, "snapshots-taf.txt"), string.Join("\n", pickedTafs) + "\n");
        Console.WriteLine($"snapshots-metar.txt: {pickedMetars.Count} reports, snapshots-taf.txt: {pickedTafs.Count} reports");
    }

    internal static List<string> Pick(IEnumerable<string> reports, int count)
    {
        List<(string Report, HashSet<string> Shapes)> candidates = reports
            .Distinct(StringComparer.Ordinal)
            .Select(r => (r, new HashSet<string>(Shapes(r), StringComparer.Ordinal)))
            .ToList();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var picked = new List<string>();
        while (picked.Count < count)
        {
            int best = -1;
            int bestGain = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                int gain = candidates[i].Shapes.Count(s => !seen.Contains(s));
                if (gain > bestGain)
                {
                    best = i;
                    bestGain = gain;
                }
            }

            if (best < 0)
            {
                break;
            }

            picked.Add(candidates[best].Report);
            seen.UnionWith(candidates[best].Shapes);
            candidates.RemoveAt(best);
        }

        picked.Sort(StringComparer.Ordinal);
        return picked;
    }

    internal static IEnumerable<string> Shapes(string report)
    {
        string[] tokens = report.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int remarks = Array.IndexOf(tokens, "RMK");
        IEnumerable<string> body = remarks >= 0 ? tokens.Take(remarks) : tokens;

        // Skip the whole header: the report keywords ("TAF AMD", "METAR COR", …) and then the station.
        return body.SkipWhile(HeaderKeywords.Contains).Skip(1)
            .Select(t => new string(t.Select(c => char.IsAsciiDigit(c) ? '9' : c).ToArray()));
    }
}
