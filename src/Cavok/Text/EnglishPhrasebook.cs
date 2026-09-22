using System.Globalization;
using System.Text;
using static System.FormattableString;

namespace Cavok.Text;

internal sealed class EnglishPhrasebook : Phrasebook
{
    public static readonly EnglishPhrasebook Instance = new EnglishPhrasebook();

    private static readonly NumberFormatInfo Format = new NumberFormatInfo
    {
        NumberDecimalSeparator = ".",
        NumberGroupSeparator = ",",
    };

    private EnglishPhrasebook()
    {
    }

    public override string NotAvailable => "not available";

    public override string NilReport => "No report available";

    public override string Cancelled => "Forecast cancelled";

    public override string CavokExplained => "CAVOK: 10 km or more, no cloud below 5,000 ft and no significant weather";

    public override string NoSignificantWeather => "no significant weather";

    protected override NumberFormatInfo Numbers => Format;

    public override string Label(Field field) => field switch
    {
        Field.Wind => "Wind",
        Field.Visibility => "Visibility",
        Field.RunwayVisualRange => "RVR",
        Field.Weather => "Weather",
        Field.Clouds => "Clouds",
        Field.Temperature => "Temperature",
        Field.Pressure => "QNH",
        Field.Icing => "Icing",
        Field.Turbulence => "Turbulence",
        Field.RecentWeather => "Recent",
        Field.WindShear => "Wind shear",
        Field.Sea => "Sea",
        Field.RunwayState => "Runway state",
        Field.ColorCode => "Color state",
        Field.FlightCategory => "Category",
        Field.Trend => "Trend",
        Field.Remarks => "Remarks",
        Field.MaximumTemperature => "Max temp",
        Field.MinimumTemperature => "Min temp",
        _ => "?",
    };

    public override string MetarHeader(ReportType type, string? station, DayTime? time, bool isAuto, bool isCorrected)
    {
        var text = new StringBuilder(type == ReportType.Speci ? "SPECI " : "METAR ");
        text.Append(station ?? "unknown station");
        if (time is DayTime t)
        {
            text.Append(", observed ").Append(DayAndTime(t));
        }

        if (isAuto)
        {
            text.Append(" (automated)");
        }

        if (isCorrected)
        {
            text.Append(" (corrected)");
        }

        return text.ToString();
    }

    public override string TafHeader(string? station, DayTime? issued, ValidityPeriod? validity, bool isAmended, bool isCorrected)
    {
        var text = new StringBuilder("TAF ");
        text.Append(station ?? "unknown station");
        if (issued is DayTime t)
        {
            text.Append(", issued ").Append(DayAndTime(t));
        }

        if (validity is ValidityPeriod v)
        {
            text.Append(", valid from ").Append(DayHourText(v.From)).Append(" to ").Append(DayHourText(v.To)).Append(" UTC");
        }

        if (isAmended)
        {
            text.Append(" (amended)");
        }

        if (isCorrected)
        {
            text.Append(" (corrected)");
        }

        return text.ToString();
    }

    public override string ChangeHeader(TafChange change)
    {
        string range = change.Period is ValidityPeriod p
            ? " between " + DayHourText(p.From) + " and " + DayHourText(p.To) + " UTC"
            : "";
        string head = change.Kind switch
        {
            TafChangeKind.From => change.From is DayTime f ? Invariant($"From day {f.Day} {Clock(f.Hour, f.Minute)} UTC") : "From",
            TafChangeKind.Becoming => "Becoming" + range,
            TafChangeKind.Temporary => (change.Probability is int chance ? Int(chance) + "% probability, temporarily" : "Temporarily") + range,
            TafChangeKind.Probability => (change.Probability is int chance2 ? Int(chance2) + "% probability" : "Probability") + range,
            _ => "?",
        };
        return head + ":";
    }

    public override string TrendPrefix(Trend trend)
    {
        if (trend.Kind == TrendKind.NoSignificantChange)
        {
            return "no significant change";
        }

        var text = new StringBuilder(trend.Kind == TrendKind.Becoming ? "becoming" : "temporarily");
        if (trend.From is TimeOfDay from)
        {
            text.Append(" from ").Append(Clock(from.Hour, from.Minute));
        }

        if (trend.Until is TimeOfDay until)
        {
            text.Append(" until ").Append(Clock(until.Hour, until.Minute));
        }

        if (trend.At is TimeOfDay at)
        {
            text.Append(" at ").Append(Clock(at.Hour, at.Minute));
        }

        return text.ToString();
    }

    public override string WindText(Wind wind)
    {
        if (wind.IsMissing)
        {
            return NotAvailable;
        }

        if (wind.IsCalm)
        {
            return "calm";
        }

        var parts = new List<string>();
        if (wind.IsVariable || wind.Direction is not null || wind.Speed is not null)
        {
            string direction = wind.IsVariable ? "variable" : wind.Direction is int d ? Degrees(d) : "unknown direction";
            string speed = wind.Speed is Speed s ? SpeedText(s) : "unknown speed";
            parts.Add(direction + " at " + speed);
        }

        if (wind.Gust is Speed gust)
        {
            parts.Add("gusting " + SpeedText(gust));
        }

        if (wind.VariableFrom is int from && wind.VariableTo is int to)
        {
            parts.Add("varying between " + Degrees(from) + " and " + Degrees(to));
        }

        return string.Join(", ", parts);
    }

    public override string VisibilityText(Visibility visibility)
    {
        string text;
        if (visibility.IsMissing)
        {
            text = NotAvailable;
        }
        else if (visibility.IsTenKmOrMore)
        {
            text = "10 km or more";
        }
        else if (visibility.Meters is int meters)
        {
            text = Number(meters) + " m";
        }
        else if (visibility.StatuteMiles is double miles)
        {
            string bound = visibility.IsLessThan ? "less than " : visibility.IsMoreThan ? "more than " : "";
            // Four decimals show every reportable fraction (sixteenths, 1/16SM = 0.0625) without rounding.
            text = bound + Fixed(miles, "0.####") + " SM";
        }
        else
        {
            text = "";
        }

        if (visibility.NoDirectionalVariation)
        {
            text += " (no directional variation)";
        }

        if (visibility.Minimum is MinimumVisibility minimum)
        {
            string direction = minimum.Direction is CompassDirection c ? " to the " + Compass(c) : "";
            string min = "minimum " + Number(minimum.Meters) + " m" + direction;
            text = text.Length == 0 ? min : text + ", " + min;
        }

        return text;
    }

    public override string RvrText(RunwayVisualRange rvr)
    {
        string unit = rvr.IsFeet ? " ft" : " m";
        string value;
        if (rvr.IsMissing || rvr.Value is not int minimum)
        {
            value = NotAvailable;
        }
        else if (rvr.VariableMax is int maximum)
        {
            value = "between " + Qualified(rvr.Qualifier, minimum) + unit + " and " + Qualified(rvr.VariableMaxQualifier, maximum) + unit;
        }
        else
        {
            value = Qualified(rvr.Qualifier, minimum) + unit;
        }

        string tendency = rvr.Tendency switch
        {
            RvrTendency.Up => ", increasing",
            RvrTendency.Down => ", decreasing",
            RvrTendency.NoChange => ", no change",
            _ => "",
        };
        return "runway " + rvr.Runway + ": " + value + tendency;
    }

    public override string WeatherText(WeatherPhenomenon weather)
    {
        if (weather.IsNotObservable)
        {
            return "not observable";
        }

        if (IsTornado(weather))
        {
            return "tornado or waterspout";
        }

        // DU on its own is "widespread dust"; raised by the wind (BLDU, DRDU) it is plain "dust".
        string types = JoinAnd(
            weather.Types.Select(t => t == WeatherType.Dust && weather.Descriptor is not null ? "dust" : TypeName(t)).ToList(),
            "and");
        string core = weather.Descriptor switch
        {
            null => types,
            WeatherDescriptor.Showers => types.Length == 0 ? "showers" : types + " showers",
            WeatherDescriptor.Thunderstorm => types.Length == 0 ? "thunderstorm" : "thunderstorm with " + types,
            WeatherDescriptor.Freezing => "freezing " + types,
            WeatherDescriptor.Shallow => "shallow " + types,
            WeatherDescriptor.Partial => "partial " + types,
            WeatherDescriptor.Patches => "patches of " + types,
            WeatherDescriptor.LowDrifting => "low drifting " + types,
            WeatherDescriptor.Blowing => "blowing " + types,
            _ => "?",
        };
        return weather.Intensity switch
        {
            WeatherIntensity.Light => "light " + core,
            WeatherIntensity.Heavy => "heavy " + core,
            WeatherIntensity.InVicinity => core + " in the vicinity",
            _ => core,
        };
    }

    public override string CloudText(CloudLayer layer)
    {
        if (layer.Cover is null && layer.HeightFeet is null && layer.Type is null)
        {
            return NotAvailable;
        }

        if (layer.Cover == CloudCover.VerticalVisibility)
        {
            return "sky obscured, vertical visibility " + (layer.HeightFeet is int vv ? Number(vv) + " ft" : NotAvailable);
        }

        string cover = layer.Cover switch
        {
            CloudCover.Few => "few (1–2/8)",
            CloudCover.Scattered => "scattered (3–4/8)",
            CloudCover.Broken => "broken (5–7/8)",
            CloudCover.Overcast => "overcast (8/8)",
            null => "cloud of unknown amount",
            _ => "?",
        };
        string height = layer.HeightFeet is int h ? "at " + Number(h) + " ft" : "at unknown height";
        string type = layer.Type switch
        {
            CloudType.Cumulonimbus => " with cumulonimbus",
            CloudType.ToweringCumulus => " with towering cumulus",
            _ => "",
        };
        return cover + " " + height + type;
    }

    public override string CloudConditionText(CloudCondition condition) => condition switch
    {
        CloudCondition.NoSignificantCloud => "no significant cloud",
        CloudCondition.NoCloudDetected => "no cloud detected",
        CloudCondition.SkyClear => "sky clear",
        CloudCondition.Clear => "no cloud below 12,000 ft",
        _ => "?",
    };

    public override string TemperatureText(int? temperature, int? dewPoint) =>
        (temperature is int t ? Celsius(t) : NotAvailable) + ", dew point " + (dewPoint is int d ? Celsius(d) : NotAvailable);

    public override string WindShearText(WindShear windShear) =>
        windShear.AllRunways ? "all runways" : "runway " + windShear.Runway;

    public override string SeaText(SeaCondition sea)
    {
        if (sea.SeaTemperature is null && sea.StateOfSea is null && sea.WaveHeightDecimeters is null)
        {
            return NotAvailable;
        }

        // The group always carries the sea temperature, so null means it was reported missing (W///S5).
        var parts = new List<string>
        {
            "sea temperature " + (sea.SeaTemperature is int temperature ? Celsius(temperature) : NotAvailable),
        };

        if (sea.StateOfSea is int state)
        {
            parts.Add("sea state " + Int(state));
        }

        if (sea.WaveHeightDecimeters is int height)
        {
            parts.Add("wave height " + Fixed(height / 10.0, "0.0") + " m");
        }

        return string.Join(", ", parts);
    }

    public override string RunwayStateText(RunwayState state)
    {
        if (state.SnowClosed && state.AllRunways)
        {
            return "aerodrome closed due to snow";
        }

        string runway = state.AllRunways ? "all runways" : state.IsRepeated ? "previous report repeated" : "runway " + state.Runway;
        if (state.SnowClosed)
        {
            return runway + ": closed due to snow";
        }

        var parts = new List<string>();
        if (state.Cleared)
        {
            parts.Add("contamination cleared");
        }

        if (state.Deposit is int deposit)
        {
            parts.Add(Deposit(deposit));
        }

        if (state.Extent is int extent)
        {
            parts.Add(Extent(extent));
        }

        if (state.Depth is int depth)
        {
            parts.Add(Depth(depth));
        }

        if (state.Friction is int friction)
        {
            parts.Add(Friction(friction));
        }

        return runway + ": " + (parts.Count == 0 ? NotAvailable : string.Join(", ", parts));
    }

    public override string ColorCodeText(ColorCode code)
    {
        string name = code.State switch
        {
            ColorState.BluePlus => "blue plus",
            ColorState.Blue => "blue",
            ColorState.White => "white",
            ColorState.Green => "green",
            ColorState.Yellow1 => "yellow 1",
            ColorState.Yellow2 => "yellow 2",
            ColorState.Yellow => "yellow",
            ColorState.Amber => "amber",
            ColorState.Red => "red",
            _ => "?",
        };
        return code.IsBlack ? name + " (airfield unusable)" : name;
    }

    public override string TemperatureForecastText(TemperatureForecast forecast) =>
        Celsius(forecast.Celsius) + Invariant($" on day {forecast.Time.Day} at {Clock(forecast.Time.Hour, 0)} UTC");

    public override string IcingText(IcingLayer layer)
    {
        string type = layer.Type switch
        {
            0 => "trace icing",
            1 => "light icing",
            2 => "light icing in cloud",
            3 => "light icing in precipitation",
            4 => "moderate icing",
            5 => "moderate icing in cloud",
            6 => "moderate icing in precipitation",
            7 => "severe icing",
            8 => "severe icing in cloud",
            9 => "severe icing in precipitation",
            _ => "?",
        };
        return type + " " + LayerText(layer.BaseFeet, layer.ThicknessFeet);
    }

    public override string TurbulenceText(TurbulenceLayer layer)
    {
        string type = layer.Type switch
        {
            0 => "no turbulence",
            1 => "light turbulence",
            2 => "occasional moderate turbulence in clear air",
            3 => "frequent moderate turbulence in clear air",
            4 => "occasional moderate turbulence in cloud",
            5 => "frequent moderate turbulence in cloud",
            6 => "occasional severe turbulence in clear air",
            7 => "frequent severe turbulence in clear air",
            8 => "occasional severe turbulence in cloud",
            9 => "frequent severe turbulence in cloud",
            _ => "?",
        };
        return type + " " + LayerText(layer.BaseFeet, layer.ThicknessFeet);
    }

    public override string InlineWind(string text) => "wind " + text;

    public override string InlineVisibility(string text) => "visibility " + text;

    public override string InlineColorCode(string text) => "color state " + text;

    private static string DayAndTime(DayTime t) => Invariant($"day {t.Day} at {Clock(t.Hour, t.Minute)} UTC");

    private static string DayHourText(DayHour h) => Invariant($"day {h.Day} {Clock(h.Hour, 0)}");

    private static string SpeedText(Speed speed) => (speed.IsAbove ? "more than " : "") + Int(speed.Value) + " " + Unit(speed.Unit);

    private string Qualified(RvrQualifier? qualifier, int value) => qualifier switch
    {
        RvrQualifier.Above => "more than ",
        RvrQualifier.Below => "less than ",
        _ => "",
    } + Number(value);

    private static string Compass(CompassDirection direction) => direction switch
    {
        CompassDirection.North => "north",
        CompassDirection.NorthEast => "northeast",
        CompassDirection.East => "east",
        CompassDirection.SouthEast => "southeast",
        CompassDirection.South => "south",
        CompassDirection.SouthWest => "southwest",
        CompassDirection.West => "west",
        CompassDirection.NorthWest => "northwest",
        _ => "?",
    };

    private static string TypeName(WeatherType type) => type switch
    {
        WeatherType.Drizzle => "drizzle",
        WeatherType.Rain => "rain",
        WeatherType.Snow => "snow",
        WeatherType.SnowGrains => "snow grains",
        WeatherType.IceCrystals => "ice crystals",
        WeatherType.IcePellets => "ice pellets",
        WeatherType.Hail => "hail",
        WeatherType.SmallHail => "small hail",
        WeatherType.UnknownPrecipitation => "unknown precipitation",
        WeatherType.Mist => "mist",
        WeatherType.Fog => "fog",
        WeatherType.Smoke => "smoke",
        WeatherType.VolcanicAsh => "volcanic ash",
        WeatherType.Dust => "widespread dust",
        WeatherType.Sand => "sand",
        WeatherType.Haze => "haze",
        WeatherType.DustWhirls => "dust whirls",
        WeatherType.Squalls => "squalls",
        WeatherType.FunnelCloud => "funnel cloud",
        WeatherType.Sandstorm => "sandstorm",
        WeatherType.Duststorm => "duststorm",
        _ => "?",
    };

    // "from 11,000 to 20,000 ft", "up to 2,000 ft" (base at the surface) or "from 5,000 ft" (no thickness given).
    private string LayerText(int baseFeet, int thicknessFeet)
    {
        if (thicknessFeet == 0)
        {
            return "from " + Number(baseFeet) + " ft";
        }

        string top = Number(baseFeet + thicknessFeet) + " ft";
        return baseFeet == 0 ? "up to " + top : "from " + Number(baseFeet) + " to " + top;
    }

    private static string Deposit(int code) => code switch
    {
        0 => "clear and dry",
        1 => "damp",
        2 => "wet or water patches",
        3 => "rime or frost",
        4 => "dry snow",
        5 => "wet snow",
        6 => "slush",
        7 => "ice",
        8 => "compacted or rolled snow",
        9 => "frozen ruts or ridges",
        _ => "deposit code " + Int(code),
    };

    private static string Extent(int code) => code switch
    {
        1 => "covering 10% or less",
        2 => "covering 11–25%",
        5 => "covering 26–50%",
        9 => "covering 51–100%",
        _ => "extent code " + Int(code),
    };

    private static string Depth(int code) => code switch
    {
        0 => "depth less than 1 mm",
        >= 1 and <= 90 => "depth " + Int(code) + " mm",
        92 => "depth 10 cm",
        93 => "depth 15 cm",
        94 => "depth 20 cm",
        95 => "depth 25 cm",
        96 => "depth 30 cm",
        97 => "depth 35 cm",
        98 => "depth 40 cm or more",
        99 => "runway not operational",
        _ => "depth code " + Int(code),
    };

    private string Friction(int code) => code switch
    {
        >= 1 and <= 90 => "friction coefficient " + Fixed(code / 100.0, "0.00"),
        91 => "braking action poor",
        92 => "braking action medium to poor",
        93 => "braking action medium",
        94 => "braking action medium to good",
        95 => "braking action good",
        99 => "braking action unreliable",
        _ => "friction code " + Int(code),
    };
}
