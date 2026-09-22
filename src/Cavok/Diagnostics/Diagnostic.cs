using System.Globalization;
using System.Text;
using Cavok.Text;

namespace Cavok;

/// <summary>
/// A problem found while parsing: a group that could not be understood (<see cref="DiagnosticSeverity.Error"/>)
/// or a group that was understood but looks suspicious (<see cref="DiagnosticSeverity.Warning"/>).
/// </summary>
public sealed record Diagnostic
{
    internal Diagnostic(DiagnosticSeverity severity, DiagnosticCode code, string token, int position, int length, string source)
    {
        Severity = severity;
        Code = code;
        Token = token;
        Position = position;
        Length = length;
        Source = source;
    }

    /// <summary>Whether this is an error or a warning.</summary>
    public DiagnosticSeverity Severity { get; }

    /// <summary>The kind of problem.</summary>
    public DiagnosticCode Code { get; }

    /// <summary>Stable identifier, for example <c>CAV004</c>.</summary>
    public string Id => "CAV" + ((int)Code).ToString("000", CultureInfo.InvariantCulture);

    /// <summary>The offending text as it appears in the report; empty when something is missing.</summary>
    public string Token { get; }

    /// <summary>Zero-based character index of <see cref="Token"/> in the report.</summary>
    public int Position { get; }

    /// <summary>Length of <see cref="Token"/> in characters.</summary>
    public int Length { get; }

    /// <summary>English description of the problem.</summary>
    public string Message => Describe(Language.English);

    internal string Source { get; }

    /// <summary>Describes the problem in the given language.</summary>
    /// <param name="language">The language of the description.</param>
    /// <returns>A one-line description; control characters in the quoted token are shown as spaces.</returns>
    public string Describe(Language language) => DiagnosticMessages.Format(Code, Flatten(Token), language);

    /// <summary>Formats the diagnostic with the report and a caret line pointing at the offending group.</summary>
    /// <returns>Three lines separated by <c>\n</c>, or one line when the report is empty.</returns>
    public override string ToString()
    {
        var text = new StringBuilder();
        text.Append(Severity == DiagnosticSeverity.Error ? "error " : "warning ")
            .Append(Id)
            .Append(": ")
            .Append(Message);
        if (Source.Length == 0)
        {
            return text.ToString();
        }

        const int context = 40;
        int position = Math.Min(Position, Source.Length);
        int start = Math.Max(0, position - context);
        int end = Math.Min(Source.Length, position + Length + context);
        string prefix = start > 0 ? "..." : "";
        string suffix = end < Source.Length ? "..." : "";
        text.Append("\n  ")
            .Append(prefix)
            .Append(Flatten(Source.Substring(start, end - start)))
            .Append(suffix)
            .Append("\n  ")
            .Append(' ', prefix.Length + position - start)
            .Append('^', Math.Max(1, Length));
        return text.ToString();
    }

    // Control characters (line breaks, ESC sequences) become spaces, so they cannot break or restyle the output.
    private static string Flatten(string text)
    {
        char[] chars = text.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (char.IsControl(chars[i]))
            {
                chars[i] = ' ';
            }
        }

        return new string(chars);
    }
}
