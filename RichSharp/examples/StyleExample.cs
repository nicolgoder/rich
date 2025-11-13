using RichSharp;

namespace RichSharp.Examples;

/// <summary>
/// Demonstrates all text styling options in RichSharp.
/// </summary>
public class StyleExample
{
    public static void Main(string[] args)
    {
        var console = new RichConsole();

        console.PrintLine("=== Style Examples ===\n");

        // Basic styles
        console.PrintLine("Basic text attributes:");
        console.PrintLine("Normal text");
        console.PrintLine("Bold text", new Style(bold: true));
        console.PrintLine("Dim text", new Style(dim: true));
        console.PrintLine("Italic text", new Style(italic: true));
        console.PrintLine("Underline text", new Style(underline: true));
        console.PrintLine("Strikethrough text", new Style(strike: true));
        console.PrintLine("Overline text", new Style(overline: true));
        console.PrintLine();

        // Combined styles
        console.PrintLine("Combined attributes:");
        console.PrintLine("Bold + Italic", new Style(bold: true, italic: true));
        console.PrintLine("Bold + Underline", new Style(bold: true, underline: true));
        console.PrintLine("Italic + Strikethrough", new Style(italic: true, strike: true));
        console.PrintLine();

        // Reverse video
        console.PrintLine("Reverse video (swap fg/bg):", new Style(reverse: true));
        console.PrintLine();

        // Style combinations with colors
        console.PrintLine("Styles with colors:");
        console.PrintLine("Bold Red",
            new Style(bold: true, color: Color.Parse("red")));
        console.PrintLine("Italic Blue",
            new Style(italic: true, color: Color.Parse("blue")));
        console.PrintLine("Underline Green",
            new Style(underline: true, color: Color.Parse("green")));
        console.PrintLine();

        // Multiple attributes
        console.PrintLine("All the things:",
            new Style(
                bold: true,
                italic: true,
                underline: true,
                color: Color.Parse("bright_magenta")
            )
        );
        console.PrintLine();

        // Style parsing
        console.PrintLine("Parsed styles:");
        console.PrintLine("bold red", Style.Parse("bold red"));
        console.PrintLine("italic blue on yellow", Style.Parse("italic blue on yellow"));
        console.PrintLine("b i u green", Style.Parse("b i u green"));
        console.PrintLine();

        // Style combining
        console.PrintLine("Style combining:");
        var base Style = new Style(bold: true);
        var colorStyle = new Style(color: Color.Parse("cyan"));
        var combined = baseStyle.Combine(colorStyle);
        console.PrintLine("Bold + Cyan (combined)", combined);
        console.PrintLine();

        // Using + operator
        var style1 = new Style(bold: true);
        var style2 = new Style(italic: true, color: Color.Parse("red"));
        console.PrintLine("Bold + Italic Red (using + operator)", style1 + style2);
        console.PrintLine();
    }
}
