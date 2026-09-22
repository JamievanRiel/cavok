---
_layout: landing
---

# Cavok

A METAR and TAF parser for .NET that treats the European ICAO format as a first-class citizen: CAVOK, QNH in
hectopascals, NOSIG/BECMG/TEMPO trends, TAF change groups with PROB30/PROB40, and the missing values that automatic
stations send. Reports are parsed into immutable records, problems are reported as precise diagnostics instead of
exceptions, and every report can be described in English or Dutch.

```
dotnet add package Cavok
```

[!code-csharp[](../samples/Cavok.Samples/Program.cs#metar-basic)]

- [Getting started](articles/getting-started.md)
- [API reference](api/Cavok.yml)
