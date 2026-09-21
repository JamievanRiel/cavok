namespace Cavok.Parsing;

internal enum GroupKind
{
    Auto,
    Corrected,
    Wind,
    WindVariation,
    Cavok,
    Visibility,
    MinimumVisibility,
    RunwayVisualRange,
    Weather,
    NoSignificantWeather,
    Cloud,
    CloudCondition,
    Temperature,
    Pressure,
    RecentWeather,
    WindShear,
    Sea,
    RunwayState,
    ColorCode,
}

// A recognised group: its kind, how many tokens it spans and its parsed value.
internal sealed class Group
{
    public Group(GroupKind kind, int tokenCount, object? value)
    {
        Kind = kind;
        TokenCount = tokenCount;
        Value = value;
    }

    public GroupKind Kind { get; }

    public int TokenCount { get; }

    public object? Value { get; }
}
