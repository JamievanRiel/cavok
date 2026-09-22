# TAF

A `Taf` has a validity period, base conditions (`Base`) and change groups (`Changes`), each with its own
`ForecastConditions`.

| Change group | Example | `TafChange` |
|---|---|---|
| From | `FM211400` | `Kind = From`, `From = day 21 14:00` |
| Becoming | `BECMG 2106/2109` | `Kind = Becoming`, `Period` |
| Temporary | `TEMPO 2114/2118` | `Kind = Temporary`, `Period` |
| Probability | `PROB30 2107/2109` | `Kind = Probability`, `Probability = 30`, `Period` |
| Probability with tempo | `PROB40 TEMPO 2204/2207` | `Kind = Temporary`, `Probability = 40`, `Period` |

Temperature forecasts (`TX15/2114Z`, `TNM02/2205Z`) are collected in `Temperatures` wherever they appear.
`AMD`, `COR`, `NIL` and `CNL` set `IsAmended`, `IsCorrected`, `IsNil` and `IsCancelled`.

## Military TAFs

Military forecasts, mostly from UK and US aerodromes, add a few more groups to the base conditions and every
change group:

- `QNH3043INS` — forecast lowest QNH in inches of mercury (UK military), parsed into `ForecastConditions.Pressure`
  alongside the ICAO `Q1013` and FAA `A2992` forms.
- `6IchihihitL`, for example `651109` — a forecast icing layer, parsed into `ForecastConditions.Icing` as an
  `IcingLayer`: `651109` is moderate icing in cloud from 11,000 ft, 9,000 ft thick (11,000 to 20,000 ft).
- `5BhBhBhBtL`, for example `520002` — a forecast turbulence layer, parsed into `ForecastConditions.Turbulence` as
  a `TurbulenceLayer`: `520002` is occasional moderate turbulence in clear air from the surface to 2,000 ft.

US military forecasts sometimes close with a plain-language statement such as `LAST NO AMDS AFT 2020 NEXT 2104`
(last forecast of the day, no amendments after day 20 20:00 UTC, next forecast issued day 21 04:00 UTC). Cavok does
not parse the statement further; it is kept as text in `Taf.Remarks`, the same place as `RMK …`.

## What a change group contains

A change group only contains the elements it mentions. For `BECMG` and `TEMPO` the other elements stay as they
were; after `FM` the forecast is replaced completely. Cavok reports what the TAF says and does not merge groups
into a timeline. `ForecastConditions.FlightCategory` is computed from the visibility and cloud in that block only.

[!code-csharp[](../../samples/Cavok.Samples/Program.cs#taf-basic)]
