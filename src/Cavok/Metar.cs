using Cavok.Parsing;

namespace Cavok;

/// <summary>
/// A parsed METAR or SPECI report. Use <see cref="Parse(string)"/> to parse leniently or
/// <see cref="ParseStrict(string)"/> to throw on errors.
/// </summary>
public sealed record Metar
{
    /// <summary>The report exactly as given to the parser.</summary>
    public string Raw { get; init; } = "";

    /// <summary>METAR or SPECI. Reports without a type word are METARs.</summary>
    public ReportType Type { get; init; }

    /// <summary>ICAO location indicator, for example <c>EHAM</c>; <c>null</c> when missing or invalid.</summary>
    public string? Station { get; init; }

    /// <summary>Observation day and time (UTC).</summary>
    public DayTime? Time { get; init; }

    /// <summary>True for a fully automated report (<c>AUTO</c>).</summary>
    public bool IsAuto { get; init; }

    /// <summary>True for a corrected report (<c>COR</c>).</summary>
    public bool IsCorrected { get; init; }

    /// <summary>True for a missing report (<c>NIL</c>).</summary>
    public bool IsNil { get; init; }

    /// <summary>Surface wind.</summary>
    public Wind? Wind { get; init; }

    /// <summary>Prevailing visibility (not set when <see cref="IsCavok"/>).</summary>
    public Visibility? Visibility { get; init; }

    /// <summary>True for <c>CAVOK</c>.</summary>
    public bool IsCavok { get; init; }

    /// <summary>Runway visual ranges.</summary>
    public IReadOnlyList<RunwayVisualRange> RunwayVisualRanges { get; init; } = Array.Empty<RunwayVisualRange>();

    /// <summary>Present weather.</summary>
    public IReadOnlyList<WeatherPhenomenon> Weather { get; init; } = Array.Empty<WeatherPhenomenon>();

    /// <summary>Cloud layers and vertical visibility.</summary>
    public IReadOnlyList<CloudLayer> Clouds { get; init; } = Array.Empty<CloudLayer>();

    /// <summary><c>NSC</c>, <c>NCD</c>, <c>SKC</c> or <c>CLR</c>, if reported.</summary>
    public CloudCondition? CloudCondition { get; init; }

    /// <summary>Air temperature in °C.</summary>
    public int? Temperature { get; init; }

    /// <summary>Dew point in °C.</summary>
    public int? DewPoint { get; init; }

    /// <summary>QNH; <c>null</c> when missing (<c>Q////</c>) or not reported.</summary>
    public Pressure? Pressure { get; init; }

    /// <summary>Recent weather (<c>RE</c> groups).</summary>
    public IReadOnlyList<WeatherPhenomenon> RecentWeather { get; init; } = Array.Empty<WeatherPhenomenon>();

    /// <summary>Wind shear groups.</summary>
    public IReadOnlyList<WindShear> WindShear { get; init; } = Array.Empty<WindShear>();

    /// <summary>Sea-surface temperature and state of the sea or wave height.</summary>
    public SeaCondition? Sea { get; init; }

    /// <summary>Runway state groups.</summary>
    public IReadOnlyList<RunwayState> RunwayStates { get; init; } = Array.Empty<RunwayState>();

    /// <summary>Military colour states outside the trend (for example <c>BLU BLU</c>).</summary>
    public IReadOnlyList<ColorCode> ColorCodes { get; init; } = Array.Empty<ColorCode>();

    /// <summary>Trend forecasts (<c>NOSIG</c>, <c>BECMG</c>, <c>TEMPO</c>).</summary>
    public IReadOnlyList<Trend> Trends { get; init; } = Array.Empty<Trend>();

    /// <summary>Everything after <c>RMK</c>, unparsed; <c>null</c> when there are no remarks.</summary>
    public string? Remarks { get; init; }

    /// <summary>Problems found while parsing.</summary>
    public IReadOnlyList<Diagnostic> Diagnostics { get; init; } = Array.Empty<Diagnostic>();

    /// <summary>True when at least one diagnostic is an error.</summary>
    public bool HasErrors => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

    /// <summary>Flight category from the observed visibility and ceiling; <c>null</c> when neither is known.</summary>
    public FlightCategory? FlightCategory => FlightCategoryCalculator.Compute(Visibility, IsCavok, Clouds, CloudCondition);

    /// <summary>
    /// Parses a METAR or SPECI. Never throws for bad content: whatever is understood is returned and every
    /// problem is listed in <see cref="Diagnostics"/>.
    /// </summary>
    /// <param name="raw">The report, with or without the <c>METAR</c>/<c>SPECI</c> prefix.</param>
    /// <returns>The parsed report.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="raw"/> is <c>null</c>.</exception>
    public static Metar Parse(string raw)
    {
        if (raw is null)
        {
            throw new ArgumentNullException(nameof(raw));
        }

        return MetarParser.Parse(raw);
    }

    /// <summary>Parses a METAR or SPECI and throws when it contains errors; warnings are allowed.</summary>
    /// <param name="raw">The report, with or without the <c>METAR</c>/<c>SPECI</c> prefix.</param>
    /// <returns>The parsed report.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="raw"/> is <c>null</c>.</exception>
    /// <exception cref="CavokParseException">The report contains at least one error.</exception>
    public static Metar ParseStrict(string raw)
    {
        Metar metar = Parse(raw);
        if (metar.HasErrors)
        {
            throw new CavokParseException(raw, metar.Diagnostics);
        }

        return metar;
    }
}
