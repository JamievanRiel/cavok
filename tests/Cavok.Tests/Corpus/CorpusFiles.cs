namespace Cavok.Tests.Corpus;

internal static class CorpusFiles
{
    public static string FullPath(string name) => Path.Combine(AppContext.BaseDirectory, "Corpus", name);

    // Non-empty lines that are not '#' comments.
    public static IReadOnlyList<string> Lines(string name) =>
        File.ReadAllLines(FullPath(name))
            .Where(line => line.Length > 0 && !line.StartsWith("#", StringComparison.Ordinal))
            .ToArray();

    public static HashSet<string> KnownIssues() => new HashSet<string>(Lines("known-issues.txt"), StringComparer.Ordinal);
}
