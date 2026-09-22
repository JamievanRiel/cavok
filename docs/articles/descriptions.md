# Descriptions

`Describe(Language.English)` and `Describe(Language.Dutch)` turn a report into readable text: one line per
element, with aligned labels. TAF change groups follow as indented blocks.

[!code-csharp[](../../samples/Cavok.Samples/Program.cs#metar-describe)]

```
TAF EHAM, issued day 21 at 04:40 UTC, valid from day 21 06:00 to day 22 12:00 UTC
Wind:         270° at 5 kt
Visibility:   10 km or more
Clouds:       few (1–2/8) at 3,500 ft
Category:     VFR
Becoming between day 21 06:00 and day 21 09:00 UTC:
  Wind:         310° at 11 kt
30% probability, temporarily between day 21 14:00 and day 21 18:00 UTC:
  Weather:      thunderstorm with rain
```

- Units stay as reported (kt, m/s, m, SM, ft, hPa, inHg). Times are UTC.
- Numbers use fixed formats per language (`1,200 ft` in English, `1.200 ft` in Dutch), independent of the machine's culture.
- Missing values read "not available" / "niet beschikbaar" — except a wholly missing pressure group (`Q////`) or
  temperature group (`/////`): the model cannot tell that apart from a value that was never reported, so
  `Pressure`, `Temperature` and `DewPoint` stay `null` and the whole line is left out, by controller ruling.
- Lines are separated by `\n`.
