namespace Cavok.Parsing;

internal static class Remarks
{
    // Everything after the RMK token, whitespace collapsed and a trailing '=' removed; null when empty.
    public static string? After(string raw, Token rmk)
    {
        int start = rmk.Position + rmk.Length;
        if (start >= raw.Length)
        {
            return null;
        }

        string[] words = raw.Substring(start).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        string text = string.Join(" ", words).TrimEnd('=').TrimEnd();
        return text.Length == 0 ? null : text;
    }
}
