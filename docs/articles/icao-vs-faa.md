# ICAO and FAA formats

Cavok follows ICAO Annex 3 and WMO FM 15/FM 51 as used in Europe, and also accepts the US (FAA) variants.

| Element | ICAO (Europe) | FAA (US) | Cavok |
|---|---|---|---|
| Visibility | metres, `9999` = 10 km or more | statute miles, `10SM`, `1 1/2SM`, `M1/4SM` | `Meters` or `StatuteMiles` |
| Good weather | `CAVOK` | not used | `IsCavok` |
| Pressure | `Q1013` (hPa) | `A2992` (inHg) | `Pressure` with both units |
| No cloud | `NSC`, `NCD` | `SKC`, `CLR` | `CloudCondition` |
| Trend in METAR | `NOSIG`, `BECMG`, `TEMPO` | not used | `Trends` |
| Wind units | `KT`, sometimes `MPS` | `KT` | `Speed.Unit` |
| RVR | metres | feet (`FT`) | `RunwayVisualRange.IsFeet` |
| Remarks | rare | common (`RMK AO2 SLP…`) | kept as text in `Remarks` |
| TAF changes | `BECMG`, `TEMPO`, `PROB30/40` | mostly `FM`, `TEMPO` | all supported |
