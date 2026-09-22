namespace Cavok.Parsing;

internal sealed class MetarBuilder
{
    public MetarBuilder(string raw)
    {
        Raw = raw;
    }

    public string Raw { get; }

    public ReportType Type { get; set; } = ReportType.Metar;

    public string? Station { get; set; }

    public DayTime? Time { get; set; }

    public bool IsAuto { get; set; }

    public bool IsCorrected { get; set; }

    public bool IsNil { get; set; }

    public ConditionsBuilder Observed { get; } = new ConditionsBuilder();

    public List<RunwayVisualRange> RunwayVisualRanges { get; } = new List<RunwayVisualRange>();

    public TemperaturePair? Temperature { get; set; }

    public List<WeatherPhenomenon> RecentWeather { get; } = new List<WeatherPhenomenon>();

    public List<WindShear> WindShears { get; } = new List<WindShear>();

    public SeaCondition? Sea { get; set; }

    public List<RunwayState> RunwayStates { get; } = new List<RunwayState>();

    public List<Trend> Trends { get; } = new List<Trend>();

    public string? Remarks { get; set; }

    public bool CanAccept(Group group) => group.Kind switch
    {
        GroupKind.Auto => !IsAuto,
        GroupKind.Corrected => !IsCorrected,
        GroupKind.RunwayVisualRange => true,
        GroupKind.Temperature => Temperature is null,
        GroupKind.RecentWeather => true,
        GroupKind.WindShear => true,
        GroupKind.Sea => Sea is null,
        GroupKind.RunwayState => true,
        _ => Observed.CanAccept(group),
    };

    public void Apply(Group group)
    {
        switch (group.Kind)
        {
            case GroupKind.Auto:
                IsAuto = true;
                break;
            case GroupKind.Corrected:
                IsCorrected = true;
                break;
            case GroupKind.RunwayVisualRange:
                RunwayVisualRanges.Add((RunwayVisualRange)group.Value!);
                break;
            case GroupKind.Temperature:
                Temperature = (TemperaturePair)group.Value!;
                break;
            case GroupKind.RecentWeather:
                RecentWeather.Add((WeatherPhenomenon)group.Value!);
                break;
            case GroupKind.WindShear:
                WindShears.Add((WindShear)group.Value!);
                break;
            case GroupKind.Sea:
                Sea = (SeaCondition)group.Value!;
                break;
            case GroupKind.RunwayState:
                RunwayStates.Add((RunwayState)group.Value!);
                break;
            default:
                Observed.Apply(group);
                break;
        }
    }

    // The records copy the lists into EquatableArrays, so the builder's lists are not shared.
    public Metar Build(DiagnosticBag diagnostics)
    {
        ForecastConditions observed = Observed.Build();
        return new Metar
        {
            Raw = Raw,
            Type = Type,
            Station = Station,
            Time = Time,
            IsAuto = IsAuto,
            IsCorrected = IsCorrected,
            IsNil = IsNil,
            Wind = observed.Wind,
            Visibility = observed.Visibility,
            IsCavok = observed.IsCavok,
            RunwayVisualRanges = RunwayVisualRanges,
            Weather = observed.Weather,
            Clouds = observed.Clouds,
            CloudCondition = observed.CloudCondition,
            Temperature = Temperature?.Temperature,
            DewPoint = Temperature?.DewPoint,
            Pressure = observed.Pressure,
            RecentWeather = RecentWeather,
            WindShear = WindShears,
            Sea = Sea,
            RunwayStates = RunwayStates,
            ColorCodes = observed.ColorCodes,
            Trends = Trends,
            Remarks = Remarks,
            Diagnostics = diagnostics.Items,
        };
    }
}
