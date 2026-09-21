namespace Cavok;

/// <summary>The kind of problem a <see cref="Diagnostic"/> reports. The numbers are stable (<c>CAV001</c> …).</summary>
public enum DiagnosticCode
{
    /// <summary>A group that matches no known format.</summary>
    UnknownGroup = 1,

    /// <summary>The station identifier is not four letters or digits.</summary>
    InvalidStation = 2,

    /// <summary>The observation or issue time is not in <c>DDHHMMZ</c> format.</summary>
    InvalidTime = 3,

    /// <summary>A visibility group is malformed.</summary>
    InvalidVisibility = 4,

    /// <summary>A wind group is malformed.</summary>
    InvalidWind = 5,

    /// <summary>A wind direction variation group (<c>200V280</c>) is malformed or unusable.</summary>
    InvalidWindVariation = 6,

    /// <summary>A runway visual range group is malformed.</summary>
    InvalidRvr = 7,

    /// <summary>A present or recent weather group is malformed or not allowed here.</summary>
    InvalidWeather = 8,

    /// <summary>A cloud group is malformed.</summary>
    InvalidCloud = 9,

    /// <summary>A temperature/dew point group is malformed.</summary>
    InvalidTemperature = 10,

    /// <summary>A pressure group is malformed.</summary>
    InvalidPressure = 11,

    /// <summary>A runway state group is malformed.</summary>
    InvalidRunwayState = 12,

    /// <summary>A trend time group (<c>FM</c>/<c>TL</c>/<c>AT</c>) is malformed.</summary>
    InvalidTrend = 13,

    /// <summary>A validity period (<c>DDHH/DDHH</c>) is malformed.</summary>
    InvalidValidity = 14,

    /// <summary>A TAF change group (<c>FM</c>, <c>BECMG</c>, <c>TEMPO</c>, <c>PROB</c>) is malformed or incomplete.</summary>
    InvalidChangeGroup = 15,

    /// <summary>A TAF temperature forecast (<c>TX</c>/<c>TN</c>) is malformed.</summary>
    InvalidTemperatureForecast = 16,

    /// <summary>A TAF was given to the METAR parser or the other way round.</summary>
    WrongReportType = 17,

    /// <summary>The station identifier is missing.</summary>
    MissingStation = 18,

    /// <summary>The observation or issue time is missing.</summary>
    MissingTime = 19,

    /// <summary>The TAF validity period is missing.</summary>
    MissingValidity = 20,

    /// <summary>A recognised group appears later than the ICAO order allows.</summary>
    OutOfOrder = 21,

    /// <summary>A group appears more than once where only one is allowed; the first is used.</summary>
    Duplicate = 22,

    /// <summary>The gust speed is not higher than the mean wind speed.</summary>
    GustNotAboveSpeed = 23,

    /// <summary>The dew point is higher than the temperature.</summary>
    DewPointAboveTemperature = 24,

    /// <summary>The input contains no groups.</summary>
    EmptyInput = 25,

    /// <summary>The input is longer than 65,536 characters and was not parsed.</summary>
    InputTooLong = 26,

    /// <summary>An unexpected internal error; please report the input.</summary>
    InternalError = 27,
}
