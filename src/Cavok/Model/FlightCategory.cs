namespace Cavok;

/// <summary>
/// Flight category from ceiling and visibility using the FAA/AWC thresholds (there is no official European
/// equivalent). Higher values are worse.
/// </summary>
public enum FlightCategory
{
    /// <summary>Visual flight rules: ceiling above 3,000 ft and visibility above 5 statute miles.</summary>
    Vfr = 0,

    /// <summary>Marginal VFR: ceiling 1,000–3,000 ft and/or visibility 3–5 statute miles.</summary>
    Mvfr = 1,

    /// <summary>Instrument flight rules: ceiling 500 to below 1,000 ft and/or visibility 1 to below 3 statute miles.</summary>
    Ifr = 2,

    /// <summary>Low IFR: ceiling below 500 ft and/or visibility below 1 statute mile.</summary>
    Lifr = 3,
}
