# Diagnostics

Every problem is a `Diagnostic` with a severity, a stable code, the offending text and its position.

- **Errors**: a group was not understood and was skipped, or a required group is missing.
- **Warnings**: the group was understood and used, but looks wrong (out of order, duplicate, gust not above the
  mean wind, dew point above temperature).

[!code-csharp[](../../samples/Cavok.Samples/Program.cs#diagnostics)]

`Diagnostic.Describe(Language.Dutch)` gives the message in Dutch.

## Guarantees

For any string, `Parse` only throws `ArgumentNullException` (for `null`) and `ParseStrict` only throws
`CavokParseException` in addition. Parsing takes linear time; input longer than 65,536 characters is rejected with
`InputTooLong`. These guarantees are checked with property-based tests on random and mutated input.

## Codes

| Id | Code |
|---|---|
| CAV001 | UnknownGroup |
| CAV002 | InvalidStation |
| CAV003 | InvalidTime |
| CAV004 | InvalidVisibility |
| CAV005 | InvalidWind |
| CAV006 | InvalidWindVariation |
| CAV007 | InvalidRvr |
| CAV008 | InvalidWeather |
| CAV009 | InvalidCloud |
| CAV010 | InvalidTemperature |
| CAV011 | InvalidPressure |
| CAV012 | InvalidRunwayState |
| CAV013 | InvalidTrend |
| CAV014 | InvalidValidity |
| CAV015 | InvalidChangeGroup |
| CAV016 | InvalidTemperatureForecast |
| CAV017 | WrongReportType |
| CAV018 | MissingStation |
| CAV019 | MissingTime |
| CAV020 | MissingValidity |
| CAV021 | OutOfOrder (warning) |
| CAV022 | Duplicate (warning) |
| CAV023 | GustNotAboveSpeed (warning) |
| CAV024 | DewPointAboveTemperature (warning) |
| CAV025 | EmptyInput |
| CAV026 | InputTooLong |
| CAV027 | InternalError |
