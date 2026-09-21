using System.Text;

namespace Cavok.Text;

internal sealed class DescriptionWriter
{
    private readonly StringBuilder _text = new StringBuilder();
    private readonly Phrasebook _phrasebook;
    private readonly int _labelWidth;

    public DescriptionWriter(Phrasebook phrasebook)
    {
        _phrasebook = phrasebook;
        _labelWidth = Enum.GetValues(typeof(Field)).Cast<Field>().Max(f => phrasebook.Label(f).Length) + 2;
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
