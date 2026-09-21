namespace Cavok;

/// <summary>Intensity or proximity of a weather phenomenon.</summary>
public enum WeatherIntensity
{
    /// <summary>Moderate (no sign).</summary>
    Moderate,

    /// <summary>Light (<c>-</c>).</summary>
    Light,

    /// <summary>Heavy (<c>+</c>).</summary>
    Heavy,

    /// <summary>In the vicinity of the aerodrome (<c>VC</c>).</summary>
    InVicinity,
}

/// <summary>Descriptor of a weather phenomenon.</summary>
public enum WeatherDescriptor
{
    /// <summary>Shallow (<c>MI</c>).</summary>
    Shallow,

    /// <summary>Partial (<c>PR</c>).</summary>
    Partial,

    /// <summary>Patches (<c>BC</c>).</summary>
    Patches,

    /// <summary>Low drifting (<c>DR</c>).</summary>
    LowDrifting,

    /// <summary>Blowing (<c>BL</c>).</summary>
    Blowing,

    /// <summary>Showers (<c>SH</c>).</summary>
    Showers,

    /// <summary>Thunderstorm (<c>TS</c>).</summary>
    Thunderstorm,

    /// <summary>Freezing (<c>FZ</c>).</summary>
    Freezing,
}

/// <summary>Precipitation, obscuration or other weather type.</summary>
public enum WeatherType
{
    /// <summary>Drizzle (<c>DZ</c>).</summary>
    Drizzle,

    /// <summary>Rain (<c>RA</c>).</summary>
    Rain,

    /// <summary>Snow (<c>SN</c>).</summary>
    Snow,

    /// <summary>Snow grains (<c>SG</c>).</summary>
    SnowGrains,

    /// <summary>Ice crystals (<c>IC</c>).</summary>
    IceCrystals,

    /// <summary>Ice pellets (<c>PL</c>).</summary>
    IcePellets,

    /// <summary>Hail (<c>GR</c>).</summary>
    Hail,

    /// <summary>Small hail and/or snow pellets (<c>GS</c>).</summary>
    SmallHail,

    /// <summary>Unknown precipitation (<c>UP</c>, automatic stations).</summary>
    UnknownPrecipitation,

    /// <summary>Mist (<c>BR</c>).</summary>
    Mist,

    /// <summary>Fog (<c>FG</c>).</summary>
    Fog,

    /// <summary>Smoke (<c>FU</c>).</summary>
    Smoke,

    /// <summary>Volcanic ash (<c>VA</c>).</summary>
    VolcanicAsh,

    /// <summary>Widespread dust (<c>DU</c>).</summary>
    Dust,

    /// <summary>Sand (<c>SA</c>).</summary>
    Sand,

    /// <summary>Haze (<c>HZ</c>).</summary>
    Haze,

    /// <summary>Dust or sand whirls (<c>PO</c>).</summary>
    DustWhirls,

    /// <summary>Squalls (<c>SQ</c>).</summary>
    Squalls,

    /// <summary>Funnel cloud; tornado or waterspout when heavy (<c>FC</c>).</summary>
    FunnelCloud,

    /// <summary>Sandstorm (<c>SS</c>).</summary>
    Sandstorm,

    /// <summary>Duststorm (<c>DS</c>).</summary>
    Duststorm,
}

/// <summary>A present or recent weather group such as <c>-SHRA</c>, <c>+TSRAGS</c> or <c>VCFG</c>.</summary>
public sealed record WeatherPhenomenon
{
    /// <summary>Intensity or proximity.</summary>
    public WeatherIntensity Intensity { get; init; } = WeatherIntensity.Moderate;

    /// <summary>Descriptor, if any.</summary>
    public WeatherDescriptor? Descriptor { get; init; }

    /// <summary>Weather types in reported order; may be empty (<c>TS</c>, <c>VCSH</c>).</summary>
    public IReadOnlyList<WeatherType> Types { get; init; } = Array.Empty<WeatherType>();

    /// <summary>True for <c>//</c>: not observable by an automatic station.</summary>
    public bool IsNotObservable { get; init; }
}
