namespace Cavok;

/// <summary>Kind of a TAF change group.</summary>
public enum TafChangeKind
{
    /// <summary>From a moment on, all conditions change (<c>FM211400</c>).</summary>
    From,

    /// <summary>Conditions change gradually during the period (<c>BECMG</c>).</summary>
    Becoming,

    /// <summary>Temporary fluctuations during the period (<c>TEMPO</c>, also <c>PROB30 TEMPO</c>).</summary>
    Temporary,

    /// <summary>A probability of the conditions during the period (<c>PROB30</c>, <c>PROB40</c>).</summary>
    Probability,
}

/// <summary>A TAF change group with its forecast conditions.</summary>
public sealed record TafChange
{
    /// <summary>The kind of change.</summary>
    public TafChangeKind Kind { get; init; }

    /// <summary>Probability in percent (30 or 40) for <c>PROB</c> groups, also combined with <c>TEMPO</c>.</summary>
    public int? Probability { get; init; }

    /// <summary>Start of an <c>FM</c> group.</summary>
    public DayTime? From { get; init; }

    /// <summary>Period of a <c>BECMG</c>, <c>TEMPO</c> or <c>PROB</c> group.</summary>
    public ValidityPeriod? Period { get; init; }

    /// <summary>The forecast conditions in this group.</summary>
    public ForecastConditions Conditions { get; init; } = new ForecastConditions();
}
