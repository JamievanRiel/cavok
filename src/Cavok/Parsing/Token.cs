namespace Cavok.Parsing;

// A whitespace-separated group. Text is upper-cased; Position is its index in the original report.
internal readonly struct Token
{
    public Token(string text, int position)
    {
        Text = text;
        Position = position;
    }

    public string Text { get; }

    public int Position { get; }

    public int Length => Text.Length;

    public override string ToString() => Text;
}
