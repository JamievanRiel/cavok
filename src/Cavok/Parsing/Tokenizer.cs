namespace Cavok.Parsing;

internal static class Tokenizer
{
    // Splits a report on any whitespace. Tokens are upper-cased (which never changes their length) and
    // a trailing '=' end-of-report marker is removed. Positions refer to the original string.
    public static List<Token> Tokenize(string raw)
    {
        var tokens = new List<Token>();
        int i = 0;
        while (i < raw.Length)
        {
            while (i < raw.Length && char.IsWhiteSpace(raw[i]))
            {
                i++;
            }

            int start = i;
            while (i < raw.Length && !char.IsWhiteSpace(raw[i]))
            {
                i++;
            }

            int end = i;
            while (end > start && raw[end - 1] == '=')
            {
                end--;
            }

            if (end > start)
            {
                tokens.Add(new Token(raw.Substring(start, end - start).ToUpperInvariant(), start));
            }
        }

        return tokens;
    }
}
