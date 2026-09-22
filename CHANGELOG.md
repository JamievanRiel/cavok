# Changelog

## 0.1.0

First release.

- Parse METAR, SPECI and TAF reports into immutable records
- European ICAO format: CAVOK, QNH, NSC/NCD/NSW, NOSIG/BECMG/TEMPO trends, FM/BECMG/TEMPO/PROB30/PROB40 change groups, TX/TN
- Missing values from automatic stations, runway visual range, runway state, wind shear, sea state, military colour states
- US format: statute miles, inches of mercury, CLR, remarks
- Military TAF icing and turbulence forecast groups
- Diagnostics with stable codes, positions and caret output; `ParseStrict` throws `CavokParseException`
- Flight category (VFR, MVFR, IFR, LIFR)
- Descriptions in English and Dutch
