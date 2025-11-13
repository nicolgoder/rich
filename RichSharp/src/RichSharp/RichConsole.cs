using System.Text;

namespace RichSharp;

/// <summary>
/// A high-level console interface for Rich.
/// Handles all console output with styling and color support.
/// </summary>
public class RichConsole
{
    private readonly TextWriter _output;
    private readonly bool _forceTerminal;
    private readonly ColorSystem _colorSystem;

    /// <summary>
    /// Default width of the console.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Default height of the console.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Color system to use.
    /// </summary>
    public ColorSystem ColorSystem => _colorSystem;

    /// <summary>
    /// Tab size for rendering.
    /// </summary>
    public int TabSize { get; set; } = 4;

    /// <summary>
    /// Creates a new RichConsole instance.
    /// </summary>
    public RichConsole(
        TextWriter? output = null,
        int? width = null,
        int? height = null,
        ColorSystem? colorSystem = null,
        bool forceTerminal = false)
    {
        _output = output ?? Console.Out;
        _forceTerminal = forceTerminal;
        _colorSystem = colorSystem ?? DetectColorSystem();

        // Try to get console dimensions
        try
        {
            Width = width ?? Console.WindowWidth;
            Height = height ?? Console.WindowHeight;
        }
        catch
        {
            Width = width ?? 80;
            Height = height ?? 25;
        }
    }

    /// <summary>
    /// Detect the color system supported by the terminal.
    /// </summary>
    private static ColorSystem DetectColorSystem()
    {
        // Check environment variables
        var term = Environment.GetEnvironmentVariable("TERM");
        var colorterm = Environment.GetEnvironmentVariable("COLORTERM");

        if (colorterm == "truecolor" || colorterm == "24bit")
            return ColorSystem.Truecolor;

        if (term != null && term.Contains("256color"))
            return ColorSystem.EightBit;

        // On Windows, check if ANSI escape codes are supported
        if (OperatingSystem.IsWindows())
        {
            try
            {
                // Try to enable virtual terminal processing
                var handle = GetStdHandle(-11); // STD_OUTPUT_HANDLE
                if (handle != IntPtr.Zero)
                {
                    if (GetConsoleMode(handle, out uint mode))
                    {
                        mode |= 0x0004; // ENABLE_VIRTUAL_TERMINAL_PROCESSING
                        if (SetConsoleMode(handle, mode))
                            return ColorSystem.Truecolor;
                    }
                }
            }
            catch
            {
                // Fall back to standard colors
            }
            return ColorSystem.Windows;
        }

        return ColorSystem.Standard;
    }

    // Windows Console API P/Invoke
    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    /// <summary>
    /// Print a renderable object to the console.
    /// </summary>
    public void Print(IRenderable renderable)
    {
        var options = new ConsoleOptions(
            MinWidth: 0,
            MaxWidth: Width,
            Height: Height
        );

        var segments = renderable.Render(this, options);
        RenderSegments(segments);
    }

    /// <summary>
    /// Print text to the console.
    /// </summary>
    public void Print(string text, Style? style = null)
    {
        var textObj = new Text(text, style);
        Print(textObj);
    }

    /// <summary>
    /// Print styled text with inline markup.
    /// </summary>
    public void Print(params object[] parts)
    {
        var text = Text.Assemble(parts);
        Print(text);
    }

    /// <summary>
    /// Write a line to the console.
    /// </summary>
    public void PrintLine(string text = "", Style? style = null)
    {
        Print(text + "\n", style);
    }

    /// <summary>
    /// Render segments to the output.
    /// </summary>
    private void RenderSegments(IEnumerable<Segment> segments)
    {
        var builder = new StringBuilder();
        Style? currentStyle = null;

        foreach (var segment in segments)
        {
            if (segment.IsEmpty)
                continue;

            // Apply style if different from current
            if (segment.Style != currentStyle)
            {
                if (currentStyle != null && !currentStyle.IsNull)
                {
                    // Reset previous style
                    builder.Append("\x1b[0m");
                }

                if (segment.Style != null && !segment.Style.IsNull)
                {
                    segment.Style.Render(builder, _colorSystem);
                }

                currentStyle = segment.Style;
            }

            builder.Append(segment.Text);
        }

        // Reset style at the end
        if (currentStyle != null && !currentStyle.IsNull)
        {
            builder.Append("\x1b[0m");
        }

        _output.Write(builder.ToString());
        _output.Flush();
    }

    /// <summary>
    /// Render a renderable to a string (without ANSI codes).
    /// </summary>
    public string RenderToPlainText(IRenderable renderable)
    {
        var options = new ConsoleOptions(
            MinWidth: 0,
            MaxWidth: Width,
            Height: Height
        );

        var segments = renderable.Render(this, options);
        return string.Join("", segments.Select(s => s.Text));
    }

    /// <summary>
    /// Create console options for rendering.
    /// </summary>
    public ConsoleOptions GetOptions(int? maxWidth = null)
    {
        return new ConsoleOptions(
            MinWidth: 0,
            MaxWidth: maxWidth ?? Width,
            Height: Height
        );
    }
}
