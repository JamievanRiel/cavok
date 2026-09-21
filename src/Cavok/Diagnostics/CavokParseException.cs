namespace Cavok;

/// <summary>Thrown by the <c>ParseStrict</c> methods when a report contains at least one error.</summary>
public sealed class CavokParseException : FormatException
{
    internal CavokParseException(string raw, IReadOnlyList<Diagnostic> diagnostics)
        : base(BuildMessage(diagnostics))
    {
        Raw = raw;
        Diagnostics = diagnostics;
    }

    /// <summary>The report that failed to parse.</summary>
    public string Raw { get; }

    /// <summary>All diagnostics for the report, including warnings.</summary>
    public IReadOnlyList<Diagnostic> Diagnostics { get; }

    private static string BuildMessage(IReadOnlyList<Diagnostic> diagnostics)
    {
        Diagnostic[] errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length == 0)
        {
            return "The report could not be parsed.";
        }

        return errors.Length == 1
            ? "The report contains an error:\n" + errors[0]
            : "The report contains " + errors.Length.ToString(System.Globalization.CultureInfo.InvariantCulture)
              + " errors; the first is:\n" + errors[0];
    }
}
