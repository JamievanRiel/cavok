using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cavok.Tests.Corpus;

namespace Cavok.Tests.Snapshots;

public class SnapshotTests
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        NewLine = "\n",
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
    };

    [Fact]
    public void Metars() => Snapshot.Verify("metar", CorpusFiles.Lines("snapshots-metar.txt").Select(DescribeMetar));

    [Fact]
    public void Tafs() => Snapshot.Verify("taf", CorpusFiles.Lines("snapshots-taf.txt").Select(DescribeTaf));

    [Fact]
    public void EdgeCases() => Snapshot.Verify(
        "edge",
        CorpusFiles.Lines("snapshots-edge.txt").Select(raw => raw.StartsWith("TAF", StringComparison.Ordinal) ? DescribeTaf(raw) : DescribeMetar(raw)));

    private static string DescribeMetar(string raw)
    {
        Metar metar = Metar.Parse(raw);
        return Block(raw, JsonSerializer.Serialize(metar, Json), metar.Describe(Language.English), metar.Describe(Language.Dutch));
    }

    private static string DescribeTaf(string raw)
    {
        Taf taf = Taf.Parse(raw);
        return Block(raw, JsonSerializer.Serialize(taf, Json), taf.Describe(Language.English), taf.Describe(Language.Dutch));
    }

    private static string Block(string raw, string json, string english, string dutch) =>
        "=== " + raw + "\n" + json + "\n--- en\n" + english + "\n--- nl\n" + dutch + "\n";
}
