namespace Cavok.Parsing;

internal static class RvrParser
{
    // R24/P2000N, R06L/0800V1200U, R01L/0600V1000FT, R24/////, R24/1100/D
    public static RunwayVisualRange? Parse(string s)
    {
        if (s.Length < 5 || s[0] != 'R')
        {
            return null;
        }

        int slash = s.IndexOf('/');
        if (slash < 3 || slash > 4)
        {
            return null;
        }

        string runway = s.Substring(1, slash - 1);
        if (!Runways.IsDesignator(runway))
        {
            return null;
        }

        string rest = s.Substring(slash + 1);
        var rvr = new RunwayVisualRange { Runway = runway };
        int i = 0;
        if (Scan.AreSlashes(rest, 0, 4))
        {
            while (i < rest.Length && rest[i] == '/')
            {
                i++;
            }

            rvr = rvr with { IsMissing = true };
        }
        else
        {
            if (!TryReadValue(rest, ref i, out RvrQualifier? qualifier, out int value))
            {
                return null;
            }

            rvr = rvr with { Qualifier = qualifier, Value = value };
            if (i < rest.Length && rest[i] == 'V')
            {
                i++;
                if (!TryReadValue(rest, ref i, out RvrQualifier? maxQualifier, out int max))
                {
                    return null;
                }

                rvr = rvr with { VariableMaxQualifier = maxQualifier, VariableMax = max };
            }

            if (i + 2 <= rest.Length && rest[i] == 'F' && rest[i + 1] == 'T')
            {
                rvr = rvr with { IsFeet = true };
                i += 2;
            }
        }

        if (i < rest.Length && rest[i] == '/')
        {
            i++;
        }

        if (i < rest.Length)
        {
            RvrTendency? tendency = rest[i] switch
            {
                'U' => RvrTendency.Up,
                'D' => RvrTendency.Down,
                'N' => RvrTendency.NoChange,
                _ => null,
            };
            if (tendency is null)
            {
                return null;
            }

            rvr = rvr with { Tendency = tendency };
            i++;
        }

        return i == rest.Length ? rvr : null;
    }

    private static bool TryReadValue(string s, ref int i, out RvrQualifier? qualifier, out int value)
    {
        qualifier = null;
        value = 0;
        if (i < s.Length && s[i] == 'P')
        {
            qualifier = RvrQualifier.Above;
            i++;
        }
        else if (i < s.Length && s[i] == 'M')
        {
            qualifier = RvrQualifier.Below;
            i++;
        }

        int? number = Scan.Number(s, i, 4);
        if (number is null)
        {
            return false;
        }

        value = number.Value;
        i += 4;
        return true;
    }
}
