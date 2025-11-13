using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace RichSharp;

/// <summary>
/// One of the 4 color systems supported by terminals.
/// </summary>
public enum ColorSystem
{
    Standard = 1,    // 16 colors
    EightBit = 2,    // 256 colors
    Truecolor = 3,   // 16.7M colors
    Windows = 4      // Windows console colors
}

/// <summary>
/// Type of color stored in Color class.
/// </summary>
public enum ColorType
{
    Default = 0,
    Standard = 1,
    EightBit = 2,
    Truecolor = 3,
    Windows = 4
}

/// <summary>
/// Exception thrown when a color cannot be parsed.
/// </summary>
public class ColorParseException : Exception
{
    public ColorParseException(string message) : base(message) { }
}

/// <summary>
/// Terminal color definition.
/// </summary>
/// <param name="Name">The name of the color (typically the input to Color.Parse).</param>
/// <param name="Type">The type of the color.</param>
/// <param name="Number">The color number, if a standard color, or null.</param>
/// <param name="Triplet">A triplet of color components, if an RGB color.</param>
public record Color(
    string Name,
    ColorType Type,
    int? Number = null,
    ColorTriplet? Triplet = null)
{
    private static readonly Regex ColorRegex = new(
        @"^#([0-9a-f]{6})$|^color\(([0-9]{1,3})\)$|^rgb\(([\d\s,]+)\)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly ConcurrentDictionary<string, Color> ParseCache = new();
    private static readonly ConcurrentDictionary<(Color, bool), string[]> AnsiCodeCache = new();

    /// <summary>
    /// ANSI color name to number mapping (256 color palette).
    /// </summary>
    public static readonly IReadOnlyDictionary<string, int> AnsiColorNames = new Dictionary<string, int>
    {
        ["black"] = 0, ["red"] = 1, ["green"] = 2, ["yellow"] = 3,
        ["blue"] = 4, ["magenta"] = 5, ["cyan"] = 6, ["white"] = 7,
        ["bright_black"] = 8, ["bright_red"] = 9, ["bright_green"] = 10,
        ["bright_yellow"] = 11, ["bright_blue"] = 12, ["bright_magenta"] = 13,
        ["bright_cyan"] = 14, ["bright_white"] = 15,
        ["grey0"] = 16, ["gray0"] = 16, ["navy_blue"] = 17, ["dark_blue"] = 18,
        ["blue3"] = 20, ["blue1"] = 21, ["dark_green"] = 22,
        ["deep_sky_blue4"] = 25, ["dodger_blue3"] = 26, ["dodger_blue2"] = 27,
        ["green4"] = 28, ["spring_green4"] = 29, ["turquoise4"] = 30,
        ["deep_sky_blue3"] = 32, ["dodger_blue1"] = 33, ["green3"] = 40,
        ["spring_green3"] = 41, ["dark_cyan"] = 36, ["light_sea_green"] = 37,
        ["deep_sky_blue2"] = 38, ["deep_sky_blue1"] = 39, ["spring_green2"] = 47,
        ["cyan3"] = 43, ["dark_turquoise"] = 44, ["turquoise2"] = 45,
        ["green1"] = 46, ["spring_green1"] = 48, ["medium_spring_green"] = 49,
        ["cyan2"] = 50, ["cyan1"] = 51, ["dark_red"] = 88, ["deep_pink4"] = 125,
        ["purple4"] = 55, ["purple3"] = 56, ["blue_violet"] = 57, ["orange4"] = 94,
        ["grey37"] = 59, ["gray37"] = 59, ["medium_purple4"] = 60,
        ["slate_blue3"] = 62, ["royal_blue1"] = 63, ["chartreuse4"] = 64,
        ["dark_sea_green4"] = 71, ["pale_turquoise4"] = 66, ["steel_blue"] = 67,
        ["steel_blue3"] = 68, ["cornflower_blue"] = 69, ["chartreuse3"] = 76,
        ["cadet_blue"] = 73, ["sky_blue3"] = 74, ["steel_blue1"] = 81,
        ["pale_green3"] = 114, ["sea_green3"] = 78, ["aquamarine3"] = 79,
        ["medium_turquoise"] = 80, ["chartreuse2"] = 112, ["sea_green2"] = 83,
        ["sea_green1"] = 85, ["aquamarine1"] = 122, ["dark_slate_gray2"] = 87,
        ["dark_magenta"] = 91, ["dark_violet"] = 128, ["purple"] = 129,
        ["light_pink4"] = 95, ["plum4"] = 96, ["medium_purple3"] = 98,
        ["slate_blue1"] = 99, ["yellow4"] = 106, ["wheat4"] = 101,
        ["grey53"] = 102, ["gray53"] = 102, ["light_slate_grey"] = 103,
        ["light_slate_gray"] = 103, ["medium_purple"] = 104, ["light_slate_blue"] = 105,
        ["dark_olive_green3"] = 149, ["dark_sea_green"] = 108, ["light_sky_blue3"] = 110,
        ["sky_blue2"] = 111, ["dark_sea_green3"] = 150, ["dark_slate_gray3"] = 116,
        ["sky_blue1"] = 117, ["chartreuse1"] = 118, ["light_green"] = 120,
        ["pale_green1"] = 156, ["dark_slate_gray1"] = 123, ["red3"] = 160,
        ["medium_violet_red"] = 126, ["magenta3"] = 164, ["dark_orange3"] = 166,
        ["indian_red"] = 167, ["hot_pink3"] = 168, ["medium_orchid3"] = 133,
        ["medium_orchid"] = 134, ["medium_purple2"] = 140, ["dark_goldenrod"] = 136,
        ["light_salmon3"] = 173, ["rosy_brown"] = 138, ["grey63"] = 139,
        ["gray63"] = 139, ["medium_purple1"] = 141, ["gold3"] = 178,
        ["dark_khaki"] = 143, ["navajo_white3"] = 144, ["grey69"] = 145,
        ["gray69"] = 145, ["light_steel_blue3"] = 146, ["light_steel_blue"] = 147,
        ["yellow3"] = 184, ["dark_sea_green2"] = 157, ["light_cyan3"] = 152,
        ["light_sky_blue1"] = 153, ["green_yellow"] = 154, ["dark_olive_green2"] = 155,
        ["dark_sea_green1"] = 193, ["pale_turquoise1"] = 159, ["deep_pink3"] = 162,
        ["magenta2"] = 200, ["hot_pink2"] = 169, ["orchid"] = 170,
        ["medium_orchid1"] = 207, ["orange3"] = 172, ["light_pink3"] = 174,
        ["pink3"] = 175, ["plum3"] = 176, ["violet"] = 177,
        ["light_goldenrod3"] = 179, ["tan"] = 180, ["misty_rose3"] = 181,
        ["thistle3"] = 182, ["plum2"] = 183, ["khaki3"] = 185,
        ["light_goldenrod2"] = 222, ["light_yellow3"] = 187, ["grey84"] = 188,
        ["gray84"] = 188, ["light_steel_blue1"] = 189, ["yellow2"] = 190,
        ["dark_olive_green1"] = 192, ["honeydew2"] = 194, ["light_cyan1"] = 195,
        ["red1"] = 196, ["deep_pink2"] = 197, ["deep_pink1"] = 199,
        ["magenta1"] = 201, ["orange_red1"] = 202, ["indian_red1"] = 204,
        ["hot_pink"] = 206, ["dark_orange"] = 208, ["salmon1"] = 209,
        ["light_coral"] = 210, ["pale_violet_red1"] = 211, ["orchid2"] = 212,
        ["orchid1"] = 213, ["orange1"] = 214, ["sandy_brown"] = 215,
        ["light_salmon1"] = 216, ["light_pink1"] = 217, ["pink1"] = 218,
        ["plum1"] = 219, ["gold1"] = 220, ["navajo_white1"] = 223,
        ["misty_rose1"] = 224, ["thistle1"] = 225, ["yellow1"] = 226,
        ["light_goldenrod1"] = 227, ["khaki1"] = 228, ["wheat1"] = 229,
        ["cornsilk1"] = 230, ["grey100"] = 231, ["gray100"] = 231,
        ["grey3"] = 232, ["gray3"] = 232, ["grey7"] = 233, ["gray7"] = 233,
        ["grey11"] = 234, ["gray11"] = 234, ["grey15"] = 235, ["gray15"] = 235,
        ["grey19"] = 236, ["gray19"] = 236, ["grey23"] = 237, ["gray23"] = 237,
        ["grey27"] = 238, ["gray27"] = 238, ["grey30"] = 239, ["gray30"] = 239,
        ["grey35"] = 240, ["gray35"] = 240, ["grey39"] = 241, ["gray39"] = 241,
        ["grey42"] = 242, ["gray42"] = 242, ["grey46"] = 243, ["gray46"] = 243,
        ["grey50"] = 244, ["gray50"] = 244, ["grey54"] = 245, ["gray54"] = 245,
        ["grey58"] = 246, ["gray58"] = 246, ["grey62"] = 247, ["gray62"] = 247,
        ["grey66"] = 248, ["gray66"] = 248, ["grey70"] = 249, ["gray70"] = 249,
        ["grey74"] = 250, ["gray74"] = 250, ["grey78"] = 251, ["gray78"] = 251,
        ["grey82"] = 252, ["gray82"] = 252, ["grey85"] = 253, ["gray85"] = 253,
        ["grey89"] = 254, ["gray89"] = 254, ["grey93"] = 255, ["gray93"] = 255
    };

    /// <summary>
    /// Get the native color system for this color.
    /// </summary>
    public ColorSystem System => Type == ColorType.Default
        ? ColorSystem.Standard
        : (ColorSystem)(int)Type;

    /// <summary>
    /// Check if the color is ultimately defined by the system.
    /// </summary>
    public bool IsSystemDefined => System != ColorSystem.EightBit && System != ColorSystem.Truecolor;

    /// <summary>
    /// Check if the color is a default color.
    /// </summary>
    public bool IsDefault => Type == ColorType.Default;

    /// <summary>
    /// Create a Color from its 8-bit ANSI number.
    /// </summary>
    public static Color FromAnsi(int number)
    {
        if (number < 0 || number > 255)
            throw new ArgumentOutOfRangeException(nameof(number), "Color number must be 0-255");

        return new Color(
            $"color({number})",
            number < 16 ? ColorType.Standard : ColorType.EightBit,
            Number: number
        );
    }

    /// <summary>
    /// Create a truecolor RGB color from a triplet of values.
    /// </summary>
    public static Color FromTriplet(ColorTriplet triplet)
    {
        return new Color(triplet.Hex, ColorType.Truecolor, Triplet: triplet);
    }

    /// <summary>
    /// Create a truecolor from three color components in the range 0-255.
    /// </summary>
    public static Color FromRgb(int red, int green, int blue)
    {
        return FromTriplet(new ColorTriplet((byte)red, (byte)green, (byte)blue));
    }

    /// <summary>
    /// Get a Color instance representing the default color.
    /// </summary>
    public static Color Default() => new("default", ColorType.Default);

    /// <summary>
    /// Parse a color definition.
    /// </summary>
    public static Color Parse(string color)
    {
        return ParseCache.GetOrAdd(color, static c =>
        {
            var originalColor = c;
            c = c.ToLowerInvariant().Trim();

            if (c == "default")
                return new Color(c, ColorType.Default);

            if (AnsiColorNames.TryGetValue(c, out int colorNumber))
            {
                return new Color(
                    c,
                    colorNumber < 16 ? ColorType.Standard : ColorType.EightBit,
                    Number: colorNumber
                );
            }

            var match = ColorRegex.Match(c);
            if (!match.Success)
                throw new ColorParseException($"'{originalColor}' is not a valid color");

            var color24 = match.Groups[1].Value;
            var color8 = match.Groups[2].Value;
            var colorRgb = match.Groups[3].Value;

            if (!string.IsNullOrEmpty(color24))
            {
                var triplet = new ColorTriplet(
                    Convert.ToByte(color24[0..2], 16),
                    Convert.ToByte(color24[2..4], 16),
                    Convert.ToByte(color24[4..6], 16)
                );
                return new Color(c, ColorType.Truecolor, Triplet: triplet);
            }
            else if (!string.IsNullOrEmpty(color8))
            {
                int number = int.Parse(color8);
                if (number > 255)
                    throw new ColorParseException($"Color number must be <= 255 in '{c}'");
                return new Color(
                    c,
                    number < 16 ? ColorType.Standard : ColorType.EightBit,
                    Number: number
                );
            }
            else // colorRgb
            {
                var components = colorRgb.Split(',');
                if (components.Length != 3)
                    throw new ColorParseException($"Expected three components in '{originalColor}'");

                byte red = byte.Parse(components[0].Trim());
                byte green = byte.Parse(components[1].Trim());
                byte blue = byte.Parse(components[2].Trim());
                var triplet = new ColorTriplet(red, green, blue);
                return new Color(c, ColorType.Truecolor, Triplet: triplet);
            }
        });
    }

    /// <summary>
    /// Get the ANSI escape codes for this color.
    /// </summary>
    public string[] GetAnsiCodes(bool foreground = true)
    {
        return AnsiCodeCache.GetOrAdd((this, foreground), static key =>
        {
            var (color, fg) = key;
            return color.Type switch
            {
                ColorType.Default => new[] { fg ? "39" : "49" },
                ColorType.Windows => GetWindowsAnsiCodes(color.Number!.Value, fg),
                ColorType.Standard => GetStandardAnsiCodes(color.Number!.Value, fg),
                ColorType.EightBit => GetEightBitAnsiCodes(color.Number!.Value, fg),
                ColorType.Truecolor => GetTruecolorAnsiCodes(color.Triplet!.Value, fg),
                _ => new[] { fg ? "39" : "49" }
            };
        });
    }

    private static string[] GetWindowsAnsiCodes(int number, bool foreground)
    {
        var (fore, back) = number < 8 ? (30, 40) : (82, 92);
        return new[] { (foreground ? fore + number : back + number).ToString() };
    }

    private static string[] GetStandardAnsiCodes(int number, bool foreground)
    {
        if (foreground)
        {
            return number < 8
                ? new[] { (30 + number).ToString() }
                : new[] { (82 + number).ToString() };
        }
        else
        {
            return number < 8
                ? new[] { (40 + number).ToString() }
                : new[] { (92 + number).ToString() };
        }
    }

    private static string[] GetEightBitAnsiCodes(int number, bool foreground)
    {
        return foreground
            ? new[] { "38", "5", number.ToString() }
            : new[] { "48", "5", number.ToString() };
    }

    private static string[] GetTruecolorAnsiCodes(ColorTriplet triplet, bool foreground)
    {
        return foreground
            ? new[] { "38", "2", triplet.Red.ToString(), triplet.Green.ToString(), triplet.Blue.ToString() }
            : new[] { "48", "2", triplet.Red.ToString(), triplet.Green.ToString(), triplet.Blue.ToString() };
    }
}
