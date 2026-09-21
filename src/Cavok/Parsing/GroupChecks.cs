namespace Cavok.Parsing;

// Plausibility warnings for groups that were understood.
internal static class GroupChecks
{
    public static void Check(Group group, Token first, Token last, DiagnosticBag diagnostics)
    {
        if (group.Value is Wind { Speed: Speed speed, Gust: Speed gust } && gust.Value <= speed.Value)
        {
            diagnostics.Warning(DiagnosticCode.GustNotAboveSpeed, first, last);
        }

        if (group.Value is TemperaturePair { Temperature: int temperature, DewPoint: int dewPoint } && dewPoint > temperature)
        {
            diagnostics.Warning(DiagnosticCode.DewPointAboveTemperature, first, last);
        }
    }
}
