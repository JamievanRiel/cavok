namespace Cavok.Parsing;

internal static class WindShearParser
{
    // WS R24L, WS RWY24, WS ALL RWY
    public static WindShear? Read(TokenCursor cursor, out int tokenCount)
    {
        tokenCount = 0;
        if (cursor.PeekText() != "WS")
        {
            return null;
        }

        string? next = cursor.PeekText(1);
        if (next == "ALL" && cursor.PeekText(2) == "RWY")
        {
            tokenCount = 3;
            return new WindShear { AllRunways = true };
        }

        string? runway = null;
        if (next is not null && next.StartsWith("RWY", StringComparison.Ordinal))
        {
            runway = next.Substring(3);
        }
        else if (next is not null && next.Length > 1 && next[0] == 'R')
        {
            runway = next.Substring(1);
        }

        if (runway is null || !Runways.IsDesignator(runway))
        {
            return null;
        }

        tokenCount = 2;
        return new WindShear { Runway = runway };
    }
}
