using System;
using System.Collections.Generic;
using SnapDesk.Core;
using SnapDesk.Platform.Interfaces;

namespace SnapDesk.Platform.Common;

/// <summary>
/// Cross-platform stub implementation of IWindowApi that provides safe defaults
/// on platforms that don't support window management. This prevents crashes
/// and allows the application to run on any platform.
/// </summary>
public class StubWindowApi : IWindowApi
{
	// === BATCH A1: Basic Window Operations ===
	/// <summary>
	/// Gets the desktop window handle.
	/// </summary>
	/// <returns>IntPtr.Zero (not supported on this platform)</returns>
	public IntPtr GetDesktopWindow()
	{
		return IntPtr.Zero;
	}

	/// <summary>
	/// Gets the shell window handle (usually the taskbar/desktop host).
	/// </summary>
	/// <returns>IntPtr.Zero (not supported on this platform)</returns>
	public IntPtr GetShellWindow()
	{
		return IntPtr.Zero;
	}

	/// <summary>
	/// Gets the foreground (active) window handle.
	/// </summary>
	/// <returns>IntPtr.Zero (not supported on this platform)</returns>
	public IntPtr GetForegroundWindow()
	{
		return IntPtr.Zero;
	}

	/// <summary>
	/// Checks if a handle is a valid window.
	/// </summary>
	/// <param name="hWnd">Window handle to check</param>
	/// <returns>False (not supported on this platform)</returns>
	public bool IsWindow(IntPtr hWnd)
	{
		return false;
	}

	/// <summary>
	/// Checks if a window is visible.
	/// </summary>
	/// <param name="hWnd">Window handle to check</param>
	/// <returns>False (not supported on this platform)</returns>
	public bool IsWindowVisible(IntPtr hWnd)
	{
		return false;
	}

	/// <summary>
	/// Attempts to get the window title text. Returns false with error details on failure.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="text">Empty string (not supported)</param>
	/// <param name="error">Platform not supported message</param>
	/// <returns>False (not supported on this platform)</returns>
	public bool TryGetWindowText(IntPtr hWnd, out string text, out string error)
	{
		text = string.Empty;
		error = "Window management not supported on this platform.";
		return false;
	}

	/// <summary>
	/// Attempts to get the window class name. Returns false with error details on failure.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="className">Empty string (not supported)</param>
	/// <param name="error">Platform not supported message</param>
	/// <returns>False (not supported on this platform)</returns>
	public bool TryGetClassName(IntPtr hWnd, out string className, out string error)
	{
		className = string.Empty;
		error = "Window management not supported on this platform.";
		return false;
	}

	/// <summary>
	/// Attempts to get the window rectangle and convert to position/size. Returns false with error on failure.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="position">Default position (0,0)</param>
	/// <param name="size">Default size (0,0)</param>
	/// <param name="error">Platform not supported message</param>
	/// <returns>False (not supported on this platform)</returns>
	public bool TryGetWindowRect(IntPtr hWnd, out Point position, out Size size, out string error)
	{
		position = new Point(0, 0);
		size = new Size(0, 0);
		error = "Window management not supported on this platform.";
		return false;
	}

	// === BATCH A2: Enhanced Window Queries ===
	/// <summary>
	/// Attempts to get the process ID that owns a window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="processId">Default process ID (0)</param>
	/// <param name="error">Platform not supported message</param>
	/// <returns>False (not supported on this platform)</returns>
	public bool TryGetWindowProcessId(IntPtr hWnd, out int processId, out string error)
	{
		processId = 0;
		error = "Window management not supported on this platform.";
		return false;
	}

	/// <summary>
	/// Attempts to get the thread ID that owns a window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="threadId">Default thread ID (0)</param>
	/// <param name="error">Platform not supported message</param>
	/// <returns>False (not supported on this platform)</returns>
	public bool TryGetWindowThreadId(IntPtr hWnd, out int threadId, out string error)
	{
		threadId = 0;
		error = "Window management not supported on this platform.";
		return false;
	}

	/// <summary>
	/// Attempts to get the parent window of a given window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="parentHwnd">Default parent handle (IntPtr.Zero)</param>
	/// <param name="error">Platform not supported message</param>
	/// <returns>False (not supported on this platform)</returns>
	public bool TryGetParentWindow(IntPtr hWnd, out IntPtr parentHwnd, out string error)
	{
		parentHwnd = IntPtr.Zero;
		error = "Window management not supported on this platform.";
		return false;
	}

	/// <summary>
	/// Attempts to get the owner window of a given window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="ownerHwnd">Default owner handle (IntPtr.Zero)</param>
	/// <param name="error">Platform not supported message</param>
	/// <returns>False (not supported on this platform)</returns>
	public bool TryGetOwnerWindow(IntPtr hWnd, out IntPtr ownerHwnd, out string error)
	{
		ownerHwnd = IntPtr.Zero;
		error = "Window management not supported on this platform.";
		return false;
	}

	/// <summary>
	/// Attempts to enumerate all child windows of a given window.
	/// </summary>
	/// <param name="hWnd">Parent window handle</param>
	/// <param name="childWindows">Empty list (not supported)</param>
	/// <param name="error">Platform not supported message</param>
	/// <returns>False (not supported on this platform)</returns>
	public bool TryGetChildWindows(IntPtr hWnd, out List<IntPtr> childWindows, out string error)
	{
		childWindows = new List<IntPtr>();
		error = "Window management not supported on this platform.";
		return false;
	}

	/// <summary>
	/// Checks if a window is minimized.
	/// </summary>
	/// <param name="hWnd">Window handle to check</param>
	/// <returns>False (not supported on this platform)</returns>
	public bool IsWindowMinimized(IntPtr hWnd)
	{
		return false;
	}

	/// <summary>
	/// Checks if a window is maximized.
	/// </summary>
	/// <param name="hWnd">Window handle to check</param>
	/// <returns>False (not supported on this platform)</returns>
	public bool IsWindowMaximized(IntPtr hWnd)
	{
		return false;
	}

	/// <summary>
	/// Attempts to get the monitor handle that contains a window.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="monitorHandle">Default monitor handle (IntPtr.Zero)</param>
	/// <param name="error">Platform not supported message</param>
	/// <returns>False (not supported on this platform)</returns>
	public bool TryGetWindowMonitor(IntPtr hWnd, out IntPtr monitorHandle, out string error)
	{
		monitorHandle = IntPtr.Zero;
		error = "Window management not supported on this platform.";
		return false;
	}

	/// <summary>
	/// Attempts to get the window's extended style information.
	/// </summary>
	/// <param name="hWnd">Window handle</param>
	/// <param name="style">Default style object (all false)</param>
	/// <param name="error">Platform not supported message</param>
	/// <returns>False (not supported on this platform)</returns>
	public bool TryGetWindowStyle(IntPtr hWnd, out WindowStyle style, out string error)
	{
		style = new WindowStyle();
		error = "Window management not supported on this platform.";
		return false;
	}

	// === BATCH B: Window Manipulation Operations ===
	
	public bool TryMoveWindow(IntPtr hWnd, int x, int y, out string error)
	{
		error = "Window manipulation not supported on this platform.";
		return false;
	}

	public bool TryResizeWindow(IntPtr hWnd, int width, int height, out string error)
	{
		error = "Window manipulation not supported on this platform.";
		return false;
	}

	public bool TrySetWindowBounds(IntPtr hWnd, int x, int y, int width, int height, out string error)
	{
		error = "Window manipulation not supported on this platform.";
		return false;
	}

	public bool TryMinimizeWindow(IntPtr hWnd, out string error)
	{
		error = "Window manipulation not supported on this platform.";
		return false;
	}

	public bool TryMaximizeWindow(IntPtr hWnd, out string error)
	{
		error = "Window manipulation not supported on this platform.";
		return false;
	}

	public bool TryRestoreWindow(IntPtr hWnd, out string error)
	{
		error = "Window manipulation not supported on this platform.";
		return false;
	}

	public bool TryShowWindow(IntPtr hWnd, out string error)
	{
		error = "Window manipulation not supported on this platform.";
		return false;
	}

	public bool TryHideWindow(IntPtr hWnd, out string error)
	{
		error = "Window manipulation not supported on this platform.";
		return false;
	}

	public bool TryBringWindowToFront(IntPtr hWnd, out string error)
	{
		error = "Window manipulation not supported on this platform.";
		return false;
	}

	public bool TrySetForegroundWindow(IntPtr hWnd, out string error)
	{
		error = "Window manipulation not supported on this platform.";
		return false;
	}

	public bool TrySetAlwaysOnTop(IntPtr hWnd, bool alwaysOnTop, out string error)
	{
		error = "Window manipulation not supported on this platform.";
		return false;
	}

	public bool TrySetWindowTransparency(IntPtr hWnd, byte alpha, out string error)
	{
		error = "Window manipulation not supported on this platform.";
		return false;
	}
}
