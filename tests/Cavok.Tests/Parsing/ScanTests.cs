using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class ScanTests
{
    [Theory]
    [InlineData("1234", 0, 4, 1234)]
    [InlineData("Q1013", 1, 4, 1013)]
    [InlineData("0005", 0, 4, 5)]
    public void NumberReadsAsciiDigits(string text, int start, int count, int expected) =>
        Assert.Equal(expected, Scan.Number(text, start, count));

    [Theory]
    [InlineData("12a4", 0, 4)]
    [InlineData("123", 0, 4)]
    [InlineData("123", -1, 2)]
    [InlineData("١٢٣٤", 0, 4)] // Arabic-Indic digits are not ASCII digits
    [InlineData("1234", 0, 0)]
    [InlineData("12345678901", 0, 11)]
    public void NumberRejectsNonDigitsAndOutOfRangeRequests(string text, int start, int count) =>
        Assert.Null(Scan.Number(text, start, count));

    [Fact]
    public void SlashHelpers()
    {
        Assert.True(Scan.IsAllSlashes("////"));
        Assert.False(Scan.IsAllSlashes(""));
        Assert.False(Scan.IsAllSlashes("//1/"));
        Assert.True(Scan.AreSlashes("R24/////", 4, 4));
        Assert.False(Scan.AreSlashes("R24/", 4, 1));
    }

    [Fact]
    public void CharacterClasses()
    {
        Assert.True(Scan.IsAlphanumeric("K1V4"));
        Assert.False(Scan.IsAlphanumeric("E-AM"));
        Assert.False(Scan.IsAlphanumeric(""));
        Assert.Equal(5, Scan.CountDigits("21112OZ"));
    }
}
