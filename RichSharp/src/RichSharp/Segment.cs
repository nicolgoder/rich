using System.Collections.Immutable;

namespace RichSharp;

/// <summary>
/// A piece of text with associated style. Segments are produced by the Console render process
/// and are ultimately converted into strings to be written to the terminal.
/// </summary>
/// <param name="Text">A piece of text.</param>
/// <param name="Style">An optional style to apply to the text.</param>
/// <param name="Control">Optional sequence of control codes.</param>
public readonly record struct Segment(
    string Text,
    Style? Style = null,
    ImmutableArray<ControlCode>? Control = null)
{
    /// <summary>
    /// The number of terminal cells required to display this segment's text.
    /// </summary>
    public int CellLength => Control.HasValue && Control.Value.Length > 0 ? 0 : Cells.CellLength(Text);

    /// <summary>
    /// Check if the segment contains text.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Text);

    /// <summary>
    /// Check if the segment contains control codes.
    /// </summary>
    public bool IsControl => Control.HasValue && Control.Value.Length > 0;

    /// <summary>
    /// Make a new line segment.
    /// </summary>
    public static Segment Line() => new("\n");

    /// <summary>
    /// Split segment into two segments at the specified column.
    /// If the cut point falls in the middle of a 2-cell wide character then it is replaced
    /// by two spaces, to preserve the display width of the parent segment.
    /// </summary>
    /// <param name="cut">Offset within the segment to cut.</param>
    /// <returns>Two segments.</returns>
    public (Segment Left, Segment Right) SplitCells(int cut)
    {
        if (cut < 0)
            throw new ArgumentOutOfRangeException(nameof(cut), "Cut position must be >= 0");

        if (Cells.IsSingleCellWidths(Text))
        {
            // Fast path with all 1-cell characters
            if (cut >= Text.Length)
                return (this, new Segment("", Style, Control));

            return (
                new Segment(Text[..cut], Style, Control),
                new Segment(Text[cut..], Style, Control)
            );
        }

        return SplitCellsSlow(cut);
    }

    private (Segment Left, Segment Right) SplitCellsSlow(int cut)
    {
        int cellLength = CellLength;
        if (cut >= cellLength)
            return (this, new Segment("", Style, Control));

        // Find the position to cut
        int pos = Math.Min((int)((cut / (double)cellLength) * Text.Length), Text.Length - 1);

        while (true)
        {
            string before = Text[..pos];
            int cellPos = Cells.CellLength(before);
            int outBy = cellPos - cut;

            if (outBy == 0)
            {
                return (
                    new Segment(before, Style, Control),
                    new Segment(Text[pos..], Style, Control)
                );
            }

            // Handle double-width character splits
            if (pos < Text.Length)
            {
                var currentRune = System.Text.Rune.GetRuneAt(Text, pos);
                if (outBy == -1 && Cells.GetCharacterCellSize(currentRune) == 2)
                {
                    return (
                        new Segment(before + " ", Style, Control),
                        new Segment(" " + Text[(pos + currentRune.Utf16SequenceLength)..], Style, Control)
                    );
                }
            }

            if (pos > 0)
            {
                var prevRune = System.Text.Rune.GetRuneAt(Text, pos - 1);
                if (outBy == 1 && Cells.GetCharacterCellSize(prevRune) == 2)
                {
                    return (
                        new Segment(Text[..(pos - 1)] + " ", Style, Control),
                        new Segment(" " + Text[pos..], Style, Control)
                    );
                }
            }

            if (cellPos < cut)
                pos++;
            else
                pos--;

            if (pos < 0 || pos >= Text.Length)
                break;
        }

        // Fallback
        return (this, new Segment("", Style, Control));
    }

    /// <summary>
    /// Apply style(s) to an iterable of segments.
    /// Returns segments where the style is replaced by style + segment.style + postStyle.
    /// </summary>
    public static IEnumerable<Segment> ApplyStyle(
        IEnumerable<Segment> segments,
        Style? style = null,
        Style? postStyle = null)
    {
        if (style == null && postStyle == null)
        {
            foreach (var segment in segments)
                yield return segment;
            yield break;
        }

        foreach (var segment in segments)
        {
            var newStyle = style;
            if (segment.Style is not null)
                newStyle = newStyle?.Combine(segment.Style) ?? segment.Style;
            if (postStyle is not null)
                newStyle = newStyle?.Combine(postStyle) ?? postStyle;

            yield return segment with { Style = newStyle };
        }
    }

    /// <summary>
    /// Filter out empty segments.
    /// </summary>
    public static IEnumerable<Segment> FilterControl(
        IEnumerable<Segment> segments,
        bool isControl)
    {
        foreach (var segment in segments)
        {
            if (segment.IsControl == isControl)
                yield return segment;
        }
    }

    /// <summary>
    /// Split segments into lines.
    /// </summary>
    public static IEnumerable<IEnumerable<Segment>> SplitLines(IEnumerable<Segment> segments)
    {
        var line = new List<Segment>();

        foreach (var segment in segments)
        {
            if (segment.Text.Contains('\n'))
            {
                var lines = segment.Text.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i > 0)
                    {
                        yield return line;
                        line = new List<Segment>();
                    }

                    if (!string.IsNullOrEmpty(lines[i]))
                        line.Add(segment with { Text = lines[i] });
                }
            }
            else
            {
                line.Add(segment);
            }
        }

        if (line.Count > 0)
            yield return line;
    }
}
