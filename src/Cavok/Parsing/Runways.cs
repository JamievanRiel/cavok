namespace Cavok.Parsing;

internal static class Runways
{
    // Two digits optionally followed by L, C or R (24, 06L). 88 and 99 are used by runway state groups.
    public static bool IsDesignator(string s) =>
        (s.Length == 2 || s.Length == 3)
        && Scan.AreDigits(s, 0, 2)
        && (s.Length == 2 || s[2] == 'L' || s[2] == 'C' || s[2] == 'R');
}
