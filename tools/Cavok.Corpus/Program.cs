using Cavok.Corpus;

if (args.Length == 0 || args[0] is "-h" or "--help")
{
    Console.WriteLine("Usage: dotnet run --project tools/Cavok.Corpus -- <command> [--dir <corpus directory>]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  fetch    download METARs and TAFs from aviationweather.gov and merge them into the corpus");
    return 0;
}

string dir = Path.GetFullPath(ArgValue(args, "--dir") ?? Path.Combine("tests", "Cavok.Tests", "Corpus"));
Directory.CreateDirectory(dir);

switch (args[0])
{
    case "fetch":
        await CorpusFetcher.FetchAsync(dir);
        return 0;
    default:
        Console.Error.WriteLine($"Unknown command '{args[0]}'. Run without arguments for help.");
        return 1;
}

static string? ArgValue(string[] args, string name)
{
    int index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}
