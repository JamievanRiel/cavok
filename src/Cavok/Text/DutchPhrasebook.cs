using System.Globalization;
using System.Text;
using static System.FormattableString;

namespace Cavok.Text;

internal sealed class DutchPhrasebook : Phrasebook
{
    public static readonly DutchPhrasebook Instance = new DutchPhrasebook();

    private static readonly NumberFormatInfo Format = new NumberFormatInfo
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = ".",
    };

    private DutchPhrasebook()
    {
    }

    public override string NotAvailable => "niet beschikbaar";

    public override string NilReport => "Geen bericht beschikbaar";

    public override string Cancelled => "Verwachting geannuleerd";

    public override string CavokExplained => "CAVOK: 10 km of meer, geen bewolking onder 5.000 ft en geen significant weer";

    public override string NoSignificantWeather => "geen significant weer";

    protected override NumberFormatInfo Numbers => Format;

    public override string Label(Field field) => field switch
    {
        Field.Wind => "Wind",
        Field.Visibility => "Zicht",
        Field.RunwayVisualRange => "Baanzicht",
        Field.Weather => "Weer",
        Field.Clouds => "Bewolking",
        Field.Temperature => "Temperatuur",
        Field.Pressure => "QNH",
        Field.Icing => "IJsafzetting",
        Field.Turbulence => "Turbulentie",
        Field.RecentWeather => "Recent weer",
        Field.WindShear => "Windschering",
        Field.Sea => "Zee",
        Field.RunwayState => "Baantoestand",
        Field.ColorCode => "Kleurcode",
        Field.FlightCategory => "Categorie",
        Field.Trend => "Trend",
        Field.Remarks => "Opmerkingen",
        Field.MaximumTemperature => "Max. temp.",
        Field.MinimumTemperature => "Min. temp.",
        _ => "?",
    };

    public override string MetarHeader(ReportType type, string? station, DayTime? time, bool isAuto, bool isCorrected)
    {
        var text = new StringBuilder(type == ReportType.Speci ? "SPECI " : "METAR ");
        text.Append(station ?? "onbekend station");
        if (time is DayTime t)
        {
            text.Append(", waarneming ").Append(DayAndTime(t));
        }

        if (isAuto)
        {
            text.Append(" (automatisch)");
        }

        if (isCorrected)
        {
            text.Append(" (gecorrigeerd)");
        }

        return text.ToString();
    }

    public override string TafHeader(string? station, DayTime? issued, ValidityPeriod? validity, bool isAmended, bool isCorrected)
    {
        var text = new StringBuilder("TAF ");
        text.Append(station ?? "onbekend station");
        if (issued is DayTime t)
        {
            text.Append(", uitgegeven ").Append(DayAndTime(t));
        }

        if (validity is ValidityPeriod v)
        {
            text.Append(", geldig van ").Append(DayHourText(v.From)).Append(" tot ").Append(DayHourText(v.To)).Append(" UTC");
        }

        if (isAmended)
        {
            text.Append(" (gewijzigd)");
        }

        if (isCorrected)
        {
            text.Append(" (gecorrigeerd)");
        }

        return text.ToString();
    }

    public override string ChangeHeader(TafChange change)
    {
        string range = change.Period is ValidityPeriod p
            ? " tussen " + DayHourText(p.From) + " en " + DayHourText(p.To) + " UTC"
            : "";
        string head = change.Kind switch
        {
            TafChangeKind.From => change.From is DayTime f ? Invariant($"Vanaf dag {f.Day} {Clock(f.Hour, f.Minute)} UTC") : "Vanaf",
            TafChangeKind.Becoming => "Geleidelijk" + range,
            TafChangeKind.Temporary => (change.Probability is int chance ? Int(chance) + "% kans, tijdelijk" : "Tijdelijk") + range,
            TafChangeKind.Probability => (change.Probability is int chance2 ? Int(chance2) + "% kans" : "Kans") + range,
            _ => "?",
        };
        return head + ":";
    }

    public override string TrendPrefix(Trend trend)
    {
        if (trend.Kind == TrendKind.NoSignificantChange)
        {
            return "geen significante verandering";
        }

        var text = new StringBuilder(trend.Kind == TrendKind.Becoming ? "geleidelijk" : "tijdelijk");
        if (trend.From is TimeOfDay from)
        {
            text.Append(" vanaf ").Append(Clock(from.Hour, from.Minute));
        }

        if (trend.Until is TimeOfDay until)
        {
            text.Append(" tot ").Append(Clock(until.Hour, until.Minute));
        }

        if (trend.At is TimeOfDay at)
        {
            text.Append(" om ").Append(Clock(at.Hour, at.Minute));
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
            return "windstil";
        }

        var parts = new List<string>();
        if (wind.IsVariable || wind.Direction is not null || wind.Speed is not null)
        {
            string direction = wind.IsVariable ? "variabel" : wind.Direction is int d ? Degrees(d) : "onbekende richting";
            string speed = wind.Speed is Speed s ? SpeedText(s) : "onbekende snelheid";
            parts.Add(direction + " met " + speed);
        }

        if (wind.Gust is Speed gust)
        {
            parts.Add("windstoten tot " + SpeedText(gust));
        }

        if (wind.VariableFrom is int from && wind.VariableTo is int to)
        {
            parts.Add("variërend tussen " + Degrees(from) + " en " + Degrees(to));
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
            text = "10 km of meer";
        }
        else if (visibility.Meters is int meters)
        {
            text = Number(meters) + " m";
        }
        else if (visibility.StatuteMiles is double miles)
        {
            string bound = visibility.IsLessThan ? "minder dan " : visibility.IsMoreThan ? "meer dan " : "";
            // Four decimals show every reportable fraction (sixteenths, 1/16SM = 0,0625) without rounding.
            text = bound + Fixed(miles, "0.####") + " SM";
        }
        else
        {
            text = "";
        }

        if (visibility.NoDirectionalVariation)
        {
            text += " (geen richtingsvariatie)";
        }

        if (visibility.Minimum is MinimumVisibility minimum)
        {
            string direction = minimum.Direction is CompassDirection c ? " in het " + Compass(c) : "";
            string min = "minimaal " + Number(minimum.Meters) + " m" + direction;
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
            value = "tussen " + Qualified(rvr.Qualifier, minimum) + unit + " en " + Qualified(rvr.VariableMaxQualifier, maximum) + unit;
        }
        else
        {
            value = Qualified(rvr.Qualifier, minimum) + unit;
        }

        string tendency = rvr.Tendency switch
        {
            RvrTendency.Up => ", toenemend",
            RvrTendency.Down => ", afnemend",
            RvrTendency.NoChange => ", geen verandering",
            _ => "",
        };
        return "baan " + rvr.Runway + ": " + value + tendency;
    }

    public override string WeatherText(WeatherPhenomenon weather)
    {
        if (weather.IsNotObservable)
        {
            return "niet waarneembaar";
        }

        if (IsTornado(weather))
        {
            return "tornado of waterhoos";
        }

        (string core, bool neuter) = Core(weather);
        return weather.Intensity switch
        {
            WeatherIntensity.Light => (neuter ? "licht " : "lichte ") + core,
            WeatherIntensity.Heavy => (neuter ? "zwaar " : "zware ") + core,
            WeatherIntensity.InVicinity => core + " in de omgeving",
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
            return "lucht onzichtbaar, verticaal zicht " + (layer.HeightFeet is int vv ? Number(vv) + " ft" : NotAvailable);
        }

        string cover = layer.Cover switch
        {
            CloudCover.Few => "enkele wolken (1–2/8)",
            CloudCover.Scattered => "verspreid (3–4/8)",
            CloudCover.Broken => "gebroken (5–7/8)",
            CloudCover.Overcast => "geheel bewolkt (8/8)",
            null => "bewolking van onbekende hoeveelheid",
            _ => "?",
        };
        string height = layer.HeightFeet is int h ? "op " + Number(h) + " ft" : "op onbekende hoogte";
        string type = layer.Type switch
        {
            CloudType.Cumulonimbus => " met cumulonimbus",
            CloudType.ToweringCumulus => " met torenvormige cumulus",
            _ => "",
        };
        return cover + " " + height + type;
    }

    public override string CloudConditionText(CloudCondition condition) => condition switch
    {
        CloudCondition.NoSignificantCloud => "geen significante bewolking",
        CloudCondition.NoCloudDetected => "geen bewolking waargenomen",
        CloudCondition.SkyClear => "onbewolkt",
        CloudCondition.Clear => "geen bewolking onder 12.000 ft",
        _ => "?",
    };

    public override string TemperatureText(int? temperature, int? dewPoint) =>
        (temperature is int t ? Celsius(t) : NotAvailable) + ", dauwpunt " + (dewPoint is int d ? Celsius(d) : NotAvailable);

    public override string WindShearText(WindShear windShear) =>
        windShear.AllRunways ? "alle banen" : "baan " + windShear.Runway;

    public override string SeaText(SeaCondition sea)
    {
        if (sea.SeaTemperature is null && sea.StateOfSea is null && sea.WaveHeightDecimeters is null)
        {
            return NotAvailable;
        }

        // The group always carries the sea temperature, so null means it was reported missing (W///S5).
        var parts = new List<string>
        {
            "zeewatertemperatuur " + (sea.SeaTemperature is int temperature ? Celsius(temperature) : NotAvailable),
        };

        if (sea.StateOfSea is int state)
        {
            parts.Add("zeegang " + Int(state));
        }

        if (sea.WaveHeightDecimeters is int height)
        {
            parts.Add("golfhoogte " + Fixed(height / 10.0, "0.0") + " m");
        }

        return string.Join(", ", parts);
    }

    public override string RunwayStateText(RunwayState state)
    {
        if (state.SnowClosed && state.AllRunways)
        {
            return "luchthaven gesloten wegens sneeuw";
        }

        string runway = state.AllRunways ? "alle banen" : state.IsRepeated ? "vorig bericht herhaald" : "baan " + state.Runway;
        if (state.SnowClosed)
        {
            return runway + ": gesloten wegens sneeuw";
        }

        var parts = new List<string>();
        if (state.Cleared)
        {
            parts.Add("verontreiniging verwijderd");
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
            ColorState.BluePlus => "blauw plus",
            ColorState.Blue => "blauw",
            ColorState.White => "wit",
            ColorState.Green => "groen",
            ColorState.Yellow1 => "geel 1",
            ColorState.Yellow2 => "geel 2",
            ColorState.Yellow => "geel",
            ColorState.Amber => "amber",
            ColorState.Red => "rood",
            _ => "?",
        };
        return code.IsBlack ? name + " (vliegveld onbruikbaar)" : name;
    }

    public override string TemperatureForecastText(TemperatureForecast forecast) =>
        Celsius(forecast.Celsius) + Invariant($" op dag {forecast.Time.Day} om {Clock(forecast.Time.Hour, 0)} UTC");

    public override string IcingText(IcingLayer layer)
    {
        string type = layer.Type switch
        {
            0 => "sporen van ijsafzetting",
            1 => "lichte ijsafzetting",
            2 => "lichte ijsafzetting in wolken",
            3 => "lichte ijsafzetting in neerslag",
            4 => "matige ijsafzetting",
            5 => "matige ijsafzetting in wolken",
            6 => "matige ijsafzetting in neerslag",
            7 => "zware ijsafzetting",
            8 => "zware ijsafzetting in wolken",
            9 => "zware ijsafzetting in neerslag",
            _ => "?",
        };
        return type + " " + LayerText(layer.BaseFeet, layer.ThicknessFeet);
    }

    public override string TurbulenceText(TurbulenceLayer layer)
    {
        string type = layer.Type switch
        {
            0 => "geen turbulentie",
            1 => "lichte turbulentie",
            2 => "af en toe matige turbulentie in heldere lucht",
            3 => "vaak matige turbulentie in heldere lucht",
            4 => "af en toe matige turbulentie in wolken",
            5 => "vaak matige turbulentie in wolken",
            6 => "af en toe zware turbulentie in heldere lucht",
            7 => "vaak zware turbulentie in heldere lucht",
            8 => "af en toe zware turbulentie in wolken",
            9 => "vaak zware turbulentie in wolken",
            _ => "?",
        };
        return type + " " + LayerText(layer.BaseFeet, layer.ThicknessFeet);
    }

    public override string InlineWind(string text) => "wind " + text;

    public override string InlineVisibility(string text) => "zicht " + text;

    public override string InlineColorCode(string text) => "kleurcode " + text;

    private static string DayAndTime(DayTime t) => Invariant($"dag {t.Day} om {Clock(t.Hour, t.Minute)} UTC");

    private static string DayHourText(DayHour h) => Invariant($"dag {h.Day} {Clock(h.Hour, 0)}");

    private static string SpeedText(Speed speed) => (speed.IsAbove ? "meer dan " : "") + Int(speed.Value) + " " + Unit(speed.Unit);

    private string Qualified(RvrQualifier? qualifier, int value) => qualifier switch
    {
        RvrQualifier.Above => "meer dan ",
        RvrQualifier.Below => "minder dan ",
        _ => "",
    } + Number(value);

    private static string Compass(CompassDirection direction) => direction switch
    {
        CompassDirection.North => "noorden",
        CompassDirection.NorthEast => "noordoosten",
        CompassDirection.East => "oosten",
        CompassDirection.SouthEast => "zuidoosten",
        CompassDirection.South => "zuiden",
        CompassDirection.SouthWest => "zuidwesten",
        CompassDirection.West => "westen",
        CompassDirection.NorthWest => "noordwesten",
        _ => "?",
    };

    // The phrase without intensity, and whether it starts with a neuter ("het") noun, which takes an
    // uninflected adjective ("licht onweer", "laag opwaaiend zand" versus "lichte regen").
    private static (string Text, bool Neuter) Core(WeatherPhenomenon weather)
    {
        IReadOnlyList<WeatherType> types = weather.Types;
        string list = JoinAnd(types.Select(Noun).ToList(), "en");
        bool neuter = types.Count > 0 && IsNeuter(types[0]);
        bool single = types.Count == 1;
        switch (weather.Descriptor)
        {
            case null:
                return (list, neuter);
            case WeatherDescriptor.Showers:
                if (types.Count == 0)
                {
                    return ("buien", false);
                }

                if (single && types[0] == WeatherType.Rain)
                {
                    return ("regenbuien", false);
                }

                if (single && types[0] == WeatherType.Snow)
                {
                    return ("sneeuwbuien", false);
                }

                if (single && types[0] == WeatherType.Hail)
                {
                    return ("hagelbuien", false);
                }

                return ("buien met " + list, false);
            case WeatherDescriptor.Thunderstorm:
                return types.Count == 0 ? ("onweer", true) : ("onweersbuien met " + list, false);
            case WeatherDescriptor.Freezing:
                return single && types[0] == WeatherType.Fog ? ("aanvriezende mist", false) : ("onderkoelde " + list, false);
            case WeatherDescriptor.Shallow:
                return (Adjective("ondiep", neuter) + list, neuter);
            case WeatherDescriptor.Partial:
                return (Adjective("gedeeltelijk", neuter) + list, neuter);
            case WeatherDescriptor.Patches:
                return single && types[0] == WeatherType.Fog ? ("mistbanken", false) : ("plaatselijk " + list, neuter);
            case WeatherDescriptor.LowDrifting:
                return ("laag " + Adjective("opwaaiend", neuter) + list, neuter);
            case WeatherDescriptor.Blowing:
                return ("hoog " + Adjective("opwaaiend", neuter) + list, neuter);
            default:
                return ("?", false);
        }
    }

    private static string Adjective(string stem, bool neuter) => neuter ? stem + " " : stem + "e ";

    private static bool IsNeuter(WeatherType type) => type == WeatherType.Dust || type == WeatherType.Sand;

    private static string Noun(WeatherType type) => type switch
    {
        WeatherType.Drizzle => "motregen",
        WeatherType.Rain => "regen",
        WeatherType.Snow => "sneeuw",
        WeatherType.SnowGrains => "motsneeuw",
        WeatherType.IceCrystals => "ijsnaalden",
        WeatherType.IcePellets => "ijskorrels",
        WeatherType.Hail => "hagel",
        WeatherType.SmallHail => "korrelhagel",
        WeatherType.UnknownPrecipitation => "onbekende neerslag",
        WeatherType.Mist => "nevel",
        WeatherType.Fog => "mist",
        WeatherType.Smoke => "rook",
        WeatherType.VolcanicAsh => "vulkanische as",
        WeatherType.Dust => "stof",
        WeatherType.Sand => "zand",
        WeatherType.Haze => "heiigheid",
        WeatherType.DustWhirls => "stof- of zandhozen",
        WeatherType.Squalls => "rukwinden",
        WeatherType.FunnelCloud => "trechterwolk",
        WeatherType.Sandstorm => "zandstorm",
        WeatherType.Duststorm => "stofstorm",
        _ => "?",
    };

    // "van 11.000 tot 20.000 ft", "tot 2.000 ft" (base at the surface) or "vanaf 5.000 ft tot de wolkentoppen"
    // (thickness digit 0, WMO code table 4013).
    private string LayerText(int baseFeet, int thicknessFeet)
    {
        if (thicknessFeet == 0)
        {
            return (baseFeet == 0 ? "vanaf het oppervlak" : "vanaf " + Number(baseFeet) + " ft") + " tot de wolkentoppen";
        }

        string top = Number(baseFeet + thicknessFeet) + " ft";
        return baseFeet == 0 ? "tot " + top : "van " + Number(baseFeet) + " tot " + top;
    }

    private static string Deposit(int code) => code switch
    {
        0 => "schoon en droog",
        1 => "vochtig",
        2 => "nat of plassen",
        3 => "rijp of vorst",
        4 => "droge sneeuw",
        5 => "natte sneeuw",
        6 => "sneeuwbrij",
        7 => "ijs",
        8 => "aangedrukte of gewalste sneeuw",
        9 => "bevroren sporen of richels",
        _ => "afzettingscode " + Int(code),
    };

    private static string Extent(int code) => code switch
    {
        1 => "bedekking 10% of minder",
        2 => "bedekking 11–25%",
        5 => "bedekking 26–50%",
        9 => "bedekking 51–100%",
        _ => "bedekkingscode " + Int(code),
    };

    private static string Depth(int code) => code switch
    {
        0 => "diepte minder dan 1 mm",
        >= 1 and <= 90 => "diepte " + Int(code) + " mm",
        92 => "diepte 10 cm",
        93 => "diepte 15 cm",
        94 => "diepte 20 cm",
        95 => "diepte 25 cm",
        96 => "diepte 30 cm",
        97 => "diepte 35 cm",
        98 => "diepte 40 cm of meer",
        99 => "baan niet in gebruik",
        _ => "dieptecode " + Int(code),
    };

    private string Friction(int code) => code switch
    {
        >= 1 and <= 90 => "wrijvingscoëfficiënt " + Fixed(code / 100.0, "0.00"),
        91 => "remwerking slecht",
        92 => "remwerking matig tot slecht",
        93 => "remwerking matig",
        94 => "remwerking matig tot goed",
        95 => "remwerking goed",
        99 => "remwerking onbetrouwbaar",
        _ => "wrijvingscode " + Int(code),
    };
}
