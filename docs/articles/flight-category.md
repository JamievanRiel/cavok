# Flight category

`Metar.FlightCategory` and `ForecastConditions.FlightCategory` use the thresholds of the US Aviation Weather Center.
There is no official European equivalent, so treat the category as an indication.

| Category | Ceiling | or visibility |
|---|---|---|
| LIFR | below 500 ft | below 1 SM (1,609 m) |
| IFR | 500 to below 1,000 ft | 1 to below 3 SM (4,828 m) |
| MVFR | 1,000 to 3,000 ft | 3 to 5 SM (8,047 m) |
| VFR | above 3,000 ft | above 5 SM |

- The ceiling is the lowest broken (`BKN`), overcast (`OVC`) or vertical visibility (`VV`) layer with a known height.
  A ceiling layer of unknown height (`BKN///`, `OVC///`, `VV///`) reported below every known ceiling makes the
  ceiling unknown, so the visibility decides.
- The worse of the two categories wins. When only one is known, it decides; when neither is known the category is `null`.
- `CAVOK` is VFR. `NSC`, `NCD`, `SKC`, `CLR` and few/scattered layers mean there is no ceiling.
- Metric visibility is compared in metres, so `8000` is just below 5 SM and gives MVFR.
- "Less than" and "more than" statute miles count as below or above the number: `M3SM` is IFR and `P5SM` is VFR.
