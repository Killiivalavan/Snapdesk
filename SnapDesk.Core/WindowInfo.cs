using System;

namespace SnapDesk.Core;

/// <summary>
/// Represents information about a specific window in a layout
/// </summary>
public class WindowInfo
{
    /// <summary>
    /// Unique identifier for the window (Windows handle)
    /// </summary>
    public string WindowId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the process that owns this window (e.g., "Code", "chrome")
    /// </summary>
    public string ProcessName { get; set; } = string.Empty;

    /// <summary>
    /// Title text displayed in the window's title bar
    /// </summary>
    public string WindowTitle { get; set; } = string.Empty;

    /// <summary>
    /// Windows class name for the window (used for identification)
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// X and Y coordinates of the window's top-left corner
    /// </summary>
    public Point Position { get; set; }

    /// <summary>
    /// Width and height of the window
    /// </summary>
    public Size Size { get; set; }

    /// <summary>
    /// Current state of the window (normal, maximized, minimized)
    /// </summary>
    public WindowState State { get; set; }

    /// <summary>
    /// Which monitor this window is displayed on (0 = primary, 1 = secondary, etc.)
    /// </summary>
    public int Monitor { get; set; }

    /// <summary>
    /// Z-order of the window (stacking order - higher numbers are on top)
    /// </summary>
    public int ZOrder { get; set; }

    /// <summary>
    /// Whether the window is currently visible
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public WindowInfo()
    {
        Position = new Point(0, 0);
        Size = new Size(800, 600);
        State = WindowState.Normal;
        Monitor = 0;
        ZOrder = 0;
        IsVisible = true;
    }

    /// <summary>
    /// Constructor with basic window information
    /// </summary>
    /// <param name="processName">Name of the process</param>
    /// <param name="windowTitle">Window title</param>
    /// <param name="position">Window position</param>
    /// <param name="size">Window size</param>
    public WindowInfo(string processName, string windowTitle, Point position, Size size) : this()
    {
        ProcessName = processName;
        WindowTitle = windowTitle;
        Position = position;
        Size = size;
    }
}

/// <summary>
/// Represents a 2D point with X and Y coordinates
/// </summary>
public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}

/// <summary>
/// Represents dimensions with width and height
/// </summary>
public struct Size
{
    public int Width { get; set; }
    public int Height { get; set; }

    public Size(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public override string ToString()
    {
        return $"{Width} x {Height}";
    }
}

/// <summary>
/// Possible states of a window
/// </summary>
public enum WindowState
{
    /// <summary>
    /// Window is in normal state (not maximized or minimized)
    /// </summary>
    Normal = 0,

    /// <summary>
    /// Window is maximized (fills the entire screen)
    /// </summary>
    Maximized = 1,

    /// <summary>
    /// Window is minimized (hidden in taskbar)
    /// </summary>
    Minimized = 2
}
