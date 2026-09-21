namespace Cavok.Parsing;

// Recognises the group at the cursor without consuming it. Context-free: the METAR and TAF parsers decide
// whether a recognised group is allowed where it appears. Keywords (NOSIG, TEMPO, BECMG, FM…, PROB…, TX/TN,
// RMK) are not groups and return null.
internal static class GroupReader
{
    public static Group? Read(TokenCursor cursor)
    {
        string text = cursor.Current.Text;
        switch (text)
        {
            case "AUTO":
                return new Group(GroupKind.Auto, 1, null);
            case "COR":
                return new Group(GroupKind.Corrected, 1, null);
            case "CAVOK":
                return new Group(GroupKind.Cavok, 1, null);
            case "NSW":
                return new Group(GroupKind.NoSignificantWeather, 1, null);
        }

        if (CloudParser.ParseCondition(text) is CloudCondition condition)
        {
            return new Group(GroupKind.CloudCondition, 1, condition);
        }

        if (WindParser.Parse(text) is Wind wind)
        {
            return new Group(GroupKind.Wind, 1, wind);
        }

        if (WindParser.ParseVariation(text) is WindVariation variation)
        {
            return new Group(GroupKind.WindVariation, 1, variation);
        }

        if (VisibilityParser.Read(cursor, out int visibilityTokens) is Visibility visibility)
        {
            return new Group(GroupKind.Visibility, visibilityTokens, visibility);
        }

        if (VisibilityParser.ParseMinimum(text) is MinimumVisibility minimum)
        {
            return new Group(GroupKind.MinimumVisibility, 1, minimum);
        }

        if (RunwayStateParser.Parse(text) is RunwayState state)
        {
            return new Group(GroupKind.RunwayState, 1, state);
        }

        if (RvrParser.Parse(text) is RunwayVisualRange rvr)
        {
            return new Group(GroupKind.RunwayVisualRange, 1, rvr);
        }

        if (WeatherParser.Parse(text) is WeatherPhenomenon weather)
        {
            return new Group(GroupKind.Weather, 1, weather);
        }

        if (text.Length > 2 && text.StartsWith("RE", StringComparison.Ordinal)
            && WeatherParser.Parse(text.Substring(2)) is WeatherPhenomenon recent)
        {
            return new Group(GroupKind.RecentWeather, 1, recent);
        }

        if (CloudParser.ParseLayer(text) is CloudLayer layer)
        {
            return new Group(GroupKind.Cloud, 1, layer);
        }

        if (TemperatureParser.Parse(text) is TemperaturePair temperature)
        {
            return new Group(GroupKind.Temperature, 1, temperature);
        }

        if (PressureParser.TryParse(text, out Pressure? pressure))
        {
            return new Group(GroupKind.Pressure, 1, pressure);
        }

        if (WindShearParser.Read(cursor, out int windShearTokens) is WindShear windShear)
        {
            return new Group(GroupKind.WindShear, windShearTokens, windShear);
        }

        if (SeaParser.Parse(text) is SeaCondition sea)
        {
            return new Group(GroupKind.Sea, 1, sea);
        }

        if (ColorCodeParser.Parse(text) is IReadOnlyList<ColorCode> colors)
        {
            return new Group(GroupKind.ColorCode, 1, colors);
        }

        return null;
    }
}
