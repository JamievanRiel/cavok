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
        SpeedUnit.MetersPerSecond => Value * UnitConversions.KnotsPerMeterPerSecond,
        SpeedUnit.KilometersPerHour => Value / UnitConversions.KilometersPerHourPerKnot,
        _ => Value,
    };

    /// <summary>The speed converted to metres per second.</summary>
    /// <returns>The speed in metres per second.</returns>
    public double ToMetersPerSecond() => Unit switch
    {
        SpeedUnit.Knots => Value * UnitConversions.MetersPerSecondPerKnot,
        SpeedUnit.KilometersPerHour => Value / 3.6,
        _ => Value,
    };

    /// <summary>The speed converted to kilometres per hour.</summary>
    /// <returns>The speed in kilometres per hour.</returns>
    public double ToKilometersPerHour() => Unit switch
    {
        SpeedUnit.Knots => Value * UnitConversions.KilometersPerHourPerKnot,
        SpeedUnit.MetersPerSecond => Value * 3.6,
        _ => Value,
    };
}

/// <summary>Conversion factors shared by the model types.</summary>
internal static class UnitConversions
{
    internal const double MetersPerFoot = 0.3048;
    internal const double MetersPerSecondPerKnot = 0.514444;
    internal const double KnotsPerMeterPerSecond = 1.943844;
    internal const double KilometersPerHourPerKnot = 1.852;
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
