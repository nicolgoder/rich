using FluentAssertions;
using Xunit;

namespace RichSharp.Tests;

public class RichConsoleTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Arrange
        var output = new StringWriter();

        // Act
        var console = new RichConsole(output, width: 80, height: 25);

        // Assert
        console.Width.Should().Be(80);
        console.Height.Should().Be(25);
        console.TabSize.Should().Be(4);
    }

    [Fact]
    public void Constructor_WithoutParameters_ShouldUseDefaults()
    {
        // Act
        var console = new RichConsole(new StringWriter());

        // Assert
        console.Width.Should().BeGreaterThan(0);
        console.Height.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Print_WithText_ShouldWriteToOutput()
    {
        // Arrange
        var output = new StringWriter();
        var console = new RichConsole(output);

        // Act
        console.Print("Hello World");

        // Assert
        output.ToString().Should().Contain("Hello World");
    }

    [Fact]
    public void Print_WithStyle_ShouldIncludeAnsiCodes()
    {
        // Arrange
        var output = new StringWriter();
        var console = new RichConsole(output);
        var style = new Style(bold: true);

        // Act
        console.Print("Bold Text", style);

        // Assert
        var result = output.ToString();
        result.Should().Contain("\x1b["); // ANSI escape sequence
        result.Should().Contain("Bold Text");
    }

    [Fact]
    public void Print_WithRenderable_ShouldRenderCorrectly()
    {
        // Arrange
        var output = new StringWriter();
        var console = new RichConsole(output);
        var text = new Text("Styled", new Style(bold: true));

        // Act
        console.Print(text);

        // Assert
        output.ToString().Should().Contain("Styled");
    }

    [Fact]
    public void PrintLine_ShouldAddNewline()
    {
        // Arrange
        var output = new StringWriter();
        var console = new RichConsole(output);

        // Act
        console.PrintLine("Test");

        // Assert
        output.ToString().Should().EndWith("\n");
    }

    [Fact]
    public void RenderToPlainText_ShouldStripFormatting()
    {
        // Arrange
        var console = new RichConsole(new StringWriter());
        var text = new Text("Hello", new Style(bold: true, color: Color.FromRgb(255, 0, 0)));

        // Act
        var plain = console.RenderToPlainText(text);

        // Assert
        plain.Should().Be("Hello");
        plain.Should().NotContain("\x1b[");
    }

    [Fact]
    public void GetOptions_ShouldCreateConsoleOptions()
    {
        // Arrange
        var console = new RichConsole(new StringWriter(), width: 100);

        // Act
        var options = console.GetOptions();

        // Assert
        options.MaxWidth.Should().Be(100);
        options.MinWidth.Should().Be(0);
    }

    [Fact]
    public void GetOptions_WithCustomMaxWidth_ShouldUseProvided()
    {
        // Arrange
        var console = new RichConsole(new StringWriter(), width: 100);

        // Act
        var options = console.GetOptions(maxWidth: 50);

        // Assert
        options.MaxWidth.Should().Be(50);
    }

    [Fact]
    public void ColorSystem_ShouldBeDetected()
    {
        // Arrange
        var console = new RichConsole(new StringWriter());

        // Assert
        console.ColorSystem.Should().BeOneOf(
            ColorSystem.Standard,
            ColorSystem.EightBit,
            ColorSystem.Truecolor,
            ColorSystem.Windows
        );
    }

    [Fact]
    public void Print_WithMultipleStyles_ShouldResetBetweenSegments()
    {
        // Arrange
        var output = new StringWriter();
        var console = new RichConsole(output);
        var text = new Text();
        text.Append("Normal ");
        text.Append("Bold", new Style(bold: true));
        text.Append(" Normal");

        // Act
        console.Print(text);

        // Assert
        var result = output.ToString();
        result.Should().Contain("\x1b[0m"); // Reset code
    }
}
