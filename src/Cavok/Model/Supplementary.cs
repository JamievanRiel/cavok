namespace Cavok;

/// <summary>Wind shear reported for a runway (<c>WS R24L</c>) or all runways (<c>WS ALL RWY</c>).</summary>
public sealed record WindShear
{
    /// <summary>True for <c>WS ALL RWY</c>.</summary>
    public bool AllRunways { get; init; }

    /// <summary>The runway designator, for example <c>24L</c>.</summary>
    public string? Runway { get; init; }
}

/// <summary>Sea-surface temperature with state of the sea (<c>W15/S4</c>) or significant wave height (<c>W18/H14</c>).</summary>
public sealed record SeaCondition
{
    /// <summary>Sea-surface temperature in °C; <c>null</c> when missing.</summary>
    public int? SeaTemperature { get; init; }

    /// <summary>State of the sea, WMO code 0 (calm) to 9 (phenomenal).</summary>
    public int? StateOfSea { get; init; }

    /// <summary>Significant wave height in decimetres.</summary>
    public int? WaveHeightDecimeters { get; init; }
}

/// <summary>
/// State of the runway (<c>R24/290050</c>, <c>R24/CLRD62</c>, <c>R/SNOCLO</c> or the legacy 8-digit form):
/// deposit, extent of contamination, depth and friction coefficient or braking action, as WMO codes.
/// </summary>
public sealed record RunwayState
{
    /// <summary>The runway designator; <c>null</c> for all runways or a repeated report.</summary>
    public string? Runway { get; init; }

    /// <summary>True for all runways (<c>R88</c> or <c>SNOCLO</c>).</summary>
    public bool AllRunways { get; init; }

    /// <summary>True when the previous report is repeated (<c>R99</c>).</summary>
    public bool IsRepeated { get; init; }

    /// <summary>True when the aerodrome or runway is closed due to snow (<c>SNOCLO</c>).</summary>
    public bool SnowClosed { get; init; }

    /// <summary>True when contamination has been cleared (<c>CLRD</c>).</summary>
    public bool Cleared { get; init; }

    /// <summary>Runway deposit, WMO code 0–9.</summary>
    public int? Deposit { get; init; }

    /// <summary>Extent of contamination, WMO code 1, 2, 5 or 9.</summary>
    public int? Extent { get; init; }

    /// <summary>Depth of deposit, WMO code 00–99.</summary>
    public int? Depth { get; init; }

    /// <summary>Friction coefficient (01–90) or braking action (91–95, 99).</summary>
    public int? Friction { get; init; }

    /// <summary>The group as it appears in the report.</summary>
    public string RawGroup { get; init; } = "";
}

/// <summary>Military aerodrome colour state.</summary>
public enum ColorState
{
    /// <summary>Blue plus (<c>BLU+</c>).</summary>
    BluePlus,

    /// <summary>Blue (<c>BLU</c>).</summary>
    Blue,

    /// <summary>White (<c>WHT</c>).</summary>
    White,

    /// <summary>Green (<c>GRN</c>).</summary>
    Green,

    /// <summary>Yellow 1 (<c>YLO1</c>).</summary>
    Yellow1,

    /// <summary>Yellow 2 (<c>YLO2</c>).</summary>
    Yellow2,

    /// <summary>Yellow (<c>YLO</c>).</summary>
    Yellow,

    /// <summary>Amber (<c>AMB</c>).</summary>
    Amber,

    /// <summary>Red (<c>RED</c>).</summary>
    Red,
}

/// <summary>A military colour state such as <c>BLU</c> or <c>BLACKBLU+</c>.</summary>
/// <param name="State">The colour state.</param>
/// <param name="IsBlack">True with the <c>BLACK</c> prefix: the aerodrome is unusable for reasons other than weather.</param>
public readonly record struct ColorCode(ColorState State, bool IsBlack);
