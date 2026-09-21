namespace Cavok;

/// <summary>A surface wind group such as <c>24012G25KT</c>, optionally combined with a variation group such as <c>200V280</c>.</summary>
public sealed record Wind
{
    /// <summary>Direction the wind blows from in degrees; <c>null</c> when variable (<c>VRB</c>) or missing.</summary>
    public int? Direction { get; init; }

    /// <summary>True for <c>VRB</c>.</summary>
    public bool IsVariable { get; init; }

    /// <summary>Mean wind speed; <c>null</c> when missing.</summary>
    public Speed? Speed { get; init; }

    /// <summary>Gust speed, if reported.</summary>
    public Speed? Gust { get; init; }

    /// <summary>Start of the direction variation (<c>200</c> in <c>200V280</c>).</summary>
    public int? VariableFrom { get; init; }

    /// <summary>End of the direction variation (<c>280</c> in <c>200V280</c>).</summary>
    public int? VariableTo { get; init; }

    /// <summary>True for <c>00000KT</c>.</summary>
    public bool IsCalm { get; init; }

    /// <summary>True when the whole group is missing (<c>/////KT</c>).</summary>
    public bool IsMissing { get; init; }
}
