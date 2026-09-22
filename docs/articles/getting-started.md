# Getting started

Install the package:

```
dotnet add package Cavok
```

## Parse a METAR

`Metar.Parse` returns a `Metar` record. Every group is a typed property; groups that occur more than once, such
as clouds or runway visual ranges, are lists.

[!code-csharp[](../../samples/Cavok.Samples/Program.cs#metar-basic)]

## Describe it

[!code-csharp[](../../samples/Cavok.Samples/Program.cs#metar-describe)]

## Parse a TAF

`Taf.Parse` returns the base forecast and a list of change groups, each with its own `ForecastConditions`.

[!code-csharp[](../../samples/Cavok.Samples/Program.cs#taf-basic)]

## Lenient and strict parsing

`Parse` never throws for bad content. Whatever can be understood is returned and every problem is listed in
`Diagnostics`. `ParseStrict` throws a `CavokParseException` when there is at least one error; warnings are allowed.

[!code-csharp[](../../samples/Cavok.Samples/Program.cs#strict)]

## Day and time

Reports only contain the day of the month. `DayTime.ToDateTimeOffset` picks the month and year closest to a
reference moment, typically when the report was received:

[!code-csharp[](../../samples/Cavok.Samples/Program.cs#datetime)]
