# METAR

| Group | Example | Property |
|---|---|---|
| Report type | `METAR`, `SPECI` | `Type` |
| Station | `EHAM` | `Station` |
| Time | `211125Z` | `Time` (`DayTime`) |
| Modifiers | `AUTO`, `COR`, `NIL` | `IsAuto`, `IsCorrected`, `IsNil` |
| Wind | `24012G25KT`, `VRB03KT`, `00000KT`, `270P49MPS` | `Wind` |
| Wind variation | `200V280` | `Wind.VariableFrom`, `Wind.VariableTo` |
| Visibility | `9999`, `0800`, `CAVOK`, `9999NDV`, `1 1/2SM` | `Visibility`, `IsCavok` |
| Minimum visibility | `1500SW` | `Visibility.Minimum` |
| Runway visual range | `R24/P2000N`, `R06L/0800V1200U` | `RunwayVisualRanges` |
| Present weather | `-SHRA`, `+TSRAGS`, `VCFG`, `//` | `Weather` |
| Cloud | `FEW012`, `BKN030CB`, `VV002`, `NSC`, `NCD` | `Clouds`, `CloudCondition` |
| Temperature | `12/09`, `M05/M07` | `Temperature`, `DewPoint` |
| Pressure | `Q1013`, `A2992` | `Pressure` |
| Recent weather | `RERA`, `RE//` | `RecentWeather` |
| Wind shear | `WS R24`, `WS ALL RWY` | `WindShear` |
| Sea state | `W15/S4`, `W18/H14` | `Sea` |
| Runway state | `R24/290050`, `R24/CLRD62`, `R/SNOCLO` | `RunwayStates` |
| Military colour state | `BLU`, `BLU+BLU+`, `BLACKBLU+` | `ColorCodes` |
| Trend | `NOSIG`, `BECMG FM1030 30015KT`, `TEMPO 4000 SHRA` | `Trends` |
| Remarks | `RMK …` | `Remarks` (unparsed) |

## Missing values

Automatic stations send slashes for values they cannot measure. These are not errors: the value is `null` or a
flag such as `Wind.IsMissing`, `Visibility.IsMissing`, `WeatherPhenomenon.IsNotObservable` or
`CloudLayer.IsTypeNotObservable` is set. Accepted forms include `/////KT` (wind), `////` and `////SM` (visibility
missing in metres or statute miles — `Visibility.IsMissing`), `//` (weather) and `//////CB` (a cloud layer whose
amount, height and type are all unknown). French automatic stations may also send `///CB` or `///TCU` on their own:
a cumulonimbus or towering cumulus was detected but its amount and height were not, so `Cover` and `HeightFeet` are
`null` while `Type` is set.

## Visibility

`9999` means 10 km or more. It is stored as `Meters = 10000` with `IsTenKmOrMore = true`. Statute miles are kept
as reported in `StatuteMiles`; `ToMeters()` converts either form.

## Trends

Groups after `BECMG` or `TEMPO` belong to that trend (`Trend.Conditions`) until the next trend keyword or `RMK`.
Groups that cannot occur in a trend, such as `RERA`, are reported as out of order and added to the report itself.
