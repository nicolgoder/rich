namespace RichSharp;

/// <summary>
/// Options for rendering to the console.
/// </summary>
/// <param name="MinWidth">Minimum width of the renderable.</param>
/// <param name="MaxWidth">Maximum width of the renderable.</param>
/// <param name="Height">Maximum height of the renderable or null for no limit.</param>
/// <param name="Encoding">Text encoding (default UTF-8).</param>
/// <param name="Justify">Justification for text.</param>
/// <param name="Overflow">Overflow method.</param>
/// <param name="NoWrap">Disable text wrapping.</param>
/// <param name="Highlight">Enable highlighting.</param>
public record ConsoleOptions(
    int MinWidth,
    int MaxWidth,
    int? Height = null,
    string Encoding = "utf-8",
    JustifyMethod? Justify = null,
    OverflowMethod Overflow = OverflowMethod.Fold,
    bool NoWrap = false,
    bool Highlight = true
)
{
    /// <summary>
    /// Updates the options with a new width range.
    /// </summary>
    public ConsoleOptions Update(int? minWidth = null, int? maxWidth = null, int? height = null)
    {
        return this with
        {
            MinWidth = minWidth ?? MinWidth,
            MaxWidth = maxWidth ?? MaxWidth,
            Height = height ?? Height
        };
    }
}

/// <summary>
/// Text justification methods.
/// </summary>
public enum JustifyMethod
{
    Left,
    Center,
    Right,
    Full
}

/// <summary>
/// Methods for handling overflow text.
/// </summary>
public enum OverflowMethod
{
    Fold,
    Crop,
    Ellipsis,
    Ignore
}
