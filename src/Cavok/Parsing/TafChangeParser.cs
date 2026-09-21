namespace Cavok.Parsing;

internal sealed class TafChangeHeader
{
    private readonly TafChangeKind _kind;
    private readonly int? _probability;
    private readonly DayTime? _from;
    private readonly ValidityPeriod? _period;

    public TafChangeHeader(TafChangeKind kind, int? probability, DayTime? from, ValidityPeriod? period)
    {
        _kind = kind;
        _probability = probability;
        _from = from;
        _period = period;
    }

    public TafChange Build(ConditionsBuilder conditions) => new TafChange
    {
        Kind = _kind,
        Probability = _probability,
        From = _from,
        Period = _period,
        Conditions = conditions.Build(),
    };
}

internal static class TafChangeParser
{
    public static bool IsChangeStart(string text) =>
        text == "BECMG"
        || text == "TEMPO"
        || (text.Length == 6 && text.StartsWith("PROB", StringComparison.Ordinal))
        || (text.Length == 8 && text.StartsWith("FM", StringComparison.Ordinal));

    // Reads FMDDHHMM, BECMG DDHH/DDHH, TEMPO DDHH/DDHH, PROB30 [TEMPO] DDHH/DDHH. Always moves past the
    // first token (consumed when valid, skipped with an error otherwise).
    public static TafChangeHeader ReadHeader(TokenCursor cursor, DiagnosticBag diagnostics)
    {
        Token first = cursor.Current;
        string text = first.Text;
        if (text.StartsWith("FM", StringComparison.Ordinal))
        {
            DayTime? from = TimeParsers.ParseFromGroup(text);
            if (from is null)
            {
                diagnostics.Error(DiagnosticCode.InvalidChangeGroup, first);
                cursor.Skip();
            }
            else
            {
                cursor.Consume();
            }

            return new TafChangeHeader(TafChangeKind.From, null, from, null);
        }

        TafChangeKind kind;
        int? probability = null;
        if (text.StartsWith("PROB", StringComparison.Ordinal))
        {
            probability = Scan.Number(text, 4, 2);
            if (probability != 30 && probability != 40)
            {
                diagnostics.Error(DiagnosticCode.InvalidChangeGroup, first);
                cursor.Skip();
            }
            else
            {
                cursor.Consume();
            }

            kind = TafChangeKind.Probability;
            if (cursor.PeekText() == "TEMPO")
            {
                kind = TafChangeKind.Temporary;
                cursor.Consume();
            }
        }
        else
        {
            kind = text == "BECMG" ? TafChangeKind.Becoming : TafChangeKind.Temporary;
            cursor.Consume();
        }

        ValidityPeriod? period = null;
        string? next = cursor.PeekText();
        if (next is not null && TimeParsers.ParsePeriod(next) is ValidityPeriod parsed)
        {
            period = parsed;
            cursor.Consume();
        }
        else if (next is not null && TimeParsers.LooksLikePeriod(next))
        {
            diagnostics.Error(DiagnosticCode.InvalidValidity, cursor.Current);
            cursor.Skip();
        }
        else if (!diagnostics.Items.Any(d => d.Position == first.Position))
        {
            diagnostics.Error(DiagnosticCode.InvalidChangeGroup, first);
        }

        return new TafChangeHeader(kind, probability, null, period);
    }
}
