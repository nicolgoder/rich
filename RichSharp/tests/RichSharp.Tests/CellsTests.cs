using FluentAssertions;
using Xunit;

namespace RichSharp.Tests;

public class CellsTests
{
    [Theory]
    [InlineData("hello", 5)]
    [InlineData("", 0)]
    [InlineData("a", 1)]
    [InlineData("abc123", 6)]
    public void CellLength_ShouldReturnCorrectLengthForAscii(string text, int expected)
    {
        // Act
        var length = Cells.CellLength(text);

        // Assert
        length.Should().Be(expected);
    }

    [Theory]
    [InlineData("你好", 4)]  // Two Chinese characters = 4 cells
    [InlineData("こんにちは", 10)]  // Five Japanese characters = 10 cells
    public void CellLength_ShouldHandleWideCharacters(string text, int expected)
    {
        // Act
        var length = Cells.CellLength(text);

        // Assert
        length.Should().Be(expected);
    }

    [Fact]
    public void CellLength_ShouldReturnZeroForNull()
    {
        // Act
        var length = Cells.CellLength(null);

        // Assert
        length.Should().Be(0);
    }

    [Fact]
    public void IsSingleCellWidths_ShouldReturnTrueForAscii()
    {
        // Act
        var result = Cells.IsSingleCellWidths("Hello World!");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSingleCellWidths_ShouldReturnFalseForWideChars()
    {
        // Act
        var result = Cells.IsSingleCellWidths("你好");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData('a', 1)]
    [InlineData(' ', 1)]
    [InlineData('A', 1)]
    [InlineData('\n', 0)]  // Control character
    [InlineData('\t', 0)]  // Control character
    public void GetCharacterCellSize_ShouldReturnCorrectSize(char ch, int expected)
    {
        // Arrange
        var rune = new System.Text.Rune(ch);

        // Act
        var size = Cells.GetCharacterCellSize(rune);

        // Assert
        size.Should().Be(expected);
    }

    [Fact]
    public void SetCellSize_ShouldPadShortText()
    {
        // Act
        var result = Cells.SetCellSize("hi", 5);

        // Assert
        result.Should().Be("hi   ");
        Cells.CellLength(result).Should().Be(5);
    }

    [Fact]
    public void SetCellSize_ShouldTruncateLongText()
    {
        // Act
        var result = Cells.SetCellSize("hello world", 5);

        // Assert
        result.Should().Be("hello");
        Cells.CellLength(result).Should().Be(5);
    }

    [Fact]
    public void SetCellSize_ShouldReturnSameLengthText()
    {
        // Act
        var result = Cells.SetCellSize("hello", 5);

        // Assert
        result.Should().Be("hello");
    }
}
