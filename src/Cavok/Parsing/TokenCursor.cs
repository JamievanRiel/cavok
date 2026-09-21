namespace Cavok.Parsing;

// Walks the tokens of a report and remembers which ones were parsed successfully (consumed).
internal sealed class TokenCursor
{
    private readonly IReadOnlyList<Token> _tokens;
    private readonly bool[] _consumed;

    public TokenCursor(IReadOnlyList<Token> tokens)
    {
        _tokens = tokens;
        _consumed = new bool[tokens.Count];
    }

    public static TokenCursor Empty => new TokenCursor(Array.Empty<Token>());

    public int Index { get; private set; }

    public bool AtEnd => Index >= _tokens.Count;

    public Token Current => _tokens[Index];

    public IReadOnlyList<Token> Tokens => _tokens;

    public bool TryPeek(int offset, out Token token)
    {
        int index = Index + offset;
        if (index >= 0 && index < _tokens.Count)
        {
            token = _tokens[index];
            return true;
        }

        token = default;
        return false;
    }

    public string? PeekText(int offset = 0) => TryPeek(offset, out Token token) ? token.Text : null;

    // Marks `count` tokens as parsed and moves past them.
    public void Consume(int count = 1)
    {
        for (int i = 0; i < count && Index < _tokens.Count; i++)
        {
            _consumed[Index] = true;
            Index++;
        }
    }

    // Moves past the current token without marking it; the caller reports a diagnostic for it.
    public void Skip()
    {
        if (Index < _tokens.Count)
        {
            Index++;
        }
    }

    public void ConsumeRest() => Consume(_tokens.Count - Index);

    public bool IsConsumed(int index) => _consumed[index];

    public void MoveTo(int index) => Index = Math.Min(Math.Max(index, 0), _tokens.Count);
}
