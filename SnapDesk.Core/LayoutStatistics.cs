namespace SnapDesk.Core;

/// <summary>
/// Statistics about layouts
/// </summary>
public class LayoutStatistics
{
    /// <summary>
    /// Total number of layouts
    /// </summary>
    public int TotalLayouts { get; set; }

    /// <summary>
    /// Number of active layouts
    /// </summary>
    public int ActiveLayouts { get; set; }

    /// <summary>
    /// Number of layouts with hotkeys
    /// </summary>
    public int LayoutsWithHotkeys { get; set; }

    /// <summary>
    /// Average number of windows per layout
    /// </summary>
    public double AverageWindowsPerLayout { get; set; }

    /// <summary>
    /// Name of the most recently created layout
    /// </summary>
    public string? MostRecentLayout { get; set; }

    /// <summary>
    /// Name of the oldest layout
    /// </summary>
    public string? OldestLayout { get; set; }

    /// <summary>
    /// Total number of windows across all layouts
    /// </summary>
    public int TotalWindows { get; set; }

    /// <summary>
    /// Number of layouts created today
    /// </summary>
    public int LayoutsCreatedToday { get; set; }

    /// <summary>
    /// Number of layouts updated today
    /// </summary>
    public int LayoutsUpdatedToday { get; set; }
}

