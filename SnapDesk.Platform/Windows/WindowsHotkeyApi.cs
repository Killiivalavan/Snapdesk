using System;
using System.Runtime.Versioning;
using SnapDesk.Platform.Interfaces;

namespace SnapDesk.Platform.Windows;

/// <summary>
/// Windows implementation of the IHotkeyApi interface. This class provides
/// Windows-specific global hotkey management capabilities using the Win32 API.
/// </summary>
[SupportedOSPlatform("windows")]
public class WindowsHotkeyApi : IHotkeyApi
{
	/// <summary>
	/// Attempts to register a global hotkey that will work system-wide.
	/// </summary>
	/// <param name="id">Unique identifier for the hotkey (0x0000-0xBFFF range)</param>
	/// <param name="modifiers">Modifier keys that must be pressed</param>
	/// <param name="virtualKey">Virtual key code for the main key</param>
	/// <param name="error">Error message if registration fails</param>
	/// <returns>True if successful, false otherwise</returns>
	public bool TryRegisterHotkey(int id, HotkeyModifiers modifiers, int virtualKey, out string error)
	{
		return HotkeyApi.TryRegisterHotkey(id, modifiers, virtualKey, out error);
	}

	/// <summary>
	/// Attempts to unregister a previously registered global hotkey.
	/// </summary>
	/// <param name="id">Identifier of the hotkey to unregister</param>
	/// <param name="error">Error message if unregistration fails</param>
	/// <returns>True if successful, false otherwise</returns>
	public bool TryUnregisterHotkey(int id, out string error)
	{
		return HotkeyApi.TryUnregisterHotkey(id, out error);
	}

	/// <summary>
	/// Checks if a hotkey with the specified ID is currently registered.
	/// </summary>
	/// <param name="id">Hotkey identifier to check</param>
	/// <returns>True if the hotkey is registered, false otherwise</returns>
	public bool IsHotkeyRegistered(int id)
	{
		return HotkeyApi.IsHotkeyRegistered(id);
	}

	/// <summary>
	/// Gets information about the current hotkey registration status.
	/// </summary>
	/// <returns>Hotkey system information including registered count and platform details</returns>
	public HotkeySystemInfo GetSystemInfo()
	{
		return HotkeyApi.GetSystemInfo();
	}

	/// <summary>
	/// Unregisters all currently registered hotkeys. This is important for cleanup.
	/// </summary>
	/// <returns>Number of hotkeys successfully unregistered</returns>
	public int UnregisterAllHotkeys()
	{
		return HotkeyApi.UnregisterAllHotkeys();
	}
}
