using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SnapDesk.Shared;

namespace SnapDesk.Core.Interfaces;

/// <summary>
/// Service interface for managing desktop window operations
/// </summary>
public interface IWindowService
{
    /// <summary>
    /// Gets all currently visible windows on the desktop
    /// </summary>
    /// <returns>Collection of window information</returns>
    Task<IEnumerable<WindowInfo>> GetCurrentWindowsAsync();

    /// <summary>
    /// Gets windows for a specific process
    /// </summary>
    /// <param name="processName">Name of the process</param>
    /// <returns>Collection of windows for the specified process</returns>
    Task<IEnumerable<WindowInfo>> GetWindowsByProcessAsync(string processName);

    /// <summary>
    /// Gets windows by title (partial match)
    /// </summary>
    /// <param name="title">Window title to search for</param>
    /// <returns>Collection of matching windows</returns>
    Task<IEnumerable<WindowInfo>> GetWindowsByTitleAsync(string title);

    /// <summary>
    /// Gets windows by class name
    /// </summary>
    /// <param name="className">Window class name</param>
    /// <returns>Collection of windows with the specified class</returns>
    Task<IEnumerable<WindowInfo>> GetWindowsByClassAsync(string className);

    /// <summary>
    /// Restores a window to its saved position and size
    /// </summary>
    /// <param name="window">Window information to restore</param>
    /// <returns>True if restoration was successful</returns>
    Task<bool> RestoreWindowAsync(WindowInfo window);

    /// <summary>
    /// Restores multiple windows to their saved positions and sizes
    /// </summary>
    /// <param name="windows">Collection of windows to restore</param>
    /// <returns>Number of successfully restored windows</returns>
    Task<int> RestoreWindowsAsync(IEnumerable<WindowInfo> windows);

    /// <summary>
    /// Saves the current state of a window
    /// </summary>
    /// <param name="window">Window to save</param>
    /// <returns>True if save was successful</returns>
    Task<bool> SaveWindowStateAsync(WindowInfo window);

    /// <summary>
    /// Moves a window to a new position
    /// </summary>
    /// <param name="windowId">Window identifier</param>
    /// <param name="position">New position</param>
    /// <returns>True if move was successful</returns>
    Task<bool> MoveWindowAsync(string windowId, Point position);

    /// <summary>
    /// Resizes a window to new dimensions
    /// </summary>
    /// <param name="windowId">Window identifier</param>
    /// <param name="size">New size</param>
    /// <returns>True if resize was successful</returns>
    Task<bool> ResizeWindowAsync(string windowId, Size size);

    /// <summary>
    /// Changes the state of a window (normal, minimized, maximized)
    /// </summary>
    /// <param name="windowId">Window identifier</param>
    /// <param name="state">New window state</param>
    /// <returns>True if state change was successful</returns>
    Task<bool> SetWindowStateAsync(string windowId, WindowState state);

    /// <summary>
    /// Shows a hidden window
    /// </summary>
    /// <param name="windowId">Window identifier</param>
    /// <returns>True if show was successful</returns>
    Task<bool> ShowWindowAsync(string windowId);

    /// <summary>
    /// Hides a visible window
    /// </summary>
    /// <param name="windowId">Window identifier</param>
    /// <returns>True if hide was successful</returns>
    Task<bool> HideWindowAsync(string windowId);

    /// <summary>
    /// Brings a window to the front (top of Z-order)
    /// </summary>
    /// <param name="windowId">Window identifier</param>
    /// <returns>True if bring to front was successful</returns>
    Task<bool> BringWindowToFrontAsync(string windowId);

    /// <summary>
    /// Sends a window to the back (bottom of Z-order)
    /// </summary>
    /// <param name="windowId">Window identifier</param>
    /// <returns>True if send to back was successful</returns>
    Task<bool> SendWindowToBackAsync(string windowId);

    /// <summary>
    /// Moves a window to a different monitor
    /// </summary>
    /// <param name="windowId">Window identifier</param>
    /// <param name="monitorIndex">Target monitor index</param>
    /// <returns>True if monitor move was successful</returns>
    Task<bool> MoveWindowToMonitorAsync(string windowId, int monitorIndex);

    /// <summary>
    /// Finds a window by its information
    /// </summary>
    /// <param name="windowInfo">Window information to search for</param>
    /// <returns>Window handle if found, IntPtr.Zero otherwise</returns>
    Task<IntPtr> FindWindowByInfoAsync(WindowInfo windowInfo);

    /// <summary>
    /// Gets detailed information about a specific window
    /// </summary>
    /// <param name="windowId">Window identifier</param>
    /// <returns>Detailed window information if found, null otherwise</returns>
    Task<WindowInfo?> GetWindowDetailsAsync(string windowId);

    /// <summary>
    /// Refreshes window information (updates positions, states, etc.)
    /// </summary>
    /// <param name="windowId">Window identifier</param>
    /// <returns>Updated window information</returns>
    Task<WindowInfo?> RefreshWindowInfoAsync(string windowId);

    /// <summary>
    /// Checks if a window is still valid/exists
    /// </summary>
    /// <param name="windowId">Window identifier</param>
    /// <returns>True if window is valid</returns>
    Task<bool> IsWindowValidAsync(string windowId);

    /// <summary>
    /// Gets the current monitor configuration
    /// </summary>
    /// <returns>Collection of monitor information</returns>
    Task<IEnumerable<MonitorInfo>> GetMonitorConfigurationAsync();

    /// <summary>
    /// Gets the primary monitor
    /// </summary>
    /// <returns>Primary monitor information</returns>
    Task<MonitorInfo?> GetPrimaryMonitorAsync();

    /// <summary>
    /// Gets monitor by index
    /// </summary>
    /// <param name="index">Monitor index</param>
    /// <returns>Monitor information if found, null otherwise</returns>
    Task<MonitorInfo?> GetMonitorByIndexAsync(int index);

    /// <summary>
    /// Captures the current desktop layout
    /// </summary>
    /// <returns>Collection of all visible windows</returns>
    Task<IEnumerable<WindowInfo>> CaptureDesktopLayoutAsync();

    /// <summary>
    /// Gets window statistics
    /// </summary>
    /// <returns>Statistics about current windows</returns>
    Task<WindowStatistics> GetWindowStatisticsAsync();
}

/// <summary>
/// Statistics about current windows
/// </summary>
public class WindowStatistics
{
    /// <summary>
    /// Total number of windows
    /// </summary>
    public int TotalWindows { get; set; }

    /// <summary>
    /// Number of visible windows
    /// </summary>
    public int VisibleWindows { get; set; }

    /// <summary>
    /// Number of minimized windows
    /// </summary>
    public int MinimizedWindows { get; set; }

    /// <summary>
    /// Number of maximized windows
    /// </summary>
    public int MaximizedWindows { get; set; }

    /// <summary>
    /// Number of normal windows
    /// </summary>
    public int NormalWindows { get; set; }

    /// <summary>
    /// Number of windows on primary monitor
    /// </summary>
    public int WindowsOnPrimaryMonitor { get; set; }

    /// <summary>
    /// Number of unique processes
    /// </summary>
    public int UniqueProcesses { get; set; }

    /// <summary>
    /// Most common process name
    /// </summary>
    public string? MostCommonProcess { get; set; }

    /// <summary>
    /// Average window size
    /// </summary>
    public Size AverageWindowSize { get; set; }

    /// <summary>
    /// Total desktop area covered by windows
    /// </summary>
    public int TotalWindowArea { get; set; }
}
