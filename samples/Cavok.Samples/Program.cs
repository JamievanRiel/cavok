using Cavok;

#region metar-basic
Metar metar = Metar.Parse("METAR EHAM 211125Z 24012G25KT 200V280 9999 -SHRA FEW012 BKN030CB 12/09 Q1013 TEMPO 4000 SHRA");

Console.WriteLine(metar.Station);                // EHAM
Console.WriteLine(metar.Wind?.Direction);        // 240
Console.WriteLine(metar.Wind?.Gust?.Value);      // 25
Console.WriteLine(metar.Visibility?.Meters);     // 10000 (9999 means 10 km or more)
Console.WriteLine(metar.Clouds[1].Type);         // Cumulonimbus
Console.WriteLine(metar.Pressure?.Hectopascals); // 1013
Console.WriteLine(metar.FlightCategory);         // Mvfr
Console.WriteLine(metar.Trends[0].Kind);         // Temporary
#endregion

#region metar-describe
Console.WriteLine(metar.Describe(Language.English));
Console.WriteLine(metar.Describe(Language.Dutch));
#endregion

#region taf-basic
Taf taf = Taf.Parse("TAF EHAM 210440Z 2106/2212 27005KT 9999 FEW035 BECMG 2106/2109 31011KT PROB30 TEMPO 2114/2118 TSRA");

foreach (TafChange change in taf.Changes)
{
    Console.WriteLine($"{change.Kind} {change.Probability} {change.Period?.From} {change.Conditions.Wind?.Direction}");
}

Console.WriteLine(taf.Describe(Language.English));
#endregion

#region diagnostics
Metar broken = Metar.Parse("METAR EHAM 211125Z 24012KT 2400O -RA FEW012 12/09 Q1013");
foreach (Diagnostic diagnostic in broken.Diagnostics)
{
    Console.WriteLine(diagnostic);
    // error CAV004: invalid visibility group "2400O"; expected 4 digits (e.g. 0800, 9999)
    //   METAR EHAM 211125Z 24012KT 2400O -RA FEW012 12/09 Q1013
    //                              ^^^^^
}

Console.WriteLine(broken.Clouds.Count); // 1: the rest of the report is still parsed
#endregion

#region strict
try
{
    Metar.ParseStrict("METAR EHAM 211125Z 24012KT 2400O Q1013");
}
catch (CavokParseException exception)
{
    Console.WriteLine(exception.Diagnostics[0].Describe(Language.Dutch));
    // ongeldige zichtgroep "2400O"; verwacht 4 cijfers (bijv. 0800, 9999)
}
#endregion

#region datetime
// Reports only contain the day of the month; resolve it against the moment the report was received.
DateTimeOffset observed = metar.Time!.Value.ToDateTimeOffset(new DateTimeOffset(2026, 9, 21, 11, 40, 0, TimeSpan.Zero));
Console.WriteLine(observed.ToString("u", System.Globalization.CultureInfo.InvariantCulture)); // 2026-09-21 11:25:00Z
#endregion
