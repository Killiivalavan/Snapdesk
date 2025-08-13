using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using SnapDesk.Core;
using SnapDesk.Platform.Interfaces;

namespace SnapDesk.Platform.Windows;

/// <summary>
/// Windows implementation of the IWindowApi interface. This class provides
/// Windows-specific window management capabilities using the Win32 API.
/// </summary>
[SupportedOSPlatform("windows")]
public class WindowsWindowApi : IWindowApi
{
	// === BATCH A1: Basic Window Operations ===
	/// <summary>
	/// Gets the desktop window handle.
	/// </summary>
	/// <returns>Desktop window handle, or IntPtr.Zero if not supported</returns>
	public IntPtr GetDesktopWindow()
	{
		return WindowApi.GetDesktopWindow();
	}

	/// <summary>
	/// Gets the shell window handle (usually the taskbar/desktop host).
	/// </summary>
	/// <returns>Shell window handle, or IntPtr.Zero if not supported</returns>
	public IntPtr GetShellWindow()
	{
		return WindowApi.GetShellWindow();
	}

	/// <summary>
	/// Gets the foreground (active) window handle.
	/// </summary>
	/// <returns>Foreground window handle, or IntPtr.Zero if not supported</returns>
	public IntPtr GetForegroundWindow()
	{
		return WindowApi.GetForegroundWindow();
	}

	/// <summary>
	/// Checks if a handle is a valid window.
	/// </summary>
	/// <param name="hWnd">Window handle to check</param>
	/// <returns>True if valid window, false otherwise</returns>
	public bool IsWindow(IntPtr hWnd)
	{
		return WindowApi.IsWindow(hWnd);
	}

	/// <summary>
	/// Checks if a window is visible.
	/// </summary>
	/// <param name="hWnd">Window handle to check</param>
	/// <returns>True if window is visible, false otherwise</returns>
	public bool IsWindowVisible(IntPtr hWnd)
	{
		return WindowApi.IsWindowVisible(hWnd);
	}

	/// <summary>
	/// Attempts to get the window title text. Returns false with error details on failure.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="text">Window title text if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	public bool TryGetWindowText(IntPtr hWnd, out string text, out string error)
	{
		return WindowApi.TryGetWindowText(hWnd, out text, out error);
	}

	/// <summary>
	/// Attempts to get the window class name. Returns false with error details on failure.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="className">Window class name if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	public bool TryGetClassName(IntPtr hWnd, out string className, out string error)
	{
		return WindowApi.TryGetClassName(hWnd, out className, out error);
	}

	/// <summary>
	/// Attempts to get the window rectangle and convert to position/size. Returns false with error on failure.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="position">Window position if successful</param>
	/// <param name="size">Window size if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	public bool TryGetWindowRect(IntPtr hWnd, out Point position, out Size size, out string error)
	{
		return WindowApi.TryGetWindowRect(hWnd, out position, out size, out error);
	}

	// === BATCH A2: Enhanced Window Queries ===
	/// <summary>
	/// Attempts to get the process ID that owns a window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="processId">Process ID if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	public bool TryGetWindowProcessId(IntPtr hWnd, out int processId, out string error)
	{
		return WindowApi.TryGetWindowProcessId(hWnd, out processId, out error);
	}

	/// <summary>
	/// Attempts to get the thread ID that owns a window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="threadId">Thread ID if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	public bool TryGetWindowThreadId(IntPtr hWnd, out int threadId, out string error)
	{
		return WindowApi.TryGetWindowThreadId(hWnd, out threadId, out error);
	}

	/// <summary>
	/// Attempts to get the parent window of a given window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="parentHwnd">Parent window handle if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	public bool TryGetParentWindow(IntPtr hWnd, out IntPtr parentHwnd, out string error)
	{
		return WindowApi.TryGetParentWindow(hWnd, out parentHwnd, out error);
	}

	/// <summary>
	/// Attempts to get the owner window of a given window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="ownerHwnd">Owner window handle if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	public bool TryGetOwnerWindow(IntPtr hWnd, out IntPtr ownerHwnd, out string error)
	{
		return WindowApi.TryGetOwnerWindow(hWnd, out ownerHwnd, out error);
	}

	/// <summary>
	/// Attempts to enumerate all child windows of a given window.
	/// </summary>
	/// <param name="hWnd">Parent window handle</param>
	/// <param name="childWindows">List of child window handles if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	public bool TryGetChildWindows(IntPtr hWnd, out List<IntPtr> childWindows, out string error)
	{
		return WindowApi.TryGetChildWindows(hWnd, out childWindows, out error);
	}

	/// <summary>
	/// Checks if a window is minimized.
	/// </summary>
	/// <param name="hWnd">Window handle to check</param>
	/// <returns>True if window is minimized, false otherwise</returns>
	public bool IsWindowMinimized(IntPtr hWnd)
	{
		return WindowApi.IsWindowMinimized(hWnd);
	}

	/// <summary>
	/// Checks if a window is maximized.
	/// </summary>
	/// <param name="hWnd">Window handle to check</param>
	/// <returns>True if window is maximized, false otherwise</returns>
	public bool IsWindowMaximized(IntPtr hWnd)
	{
		return WindowApi.IsWindowMaximized(hWnd);
	}

	/// <summary>
	/// Attempts to get the monitor handle that contains a window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="monitorHandle">Monitor handle if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	public bool TryGetWindowMonitor(IntPtr hWnd, out IntPtr monitorHandle, out string error)
	{
		return WindowApi.TryGetWindowMonitor(hWnd, out monitorHandle, out error);
	}

	/// <summary>
	/// Attempts to get the window's extended style information.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="style">Window style flags if successful</param>
	/// <param name="error">Error message if failed</param>
	/// <returns>True if successful, false otherwise</returns>
	public bool TryGetWindowStyle(IntPtr hWnd, out WindowStyle style, out string error)
	{
		return WindowApi.TryGetWindowStyle(hWnd, out style, out error);
	}

	// === BATCH B: Window Manipulation Operations ===
	
	public bool TryMoveWindow(IntPtr hWnd, int x, int y, out string error)
	{
		return WindowApi.TryMoveWindow(hWnd, x, y, out error);
	}

	public bool TryResizeWindow(IntPtr hWnd, int width, int height, out string error)
	{
		return WindowApi.TryResizeWindow(hWnd, width, height, out error);
	}

	public bool TrySetWindowBounds(IntPtr hWnd, int x, int y, int width, int height, out string error)
	{
		return WindowApi.TrySetWindowBounds(hWnd, x, y, width, height, out error);
	}

	public bool TryMinimizeWindow(IntPtr hWnd, out string error)
	{
		return WindowApi.TryMinimizeWindow(hWnd, out error);
	}

	public bool TryMaximizeWindow(IntPtr hWnd, out string error)
	{
		return WindowApi.TryMaximizeWindow(hWnd, out error);
	}

	public bool TryRestoreWindow(IntPtr hWnd, out string error)
	{
		return WindowApi.TryRestoreWindow(hWnd, out error);
	}

	public bool TryShowWindow(IntPtr hWnd, out string error)
	{
		return WindowApi.TryShowWindow(hWnd, out error);
	}

	public bool TryHideWindow(IntPtr hWnd, out string error)
	{
		return WindowApi.TryHideWindow(hWnd, out error);
	}

	public bool TryBringWindowToFront(IntPtr hWnd, out string error)
	{
		return WindowApi.TryBringWindowToFront(hWnd, out error);
	}

	public bool TrySetForegroundWindow(IntPtr hWnd, out string error)
	{
		return WindowApi.TrySetForegroundWindow(hWnd, out error);
	}

	public bool TrySetAlwaysOnTop(IntPtr hWnd, bool alwaysOnTop, out string error)
	{
		return WindowApi.TrySetAlwaysOnTop(hWnd, alwaysOnTop, out error);
	}

	public bool TrySetWindowTransparency(IntPtr hWnd, byte alpha, out string error)
	{
		return WindowApi.TrySetWindowTransparency(hWnd, alpha, out error);
	}
}
