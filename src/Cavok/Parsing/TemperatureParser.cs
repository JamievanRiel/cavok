namespace Cavok.Parsing;

internal readonly struct TemperaturePair
{
    public TemperaturePair(int? temperature, int? dewPoint)
    {
        Temperature = temperature;
        DewPoint = dewPoint;
    }

    public int? Temperature { get; }

    public int? DewPoint { get; }
}

internal static class TemperatureParser
{
    // (M?dd|//)/(M?dd|//)? — 12/09, M05/M07, 17///, /////, 12/
    public static TemperaturePair? Parse(string s)
    {
        int i = 0;
        if (!TryReadPart(s, ref i, out int? temperature))
        {
            return null;
        }

        if (i >= s.Length || s[i] != '/')
        {
            return null;
        }

        i++;
        int? dewPoint = null;
        if (i < s.Length && !TryReadPart(s, ref i, out dewPoint))
        {
            return null;
        }

        return i == s.Length ? new TemperaturePair(temperature, dewPoint) : null;
    }

    private static bool TryReadPart(string s, ref int i, out int? value)
    {
        value = null;
        if (Scan.AreSlashes(s, i, 2))
        {
            i += 2;
            return true;
        }

        bool negative = i < s.Length && s[i] == 'M';
        int start = negative ? i + 1 : i;
        int? number = Scan.Number(s, start, 2);
        if (number is null)
        {
            return false;
        }

        value = negative ? -number.Value : number.Value;
        i = start + 2;
        return true;
    }
}
