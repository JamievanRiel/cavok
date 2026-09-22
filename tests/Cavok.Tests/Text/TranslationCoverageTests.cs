using Cavok.Text;

namespace Cavok.Tests.Text;

public class TranslationCoverageTests
{
    public static TheoryData<Language> Languages => new() { Language.English, Language.Dutch };

    [Fact]
    public void LabelAlignmentCoversEveryField() =>
        Assert.Equal(Enum.GetValues<Field>().OrderBy(f => f), DescriptionWriter.Fields.OrderBy(f => f));

    [Theory]
    [MemberData(nameof(Languages))]
    public void EveryEnumValueHasText(Language language)
    {
        Phrasebook p = Phrasebook.For(language);

        foreach (WeatherType type in Enum.GetValues<WeatherType>())
        {
            AssertText(p.WeatherText(new WeatherPhenomenon { Types = new[] { type } }));
        }

        foreach (WeatherDescriptor descriptor in Enum.GetValues<WeatherDescriptor>())
        {
            AssertText(p.WeatherText(new WeatherPhenomenon { Descriptor = descriptor, Types = new[] { WeatherType.Rain } }));
        }

        foreach (WeatherIntensity intensity in Enum.GetValues<WeatherIntensity>())
        {
            AssertText(p.WeatherText(new WeatherPhenomenon { Intensity = intensity, Types = new[] { WeatherType.Rain } }));
        }

        foreach (CloudCover cover in Enum.GetValues<CloudCover>())
        {
            AssertText(p.CloudText(new CloudLayer { Cover = cover, HeightFeet = 1000 }));
        }

        foreach (CloudType type in Enum.GetValues<CloudType>())
        {
            AssertText(p.CloudText(new CloudLayer { Cover = CloudCover.Broken, HeightFeet = 1000, Type = type }));
        }

        foreach (CloudCondition condition in Enum.GetValues<CloudCondition>())
        {
            AssertText(p.CloudConditionText(condition));
        }

        foreach (CompassDirection direction in Enum.GetValues<CompassDirection>())
        {
            AssertText(p.VisibilityText(new Visibility { Meters = 3000, Minimum = new MinimumVisibility(1500, direction) }));
        }

        foreach (RvrTendency tendency in Enum.GetValues<RvrTendency>())
        {
            AssertText(p.RvrText(new RunwayVisualRange { Runway = "24", Value = 800, Tendency = tendency }));
        }

        foreach (RvrQualifier qualifier in Enum.GetValues<RvrQualifier>())
        {
            AssertText(p.RvrText(new RunwayVisualRange { Runway = "24", Value = 800, Qualifier = qualifier }));
        }

        foreach (SpeedUnit unit in Enum.GetValues<SpeedUnit>())
        {
            AssertText(p.WindText(new Wind { Direction = 240, Speed = new Speed(10, unit) }));
        }

        foreach (ColorState state in Enum.GetValues<ColorState>())
        {
            AssertText(p.ColorCodeText(new ColorCode(state, false)));
        }

        foreach (TrendKind kind in Enum.GetValues<TrendKind>())
        {
            AssertText(p.TrendPrefix(new Trend { Kind = kind, From = new TimeOfDay(10, 0), Until = new TimeOfDay(11, 0), At = new TimeOfDay(10, 30) }));
        }

        foreach (TafChangeKind kind in Enum.GetValues<TafChangeKind>())
        {
            var change = new TafChange
            {
                Kind = kind,
                Probability = 30,
                From = new DayTime(21, 14, 0),
                Period = new ValidityPeriod(new DayHour(21, 6), new DayHour(21, 9)),
            };
            AssertText(p.ChangeHeader(change));
        }

        foreach (Field field in Enum.GetValues<Field>())
        {
            AssertText(p.Label(field));
        }
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void EveryRunwayStateCodeHasText(Language language)
    {
        Phrasebook p = Phrasebook.For(language);

        for (int code = 0; code <= 99; code++)
        {
            AssertText(p.RunwayStateText(new RunwayState { Runway = "24", Deposit = code % 10, Extent = code % 10, Depth = code, Friction = code }));
        }
    }

    [Theory]
    [MemberData(nameof(Languages))]
    public void EveryIcingAndTurbulenceCodeHasText(Language language)
    {
        Phrasebook p = Phrasebook.For(language);

        for (int code = 0; code <= 9; code++)
        {
            AssertText(p.IcingText(new IcingLayer(code, 1000, 2000)));
            AssertText(p.TurbulenceText(new TurbulenceLayer(code, 1000, 2000)));
        }
    }

    [Fact]
    public void DutchIsReallyDutch() =>
        Assert.NotEqual(Phrasebook.For(Language.English).Label(Field.Visibility), Phrasebook.For(Language.Dutch).Label(Field.Visibility));

    [Fact]
    public void EveryDiagnosticCodeHasEnglishAndDutchText()
    {
        foreach (DiagnosticCode code in Enum.GetValues<DiagnosticCode>())
        {
            string english = DiagnosticMessages.Format(code, "X", Language.English);
            string dutch = DiagnosticMessages.Format(code, "X", Language.Dutch);

            Assert.DoesNotContain("unknown diagnostic", english, StringComparison.Ordinal);
            Assert.DoesNotContain("onbekende melding", dutch, StringComparison.Ordinal);
            Assert.NotEqual(english, dutch);
        }
    }

    private static void AssertText(string text)
    {
        Assert.False(string.IsNullOrWhiteSpace(text));
        Assert.DoesNotContain("?", text, StringComparison.Ordinal);
    }
}
