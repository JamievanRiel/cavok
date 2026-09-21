namespace Cavok.Parsing;

internal static class CloudParser
{
    public static CloudCondition? ParseCondition(string s) => s switch
    {
        "NSC" => CloudCondition.NoSignificantCloud,
        "NCD" => CloudCondition.NoCloudDetected,
        "SKC" => CloudCondition.SkyClear,
        "CLR" => CloudCondition.Clear,
        _ => null,
    };

    // (FEW|SCT|BKN|OVC|///)(ddd|///)(CB|TCU|///)? or VV(ddd|///)
    public static CloudLayer? ParseLayer(string s)
    {
        if (s.Length < 5)
        {
            return null;
        }

        CloudCover? cover;
        int i;
        if (s.StartsWith("VV", StringComparison.Ordinal))
        {
            cover = CloudCover.VerticalVisibility;
            i = 2;
        }
        else
        {
            switch (s.Substring(0, 3))
            {
                case "FEW":
                    cover = CloudCover.Few;
                    break;
                case "SCT":
                    cover = CloudCover.Scattered;
                    break;
                case "BKN":
                    cover = CloudCover.Broken;
                    break;
                case "OVC":
                    cover = CloudCover.Overcast;
                    break;
                case "///":
                    cover = null;
                    break;
                default:
                    return null;
            }

            i = 3;
        }

        if (i + 3 > s.Length)
        {
            return null;
        }

        int? height = null;
        if (!Scan.AreSlashes(s, i, 3))
        {
            height = Scan.Number(s, i, 3) * 100;
            if (height is null)
            {
                return null;
            }
        }

        string rest = s.Substring(i + 3);
        if (cover == CloudCover.VerticalVisibility && rest.Length > 0)
        {
            return null;
        }

        switch (rest)
        {
            case "":
                return new CloudLayer { Cover = cover, HeightFeet = height };
            case "CB":
                return new CloudLayer { Cover = cover, HeightFeet = height, Type = CloudType.Cumulonimbus };
            case "TCU":
                return new CloudLayer { Cover = cover, HeightFeet = height, Type = CloudType.ToweringCumulus };
            case "///":
                return new CloudLayer { Cover = cover, HeightFeet = height, IsTypeNotObservable = true };
            default:
                return null;
        }
    }
}
