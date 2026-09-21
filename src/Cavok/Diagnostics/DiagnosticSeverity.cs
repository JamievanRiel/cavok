namespace Cavok;

/// <summary>How serious a <see cref="Diagnostic"/> is.</summary>
public enum DiagnosticSeverity
{
    /// <summary>The group was understood but looks suspicious; its value is used.</summary>
    Warning,

    /// <summary>A group could not be understood and was skipped, or a required group is missing.</summary>
    Error,
}
