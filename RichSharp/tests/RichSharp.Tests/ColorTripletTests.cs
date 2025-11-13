using FluentAssertions;
using Xunit;

namespace RichSharp.Tests;

public class ColorTripletTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        // Arrange & Act
        var triplet = new ColorTriplet(255, 128, 64);

        // Assert
        triplet.Red.Should().Be(255);
        triplet.Green.Should().Be(128);
        triplet.Blue.Should().Be(64);
    }

    [Fact]
    public void Hex_ShouldReturnCorrectFormat()
    {
        // Arrange
        var triplet = new ColorTriplet(255, 0, 127);

        // Act
        var hex = triplet.Hex;

        // Assert
        hex.Should().Be("#ff007f");
    }

    [Fact]
    public void Rgb_ShouldReturnCorrectFormat()
    {
        // Arrange
        var triplet = new ColorTriplet(100, 150, 200);

        // Act
        var rgb = triplet.Rgb;

        // Assert
        rgb.Should().Be("rgb(100,150,200)");
    }

    [Fact]
    public void Normalized_ShouldReturnValuesInRangeZeroToOne()
    {
        // Arrange
        var triplet = new ColorTriplet(255, 128, 0);

        // Act
        var (r, g, b) = triplet.Normalized;

        // Assert
        r.Should().BeApproximately(1.0f, 0.01f);
        g.Should().BeApproximately(0.502f, 0.01f);
        b.Should().Be(0.0f);
    }

    [Fact]
    public void FromHex_ShouldParseValidHexColor()
    {
        // Act
        var triplet = ColorTriplet.FromHex("#ff8040");

        // Assert
        triplet.Red.Should().Be(255);
        triplet.Green.Should().Be(128);
        triplet.Blue.Should().Be(64);
    }

    [Fact]
    public void FromHex_ShouldParseHexColorWithoutHashPrefix()
    {
        // Act
        var triplet = ColorTriplet.FromHex("ff8040");

        // Assert
        triplet.Red.Should().Be(255);
        triplet.Green.Should().Be(128);
        triplet.Blue.Should().Be(64);
    }

    [Fact]
    public void FromHex_ShouldThrowOnInvalidLength()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ColorTriplet.FromHex("#fff"));
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var triplet1 = new ColorTriplet(100, 150, 200);
        var triplet2 = new ColorTriplet(100, 150, 200);
        var triplet3 = new ColorTriplet(100, 150, 201);

        // Assert
        triplet1.Should().Be(triplet2);
        triplet1.Should().NotBe(triplet3);
    }
}
