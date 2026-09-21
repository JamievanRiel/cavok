namespace Cavok;

internal static class FlightCategoryCalculator
{
    private const double Mile = 1609.344;

    // The worse of the visibility and the ceiling category; when only one is known it decides. The ceiling
    // is the lowest broken, overcast or vertical-visibility layer with a known height. Reported cloud
    // without a ceiling (few/scattered layers, NSC, NCD, SKC, CLR) counts as "no ceiling".
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
            byVisibility = meters < Mile ? FlightCategory.Lifr
                : meters < 3 * Mile ? FlightCategory.Ifr
                : meters <= 5 * Mile ? FlightCategory.Mvfr
                : FlightCategory.Vfr;
        }

        int? ceiling = null;
        foreach (CloudLayer layer in clouds)
        {
            bool isCeiling = layer.Cover is CloudCover.Broken or CloudCover.Overcast or CloudCover.VerticalVisibility;
            if (isCeiling && layer.HeightFeet is int height && (ceiling is null || height < ceiling))
            {
                ceiling = height;
            }
        }

        FlightCategory? byCeiling = null;
        if (ceiling is int feet)
        {
            byCeiling = feet < 500 ? FlightCategory.Lifr
                : feet < 1000 ? FlightCategory.Ifr
                : feet <= 3000 ? FlightCategory.Mvfr
                : FlightCategory.Vfr;
        }
        else if (cloudCondition is not null || clouds.Any(layer => layer.HeightFeet is not null))
        {
            byCeiling = FlightCategory.Vfr;
        }

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
}
