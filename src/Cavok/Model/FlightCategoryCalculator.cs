namespace Cavok;

internal static class FlightCategoryCalculator
{
    private const double Mile = 1609.344;

    // The worse of the visibility and the ceiling category; when only one is known it decides. The ceiling
    // is the lowest broken, overcast or vertical-visibility layer with a known height. Reported cloud
    // without a ceiling (few/scattered layers, NSC, NCD, SKC, CLR) counts as "no ceiling". A ceiling layer of
    // unknown height (BKN///, OVC///, VV///) reported before every known ceiling (layers are reported from the
    // lowest up) makes the ceiling unknown.
    public static FlightCategory? Compute(
        Visibility? visibility, bool isCavok, IReadOnlyList<CloudLayer> clouds, CloudCondition? cloudCondition)
    {
        if (isCavok)
        {
            return FlightCategory.Vfr;
        }

        FlightCategory? byVisibility = null;
        if (visibility?.ToMeters() is double meters)
        {
            // "Less than 3 SM" (M3SM) is below 3 SM and "more than 5 SM" (P5SM) is above 5 SM.
            bool lessThan = visibility.IsLessThan;
            bool moreThan = visibility.IsMoreThan;
            bool Below(double limit) => lessThan ? meters <= limit : meters < limit;
            bool AtMost(double limit) => moreThan ? meters < limit : meters <= limit;
            byVisibility = Below(Mile) ? FlightCategory.Lifr
                : Below(3 * Mile) ? FlightCategory.Ifr
                : AtMost(5 * Mile) ? FlightCategory.Mvfr
                : FlightCategory.Vfr;
        }

        FlightCategory? byCeiling = CeilingCategory(clouds, cloudCondition);

        if (byVisibility is null)
        {
            return byCeiling;
        }

        if (byCeiling is null)
        {
            return byVisibility;
        }

        return (FlightCategory)Math.Max((int)byVisibility.Value, (int)byCeiling.Value);
    }

    // The category of the ceiling; VFR for reported cloud without a ceiling; null when there is no cloud
    // information or the ceiling is unknown.
    private static FlightCategory? CeilingCategory(IReadOnlyList<CloudLayer> clouds, CloudCondition? cloudCondition)
    {
        int? ceiling = null;
        foreach (CloudLayer layer in clouds)
        {
            if (layer.Cover is not (CloudCover.Broken or CloudCover.Overcast or CloudCover.VerticalVisibility))
            {
                continue;
            }

            if (layer.HeightFeet is not int height)
            {
                if (ceiling is null)
                {
                    // A ceiling of unknown height below every known ceiling.
                    return null;
                }

                continue;
            }

            if (ceiling is null || height < ceiling)
            {
                ceiling = height;
            }
        }

        if (ceiling is int feet)
        {
            return feet < 500 ? FlightCategory.Lifr
                : feet < 1000 ? FlightCategory.Ifr
                : feet <= 3000 ? FlightCategory.Mvfr
                : FlightCategory.Vfr;
        }

        return cloudCondition is not null || clouds.Any(layer => layer.HeightFeet is not null) ? FlightCategory.Vfr : null;
    }
}
