using System.Collections.Concurrent;
using System.Text;

namespace RichSharp;

/// <summary>
/// A terminal style. A terminal style consists of a color (color), a background color (bgcolor),
/// and a number of attributes, such as bold, italic etc.
/// The attributes have 3 states: they can either be on (true), off (false), or not set (null).
/// </summary>
public class Style : IEquatable<Style>
{
    private static readonly ConcurrentDictionary<string, Style> ParseCache = new();
    private static readonly Style _null = new();

    // Bit flags for attributes
    [Flags]
    private enum Attributes
    {
        None = 0,
        Bold = 1 << 0,
        Dim = 1 << 1,
        Italic = 1 << 2,
        Underline = 1 << 3,
        Blink = 1 << 4,
        Blink2 = 1 << 5,
        Reverse = 1 << 6,
        Conceal = 1 << 7,
        Strike = 1 << 8,
        Underline2 = 1 << 9,
        Frame = 1 << 10,
        Encircle = 1 << 11,
        Overline = 1 << 12
    }

    private readonly Color? _color;
    private readonly Color? _bgcolor;
    private readonly Attributes _attributes;
    private readonly Attributes _setAttributes;
    private readonly string? _link;
    private readonly string _linkId;
    private readonly bool _isNull;
    private int? _hashCode;

    /// <summary>
    /// Creates a new Style with the specified properties.
    /// </summary>
    public Style(
        Color? color = null,
        Color? bgcolor = null,
        bool? bold = null,
        bool? dim = null,
        bool? italic = null,
        bool? underline = null,
        bool? blink = null,
        bool? blink2 = null,
        bool? reverse = null,
        bool? conceal = null,
        bool? strike = null,
        bool? underline2 = null,
        bool? frame = null,
        bool? encircle = null,
        bool? overline = null,
        string? link = null)
    {
        _color = color;
        _bgcolor = bgcolor;
        _link = link;
        _linkId = link != null ? $"link{Random.Shared.Next(0, 999999)}" : string.Empty;

        // Build set attributes bitmask
        _setAttributes = Attributes.None;
        if (bold.HasValue) _setAttributes |= Attributes.Bold;
        if (dim.HasValue) _setAttributes |= Attributes.Dim;
        if (italic.HasValue) _setAttributes |= Attributes.Italic;
        if (underline.HasValue) _setAttributes |= Attributes.Underline;
        if (blink.HasValue) _setAttributes |= Attributes.Blink;
        if (blink2.HasValue) _setAttributes |= Attributes.Blink2;
        if (reverse.HasValue) _setAttributes |= Attributes.Reverse;
        if (conceal.HasValue) _setAttributes |= Attributes.Conceal;
        if (strike.HasValue) _setAttributes |= Attributes.Strike;
        if (underline2.HasValue) _setAttributes |= Attributes.Underline2;
        if (frame.HasValue) _setAttributes |= Attributes.Frame;
        if (encircle.HasValue) _setAttributes |= Attributes.Encircle;
        if (overline.HasValue) _setAttributes |= Attributes.Overline;

        // Build attributes bitmask
        _attributes = Attributes.None;
        if (bold == true) _attributes |= Attributes.Bold;
        if (dim == true) _attributes |= Attributes.Dim;
        if (italic == true) _attributes |= Attributes.Italic;
        if (underline == true) _attributes |= Attributes.Underline;
        if (blink == true) _attributes |= Attributes.Blink;
        if (blink2 == true) _attributes |= Attributes.Blink2;
        if (reverse == true) _attributes |= Attributes.Reverse;
        if (conceal == true) _attributes |= Attributes.Conceal;
        if (strike == true) _attributes |= Attributes.Strike;
        if (underline2 == true) _attributes |= Attributes.Underline2;
        if (frame == true) _attributes |= Attributes.Frame;
        if (encircle == true) _attributes |= Attributes.Encircle;
        if (overline == true) _attributes |= Attributes.Overline;

        _isNull = _setAttributes == Attributes.None && color == null && bgcolor == null && link == null;
    }

    /// <summary>
    /// Create a 'null' style, equivalent to Style(), but cached for performance.
    /// </summary>
    public static Style Null => _null;

    /// <summary>
    /// Create a new style with colors and no attributes.
    /// </summary>
    public static Style FromColor(Color? color = null, Color? bgcolor = null)
    {
        return new Style(color: color, bgcolor: bgcolor);
    }

    // Properties for attributes
    public bool? Bold => GetAttribute(Attributes.Bold);
    public bool? Dim => GetAttribute(Attributes.Dim);
    public bool? Italic => GetAttribute(Attributes.Italic);
    public bool? Underline => GetAttribute(Attributes.Underline);
    public bool? Blink => GetAttribute(Attributes.Blink);
    public bool? Blink2 => GetAttribute(Attributes.Blink2);
    public bool? Reverse => GetAttribute(Attributes.Reverse);
    public bool? Conceal => GetAttribute(Attributes.Conceal);
    public bool? Strike => GetAttribute(Attributes.Strike);
    public bool? Underline2 => GetAttribute(Attributes.Underline2);
    public bool? Frame => GetAttribute(Attributes.Frame);
    public bool? Encircle => GetAttribute(Attributes.Encircle);
    public bool? Overline => GetAttribute(Attributes.Overline);

    /// <summary>
    /// The foreground color or null if it is not set.
    /// </summary>
    public Color? Color => _color;

    /// <summary>
    /// The background color or null if it is not set.
    /// </summary>
    public Color? BgColor => _bgcolor;

    /// <summary>
    /// Link text, if set.
    /// </summary>
    public string? Link => _link;

    /// <summary>
    /// Link ID used in ANSI codes.
    /// </summary>
    public string LinkId => _linkId;

    /// <summary>
    /// Check if the style specified a transparent background.
    /// </summary>
    public bool TransparentBackground => _bgcolor == null || _bgcolor.IsDefault;

    /// <summary>
    /// A Style with background only.
    /// </summary>
    public Style BackgroundStyle => new(bgcolor: _bgcolor);

    /// <summary>
    /// Check if the style is null (has no attributes, colors, or links).
    /// </summary>
    public bool IsNull => _isNull;

    private bool? GetAttribute(Attributes attribute)
    {
        if ((_setAttributes & attribute) != 0)
            return (_attributes & attribute) != 0;
        return null;
    }

    /// <summary>
    /// Combine this style with another style.
    /// </summary>
    public Style Combine(Style other)
    {
        if (other._isNull)
            return this;
        if (_isNull)
            return other;

        // Merge attributes
        bool? MergeAttribute(Attributes attr) =>
            (other._setAttributes & attr) != 0 ? other.GetAttribute(attr) : GetAttribute(attr);

        return new Style(
            color: other._color ?? _color,
            bgcolor: other._bgcolor ?? _bgcolor,
            bold: MergeAttribute(Attributes.Bold),
            dim: MergeAttribute(Attributes.Dim),
            italic: MergeAttribute(Attributes.Italic),
            underline: MergeAttribute(Attributes.Underline),
            blink: MergeAttribute(Attributes.Blink),
            blink2: MergeAttribute(Attributes.Blink2),
            reverse: MergeAttribute(Attributes.Reverse),
            conceal: MergeAttribute(Attributes.Conceal),
            strike: MergeAttribute(Attributes.Strike),
            underline2: MergeAttribute(Attributes.Underline2),
            frame: MergeAttribute(Attributes.Frame),
            encircle: MergeAttribute(Attributes.Encircle),
            overline: MergeAttribute(Attributes.Overline),
            link: other._link ?? _link
        );
    }

    /// <summary>
    /// Generate ANSI escape code for this style.
    /// </summary>
    public string GetAnsiCode(ColorSystem colorSystem = ColorSystem.Truecolor)
    {
        if (_isNull)
            return "";

        var codes = new List<string>();

        // SGR codes for text attributes
        var activeAttrs = _attributes & _setAttributes;
        if ((activeAttrs & Attributes.Bold) != 0) codes.Add("1");
        if ((activeAttrs & Attributes.Dim) != 0) codes.Add("2");
        if ((activeAttrs & Attributes.Italic) != 0) codes.Add("3");
        if ((activeAttrs & Attributes.Underline) != 0) codes.Add("4");
        if ((activeAttrs & Attributes.Blink) != 0) codes.Add("5");
        if ((activeAttrs & Attributes.Blink2) != 0) codes.Add("6");
        if ((activeAttrs & Attributes.Reverse) != 0) codes.Add("7");
        if ((activeAttrs & Attributes.Conceal) != 0) codes.Add("8");
        if ((activeAttrs & Attributes.Strike) != 0) codes.Add("9");
        if ((activeAttrs & Attributes.Underline2) != 0) codes.Add("21");
        if ((activeAttrs & Attributes.Frame) != 0) codes.Add("51");
        if ((activeAttrs & Attributes.Encircle) != 0) codes.Add("52");
        if ((activeAttrs & Attributes.Overline) != 0) codes.Add("53");

        // Foreground color
        if (_color != null)
            codes.AddRange(_color.GetAnsiCodes(foreground: true));

        // Background color
        if (_bgcolor != null)
            codes.AddRange(_bgcolor.GetAnsiCodes(foreground: false));

        return string.Join(";", codes);
    }

    /// <summary>
    /// Render the ANSI codes for this style to a StringBuilder.
    /// </summary>
    public void Render(StringBuilder builder, ColorSystem colorSystem = ColorSystem.Truecolor)
    {
        if (_isNull)
            return;

        var ansiCode = GetAnsiCode(colorSystem);
        if (!string.IsNullOrEmpty(ansiCode))
        {
            builder.Append("\x1b[");
            builder.Append(ansiCode);
            builder.Append('m');
        }
    }

    /// <summary>
    /// Parse a style definition string.
    /// </summary>
    public static Style Parse(string styleDefinition)
    {
        return ParseCache.GetOrAdd(styleDefinition, static def =>
        {
            def = def.Trim();
            if (string.IsNullOrEmpty(def) || def == "none")
                return Null;

            // Simple parser for basic style definitions
            // Format: "bold red on blue" or "italic #ff0000" etc.
            var parts = def.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            Color? color = null;
            Color? bgcolor = null;
            bool? bold = null, dim = null, italic = null, underline = null;
            bool? blink = null, blink2 = null, reverse = null, conceal = null;
            bool? strike = null, underline2 = null, frame = null, encircle = null, overline = null;

            bool onBackground = false;

            foreach (var part in parts)
            {
                var lowerPart = part.ToLowerInvariant();

                if (lowerPart == "on")
                {
                    onBackground = true;
                    continue;
                }

                // Check for attributes
                switch (lowerPart)
                {
                    case "bold" or "b": bold = true; continue;
                    case "dim" or "d": dim = true; continue;
                    case "italic" or "i": italic = true; continue;
                    case "underline" or "u": underline = true; continue;
                    case "blink": blink = true; continue;
                    case "blink2": blink2 = true; continue;
                    case "reverse" or "r": reverse = true; continue;
                    case "conceal" or "c": conceal = true; continue;
                    case "strike" or "s": strike = true; continue;
                    case "underline2" or "uu": underline2 = true; continue;
                    case "frame": frame = true; continue;
                    case "encircle": encircle = true; continue;
                    case "overline" or "o": overline = true; continue;
                    case "not":
                        // Handle "not bold", "not italic" etc.
                        continue;
                }

                // Try to parse as color
                try
                {
                    var parsedColor = RichSharp.Color.Parse(part);
                    if (onBackground)
                        bgcolor = parsedColor;
                    else
                        color = parsedColor;
                }
                catch (ColorParseException)
                {
                    // Ignore invalid colors
                }
            }

            return new Style(
                color, bgcolor, bold, dim, italic, underline,
                blink, blink2, reverse, conceal, strike,
                underline2, frame, encircle, overline
            );
        });
    }

    public override string ToString()
    {
        if (_isNull)
            return "none";

        var parts = new List<string>();

        if (Bold == true) parts.Add("bold");
        if (Dim == true) parts.Add("dim");
        if (Italic == true) parts.Add("italic");
        if (Underline == true) parts.Add("underline");
        if (Blink == true) parts.Add("blink");
        if (Blink2 == true) parts.Add("blink2");
        if (Reverse == true) parts.Add("reverse");
        if (Conceal == true) parts.Add("conceal");
        if (Strike == true) parts.Add("strike");
        if (Underline2 == true) parts.Add("underline2");
        if (Frame == true) parts.Add("frame");
        if (Encircle == true) parts.Add("encircle");
        if (Overline == true) parts.Add("overline");

        if (_color != null)
            parts.Add(_color.Name);
        if (_bgcolor != null)
        {
            parts.Add("on");
            parts.Add(_bgcolor.Name);
        }
        if (_link != null)
        {
            parts.Add("link");
            parts.Add(_link);
        }

        return string.Join(" ", parts);
    }

    public override bool Equals(object? obj) => Equals(obj as Style);

    public bool Equals(Style? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return GetHashCode() == other.GetHashCode();
    }

    public override int GetHashCode()
    {
        if (_hashCode.HasValue)
            return _hashCode.Value;

        _hashCode = HashCode.Combine(_color, _bgcolor, _attributes, _setAttributes, _link);
        return _hashCode.Value;
    }

    public static bool operator ==(Style? left, Style? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Style? left, Style? right) => !(left == right);

    /// <summary>
    /// Combine two styles using the + operator.
    /// </summary>
    public static Style operator +(Style left, Style right) => left.Combine(right);
}
