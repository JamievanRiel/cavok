using System.Text;

namespace Cavok.Text;

internal sealed class DescriptionWriter
{
    // Every Field, for the label width. A plain list instead of Enum.GetValues, which is not AOT-safe
    // (IL3050); a test checks that it stays complete.
    internal static readonly Field[] Fields =
    {
        Field.Wind,
        Field.Visibility,
        Field.RunwayVisualRange,
        Field.Weather,
        Field.Clouds,
        Field.Temperature,
        Field.Pressure,
        Field.Icing,
        Field.Turbulence,
        Field.RecentWeather,
        Field.WindShear,
        Field.Sea,
        Field.RunwayState,
        Field.ColorCode,
        Field.FlightCategory,
        Field.Trend,
        Field.Remarks,
        Field.MaximumTemperature,
        Field.MinimumTemperature,
    };

    private readonly StringBuilder _text = new StringBuilder();
    private readonly Phrasebook _phrasebook;
    private readonly int _labelWidth;

    public DescriptionWriter(Phrasebook phrasebook)
    {
        _phrasebook = phrasebook;
        _labelWidth = Fields.Max(f => phrasebook.Label(f).Length) + 2;
    }

    public void Line(string text, int indent = 0)
    {
        if (_text.Length > 0)
        {
            _text.Append('\n');
        }

        _text.Append(' ', indent * 2).Append(text);
    }

    public void Add(Field field, string value, int indent = 0) =>
        Line((_phrasebook.Label(field) + ":").PadRight(_labelWidth) + value, indent);

    public override string ToString() => _text.ToString();
}
