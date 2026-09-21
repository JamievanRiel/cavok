using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class PressureParserTests
{
    [Theory]
    [InlineData("Q1013", 1013.0, PressureUnit.Hectopascals)]
    [InlineData("Q0995", 995.0, PressureUnit.Hectopascals)]
    [InlineData("A2992", 29.92, PressureUnit.InchesOfMercury)]
    [InlineData("QNH3043INS", 30.43, PressureUnit.InchesOfMercury)]
    public void ParsesPressure(string text, double value, PressureUnit unit)
    {
        Assert.True(PressureParser.TryParse(text, out Pressure? pressure));
        Assert.Equal(unit, pressure!.Value.Unit);
        Assert.Equal(value, pressure.Value.Value, 2);
    }

    [Theory]
    [InlineData("Q////")]
    [InlineData("A////")]
    public void MissingPressureIsRecognisedWithoutValue(string text)
    {
        Assert.True(PressureParser.TryParse(text, out Pressure? pressure));
        Assert.Null(pressure);
    }

    [Theory]
    [InlineData("Q101")]
    [InlineData("Q10134")]
    [InlineData("AMB")]
    [InlineData("QNH3043")]
    [InlineData("A29.9")]
    [InlineData("QNH30A3INS")]
    public void RejectsOtherGroups(string text) => Assert.False(PressureParser.TryParse(text, out _));

    [Fact]
    public void ConvertsBetweenUnits()
    {
        Assert.Equal(29.92, new Pressure(1013.25, PressureUnit.Hectopascals).InchesOfMercury, 2);
        Assert.Equal(1013.2, new Pressure(29.92, PressureUnit.InchesOfMercury).Hectopascals, 1);
    }
}
