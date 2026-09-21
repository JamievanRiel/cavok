namespace Cavok.Parsing;

internal static class PressureParser
{
    // Q1013 (hPa), A2992 (inHg), QNH3043INS (inHg, UK military TAF). Returns true with a null value
    // for a missing value (Q//// or A////).
    public static bool TryParse(string s, out Pressure? value)
    {
        value = null;
        if (s.Length == 5 && (s[0] == 'Q' || s[0] == 'A'))
        {
            if (Scan.AreSlashes(s, 1, 4))
            {
                return true;
            }

            int? number = Scan.Number(s, 1, 4);
            if (number is null)
            {
                return false;
            }

            value = s[0] == 'Q'
                ? new Pressure(number.Value, PressureUnit.Hectopascals)
                : new Pressure(number.Value / 100.0, PressureUnit.InchesOfMercury);
            return true;
        }

        if (s.Length == 10 && s.StartsWith("QNH", StringComparison.Ordinal) && s.EndsWith("INS", StringComparison.Ordinal))
        {
            int? number = Scan.Number(s, 3, 4);
            if (number is null)
            {
                return false;
            }

            value = new Pressure(number.Value / 100.0, PressureUnit.InchesOfMercury);
            return true;
        }

        return false;
    }
}
