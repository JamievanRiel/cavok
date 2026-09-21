namespace Cavok;

/// <summary>Unit of a wind speed.</summary>
public enum SpeedUnit
{
    /// <summary>Knots (<c>KT</c>).</summary>
    Knots,

    /// <summary>Metres per second (<c>MPS</c>).</summary>
    MetersPerSecond,

    /// <summary>Kilometres per hour (<c>KMH</c>).</summary>
    KilometersPerHour,
}

/// <summary>A wind speed as reported.</summary>
/// <param name="Value">The reported value.</param>
/// <param name="Unit">The unit of the value.</param>
/// <param name="IsAbove">True when reported as "more than" (<c>P49MPS</c>, <c>P99KT</c>).</param>
public readonly record struct Speed(int Value, SpeedUnit Unit, bool IsAbove = false)
{
    /// <summary>The speed converted to knots.</summary>
    /// <returns>The speed in knots.</returns>
    public double ToKnots() => Unit switch
    {
        SpeedUnit.MetersPerSecond => Value * 1.943844,
        SpeedUnit.KilometersPerHour => Value / 1.852,
        _ => Value,
    };
}

/// <summary>Unit of a pressure value.</summary>
public enum PressureUnit
{
    /// <summary>Hectopascals (<c>Q1013</c>).</summary>
    Hectopascals,

    /// <summary>Inches of mercury (<c>A2992</c>, <c>QNH2992INS</c>).</summary>
    InchesOfMercury,
}

/// <summary>An altimeter setting (QNH) as reported.</summary>
/// <param name="Value">The reported value.</param>
/// <param name="Unit">The unit of the value.</param>
public readonly record struct Pressure(double Value, PressureUnit Unit)
{
    private const double HectopascalsPerInchOfMercury = 33.8639;

    /// <summary>The pressure in hectopascals.</summary>
    public double Hectopascals => Unit == PressureUnit.Hectopascals ? Value : Value * HectopascalsPerInchOfMercury;

    /// <summary>The pressure in inches of mercury.</summary>
    public double InchesOfMercury => Unit == PressureUnit.InchesOfMercury ? Value : Value / HectopascalsPerInchOfMercury;
}

/// <summary>An eight-point compass direction.</summary>
public enum CompassDirection
{
    /// <summary>North (<c>N</c>).</summary>
    North,

    /// <summary>North-east (<c>NE</c>).</summary>
    NorthEast,

    /// <summary>East (<c>E</c>).</summary>
    East,

    /// <summary>South-east (<c>SE</c>).</summary>
    SouthEast,

    /// <summary>South (<c>S</c>).</summary>
    South,

    /// <summary>South-west (<c>SW</c>).</summary>
    SouthWest,

    /// <summary>West (<c>W</c>).</summary>
    West,

    /// <summary>North-west (<c>NW</c>).</summary>
    NorthWest,
}
