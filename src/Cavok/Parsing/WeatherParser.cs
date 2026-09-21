namespace Cavok.Parsing;

internal static class WeatherParser
{
    private static readonly (string Code, WeatherDescriptor Descriptor)[] Descriptors =
    {
        ("MI", WeatherDescriptor.Shallow),
        ("PR", WeatherDescriptor.Partial),
        ("BC", WeatherDescriptor.Patches),
        ("DR", WeatherDescriptor.LowDrifting),
        ("BL", WeatherDescriptor.Blowing),
        ("SH", WeatherDescriptor.Showers),
        ("TS", WeatherDescriptor.Thunderstorm),
        ("FZ", WeatherDescriptor.Freezing),
    };

    private static readonly (string Code, WeatherType Type)[] Types =
    {
        ("DZ", WeatherType.Drizzle),
        ("RA", WeatherType.Rain),
        ("SN", WeatherType.Snow),
        ("SG", WeatherType.SnowGrains),
        ("IC", WeatherType.IceCrystals),
        ("PL", WeatherType.IcePellets),
        ("GR", WeatherType.Hail),
        ("GS", WeatherType.SmallHail),
        ("UP", WeatherType.UnknownPrecipitation),
        ("BR", WeatherType.Mist),
        ("FG", WeatherType.Fog),
        ("FU", WeatherType.Smoke),
        ("VA", WeatherType.VolcanicAsh),
        ("DU", WeatherType.Dust),
        ("SA", WeatherType.Sand),
        ("HZ", WeatherType.Haze),
        ("PO", WeatherType.DustWhirls),
        ("SQ", WeatherType.Squalls),
        ("FC", WeatherType.FunnelCloud),
        ("SS", WeatherType.Sandstorm),
        ("DS", WeatherType.Duststorm),
    };

    // (-|+|VC)?(descriptor)?(type)* with at least a type, or TS, or VCSH; "//" is not observable.
    public static WeatherPhenomenon? Parse(string s)
    {
        if (s == "//")
        {
            return new WeatherPhenomenon { IsNotObservable = true };
        }

        int i = 0;
        WeatherIntensity intensity = WeatherIntensity.Moderate;
        if (s.Length > 0 && s[0] == '-')
        {
            intensity = WeatherIntensity.Light;
            i = 1;
        }
        else if (s.Length > 0 && s[0] == '+')
        {
            intensity = WeatherIntensity.Heavy;
            i = 1;
        }
        else if (s.StartsWith("VC", StringComparison.Ordinal))
        {
            intensity = WeatherIntensity.InVicinity;
            i = 2;
        }

        WeatherDescriptor? descriptor = null;
        foreach ((string code, WeatherDescriptor value) in Descriptors)
        {
            if (Matches(s, i, code))
            {
                descriptor = value;
                i += 2;
                break;
            }
        }

        var types = new List<WeatherType>();
        while (i < s.Length)
        {
            int before = i;
            foreach ((string code, WeatherType value) in Types)
            {
                if (Matches(s, i, code))
                {
                    types.Add(value);
                    i += 2;
                    break;
                }
            }

            if (i == before)
            {
                return null;
            }
        }

        if (types.Count == 0)
        {
            bool allowed = descriptor == WeatherDescriptor.Thunderstorm
                || (intensity == WeatherIntensity.InVicinity && descriptor == WeatherDescriptor.Showers);
            if (!allowed)
            {
                return null;
            }
        }

        return new WeatherPhenomenon { Intensity = intensity, Descriptor = descriptor, Types = types.ToArray() };
    }

    private static bool Matches(string s, int start, string code) =>
        start + 2 <= s.Length && s[start] == code[0] && s[start + 1] == code[1];
}
