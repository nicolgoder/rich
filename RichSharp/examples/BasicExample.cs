using RichSharp;

namespace RichSharp.Examples;

/// <summary>
/// Basic example demonstrating RichSharp functionality.
/// </summary>
public class BasicExample
{
    public static void Main(string[] args)
    {
        var console = new RichConsole();

        // Simple text printing
        console.PrintLine("Hello, RichSharp!");

        // Styled text
        console.Print("Bold text", new Style(bold: true));
        console.PrintLine();

        // Colored text
        var redStyle = new Style(color: Color.FromRgb(255, 0, 0));
        console.Print("Red text", redStyle);
        console.PrintLine();

        // Combined styles
        var fancyStyle = new Style(
            color: Color.Parse("bright_cyan"),
            bgcolor: Color.Parse("blue"),
            bold: true,
            italic: true
        );
        console.Print("Fancy styled text!", fancyStyle);
        console.PrintLine();

        // Using Text class
        var text = new Text();
        text.Append("This is ");
        text.Append("bold", new Style(bold: true));
        text.Append(" and this is ");
        text.Append("italic", new Style(italic: true));
        text.Append("!");
        console.Print(text);
        console.PrintLine();

        // Named colors
        console.Print("Red ", new Style(color: Color.Parse("red")));
        console.Print("Green ", new Style(color: Color.Parse("green")));
        console.Print("Blue", new Style(color: Color.Parse("blue")));
        console.PrintLine();

        // Background colors
        console.Print("Text with background",
            new Style(
                color: Color.Parse("white"),
                bgcolor: Color.Parse("dark_blue")
            )
        );
        console.PrintLine();

        // Assembling text
        var assembled = Text.Assemble(
            "Normal text, ",
            ("bold text", new Style(bold: true)),
            ", and ",
            ("colored text", new Style(color: Color.FromRgb(255, 165, 0)))
        );
        console.Print(assembled);
        console.PrintLine();
    }
}
