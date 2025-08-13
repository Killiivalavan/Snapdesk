using System;
using System.ComponentModel.DataAnnotations;

namespace SnapDesk.Core;

/// <summary>
/// Represents information about a monitor/screen in the desktop layout
/// </summary>
public class MonitorInfo
{
    /// <summary>
    /// Index of the monitor (0 = primary, 1 = secondary, etc.)
    /// </summary>
    [Range(0, 10)] // Assuming max 10 monitors
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
    [Range(72, 300)] // Reasonable DPI range
    public int Dpi { get; set; }

    /// <summary>
    /// Refresh rate of the monitor in Hz
    /// </summary>
    [Range(30, 360)] // Reasonable refresh rate range
    public int RefreshRate { get; set; }

    /// <summary>
    /// Name/identifier of the monitor
    /// </summary>
    [StringLength(100)]
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

    /// <summary>
    /// Sets this monitor as the primary monitor
    /// </summary>
    public void SetAsPrimary()
    {
        IsPrimary = true;
    }

    /// <summary>
    /// Removes primary status from this monitor
    /// </summary>
    public void RemovePrimaryStatus()
    {
        IsPrimary = false;
    }

    /// <summary>
    /// Updates the monitor bounds
    /// </summary>
    /// <param name="newBounds">New bounds for the monitor</param>
    public void UpdateBounds(Rectangle newBounds)
    {
        if (newBounds.Width <= 0 || newBounds.Height <= 0)
            throw new ArgumentException("Monitor dimensions must be positive", nameof(newBounds));

        Bounds = newBounds;
    }

    /// <summary>
    /// Updates the working area
    /// </summary>
    /// <param name="newWorkingArea">New working area</param>
    public void UpdateWorkingArea(Rectangle newWorkingArea)
    {
        if (newWorkingArea.Width <= 0 || newWorkingArea.Height <= 0)
            throw new ArgumentException("Working area dimensions must be positive", nameof(newWorkingArea));

        // Ensure working area is within bounds
        if (newWorkingArea.X < Bounds.X || newWorkingArea.Y < Bounds.Y ||
            newWorkingArea.Right > Bounds.Right || newWorkingArea.Bottom > Bounds.Bottom)
        {
            throw new ArgumentException("Working area must be within monitor bounds", nameof(newWorkingArea));
        }

        WorkingArea = newWorkingArea;
    }

    /// <summary>
    /// Gets the taskbar height (difference between bounds and working area)
    /// </summary>
    /// <returns>Taskbar height in pixels</returns>
    public int GetTaskbarHeight()
    {
        return Bounds.Height - WorkingArea.Height;
    }

    /// <summary>
    /// Gets the taskbar width (difference between bounds and working area)
    /// </summary>
    /// <returns>Taskbar width in pixels</returns>
    public int GetTaskbarWidth()
    {
        return Bounds.Width - WorkingArea.Width;
    }

    /// <summary>
    /// Checks if a point is within this monitor's bounds
    /// </summary>
    /// <param name="point">Point to check</param>
    /// <returns>True if point is within monitor bounds</returns>
    public bool ContainsPoint(Point point)
    {
        return Bounds.Contains(point);
    }

    /// <summary>
    /// Checks if a point is within the working area
    /// </summary>
    /// <param name="point">Point to check</param>
    /// <returns>True if point is within working area</returns>
    public bool ContainsPointInWorkingArea(Point point)
    {
        return WorkingArea.Contains(point);
    }

    /// <summary>
    /// Gets the center point of the monitor
    /// </summary>
    /// <returns>Center point of the monitor</returns>
    public Point GetCenter()
    {
        return Bounds.Center;
    }

    /// <summary>
    /// Gets the center point of the working area
    /// </summary>
    /// <returns>Center point of the working area</returns>
    public Point GetWorkingAreaCenter()
    {
        return WorkingArea.Center;
    }

    /// <summary>
    /// Gets the monitor's resolution
    /// </summary>
    /// <returns>Resolution as a Size</returns>
    public Size GetResolution()
    {
        return new Size(Bounds.Width, Bounds.Height);
    }

    /// <summary>
    /// Gets the working area resolution
    /// </summary>
    /// <returns>Working area resolution as a Size</returns>
    public Size GetWorkingAreaResolution()
    {
        return new Size(WorkingArea.Width, WorkingArea.Height);
    }

    /// <summary>
    /// Calculates the scaling factor based on DPI
    /// </summary>
    /// <returns>Scaling factor (1.0 = 96 DPI, 1.25 = 120 DPI, etc.)</returns>
    public double GetScalingFactor()
    {
        return (double)Dpi / 96.0;
    }

    /// <summary>
    /// Checks if this monitor has a high DPI
    /// </summary>
    /// <returns>True if DPI is above 120</returns>
    public bool IsHighDpi => Dpi > 120;

    /// <summary>
    /// Checks if this monitor has a high refresh rate
    /// </summary>
    /// <returns>True if refresh rate is above 60 Hz</returns>
    public bool IsHighRefreshRate => RefreshRate > 60;

    /// <summary>
    /// Gets the aspect ratio of the monitor
    /// </summary>
    /// <returns>Aspect ratio (width/height)</returns>
    public double AspectRatio => (double)Bounds.Width / Bounds.Height;

    /// <summary>
    /// Gets a summary of the monitor
    /// </summary>
    /// <returns>Formatted summary string</returns>
    public string GetSummary()
    {
        var primary = IsPrimary ? " (Primary)" : "";
        return $"Monitor {Index}{primary}: {Bounds.Width}x{Bounds.Height} @ {RefreshRate}Hz, {Dpi} DPI";
    }

    /// <summary>
    /// Validates the monitor configuration
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid()
    {
        return Index >= 0 &&
               Bounds.Width > 0 &&
               Bounds.Height > 0 &&
               WorkingArea.Width > 0 &&
               WorkingArea.Height > 0 &&
               Dpi >= 72 &&
               Dpi <= 300 &&
               RefreshRate >= 30 &&
               RefreshRate <= 360;
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

    /// <summary>
    /// Gets the intersection of two rectangles
    /// </summary>
    /// <param name="other">Other rectangle</param>
    /// <returns>Intersection rectangle, or empty if no intersection</returns>
    public Rectangle GetIntersection(Rectangle other)
    {
        if (!IntersectsWith(other))
            return new Rectangle(0, 0, 0, 0);

        var left = Math.Max(X, other.X);
        var top = Math.Max(Y, other.Y);
        var right = Math.Min(Right, other.Right);
        var bottom = Math.Min(Bottom, other.Bottom);

        return new Rectangle(left, top, right - left, bottom - top);
    }

    /// <summary>
    /// Gets the union of two rectangles
    /// </summary>
    /// <param name="other">Other rectangle</param>
    /// <returns>Union rectangle that contains both rectangles</returns>
    public Rectangle GetUnion(Rectangle other)
    {
        var left = Math.Min(X, other.X);
        var top = Math.Min(Y, other.Y);
        var right = Math.Max(Right, other.Right);
        var bottom = Math.Max(Bottom, other.Bottom);

        return new Rectangle(left, top, right - left, bottom - top);
    }

    /// <summary>
    /// Inflates the rectangle by the specified amount
    /// </summary>
    /// <param name="width">Amount to add to width</param>
    /// <param name="height">Amount to add to height</param>
    /// <returns>Inflated rectangle</returns>
    public Rectangle Inflate(int width, int height)
    {
        return new Rectangle(X - width, Y - height, Width + width * 2, Height + height * 2);
    }

    /// <summary>
    /// Deflates the rectangle by the specified amount
    /// </summary>
    /// <param name="width">Amount to subtract from width</param>
    /// <param name="height">Amount to subtract from height</param>
    /// <returns>Deflated rectangle</returns>
    public Rectangle Deflate(int width, int height)
    {
        return new Rectangle(X + width, Y + height, Width - width * 2, Height - height * 2);
    }

    /// <summary>
    /// Checks if this rectangle is empty (zero width or height)
    /// </summary>
    /// <returns>True if empty</returns>
    public bool IsEmpty => Width <= 0 || Height <= 0;

    /// <summary>
    /// Checks if this rectangle is a square
    /// </summary>
    /// <returns>True if square</returns>
    public bool IsSquare => Width == Height;

    public override string ToString()
    {
        return $"({X}, {Y}, {Width} x {Height})";
    }
}
