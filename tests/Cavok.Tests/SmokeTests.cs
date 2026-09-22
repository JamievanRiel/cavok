using System.Reflection;
using System.Runtime.Versioning;

namespace Cavok.Tests;

public class SmokeTests
{
    [Fact]
    public void LibraryIsReferenced() => Assert.Equal(1, (int)Language.Dutch);

    // The tests use the net8.0 build of the library unless the build is run with -p:CavokTestTarget=netstandard2.0.
    [Fact]
    public void TestsRunAgainstTheChosenLibraryBuild()
    {
        string? chosen = typeof(SmokeTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .SingleOrDefault(a => a.Key == "CavokTestTarget")?.Value;
        string expected = chosen == "netstandard2.0" ? ".NETStandard,Version=v2.0" : ".NETCoreApp,Version=v8.0";

        Assert.Equal(expected, typeof(Metar).Assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName);
    }
}
