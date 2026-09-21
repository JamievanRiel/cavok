namespace Cavok.Parsing;

internal static class ColorCodeParser
{
    // Longer codes first so "BLU+" wins over "BLU" and "YLO1" over "YLO".
    private static readonly (string Code, ColorState State)[] Codes =
    {
        ("BLU+", ColorState.BluePlus),
        ("BLU", ColorState.Blue),
        ("WHT", ColorState.White),
        ("GRN", ColorState.Green),
        ("YLO1", ColorState.Yellow1),
        ("YLO2", ColorState.Yellow2),
        ("YLO", ColorState.Yellow),
        ("AMB", ColorState.Amber),
        ("RED", ColorState.Red),
    };

    // One or more colour states, possibly concatenated (BLU+BLU+), each optionally prefixed with BLACK.
    public static IReadOnlyList<ColorCode>? Parse(string s)
    {
        var codes = new List<ColorCode>();
        int i = 0;
        while (i < s.Length)
        {
            bool black = StartsAt(s, i, "BLACK");
            if (black)
            {
                i += 5;
            }

            int matched = -1;
            for (int k = 0; k < Codes.Length; k++)
            {
                if (StartsAt(s, i, Codes[k].Code))
                {
                    matched = k;
                    break;
                }
            }

            if (matched < 0)
            {
                return null;
            }

            codes.Add(new ColorCode(Codes[matched].State, black));
            i += Codes[matched].Code.Length;
        }

        return codes.Count > 0 ? codes.ToArray() : null;
    }

    private static bool StartsAt(string s, int start, string value) =>
        start + value.Length <= s.Length && string.CompareOrdinal(s, start, value, 0, value.Length) == 0;
}
