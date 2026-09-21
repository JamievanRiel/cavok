namespace Cavok.Parsing;

internal sealed class DiagnosticBag
{
    private readonly List<Diagnostic> _items = new List<Diagnostic>();
    private readonly string _source;

    public DiagnosticBag(string source)
    {
        _source = source;
    }

    public IReadOnlyList<Diagnostic> Items => _items;

    public bool HasErrors { get; private set; }

    public int SourceLength => _source.Length;

    public void Error(DiagnosticCode code, Token token) =>
        Add(DiagnosticSeverity.Error, code, token.Position, token.Length);

    public void Error(DiagnosticCode code, Token first, Token last) =>
        Add(DiagnosticSeverity.Error, code, first.Position, last.Position + last.Length - first.Position);

    public void Warning(DiagnosticCode code, Token token) =>
        Add(DiagnosticSeverity.Warning, code, token.Position, token.Length);

    public void Warning(DiagnosticCode code, Token first, Token last) =>
        Add(DiagnosticSeverity.Warning, code, first.Position, last.Position + last.Length - first.Position);

    // A zero-length error, used when something is missing or the whole input is rejected.
    public void ErrorAt(DiagnosticCode code, int position) =>
        Add(DiagnosticSeverity.Error, code, Math.Min(Math.Max(position, 0), _source.Length), 0);

    public void InternalError(TokenCursor cursor)
    {
        if (cursor.AtEnd)
        {
            ErrorAt(DiagnosticCode.InternalError, _source.Length);
        }
        else
        {
            Error(DiagnosticCode.InternalError, cursor.Current);
        }
    }

    private void Add(DiagnosticSeverity severity, DiagnosticCode code, int position, int length)
    {
        string token = length > 0 ? _source.Substring(position, length) : "";
        _items.Add(new Diagnostic(severity, code, token, position, length, _source));
        if (severity == DiagnosticSeverity.Error)
        {
            HasErrors = true;
        }
    }
}
