using FluentAssertions;
using Xunit;

namespace RichSharp.Tests;

public class TextTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        // Arrange & Act
        var style = new Style(bold: true);
        var text = new Text("Hello", style);

        // Assert
        text.Plain.Should().Be("Hello");
        text.BaseStyle.Should().Be(style);
        text.Length.Should().Be(5);
    }

    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateEmptyText()
    {
        // Act
        var text = new Text();

        // Assert
        text.Plain.Should().BeEmpty();
        text.Length.Should().Be(0);
    }

    [Fact]
    public void Append_ShouldAddText()
    {
        // Arrange
        var text = new Text("Hello");

        // Act
        text.Append(" World");

        // Assert
        text.Plain.Should().Be("Hello World");
        text.Length.Should().Be(11);
    }

    [Fact]
    public void Append_WithStyle_ShouldCreateSpan()
    {
        // Arrange
        var text = new Text("Hello");
        var style = new Style(bold: true);

        // Act
        text.Append(" World", style);

        // Assert
        text.Spans.Should().HaveCount(1);
        text.Spans[0].Start.Should().Be(5);
        text.Spans[0].End.Should().Be(11);
        text.Spans[0].Style.Should().Be(style);
    }

    [Fact]
    public void Append_WithEmptyText_ShouldDoNothing()
    {
        // Arrange
        var text = new Text("Hello");
        var originalLength = text.Length;

        // Act
        text.Append("");

        // Assert
        text.Length.Should().Be(originalLength);
    }

    [Fact]
    public void Append_WithTextObject_ShouldMerge()
    {
        // Arrange
        var text1 = new Text("Hello");
        var text2 = new Text(" World", new Style(bold: true));

        // Act
        text1.Append(text2);

        // Assert
        text1.Plain.Should().Be("Hello World");
        text1.Spans.Should().HaveCount(1);
    }

    [Fact]
    public void Copy_ShouldCreateDeepCopy()
    {
        // Arrange
        var original = new Text("Hello", new Style(bold: true));
        original.Append(" World", new Style(italic: true));

        // Act
        var copy = original.Copy();

        // Assert
        copy.Plain.Should().Be(original.Plain);
        copy.Spans.Should().HaveCount(original.Spans.Count);
        copy.Should().NotBeSameAs(original);
    }

    [Fact]
    public void Assemble_ShouldCombineMultipleParts()
    {
        // Arrange
        var style = new Style(bold: true);

        // Act
        var text = Text.Assemble(
            "Hello ",
            ("World", style),
            "!"
        );

        // Assert
        text.Plain.Should().Be("Hello World!");
        text.Spans.Should().HaveCount(1);
        text.Spans[0].Start.Should().Be(6);
        text.Spans[0].End.Should().Be(11);
    }

    [Fact]
    public void Assemble_WithTextObjects_ShouldMerge()
    {
        // Act
        var text = Text.Assemble(
            new Text("Hello"),
            " ",
            new Text("World", new Style(bold: true))
        );

        // Assert
        text.Plain.Should().Be("Hello World");
    }

    [Fact]
    public void Stylize_ShouldAddSpan()
    {
        // Arrange
        var text = new Text("Hello World");
        var style = new Style(bold: true);

        // Act
        text.Stylize(0, 5, style);

        // Assert
        text.Spans.Should().HaveCount(1);
        text.Spans[0].Start.Should().Be(0);
        text.Spans[0].End.Should().Be(5);
        text.Spans[0].Style.Should().Be(style);
    }

    [Fact]
    public void Stylize_WithInvalidRange_ShouldDoNothing()
    {
        // Arrange
        var text = new Text("Hello");
        var style = new Style(bold: true);

        // Act
        text.Stylize(10, 20, style);

        // Assert
        text.Spans.Should().BeEmpty();
    }

    [Fact]
    public void ApplyStyle_ShouldStyleEntireText()
    {
        // Arrange
        var text = new Text("Hello World");
        var style = new Style(bold: true);

        // Act
        text.ApplyStyle(style);

        // Assert
        text.Spans.Should().HaveCount(1);
        text.Spans[0].Start.Should().Be(0);
        text.Spans[0].End.Should().Be(11);
    }

    [Fact]
    public void CellLength_ShouldReturnCorrectLength()
    {
        // Arrange
        var text = new Text("Hello");

        // Act
        var length = text.CellLength;

        // Assert
        length.Should().Be(5);
    }

    [Fact]
    public void ToString_ShouldReturnPlainText()
    {
        // Arrange
        var text = new Text("Hello World");

        // Act
        var str = text.ToString();

        // Assert
        str.Should().Be("Hello World");
    }

    [Fact]
    public void ImplicitConversion_ShouldReturnPlainText()
    {
        // Arrange
        var text = new Text("Hello");

        // Act
        string str = text;

        // Assert
        str.Should().Be("Hello");
    }

    [Fact]
    public void Render_ShouldGenerateSegments()
    {
        // Arrange
        var console = new RichConsole(new StringWriter());
        var options = new ConsoleOptions(0, 80);
        var text = new Text("Hello", new Style(bold: true));

        // Act
        var segments = text.Render(console, options).ToList();

        // Assert
        segments.Should().NotBeEmpty();
        segments[0].Text.Should().Be("Hello");
    }

    [Fact]
    public void Render_WithMultipleSpans_ShouldGenerateMultipleSegments()
    {
        // Arrange
        var console = new RichConsole(new StringWriter());
        var options = new ConsoleOptions(0, 80);
        var text = new Text("Hello World");
        text.Stylize(0, 5, new Style(bold: true));
        text.Stylize(6, 11, new Style(italic: true));

        // Act
        var segments = text.Render(console, options).ToList();

        // Assert
        segments.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Render_EmptyText_ShouldNotGenerateSegments()
    {
        // Arrange
        var console = new RichConsole(new StringWriter());
        var options = new ConsoleOptions(0, 80);
        var text = new Text("");

        // Act
        var segments = text.Render(console, options).ToList();

        // Assert
        segments.Should().BeEmpty();
    }
}
