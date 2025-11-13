using FluentAssertions;
using Xunit;

namespace RichSharp.Tests;

public class StyleTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        // Arrange
        var color = Color.FromRgb(255, 0, 0);
        var bgcolor = Color.FromRgb(0, 0, 255);

        // Act
        var style = new Style(
            color: color,
            bgcolor: bgcolor,
            bold: true,
            italic: true
        );

        // Assert
        style.Color.Should().Be(color);
        style.BgColor.Should().Be(bgcolor);
        style.Bold.Should().BeTrue();
        style.Italic.Should().BeTrue();
        style.Underline.Should().BeNull();
    }

    [Fact]
    public void Null_ShouldReturnNullStyle()
    {
        // Act
        var style = Style.Null;

        // Assert
        style.IsNull.Should().BeTrue();
        style.Color.Should().BeNull();
        style.BgColor.Should().BeNull();
    }

    [Fact]
    public void FromColor_ShouldCreateStyleWithColors()
    {
        // Arrange
        var color = Color.FromRgb(255, 0, 0);
        var bgcolor = Color.FromRgb(0, 0, 255);

        // Act
        var style = Style.FromColor(color, bgcolor);

        // Assert
        style.Color.Should().Be(color);
        style.BgColor.Should().Be(bgcolor);
        style.Bold.Should().BeNull();
    }

    [Fact]
    public void Combine_ShouldMergeStyles()
    {
        // Arrange
        var style1 = new Style(bold: true, italic: false);
        var style2 = new Style(italic: true, underline: true);

        // Act
        var combined = style1.Combine(style2);

        // Assert
        combined.Bold.Should().BeTrue();  // From style1
        combined.Italic.Should().BeTrue(); // Overridden by style2
        combined.Underline.Should().BeTrue(); // From style2
    }

    [Fact]
    public void Combine_ShouldOverrideColors()
    {
        // Arrange
        var style1 = new Style(color: Color.FromRgb(255, 0, 0));
        var style2 = new Style(color: Color.FromRgb(0, 255, 0));

        // Act
        var combined = style1.Combine(style2);

        // Assert
        combined.Color.Should().Be(Color.FromRgb(0, 255, 0));
    }

    [Fact]
    public void Combine_WithNullStyle_ShouldReturnOriginal()
    {
        // Arrange
        var style = new Style(bold: true);

        // Act
        var combined = style.Combine(Style.Null);

        // Assert
        combined.Should().Be(style);
    }

    [Fact]
    public void GetAnsiCode_ShouldGenerateCorrectCodes()
    {
        // Arrange
        var style = new Style(bold: true, italic: true);

        // Act
        var code = style.GetAnsiCode();

        // Assert
        code.Should().Contain("1"); // Bold
        code.Should().Contain("3"); // Italic
    }

    [Fact]
    public void GetAnsiCode_ShouldIncludeColorCodes()
    {
        // Arrange
        var style = new Style(color: Color.FromRgb(255, 0, 0));

        // Act
        var code = style.GetAnsiCode();

        // Assert
        code.Should().Contain("38"); // Foreground color prefix
        code.Should().Contain("255");
        code.Should().Contain("0");
    }

    [Fact]
    public void GetAnsiCode_ForNullStyle_ShouldReturnEmpty()
    {
        // Arrange
        var style = Style.Null;

        // Act
        var code = style.GetAnsiCode();

        // Assert
        code.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ShouldParseBasicAttributes()
    {
        // Act
        var style = Style.Parse("bold italic");

        // Assert
        style.Bold.Should().BeTrue();
        style.Italic.Should().BeTrue();
    }

    [Fact]
    public void Parse_ShouldParseColors()
    {
        // Act
        var style = Style.Parse("red on blue");

        // Assert
        style.Color.Should().NotBeNull();
        style.Color!.Name.Should().Be("red");
        style.BgColor.Should().NotBeNull();
        style.BgColor!.Name.Should().Be("blue");
    }

    [Fact]
    public void Parse_ShouldHandleNone()
    {
        // Act
        var style = Style.Parse("none");

        // Assert
        style.IsNull.Should().BeTrue();
    }

    [Fact]
    public void Parse_ShouldHandleShorthandAttributes()
    {
        // Act
        var style = Style.Parse("b i u");

        // Assert
        style.Bold.Should().BeTrue();
        style.Italic.Should().BeTrue();
        style.Underline.Should().BeTrue();
    }

    [Fact]
    public void Parse_ShouldBeCached()
    {
        // Act
        var style1 = Style.Parse("bold red");
        var style2 = Style.Parse("bold red");

        // Assert
        ReferenceEquals(style1, style2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnStyleDefinition()
    {
        // Arrange
        var style = new Style(bold: true, color: Color.Parse("red"));

        // Act
        var str = style.ToString();

        // Assert
        str.Should().Contain("bold");
        str.Should().Contain("red");
    }

    [Fact]
    public void ToString_ForNullStyle_ShouldReturnNone()
    {
        // Act
        var str = Style.Null.ToString();

        // Assert
        str.Should().Be("none");
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var style1 = new Style(bold: true, color: Color.FromRgb(255, 0, 0));
        var style2 = new Style(bold: true, color: Color.FromRgb(255, 0, 0));
        var style3 = new Style(bold: false, color: Color.FromRgb(255, 0, 0));

        // Assert
        style1.Should().Be(style2);
        style1.Should().NotBe(style3);
    }

    [Fact]
    public void OperatorPlus_ShouldCombineStyles()
    {
        // Arrange
        var style1 = new Style(bold: true);
        var style2 = new Style(italic: true);

        // Act
        var combined = style1 + style2;

        // Assert
        combined.Bold.Should().BeTrue();
        combined.Italic.Should().BeTrue();
    }

    [Fact]
    public void BackgroundStyle_ShouldReturnStyleWithOnlyBackground()
    {
        // Arrange
        var style = new Style(
            color: Color.FromRgb(255, 0, 0),
            bgcolor: Color.FromRgb(0, 0, 255),
            bold: true
        );

        // Act
        var bgStyle = style.BackgroundStyle;

        // Assert
        bgStyle.BgColor.Should().Be(style.BgColor);
        bgStyle.Color.Should().BeNull();
        bgStyle.Bold.Should().BeNull();
    }

    [Fact]
    public void TransparentBackground_ShouldReturnTrueWhenNoBackground()
    {
        // Arrange
        var style = new Style(color: Color.FromRgb(255, 0, 0));

        // Assert
        style.TransparentBackground.Should().BeTrue();
    }

    [Fact]
    public void TransparentBackground_ShouldReturnFalseWhenHasBackground()
    {
        // Arrange
        var style = new Style(bgcolor: Color.FromRgb(0, 0, 255));

        // Assert
        style.TransparentBackground.Should().BeFalse();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(null, null)]
    public void BoldProperty_ShouldReflectConstructorValue(bool? bold, bool? expected)
    {
        // Arrange & Act
        var style = new Style(bold: bold);

        // Assert
        style.Bold.Should().Be(expected);
    }

    [Fact]
    public void AllAttributes_ShouldBeSettable()
    {
        // Act
        var style = new Style(
            bold: true,
            dim: true,
            italic: true,
            underline: true,
            blink: true,
            blink2: true,
            reverse: true,
            conceal: true,
            strike: true,
            underline2: true,
            frame: true,
            encircle: true,
            overline: true
        );

        // Assert
        style.Bold.Should().BeTrue();
        style.Dim.Should().BeTrue();
        style.Italic.Should().BeTrue();
        style.Underline.Should().BeTrue();
        style.Blink.Should().BeTrue();
        style.Blink2.Should().BeTrue();
        style.Reverse.Should().BeTrue();
        style.Conceal.Should().BeTrue();
        style.Strike.Should().BeTrue();
        style.Underline2.Should().BeTrue();
        style.Frame.Should().BeTrue();
        style.Encircle.Should().BeTrue();
        style.Overline.Should().BeTrue();
    }
}
