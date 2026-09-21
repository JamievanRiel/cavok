using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class HeaderParsersTests
{
    [Theory]
    [InlineData("EHAM", true)]
    [InlineData("K1V4", true)]
    [InlineData("AUTO", false)]
    [InlineData("EHA", false)]
    [InlineData("EHAMX", false)]
    [InlineData("E-AM", false)]
    [InlineData("1HAM", false)]
    public void IsStation(string text, bool expected) => Assert.Equal(expected, HeaderParsers.IsStation(text));

    [Fact]
    public void ReadsStation()
    {
        (TokenCursor cursor, DiagnosticBag bag) = Setup("EHAM 211125Z");

        Assert.Equal("EHAM", HeaderParsers.ReadStation(cursor, bag));
        Assert.Equal(1, cursor.Index);
        Assert.Empty(bag.Items);
    }

    [Fact]
    public void ReportsInvalidStation()
    {
        (TokenCursor cursor, DiagnosticBag bag) = Setup("EH4 211125Z");

        Assert.Null(HeaderParsers.ReadStation(cursor, bag));
        Assert.Equal(DiagnosticCode.InvalidStation, Assert.Single(bag.Items).Code);
        Assert.Equal(1, cursor.Index);
    }

    [Fact]
    public void ReportsMissingStationWhereItWasExpected()
    {
        (TokenCursor cursor, DiagnosticBag bag) = Setup("211125Z");

        Assert.Null(HeaderParsers.ReadStation(cursor, bag));
        Diagnostic diagnostic = Assert.Single(bag.Items);
        Assert.Equal(DiagnosticCode.MissingStation, diagnostic.Code);
        Assert.Equal(0, diagnostic.Position);
        Assert.Equal(0, diagnostic.Length);
        Assert.Equal(0, cursor.Index);
    }

    [Fact]
    public void ReadsTime()
    {
        (TokenCursor cursor, DiagnosticBag bag) = Setup("211125Z 24012KT");

        Assert.Equal(new DayTime(21, 11, 25), HeaderParsers.ReadTime(cursor, bag));
        Assert.Empty(bag.Items);
    }

    [Theory]
    [InlineData("21112OZ 24012KT", DiagnosticCode.InvalidTime, 1)]
    [InlineData("24012KT", DiagnosticCode.MissingTime, 0)]
    [InlineData("", DiagnosticCode.MissingTime, 0)]
    public void ReportsTimeProblems(string text, DiagnosticCode code, int indexAfter)
    {
        (TokenCursor cursor, DiagnosticBag bag) = Setup(text);

        Assert.Null(HeaderParsers.ReadTime(cursor, bag));
        Assert.Equal(code, Assert.Single(bag.Items).Code);
        Assert.Equal(indexAfter, cursor.Index);
    }

    private static (TokenCursor, DiagnosticBag) Setup(string text) =>
        (new TokenCursor(Tokenizer.Tokenize(text)), new DiagnosticBag(text));
}
