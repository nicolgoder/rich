using FluentAssertions;
using Xunit;

namespace RichSharp.Tests;

public class ColorTests
{
    [Fact]
    public void FromAnsi_ShouldCreateStandardColor()
    {
        // Act
        var color = Color.FromAnsi(7);

        // Assert
        color.Type.Should().Be(ColorType.Standard);
        color.Number.Should().Be(7);
    }

    [Fact]
    public void FromAnsi_ShouldCreateEightBitColor()
    {
        // Act
        var color = Color.FromAnsi(200);

        // Assert
        color.Type.Should().Be(ColorType.EightBit);
        color.Number.Should().Be(200);
    }

    [Fact]
    public void FromAnsi_ShouldThrowOnInvalidNumber()
    {
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Color.FromAnsi(256));
        Assert.Throws<ArgumentOutOfRangeException>(() => Color.FromAnsi(-1));
    }

    [Fact]
    public void FromRgb_ShouldCreateTruecolorColor()
    {
        // Act
        var color = Color.FromRgb(255, 128, 64);

        // Assert
        color.Type.Should().Be(ColorType.Truecolor);
        color.Triplet.Should().NotBeNull();
        color.Triplet!.Value.Red.Should().Be(255);
        color.Triplet.Value.Green.Should().Be(128);
        color.Triplet.Value.Blue.Should().Be(64);
    }

    [Fact]
    public void FromTriplet_ShouldCreateTruecolorColor()
    {
        // Arrange
        var triplet = new ColorTriplet(100, 150, 200);

        // Act
        var color = Color.FromTriplet(triplet);

        // Assert
        color.Type.Should().Be(ColorType.Truecolor);
        color.Triplet.Should().Be(triplet);
    }

    [Fact]
    public void Default_ShouldCreateDefaultColor()
    {
        // Act
        var color = Color.Default();

        // Assert
        color.Type.Should().Be(ColorType.Default);
        color.IsDefault.Should().BeTrue();
    }

    [Theory]
    [InlineData("red", ColorType.Standard, 1)]
    [InlineData("blue", ColorType.Standard, 4)]
    [InlineData("bright_white", ColorType.Standard, 15)]
    [InlineData("dark_blue", ColorType.EightBit, 18)]
    [InlineData("gold1", ColorType.EightBit, 220)]
    public void Parse_ShouldParseNamedColors(string name, ColorType expectedType, int expectedNumber)
    {
        // Act
        var color = Color.Parse(name);

        // Assert
        color.Type.Should().Be(expectedType);
        color.Number.Should().Be(expectedNumber);
    }

    [Fact]
    public void Parse_ShouldParseHexColors()
    {
        // Act
        var color = Color.Parse("#ff8040");

        // Assert
        color.Type.Should().Be(ColorType.Truecolor);
        color.Triplet.Should().NotBeNull();
        color.Triplet!.Value.Red.Should().Be(255);
        color.Triplet.Value.Green.Should().Be(128);
        color.Triplet.Value.Blue.Should().Be(64);
    }

    [Fact]
    public void Parse_ShouldParseColorFunction()
    {
        // Act
        var color = Color.Parse("color(42)");

        // Assert
        color.Type.Should().Be(ColorType.EightBit);
        color.Number.Should().Be(42);
    }

    [Fact]
    public void Parse_ShouldParseRgbFunction()
    {
        // Act
        var color = Color.Parse("rgb(100,150,200)");

        // Assert
        color.Type.Should().Be(ColorType.Truecolor);
        color.Triplet.Should().NotBeNull();
        color.Triplet!.Value.Red.Should().Be(100);
        color.Triplet.Value.Green.Should().Be(150);
        color.Triplet.Value.Blue.Should().Be(200);
    }

    [Fact]
    public void Parse_ShouldThrowOnInvalidColor()
    {
        // Assert
        Assert.Throws<ColorParseException>(() => Color.Parse("notacolor"));
    }

    [Fact]
    public void Parse_ShouldBeCaseInsensitive()
    {
        // Act
        var color1 = Color.Parse("RED");
        var color2 = Color.Parse("red");

        // Assert
        color1.Number.Should().Be(color2.Number);
    }

    [Fact]
    public void Parse_ShouldCacheResults()
    {
        // Act
        var color1 = Color.Parse("red");
        var color2 = Color.Parse("red");

        // Assert - same reference means cached
        ReferenceEquals(color1, color2).Should().BeTrue();
    }

    [Fact]
    public void GetAnsiCodes_ShouldReturnDefaultCodes()
    {
        // Arrange
        var color = Color.Default();

        // Act
        var codes = color.GetAnsiCodes(foreground: true);

        // Assert
        codes.Should().Equal("39");
    }

    [Fact]
    public void GetAnsiCodes_ShouldReturnStandardForegroundCodes()
    {
        // Arrange
        var color = Color.FromAnsi(3); // yellow

        // Act
        var codes = color.GetAnsiCodes(foreground: true);

        // Assert
        codes.Should().Equal("33");
    }

    [Fact]
    public void GetAnsiCodes_ShouldReturnStandardBackgroundCodes()
    {
        // Arrange
        var color = Color.FromAnsi(3); // yellow

        // Act
        var codes = color.GetAnsiCodes(foreground: false);

        // Assert
        codes.Should().Equal("43");
    }

    [Fact]
    public void GetAnsiCodes_ShouldReturnEightBitCodes()
    {
        // Arrange
        var color = Color.FromAnsi(200);

        // Act
        var codes = color.GetAnsiCodes(foreground: true);

        // Assert
        codes.Should().Equal("38", "5", "200");
    }

    [Fact]
    public void GetAnsiCodes_ShouldReturnTruecolorCodes()
    {
        // Arrange
        var color = Color.FromRgb(255, 128, 64);

        // Act
        var codes = color.GetAnsiCodes(foreground: true);

        // Assert
        codes.Should().Equal("38", "2", "255", "128", "64");
    }

    [Fact]
    public void IsSystemDefined_ShouldReturnTrueForStandardColors()
    {
        // Arrange
        var color = Color.FromAnsi(7);

        // Assert
        color.IsSystemDefined.Should().BeTrue();
    }

    [Fact]
    public void IsSystemDefined_ShouldReturnFalseForTruecolor()
    {
        // Arrange
        var color = Color.FromRgb(255, 0, 0);

        // Assert
        color.IsSystemDefined.Should().BeFalse();
    }

    [Fact]
    public void System_ShouldReturnCorrectColorSystem()
    {
        // Arrange
        var standard = Color.FromAnsi(7);
        var eightBit = Color.FromAnsi(200);
        var truecolor = Color.FromRgb(255, 0, 0);

        // Assert
        standard.System.Should().Be(ColorSystem.Standard);
        eightBit.System.Should().Be(ColorSystem.EightBit);
        truecolor.System.Should().Be(ColorSystem.Truecolor);
    }
}
