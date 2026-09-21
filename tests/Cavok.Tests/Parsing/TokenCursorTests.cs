using Cavok.Parsing;

namespace Cavok.Tests.Parsing;

public class TokenCursorTests
{
    [Fact]
    public void TracksConsumedAndSkippedTokens()
    {
        var cursor = new TokenCursor(Tokenizer.Tokenize("A B C D"));

        cursor.Consume();
        cursor.Skip();
        cursor.Consume(2);

        Assert.True(cursor.AtEnd);
        Assert.True(cursor.IsConsumed(0));
        Assert.False(cursor.IsConsumed(1));
        Assert.True(cursor.IsConsumed(2));
        Assert.True(cursor.IsConsumed(3));
    }

    [Fact]
    public void PeekTextReturnsNullPastTheEnd()
    {
        var cursor = new TokenCursor(Tokenizer.Tokenize("A"));

        Assert.Equal("A", cursor.PeekText());
        Assert.Null(cursor.PeekText(1));
        Assert.False(cursor.TryPeek(-1, out _));
    }

    [Fact]
    public void ConsumeRestMarksEverythingAsConsumed()
    {
        var cursor = new TokenCursor(Tokenizer.Tokenize("RMK A B"));

        cursor.ConsumeRest();

        Assert.True(cursor.AtEnd);
        Assert.True(cursor.IsConsumed(2));
    }

    [Fact]
    public void MoveToJumpsWithoutMarking()
    {
        var cursor = new TokenCursor(Tokenizer.Tokenize("A B C"));

        cursor.MoveTo(2);

        Assert.Equal("C", cursor.Current.Text);
        Assert.False(cursor.IsConsumed(0));
        Assert.True(TokenCursor.Empty.AtEnd);
    }
}
