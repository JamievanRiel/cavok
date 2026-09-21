namespace Cavok.Parsing;

internal static class VisibilityParser
{
    public const double MetersPerStatuteMile = 1609.344;

    // Reads a visibility group at the cursor, including the two-token form "1 1/2SM".
    public static Visibility? Read(TokenCursor cursor, out int tokenCount)
    {
        tokenCount = 1;
        string s = cursor.Current.Text;
        Visibility? visibility = ParseMetric(s) ?? ParseStatuteMiles(s);
        if (visibility is not null)
        {
            return visibility;
        }

        string? next = cursor.PeekText(1);
        if (next is null || s.Length > 2 || !Scan.AreDigits(s, 0, s.Length) || next.IndexOf('/') < 0)
        {
            return null;
        }

        Visibility? fraction = ParseStatuteMiles(next);
        if (fraction is { StatuteMiles: double miles, IsLessThan: false, IsMoreThan: false } && miles < 1)
        {
            tokenCount = 2;
            return fraction with { StatuteMiles = (Scan.Number(s, 0, s.Length) ?? 0) + miles };
        }

        return null;
    }

    // 0000-9999, 9999NDV, ////, ////NDV
    public static Visibility? ParseMetric(string s)
    {
        bool ndv = s.Length == 7 && s.EndsWith("NDV", StringComparison.Ordinal);
        if (s.Length != 4 && !ndv)
        {
            return null;
        }

        if (Scan.AreSlashes(s, 0, 4))
        {
            return new Visibility { IsMissing = true, NoDirectionalVariation = ndv };
        }

        int? meters = Scan.Number(s, 0, 4);
        if (meters is null)
        {
            return null;
        }

        bool tenKm = meters == 9999;
        return new Visibility
        {
            Meters = tenKm ? 10000 : meters,
            IsTenKmOrMore = tenKm,
            NoDirectionalVariation = ndv,
        };
    }

    // 10SM, P6SM, 1/2SM, M1/4SM
    public static Visibility? ParseStatuteMiles(string s)
    {
        if (s.Length < 3 || !s.EndsWith("SM", StringComparison.Ordinal))
        {
            return null;
        }

        string body = s.Substring(0, s.Length - 2);
        bool less = body[0] == 'M';
        bool more = body[0] == 'P';
        if (less || more)
        {
            body = body.Substring(1);
        }

        double? miles = ParseMiles(body);
        if (miles is null)
        {
            return null;
        }

        return new Visibility { StatuteMiles = miles, IsLessThan = less, IsMoreThan = more };
    }

    // 1500SW: four digits and a compass direction.
    public static MinimumVisibility? ParseMinimum(string s)
    {
        if (s.Length < 5 || s.Length > 6)
        {
            return null;
        }

        int? meters = Scan.Number(s, 0, 4);
        CompassDirection? direction = ParseDirection(s.Substring(4));
        if (meters is null || direction is null)
        {
            return null;
        }

        return new MinimumVisibility(meters.Value, direction);
    }

    public static CompassDirection? ParseDirection(string s) => s switch
    {
        "N" => CompassDirection.North,
        "NE" => CompassDirection.NorthEast,
        "E" => CompassDirection.East,
        "SE" => CompassDirection.SouthEast,
        "S" => CompassDirection.South,
        "SW" => CompassDirection.SouthWest,
        "W" => CompassDirection.West,
        "NW" => CompassDirection.NorthWest,
        _ => null,
    };

    private static double? ParseMiles(string body)
    {
        int slash = body.IndexOf('/');
        if (slash < 0)
        {
            return body.Length is >= 1 and <= 2 ? Scan.Number(body, 0, body.Length) : null;
        }

        int denominatorLength = body.Length - slash - 1;
        if (slash < 1 || slash > 2 || denominatorLength < 1 || denominatorLength > 2)
        {
            return null;
        }

        int? numerator = Scan.Number(body, 0, slash);
        int? denominator = Scan.Number(body, slash + 1, denominatorLength);
        if (numerator is null || denominator is null || denominator == 0)
        {
            return null;
        }

        return (double)numerator.Value / denominator.Value;
    }
}
