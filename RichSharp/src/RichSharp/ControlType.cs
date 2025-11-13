namespace RichSharp;

/// <summary>
/// Non-printable control codes which typically translate to ANSI codes.
/// </summary>
public enum ControlType
{
    Bell = 1,
    CarriageReturn = 2,
    Home = 3,
    Clear = 4,
    ShowCursor = 5,
    HideCursor = 6,
    EnableAltScreen = 7,
    DisableAltScreen = 8,
    CursorUp = 9,
    CursorDown = 10,
    CursorForward = 11,
    CursorBackward = 12,
    CursorMoveToColumn = 13,
    CursorMoveTo = 14,
    EraseInLine = 15,
    SetWindowTitle = 16
}

/// <summary>
/// Represents a control code that can be embedded in a segment.
/// </summary>
public abstract record ControlCode(ControlType Type);

/// <summary>
/// A simple control code with no parameters.
/// </summary>
public record SimpleControlCode(ControlType Type) : ControlCode(Type);

/// <summary>
/// A control code with a single integer parameter.
/// </summary>
public record IntControlCode(ControlType Type, int Value) : ControlCode(Type);

/// <summary>
/// A control code with a string parameter.
/// </summary>
public record StringControlCode(ControlType Type, string Value) : ControlCode(Type);

/// <summary>
/// A control code with two integer parameters.
/// </summary>
public record TwoIntControlCode(ControlType Type, int Value1, int Value2) : ControlCode(Type);
