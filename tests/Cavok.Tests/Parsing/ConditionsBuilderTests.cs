using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class ConditionsBuilderTests
{
    [Fact]
    public void CombinesWindAndVariation()
    {
        (ConditionsBuilder builder, DiagnosticBag bag) = Build("24012KT 200V280");

        Wind wind = builder.Build().Wind!;

        Assert.Equal(240, wind.Direction);
        Assert.Equal(200, wind.VariableFrom);
        Assert.Equal(280, wind.VariableTo);
        Assert.Empty(bag.Items);
    }

    [Fact]
    public void VariationWithoutWindIsKept()
    {
        (ConditionsBuilder builder, _) = Build("200V280");

        Wind wind = builder.Build().Wind!;

        Assert.Null(wind.Direction);
        Assert.Equal(200, wind.VariableFrom);
    }

    [Fact]
    public void DuplicateWindIsAWarningAndTheFirstWins()
    {
        (ConditionsBuilder builder, DiagnosticBag bag) = Build("24012KT 25015KT");

        Assert.Equal(240, builder.Build().Wind!.Direction);
        Diagnostic diagnostic = Assert.Single(bag.Items);
        Assert.Equal(DiagnosticCode.Duplicate, diagnostic.Code);
        Assert.Equal("25015KT", diagnostic.Token);
    }

    [Fact]
    public void SecondPlainVisibilityIsAMinimumWithoutDirection()
    {
        (ConditionsBuilder builder, DiagnosticBag bag) = Build("9999 1500");

        Visibility visibility = builder.Build().Visibility!;

        Assert.True(visibility.IsTenKmOrMore);
        Assert.Equal(new MinimumVisibility(1500, null), visibility.Minimum);
        Assert.Empty(bag.Items);
    }

    [Fact]
    public void MinimumVisibilityWithDirection()
    {
        (ConditionsBuilder builder, _) = Build("9999 1500SW");

        Assert.Equal(new MinimumVisibility(1500, CompassDirection.SouthWest), builder.Build().Visibility!.Minimum);
    }

    [Fact]
    public void CavokThenVisibilityIsADuplicate()
    {
        (ConditionsBuilder builder, DiagnosticBag bag) = Build("CAVOK 9999");

        Assert.True(builder.Build().IsCavok);
        Assert.Null(builder.Build().Visibility);
        Assert.Equal(DiagnosticCode.Duplicate, Assert.Single(bag.Items).Code);
    }

    [Fact]
    public void CollectsWeatherCloudsAndColorCodes()
    {
        (ConditionsBuilder builder, DiagnosticBag bag) = Build("-SHRA BR FEW010 BKN020 BLU+BLU+");

        ForecastConditions conditions = builder.Build();

        Assert.Equal(2, conditions.Weather.Count);
        Assert.Equal(2, conditions.Clouds.Count);
        Assert.Equal(2, conditions.ColorCodes.Count);
        Assert.Empty(bag.Items);
    }

    [Fact]
    public void MissingPressureStillCountsForDuplicates()
    {
        (ConditionsBuilder builder, DiagnosticBag bag) = Build("Q//// Q1013");

        Assert.Null(builder.Build().Pressure);
        Assert.Equal(DiagnosticCode.Duplicate, Assert.Single(bag.Items).Code);
    }

    [Fact]
    public void WarnsWhenGustIsNotAboveSpeed()
    {
        (_, DiagnosticBag bag) = Build("24012G10KT");

        Assert.Equal(DiagnosticCode.GustNotAboveSpeed, Assert.Single(bag.Items).Code);
    }

    [Fact]
    public void NoSignificantWeather() => Assert.True(Build("NSW").Builder.Build().NoSignificantWeather);

    [Theory]
    [InlineData("Wind", true)]
    [InlineData("Pressure", true)]
    [InlineData("ColorCode", true)]
    [InlineData("Temperature", false)]
    [InlineData("RunwayVisualRange", false)]
    [InlineData("Auto", false)]
    public void HandlesOnlyConditionGroups(string kind, bool expected) =>
        Assert.Equal(expected, ConditionsBuilder.Handles(Enum.Parse<GroupKind>(kind)));

    [Fact]
    public void FlightCategoryIsComputedFromTheGroup() =>
        Assert.Equal(FlightCategory.Ifr, Build("3000 BKN008").Builder.Build().FlightCategory);

    private static (ConditionsBuilder Builder, DiagnosticBag Bag) Build(string text)
    {
        var builder = new ConditionsBuilder();
        var bag = new DiagnosticBag(text);
        var cursor = new TokenCursor(Tokenizer.Tokenize(text));
        while (!cursor.AtEnd)
        {
            Group group = GroupReader.Read(cursor) ?? throw new InvalidOperationException("Unrecognised: " + cursor.Current.Text);
            Token first = cursor.Current;
            Token last = cursor.Tokens[cursor.Index + group.TokenCount - 1];
            builder.Add(group, first, last, bag);
            cursor.Consume(group.TokenCount);
        }

        return (builder, bag);
    }
}
