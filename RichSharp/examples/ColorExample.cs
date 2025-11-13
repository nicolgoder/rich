using RichSharp;

namespace RichSharp.Examples;

/// <summary>
/// Demonstrates all the different ways to specify colors in RichSharp.
/// </summary>
public class ColorExample
{
    public static void Main(string[] args)
    {
        var console = new RichConsole();

        console.PrintLine("=== Color Examples ===\n");

        // Named colors (16 standard colors)
        console.PrintLine("Standard named colors:");
        console.Print("black ", new Style(color: Color.Parse("black")));
        console.Print("red ", new Style(color: Color.Parse("red")));
        console.Print("green ", new Style(color: Color.Parse("green")));
        console.Print("yellow ", new Style(color: Color.Parse("yellow")));
        console.PrintLine();
        console.Print("blue ", new Style(color: Color.Parse("blue")));
        console.Print("magenta ", new Style(color: Color.Parse("magenta")));
        console.Print("cyan ", new Style(color: Color.Parse("cyan")));
        console.Print("white", new Style(color: Color.Parse("white")));
        console.PrintLine("\n");

        // Bright colors
        console.PrintLine("Bright colors:");
        console.Print("bright_red ", new Style(color: Color.Parse("bright_red")));
        console.Print("bright_green ", new Style(color: Color.Parse("bright_green")));
        console.Print("bright_blue", new Style(color: Color.Parse("bright_blue")));
        console.PrintLine("\n");

        // 256-color palette
        console.PrintLine("Extended 256-color palette:");
        console.Print("gold1 ", new Style(color: Color.Parse("gold1")));
        console.Print("orange1 ", new Style(color: Color.Parse("orange1")));
        console.Print("dark_orange ", new Style(color: Color.Parse("dark_orange")));
        console.Print("hot_pink", new Style(color: Color.Parse("hot_pink")));
        console.PrintLine("\n");

        // RGB colors
        console.PrintLine("True color (RGB):");
        console.Print("Pure Red ", new Style(color: Color.FromRgb(255, 0, 0)));
        console.Print("Pure Green ", new Style(color: Color.FromRgb(0, 255, 0)));
        console.Print("Pure Blue ", new Style(color: Color.FromRgb(0, 0, 255)));
        console.PrintLine();
        console.Print("Orange ", new Style(color: Color.FromRgb(255, 165, 0)));
        console.Print("Purple ", new Style(color: Color.FromRgb(128, 0, 128)));
        console.Print("Teal", new Style(color: Color.FromRgb(0, 128, 128)));
        console.PrintLine("\n");

        // Hex colors
        console.PrintLine("Hex colors:");
        console.Print("#FF0000 ", new Style(color: Color.Parse("#FF0000")));
        console.Print("#00FF00 ", new Style(color: Color.Parse("#00FF00")));
        console.Print("#0000FF", new Style(color: Color.Parse("#0000FF")));
        console.PrintLine("\n");

        // Color functions
        console.PrintLine("Color function (256-color palette by number):");
        console.Print("color(196) ", new Style(color: Color.Parse("color(196)")));
        console.Print("color(46) ", new Style(color: Color.Parse("color(46)")));
        console.Print("color(21)", new Style(color: Color.Parse("color(21)")));
        console.PrintLine("\n");

        // RGB function
        console.PrintLine("RGB function:");
        console.Print("rgb(255,100,50) ", new Style(color: Color.Parse("rgb(255,100,50)")));
        console.Print("rgb(50,200,150)", new Style(color: Color.Parse("rgb(50,200,150)")));
        console.PrintLine("\n");

        // Background colors
        console.PrintLine("Background colors:");
        console.Print("  White on Red  ",
            new Style(color: Color.Parse("white"), bgcolor: Color.Parse("red")));
        console.Print("  Black on Yellow  ",
            new Style(color: Color.Parse("black"), bgcolor: Color.Parse("yellow")));
        console.Print("  White on Blue  ",
            new Style(color: Color.Parse("white"), bgcolor: Color.Parse("blue")));
        console.PrintLine("\n");

        // Gradient effect using RGB
        console.PrintLine("Gradient effect:");
        for (int i = 0; i <= 255; i += 16)
        {
            console.Print("█", new Style(color: Color.FromRgb(i, 0, 255 - i)));
        }
        console.PrintLine();
    }
}
