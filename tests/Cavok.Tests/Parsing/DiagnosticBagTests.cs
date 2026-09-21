using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class DiagnosticBagTests
{
    [Fact]
    public void UsesTheOriginalTextOfTheReport()
    {
        const string raw = "metar eham 211125z 2400o";
        var bag = new DiagnosticBag(raw);

        bag.Error(DiagnosticCode.InvalidVisibility, Tokenizer.Tokenize(raw)[3]);

        Diagnostic diagnostic = Assert.Single(bag.Items);
        Assert.Equal("2400o", diagnostic.Token);
        Assert.Equal(19, diagnostic.Position);
        Assert.True(bag.HasErrors);
    }

    [Fact]
    public void SpansMultipleTokens()
    {
        const string raw = "WS ALL RWY";
        List<Token> tokens = Tokenizer.Tokenize(raw);
        var bag = new DiagnosticBag(raw);

        bag.Warning(DiagnosticCode.Duplicate, tokens[0], tokens[2]);

        Diagnostic diagnostic = Assert.Single(bag.Items);
        Assert.Equal("WS ALL RWY", diagnostic.Token);
        Assert.Equal(10, diagnostic.Length);
        Assert.False(bag.HasErrors);
    }

    [Fact]
    public void ErrorAtCreatesAZeroLengthDiagnostic()
    {
        var bag = new DiagnosticBag("METAR 211125Z");

        bag.ErrorAt(DiagnosticCode.MissingStation, 6);

        Diagnostic diagnostic = Assert.Single(bag.Items);
        Assert.Equal("", diagnostic.Token);
        Assert.Equal(6, diagnostic.Position);
        Assert.Equal(0, diagnostic.Length);
    }

    [Fact]
    public void InternalErrorPointsAtTheCurrentToken()
    {
        const string raw = "A B";
        var cursor = new TokenCursor(Tokenizer.Tokenize(raw));
        cursor.Consume();
        var bag = new DiagnosticBag(raw);

        bag.InternalError(cursor);
        cursor.Consume();
        bag.InternalError(cursor);

        Assert.Equal(new[] { 2, 3 }, bag.Items.Select(d => d.Position));
        Assert.All(bag.Items, d => Assert.Equal(DiagnosticCode.InternalError, d.Code));
    }
}
