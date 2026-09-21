namespace Cavok.Parsing;

internal static class SeaParser
{
    // W(M?dd|//)/(S(d|/)|H(d{1,3}|/{1,3})) — W15/S4, W///S5, W18/H14, W///H///
    public static SeaCondition? Parse(string s)
    {
        if (s.Length < 5 || s[0] != 'W')
        {
            return null;
        }

        int i = 1;
        int? temperature = null;
        if (Scan.AreSlashes(s, i, 2))
        {
            i += 2;
        }
        else
        {
            bool negative = s[i] == 'M';
            int start = negative ? i + 1 : i;
            int? number = Scan.Number(s, start, 2);
            if (number is null)
            {
                return null;
            }

            temperature = negative ? -number.Value : number.Value;
            i = start + 2;
        }

        if (i + 1 >= s.Length || s[i] != '/')
        {
            return null;
        }

        char kind = s[i + 1];
        string rest = s.Substring(i + 2);
        bool missing = Scan.IsAllSlashes(rest);
        if (kind == 'S' && rest.Length == 1)
        {
            int? state = Scan.Number(rest, 0, 1);
            return state is not null || missing ? new SeaCondition { SeaTemperature = temperature, StateOfSea = state } : null;
        }

        if (kind == 'H' && rest.Length >= 1 && rest.Length <= 3)
        {
            int? height = Scan.Number(rest, 0, rest.Length);
            return height is not null || missing ? new SeaCondition { SeaTemperature = temperature, WaveHeightDecimeters = height } : null;
        }

        return null;
    }
}
