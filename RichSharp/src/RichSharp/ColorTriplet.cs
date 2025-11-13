namespace RichSharp;

/// <summary>
/// The red, green, and blue components of a color.
/// </summary>
/// <param name="Red">Red component in 0 to 255 range.</param>
/// <param name="Green">Green component in 0 to 255 range.</param>
/// <param name="Blue">Blue component in 0 to 255 range.</param>
public readonly record struct ColorTriplet(byte Red, byte Green, byte Blue)
{
    /// <summary>
    /// Get the color triplet in CSS hex format.
    /// </summary>
    public string Hex => $"#{Red:x2}{Green:x2}{Blue:x2}";

    /// <summary>
    /// Get the color in RGB format.
    /// </summary>
    public string Rgb => $"rgb({Red},{Green},{Blue})";

    /// <summary>
    /// Convert components into floats between 0 and 1.
    /// </summary>
    public (float R, float G, float B) Normalized => (Red / 255f, Green / 255f, Blue / 255f);

    /// <summary>
    /// Creates a ColorTriplet from hex string (e.g., "#FF0000" or "FF0000").
    /// </summary>
    public static ColorTriplet FromHex(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length != 6)
            throw new ArgumentException("Hex color must be 6 characters", nameof(hex));

        return new ColorTriplet(
            Convert.ToByte(hex.Substring(0, 2), 16),
            Convert.ToByte(hex.Substring(2, 2), 16),
            Convert.ToByte(hex.Substring(4, 2), 16)
        );
    }
}
