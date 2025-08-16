using System;
using System.Collections.Generic;
using SnapDesk.Shared;

namespace SnapDesk.Platform.Interfaces;

/// <summary>
/// Cross-platform interface for window operations. This interface can be implemented
/// on different platforms (Windows, macOS, Linux) to provide platform-specific
/// window management capabilities.
/// </summary>
public interface IWindowApi
{
	// === BATCH A1: Basic Window Operations ===
	/// <summary>
	/// Gets the desktop window handle.
	/// </summary>
	/// <returns>Desktop window handle, or IntPtr.Zero if not supported</returns>
	IntPtr GetDesktopWindow();

	/// <summary>
	/// Gets the shell window handle (usually the taskbar/desktop host).
	/// </summary>
	/// <returns>Shell window handle, or IntPtr.Zero if not supported</returns>
	IntPtr GetShellWindow();

	/// <summary>
	/// Gets the foreground (active) window handle.
	/// </summary>
	/// <returns>Foreground window handle, or IntPtr.Zero if not supported</returns>
	IntPtr GetForegroundWindow();

	/// <summary>
	/// Checks if a handle is a valid window.
	/// </summary>
	/// <param name="hWnd">Window handle to check</param>
	/// <returns>True if valid window, false otherwise</returns>
	bool IsWindow(IntPtr hWnd);

	/// <summary>
	/// Checks if a window is visible.
	/// </summary>
	/// <param name="hWnd">Window handle to check</param>
	/// <returns>True if window is visible, false otherwise</returns>
	bool IsWindowVisible(IntPtr hWnd);

	/// <summary>
	/// Attempts to get the window title text. Returns false with error details on failure.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="text">Window title text if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryGetWindowText(IntPtr hWnd, out string text, out string error);

	/// <summary>
	/// Attempts to get the window class name. Returns false with error details on failure.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="className">Window class name if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryGetClassName(IntPtr hWnd, out string className, out string error);

	/// <summary>
	/// Attempts to get the window rectangle and convert to position/size. Returns false with error on failure.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="position">Window position if successful</param>
	/// <param name="size">Window size if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryGetWindowRect(IntPtr hWnd, out Point position, out Size size, out string error);

	// === BATCH A2: Enhanced Window Queries ===
	/// <summary>
	/// Attempts to get the process ID that owns a window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="processId">Process ID if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryGetWindowProcessId(IntPtr hWnd, out int processId, out string error);

	/// <summary>
	/// Attempts to get the thread ID that owns a window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="threadId">Thread ID if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryGetWindowThreadId(IntPtr hWnd, out int threadId, out string error);

	/// <summary>
	/// Attempts to get the parent window of a given window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="parentHwnd">Parent window handle if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryGetParentWindow(IntPtr hWnd, out IntPtr parentHwnd, out string error);

	/// <summary>
	/// Attempts to get the owner window of a given window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="ownerHwnd">Owner window handle if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryGetOwnerWindow(IntPtr hWnd, out IntPtr ownerHwnd, out string error);

	/// <summary>
	/// Attempts to enumerate all child windows of a given window.
	/// </summary>
	/// <param name="hWnd">Parent window handle</param>
	/// <param name="childWindows">List of child window handles if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryGetChildWindows(IntPtr hWnd, out List<IntPtr> childWindows, out string error);

	/// <summary>
	/// Checks if a window is minimized.
	/// </summary>
	/// <param name="hWnd">Window handle to check</param>
	/// <returns>True if window is minimized, false otherwise</returns>
	bool IsWindowMinimized(IntPtr hWnd);

	/// <summary>
	/// Checks if a window is maximized.
	/// </summary>
	/// <param name="hWnd">Window handle to check</param>
	/// <returns>True if window is maximized, false otherwise</returns>
	bool IsWindowMaximized(IntPtr hWnd);

	/// <summary>
	/// Attempts to get the monitor handle that contains a window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="monitorHandle">Monitor handle if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryGetWindowMonitor(IntPtr hWnd, out IntPtr monitorHandle, out string error);

	/// <summary>
	/// Attempts to get the window's extended style information.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="style">Window style flags if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryGetWindowStyle(IntPtr hWnd, out WindowStyle style, out string error);

	// === BATCH B: Window Manipulation Operations ===
	
	/// <summary>
	/// Attempts to move a window to a specific position.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="x">New X coordinate</param>
	/// <param name="y">New Y coordinate</param>
	/// <param name="error">Error message if operation fails</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryMoveWindow(IntPtr hWnd, int x, int y, out string error);

	/// <summary>
	/// Attempts to resize a window to specific dimensions.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="width">New width</param>
	/// <param name="height">New height</param>
	/// <param name="error">Error message if operation fails</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryResizeWindow(IntPtr hWnd, int width, int height, out string error);

	/// <summary>
	/// Attempts to set both position and size of a window in one operation.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="x">New X coordinate</param>
	/// <param name="y">New Y coordinate</param>
	/// <param name="width">New width</param>
	/// <param name="height">New height</param>
	/// <param name="error">Error message if operation fails</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TrySetWindowBounds(IntPtr hWnd, int x, int y, int width, int height, out string error);

	/// <summary>
	/// Attempts to minimize a window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="error">Error message if operation fails</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryMinimizeWindow(IntPtr hWnd, out string error);

	/// <summary>
	/// Attempts to maximize a window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="error">Error message if operation fails</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryMaximizeWindow(IntPtr hWnd, out string error);

	/// <summary>
	/// Attempts to restore a window from minimized or maximized state.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="error">Error message if operation fails</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryRestoreWindow(IntPtr hWnd, out string error);

	/// <summary>
	/// Attempts to show a window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="error">Error message if operation fails</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryShowWindow(IntPtr hWnd, out string error);

	/// <summary>
	/// Attempts to hide a window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="error">Error message if operation fails</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryHideWindow(IntPtr hWnd, out string error);

	/// <summary>
	/// Attempts to bring a window to the front and give it focus.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="error">Error message if operation fails</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryBringWindowToFront(IntPtr hWnd, out string error);

	/// <summary>
	/// Attempts to set a window as the foreground window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="error">Error message if operation fails</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TrySetForegroundWindow(IntPtr hWnd, out string error);

	/// <summary>
	/// Attempts to set a window as always-on-top.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="alwaysOnTop">Whether the window should be always on top</param>
	/// <param name="error">Error message if operation fails</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TrySetAlwaysOnTop(IntPtr hWnd, bool alwaysOnTop, out string error);

	/// <summary>
	/// Attempts to set window transparency (alpha blending).
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="alpha">Transparency value (0 = fully transparent, 255 = fully opaque)</param>
	/// <param name="error">Error message if operation fails</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TrySetWindowTransparency(IntPtr hWnd, byte alpha, out string error);

	/// <summary>
	/// Attempts to set window position, size, and Z-order using SetWindowPos.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="hWndInsertAfter">Window to insert after (use IntPtr(1) for HWND_BOTTOM, IntPtr(0) for HWND_TOP)</param>
	/// <param name="x">New X coordinate (ignored if SWP_NOMOVE flag is set)</param>
	/// <param name="y">New Y coordinate (ignored if SWP_NOMOVE flag is set)</param>
	/// <param name="cx">New width (ignored if SWP_NOSIZE flag is set)</param>
	/// <param name="cy">New height (ignored if SWP_NOSIZE flag is set)</param>
	/// <param name="flags">SetWindowPos flags</param>
	/// <returns>True if successful, false otherwise</returns>
	bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SetWindowPosFlags flags);

	/// <summary>
	/// Gets all top-level windows on the desktop.
	/// </summary>
	/// <returns>List of window handles for all top-level windows</returns>
	List<IntPtr> GetAllWindows();

	/// <summary>
	/// Enumerates all monitors and returns platform-level descriptors.
	/// Order defines monitor indices (0..n-1), primary flagged accordingly.
	/// </summary>
	List<SnapDesk.Platform.Common.MonitorDescriptor> GetAllMonitors();
}
