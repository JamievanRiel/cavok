namespace Cavok.Parsing;

// Chooses the most specific error code for a token that no group parser accepted.
internal static class GroupGuesser
{
    public static DiagnosticCode Guess(string t, bool taf = false)
    {
        if (t.EndsWith("KT", StringComparison.Ordinal) || t.EndsWith("MPS", StringComparison.Ordinal)
            || t.EndsWith("KMH", StringComparison.Ordinal))
        {
            return DiagnosticCode.InvalidWind;
        }

        if (t.Length == 7 && t[3] == 'V')
        {
            return DiagnosticCode.InvalidWindVariation;
        }

        int slash = t.IndexOf('/');
        if (t.Length >= 4 && t[0] == 'R' && Scan.IsDigit(t[1]) && slash > 0)
        {
            string rest = t.Substring(slash + 1);
            return rest.Length == 6 || rest.StartsWith("CLRD", StringComparison.Ordinal)
                ? DiagnosticCode.InvalidRunwayState
                : DiagnosticCode.InvalidRvr;
        }

        if (StartsWithAny(t, "FEW", "SCT", "BKN", "OVC", "VV"))
        {
            return DiagnosticCode.InvalidCloud;
        }

        if (StartsWithAny(t, "TX", "TN"))
        {
            return DiagnosticCode.InvalidTemperatureForecast;
        }

        if (t.StartsWith("PROB", StringComparison.Ordinal) || t == "INTER"
            || (t.StartsWith("FM", StringComparison.Ordinal) && t.Length >= 7))
        {
            return DiagnosticCode.InvalidChangeGroup;
        }

        if (TimeParsers.IsTrendTimeShape(t))
        {
            return DiagnosticCode.InvalidTrend;
        }

        if (t.Length == 5 && (t[0] == 'Q' || t[0] == 'A'))
        {
            return DiagnosticCode.InvalidPressure;
        }

        if (TimeParsers.LooksLikeDayTime(t))
        {
            return DiagnosticCode.InvalidTime;
        }

        if (TimeParsers.LooksLikePeriod(t))
        {
            return !taf && t.Length == 6 ? DiagnosticCode.InvalidTime : DiagnosticCode.InvalidValidity;
        }

        if (slash > 0 && t.Length <= 7 && (Scan.IsDigit(t[0]) || t[0] == 'M'))
        {
            return DiagnosticCode.InvalidTemperature;
        }

        if ((t.Length == 4 || t.Length == 5) && Scan.IsDigit(t[0]) && Scan.CountDigits(t) >= 3)
        {
            return DiagnosticCode.InvalidVisibility;
        }

        if (t.EndsWith("SM", StringComparison.Ordinal))
        {
            return DiagnosticCode.InvalidVisibility;
        }

        if (t[0] == '-' || t[0] == '+' || StartsWithAny(t, "VC", "RE"))
        {
            return DiagnosticCode.InvalidWeather;
        }

        return DiagnosticCode.UnknownGroup;
    }

    private static bool StartsWithAny(string t, params string[] prefixes)
    {
        foreach (string prefix in prefixes)
        {
            if (t.StartsWith(prefix, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
