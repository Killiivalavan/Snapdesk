using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SnapDesk.Core;
using SnapDesk.Core.Interfaces;
using SnapDesk.Platform.Interfaces;
using SnapDesk.Shared;

namespace SnapDesk.Core.Services;

/// <summary>
/// Service for managing desktop window operations
/// </summary>
public class WindowService : IWindowService
{
    private readonly IWindowApi _windowApi;
    private readonly ILogger<WindowService> _logger;
	private readonly Dictionary<IntPtr, int> _monitorHandleToIndex = new();
	private int _nextMonitorIndex = 0;

    public WindowService(
        IWindowApi windowApi,
        ILogger<WindowService> logger)
    {
        _windowApi = windowApi ?? throw new ArgumentNullException(nameof(windowApi));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all currently visible windows on the desktop
    /// </summary>
    /// <returns>Collection of window information</returns>
    public async Task<IEnumerable<WindowInfo>> GetCurrentWindowsAsync()
    {
        try
        {
            _logger.LogDebug("Getting current windows from platform layer");
            
            // Get all window handles from the platform layer
            var windowHandles = _windowApi.GetAllWindows();
            var windows = new List<WindowInfo>();

            foreach (var handle in windowHandles)
            {
                try
                {
                    // Check if window is valid and visible
                    if (!_windowApi.IsWindow(handle) || !_windowApi.IsWindowVisible(handle))
                        continue;

                    // Get window information
                    string title = string.Empty, className = string.Empty, titleError = string.Empty, classError = string.Empty, rectError = string.Empty;
                    Point position = new Point();
                    Size size = new Size();
                    
                    if (_windowApi.TryGetWindowText(handle, out title, out titleError) &&
                        _windowApi.TryGetClassName(handle, out className, out classError) &&
                        _windowApi.TryGetWindowRect(handle, out position, out size, out rectError))
                    {
                        // Get process information if available
                        var processId = 0;
                        var processName = string.Empty;
                        
                        if (_windowApi.TryGetWindowProcessId(handle, out var pid, out var pidError))
                        {
                            processId = pid;
                            try
                            {
                                // Resolve real process name from PID for long-term accuracy
                                processName = Process.GetProcessById(pid).ProcessName;
                            }
                            catch
                            {
                                // Fallback to placeholder if resolution fails
                            processName = $"Process_{pid}";
                            }
                        }

					// Determine window state
                        var state = WindowState.Normal;
                        try
                        {
                            if (_windowApi.IsWindowMinimized(handle)) state = WindowState.Minimized;
                            else if (_windowApi.IsWindowMaximized(handle)) state = WindowState.Maximized;
                        }
                        catch { /* best-effort */ }

					// Determine monitor index using platform monitor handle mapping
					int monitorIndex = GetMonitorIndexForWindow(handle);

                        // Create WindowInfo object
                        var windowInfo = new WindowInfo
                        {
                            WindowId = handle.ToString("X"), // Use handle as ID for now
                            ProcessName = processName,
                            WindowTitle = title ?? "Unknown Title",
                            ClassName = className ?? "Unknown Class",
                            Position = position,
                            Size = size,
                            State = state,
						Monitor = monitorIndex,
                            ZOrder = 0, // Default Z-order, will be enhanced later
                            IsVisible = true
                        };

                        windows.Add(windowInfo);
                        _logger.LogTrace("Added window: {Title} ({Class}) at ({X}, {Y}) {Width}x{Height}", 
                            title, className, position.X, position.Y, size.Width, size.Height);
                    }
                    else
                    {
                        _logger.LogDebug("Failed to get window info for handle {Handle}: Title={TitleError}, Class={ClassError}, Rect={RectError}", 
                            handle, titleError, classError, rectError);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing window handle {Handle}", handle);
                }
            }

            _logger.LogInformation("Retrieved {Count} visible windows", windows.Count);
            return windows;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current windows");
            throw;
        }
    }

    // TODO: Implement remaining methods incrementally
    // The following methods will be implemented one by one in subsequent steps
    
    public Task<IEnumerable<WindowInfo>> GetWindowsByProcessAsync(string processName) => throw new NotImplementedException();
    public Task<IEnumerable<WindowInfo>> GetWindowsByTitleAsync(string title) => throw new NotImplementedException();
    public Task<IEnumerable<WindowInfo>> GetWindowsByClassAsync(string className) => throw new NotImplementedException();
    public async Task<bool> RestoreWindowAsync(WindowInfo window)
    {
        if (window == null) return false;
        var handle = await FindWindowByInfoAsync(window);
        if (handle == IntPtr.Zero) return false;

        // Move/resize first, then apply state and visibility
        var setBounds = _windowApi.TrySetWindowBounds(handle, window.Position.X, window.Position.Y, window.Size.Width, window.Size.Height, out var boundsErr);
        if (!setBounds) _logger.LogWarning("Failed to set bounds during restore: {Error}", boundsErr);

        var stateOk = await SetWindowStateAsync(handle.ToString("X"), window.State);
        if (!stateOk) _logger.LogWarning("Failed to set state during restore for {Id}", window.WindowId);

        if (window.IsVisible)
        {
            _ = _windowApi.TryShowWindow(handle, out _);
        }
        else
        {
            _ = _windowApi.TryHideWindow(handle, out _);
        }

        return true;
    }

    public async Task<int> RestoreWindowsAsync(IEnumerable<WindowInfo> windows)
    {
        if (windows == null) return 0;
        int restored = 0;
        foreach (var w in windows)
        {
            if (await RestoreWindowAsync(w)) restored++;
        }
        return restored;
    }

    public async Task<bool> SaveWindowStateAsync(WindowInfo window)
    {
        try
        {
            if (window == null) return false;
            var handle = await FindWindowByInfoAsync(window);
            if (handle == IntPtr.Zero) return false;

            if (_windowApi.TryGetWindowRect(handle, out var pos, out var size, out _))
            {
                window.Position = pos;
                window.Size = size;
                window.State = _windowApi.IsWindowMinimized(handle) ? WindowState.Minimized : (_windowApi.IsWindowMaximized(handle) ? WindowState.Maximized : WindowState.Normal);
                window.IsVisible = _windowApi.IsWindowVisible(handle);
                window.Monitor = GetMonitorIndexForWindow(handle);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SaveWindowStateAsync failed");
            return false;
        }
    }
    public async Task<bool> MoveWindowAsync(string windowId, Point position)
    {
        if (!TryParseWindowId(windowId, out var handle)) return false;
        var ok = _windowApi.TryMoveWindow(handle, position.X, position.Y, out var error);
        if (!ok) _logger.LogWarning("MoveWindow failed: {Error}", error);
        return await Task.FromResult(ok);
    }

    public async Task<bool> ResizeWindowAsync(string windowId, Size size)
    {
        if (!TryParseWindowId(windowId, out var handle)) return false;
        var ok = _windowApi.TryResizeWindow(handle, size.Width, size.Height, out var error);
        if (!ok) _logger.LogWarning("ResizeWindow failed: {Error}", error);
        return await Task.FromResult(ok);
    }

    public async Task<bool> SetWindowStateAsync(string windowId, WindowState state)
    {
        if (!TryParseWindowId(windowId, out var handle)) return false;
        bool ok = false;
        string error = string.Empty;
        switch (state)
        {
            case WindowState.Minimized:
                ok = _windowApi.TryMinimizeWindow(handle, out error);
                break;
            case WindowState.Maximized:
                ok = _windowApi.TryMaximizeWindow(handle, out error);
                break;
            default:
                ok = _windowApi.TryRestoreWindow(handle, out error);
                break;
        }
        if (!ok) _logger.LogWarning("SetWindowState to {State} failed: {Error}", state, error);
        return await Task.FromResult(ok);
    }

    public async Task<bool> ShowWindowAsync(string windowId)
    {
        if (!TryParseWindowId(windowId, out var handle)) return false;
        var ok = _windowApi.TryShowWindow(handle, out var error);
        if (!ok) _logger.LogWarning("ShowWindow failed: {Error}", error);
        return await Task.FromResult(ok);
    }

    public async Task<bool> HideWindowAsync(string windowId)
    {
        if (!TryParseWindowId(windowId, out var handle)) return false;
        var ok = _windowApi.TryHideWindow(handle, out var error);
        if (!ok) _logger.LogWarning("HideWindow failed: {Error}", error);
        return await Task.FromResult(ok);
    }

    public async Task<bool> BringWindowToFrontAsync(string windowId)
    {
        if (!TryParseWindowId(windowId, out var handle)) return false;
        var ok = _windowApi.TryBringWindowToFront(handle, out var error);
        if (!ok) _logger.LogWarning("BringWindowToFront failed: {Error}", error);
        return await Task.FromResult(ok);
    }

    public async Task<bool> SendWindowToBackAsync(string windowId)
    {
        // Not supported by platform API yet; return false with log for now
        _logger.LogInformation("SendWindowToBackAsync not supported by platform implementation yet");
        return await Task.FromResult(false);
    }

    public async Task<bool> MoveWindowToMonitorAsync(string windowId, int monitorIndex)
    {
        try
        {
            if (!TryParseWindowId(windowId, out var handle)) return false;

            var monitor = await GetMonitorByIndexAsync(monitorIndex);
            if (monitor == null)
            {
                _logger.LogWarning("Target monitor index {Index} not found", monitorIndex);
                return false;
            }

            // Get current size
            if (!_windowApi.TryGetWindowRect(handle, out var pos, out var size, out var rectError))
            {
                _logger.LogWarning("Failed to get window rect for move to monitor: {Error}", rectError);
                return false;
            }

            // Position near top-left of target monitor's working area, clamped
            var newX = monitor.WorkingArea.X + 20;
            var newY = monitor.WorkingArea.Y + 20;
            var width = Math.Min(size.Width, monitor.WorkingArea.Width);
            var height = Math.Min(size.Height, monitor.WorkingArea.Height);

            var ok = _windowApi.TrySetWindowBounds(handle, newX, newY, width, height, out var error);
            if (!ok) _logger.LogWarning("MoveWindowToMonitor failed: {Error}", error);
            return ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MoveWindowToMonitorAsync failed");
            return false;
        }
    }

    public async Task<IntPtr> FindWindowByInfoAsync(WindowInfo windowInfo)
    {
        try
        {
            if (windowInfo == null)
                return IntPtr.Zero;

            // If a valid window ID is provided and points to a live window, return it immediately
            if (!string.IsNullOrWhiteSpace(windowInfo.WindowId) && TryParseWindowId(windowInfo.WindowId, out var parsedHandle))
            {
                if (_windowApi.IsWindow(parsedHandle))
                    return parsedHandle;
            }

            var handles = _windowApi.GetAllWindows();
            foreach (var handle in handles)
            {
                if (!_windowApi.IsWindow(handle))
                    continue;

                // Gather attributes for matching
                _ = _windowApi.TryGetWindowText(handle, out var title, out _);
                _ = _windowApi.TryGetClassName(handle, out var className, out _);
                var processName = string.Empty;
                if (_windowApi.TryGetWindowProcessId(handle, out var pid, out _))
                {
                    try { processName = Process.GetProcessById(pid).ProcessName; }
                    catch { processName = string.Empty; }
                }

                // Match all provided non-empty criteria (case-insensitive; title as contains)
                var matches = true;
                if (!string.IsNullOrWhiteSpace(windowInfo.WindowTitle))
                {
                    matches &= !string.IsNullOrEmpty(title) && title.IndexOf(windowInfo.WindowTitle, StringComparison.OrdinalIgnoreCase) >= 0;
                }
                if (!string.IsNullOrWhiteSpace(windowInfo.ClassName))
                {
                    matches &= !string.IsNullOrEmpty(className) && className.Equals(windowInfo.ClassName, StringComparison.OrdinalIgnoreCase);
                }
                if (!string.IsNullOrWhiteSpace(windowInfo.ProcessName))
                {
                    var providedProcess = windowInfo.ProcessName;
                    bool procMatch;
                    if (TryParsePidPlaceholder(providedProcess, out var expectedPid))
                    {
                        // Match by PID when placeholder format is provided
                        procMatch = _windowApi.TryGetWindowProcessId(handle, out var actualPid, out _) && actualPid == expectedPid;
                    }
                    else
                    {
                        procMatch = !string.IsNullOrEmpty(processName) && processName.Equals(providedProcess, StringComparison.OrdinalIgnoreCase);
                    }
                    matches &= procMatch;
                }

                if (matches)
                    return handle;
            }

            return IntPtr.Zero;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find window by info");
            return IntPtr.Zero;
        }
    }

    public async Task<WindowInfo?> GetWindowDetailsAsync(string windowId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(windowId))
                return null;

            if (!TryParseWindowId(windowId, out var handle))
                return null;

            if (!_windowApi.IsWindow(handle))
                return null;

            return await Task.FromResult(BuildWindowInfo(handle, windowId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get window details for {WindowId}", windowId);
            throw;
        }
    }

    public async Task<WindowInfo?> RefreshWindowInfoAsync(string windowId)
    {
        // For now, this is equivalent to fetching details again
        return await GetWindowDetailsAsync(windowId);
    }

    public async Task<bool> IsWindowValidAsync(string windowId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(windowId))
                return false;

            if (!TryParseWindowId(windowId, out var handle))
                return false;

            return await Task.FromResult(_windowApi.IsWindow(handle));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate window ID {WindowId}", windowId);
            return false;
        }
    }

    public async Task<IEnumerable<MonitorInfo>> GetMonitorConfigurationAsync()
    {
        try
        {
            var descriptors = _windowApi.GetAllMonitors();
            var monitors = new List<MonitorInfo>();
            foreach (var d in descriptors)
            {
                var mi = new MonitorInfo
                {
                    Index = d.Index,
                    IsPrimary = d.IsPrimary,
                    Bounds = new Rectangle(d.BoundsX, d.BoundsY, d.BoundsWidth, d.BoundsHeight),
                    WorkingArea = new Rectangle(d.WorkingX, d.WorkingY, d.WorkingWidth, d.WorkingHeight),
                    Dpi = d.Dpi,
                    RefreshRate = d.RefreshRate,
                    Name = string.IsNullOrWhiteSpace(d.Name) ? $"Monitor {d.Index}" : d.Name
                };
                monitors.Add(mi);
            }
            return await Task.FromResult(monitors.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get monitor configuration");
            throw;
        }
    }

    public async Task<MonitorInfo?> GetPrimaryMonitorAsync()
    {
        var monitors = (await GetMonitorConfigurationAsync()).ToList();
        return monitors.FirstOrDefault(m => m.IsPrimary) ?? monitors.FirstOrDefault(m => m.Index == 0);
    }

    public async Task<MonitorInfo?> GetMonitorByIndexAsync(int index)
    {
        var monitors = await GetMonitorConfigurationAsync();
        return monitors.FirstOrDefault(m => m.Index == index);
    }
    public async Task<IEnumerable<WindowInfo>> CaptureDesktopLayoutAsync()
    {
        var windows = await GetCurrentWindowsAsync();
        return windows;
    }
    public Task<WindowStatistics> GetWindowStatisticsAsync() => throw new NotImplementedException();

    private static bool TryParseWindowId(string windowId, out IntPtr handle)
    {
        handle = IntPtr.Zero;
        try
        {
            // WindowId is stored as hex string without 0x prefix
            var value = Convert.ToInt64(windowId, 16);
            handle = new IntPtr(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private WindowInfo BuildWindowInfo(IntPtr handle, string? windowIdOverride = null)
    {
        string title = string.Empty, className = string.Empty;
        Point position = new Point();
        Size size = new Size();

        _ = _windowApi.TryGetWindowText(handle, out title, out _);
        _ = _windowApi.TryGetClassName(handle, out className, out _);
        _ = _windowApi.TryGetWindowRect(handle, out position, out size, out _);

        var processName = string.Empty;
        if (_windowApi.TryGetWindowProcessId(handle, out var pid, out _))
        {
            try { processName = Process.GetProcessById(pid).ProcessName; }
            catch { processName = $"Process_{pid}"; }
        }

        var state = WindowState.Normal;
        try
        {
            if (_windowApi.IsWindowMinimized(handle)) state = WindowState.Minimized;
            else if (_windowApi.IsWindowMaximized(handle)) state = WindowState.Maximized;
        }
        catch { /* best-effort */ }

        var windowInfo = new WindowInfo
        {
            WindowId = !string.IsNullOrWhiteSpace(windowIdOverride) ? windowIdOverride : handle.ToString("X"),
            ProcessName = processName,
            WindowTitle = title ?? string.Empty,
            ClassName = className ?? string.Empty,
            Position = position,
            Size = size,
            State = state,
            Monitor = GetMonitorIndexForWindow(handle),
            ZOrder = 0,
            IsVisible = true
        };

        return windowInfo;
    }

    private static bool TryParsePidPlaceholder(string processName, out int pid)
    {
        pid = 0;
        if (string.IsNullOrWhiteSpace(processName))
            return false;
        if (!processName.StartsWith("Process_", StringComparison.OrdinalIgnoreCase))
            return false;
        var suffix = processName.Substring("Process_".Length);
        return int.TryParse(suffix, out pid);
    }

    private int GetMonitorIndexForWindow(IntPtr windowHandle)
    {
        try
        {
			if (_windowApi.TryGetWindowMonitor(windowHandle, out var monitorHandle, out _))
            {
				// Build mapping from platform-reported monitors to indices
				if (_monitorHandleToIndex.Count == 0)
				{
					var monitors = _windowApi.GetAllMonitors();
					foreach (var mon in monitors)
					{
						_monitorHandleToIndex[mon.Handle] = mon.Index;
						if (mon.Index >= _nextMonitorIndex) _nextMonitorIndex = mon.Index + 1;
					}
				}

				if (_monitorHandleToIndex.TryGetValue(monitorHandle, out var existingIndex))
					return existingIndex;

				// Fallback: assign next index to unknown handle
				var assignedIndex = _nextMonitorIndex++;
				_monitorHandleToIndex[monitorHandle] = assignedIndex;
				return assignedIndex;
            }
        }
        catch
        {
            // Fallthrough to default
        }
        // Default to primary monitor index 0 when unknown
        return 0;
    }
}
