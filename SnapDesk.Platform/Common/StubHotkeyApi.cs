using SnapDesk.Platform.Interfaces;

namespace SnapDesk.Platform.Common;

/// <summary>
/// Cross-platform stub implementation of IHotkeyApi that provides safe defaults
/// on platforms that don't support global hotkeys. This prevents crashes
/// and allows the application to run on any platform.
/// </summary>
public class StubHotkeyApi : IHotkeyApi
{
	/// <summary>
	/// Attempts to register a global hotkey that will work system-wide.
	/// </summary>
	/// <param name="id">Unique identifier for the hotkey</param>
	/// <param name="modifiers">Modifier keys that must be pressed</param>
	/// <param name="virtualKey">Virtual key code for the main key</param>
	/// <param name="error">Error message indicating platform limitation</param>
	/// <returns>Always false on unsupported platforms</returns>
	public bool TryRegisterHotkey(int id, HotkeyModifiers modifiers, int virtualKey, out string error)
	{
		error = "Global hotkeys are not supported on this platform.";
		return false;
	}

	/// <summary>
	/// Attempts to unregister a previously registered global hotkey.
	/// </summary>
	/// <param name="id">Identifier of the hotkey to unregister</param>
	/// <param name="error">Error message indicating platform limitation</param>
	/// <returns>Always false on unsupported platforms</returns>
	public bool TryUnregisterHotkey(int id, out string error)
	{
		error = "Global hotkeys are not supported on this platform.";
		return false;
	}

	/// <summary>
	/// Checks if a hotkey with the specified ID is currently registered.
	/// </summary>
	/// <param name="id">Hotkey identifier to check</param>
	/// <returns>Always false on unsupported platforms</returns>
	public bool IsHotkeyRegistered(int id)
	{
		return false;
	}

	/// <summary>
	/// Gets information about the current hotkey registration status.
	/// </summary>
	/// <returns>Hotkey system information indicating platform limitations</returns>
	public HotkeySystemInfo GetSystemInfo()
	{
		return new HotkeySystemInfo
		{
			RegisteredHotkeyCount = 0,
			MaxHotkeyCount = 0,
			SupportsGlobalHotkeys = false,
			PlatformDescription = "Cross-platform - Global hotkeys not supported",
			Limitations = "This platform does not support system-wide hotkey registration. Hotkey functionality will be disabled."
		};
	}
}
