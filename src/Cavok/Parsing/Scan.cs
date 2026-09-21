namespace Cavok.Parsing;

// Character helpers. Only ASCII digits count as digits; report text is never parsed with int.Parse.
internal static class Scan
{
    public static bool IsDigit(char c) => c >= '0' && c <= '9';

    public static bool IsLetter(char c) => c >= 'A' && c <= 'Z';

    public static bool AreDigits(string s, int start, int count)
    {
        if (start < 0 || count < 0 || start + count > s.Length)
        {
            return false;
        }

        for (int i = start; i < start + count; i++)
        {
            if (!IsDigit(s[i]))
            {
                return false;
            }
        }

        return true;
    }

    public static bool AreSlashes(string s, int start, int count)
    {
        if (start < 0 || count < 1 || start + count > s.Length)
        {
            return false;
        }

        for (int i = start; i < start + count; i++)
        {
            if (s[i] != '/')
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsAllSlashes(string s) => s.Length > 0 && AreSlashes(s, 0, s.Length);

    public static bool IsAlphanumeric(string s)
    {
        if (s.Length == 0)
        {
            return false;
        }

        foreach (char c in s)
        {
            if (!IsLetter(c) && !IsDigit(c))
            {
                return false;
            }
        }

        return true;
    }

    public static int CountDigits(string s)
    {
        int count = 0;
        foreach (char c in s)
        {
            if (IsDigit(c))
            {
                count++;
            }
        }

        return count;
    }

    // Reads `count` (1–9) ASCII digits at `start`; null when out of range or not all digits.
    public static int? Number(string s, int start, int count)
    {
        if (count < 1 || count > 9 || !AreDigits(s, start, count))
        {
            return null;
        }

        int value = 0;
        for (int i = start; i < start + count; i++)
        {
            value = (value * 10) + (s[i] - '0');
        }

        return value;
    }
}
