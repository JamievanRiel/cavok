using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace Cavok.Corpus;

// Downloads real reports from the aviationweather.gov Data API and merges them into the corpus files.
internal static class CorpusFetcher
{
    private const int MetarStations = 240;
    private const int MetarStationsPerRequest = 8; // the API returns at most ~400 reports per request
    private const int MetarHours = 24;
    private const int TafStationsPerRequest = 100;
    private const int TafSteps = 8; // TAFs valid every 6 hours over the last 2 days
    private const int WorldSamplePerRun = 250;
    private const int WorldSampleMax = 1000;
    private static readonly TimeSpan PauseBetweenRequests = TimeSpan.FromMilliseconds(500);

    public static async Task FetchAsync(string dir)
    {
        using var http = new HttpClient
        {
            BaseAddress = new Uri("https://aviationweather.gov"),
            Timeout = TimeSpan.FromSeconds(60),
        };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("Cavok-Corpus/1.0 (+https://github.com/JamievanRiel/cavok)");

        List<(string Station, string Raw)> metarCache =
            ParseMetarCsv(await DownloadGzipAsync(http, "/data/cache/metars.cache.csv.gz")).ToList();
        List<(string Station, string Raw)> tafCache =
            ParseTafXml(await DownloadGzipAsync(http, "/data/cache/tafs.cache.xml.gz")).ToList();
        Console.WriteLine($"Cache: {metarCache.Count} METARs, {tafCache.Count} TAFs");

        var random = new Random();
        List<string> euMetars = metarCache.Where(m => IsEuropean(m.Station)).Select(m => m.Raw).ToList();
        string[] metarStations = metarCache.Select(m => m.Station).Where(IsEuropean).Distinct()
            .OrderBy(_ => random.Next()).Take(MetarStations).ToArray();
        foreach (string[] batch in metarStations.Chunk(MetarStationsPerRequest))
        {
            string text = await GetTextAsync(http, $"/api/data/metar?ids={string.Join(',', batch)}&format=raw&hours={MetarHours}");
            euMetars.AddRange(SplitRawMetars(text));
        }

        List<string> euTafs = tafCache.Where(t => IsEuropean(t.Station)).Select(t => t.Raw).ToList();
        string[] tafStations = tafCache.Select(t => t.Station).Where(IsEuropean).Distinct().ToArray();
        DateTime now = DateTime.UtcNow;
        for (int step = 1; step <= TafSteps; step++)
        {
            string date = now.AddHours(-6 * step).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
            foreach (string[] batch in tafStations.Chunk(TafStationsPerRequest))
            {
                string text = await GetTextAsync(http, $"/api/data/taf?ids={string.Join(',', batch)}&format=raw&time=valid&date={date}");
                euTafs.AddRange(SplitRawTafs(text));
            }
        }

        string worldPath = Path.Combine(dir, "metar-world.txt");
        int room = Math.Max(0, Math.Min(WorldSamplePerRun, WorldSampleMax - ReadLines(worldPath).Count));
        IEnumerable<string> world = metarCache.Where(m => !IsEuropean(m.Station)).Select(m => m.Raw)
            .OrderBy(_ => random.Next()).Take(room);

        int addedMetars = Merge(Path.Combine(dir, "metar-eu.txt"), euMetars);
        int addedTafs = Merge(Path.Combine(dir, "taf-eu.txt"), euTafs);
        int addedWorld = Merge(worldPath, world);
        Console.WriteLine($"metar-eu.txt +{addedMetars}, taf-eu.txt +{addedTafs}, metar-world.txt +{addedWorld}");
    }

    internal static bool IsEuropean(string station) =>
        station.Length == 4 && (station[0] == 'E' || station[0] == 'L' || station[0] == 'B');

    internal static IEnumerable<(string Station, string Raw)> ParseMetarCsv(string csv)
    {
        using var reader = new StringReader(csv);
        reader.ReadLine(); // header: raw_text,station_id,...
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            List<string> fields = SplitCsv(line);
            if (fields.Count < 2)
            {
                continue;
            }

            string raw = Normalize(fields[0]);
            if (raw.StartsWith("METAR ", StringComparison.Ordinal) || raw.StartsWith("SPECI ", StringComparison.Ordinal))
            {
                yield return (fields[1], raw);
            }
        }
    }

    internal static IEnumerable<(string Station, string Raw)> ParseTafXml(string xml)
    {
        foreach (XElement taf in XDocument.Parse(xml).Descendants("TAF"))
        {
            string raw = Normalize((string?)taf.Element("raw_text") ?? "");
            string station = (string?)taf.Element("station_id") ?? "";
            if (raw.StartsWith("TAF", StringComparison.Ordinal))
            {
                yield return (station, raw);
            }
        }
    }

    // Raw METAR output: one report per line.
    internal static IEnumerable<string> SplitRawMetars(string text) =>
        text.Split('\n')
            .Select(Normalize)
            .Where(l => l.StartsWith("METAR ", StringComparison.Ordinal) || l.StartsWith("SPECI ", StringComparison.Ordinal));

    // Raw TAF output: a report starts with "TAF" at column 0; continuation lines are indented.
    internal static IEnumerable<string> SplitRawTafs(string text)
    {
        var current = new List<string>();
        foreach (string line in text.Split('\n'))
        {
            if (line.StartsWith("TAF", StringComparison.Ordinal) && current.Count > 0)
            {
                yield return Normalize(string.Join(' ', current));
                current.Clear();
            }

            if (line.Trim().Length > 0)
            {
                current.Add(line);
            }
        }

        if (current.Count > 0)
        {
            string last = Normalize(string.Join(' ', current));
            if (last.StartsWith("TAF", StringComparison.Ordinal))
            {
                yield return last;
            }
        }
    }

    internal static List<string> SplitCsv(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        bool quoted = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    quoted = false;
                }
                else
                {
                    current.Append(c);
                }
            }
            else if (c == '"')
            {
                quoted = true;
            }
            else if (c == ',')
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());
        return fields;
    }

    internal static string Normalize(string raw) =>
        string.Join(' ', raw.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    internal static List<string> ReadLines(string path) =>
        File.Exists(path) ? File.ReadAllLines(path).Where(l => l.Length > 0).ToList() : new List<string>();

    private static async Task<string> DownloadGzipAsync(HttpClient http, string path)
    {
        await using Stream compressed = await http.GetStreamAsync(path);
        await using var gzip = new GZipStream(compressed, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip);
        return await reader.ReadToEndAsync();
    }

    private static async Task<string> GetTextAsync(HttpClient http, string path)
    {
        try
        {
            return await http.GetStringAsync(path);
        }
        catch (HttpRequestException e)
        {
            Console.Error.WriteLine($"warning: {path}: {e.Message}");
            return "";
        }
        catch (TaskCanceledException)
        {
            Console.Error.WriteLine($"warning: {path}: timed out");
            return "";
        }
        finally
        {
            await Task.Delay(PauseBetweenRequests);
        }
    }

    private static int Merge(string path, IEnumerable<string> lines)
    {
        var set = new HashSet<string>(ReadLines(path), StringComparer.Ordinal);
        int before = set.Count;
        foreach (string line in lines)
        {
            set.Add(line);
        }

        List<string> sorted = set.ToList();
        sorted.Sort(StringComparer.Ordinal);
        File.WriteAllText(path, string.Join("\n", sorted) + "\n");
        return set.Count - before;
    }
}
