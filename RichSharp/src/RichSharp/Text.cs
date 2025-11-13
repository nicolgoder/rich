using System.Collections.Immutable;
using System.Text;

namespace RichSharp;

/// <summary>
/// A marked up region in some text.
/// </summary>
/// <param name="Start">Span start index.</param>
/// <param name="End">Span end index.</param>
/// <param name="Style">Style associated with the span.</param>
public readonly record struct Span(int Start, int End, Style Style)
{
    /// <summary>
    /// Check if the span is non-empty.
    /// </summary>
    public bool IsValid => End > Start;

    /// <summary>
    /// Split a span into 2 from a given offset.
    /// </summary>
    public (Span First, Span? Second) Split(int offset)
    {
        if (offset < Start || offset >= End)
            return (this, null);

        return (
            new Span(Start, Math.Min(End, offset), Style),
            new Span(Math.Min(End, offset), End, Style)
        );
    }

    /// <summary>
    /// Move start and end by a given offset.
    /// </summary>
    public Span Move(int offset) => new(Start + offset, End + offset, Style);

    /// <summary>
    /// Crop the span at the given offset.
    /// </summary>
    public Span RightCrop(int offset)
    {
        if (offset >= End)
            return this;
        return new Span(Start, Math.Min(offset, End), Style);
    }

    /// <summary>
    /// Extend the span by the given number of cells.
    /// </summary>
    public Span Extend(int cells) => cells > 0 ? new Span(Start, End + cells, Style) : this;
}

/// <summary>
/// Text with color and style. Represents styled text with multiple style spans.
/// </summary>
public class Text : IRenderable
{
    private readonly List<string> _text;
    private readonly List<Span> _spans;
    private int _length;

    /// <summary>
    /// Base style for the text.
    /// </summary>
    public Style? BaseStyle { get; set; }

    /// <summary>
    /// Justify method for the text.
    /// </summary>
    public JustifyMethod? Justify { get; set; }

    /// <summary>
    /// Overflow method for the text.
    /// </summary>
    public OverflowMethod? Overflow { get; set; }

    /// <summary>
    /// Disable text wrapping.
    /// </summary>
    public bool? NoWrap { get; set; }

    /// <summary>
    /// Character to end text with (e.g., newline).
    /// </summary>
    public string End { get; set; } = "\n";

    /// <summary>
    /// Number of spaces per tab.
    /// </summary>
    public int? TabSize { get; set; }

    /// <summary>
    /// Creates a new Text instance.
    /// </summary>
    public Text(
        string text = "",
        Style? style = null,
        JustifyMethod? justify = null,
        OverflowMethod? overflow = null,
        bool? noWrap = null,
        string end = "\n",
        int? tabSize = null,
        List<Span>? spans = null)
    {
        _text = new List<string> { text };
        BaseStyle = style;
        Justify = justify;
        Overflow = overflow;
        NoWrap = noWrap;
        End = end;
        TabSize = tabSize;
        _spans = spans ?? new List<Span>();
        _length = text.Length;
    }

    /// <summary>
    /// Get the plain text without formatting.
    /// </summary>
    public string Plain => string.Join("", _text);

    /// <summary>
    /// Get the length of the text.
    /// </summary>
    public int Length => _length;

    /// <summary>
    /// Get the cell length (accounting for wide characters).
    /// </summary>
    public int CellLength => Cells.CellLength(Plain);

    /// <summary>
    /// Get all spans.
    /// </summary>
    public IReadOnlyList<Span> Spans => _spans;

    /// <summary>
    /// Append text to this Text instance.
    /// </summary>
    public void Append(string text, Style? style = null)
    {
        if (string.IsNullOrEmpty(text))
            return;

        int startOffset = _length;
        _text.Add(text);
        _length += text.Length;

        if (style != null && !style.IsNull)
        {
            _spans.Add(new Span(startOffset, _length, style));
        }
    }

    /// <summary>
    /// Append another Text instance.
    /// </summary>
    public void Append(Text other)
    {
        if (other.Length == 0)
            return;

        int offset = _length;
        _text.AddRange(other._text);
        _length += other._length;

        foreach (var span in other._spans)
        {
            _spans.Add(span.Move(offset));
        }
    }

    /// <summary>
    /// Create a copy of this Text instance.
    /// </summary>
    public Text Copy()
    {
        return new Text(
            Plain,
            BaseStyle,
            Justify,
            Overflow,
            NoWrap,
            End,
            TabSize,
            new List<Span>(_spans)
        );
    }

    /// <summary>
    /// Create a Text instance by assembling strings and styled text.
    /// </summary>
    public static Text Assemble(params object[] parts)
    {
        var result = new Text();

        foreach (var part in parts)
        {
            switch (part)
            {
                case string str:
                    result.Append(str);
                    break;
                case Text txt:
                    result.Append(txt);
                    break;
                case (string str, Style style):
                    result.Append(str, style);
                    break;
                case (string str, string styleStr):
                    result.Append(str, Style.Parse(styleStr));
                    break;
            }
        }

        return result;
    }

    /// <summary>
    /// Stylize text between start and end positions.
    /// </summary>
    public void Stylize(int start, int end, Style style)
    {
        if (start >= end || start >= _length)
            return;

        end = Math.Min(end, _length);
        _spans.Add(new Span(start, end, style));
    }

    /// <summary>
    /// Apply a style to the entire text.
    /// </summary>
    public void ApplyStyle(Style style)
    {
        if (_length > 0)
            _spans.Add(new Span(0, _length, style));
    }

    /// <summary>
    /// Render this text to segments.
    /// </summary>
    public IEnumerable<Segment> Render(RichConsole console, ConsoleOptions options)
    {
        var text = Plain;
        if (string.IsNullOrEmpty(text))
            yield break;

        // Build a list of style changes at each position
        var styleMap = new Dictionary<int, List<(bool isStart, Style style)>>();

        foreach (var span in _spans)
        {
            if (!styleMap.ContainsKey(span.Start))
                styleMap[span.Start] = new List<(bool, Style)>();
            styleMap[span.Start].Add((true, span.Style));

            if (!styleMap.ContainsKey(span.End))
                styleMap[span.End] = new List<(bool, Style)>();
            styleMap[span.End].Add((false, span.Style));
        }

        // Render segments
        var currentStyle = BaseStyle;
        var lastPos = 0;

        var sortedPositions = styleMap.Keys.OrderBy(k => k).ToList();
        sortedPositions.Add(text.Length);

        foreach (var pos in sortedPositions)
        {
            if (pos > lastPos && lastPos < text.Length)
            {
                var segment = text.Substring(lastPos, Math.Min(pos, text.Length) - lastPos);
                yield return new Segment(segment, currentStyle);
            }

            if (styleMap.TryGetValue(pos, out var changes))
            {
                foreach (var (isStart, style) in changes)
                {
                    if (isStart)
                    {
                        currentStyle = currentStyle?.Combine(style) ?? style;
                    }
                }
            }

            lastPos = pos;
        }
    }

    public override string ToString() => Plain;

    public static implicit operator string(Text text) => text.Plain;
}
