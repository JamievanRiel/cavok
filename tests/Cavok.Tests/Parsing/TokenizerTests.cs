using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class TokenizerTests
{
    [Fact]
    public void SplitsOnAnyWhitespaceAndKeepsOriginalPositions()
    {
        List<Token> tokens = Tokenizer.Tokenize("TAF EHAM\n  BECMG\t2106/2109");

        Assert.Equal(new[] { "TAF", "EHAM", "BECMG", "2106/2109" }, tokens.Select(t => t.Text));
        Assert.Equal(new[] { 0, 4, 11, 17 }, tokens.Select(t => t.Position));
    }

    [Fact]
    public void UppercasesTokens() =>
        Assert.Equal(new[] { "METAR", "EHAM" }, Tokenizer.Tokenize("metar eham").Select(t => t.Text));

    [Theory]
    [InlineData("Q1013=", "Q1013")]
    [InlineData("Q1013==", "Q1013")]
    public void StripsTheTrailingEndMarker(string input, string expected) =>
        Assert.Equal(expected, Assert.Single(Tokenizer.Tokenize(input)).Text);

    [Fact]
    public void DropsTokensThatAreOnlyEndMarkers() =>
        Assert.Equal(new[] { "Q1013" }, Tokenizer.Tokenize("Q1013 =").Select(t => t.Text));

    [Theory]
    [InlineData("")]
    [InlineData("   \n\t ")]
    public void EmptyInputGivesNoTokens(string input) => Assert.Empty(Tokenizer.Tokenize(input));
}
