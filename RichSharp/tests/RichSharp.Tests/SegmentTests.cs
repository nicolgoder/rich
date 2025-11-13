using FluentAssertions;
using Xunit;
using System.Collections.Immutable;

namespace RichSharp.Tests;

public class SegmentTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        // Arrange
        var style = new Style(bold: true);

        // Act
        var segment = new Segment("Hello", style);

        // Assert
        segment.Text.Should().Be("Hello");
        segment.Style.Should().Be(style);
        segment.Control.Should().BeNull();
    }

    [Fact]
    public void CellLength_ShouldReturnCorrectLength()
    {
        // Arrange
        var segment = new Segment("Hello");

        // Act
        var length = segment.CellLength;

        // Assert
        length.Should().Be(5);
    }

    [Fact]
    public void CellLength_WithControlCodes_ShouldReturnZero()
    {
        // Arrange
        var control = ImmutableArray.Create<ControlCode>(new SimpleControlCode(ControlType.Bell));
        var segment = new Segment("Hello", Control: control);

        // Act
        var length = segment.CellLength;

        // Assert
        length.Should().Be(0);
    }

    [Fact]
    public void IsEmpty_ShouldReturnTrueForEmptyText()
    {
        // Arrange
        var segment = new Segment("");

        // Assert
        segment.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_ShouldReturnFalseForNonEmptyText()
    {
        // Arrange
        var segment = new Segment("Hi");

        // Assert
        segment.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void IsControl_ShouldReturnTrueWhenControlCodesPresent()
    {
        // Arrange
        var control = ImmutableArray.Create<ControlCode>(new SimpleControlCode(ControlType.Bell));
        var segment = new Segment("", Control: control);

        // Assert
        segment.IsControl.Should().BeTrue();
    }

    [Fact]
    public void IsControl_ShouldReturnFalseWhenNoControlCodes()
    {
        // Arrange
        var segment = new Segment("Hello");

        // Assert
        segment.IsControl.Should().BeFalse();
    }

    [Fact]
    public void Line_ShouldReturnNewlineSegment()
    {
        // Act
        var segment = Segment.Line();

        // Assert
        segment.Text.Should().Be("\n");
    }

    [Fact]
    public void SplitCells_ShouldSplitAtCorrectPosition()
    {
        // Arrange
        var segment = new Segment("Hello");

        // Act
        var (left, right) = segment.SplitCells(3);

        // Assert
        left.Text.Should().Be("Hel");
        right.Text.Should().Be("lo");
    }

    [Fact]
    public void SplitCells_BeyondLength_ShouldReturnOriginalAndEmpty()
    {
        // Arrange
        var segment = new Segment("Hi");

        // Act
        var (left, right) = segment.SplitCells(10);

        // Assert
        left.Should().Be(segment);
        right.Text.Should().BeEmpty();
    }

    [Fact]
    public void SplitCells_ShouldPreserveStyle()
    {
        // Arrange
        var style = new Style(bold: true);
        var segment = new Segment("Hello", style);

        // Act
        var (left, right) = segment.SplitCells(3);

        // Assert
        left.Style.Should().Be(style);
        right.Style.Should().Be(style);
    }

    [Fact]
    public void SplitCells_WithNegativeCut_ShouldThrow()
    {
        // Arrange
        var segment = new Segment("Hello");

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => segment.SplitCells(-1));
    }

    [Fact]
    public void ApplyStyle_ShouldApplyStyleToSegments()
    {
        // Arrange
        var segments = new List<Segment>
        {
            new Segment("Hello"),
            new Segment("World")
        };
        var style = new Style(bold: true);

        // Act
        var styled = Segment.ApplyStyle(segments, style).ToList();

        // Assert
        styled.Should().HaveCount(2);
        styled[0].Style.Should().NotBeNull();
        styled[0].Style!.Bold.Should().BeTrue();
    }

    [Fact]
    public void ApplyStyle_ShouldCombineWithExistingStyle()
    {
        // Arrange
        var existingStyle = new Style(italic: true);
        var segments = new List<Segment> { new Segment("Hello", existingStyle) };
        var newStyle = new Style(bold: true);

        // Act
        var styled = Segment.ApplyStyle(segments, newStyle).ToList();

        // Assert
        styled[0].Style.Should().NotBeNull();
        styled[0].Style!.Bold.Should().BeTrue();
        styled[0].Style!.Italic.Should().BeTrue();
    }

    [Fact]
    public void FilterControl_ShouldFilterByControlStatus()
    {
        // Arrange
        var control = ImmutableArray.Create<ControlCode>(new SimpleControlCode(ControlType.Bell));
        var segments = new List<Segment>
        {
            new Segment("Text"),
            new Segment("Control", Control: control)
        };

        // Act
        var controlSegments = Segment.FilterControl(segments, true).ToList();
        var textSegments = Segment.FilterControl(segments, false).ToList();

        // Assert
        controlSegments.Should().HaveCount(1);
        controlSegments[0].Text.Should().Be("Control");
        textSegments.Should().HaveCount(1);
        textSegments[0].Text.Should().Be("Text");
    }

    [Fact]
    public void SplitLines_ShouldSplitOnNewlines()
    {
        // Arrange
        var segments = new List<Segment>
        {
            new Segment("Line1\nLine2\nLine3")
        };

        // Act
        var lines = Segment.SplitLines(segments).ToList();

        // Assert
        lines.Should().HaveCount(3);
        string.Join("", lines[0].Select(s => s.Text)).Should().Be("Line1");
        string.Join("", lines[1].Select(s => s.Text)).Should().Be("Line2");
        string.Join("", lines[2].Select(s => s.Text)).Should().Be("Line3");
    }
}
