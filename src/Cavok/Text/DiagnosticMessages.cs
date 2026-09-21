namespace Cavok.Text;

internal static class DiagnosticMessages
{
    public static string Format(DiagnosticCode code, string token, Language language)
    {
        string template = language == Language.Dutch ? Dutch(code) : English(code);
        return template.Replace("{0}", token);
    }

    private static string English(DiagnosticCode code) => code switch
    {
        DiagnosticCode.UnknownGroup => "unknown group \"{0}\"",
        DiagnosticCode.InvalidStation => "invalid station identifier \"{0}\"; expected 4 characters (e.g. EHAM)",
        DiagnosticCode.InvalidTime => "invalid time group \"{0}\"; expected DDHHMMZ (e.g. 211125Z)",
        DiagnosticCode.InvalidVisibility => "invalid visibility group \"{0}\"; expected 4 digits (e.g. 0800, 9999)",
        DiagnosticCode.InvalidWind => "invalid wind group \"{0}\"; expected e.g. 24012KT, 24012G25KT or VRB03KT",
        DiagnosticCode.InvalidWindVariation => "invalid or unusable wind variation \"{0}\"; expected e.g. 200V280 after a wind group",
        DiagnosticCode.InvalidRvr => "invalid runway visual range \"{0}\"; expected e.g. R24/P2000N or R06L/0800V1200U",
        DiagnosticCode.InvalidWeather => "invalid weather group \"{0}\"; expected e.g. -RA, +TSRA or VCSH",
        DiagnosticCode.InvalidCloud => "invalid cloud group \"{0}\"; expected e.g. FEW012, BKN030CB or VV002",
        DiagnosticCode.InvalidTemperature => "invalid temperature group \"{0}\"; expected e.g. 12/09 or M05/M07",
        DiagnosticCode.InvalidPressure => "invalid pressure group \"{0}\"; expected e.g. Q1013 or A2992",
        DiagnosticCode.InvalidRunwayState => "invalid runway state group \"{0}\"; expected e.g. R24/290050 or R24/CLRD62",
        DiagnosticCode.InvalidTrend => "invalid trend time \"{0}\"; expected e.g. FM1030, TL1100 or AT1200",
        DiagnosticCode.InvalidValidity => "invalid validity period \"{0}\"; expected DDHH/DDHH (e.g. 2106/2212)",
        DiagnosticCode.InvalidChangeGroup => "invalid change group \"{0}\"; expected e.g. FM211400, BECMG 2106/2109, TEMPO 2114/2118 or PROB30",
        DiagnosticCode.InvalidTemperatureForecast => "invalid temperature forecast \"{0}\"; expected e.g. TX15/2114Z or TNM02/2205Z",
        DiagnosticCode.WrongReportType => "\"{0}\" is a different report type; use the matching parser (Metar.Parse or Taf.Parse)",
        DiagnosticCode.MissingStation => "station identifier is missing",
        DiagnosticCode.MissingTime => "observation or issue time is missing",
        DiagnosticCode.MissingValidity => "validity period is missing",
        DiagnosticCode.OutOfOrder => "group \"{0}\" is out of the expected order",
        DiagnosticCode.Duplicate => "duplicate group \"{0}\"; the first occurrence is used",
        DiagnosticCode.GustNotAboveSpeed => "gust in \"{0}\" is not higher than the mean wind speed",
        DiagnosticCode.DewPointAboveTemperature => "dew point in \"{0}\" is higher than the temperature",
        DiagnosticCode.EmptyInput => "input is empty",
        DiagnosticCode.InputTooLong => "input is longer than 65,536 characters",
        DiagnosticCode.InternalError => "internal parser error at \"{0}\"; please report this message",
        _ => "unknown diagnostic \"{0}\"",
    };

    private static string Dutch(DiagnosticCode code) => code switch
    {
        DiagnosticCode.UnknownGroup => "onbekende groep \"{0}\"",
        DiagnosticCode.InvalidStation => "ongeldige stationscode \"{0}\"; verwacht 4 tekens (bijv. EHAM)",
        DiagnosticCode.InvalidTime => "ongeldige tijdgroep \"{0}\"; verwacht DDUUMMZ (bijv. 211125Z)",
        DiagnosticCode.InvalidVisibility => "ongeldige zichtgroep \"{0}\"; verwacht 4 cijfers (bijv. 0800, 9999)",
        DiagnosticCode.InvalidWind => "ongeldige windgroep \"{0}\"; verwacht bijv. 24012KT, 24012G25KT of VRB03KT",
        DiagnosticCode.InvalidWindVariation => "ongeldige of onbruikbare windvariatie \"{0}\"; verwacht bijv. 200V280 na een windgroep",
        DiagnosticCode.InvalidRvr => "ongeldig baanzicht \"{0}\"; verwacht bijv. R24/P2000N of R06L/0800V1200U",
        DiagnosticCode.InvalidWeather => "ongeldige weergroep \"{0}\"; verwacht bijv. -RA, +TSRA of VCSH",
        DiagnosticCode.InvalidCloud => "ongeldige wolkengroep \"{0}\"; verwacht bijv. FEW012, BKN030CB of VV002",
        DiagnosticCode.InvalidTemperature => "ongeldige temperatuurgroep \"{0}\"; verwacht bijv. 12/09 of M05/M07",
        DiagnosticCode.InvalidPressure => "ongeldige luchtdrukgroep \"{0}\"; verwacht bijv. Q1013 of A2992",
        DiagnosticCode.InvalidRunwayState => "ongeldige baantoestandgroep \"{0}\"; verwacht bijv. R24/290050 of R24/CLRD62",
        DiagnosticCode.InvalidTrend => "ongeldige trendtijd \"{0}\"; verwacht bijv. FM1030, TL1100 of AT1200",
        DiagnosticCode.InvalidValidity => "ongeldige geldigheidsperiode \"{0}\"; verwacht DDUU/DDUU (bijv. 2106/2212)",
        DiagnosticCode.InvalidChangeGroup => "ongeldige wijzigingsgroep \"{0}\"; verwacht bijv. FM211400, BECMG 2106/2109, TEMPO 2114/2118 of PROB30",
        DiagnosticCode.InvalidTemperatureForecast => "ongeldige temperatuurverwachting \"{0}\"; verwacht bijv. TX15/2114Z of TNM02/2205Z",
        DiagnosticCode.WrongReportType => "\"{0}\" is een ander soort bericht; gebruik de bijbehorende parser (Metar.Parse of Taf.Parse)",
        DiagnosticCode.MissingStation => "stationscode ontbreekt",
        DiagnosticCode.MissingTime => "waarnemings- of uitgiftetijd ontbreekt",
        DiagnosticCode.MissingValidity => "geldigheidsperiode ontbreekt",
        DiagnosticCode.OutOfOrder => "groep \"{0}\" staat niet op de verwachte plek",
        DiagnosticCode.Duplicate => "dubbele groep \"{0}\"; de eerste wordt gebruikt",
        DiagnosticCode.GustNotAboveSpeed => "windstoot in \"{0}\" is niet hoger dan de gemiddelde windsnelheid",
        DiagnosticCode.DewPointAboveTemperature => "dauwpunt in \"{0}\" is hoger dan de temperatuur",
        DiagnosticCode.EmptyInput => "invoer is leeg",
        DiagnosticCode.InputTooLong => "invoer is langer dan 65.536 tekens",
        DiagnosticCode.InternalError => "interne parserfout bij \"{0}\"; meld dit bericht alstublieft",
        _ => "onbekende melding \"{0}\"",
    };
}
