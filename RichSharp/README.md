# RichSharp

**RichSharp** is a .NET library for rich text and beautiful formatting in console applications. It's a C# port of the popular Python [Rich](https://github.com/Textualize/rich) library, bringing powerful terminal styling and formatting to .NET 8+.

## Features

- 🎨 **Rich Color Support**
  - 16 standard colors
  - 256-color palette
  - True color (16.7 million colors)
  - Named colors, RGB, and hex color codes

- ✨ **Text Styling**
  - Bold, italic, underline, strikethrough
  - Dim, blink, reverse, conceal
  - Double underline, frame, encircle, overline

- 📝 **Advanced Text Handling**
  - Style spans for partial text styling
  - Unicode and wide character support (CJK)
  - Text assembly and composition

- 🖥️ **Cross-Platform**
  - Works on Windows, macOS, and Linux
  - Automatic terminal capability detection
  - ANSI escape code generation

## Installation

Add RichSharp to your project:

```bash
dotnet add package RichSharp
```

Or add it manually to your `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="RichSharp" Version="1.0.0" />
</ItemGroup>
```

## Quick Start

```csharp
using RichSharp;

var console = new RichConsole();

// Simple colored text
console.Print("Hello, World!", new Style(color: Color.Parse("red")));

// Bold text
console.Print("Bold text", new Style(bold: true));

// Combined styles
console.Print("Fancy text!", new Style(
    color: Color.FromRgb(255, 165, 0),
    bold: true,
    italic: true
));
```

## Usage Examples

### Basic Colors

```csharp
var console = new RichConsole();

// Named colors
console.Print("Red text", new Style(color: Color.Parse("red")));
console.Print("Blue background", new Style(bgcolor: Color.Parse("blue")));

// RGB colors
console.Print("Orange", new Style(color: Color.FromRgb(255, 165, 0)));

// Hex colors
console.Print("Purple", new Style(color: Color.Parse("#800080")));

// 256-color palette
console.Print("Gold", new Style(color: Color.Parse("gold1")));
```

### Text Styling

```csharp
var console = new RichConsole();

// Individual attributes
console.PrintLine("Bold", new Style(bold: true));
console.PrintLine("Italic", new Style(italic: true));
console.PrintLine("Underline", new Style(underline: true));
console.PrintLine("Strikethrough", new Style(strike: true));

// Combined attributes
console.PrintLine("Bold and Italic",
    new Style(bold: true, italic: true));

// With colors
console.PrintLine("Bold Red",
    new Style(bold: true, color: Color.Parse("red")));
```

### Advanced Text with Spans

```csharp
var console = new RichConsole();

// Create rich text with multiple styles
var text = new Text();
text.Append("This is ");
text.Append("bold", new Style(bold: true));
text.Append(" and this is ");
text.Append("red", new Style(color: Color.Parse("red")));
text.Append("!");

console.Print(text);
```

### Text Assembly

```csharp
var console = new RichConsole();

// Assemble text from parts
var text = Text.Assemble(
    "Normal text, ",
    ("bold text", new Style(bold: true)),
    ", and ",
    ("colored text", new Style(color: Color.FromRgb(255, 0, 0)))
);

console.Print(text);
```

### Style Parsing

```csharp
var console = new RichConsole();

// Parse style from string
console.Print("Bold red text", Style.Parse("bold red"));
console.Print("Italic blue on yellow", Style.Parse("italic blue on yellow"));

// Shorthand notation
console.Print("Bold, italic, underline", Style.Parse("b i u green"));
```

### Style Combination

```csharp
// Combine styles using the Combine method
var baseStyle = new Style(bold: true);
var colorStyle = new Style(color: Color.Parse("cyan"));
var combined = baseStyle.Combine(colorStyle);

// Or use the + operator
var style = new Style(bold: true) + new Style(italic: true);
```

## Color Systems

RichSharp automatically detects your terminal's color capabilities:

- **Standard**: 16 colors (basic ANSI)
- **EightBit**: 256 colors
- **Truecolor**: 16.7 million colors (24-bit RGB)
- **Windows**: Windows console colors

You can specify a color system explicitly:

```csharp
var console = new RichConsole(colorSystem: ColorSystem.Truecolor);
```

## Named Colors

RichSharp supports all 256 named colors from the ANSI palette:

**Standard 16 colors:**
- `black`, `red`, `green`, `yellow`, `blue`, `magenta`, `cyan`, `white`
- `bright_black`, `bright_red`, `bright_green`, `bright_yellow`, `bright_blue`, `bright_magenta`, `bright_cyan`, `bright_white`

**Extended 256-color palette:**
- `navy_blue`, `dark_blue`, `blue3`, `blue1`, `dark_green`, `deep_sky_blue4`, etc.
- `gold1`, `orange1`, `hot_pink`, `orchid`, `violet`, etc.
- See [Color.cs](src/RichSharp/Color.cs) for the full list

## API Reference

### `RichConsole`

The main class for console output.

```csharp
var console = new RichConsole(
    output: Console.Out,     // TextWriter (optional)
    width: 80,              // Console width (optional)
    height: 25,             // Console height (optional)
    colorSystem: null       // ColorSystem (auto-detect if null)
);

console.Print(text);              // Print text or renderable
console.Print(text, style);       // Print styled text
console.PrintLine(text, style);   // Print with newline
```

### `Color`

Represents a terminal color.

```csharp
// Factory methods
Color.FromRgb(255, 0, 0);         // RGB color
Color.FromAnsi(196);               // ANSI color number (0-255)
Color.FromTriplet(triplet);        // From ColorTriplet
Color.Parse("red");                // Parse color name/hex/rgb
Color.Default();                   // Default terminal color

// Properties
color.Name                         // Color name
color.Type                         // ColorType enum
color.Number                       // Color number (if applicable)
color.Triplet                      // RGB triplet (if applicable)
color.System                       // ColorSystem for this color
color.IsDefault                    // Is default color?
color.IsSystemDefined             // Is system-defined color?

// Methods
color.GetAnsiCodes(foreground)    // Get ANSI escape codes
```

### `Style`

Represents text styling attributes.

```csharp
// Constructor
var style = new Style(
    color: null,              // Foreground color
    bgcolor: null,            // Background color
    bold: null,               // Bold (null = not set)
    dim: null,                // Dim
    italic: null,             // Italic
    underline: null,          // Underline
    blink: null,              // Blink
    blink2: null,             // Rapid blink
    reverse: null,            // Reverse video
    conceal: null,            // Conceal
    strike: null,             // Strikethrough
    underline2: null,         // Double underline
    frame: null,              // Frame
    encircle: null,           // Encircle
    overline: null            // Overline
);

// Factory methods
Style.Null                         // Empty style (cached)
Style.FromColor(color, bgcolor)    // Style with only colors
Style.Parse("bold red on blue")    // Parse style string

// Properties
style.Color                        // Foreground color
style.BgColor                      // Background color
style.Bold                         // Bold attribute (bool?)
style.Italic                       // Italic attribute
// ... (all attributes)
style.IsNull                       // Has no styling?
style.TransparentBackground        // Has no background?

// Methods
style.Combine(other)               // Merge with another style
style.GetAnsiCode()                // Generate ANSI code
style1 + style2                    // Combine operator
```

### `Text`

Rich text with style spans.

```csharp
// Constructor
var text = new Text(
    text: "",                 // Initial text
    style: null,             // Base style
    justify: null,           // Justify method
    overflow: null,          // Overflow method
    noWrap: null,            // Disable wrapping
    end: "\n",               // End character
    tabSize: null            // Tab size
);

// Properties
text.Plain                   // Plain text without formatting
text.Length                  // Text length
text.CellLength             // Cell length (wide chars)
text.Spans                  // Read-only span list

// Methods
text.Append(string, style)  // Append styled text
text.Append(otherText)      // Append Text instance
text.Stylize(start, end, style)  // Add style span
text.ApplyStyle(style)      // Style entire text
text.Copy()                 // Create a copy
Text.Assemble(parts...)     // Assemble from parts
```

### `Segment`

Low-level rendering primitive.

```csharp
var segment = new Segment(
    text: "Hello",           // Text content
    style: null,             // Style
    control: null            // Control codes
);

segment.CellLength           // Cell length
segment.IsEmpty              // Is empty?
segment.IsControl            // Has control codes?

segment.SplitCells(cut)      // Split at position
Segment.Line()               // New line segment
Segment.ApplyStyle(segments, style)  // Apply style to segments
```

## Performance Considerations

- **Caching**: Color parsing and style parsing results are cached for performance
- **Cell Width Calculation**: Optimized for common ASCII text with fast-path
- **StringBuilder**: ANSI code generation uses StringBuilder for efficiency

## Compatibility

- **.NET Version**: Requires .NET 8.0 or later
- **Operating Systems**: Windows, macOS, Linux
- **Terminals**: Any terminal supporting ANSI escape codes

### Windows Support

RichSharp automatically enables virtual terminal processing on Windows 10+ for full color support.

## Testing

The library includes comprehensive unit tests with near 100% coverage:

```bash
cd RichSharp
dotnet test
```

## Examples

See the [examples](examples/) directory for complete examples:

- `BasicExample.cs` - Basic usage and styling
- `ColorExample.cs` - Color demonstrations
- `StyleExample.cs` - Text styling examples

## Comparison with Python Rich

RichSharp implements the core functionality of Python's Rich library:

| Feature | Python Rich | RichSharp | Status |
|---------|------------|-----------|---------|
| Colors (16/256/truecolor) | ✅ | ✅ | Complete |
| Text styling | ✅ | ✅ | Complete |
| Style parsing | ✅ | ✅ | Complete |
| Segments | ✅ | ✅ | Complete |
| Text with spans | ✅ | ✅ | Complete |
| Console | ✅ | ✅ | Core features |
| Unicode/CJK support | ✅ | ✅ | Complete |
| Tables | ✅ | ⏳ | Planned |
| Progress bars | ✅ | ⏳ | Planned |
| Panels | ✅ | ⏳ | Planned |
| Markdown | ✅ | ⏳ | Planned |
| Syntax highlighting | ✅ | ⏳ | Planned |

## Roadmap

Future enhancements planned:

- [ ] Table rendering
- [ ] Panel/Box components
- [ ] Progress bars
- [ ] Tree rendering
- [ ] Markdown rendering (via Markdig)
- [ ] Syntax highlighting
- [ ] HTML/SVG export
- [ ] Live displays
- [ ] Spinners

## Contributing

Contributions are welcome! Please feel free to submit pull requests.

## License

RichSharp is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.

## Acknowledgments

RichSharp is inspired by and based on the design of [Rich](https://github.com/Textualize/rich) by Will McGugan.

## Author

Converted to C# by the RichSharp team.

Original Python library by Will McGugan.
