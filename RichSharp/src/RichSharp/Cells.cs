using System.Runtime.CompilerServices;
using System.Text;

namespace RichSharp;

/// <summary>
/// Utilities for calculating terminal cell widths of Unicode characters.
/// </summary>
public static class Cells
{
    // Cache for single-cell character lookups
    private static readonly HashSet<int> _singleCells;

    static Cells()
    {
        // Ranges of unicode codepoints that produce a 1-cell wide character
        var singleCellRanges = new List<(int Start, int End)>
        {
            (0x20, 0x7E),        // Latin (excluding non-printable)
            (0xA0, 0xAC),
            (0xAE, 0x002FF),
            (0x00370, 0x00482),  // Greek / Cyrillic
            (0x02500, 0x025FC),  // Box drawing, box elements, geometric shapes
            (0x02800, 0x028FF),  // Braille
        };

        _singleCells = new HashSet<int>();
        foreach (var (start, end) in singleCellRanges)
        {
            for (int i = start; i <= end; i++)
            {
                _singleCells.Add(i);
            }
        }
    }

    /// <summary>
    /// Checks if all characters in the text are single-cell width.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSingleCellWidths(string text)
    {
        foreach (var rune in text.EnumerateRunes())
        {
            if (!_singleCells.Contains(rune.Value))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Get the cell size of a single character/rune.
    /// </summary>
    /// <param name="rune">A single Unicode rune.</param>
    /// <returns>Number of cells (0, 1, or 2) occupied by that character.</returns>
    public static int GetCharacterCellSize(Rune rune)
    {
        int codepoint = rune.Value;

        // Control characters
        if (codepoint < 0x20 || (codepoint >= 0x7F && codepoint < 0xA0))
            return 0;

        // Known single-cell
        if (_singleCells.Contains(codepoint))
            return 1;

        // Use .NET's East Asian Width property as a heuristic
        // This is a simplification - the Python version uses a detailed lookup table
        if (codepoint >= 0x1100)
        {
            // Common East Asian wide character ranges
            if ((codepoint >= 0x1100 && codepoint <= 0x115F) ||  // Hangul Jamo
                (codepoint >= 0x2E80 && codepoint <= 0x9FFF) ||  // CJK
                (codepoint >= 0xAC00 && codepoint <= 0xD7A3) ||  // Hangul Syllables
                (codepoint >= 0xF900 && codepoint <= 0xFAFF) ||  // CJK Compatibility
                (codepoint >= 0xFE10 && codepoint <= 0xFE19) ||  // Vertical forms
                (codepoint >= 0xFE30 && codepoint <= 0xFE6F) ||  // CJK Compatibility Forms
                (codepoint >= 0xFF00 && codepoint <= 0xFF60) ||  // Fullwidth Forms
                (codepoint >= 0xFFE0 && codepoint <= 0xFFE6) ||  // Fullwidth Forms
                (codepoint >= 0x20000 && codepoint <= 0x2FFFD) || // CJK Extension
                (codepoint >= 0x30000 && codepoint <= 0x3FFFD))   // CJK Extension
            {
                return 2;
            }
        }

        return 1;
    }

    /// <summary>
    /// Get the number of cells required to display text.
    /// </summary>
    /// <param name="text">Text to display.</param>
    /// <returns>Number of cells required to display text.</returns>
    public static int CellLength(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        if (IsSingleCellWidths(text))
            return text.Length;

        int total = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            total += GetCharacterCellSize(rune);
        }
        return total;
    }

    /// <summary>
    /// Set the length of a string to fit within given number of cells.
    /// </summary>
    /// <param name="text">Text to fit.</param>
    /// <param name="total">Maximum number of cells.</param>
    /// <returns>Text truncated to fit within the specified cell count.</returns>
    public static string SetCellSize(string text, int total)
    {
        if (IsSingleCellWidths(text))
        {
            int size = text.Length;
            if (size < total)
                return text + new string(' ', total - size);
            return text[..total];
        }

        int cellLength = CellLength(text);
        if (cellLength == total)
            return text;

        if (cellLength < total)
            return text + new string(' ', total - cellLength);

        // Truncate to fit
        var result = new StringBuilder();
        int currentLength = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            int charSize = GetCharacterCellSize(rune);
            if (currentLength + charSize > total)
                break;
            result.Append(rune);
            currentLength += charSize;
        }

        // Pad if necessary
        if (currentLength < total)
            result.Append(' ', total - currentLength);

        return result.ToString();
    }
}
