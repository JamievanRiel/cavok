using System.Runtime.CompilerServices;

namespace Cavok.Tests.Snapshots;

// Minimal snapshot testing: compares text with <name>.verified.txt next to the calling test file. On a
// difference it writes <name>.received.txt and fails. CAVOK_ACCEPT_SNAPSHOTS=1 accepts the new output.
internal static class Snapshot
{
    public static void Verify(string name, IEnumerable<string> blocks, [CallerFilePath] string callerFile = "")
    {
        string directory = Path.GetDirectoryName(callerFile)!;
        string verifiedPath = Path.Combine(directory, name + ".verified.txt");
        string receivedPath = Path.Combine(directory, name + ".received.txt");
        string actual = string.Join("\n", blocks);

        if (Environment.GetEnvironmentVariable("CAVOK_ACCEPT_SNAPSHOTS") == "1")
        {
            File.WriteAllText(verifiedPath, actual);
            File.Delete(receivedPath);
            return;
        }

        string expected = File.Exists(verifiedPath) ? File.ReadAllText(verifiedPath).Replace("\r\n", "\n") : "";
        if (expected == actual)
        {
            File.Delete(receivedPath);
            return;
        }

        File.WriteAllText(receivedPath, actual);
        string[] expectedLines = expected.Split('\n');
        string[] actualLines = actual.Split('\n');
        int line = 0;
        while (line < expectedLines.Length && line < actualLines.Length && expectedLines[line] == actualLines[line])
        {
            line++;
        }

        Assert.Fail(
            $"Snapshot '{name}' changed at line {line + 1}.\n"
            + $"  verified: {(line < expectedLines.Length ? expectedLines[line] : "<end of file>")}\n"
            + $"  received: {(line < actualLines.Length ? actualLines[line] : "<end of file>")}\n"
            + $"Compare {receivedPath} with {verifiedPath}. If the change is intended, run the tests with CAVOK_ACCEPT_SNAPSHOTS=1.");
    }
}
