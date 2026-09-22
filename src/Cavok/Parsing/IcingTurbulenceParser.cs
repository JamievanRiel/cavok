namespace Cavok.Parsing;

// The icing (6IchihihitL) and turbulence (5BhBhBhBtL) groups of military TAFs: a type code, the base of the layer
// in hundreds of feet and its thickness in thousands of feet.
internal static class IcingTurbulenceParser
{
    public static IcingLayer? ParseIcing(string s) =>
        TryRead(s, '6', out int type, out int baseFeet, out int thicknessFeet)
            ? new IcingLayer(type, baseFeet, thicknessFeet)
            : null;

    public static TurbulenceLayer? ParseTurbulence(string s) =>
        TryRead(s, '5', out int type, out int baseFeet, out int thicknessFeet)
            ? new TurbulenceLayer(type, baseFeet, thicknessFeet)
            : null;

    private static bool TryRead(string s, char indicator, out int type, out int baseFeet, out int thicknessFeet)
    {
        type = 0;
        baseFeet = 0;
        thicknessFeet = 0;
        if (s.Length != 6 || s[0] != indicator)
        {
            return false;
        }

        int? code = Scan.Number(s, 1, 1);
        int? height = Scan.Number(s, 2, 3);
        int? thickness = Scan.Number(s, 5, 1);
        if (code is null || height is null || thickness is null)
        {
            return false;
        }

        type = code.Value;
        baseFeet = height.Value * 100;
        thicknessFeet = thickness.Value * 1000;
        return true;
    }
}
