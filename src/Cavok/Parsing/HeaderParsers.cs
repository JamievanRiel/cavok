namespace Cavok.Parsing;

internal static class HeaderParsers
{
    public static bool IsStation(string s) =>
        s.Length == 4 && Scan.IsLetter(s[0]) && Scan.IsAlphanumeric(s) && s != "AUTO";

    public static bool LooksLikeStation(string s) =>
        (s.Length == 3 || s.Length == 5) && Scan.IsLetter(s[0]) && Scan.IsAlphanumeric(s);

    public static string? ReadStation(TokenCursor cursor, DiagnosticBag diagnostics)
    {
        if (!cursor.AtEnd && IsStation(cursor.Current.Text))
        {
            string station = cursor.Current.Text;
            cursor.Consume();
            return station;
        }

        if (!cursor.AtEnd && LooksLikeStation(cursor.Current.Text))
        {
            diagnostics.Error(DiagnosticCode.InvalidStation, cursor.Current);
            cursor.Skip();
            return null;
        }

        diagnostics.ErrorAt(DiagnosticCode.MissingStation, cursor.AtEnd ? diagnostics.SourceLength : cursor.Current.Position);
        return null;
    }

    public static DayTime? ReadTime(TokenCursor cursor, DiagnosticBag diagnostics)
    {
        if (!cursor.AtEnd && TimeParsers.ParseDayTime(cursor.Current.Text) is DayTime time)
        {
            cursor.Consume();
            return time;
        }

        if (!cursor.AtEnd && TimeParsers.LooksLikeDayTime(cursor.Current.Text))
        {
            diagnostics.Error(DiagnosticCode.InvalidTime, cursor.Current);
            cursor.Skip();
            return null;
        }

        diagnostics.ErrorAt(DiagnosticCode.MissingTime, cursor.AtEnd ? diagnostics.SourceLength : cursor.Current.Position);
        return null;
    }
}
