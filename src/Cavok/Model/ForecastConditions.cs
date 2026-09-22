using Cavok.Parsing;

namespace Cavok;

/// <summary>
/// Wind, visibility, weather and cloud as observed or forecast in one block: a METAR trend, or the base or a
/// change group of a TAF. Elements that are not mentioned are absent.
/// </summary>
public sealed record ForecastConditions
{
    // The lists are held as EquatableArray so that the record compares them by content.
    private readonly EquatableArray<WeatherPhenomenon> _weather = EquatableArray<WeatherPhenomenon>.Empty;
    private readonly EquatableArray<CloudLayer> _clouds = EquatableArray<CloudLayer>.Empty;
    private readonly EquatableArray<ColorCode> _colorCodes = EquatableArray<ColorCode>.Empty;
    private readonly EquatableArray<IcingLayer> _icing = EquatableArray<IcingLayer>.Empty;
    private readonly EquatableArray<TurbulenceLayer> _turbulence = EquatableArray<TurbulenceLayer>.Empty;

    /// <summary>Wind, if given.</summary>
    public Wind? Wind { get; init; }

    /// <summary>Prevailing visibility, if given (not set when <see cref="IsCavok"/>).</summary>
    public Visibility? Visibility { get; init; }

    /// <summary>True for <c>CAVOK</c>: visibility 10 km or more, no cloud below 5,000 ft, no significant weather.</summary>
    public bool IsCavok { get; init; }

    /// <summary>Weather phenomena.</summary>
    public IReadOnlyList<WeatherPhenomenon> Weather
    {
        get => _weather;
        init => _weather = EquatableArray.From(value);
    }

    /// <summary>True for <c>NSW</c>: the significant weather has ended.</summary>
    public bool NoSignificantWeather { get; init; }

    /// <summary>Cloud layers.</summary>
    public IReadOnlyList<CloudLayer> Clouds
    {
        get => _clouds;
        init => _clouds = EquatableArray.From(value);
    }

    /// <summary><c>NSC</c>, <c>NCD</c>, <c>SKC</c> or <c>CLR</c>, if given.</summary>
    public CloudCondition? CloudCondition { get; init; }

    /// <summary>Military colour states (METAR trends only, for example <c>TEMPO WHT</c>).</summary>
    public IReadOnlyList<ColorCode> ColorCodes
    {
        get => _colorCodes;
        init => _colorCodes = EquatableArray.From(value);
    }

    /// <summary>Forecast lowest QNH (UK military TAFs, <c>QNH3043INS</c>).</summary>
    public Pressure? Pressure { get; init; }

    /// <summary>Forecast icing layers (military TAFs, group <c>6IchihihitL</c> such as <c>651109</c>).</summary>
    public IReadOnlyList<IcingLayer> Icing
    {
        get => _icing;
        init => _icing = EquatableArray.From(value);
    }

    /// <summary>Forecast turbulence layers (military TAFs, group <c>5BhBhBhBtL</c> such as <c>510005</c>).</summary>
    public IReadOnlyList<TurbulenceLayer> Turbulence
    {
        get => _turbulence;
        init => _turbulence = EquatableArray.From(value);
    }

    /// <summary>Flight category computed from the visibility and cloud in this block only; <c>null</c> when neither is given.</summary>
    public FlightCategory? FlightCategory => FlightCategoryCalculator.Compute(Visibility, IsCavok, Clouds, CloudCondition);
}
