namespace Cavok.Parsing;

internal static class Remarks
{
    // Everything after the RMK token, whitespace collapsed and a trailing '=' removed; null when empty.
    public static string? After(string raw, Token rmk) => Text(raw, rmk.Position + rmk.Length);

    // Everything from the token on, for a plain-language statement that is kept as a remark.
    public static string? From(string raw, Token first) => Text(raw, first.Position);

    private static string? Text(string raw, int start)
    {
        if (start >= raw.Length)
        {
            return null;
        }

        string[] words = raw.Substring(start).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        string text = string.Join(" ", words).TrimEnd('=').TrimEnd();
        return text.Length == 0 ? null : text;
    }
}
