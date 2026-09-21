namespace Cavok.Parsing;

internal readonly struct WindVariation
{
    public WindVariation(int from, int to)
    {
        From = from;
        To = to;
    }

    public int From { get; }

    public int To { get; }
}

internal static class WindParser
{
    // dddff(Gff)KT|MPS|KMH where ddd is 000-360, VRB or ///, ff is 2-3 digits (optionally P-prefixed) or //.
    public static Wind? Parse(string s)
    {
        SpeedUnit unit;
        int bodyLength;
        if (s.EndsWith("KT", StringComparison.Ordinal))
        {
            unit = SpeedUnit.Knots;
            bodyLength = s.Length - 2;
        }
        else if (s.EndsWith("MPS", StringComparison.Ordinal))
        {
            unit = SpeedUnit.MetersPerSecond;
            bodyLength = s.Length - 3;
        }
        else if (s.EndsWith("KMH", StringComparison.Ordinal))
        {
            unit = SpeedUnit.KilometersPerHour;
            bodyLength = s.Length - 3;
        }
        else
        {
            return null;
        }

        if (bodyLength < 5)
        {
            return null;
        }

        string body = s.Substring(0, bodyLength);
        string direction = body.Substring(0, 3);
        int? degrees = null;
        bool variable = direction == "VRB";
        bool directionMissing = direction == "///";
        if (!variable && !directionMissing)
        {
            degrees = Scan.Number(direction, 0, 3);
            if (degrees is null || degrees > 360)
            {
                return null;
            }
        }

        int i = 3;
        if (!TryReadSpeed(body, ref i, unit, out Speed? speed))
        {
            return null;
        }

        Speed? gust = null;
        if (i < body.Length)
        {
            if (body[i] != 'G')
            {
                return null;
            }

            i++;
            if (!TryReadSpeed(body, ref i, unit, out gust))
            {
                return null;
            }
        }

        if (i != body.Length)
        {
            return null;
        }

        return new Wind
        {
            Direction = degrees,
            IsVariable = variable,
            Speed = speed,
            Gust = gust,
            IsCalm = degrees == 0 && speed is { Value: 0 } && gust is null,
            IsMissing = directionMissing && speed is null,
        };
    }

    // dddVddd
    public static WindVariation? ParseVariation(string s)
    {
        if (s.Length != 7 || s[3] != 'V')
        {
            return null;
        }

        int? from = Scan.Number(s, 0, 3);
        int? to = Scan.Number(s, 4, 3);
        if (from is null || to is null || from > 360 || to > 360)
        {
            return null;
        }

        return new WindVariation(from.Value, to.Value);
    }

    // Reads "//" (missing) or P?\d{2,3}.
    private static bool TryReadSpeed(string body, ref int i, SpeedUnit unit, out Speed? speed)
    {
        speed = null;
        if (Scan.AreSlashes(body, i, 2))
        {
            i += 2;
            return true;
        }

        bool above = i < body.Length && body[i] == 'P';
        if (above)
        {
            i++;
        }

        int start = i;
        while (i < body.Length && i - start < 3 && Scan.IsDigit(body[i]))
        {
            i++;
        }

        int? value = Scan.Number(body, start, i - start);
        if (value is null || i - start < 2)
        {
            return false;
        }

        speed = new Speed(value.Value, unit, above);
        return true;
    }
}
