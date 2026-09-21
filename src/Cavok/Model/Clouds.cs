namespace Cavok;

/// <summary>Cloud amount.</summary>
public enum CloudCover
{
    /// <summary>Few, 1–2 oktas (<c>FEW</c>).</summary>
    Few,

    /// <summary>Scattered, 3–4 oktas (<c>SCT</c>).</summary>
    Scattered,

    /// <summary>Broken, 5–7 oktas (<c>BKN</c>).</summary>
    Broken,

    /// <summary>Overcast, 8 oktas (<c>OVC</c>).</summary>
    Overcast,

    /// <summary>Sky obscured; the height is the vertical visibility (<c>VV</c>).</summary>
    VerticalVisibility,
}

/// <summary>Significant convective cloud type.</summary>
public enum CloudType
{
    /// <summary>Cumulonimbus (<c>CB</c>).</summary>
    Cumulonimbus,

    /// <summary>Towering cumulus (<c>TCU</c>).</summary>
    ToweringCumulus,
}

/// <summary>A report that there are no cloud layers to give.</summary>
public enum CloudCondition
{
    /// <summary>No significant cloud (<c>NSC</c>).</summary>
    NoSignificantCloud,

    /// <summary>No cloud detected by an automatic station (<c>NCD</c>).</summary>
    NoCloudDetected,

    /// <summary>Sky clear (<c>SKC</c>).</summary>
    SkyClear,

    /// <summary>No cloud below 12,000 ft detected by an automatic station (<c>CLR</c>).</summary>
    Clear,
}

/// <summary>A cloud layer such as <c>BKN030CB</c> or a vertical visibility such as <c>VV002</c>.</summary>
public sealed record CloudLayer
{
    /// <summary>Cloud amount; <c>null</c> when not observable (<c>///</c>).</summary>
    public CloudCover? Cover { get; init; }

    /// <summary>Height of the cloud base (or vertical visibility) in feet; <c>null</c> when not observable.</summary>
    public int? HeightFeet { get; init; }

    /// <summary>Convective cloud type, if reported.</summary>
    public CloudType? Type { get; init; }

    /// <summary>True when the cloud type could not be observed (<c>///</c> after the height).</summary>
    public bool IsTypeNotObservable { get; init; }
}
