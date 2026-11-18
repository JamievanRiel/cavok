# ICAO and FAA formats

Cavok follows ICAO Annex 3 and WMO FM 15/FM 51 as used in Europe, and also accepts the US (FAA) variants. US TAF
low-level wind shear groups (`WS020/24040KT`) are not supported yet and are reported as errors.

| Element | ICAO (Europe) | FAA (US) | Cavok |
|---|---|---|---|
| Visibility | metres, `9999` = 10 km or more | statute miles, `10SM`, `1 1/2SM`, `M1/4SM` | `Meters` or `StatuteMiles`, plus `ToMeters()` |
| Good weather | `CAVOK` | not used | `IsCavok` |
| Pressure | `Q1013` (hPa) | `A2992` (inHg) | `Pressure` with `Hectopascals` and `InchesOfMercury` |
| No cloud | `NSC`, `NCD` | `SKC`, `CLR` | `CloudCondition` |
| Trend in METAR | `NOSIG`, `BECMG`, `TEMPO` | not used | `Trends` |
| Wind units | `KT`, sometimes `MPS` | `KT` | `Speed.Unit`, plus `ToKnots()`, `ToMetersPerSecond()` and `ToKilometersPerHour()` |
| RVR | metres | feet (`FT`) | `RunwayVisualRange.IsFeet`, plus `ValueMeters` and `VariableMaxMeters` |
| Remarks | rare | common (`RMK AO2 SLP…`) | kept as text in `Remarks` |
| TAF changes | `BECMG`, `TEMPO`, `PROB30/40` | mostly `FM`, `TEMPO` | all supported |

Heights reported in feet — cloud bases (`CloudLayer.HeightFeet`), icing and turbulence layers (`BaseFeet`,
`ThicknessFeet`) — also expose metres: `HeightMeters`, `BaseMeters` and `ThicknessMeters`. The significant wave
height (`SeaCondition.WaveHeightDecimeters`) has `WaveHeightMeters`.
