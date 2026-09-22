using Cavok.Parsing;
using Cavok.Text;

namespace Cavok;

/// <summary>
/// A parsed terminal aerodrome forecast. Use <see cref="Parse(string)"/> to parse leniently or
/// <see cref="ParseStrict(string)"/> to throw on errors.
/// </summary>
public sealed record Taf
{
    // The lists are held as EquatableArray so that the record compares them by content.
    private readonly EquatableArray<TafChange> _changes = EquatableArray<TafChange>.Empty;
    private readonly EquatableArray<TemperatureForecast> _temperatures = EquatableArray<TemperatureForecast>.Empty;
    private readonly EquatableArray<Diagnostic> _diagnostics = EquatableArray<Diagnostic>.Empty;

    /// <summary>The forecast exactly as given to the parser.</summary>
    public string Raw { get; init; } = "";

    /// <summary>ICAO location indicator; <c>null</c> when missing or invalid.</summary>
    public string? Station { get; init; }

    /// <summary>Issue day and time (UTC).</summary>
    public DayTime? IssueTime { get; init; }

    /// <summary>True for an amended forecast (<c>AMD</c>).</summary>
    public bool IsAmended { get; init; }

    /// <summary>True for a corrected forecast (<c>COR</c>).</summary>
    public bool IsCorrected { get; init; }

    /// <summary>True when the forecast is cancelled (<c>CNL</c>).</summary>
    public bool IsCancelled { get; init; }

    /// <summary>True for a missing forecast (<c>NIL</c>).</summary>
    public bool IsNil { get; init; }

    /// <summary>Validity period.</summary>
    public ValidityPeriod? Validity { get; init; }

    /// <summary>The initial forecast conditions.</summary>
    public ForecastConditions Base { get; init; } = new ForecastConditions();

    /// <summary>Change groups in order.</summary>
    public IReadOnlyList<TafChange> Changes
    {
        get => _changes;
        init => _changes = EquatableArray.From(value);
    }

    /// <summary>Maximum and minimum temperature forecasts (<c>TX</c>/<c>TN</c>).</summary>
    public IReadOnlyList<TemperatureForecast> Temperatures
    {
        get => _temperatures;
        init => _temperatures = EquatableArray.From(value);
    }

    /// <summary>
    /// Everything after <c>RMK</c> as text, not decoded; also the closing statement of US military forecasts such as
    /// <c>LAST NO AMDS AFT 2020 NEXT 2104</c>. Runs of whitespace (including line breaks) become one space and a
    /// trailing <c>=</c> is removed; <c>null</c> when there are no remarks.
    /// </summary>
    public string? Remarks { get; init; }

    /// <summary>Problems found while parsing.</summary>
    public IReadOnlyList<Diagnostic> Diagnostics
    {
        get => _diagnostics;
        init => _diagnostics = EquatableArray.From(value);
    }

    /// <summary>True when at least one diagnostic is an error.</summary>
    public bool HasErrors => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

    /// <summary>
    /// Parses a TAF. Never throws for bad content: whatever is understood is returned and every problem is
    /// listed in <see cref="Diagnostics"/>.
    /// </summary>
    /// <param name="raw">The forecast, with or without the <c>TAF</c> prefix; line breaks are allowed.</param>
    /// <returns>The parsed forecast.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="raw"/> is <c>null</c>.</exception>
    public static Taf Parse(string raw)
    {
        if (raw is null)
        {
            throw new ArgumentNullException(nameof(raw));
        }

        return TafParser.Parse(raw);
    }

    /// <summary>Parses a TAF and throws when it contains errors; warnings are allowed.</summary>
    /// <param name="raw">The forecast, with or without the <c>TAF</c> prefix.</param>
    /// <returns>The parsed forecast.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="raw"/> is <c>null</c>.</exception>
    /// <exception cref="CavokParseException">The forecast contains at least one error.</exception>
    public static Taf ParseStrict(string raw)
    {
        Taf taf = Parse(raw);
        if (taf.HasErrors)
        {
            throw new CavokParseException(raw, taf.Diagnostics);
        }

        return taf;
    }

    /// <summary>Describes the forecast in plain language: the base conditions and one block per change group.</summary>
    /// <param name="language">The language of the description.</param>
    /// <returns>The description; lines are separated by <c>\n</c>.</returns>
    public string Describe(Language language = Language.English) => Describer.Describe(this, language);
}
