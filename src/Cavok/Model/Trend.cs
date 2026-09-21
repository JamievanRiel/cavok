namespace Cavok;

/// <summary>Kind of a METAR trend forecast.</summary>
public enum TrendKind
{
    /// <summary>No significant change expected (<c>NOSIG</c>).</summary>
    NoSignificantChange,

    /// <summary>Conditions become (<c>BECMG</c>).</summary>
    Becoming,

    /// <summary>Temporary fluctuations (<c>TEMPO</c>).</summary>
    Temporary,
}

/// <summary>A METAR trend forecast such as <c>NOSIG</c>, <c>TEMPO 4000 SHRA</c> or <c>BECMG FM1030 30015KT</c>.</summary>
public sealed record Trend
{
    /// <summary>The kind of trend.</summary>
    public TrendKind Kind { get; init; }

    /// <summary>Start time (<c>FM</c>).</summary>
    public TimeOfDay? From { get; init; }

    /// <summary>End time (<c>TL</c>).</summary>
    public TimeOfDay? Until { get; init; }

    /// <summary>Time at which the change is expected (<c>AT</c>).</summary>
    public TimeOfDay? At { get; init; }

    /// <summary>The forecast conditions; <c>null</c> for <c>NOSIG</c>.</summary>
    public ForecastConditions? Conditions { get; init; }
}
