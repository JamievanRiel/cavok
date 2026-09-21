namespace Cavok.Parsing;

internal static class RunwayStateParser
{
    // R24/290050, R24L//////, R24/CLRD62, R24/SNOCLO, R/SNOCLO, SNOCLO and the legacy 8-digit form 24290050.
    public static RunwayState? Parse(string s)
    {
        if (s == "SNOCLO" || s == "R/SNOCLO")
        {
            return new RunwayState { AllRunways = true, SnowClosed = true, RawGroup = s };
        }

        if (s.Length == 8 && Scan.AreDigits(s, 0, 2) && IsStateBody(s, 2))
        {
            return FromBody(s.Substring(0, 2), s, 2);
        }

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
        if (rest == "SNOCLO")
        {
            return WithRunway(runway, new RunwayState { SnowClosed = true, RawGroup = s });
        }

        if (rest.Length == 6 && rest.StartsWith("CLRD", StringComparison.Ordinal)
            && (Scan.AreDigits(rest, 4, 2) || Scan.AreSlashes(rest, 4, 2)))
        {
            return WithRunway(runway, new RunwayState { Cleared = true, Friction = Scan.Number(rest, 4, 2), RawGroup = s });
        }

        return IsStateBody(s, slash + 1) ? FromBody(runway, s, slash + 1) : null;
    }

    // Exactly six characters from `start` to the end, each a digit or '/'.
    private static bool IsStateBody(string s, int start)
    {
        if (start + 6 != s.Length)
        {
            return false;
        }

        for (int i = start; i < s.Length; i++)
        {
            if (!Scan.IsDigit(s[i]) && s[i] != '/')
            {
                return false;
            }
        }

        return true;
    }

    private static RunwayState FromBody(string designator, string raw, int start) =>
        WithRunway(designator, new RunwayState
        {
            Deposit = Scan.Number(raw, start, 1),
            Extent = Scan.Number(raw, start + 1, 1),
            Depth = Scan.Number(raw, start + 2, 2),
            Friction = Scan.Number(raw, start + 4, 2),
            RawGroup = raw,
        });

    private static RunwayState WithRunway(string designator, RunwayState state)
    {
        if (designator.StartsWith("88", StringComparison.Ordinal))
        {
            return state with { AllRunways = true };
        }

        if (designator.StartsWith("99", StringComparison.Ordinal))
        {
            return state with { IsRepeated = true };
        }

        return state with { Runway = designator };
    }
}
