using System.Globalization;

namespace Cavok.Text;

// All language-specific text. Switch defaults return "?" so the translation coverage tests catch gaps.
internal abstract class Phrasebook
{
    public abstract string NotAvailable { get; }

    public abstract string NilReport { get; }

    public abstract string Cancelled { get; }

    public abstract string CavokExplained { get; }

    public abstract string NoSignificantWeather { get; }

    protected abstract NumberFormatInfo Numbers { get; }

    public static Phrasebook For(Language language) =>
        language == Language.Dutch ? DutchPhrasebook.Instance : EnglishPhrasebook.Instance;

    public abstract string Label(Field field);

    public abstract string MetarHeader(ReportType type, string? station, DayTime? time, bool isAuto, bool isCorrected);

    public abstract string TafHeader(string? station, DayTime? issued, ValidityPeriod? validity, bool isAmended, bool isCorrected);

    public abstract string ChangeHeader(TafChange change);

    public abstract string TrendPrefix(Trend trend);

    public abstract string WindText(Wind wind);

    public abstract string VisibilityText(Visibility visibility);

    public abstract string RvrText(RunwayVisualRange rvr);

    public abstract string WeatherText(WeatherPhenomenon weather);

    public abstract string CloudText(CloudLayer layer);

    public abstract string CloudConditionText(CloudCondition condition);

    public abstract string TemperatureText(int? temperature, int? dewPoint);

    public abstract string WindShearText(WindShear windShear);

    public abstract string SeaText(SeaCondition sea);

    public abstract string RunwayStateText(RunwayState state);

    public abstract string ColorCodeText(ColorCode code);

    public abstract string TemperatureForecastText(TemperatureForecast forecast);

    public abstract string IcingText(IcingLayer layer);

    public abstract string TurbulenceText(TurbulenceLayer layer);

    public abstract string InlineWind(string text);

    public abstract string InlineVisibility(string text);

    public abstract string InlineColorCode(string text);

    public string PressureText(Pressure pressure) => pressure.Unit == PressureUnit.Hectopascals
        ? Math.Round(pressure.Value).ToString("0", CultureInfo.InvariantCulture) + " hPa"
        : Fixed(pressure.Value, "0.00") + " inHg";

    protected static string Int(int value) => value.ToString(CultureInfo.InvariantCulture);

    protected static string Clock(int hour, int minute) =>
        hour.ToString("00", CultureInfo.InvariantCulture) + ":" + minute.ToString("00", CultureInfo.InvariantCulture);

    protected static string Degrees(int degrees) => degrees.ToString("000", CultureInfo.InvariantCulture) + "°";

    protected static string Celsius(int celsius) => Int(celsius) + " °C";

    protected static string Unit(SpeedUnit unit) => unit switch
    {
        SpeedUnit.Knots => "kt",
        SpeedUnit.MetersPerSecond => "m/s",
        SpeedUnit.KilometersPerHour => "km/h",
        _ => "?",
    };

    protected static bool IsTornado(WeatherPhenomenon weather) =>
        weather.Intensity == WeatherIntensity.Heavy
        && weather.Descriptor is null
        && weather.Types.Count == 1
        && weather.Types[0] == WeatherType.FunnelCloud;

    protected static string JoinAnd(IReadOnlyList<string> items, string conjunction) => items.Count switch
    {
        0 => "",
        1 => items[0],
        _ => string.Join(", ", items.Take(items.Count - 1)) + " " + conjunction + " " + items[items.Count - 1],
    };

    protected string Number(int value) => value.ToString("#,0", Numbers);

    protected string Fixed(double value, string format) => value.ToString(format, Numbers);
}
