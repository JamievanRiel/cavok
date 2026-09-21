namespace Cavok.Parsing;

internal sealed class TafBuilder
{
    public TafBuilder(string raw)
    {
        Raw = raw;
    }

    public string Raw { get; }

    public string? Station { get; set; }

    public DayTime? IssueTime { get; set; }

    public bool IsAmended { get; set; }

    public bool IsCorrected { get; set; }

    public bool IsCancelled { get; set; }

    public bool IsNil { get; set; }

    public ValidityPeriod? Validity { get; set; }

    public ConditionsBuilder Base { get; } = new ConditionsBuilder();

    public List<TafChange> Changes { get; } = new List<TafChange>();

    public List<TemperatureForecast> Temperatures { get; } = new List<TemperatureForecast>();

    public string? Remarks { get; set; }

    public Taf Build(DiagnosticBag diagnostics) => new Taf
    {
        Raw = Raw,
        Station = Station,
        IssueTime = IssueTime,
        IsAmended = IsAmended,
        IsCorrected = IsCorrected,
        IsCancelled = IsCancelled,
        IsNil = IsNil,
        Validity = Validity,
        Base = Base.Build(),
        Changes = Changes.ToArray(),
        Temperatures = Temperatures.ToArray(),
        Remarks = Remarks,
        Diagnostics = diagnostics.Items.ToArray(),
    };
}
