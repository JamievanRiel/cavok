namespace Cavok.Parsing;

internal static class MetarParser
{
    public const int MaxLength = 65536;

    private const int TrendRank = 16;

    public static Metar Parse(string raw) => Parse(raw, out _);

    // Also returns the cursor so tests can check that every token was consumed or reported.
    public static Metar Parse(string raw, out TokenCursor cursor)
    {
        var diagnostics = new DiagnosticBag(raw);
        var builder = new MetarBuilder(raw);
        cursor = TokenCursor.Empty;
        if (raw.Length > MaxLength)
        {
            diagnostics.ErrorAt(DiagnosticCode.InputTooLong, 0);
            return builder.Build(diagnostics);
        }

        cursor = new TokenCursor(Tokenizer.Tokenize(raw));
        if (cursor.AtEnd)
        {
            diagnostics.ErrorAt(DiagnosticCode.EmptyInput, 0);
            return builder.Build(diagnostics);
        }

        try
        {
            ParseHeader(cursor, builder, diagnostics);
        }
        catch (Exception)
        {
            diagnostics.InternalError(cursor);
        }

        ParseBody(cursor, builder, diagnostics);
        return builder.Build(diagnostics);
    }

    private static void ParseHeader(TokenCursor cursor, MetarBuilder builder, DiagnosticBag diagnostics)
    {
        switch (cursor.PeekText())
        {
            case "METAR":
                cursor.Consume();
                break;
            case "SPECI":
                builder.Type = ReportType.Speci;
                cursor.Consume();
                break;
            case "TAF":
                diagnostics.Error(DiagnosticCode.WrongReportType, cursor.Current);
                cursor.Skip();
                break;
        }

        while (cursor.PeekText() == "COR")
        {
            builder.IsCorrected = true;
            cursor.Consume();
        }

        builder.Station = HeaderParsers.ReadStation(cursor, diagnostics);
        builder.Time = HeaderParsers.ReadTime(cursor, diagnostics);
        if (cursor.PeekText() == "NIL")
        {
            builder.IsNil = true;
            cursor.Consume();
        }
    }

    private static void ParseBody(TokenCursor cursor, MetarBuilder builder, DiagnosticBag diagnostics)
    {
        var state = new BodyState();
        while (!cursor.AtEnd)
        {
            int before = cursor.Index;
            try
            {
                Step(cursor, builder, diagnostics, state);
            }
            catch (Exception)
            {
                diagnostics.Error(DiagnosticCode.InternalError, cursor.Tokens[before]);
            }

            if (cursor.Index <= before)
            {
                cursor.MoveTo(before + 1);
            }
        }

        if (state.Trend is not null)
        {
            builder.Trends.Add(state.Trend.Build());
        }
    }

    private static void Step(TokenCursor cursor, MetarBuilder builder, DiagnosticBag diagnostics, BodyState state)
    {
        Token token = cursor.Current;
        string text = token.Text;

        if (text == "RMK")
        {
            builder.Remarks = Remarks.After(builder.Raw, token);
            cursor.ConsumeRest();
            return;
        }

        if (text == "NOSIG" || text == "BECMG" || text == "TEMPO")
        {
            if (state.Trend is not null)
            {
                builder.Trends.Add(state.Trend.Build());
                state.Trend = null;
            }

            if (text == "NOSIG")
            {
                builder.Trends.Add(new Trend { Kind = TrendKind.NoSignificantChange });
            }
            else
            {
                state.Trend = new TrendBuilder(text == "BECMG" ? TrendKind.Becoming : TrendKind.Temporary);
            }

            state.Rank = TrendRank;
            cursor.Consume();
            return;
        }

        if (state.Trend is not null && TimeParsers.IsTrendTimeShape(text))
        {
            TimeOfDay? time = TimeParsers.ParseTimeOfDay(text, 2);
            if (time is null)
            {
                diagnostics.Error(DiagnosticCode.InvalidTrend, token);
                cursor.Skip();
                return;
            }

            if (!state.Trend.SetTime(text.Substring(0, 2), time.Value))
            {
                diagnostics.Warning(DiagnosticCode.Duplicate, token);
            }

            cursor.Consume();
            return;
        }

        if (IsMissingColorState(text, state))
        {
            state.Rank = Rank(GroupKind.ColorCode);
            cursor.Consume();
            return;
        }

        Group? group = GroupReader.Read(cursor);
        if (group is null)
        {
            diagnostics.Error(GroupGuesser.Guess(text), token);
            cursor.Skip();
            return;
        }

        Token last = cursor.Tokens[cursor.Index + group.TokenCount - 1];
        if (state.Trend is not null && TrendBuilder.Accepts(group.Kind))
        {
            state.Trend.Conditions.Add(group, token, last, diagnostics);
        }
        else if (group.Kind == GroupKind.NoSignificantWeather)
        {
            // NSW is only meaningful in a trend.
            diagnostics.Error(DiagnosticCode.InvalidWeather, token);
            cursor.Skip();
            return;
        }
        else
        {
            int rank = Rank(group.Kind);
            if (!builder.CanAccept(group))
            {
                diagnostics.Warning(DiagnosticCode.Duplicate, token, last);
            }
            else
            {
                if (rank < state.Rank)
                {
                    diagnostics.Warning(DiagnosticCode.OutOfOrder, token, last);
                }

                builder.Apply(group);
                GroupChecks.Check(group, token, last, diagnostics);
            }

            state.Rank = Math.Max(state.Rank, rank);
        }

        cursor.Consume(group.TokenCount);
    }

    // German military automatic stations send "///" for a colour state they cannot determine
    // ("… Q1023 ///"). In the colour-state position, after the pressure and supplementary groups and before any
    // trend, it is a missing colour state: nothing to record. Elsewhere "///" keeps its other meanings.
    private static bool IsMissingColorState(string text, BodyState state) =>
        text == "///"
        && state.Trend is null
        && state.Rank >= Rank(GroupKind.Pressure)
        && state.Rank <= Rank(GroupKind.ColorCode);

    // Position of each group in the ICAO order of a METAR body.
    private static int Rank(GroupKind kind) => kind switch
    {
        GroupKind.Auto or GroupKind.Corrected => 0,
        GroupKind.Wind => 1,
        GroupKind.WindVariation => 2,
        GroupKind.Cavok or GroupKind.Visibility => 3,
        GroupKind.MinimumVisibility => 4,
        GroupKind.RunwayVisualRange => 5,
        GroupKind.Weather => 6,
        GroupKind.Cloud or GroupKind.CloudCondition => 7,
        GroupKind.Temperature => 8,
        GroupKind.Pressure => 9,
        GroupKind.RecentWeather => 10,
        GroupKind.WindShear => 11,
        GroupKind.Sea => 12,
        GroupKind.RunwayState => 13,
        GroupKind.ColorCode => 14,
        _ => 15,
    };

    private sealed class BodyState
    {
        public int Rank { get; set; }

        public TrendBuilder? Trend { get; set; }
    }
}
