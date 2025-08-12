using System;

namespace SnapDesk.Core;

/// <summary>
/// Represents information about a monitor/screen in the desktop layout
/// </summary>
public class MonitorInfo
{
    /// <summary>
    /// Index of the monitor (0 = primary, 1 = secondary, etc.)
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Whether this is the primary monitor
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Full bounds of the monitor (including taskbar area)
    /// </summary>
    public Rectangle Bounds { get; set; }

    /// <summary>
    /// Working area of the monitor (excluding taskbar)
    /// </summary>
    public Rectangle WorkingArea { get; set; }

    /// <summary>
    /// DPI (dots per inch) of the monitor
    /// </summary>
    public int Dpi { get; set; }

    /// <summary>
    /// Refresh rate of the monitor in Hz
    /// </summary>
    public int RefreshRate { get; set; }

    /// <summary>
    /// Name/identifier of the monitor
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Default constructor
    /// </summary>
    public MonitorInfo()
    {
        Bounds = new Rectangle(0, 0, 1920, 1080);
        WorkingArea = new Rectangle(0, 0, 1920, 1040); // Assuming 40px taskbar
        Dpi = 96; // Standard DPI
        RefreshRate = 60; // Standard refresh rate
    }

    /// <summary>
    /// Constructor with basic monitor information
    /// </summary>
    /// <param name="index">Monitor index</param>
    /// <param name="isPrimary">Whether this is the primary monitor</param>
    /// <param name="bounds">Monitor bounds</param>
    /// <param name="workingArea">Working area</param>
    public MonitorInfo(int index, bool isPrimary, Rectangle bounds, Rectangle workingArea) : this()
    {
        Index = index;
        IsPrimary = isPrimary;
        Bounds = bounds;
        WorkingArea = workingArea;
    }
}

/// <summary>
/// Represents a rectangle with position and size
/// </summary>
public struct Rectangle
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets the left edge of the rectangle
    /// </summary>
    public int Left => X;

    /// <summary>
    /// Gets the top edge of the rectangle
    /// </summary>
    public int Top => Y;

    /// <summary>
    /// Gets the right edge of the rectangle
    /// </summary>
    public int Right => X + Width;

    /// <summary>
    /// Gets the bottom edge of the rectangle
    /// </summary>
    public int Bottom => Y + Height;

    /// <summary>
    /// Gets the center point of the rectangle
    /// </summary>
    public Point Center => new Point(X + Width / 2, Y + Height / 2);

    /// <summary>
    /// Gets the area of the rectangle
    /// </summary>
    public int Area => Width * Height;

    /// <summary>
    /// Checks if a point is inside this rectangle
    /// </summary>
    /// <param name="point">Point to check</param>
    /// <returns>True if the point is inside the rectangle</returns>
    public bool Contains(Point point)
    {
        return point.X >= X && point.X < Right && point.Y >= Y && point.Y < Bottom;
    }

    /// <summary>
    /// Checks if this rectangle intersects with another rectangle
    /// </summary>
    /// <param name="other">Other rectangle to check</param>
    /// <returns>True if the rectangles intersect</returns>
    public bool IntersectsWith(Rectangle other)
    {
        return X < other.Right && Right > other.X && Y < other.Bottom && Bottom > other.Y;
    }

    public override string ToString()
    {
        return $"({X}, {Y}, {Width} x {Height})";
    }
}
