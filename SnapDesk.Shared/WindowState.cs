namespace SnapDesk.Shared;

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
