namespace RichSharp;

/// <summary>
/// Represents an object that can be rendered to the console.
/// </summary>
public interface IRenderable
{
    /// <summary>
    /// Renders the object to the console.
    /// </summary>
    /// <param name="console">The console to render to.</param>
    /// <param name="options">The console options.</param>
    /// <returns>A collection of segments to render.</returns>
    IEnumerable<Segment> Render(RichConsole console, ConsoleOptions options);
}

/// <summary>
/// Utility methods for checking if an object is renderable.
/// </summary>
public static class RenderableExtensions
{
    /// <summary>
    /// Checks if an object may be rendered by Rich.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <returns>True if the object is renderable, false otherwise.</returns>
    public static bool IsRenderable(object? obj)
    {
        return obj is string or IRenderable;
    }

    /// <summary>
    /// Casts an object to a renderable type.
    /// </summary>
    /// <param name="renderable">The object to cast.</param>
    /// <returns>The renderable object.</returns>
    public static object CastToRenderable(object renderable)
    {
        // In C#, we use explicit interfaces rather than duck typing
        // This is a simplified version - Python's __rich__ protocol
        // allows more dynamic behavior
        return renderable;
    }
}
