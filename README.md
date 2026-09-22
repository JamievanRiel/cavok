# Cavok

[![CI](https://github.com/JamievanRiel/cavok/actions/workflows/ci.yml/badge.svg)](https://github.com/JamievanRiel/cavok/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Cavok.svg)](https://www.nuget.org/packages/Cavok)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A METAR and TAF parser for .NET that treats the European ICAO format as a first-class citizen.

```csharp
Metar metar = Metar.Parse("METAR EHAM 211125Z 24012G25KT 200V280 9999 -SHRA FEW012 BKN030CB 12/09 Q1013 TEMPO 4000 SHRA");

metar.Wind.Direction          // 240
metar.Wind.Gust               // 25 kt
metar.Clouds[1]               // broken at 3,000 ft, cumulonimbus
metar.Pressure.Hectopascals   // 1013
metar.FlightCategory          // Mvfr
metar.Describe(Language.Dutch)
```

```
METAR EHAM, waarneming dag 21 om 11:25 UTC
Wind:         240° met 12 kt, windstoten tot 25 kt, variërend tussen 200° en 280°
Zicht:        10 km of meer
Weer:         lichte regenbuien
Bewolking:    enkele wolken (1–2/8) op 1.200 ft, gebroken (5–7/8) op 3.000 ft met cumulonimbus
Temperatuur:  12 °C, dauwpunt 9 °C
QNH:          1013 hPa
Categorie:    MVFR
Trend:        tijdelijk: zicht 4.000 m; regenbuien
```

## Why Cavok?

Most METAR libraries are written with US reports in mind. Cavok handles what European reports actually contain:

- `CAVOK`, QNH in hectopascals, `NSC`, `NCD` and `NSW`
- METAR trends — `NOSIG`, `BECMG` and `TEMPO` with `FM`, `TL` and `AT`
- TAF change groups — `FM`, `BECMG`, `TEMPO`, `PROB30`, `PROB40` and `PROB30 TEMPO`, plus `TX`/`TN`
- Missing values from automatic stations (`/////KT`, `////`, `//////CB`, `17///`, `RE//`) without errors
- Runway visual range, runway state (including `CLRD` and `SNOCLO`), wind shear, sea state and military colour states (`BLU+BLU+`, `BLACKBLU+`, `TEMPO WHT`)

US and other reports parse as well: statute miles (`1 1/2SM`), `A2992`, `CLR`, `RMK`.

## Features

- **Typed, immutable records** for every group, with `Parse` (lenient) and `ParseStrict` (throws on errors)
- **Precise diagnostics**: every problem has a stable code (`CAV004`), the offending text and its position, and a caret view:
  ```
  error CAV004: invalid visibility group "2400O"; expected 4 digits (e.g. 0800, 9999)
    METAR EHAM 211125Z 24012KT 2400O -RA FEW012 12/09 Q1013
                               ^^^^^
  ```
- **Never throws on bad input**: one broken group does not stop the rest of the report from being parsed
- **Flight category** (VFR, MVFR, IFR, LIFR) for observations and every forecast block
- **Plain-language descriptions** in English and Dutch
- **No dependencies**; targets .NET Standard 2.0 and .NET 8

## Install

```
dotnet add package Cavok
```

## Usage

```csharp
using Cavok;

Taf taf = Taf.Parse("TAF EHAM 210440Z 2106/2212 27005KT 9999 FEW035 BECMG 2106/2109 31011KT PROB30 TEMPO 2114/2118 TSRA");

foreach (TafChange change in taf.Changes)
{
    Console.WriteLine($"{change.Kind} {change.Probability} {change.Period}");
}

Console.WriteLine(taf.Describe(Language.English));
```

More examples are in the [documentation](https://jamievanriel.github.io/cavok).

## How it is tested

- Unit tests for every group type, including the ICAO Annex 3 examples and edge cases
- Thousands of real METARs and TAFs from European aerodromes, fetched from the free
  [aviationweather.gov Data API](https://aviationweather.gov/data/api/), must parse without errors
- A check that every group in the corpus is either understood or reported — nothing is dropped silently
- Snapshot tests of the parsed model and both descriptions for a diverse selection of reports
- Property-based fuzzing with [FsCheck](https://fscheck.github.io/FsCheck/): random strings, random sequences of
  report groups and mutated real reports never make the parser throw

## Development

```
dotnet test                                               # all tests
CAVOK_FUZZ_CASES=10000 dotnet test                        # more fuzzing
dotnet run --project tools/Cavok.Corpus -- fetch          # add fresh reports to the corpus
dotnet run --project tools/Cavok.Corpus -- triage         # see what the parser reports on the corpus
CAVOK_ACCEPT_SNAPSHOTS=1 dotnet test                      # accept intended snapshot changes
```

## License

[MIT](LICENSE)
