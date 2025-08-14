namespace SnapDesk.Shared;

/// <summary>
/// Window style information for extended window properties.
/// </summary>
public class WindowStyle
{
    /// <summary>
    /// Whether the window has a title bar.
    /// </summary>
    public bool HasTitleBar { get; set; }

    /// <summary>
    /// Whether the window has a system menu.
    /// </summary>
    public bool HasSystemMenu { get; set; }

    /// <summary>
    /// Whether the window can be resized.
    /// </summary>
    public bool CanResize { get; set; }

    /// <summary>
    /// Whether the window can be minimized.
    /// </summary>
    public bool CanMinimize { get; set; }

    /// <summary>
    /// Whether the window can be maximized.
    /// </summary>
    public bool CanMaximize { get; set; }

    /// <summary>
    /// Whether the window is a tool window (doesn't appear in taskbar).
    /// </summary>
    public bool IsToolWindow { get; set; }

    /// <summary>
    /// Whether the window is always on top.
    /// </summary>
    public bool IsAlwaysOnTop { get; set; }
}
