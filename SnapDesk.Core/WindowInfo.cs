using System;
using System.ComponentModel.DataAnnotations;
using SnapDesk.Shared;

namespace SnapDesk.Core;

/// <summary>
/// Represents information about a specific window in a layout
/// </summary>
public class WindowInfo
{
    /// <summary>
    /// Unique identifier for the window (Windows handle)
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string WindowId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the process that owns this window (e.g., "Code", "chrome")
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string ProcessName { get; set; } = string.Empty;

    /// <summary>
    /// Title text displayed in the window's title bar
    /// </summary>
    [StringLength(200)]
    public string WindowTitle { get; set; } = string.Empty;

    /// <summary>
    /// Windows class name for the window (used for identification)
    /// </summary>
    [StringLength(100)]
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
    [Range(0, 10)] // Assuming max 10 monitors
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
        WindowId = GenerateWindowId();
    }

    /// <summary>
    /// Generates a unique identifier for the window
    /// </summary>
    /// <returns>Unique window ID</returns>
    private string GenerateWindowId()
    {
        return $"window_{ProcessName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..6]}";
    }

    /// <summary>
    /// Moves the window to a new position
    /// </summary>
    /// <param name="newPosition">New position for the window</param>
    public void MoveTo(Point newPosition)
    {
        Position = newPosition;
    }

    /// <summary>
    /// Resizes the window to new dimensions
    /// </summary>
    /// <param name="newSize">New size for the window</param>
    public void ResizeTo(Size newSize)
    {
        if (newSize.Width <= 0 || newSize.Height <= 0)
            throw new ArgumentException("Window dimensions must be positive", nameof(newSize));

        Size = newSize;
    }

    /// <summary>
    /// Changes the window state
    /// </summary>
    /// <param name="newState">New state for the window</param>
    public void ChangeState(WindowState newState)
    {
        State = newState;
    }

    /// <summary>
    /// Moves the window to a different monitor
    /// </summary>
    /// <param name="monitorIndex">Index of the target monitor</param>
    public void MoveToMonitor(int monitorIndex)
    {
        if (monitorIndex < 0)
            throw new ArgumentException("Monitor index must be non-negative", nameof(monitorIndex));

        Monitor = monitorIndex;
    }

    /// <summary>
    /// Brings the window to the front (highest Z-order)
    /// </summary>
    public void BringToFront()
    {
        ZOrder = int.MaxValue;
    }

    /// <summary>
    /// Sends the window to the back (lowest Z-order)
    /// </summary>
    public void SendToBack()
    {
        ZOrder = int.MinValue;
    }

    /// <summary>
    /// Shows the window
    /// </summary>
    public void Show()
    {
        IsVisible = true;
    }

    /// <summary>
    /// Hides the window
    /// </summary>
    public void Hide()
    {
        IsVisible = false;
    }

    /// <summary>
    /// Gets the center point of the window
    /// </summary>
    /// <returns>Center point of the window</returns>
    public Point GetCenter()
    {
        return new Point(Position.X + Size.Width / 2, Position.Y + Size.Height / 2);
    }

    /// <summary>
    /// Gets the area of the window
    /// </summary>
    /// <returns>Area in square pixels</returns>
    public int GetArea()
    {
        return Size.Width * Size.Height;
    }

    /// <summary>
    /// Checks if the window is maximized
    /// </summary>
    /// <returns>True if maximized, false otherwise</returns>
    public bool IsMaximized => State == WindowState.Maximized;

    /// <summary>
    /// Checks if the window is minimized
    /// </summary>
    /// <returns>True if minimized, false otherwise</returns>
    public bool IsMinimized => State == WindowState.Minimized;

    /// <summary>
    /// Checks if the window is in normal state
    /// </summary>
    /// <returns>True if normal, false otherwise</returns>
    public bool IsNormal => State == WindowState.Normal;

    /// <summary>
    /// Gets a summary of the window
    /// </summary>
    /// <returns>Formatted summary string</returns>
    public string GetSummary()
    {
        return $"{ProcessName}: {WindowTitle} at {Position} ({Size}) - {State}";
    }

    /// <summary>
    /// Validates the window configuration
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(WindowId) &&
               !string.IsNullOrWhiteSpace(ProcessName) &&
               Size.Width > 0 &&
               Size.Height > 0 &&
               Monitor >= 0;
    }
}
