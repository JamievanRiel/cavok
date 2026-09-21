namespace Cavok;

/// <summary>
/// A day of the month with a UTC time, as used for METAR observation times and TAF issue times
/// (for example <c>211125Z</c>). Reports do not contain the month or year.
/// </summary>
/// <param name="Day">Day of the month, 1–31.</param>
/// <param name="Hour">Hour, 0–24 (24 only occurs in forecast periods).</param>
/// <param name="Minute">Minute, 0–59.</param>
public readonly record struct DayTime(int Day, int Hour, int Minute)
{
    /// <summary>
    /// Resolves the day and time to a full UTC timestamp. Of the months around <paramref name="reference"/>
    /// in which <see cref="Day"/> exists, the one giving the timestamp closest to <paramref name="reference"/>
    /// is used, so month and year boundaries are handled.
    /// </summary>
    /// <param name="reference">A moment close to the report, typically the moment it was received.</param>
    /// <returns>The resolved UTC timestamp.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><see cref="Day"/> is not between 1 and 31.</exception>
    public DateTimeOffset ToDateTimeOffset(DateTimeOffset reference)
    {
        if (Day < 1 || Day > 31)
        {
            throw new ArgumentOutOfRangeException(nameof(Day), Day, "The day must be between 1 and 31.");
        }

        DateTime utc = reference.UtcDateTime;
        var firstOfMonth = new DateTime(utc.Year, utc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTimeOffset? best = null;
        for (int offset = -2; offset <= 1; offset++)
        {
            DateTime month = firstOfMonth.AddMonths(offset);
            if (Day > DateTime.DaysInMonth(month.Year, month.Month))
            {
                continue;
            }

            DateTimeOffset candidate = new DateTimeOffset(month.Year, month.Month, Day, 0, 0, 0, TimeSpan.Zero)
                .AddHours(Hour)
                .AddMinutes(Minute);
            if (best is null || (candidate - reference).Duration() < (best.Value - reference).Duration())
            {
                best = candidate;
            }
        }

        // Any two consecutive months include one with 31 days, so a candidate always exists.
        return best ?? throw new InvalidOperationException("No month contains the requested day.");
    }
}

/// <summary>A day of the month with a whole UTC hour, as used in TAF periods (for example <c>2106</c>).</summary>
/// <param name="Day">Day of the month, 1–31.</param>
/// <param name="Hour">Hour, 0–24. Hour 24 is midnight at the end of the day.</param>
public readonly record struct DayHour(int Day, int Hour)
{
    /// <summary>Resolves to a full UTC timestamp; see <see cref="DayTime.ToDateTimeOffset(DateTimeOffset)"/>.</summary>
    /// <param name="reference">A moment close to the report.</param>
    /// <returns>The resolved UTC timestamp.</returns>
    public DateTimeOffset ToDateTimeOffset(DateTimeOffset reference) =>
        new DayTime(Day, Hour, 0).ToDateTimeOffset(reference);
}

/// <summary>A UTC time of day as used in METAR trend groups (<c>FM1030</c>, <c>TL1100</c>, <c>AT1200</c>).</summary>
/// <param name="Hour">Hour, 0–24.</param>
/// <param name="Minute">Minute, 0–59.</param>
public readonly record struct TimeOfDay(int Hour, int Minute);

/// <summary>A TAF validity or change period (<c>DDHH/DDHH</c>).</summary>
/// <param name="From">Start of the period.</param>
/// <param name="To">End of the period.</param>
public readonly record struct ValidityPeriod(DayHour From, DayHour To);
