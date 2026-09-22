namespace Cavok.Tests.Diagnostics;

public class DiagnosticTests
{
    private const string Report = "METAR EHAM 211125Z 24012KT 2400O -RA FEW012 12/09 Q1013";

    [Fact]
    public void ToStringPointsAtTheOffendingGroup()
    {
        var diagnostic = new Diagnostic(DiagnosticSeverity.Error, DiagnosticCode.InvalidVisibility, "2400O", 27, 5, Report);

        string expected =
            "error CAV004: invalid visibility group \"2400O\"; expected 4 digits (e.g. 0800, 9999)\n" +
            "  " + Report + "\n" +
            new string(' ', 2 + 27) + "^^^^^";
        Assert.Equal(expected, diagnostic.ToString());
    }

    [Fact]
    public void LongReportsAreShownAsAnExcerpt()
    {
        string report = new string('A', 100) + " XYZ " + new string('B', 100);
        var diagnostic = new Diagnostic(DiagnosticSeverity.Warning, DiagnosticCode.UnknownGroup, "XYZ", 101, 3, report);

        string[] lines = diagnostic.ToString().Split('\n');

        Assert.Equal("warning CAV001: unknown group \"XYZ\"", lines[0]);
        Assert.StartsWith("  ...", lines[1]);
        Assert.EndsWith("...", lines[1]);
        Assert.Equal(lines[1].IndexOf("XYZ", StringComparison.Ordinal), lines[2].IndexOf('^'));
    }

    [Fact]
    public void ControlCharactersInTheReportAreShownAsSpaces()
    {
        const string report = "TAF EHAM\n  XX";
        var diagnostic = new Diagnostic(DiagnosticSeverity.Error, DiagnosticCode.UnknownGroup, "XX", 11, 2, report);

        string[] lines = diagnostic.ToString().Split('\n');

        Assert.Equal(3, lines.Length);
        Assert.Equal("  TAF EHAM   XX", lines[1]);
        Assert.Equal(new string(' ', 13) + "^^", lines[2]);
    }

    [Fact]
    public void ControlCharactersInTheTokenAreShownAsSpacesInTheMessage()
    {
        Metar metar = Metar.Parse("METAR EHAM 211125Z 24012KT X\u001B[2JY 10SM 1\n1/2SM");

        Assert.Equal(2, metar.Diagnostics.Count);
        Diagnostic unknown = metar.Diagnostics[0];
        Diagnostic duplicate = metar.Diagnostics[1];
        Assert.Equal("X\u001B[2JY", unknown.Token);
        Assert.Equal("1\n1/2SM", duplicate.Token);
        Assert.Equal("unknown group \"X [2JY\"", unknown.Message);
        Assert.Equal("duplicate group \"1 1/2SM\"; the first occurrence is used", duplicate.Message);
        Assert.Equal("dubbele groep \"1 1/2SM\"; de eerste wordt gebruikt", duplicate.Describe(Language.Dutch));
        foreach (Diagnostic diagnostic in metar.Diagnostics)
        {
            string[] lines = diagnostic.ToString().Split('\n');
            Assert.Equal(3, lines.Length);
            Assert.DoesNotContain(lines, line => line.Any(char.IsControl));
        }
    }

    [Fact]
    public void MissingElementsPointAtWhereTheyWereExpected()
    {
        var diagnostic = new Diagnostic(DiagnosticSeverity.Error, DiagnosticCode.MissingStation, "", 6, 0, "METAR 211125Z");

        string[] lines = diagnostic.ToString().Split('\n');

        Assert.Equal("error CAV018: station identifier is missing", lines[0]);
        Assert.Equal(new string(' ', 8) + "^", lines[2]);
    }

    [Fact]
    public void DescribesTheProblemInDutch()
    {
        var diagnostic = new Diagnostic(DiagnosticSeverity.Error, DiagnosticCode.InvalidVisibility, "2400O", 27, 5, Report);

        Assert.Equal("ongeldige zichtgroep \"2400O\"; verwacht 4 cijfers (bijv. 0800, 9999)", diagnostic.Describe(Language.Dutch));
        Assert.Equal(diagnostic.Describe(Language.English), diagnostic.Message);
    }

    [Theory]
    [InlineData(DiagnosticCode.UnknownGroup, "CAV001")]
    [InlineData(DiagnosticCode.InvalidVisibility, "CAV004")]
    [InlineData(DiagnosticCode.WrongReportType, "CAV017")]
    [InlineData(DiagnosticCode.InternalError, "CAV027")]
    public void IdsAreStable(DiagnosticCode code, string id) =>
        Assert.Equal(id, new Diagnostic(DiagnosticSeverity.Error, code, "", 0, 0, "").Id);

    [Fact]
    public void ParseExceptionShowsTheFirstError()
    {
        var error = new Diagnostic(DiagnosticSeverity.Error, DiagnosticCode.InvalidVisibility, "2400O", 27, 5, Report);
        var warning = new Diagnostic(DiagnosticSeverity.Warning, DiagnosticCode.OutOfOrder, "FEW012", 37, 6, Report);

        var exception = new CavokParseException(Report, new[] { warning, error });

        Assert.Contains("CAV004", exception.Message, StringComparison.Ordinal);
        Assert.Equal(2, exception.Diagnostics.Count);
        Assert.Equal(Report, exception.Raw);
        Assert.IsAssignableFrom<FormatException>(exception);
    }
}
