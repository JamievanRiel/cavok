namespace Cavok.Parsing;

internal static class TafParser
{
    public static Taf Parse(string raw) => Parse(raw, out _);

    // Also returns the cursor so tests can check that every token was consumed or reported.
    public static Taf Parse(string raw, out TokenCursor cursor)
    {
        var diagnostics = new DiagnosticBag(raw);
        var builder = new TafBuilder(raw);
        cursor = TokenCursor.Empty;
        if (raw.Length > MetarParser.MaxLength)
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

    private static void ParseHeader(TokenCursor cursor, TafBuilder builder, DiagnosticBag diagnostics)
    {
        string? first = cursor.PeekText();
        if (first == "TAF")
        {
            cursor.Consume();
        }
        else if (first == "METAR" || first == "SPECI")
        {
            diagnostics.Error(DiagnosticCode.WrongReportType, cursor.Current);
            cursor.Skip();
        }

        ReadModifiers(cursor, builder);
        builder.Station = HeaderParsers.ReadStation(cursor, diagnostics);
        ReadModifiers(cursor, builder);
        builder.IssueTime = HeaderParsers.ReadTime(cursor, diagnostics);
        if (ReadNil(cursor, builder))
        {
            return;
        }

        string? validity = cursor.PeekText();
        if (validity is not null && TimeParsers.ParsePeriod(validity) is ValidityPeriod period)
        {
            builder.Validity = period;
            cursor.Consume();
        }
        else if (validity is not null && TimeParsers.LooksLikePeriod(validity))
        {
            diagnostics.Error(DiagnosticCode.InvalidValidity, cursor.Current);
            cursor.Skip();
        }
        else
        {
            diagnostics.ErrorAt(DiagnosticCode.MissingValidity, cursor.AtEnd ? diagnostics.SourceLength : cursor.Current.Position);
        }

        if (ReadNil(cursor, builder))
        {
            return;
        }

        if (cursor.PeekText() == "CNL")
        {
            builder.IsCancelled = true;
            cursor.Consume();
        }
    }

    private static void ReadModifiers(TokenCursor cursor, TafBuilder builder)
    {
        while (true)
        {
            switch (cursor.PeekText())
            {
                case "AMD":
                    builder.IsAmended = true;
                    cursor.Consume();
                    break;
                case "COR":
                    builder.IsCorrected = true;
                    cursor.Consume();
                    break;
                default:
                    return;
            }
        }
    }

    private static bool ReadNil(TokenCursor cursor, TafBuilder builder)
    {
        if (cursor.PeekText() != "NIL")
        {
            return false;
        }

        builder.IsNil = true;
        cursor.Consume();
        return true;
    }

    private static void ParseBody(TokenCursor cursor, TafBuilder builder, DiagnosticBag diagnostics)
    {
        var state = new BodyState(builder.Base);
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

        if (state.Header is not null)
        {
            builder.Changes.Add(state.Header.Build(state.Current));
        }
    }

    // Condition groups go to the block that is currently open: the base forecast or the last change group.
    private static void Step(TokenCursor cursor, TafBuilder builder, DiagnosticBag diagnostics, BodyState state)
    {
        Token token = cursor.Current;
        string text = token.Text;

        if (text == "RMK")
        {
            builder.Remarks = Remarks.After(builder.Raw, token);
            cursor.ConsumeRest();
            return;
        }

        // US military forecasts end with a plain-language statement such as "LAST NO AMDS AFT 2020 NEXT 2104"
        // (last forecast of the day, no amendments after day 20 20Z, next forecast day 21 04Z); it is kept as a remark.
        if (text == "LAST" && cursor.PeekText(1) == "NO" && cursor.PeekText(2) == "AMDS")
        {
            builder.Remarks = Remarks.From(builder.Raw, token);
            cursor.ConsumeRest();
            return;
        }

        if (TimeParsers.ParseTemperatureForecast(text) is TemperatureForecast forecast)
        {
            builder.Temperatures.Add(forecast);
            cursor.Consume();
            return;
        }

        if (IcingTurbulenceParser.ParseIcing(text) is IcingLayer icing)
        {
            state.Current.AddIcing(icing);
            cursor.Consume();
            return;
        }

        if (IcingTurbulenceParser.ParseTurbulence(text) is TurbulenceLayer turbulence)
        {
            state.Current.AddTurbulence(turbulence);
            cursor.Consume();
            return;
        }

        if (TafChangeParser.IsChangeStart(text))
        {
            if (state.Header is not null)
            {
                builder.Changes.Add(state.Header.Build(state.Current));
            }

            state.Header = TafChangeParser.ReadHeader(cursor, diagnostics);
            state.Current = new ConditionsBuilder();
            return;
        }

        Group? group = GroupReader.Read(cursor);
        if (group is not null && group.Kind != GroupKind.ColorCode && ConditionsBuilder.Handles(group.Kind))
        {
            Token last = cursor.Tokens[cursor.Index + group.TokenCount - 1];
            state.Current.Add(group, token, last, diagnostics);
            cursor.Consume(group.TokenCount);
            return;
        }

        diagnostics.Error(group is null ? GroupGuesser.Guess(text, taf: true) : DiagnosticCode.UnknownGroup, token);
        cursor.Skip();
    }

    private sealed class BodyState
    {
        public BodyState(ConditionsBuilder current)
        {
            Current = current;
        }

        public ConditionsBuilder Current { get; set; }

        public TafChangeHeader? Header { get; set; }
    }
}
