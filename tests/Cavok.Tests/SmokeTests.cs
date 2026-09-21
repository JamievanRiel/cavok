namespace Cavok.Tests;

public class SmokeTests
{
    [Fact]
    public void LibraryIsReferenced() => Assert.Equal(1, (int)Language.Dutch);
}
