using System;

namespace SnapDesk.Platform.Interfaces;

/// <summary>
/// Cross-platform interface for global hotkey operations. This interface can be implemented
/// on different platforms (Windows, macOS, Linux) to provide platform-specific
/// global hotkey registration and event handling capabilities.
/// </summary>
public interface IHotkeyApi
{
	/// <summary>
	/// Attempts to register a global hotkey that will work system-wide.
	/// </summary>
	/// <param name="id">Unique identifier for the hotkey (0x0000-0xBFFF range)</param>
	/// <param name="modifiers">Modifier keys that must be pressed (Ctrl, Alt, Shift, Win)</param>
	/// <param name="virtualKey">Virtual key code for the main key</param>
	/// <param name="error">Error message if registration fails</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryRegisterHotkey(int id, HotkeyModifiers modifiers, int virtualKey, out string error);

	/// <summary>
	/// Attempts to unregister a previously registered global hotkey.
	/// </summary>
	/// <param name="id">Identifier of the hotkey to unregister</param>
	/// <param name="error">Error message if unregistration fails</param>
	/// <returns>True if successful, false otherwise</returns>
	bool TryUnregisterHotkey(int id, out string error);

	/// <summary>
	/// Checks if a hotkey with the specified ID is currently registered.
	/// </summary>
	/// <param name="id">Hotkey identifier to check</param>
	/// <returns>True if the hotkey is registered, false otherwise</returns>
	bool IsHotkeyRegistered(int id);

	/// <summary>
	/// Gets information about the current hotkey registration status.
	/// </summary>
	/// <returns>Hotkey system information including registered count and platform details</returns>
	HotkeySystemInfo GetSystemInfo();
}

/// <summary>
/// Modifier keys that can be combined with the main key for hotkey registration.
/// </summary>
[Flags]
public enum HotkeyModifiers
{
	/// <summary>No modifier keys</summary>
	None = 0x0000,
	
	/// <summary>Either ALT key must be held down</summary>
	Alt = 0x0001,
	
	/// <summary>Either CTRL key must be held down</summary>
	Control = 0x0002,
	
	/// <summary>Either SHIFT key must be held down</summary>
	Shift = 0x0004,
	
	/// <summary>Either WINDOWS key must be held down (reserved by OS)</summary>
	Win = 0x0008,
	
	/// <summary>Prevent keyboard auto-repeat from generating multiple notifications</summary>
	NoRepeat = 0x4000
}

/// <summary>
/// Information about the hotkey system status and capabilities.
/// </summary>
public class HotkeySystemInfo
{
	/// <summary>Number of currently registered hotkeys</summary>
	public int RegisteredHotkeyCount { get; set; }
	
	/// <summary>Maximum number of hotkeys that can be registered</summary>
	public int MaxHotkeyCount { get; set; }
	
	/// <summary>Whether the platform supports global hotkeys</summary>
	public bool SupportsGlobalHotkeys { get; set; }
	
	/// <summary>Platform-specific description</summary>
	public string PlatformDescription { get; set; } = string.Empty;
	
	/// <summary>Any limitations or restrictions on this platform</summary>
	public string Limitations { get; set; } = string.Empty;
}
