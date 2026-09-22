namespace Cavok;

/// <summary>Prevailing visibility, in metres (<c>0800</c>, <c>9999</c>) or statute miles (<c>1 1/2SM</c>).</summary>
public sealed record Visibility
{
    /// <summary>Visibility in metres when reported in metres. <c>9999</c> is stored as 10000 with <see cref="IsTenKmOrMore"/> set.</summary>
    public int? Meters { get; init; }

    /// <summary>Visibility in statute miles when reported in miles.</summary>
    public double? StatuteMiles { get; init; }

    /// <summary>True for <c>9999</c>: 10 km or more.</summary>
    public bool IsTenKmOrMore { get; init; }

    /// <summary>True for "less than" (<c>M1/4SM</c>).</summary>
    public bool IsLessThan { get; init; }

    /// <summary>True for "more than" (<c>P6SM</c>).</summary>
    public bool IsMoreThan { get; init; }

    /// <summary>True for <c>NDV</c> (no directional variation; automatic stations).</summary>
    public bool NoDirectionalVariation { get; init; }

    /// <summary>Minimum visibility with its direction (<c>1500SW</c>), if reported.</summary>
    public MinimumVisibility? Minimum { get; init; }

    /// <summary>True when the visibility is missing (<c>////</c>).</summary>
    public bool IsMissing { get; init; }

    internal const double MetersPerStatuteMile = 1609.344;

    /// <summary>The prevailing visibility in metres, converting statute miles.</summary>
    /// <returns>Metres, or <c>null</c> when no prevailing visibility is known.</returns>
    public double? ToMeters() => Meters ?? StatuteMiles * MetersPerStatuteMile;
}

/// <summary>Minimum visibility and the direction in which it is observed (<c>1500SW</c>).</summary>
/// <param name="Meters">Minimum visibility in metres.</param>
/// <param name="Direction">Direction of the minimum visibility, or <c>null</c> when not given.</param>
public sealed record MinimumVisibility(int Meters, CompassDirection? Direction);
