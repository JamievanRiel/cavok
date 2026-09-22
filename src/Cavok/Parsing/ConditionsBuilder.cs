namespace Cavok.Parsing;

// Collects wind, visibility, weather, cloud, colour-state and pressure groups, and the icing and turbulence groups
// of military TAFs. Used for the observed part of a METAR, for METAR trends and for the base and change groups of a TAF.
internal sealed class ConditionsBuilder
{
    private readonly List<WeatherPhenomenon> _weather = new List<WeatherPhenomenon>();
    private readonly List<CloudLayer> _clouds = new List<CloudLayer>();
    private readonly List<ColorCode> _colorCodes = new List<ColorCode>();
    private readonly List<IcingLayer> _icing = new List<IcingLayer>();
    private readonly List<TurbulenceLayer> _turbulence = new List<TurbulenceLayer>();
    private Wind? _wind;
    private WindVariation? _variation;
    private bool _cavok;
    private Visibility? _visibility;
    private MinimumVisibility? _minimum;
    private bool _noSignificantWeather;
    private CloudCondition? _cloudCondition;
    private bool _pressureSeen;
    private Pressure? _pressure;

    public static bool Handles(GroupKind kind) =>
        kind == GroupKind.Wind
        || kind == GroupKind.WindVariation
        || kind == GroupKind.Cavok
        || kind == GroupKind.Visibility
        || kind == GroupKind.MinimumVisibility
        || kind == GroupKind.Weather
        || kind == GroupKind.NoSignificantWeather
        || kind == GroupKind.Cloud
        || kind == GroupKind.CloudCondition
        || kind == GroupKind.ColorCode
        || kind == GroupKind.Pressure;

    // CAVOK stands for the whole visibility, so it excludes every other visibility group, a minimum visibility
    // included (Visibility stays null when IsCavok); whichever comes second is a duplicate.
    public bool CanAccept(Group group) => group.Kind switch
    {
        GroupKind.Wind => _wind is null,
        GroupKind.WindVariation => _variation is null,
        GroupKind.Cavok => !_cavok && _visibility is null && _minimum is null,
        GroupKind.Visibility => (!_cavok && _visibility is null) || CanBeMinimum(group),
        GroupKind.MinimumVisibility => !_cavok && _minimum is null,
        GroupKind.Weather => true,
        GroupKind.NoSignificantWeather => !_noSignificantWeather,
        GroupKind.Cloud => true,
        GroupKind.CloudCondition => _cloudCondition is null,
        GroupKind.ColorCode => true,
        GroupKind.Pressure => !_pressureSeen,
        _ => false,
    };

    public void Apply(Group group)
    {
        switch (group.Kind)
        {
            case GroupKind.Wind:
                _wind = (Wind)group.Value!;
                break;
            case GroupKind.WindVariation:
                _variation = (WindVariation)group.Value!;
                break;
            case GroupKind.Cavok:
                _cavok = true;
                break;
            case GroupKind.Visibility:
                {
                    var visibility = (Visibility)group.Value!;
                    if (_visibility is null && !_cavok)
                    {
                        _visibility = visibility;
                    }
                    else
                    {
                        // A second plain visibility group is the minimum visibility without a direction.
                        _minimum = new MinimumVisibility(visibility.Meters ?? 0, null);
                    }

                    break;
                }

            case GroupKind.MinimumVisibility:
                _minimum = (MinimumVisibility)group.Value!;
                break;
            case GroupKind.Weather:
                _weather.Add((WeatherPhenomenon)group.Value!);
                break;
            case GroupKind.NoSignificantWeather:
                _noSignificantWeather = true;
                break;
            case GroupKind.Cloud:
                _clouds.Add((CloudLayer)group.Value!);
                break;
            case GroupKind.CloudCondition:
                _cloudCondition = (CloudCondition)group.Value!;
                break;
            case GroupKind.ColorCode:
                _colorCodes.AddRange((IReadOnlyList<ColorCode>)group.Value!);
                break;
            case GroupKind.Pressure:
                _pressureSeen = true;
                _pressure = (Pressure?)group.Value;
                break;
        }
    }

    // Applies the group, or reports a duplicate; then runs the plausibility checks.
    public void Add(Group group, Token first, Token last, DiagnosticBag diagnostics)
    {
        if (!CanAccept(group))
        {
            diagnostics.Warning(DiagnosticCode.Duplicate, first, last);
            return;
        }

        Apply(group);
        GroupChecks.Check(group, first, last, diagnostics);
    }

    public void AddIcing(IcingLayer layer) => _icing.Add(layer);

    public void AddTurbulence(TurbulenceLayer layer) => _turbulence.Add(layer);

    // The records copy the lists into EquatableArrays, so the builder's lists are not shared.
    public ForecastConditions Build() => new ForecastConditions
    {
        Wind = BuildWind(),
        Visibility = BuildVisibility(),
        IsCavok = _cavok,
        Weather = _weather,
        NoSignificantWeather = _noSignificantWeather,
        Clouds = _clouds,
        CloudCondition = _cloudCondition,
        ColorCodes = _colorCodes,
        Pressure = _pressure,
        Icing = _icing,
        Turbulence = _turbulence,
    };

    private bool CanBeMinimum(Group group) =>
        _visibility is not null
        && _minimum is null
        && group.Value is Visibility
        {
            Meters: not null, IsTenKmOrMore: false, NoDirectionalVariation: false, IsMissing: false, StatuteMiles: null,
        };

    private Wind? BuildWind()
    {
        if (_variation is not WindVariation variation)
        {
            return _wind;
        }

        return (_wind ?? new Wind()) with { VariableFrom = variation.From, VariableTo = variation.To };
    }

    private Visibility? BuildVisibility()
    {
        if (_minimum is null)
        {
            return _visibility;
        }

        return (_visibility ?? new Visibility()) with { Minimum = _minimum };
    }
}
