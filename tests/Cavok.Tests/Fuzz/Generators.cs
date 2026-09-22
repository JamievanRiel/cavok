using Cavok.Tests.Corpus;
using FsCheck;
using FsCheck.Fluent;

namespace Cavok.Tests.Fuzz;

internal static class Generators
{
    private const string MutationCharacters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ/+-= \n\t";

    private static readonly string[] Vocabulary =
    {
        "METAR", "SPECI", "TAF", "AMD", "COR", "NIL", "CNL", "AUTO", "EHAM", "K1V4", "211125Z", "2106/2212", "211824",
        "24012KT", "24012G25KT", "VRB03KT", "00000KT", "/////KT", "270P49MPS", "240120KT", "200V280",
        "9999", "0800", "CAVOK", "////", "9999NDV", "1500SW", "1", "1/2SM", "M1/4SM", "P6SM", "10SM",
        "R24/P2000N", "R06L/0800V1200U", "R24/////", "R01L/0600V1000FT", "R24/290050", "R88/290050", "R24/CLRD62",
        "R/SNOCLO", "SNOCLO", "24290050", "-RA", "+TSRA", "VCSH", "//", "FZFG", "SHRASN", "+FC", "NSW",
        "FEW012", "BKN030CB", "SCT025TCU", "VV///", "VV001", "//////", "/////////", "//////CB", "///015",
        "NSC", "NCD", "SKC", "CLR", "12/09", "M05/M07", "17///", "/////", "12/", "Q1013", "Q////", "A2992", "QNH3043INS",
        "RERA", "RE//", "WS", "ALL", "RWY", "R24", "RWY24", "W15/S4", "W///H///", "BLU", "BLU+BLU+", "BLACKBLU+", "WHT", "RED",
        "NOSIG", "BECMG", "TEMPO", "FM1030", "TL1100", "AT1200", "FM211400", "PROB30", "PROB40", "PROB50", "INTER",
        "TX15/2114Z", "TNM02/2205Z", "RMK", "=", "M", "/", "R", "W", "Z", "V", "KT", "SM", "+", "-", "", "\n", "é", "١٢٣٤",
    };

    public static Gen<string> AnyString() => ArbMap.Default.ArbFor<string>().Generator.Select(s => s ?? "");

    public static Gen<string> TokenSoup() => Gen.ListOf(Gen.Elements(Vocabulary)).Select(tokens => string.Join(" ", tokens));

    public static Gen<string> MutatedReport()
    {
        string[] reports = CorpusFiles.Lines("metar-eu.txt").Take(1000)
            .Concat(CorpusFiles.Lines("taf-eu.txt").Take(500))
            .Concat(CorpusFiles.Lines("metar-world.txt").Take(200))
            .ToArray();
        return Gen.Elements(reports).SelectMany(report => Gen.Choose(0, int.MaxValue).Select(seed => Mutate(report, seed)));
    }

    // Deterministic for a given seed, so a failing case can be replayed.
    internal static string Mutate(string report, int seed)
    {
        var random = new Random(seed);
        List<string> tokens = report.Split(' ').ToList();
        int mutations = random.Next(1, 4);
        for (int m = 0; m < mutations && tokens.Count > 0; m++)
        {
            int i = random.Next(tokens.Count);
            switch (random.Next(5))
            {
                case 0:
                    tokens.RemoveAt(i);
                    break;
                case 1:
                    tokens.Insert(i, tokens[i]);
                    break;
                case 2:
                    {
                        int j = random.Next(tokens.Count);
                        (tokens[i], tokens[j]) = (tokens[j], tokens[i]);
                        break;
                    }

                case 3:
                    {
                        char[] chars = tokens[i].ToCharArray();
                        if (chars.Length > 0)
                        {
                            chars[random.Next(chars.Length)] = MutationCharacters[random.Next(MutationCharacters.Length)];
                        }

                        tokens[i] = new string(chars);
                        break;
                    }

                default:
                    tokens[i] = tokens[i].Substring(0, random.Next(tokens[i].Length + 1));
                    break;
            }
        }

        return string.Join(" ", tokens);
    }
}
