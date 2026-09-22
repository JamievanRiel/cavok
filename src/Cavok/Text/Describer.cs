namespace Cavok.Text;

internal static class Describer
{
    public static string Describe(Metar metar, Language language)
    {
        Phrasebook p = Phrasebook.For(language);
        var writer = new DescriptionWriter(p);
        writer.Line(p.MetarHeader(metar.Type, metar.Station, metar.Time, metar.IsAuto, metar.IsCorrected));
        if (metar.IsNil)
        {
            writer.Line(p.NilReport);
            return writer.ToString();
        }

        if (metar.Wind is Wind wind)
        {
            writer.Add(Field.Wind, p.WindText(wind));
        }

        if (metar.IsCavok)
        {
            writer.Add(Field.Visibility, p.CavokExplained);
        }
        else if (metar.Visibility is Visibility visibility)
        {
            writer.Add(Field.Visibility, p.VisibilityText(visibility));
        }

        foreach (RunwayVisualRange rvr in metar.RunwayVisualRanges)
        {
            writer.Add(Field.RunwayVisualRange, p.RvrText(rvr));
        }

        if (metar.Weather.Count > 0)
        {
            writer.Add(Field.Weather, List(metar.Weather.Select(p.WeatherText)));
        }

        if (CloudsText(p, metar.Clouds, metar.CloudCondition) is string clouds)
        {
            writer.Add(Field.Clouds, clouds);
        }

        if (metar.Temperature is not null || metar.DewPoint is not null)
        {
            writer.Add(Field.Temperature, p.TemperatureText(metar.Temperature, metar.DewPoint));
        }

        if (metar.Pressure is Pressure pressure)
        {
            writer.Add(Field.Pressure, p.PressureText(pressure));
        }

        if (metar.RecentWeather.Count > 0)
        {
            writer.Add(Field.RecentWeather, List(metar.RecentWeather.Select(p.WeatherText)));
        }

        foreach (WindShear windShear in metar.WindShear)
        {
            writer.Add(Field.WindShear, p.WindShearText(windShear));
        }

        if (metar.Sea is SeaCondition sea)
        {
            writer.Add(Field.Sea, p.SeaText(sea));
        }

        foreach (RunwayState state in metar.RunwayStates)
        {
            writer.Add(Field.RunwayState, p.RunwayStateText(state));
        }

        if (metar.ColorCodes.Count > 0)
        {
            writer.Add(Field.ColorCode, List(metar.ColorCodes.Select(p.ColorCodeText)));
        }

        if (metar.FlightCategory is FlightCategory category)
        {
            writer.Add(Field.FlightCategory, CategoryText(category));
        }

        foreach (Trend trend in metar.Trends)
        {
            writer.Add(Field.Trend, TrendText(p, trend));
        }

        if (metar.Remarks is string remarks)
        {
            writer.Add(Field.Remarks, remarks);
        }

        return writer.ToString();
    }

    public static string Describe(Taf taf, Language language)
    {
        Phrasebook p = Phrasebook.For(language);
        var writer = new DescriptionWriter(p);
        writer.Line(p.TafHeader(taf.Station, taf.IssueTime, taf.Validity, taf.IsAmended, taf.IsCorrected));
        if (taf.IsNil)
        {
            writer.Line(p.NilReport);
            return writer.ToString();
        }

        if (taf.IsCancelled)
        {
            writer.Line(p.Cancelled);
        }

        WriteConditions(writer, p, taf.Base, 0);
        foreach (TemperatureForecast forecast in taf.Temperatures)
        {
            Field field = forecast.Kind == TemperatureKind.Maximum ? Field.MaximumTemperature : Field.MinimumTemperature;
            writer.Add(field, p.TemperatureForecastText(forecast));
        }

        foreach (TafChange change in taf.Changes)
        {
            writer.Line(p.ChangeHeader(change));
            WriteConditions(writer, p, change.Conditions, 1);
        }

        if (taf.Remarks is string remarks)
        {
            writer.Add(Field.Remarks, remarks);
        }

        return writer.ToString();
    }

    private static void WriteConditions(DescriptionWriter writer, Phrasebook p, ForecastConditions conditions, int indent)
    {
        if (conditions.Wind is Wind wind)
        {
            writer.Add(Field.Wind, p.WindText(wind), indent);
        }

        if (conditions.IsCavok)
        {
            writer.Add(Field.Visibility, p.CavokExplained, indent);
        }
        else if (conditions.Visibility is Visibility visibility)
        {
            writer.Add(Field.Visibility, p.VisibilityText(visibility), indent);
        }

        if (conditions.Weather.Count > 0)
        {
            writer.Add(Field.Weather, List(conditions.Weather.Select(p.WeatherText)), indent);
        }
        else if (conditions.NoSignificantWeather)
        {
            writer.Add(Field.Weather, p.NoSignificantWeather, indent);
        }

        if (CloudsText(p, conditions.Clouds, conditions.CloudCondition) is string clouds)
        {
            writer.Add(Field.Clouds, clouds, indent);
        }

        if (conditions.ColorCodes.Count > 0)
        {
            writer.Add(Field.ColorCode, List(conditions.ColorCodes.Select(p.ColorCodeText)), indent);
        }

        if (conditions.Pressure is Pressure pressure)
        {
            writer.Add(Field.Pressure, p.PressureText(pressure), indent);
        }

        if (conditions.Icing.Count > 0)
        {
            writer.Add(Field.Icing, List(conditions.Icing.Select(p.IcingText)), indent);
        }

        if (conditions.Turbulence.Count > 0)
        {
            writer.Add(Field.Turbulence, List(conditions.Turbulence.Select(p.TurbulenceText)), indent);
        }

        if (conditions.FlightCategory is FlightCategory category)
        {
            writer.Add(Field.FlightCategory, CategoryText(category), indent);
        }
    }

    private static string TrendText(Phrasebook p, Trend trend)
    {
        string prefix = p.TrendPrefix(trend);
        string conditions = trend.Conditions is ForecastConditions c ? InlineConditions(p, c) : "";
        return conditions.Length == 0 ? prefix : prefix + ": " + conditions;
    }

    private static string InlineConditions(Phrasebook p, ForecastConditions conditions)
    {
        var parts = new List<string>();
        if (conditions.Wind is Wind wind)
        {
            parts.Add(p.InlineWind(p.WindText(wind)));
        }

        if (conditions.IsCavok)
        {
            parts.Add("CAVOK");
        }
        else if (conditions.Visibility is Visibility visibility)
        {
            parts.Add(p.InlineVisibility(p.VisibilityText(visibility)));
        }

        parts.AddRange(conditions.Weather.Select(p.WeatherText));
        if (conditions.NoSignificantWeather)
        {
            parts.Add(p.NoSignificantWeather);
        }

        if (CloudsText(p, conditions.Clouds, conditions.CloudCondition) is string clouds)
        {
            parts.Add(clouds);
        }

        if (conditions.ColorCodes.Count > 0)
        {
            parts.Add(p.InlineColorCode(List(conditions.ColorCodes.Select(p.ColorCodeText))));
        }

        if (conditions.Pressure is Pressure pressure)
        {
            parts.Add("QNH " + p.PressureText(pressure));
        }

        parts.AddRange(conditions.Icing.Select(p.IcingText));
        parts.AddRange(conditions.Turbulence.Select(p.TurbulenceText));

        return string.Join("; ", parts);
    }

    private static string? CloudsText(Phrasebook p, IReadOnlyList<CloudLayer> clouds, CloudCondition? condition)
    {
        List<string> parts = clouds.Select(p.CloudText).ToList();
        if (condition is CloudCondition value)
        {
            parts.Add(p.CloudConditionText(value));
        }

        return parts.Count == 0 ? null : string.Join(", ", parts);
    }

    private static string CategoryText(FlightCategory category) => category switch
    {
        FlightCategory.Vfr => "VFR",
        FlightCategory.Mvfr => "MVFR",
        FlightCategory.Ifr => "IFR",
        FlightCategory.Lifr => "LIFR",
        _ => "?",
    };

    private static string List(IEnumerable<string> items) => string.Join(", ", items);
}
