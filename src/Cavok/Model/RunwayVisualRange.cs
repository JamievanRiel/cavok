namespace Cavok;

/// <summary>Marks a runway visual range value as a bound.</summary>
public enum RvrQualifier
{
    /// <summary>More than the value (<c>P</c>).</summary>
    Above,

    /// <summary>Less than the value (<c>M</c>).</summary>
    Below,
}

/// <summary>Tendency of the runway visual range.</summary>
public enum RvrTendency
{
    /// <summary>Increasing (<c>U</c>).</summary>
    Up,

    /// <summary>Decreasing (<c>D</c>).</summary>
    Down,

    /// <summary>No distinct change (<c>N</c>).</summary>
    NoChange,
}

/// <summary>Runway visual range such as <c>R24/P2000N</c> or <c>R06L/0800V1200U</c>.</summary>
public sealed record RunwayVisualRange
{
    /// <summary>Runway designator, for example <c>24</c> or <c>06L</c>.</summary>
    public string Runway { get; init; } = "";

    /// <summary>The (minimum) visual range; <c>null</c> when missing.</summary>
    public int? Value { get; init; }

    /// <summary>Qualifier of <see cref="Value"/>.</summary>
    public RvrQualifier? Qualifier { get; init; }

    /// <summary>The maximum visual range when the range varies (<c>0800V1200</c>).</summary>
    public int? VariableMax { get; init; }

    /// <summary>Qualifier of <see cref="VariableMax"/>.</summary>
    public RvrQualifier? VariableMaxQualifier { get; init; }

    /// <summary>The tendency, if reported.</summary>
    public RvrTendency? Tendency { get; init; }

    /// <summary>True when the values are in feet (<c>FT</c> suffix) instead of metres.</summary>
    public bool IsFeet { get; init; }

    /// <summary>True when the value is missing (<c>R24/////</c>).</summary>
    public bool IsMissing { get; init; }
}
