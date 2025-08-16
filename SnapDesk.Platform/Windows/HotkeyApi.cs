using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using Vanara.PInvoke;
using SnapDesk.Platform.Interfaces;

namespace SnapDesk.Platform.Windows;

/// <summary>
/// Windows-specific global hotkey API implementation using Win32 RegisterHotKey and UnregisterHotKey.
/// This class provides safe wrappers around the Windows hotkey registration system.
/// </summary>
[SupportedOSPlatform("windows")]
public static class HotkeyApi
{
	// Track registered hotkeys for management and cleanup
	private static readonly HashSet<int> _registeredHotkeys = new();
	private static readonly object _lockObject = new();

	// Windows message constant for hotkey events
	private const int WM_HOTKEY = 0x0312;

	/// <summary>
	/// Attempts to register a global hotkey that will work system-wide.
	/// </summary>
	/// <param name="id">Unique identifier for the hotkey (0x0000-0xBFFF range)</param>
	/// <param name="modifiers">Modifier keys that must be pressed</param>
	/// <param name="virtualKey">Virtual key code for the main key</param>
	/// <param name="error">Error message if registration fails</param>
	/// <returns>True if successful, false otherwise</returns>
	[SupportedOSPlatform("windows")]
	public static bool TryRegisterHotkey(int id, HotkeyModifiers modifiers, int virtualKey, out string error)
	{
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		// Validate parameters
		if (id < 0 || id > 0xBFFF)
		{
			error = "Hotkey ID must be in range 0x0000-0xBFFF.";
			return false;
		}

		if (virtualKey <= 0 || virtualKey > 0xFF)
		{
			error = "Invalid virtual key code. Must be in range 0x01-0xFF (1-255).";
			return false;
		}

		// Check if already registered
		lock (_lockObject)
		{
			if (_registeredHotkeys.Contains(id))
			{
				error = $"Hotkey with ID {id} is already registered.";
				return false;
			}
		}

		try
		{
			// Convert our HotkeyModifiers enum to Win32 constants
			uint win32Modifiers = ConvertToWin32Modifiers(modifiers);

			// Register the hotkey using Win32 API
			bool success = User32.RegisterHotKey(IntPtr.Zero, id, (User32.HotKeyModifiers)win32Modifiers, (uint)virtualKey);
			
			if (!success)
			{
				// Get detailed error information
				var lastError = Kernel32.GetLastError();
				error = GetHotkeyErrorMessage((uint)lastError);
				return false;
			}

			// Track successful registration
			lock (_lockObject)
			{
				_registeredHotkeys.Add(id);
			}

			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to register hotkey: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// Attempts to unregister a previously registered global hotkey.
	/// </summary>
	/// <param name="id">Identifier of the hotkey to unregister</param>
	/// <param name="error">Error message if unregistration fails</param>
	/// <returns>True if successful, false otherwise</returns>
	[SupportedOSPlatform("windows")]
	public static bool TryUnregisterHotkey(int id, out string error)
	{
		error = string.Empty;

		if (!OperatingSystem.IsWindows())
		{
			error = "Platform not supported.";
			return false;
		}

		// Check if actually registered
		lock (_lockObject)
		{
			if (!_registeredHotkeys.Contains(id))
			{
				error = $"Hotkey with ID {id} is not registered.";
				return false;
			}
		}

		try
		{
			// Unregister the hotkey using Win32 API
			bool success = User32.UnregisterHotKey(IntPtr.Zero, id);
			
			if (!success)
			{
				// Get detailed error information
				var lastError = Kernel32.GetLastError();
				error = GetHotkeyErrorMessage((uint)lastError);
				return false;
			}

			// Remove from tracking
			lock (_lockObject)
			{
				_registeredHotkeys.Remove(id);
			}

			return true;
		}
		catch (Exception ex)
		{
			error = $"Failed to unregister hotkey: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// Checks if a hotkey with the specified ID is currently registered.
	/// </summary>
	/// <param name="id">Hotkey identifier to check</param>
	/// <returns>True if the hotkey is registered, false otherwise</returns>
	[SupportedOSPlatform("windows")]
	public static bool IsHotkeyRegistered(int id)
	{
		if (!OperatingSystem.IsWindows())
			return false;

		lock (_lockObject)
		{
			return _registeredHotkeys.Contains(id);
		}
	}

	/// <summary>
	/// Gets information about the current hotkey registration status.
	/// </summary>
	/// <returns>Hotkey system information including registered count and platform details</returns>
	[SupportedOSPlatform("windows")]
	public static HotkeySystemInfo GetSystemInfo()
	{
		if (!OperatingSystem.IsWindows())
		{
			return new HotkeySystemInfo
			{
				SupportsGlobalHotkeys = false,
				PlatformDescription = "Windows - Not supported on this platform",
				Limitations = "Global hotkeys are not available on this platform."
			};
		}

		lock (_lockObject)
		{
			return new HotkeySystemInfo
			{
				RegisteredHotkeyCount = _registeredHotkeys.Count,
				MaxHotkeyCount = 0xBFFF, // Windows limit
				SupportsGlobalHotkeys = true,
				PlatformDescription = "Windows - Full global hotkey support available",
				Limitations = "F12 key is reserved by Windows. Win key combinations are reserved by the OS."
			};
		}
	}

	/// <summary>
	/// Unregisters all currently registered hotkeys. This is important for cleanup.
	/// </summary>
	/// <returns>Number of hotkeys successfully unregistered</returns>
	[SupportedOSPlatform("windows")]
	public static int UnregisterAllHotkeys()
	{
		if (!OperatingSystem.IsWindows())
			return 0;

		int unregisteredCount = 0;
		var hotkeysToRemove = new List<int>();

		lock (_lockObject)
		{
			hotkeysToRemove.AddRange(_registeredHotkeys);
		}

		foreach (int id in hotkeysToRemove)
		{
			if (TryUnregisterHotkey(id, out _))
			{
				unregisteredCount++;
			}
		}

		return unregisteredCount;
	}

	/// <summary>
	/// Converts our HotkeyModifiers enum to Win32 constants.
	/// </summary>
	private static uint ConvertToWin32Modifiers(HotkeyModifiers modifiers)
	{
		uint result = 0;

		if ((modifiers & HotkeyModifiers.Alt) != 0)
			result |= 0x0001; // MOD_ALT
		if ((modifiers & HotkeyModifiers.Control) != 0)
			result |= 0x0002; // MOD_CONTROL
		if ((modifiers & HotkeyModifiers.Shift) != 0)
			result |= 0x0004; // MOD_SHIFT
		if ((modifiers & HotkeyModifiers.Win) != 0)
			result |= 0x0008; // MOD_WIN
		if ((modifiers & HotkeyModifiers.NoRepeat) != 0)
			result |= 0x4000; // MOD_NOREPEAT

		return result;
	}

	/// <summary>
	/// Gets a human-readable error message for Win32 error codes related to hotkey operations.
	/// </summary>
	private static string GetHotkeyErrorMessage(uint errorCode)
	{
		return errorCode switch
		{
			0x00000000 => "Operation completed successfully.",
			0x00000005 => "Access denied. The hotkey may be reserved by the system.",
			0x00000057 => "Invalid parameter. Check hotkey ID and modifier combinations.",
			0x000000B7 => "Cannot create a file when that file already exists. Hotkey ID conflict.",
			_ => $"Win32 error {errorCode:X8}. Use GetLastError() for more details."
		};
	}
}
