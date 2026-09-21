namespace Cavok.Parsing;

internal static class TimeParsers
{
    // DDHHMMZ
    public static DayTime? ParseDayTime(string s) =>
        s.Length == 7 && s[6] == 'Z' ? DayHourMinute(s, 0) : null;

    // FMDDHHMM (TAF change group)
    public static DayTime? ParseFromGroup(string s) =>
        s.Length == 8 && s.StartsWith("FM", StringComparison.Ordinal) ? DayHourMinute(s, 2) : null;

    public static bool LooksLikeDayTime(string s) =>
        s.Length >= 5 && s.Length <= 8 && s[s.Length - 1] == 'Z' && Scan.CountDigits(s) >= 4;

    // DDHH/DDHH
    public static ValidityPeriod? ParsePeriod(string s)
    {
        if (s.Length != 9 || s[4] != '/')
        {
            return null;
        }

        DayHour? from = DayHourAt(s, 0);
        DayHour? to = DayHourAt(s, 5);
        return from is DayHour f && to is DayHour t ? new ValidityPeriod(f, t) : null;
    }

    // A malformed DDHH/DDHH or the obsolete DDHHHH format.
    public static bool LooksLikePeriod(string s) =>
        (s.Length == 9 && s[4] == '/' && Scan.CountDigits(s) >= 6) || (s.Length == 6 && Scan.AreDigits(s, 0, 6));

    // FMhhmm, TLhhmm, AThhmm (METAR trend)
    public static bool IsTrendTimeShape(string s) =>
        s.Length == 6
        && (s.StartsWith("FM", StringComparison.Ordinal)
            || s.StartsWith("TL", StringComparison.Ordinal)
            || s.StartsWith("AT", StringComparison.Ordinal));

    public static TimeOfDay? ParseTimeOfDay(string s, int start)
    {
        int? hour = Scan.Number(s, start, 2);
        int? minute = Scan.Number(s, start + 2, 2);
        if (hour is null || minute is null || hour > 24 || minute > 59 || (hour == 24 && minute != 0))
        {
            return null;
        }

        return new TimeOfDay(hour.Value, minute.Value);
    }

    // TX15/2114Z, TNM02/2205Z
    public static TemperatureForecast? ParseTemperatureForecast(string s)
    {
        if (s.Length < 10 || s[0] != 'T' || (s[1] != 'X' && s[1] != 'N'))
        {
            return null;
        }

        int i = 2;
        bool negative = s[i] == 'M';
        if (negative)
        {
            i++;
        }

        int? value = Scan.Number(s, i, 2);
        i += 2;
        if (value is null || s.Length != i + 6 || s[i] != '/' || s[i + 5] != 'Z')
        {
            return null;
        }

        DayHour? time = DayHourAt(s, i + 1);
        if (time is null)
        {
            return null;
        }

        TemperatureKind kind = s[1] == 'X' ? TemperatureKind.Maximum : TemperatureKind.Minimum;
        return new TemperatureForecast(kind, negative ? -value.Value : value.Value, time.Value);
    }

    private static DayTime? DayHourMinute(string s, int start)
    {
        int? day = Scan.Number(s, start, 2);
        int? hour = Scan.Number(s, start + 2, 2);
        int? minute = Scan.Number(s, start + 4, 2);
        if (day is null || hour is null || minute is null || day < 1 || day > 31 || hour > 24 || minute > 59
            || (hour == 24 && minute != 0))
        {
            return null;
        }

        return new DayTime(day.Value, hour.Value, minute.Value);
    }

    private static DayHour? DayHourAt(string s, int start)
    {
        int? day = Scan.Number(s, start, 2);
        int? hour = Scan.Number(s, start + 2, 2);
        if (day is null || hour is null || day < 1 || day > 31 || hour > 24)
        {
            return null;
        }

        return new DayHour(day.Value, hour.Value);
    }
}
