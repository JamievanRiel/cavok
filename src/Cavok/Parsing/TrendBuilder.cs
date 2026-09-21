namespace Cavok.Parsing;

internal sealed class TrendBuilder
{
    private TimeOfDay? _from;
    private TimeOfDay? _until;
    private TimeOfDay? _at;

    public TrendBuilder(TrendKind kind)
    {
        Kind = kind;
    }

    public TrendKind Kind { get; }

    public ConditionsBuilder Conditions { get; } = new ConditionsBuilder();

    // Groups that may appear in a METAR trend (pressure only appears in UK military TAFs).
    public static bool Accepts(GroupKind kind) => kind != GroupKind.Pressure && ConditionsBuilder.Handles(kind);

    // Returns false when this kind of time group was already given.
    public bool SetTime(string prefix, TimeOfDay time)
    {
        switch (prefix)
        {
            case "FM" when _from is null:
                _from = time;
                return true;
            case "TL" when _until is null:
                _until = time;
                return true;
            case "AT" when _at is null:
                _at = time;
                return true;
            default:
                return false;
        }
    }

    public Trend Build() => new Trend
    {
        Kind = Kind,
        From = _from,
        Until = _until,
        At = _at,
        Conditions = Conditions.Build(),
    };
}
