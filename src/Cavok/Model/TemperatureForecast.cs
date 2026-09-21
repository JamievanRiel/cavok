namespace Cavok;

/// <summary>Whether a TAF temperature forecast is a maximum or a minimum.</summary>
public enum TemperatureKind
{
    /// <summary>Maximum temperature (<c>TX</c>).</summary>
    Maximum,

    /// <summary>Minimum temperature (<c>TN</c>).</summary>
    Minimum,
}

/// <summary>A TAF temperature forecast such as <c>TX15/2114Z</c> or <c>TNM02/2205Z</c>.</summary>
/// <param name="Kind">Maximum or minimum.</param>
/// <param name="Celsius">The forecast temperature in °C.</param>
/// <param name="Time">When the temperature is expected.</param>
public sealed record TemperatureForecast(TemperatureKind Kind, int Celsius, DayHour Time);
