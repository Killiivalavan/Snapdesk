using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;
using SnapDesk.Core;
using SnapDesk.Platform.Interfaces;
using Vanara.PInvoke;

namespace SnapDesk.Platform.Windows;

/// <summary>
/// Read-only Windows API helpers for safe window queries. These wrappers
/// avoid throwing and instead return detailed failure information via out parameters.
/// This implementation is Windows-specific and will return safe defaults on other platforms.
/// </summary>
[SupportedOSPlatform("windows")]
public static class WindowApi
{
	// === BATCH A1: Basic Window Operations ===
	/// <summary>
	/// Gets the desktop window handle.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static IntPtr GetDesktopWindow()
	{
		if (!OperatingSystem.IsWindows())
			return IntPtr.Zero;

		var hwnd = User32.GetDesktopWindow();
		return (IntPtr)hwnd;
	}

	/// <summary>
	/// Gets the shell window handle (usually the taskbar/desktop host).
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static IntPtr GetShellWindow()
	{
		if (!OperatingSystem.IsWindows())
			return IntPtr.Zero;

		var hwnd = User32.GetShellWindow();
		return (IntPtr)hwnd;
	}

	/// <summary>
	/// Gets the foreground (active) window handle.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static IntPtr GetForegroundWindow()
	{
		if (!OperatingSystem.IsWindows())
			return IntPtr.Zero;

		var hwnd = User32.GetForegroundWindow();
		return (IntPtr)hwnd;
	}

	/// <summary>
	/// Checks if a handle is a valid window.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool IsWindow(IntPtr hWnd)
	{
		if (!OperatingSystem.IsWindows() || hWnd == IntPtr.Zero) 
			return false;

		return User32.IsWindow(new HWND(hWnd));
	}

	/// <summary>
	/// Checks if a window is visible.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool IsWindowVisible(IntPtr hWnd)
	{
		if (!OperatingSystem.IsWindows() || !IsWindow(hWnd)) 
			return false;

		return User32.IsWindowVisible(new HWND(hWnd));
	}

	/// <summary>
	/// Attempts to get the window title text. Returns false with error details on failure.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryGetWindowText(IntPtr hWnd, out string text, out string error)
	{
		text = string.Empty;
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		// Get length first to allocate buffer precisely
		int length = User32.GetWindowTextLength(new HWND(hWnd));
		if (length <= 0)
		{
			// Could be no title or an error; try to call anyway with a small buffer
			var sbEmpty = new StringBuilder(1);
			int readEmpty = User32.GetWindowText(new HWND(hWnd), sbEmpty, sbEmpty.Capacity);
			if (readEmpty <= 0)
			{
				error = "Window has no title or failed to retrieve title.";
				return false;
			}
			text = sbEmpty.ToString();
			return true;
		}

		var sb = new StringBuilder(length + 1);
		int read = User32.GetWindowText(new HWND(hWnd), sb, sb.Capacity);
		if (read <= 0)
		{
			error = "Failed to retrieve window title text.";
			return false;
		}

		text = sb.ToString();
		return true;
	}

	/// <summary>
	/// Attempts to get the window class name. Returns false with error details on failure.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryGetClassName(IntPtr hWnd, out string className, out string error)
	{
		className = string.Empty;
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		var sb = new StringBuilder(256);
		int read = User32.GetClassName(new HWND(hWnd), sb, sb.Capacity);
		if (read <= 0)
		{
			error = "Failed to retrieve window class name.";
			return false;
		}

		className = sb.ToString();
		return true;
	}

	/// <summary>
	/// Attempts to get the window rectangle and convert to position/size. Returns false with error on failure.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryGetWindowRect(IntPtr hWnd, out Point position, out Size size, out string error)
	{
		position = new Point(0, 0);
		size = new Size(0, 0);
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		if (!User32.GetWindowRect(new HWND(hWnd), out var rect))
		{
			error = "Failed to get window rectangle.";
			return false;
		}

		// RECT is left, top, right, bottom
		int width = Math.Max(0, rect.right - rect.left);
		int height = Math.Max(0, rect.bottom - rect.top);
		position = new Point(rect.left, rect.top);
		size = new Size(width, height);
		return true;
	}

	// === BATCH A2: Enhanced Window Queries ===
	/// <summary>
	/// Attempts to get the process ID that owns a window.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryGetWindowProcessId(IntPtr hWnd, out int processId, out string error)
	{
		processId = 0;
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		try
		{
			// In Vanara 4.x, GetWindowThreadProcessId takes (HWND, out uint) for process ID
			uint pid = 0;
			var threadId = User32.GetWindowThreadProcessId(new HWND(hWnd), out pid);
			if (threadId == 0 || pid == 0)
			{
				error = "Failed to get window process ID.";
				return false;
			}

			processId = (int)pid;
			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to get window process ID: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// Attempts to get the thread ID that owns a window.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryGetWindowThreadId(IntPtr hWnd, out int threadId, out string error)
	{
		threadId = 0;
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		try
		{
			// In Vanara 4.x, GetWindowThreadProcessId takes (HWND, out uint) for process ID
			uint pid = 0;
			var tid = User32.GetWindowThreadProcessId(new HWND(hWnd), out pid);
			if (tid == 0)
			{
				error = "Failed to get window thread ID.";
				return false;
			}

			threadId = (int)tid;
			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to get window thread ID: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// Attempts to get the parent window of a given window.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryGetParentWindow(IntPtr hWnd, out IntPtr parentHwnd, out string error)
	{
		parentHwnd = IntPtr.Zero;
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		var parent = User32.GetParent(new HWND(hWnd));
		if (parent == IntPtr.Zero)
		{
			// Could be no parent or an error
			var lastError = Kernel32.GetLastError();
			if (lastError != 0)
			{
				error = $"Failed to get parent window. Error: {lastError}";
				return false;
			}
			// No parent is valid (desktop window has no parent)
			return true;
		}

		parentHwnd = (IntPtr)parent;
		return true;
	}

	/// <summary>
	/// Attempts to get the owner window of a given window.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryGetOwnerWindow(IntPtr hWnd, out IntPtr ownerHwnd, out string error)
	{
		ownerHwnd = IntPtr.Zero;
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		var owner = User32.GetWindow(new HWND(hWnd), User32.GetWindowCmd.GW_OWNER);
		if (owner == IntPtr.Zero)
		{
			// Could be no owner or an error
			var lastError = Kernel32.GetLastError();
			if (lastError != 0)
			{
				error = $"Failed to get owner window. Error: {lastError}";
				return false;
			}
			// No owner is valid
			return true;
		}

		ownerHwnd = (IntPtr)owner;
		return true;
	}

	/// <summary>
	/// Attempts to enumerate all child windows of a given window.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryGetChildWindows(IntPtr hWnd, out List<IntPtr> childWindows, out string error)
	{
		childWindows = new List<IntPtr>();
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		try
		{
			var childList = new List<IntPtr>();
			User32.EnumChildWindows(new HWND(hWnd), (hwnd, lParam) =>
			{
				childList.Add((IntPtr)hwnd);
				return true; // Continue enumeration
			}, IntPtr.Zero);

			childWindows = childList;
			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to enumerate child windows: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// Checks if a window is minimized.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool IsWindowMinimized(IntPtr hWnd)
	{
		if (!OperatingSystem.IsWindows() || !IsWindow(hWnd))
			return false;

		return User32.IsIconic(new HWND(hWnd));
	}

	/// <summary>
	/// Checks if a window is maximized.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool IsWindowMaximized(IntPtr hWnd)
	{
		if (!OperatingSystem.IsWindows() || !IsWindow(hWnd))
			return false;

		return User32.IsZoomed(new HWND(hWnd));
	}

	/// <summary>
	/// Attempts to get the monitor handle that contains a window.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryGetWindowMonitor(IntPtr hWnd, out IntPtr monitorHandle, out string error)
	{
		monitorHandle = IntPtr.Zero;
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		var monitor = User32.MonitorFromWindow(new HWND(hWnd), User32.MonitorFlags.MONITOR_DEFAULTTONEAREST);
		if (monitor == IntPtr.Zero)
		{
			error = "Failed to get window monitor.";
			return false;
		}

		monitorHandle = (IntPtr)monitor;
		return true;
	}

	/// <summary>
	/// Attempts to get the window's extended style information.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryGetWindowStyle(IntPtr hWnd, out WindowStyle style, out string error)
	{
		style = new WindowStyle();
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		try
		{
			// Use Windows constants directly for window styles
			const int GWL_STYLE = -16;
			const int GWL_EXSTYLE = -20;
			
			var styleFlags = User32.GetWindowLong(new HWND(hWnd), (User32.WindowLongFlags)GWL_STYLE);
			var exStyleFlags = User32.GetWindowLong(new HWND(hWnd), (User32.WindowLongFlags)GWL_EXSTYLE);

			// Parse style flags using Windows constants
			style.HasTitleBar = (styleFlags & 0x00C00000) != 0; // WS_CAPTION
			style.HasSystemMenu = (styleFlags & 0x00080000) != 0; // WS_SYSMENU
			style.CanResize = (styleFlags & 0x00040000) != 0; // WS_THICKFRAME
			style.CanMinimize = (styleFlags & 0x00020000) != 0; // WS_MINIMIZEBOX
			style.CanMaximize = (styleFlags & 0x00010000) != 0; // WS_MAXIMIZEBOX
			style.IsToolWindow = (exStyleFlags & 0x00000080) != 0; // WS_EX_TOOLWINDOW
			style.IsAlwaysOnTop = (exStyleFlags & 0x00000008) != 0; // WS_EX_TOPMOST

			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to get window style: {ex.Message}";
			return false;
		}
	}

	// === BATCH B: Window Manipulation Operations ===
	
	/// <summary>
	/// Attempts to move a window to a specific position.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryMoveWindow(IntPtr hWnd, int x, int y, out string error)
	{
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		try
		{
			// Get current window size to preserve it
			if (!User32.GetWindowRect(new HWND(hWnd), out var rect))
			{
				error = "Failed to get current window rectangle.";
				return false;
			}

			int width = rect.right - rect.left;
			int height = rect.bottom - rect.top;

			// Move window using SetWindowPos
			if (!User32.SetWindowPos(new HWND(hWnd), IntPtr.Zero, x, y, width, height, 
				User32.SetWindowPosFlags.SWP_NOSIZE | User32.SetWindowPosFlags.SWP_NOZORDER))
			{
				error = "Failed to move window.";
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to move window: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// Attempts to resize a window to specific dimensions.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryResizeWindow(IntPtr hWnd, int width, int height, out string error)
	{
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		if (width <= 0 || height <= 0)
		{
			error = "Invalid dimensions. Width and height must be positive.";
			return false;
		}

		try
		{
			// Get current window position to preserve it
			if (!User32.GetWindowRect(new HWND(hWnd), out var rect))
			{
				error = "Failed to get current window rectangle.";
				return false;
			}

			// Resize window using SetWindowPos
			if (!User32.SetWindowPos(new HWND(hWnd), IntPtr.Zero, rect.left, rect.top, width, height, 
				User32.SetWindowPosFlags.SWP_NOMOVE | User32.SetWindowPosFlags.SWP_NOZORDER))
			{
				error = "Failed to resize window.";
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to resize window: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// Attempts to set both position and size of a window in one operation.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TrySetWindowBounds(IntPtr hWnd, int x, int y, int width, int height, out string error)
	{
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		if (width <= 0 || height <= 0)
		{
			error = "Invalid dimensions. Width and height must be positive.";
			return false;
		}

		try
		{
			// Set both position and size using SetWindowPos
			if (!User32.SetWindowPos(new HWND(hWnd), IntPtr.Zero, x, y, width, height, 
				User32.SetWindowPosFlags.SWP_NOZORDER))
			{
				error = "Failed to set window bounds.";
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to set window bounds: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// Attempts to minimize a window.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryMinimizeWindow(IntPtr hWnd, out string error)
	{
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		try
		{
			// Minimize window using ShowWindow
			var result = User32.ShowWindow(new HWND(hWnd), ShowWindowCommand.SW_MINIMIZE);
			if (!result)
			{
				error = "Failed to minimize window.";
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to minimize window: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// Attempts to maximize a window.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryMaximizeWindow(IntPtr hWnd, out string error)
	{
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		try
		{
			// Maximize window using ShowWindow
			var result = User32.ShowWindow(new HWND(hWnd), ShowWindowCommand.SW_MAXIMIZE);
			if (!result)
			{
				error = "Failed to maximize window.";
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to maximize window: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// Attempts to restore a window from minimized or maximized state.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryRestoreWindow(IntPtr hWnd, out string error)
	{
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		try
		{
			// Restore window using ShowWindow
			var result = User32.ShowWindow(new HWND(hWnd), ShowWindowCommand.SW_RESTORE);
			if (!result)
			{
				error = "Failed to restore window.";
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to restore window: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// Attempts to show a window.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryShowWindow(IntPtr hWnd, out string error)
	{
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		try
		{
			// Show window using ShowWindow
			var result = User32.ShowWindow(new HWND(hWnd), ShowWindowCommand.SW_SHOW);
			if (!result)
			{
				error = "Failed to show window.";
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to show window: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// Attempts to hide a window.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryHideWindow(IntPtr hWnd, out string error)
	{
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		try
		{
			// Hide window using ShowWindow
			var result = User32.ShowWindow(new HWND(hWnd), ShowWindowCommand.SW_HIDE);
			if (!result)
			{
				error = "Failed to hide window.";
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to hide window: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// Attempts to bring a window to the front and give it focus.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TryBringWindowToFront(IntPtr hWnd, out string error)
	{
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		try
		{
			// Bring window to front using SetWindowPos
			if (!User32.SetWindowPos(new HWND(hWnd), User32.SpecialWindowHandles.HWND_TOPMOST, 0, 0, 0, 0, 
				User32.SetWindowPosFlags.SWP_NOMOVE | User32.SetWindowPosFlags.SWP_NOSIZE))
			{
				error = "Failed to bring window to front.";
				return false;
			}

			// Remove topmost status to make it normal
			if (!User32.SetWindowPos(new HWND(hWnd), User32.SpecialWindowHandles.HWND_NOTOPMOST, 0, 0, 0, 0, 
				User32.SetWindowPosFlags.SWP_NOMOVE | User32.SetWindowPosFlags.SWP_NOSIZE))
			{
				error = "Failed to remove topmost status.";
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to bring window to front: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// Attempts to set a window as the foreground window.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TrySetForegroundWindow(IntPtr hWnd, out string error)
	{
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		try
		{
			// Set window as foreground
			if (!User32.SetForegroundWindow(new HWND(hWnd)))
			{
				error = "Failed to set foreground window.";
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to set foreground window: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// Attempts to set a window as always-on-top.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TrySetAlwaysOnTop(IntPtr hWnd, bool alwaysOnTop, out string error)
	{
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		try
		{
			var specialHandle = alwaysOnTop ? User32.SpecialWindowHandles.HWND_TOPMOST : User32.SpecialWindowHandles.HWND_NOTOPMOST;

			// Set window topmost status using SetWindowPos
			if (!User32.SetWindowPos(new HWND(hWnd), specialHandle, 0, 0, 0, 0, 
				User32.SetWindowPosFlags.SWP_NOMOVE | User32.SetWindowPosFlags.SWP_NOSIZE))
			{
				error = $"Failed to set window always-on-top to {alwaysOnTop}.";
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to set window always-on-top: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// Attempts to set window transparency (alpha blending).
	/// </summary>
	[SupportedOSPlatform("windows")]
	public static bool TrySetWindowTransparency(IntPtr hWnd, byte alpha, out string error)
	{
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		if (!IsWindow(hWnd))
		{
			error = "Invalid window handle.";
			return false;
		}

		try
		{
			// Set window transparency using SetLayeredWindowAttributes
			// First, we need to make the window layered
			var style = User32.GetWindowLong(new HWND(hWnd), User32.WindowLongFlags.GWL_EXSTYLE);
			if ((style & 0x00080000) == 0) // WS_EX_LAYERED
			{
				User32.SetWindowLong(new HWND(hWnd), User32.WindowLongFlags.GWL_EXSTYLE, style | 0x00080000);
			}

			// Set the transparency
			if (!User32.SetLayeredWindowAttributes(new HWND(hWnd), 0, alpha, User32.LayeredWindowAttributes.LWA_ALPHA))
			{
				error = "Failed to set window transparency.";
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to set window transparency: {ex.Message}";
			return false;
		}
	}
}


