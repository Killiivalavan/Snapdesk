using System;
using SnapDesk.Platform.Interfaces;
using SnapDesk.Platform.Windows;
using SnapDesk.Platform.Common;

namespace SnapDesk.Platform;

/// <summary>
/// Factory class that creates platform-specific implementations of services.
/// This allows the application to work on different operating systems while
/// providing the best available functionality for each platform.
/// </summary>
public static class PlatformFactory
{
	/// <summary>
	/// Creates the appropriate window API implementation for the current platform.
	/// </summary>
	/// <returns>Platform-specific window API implementation</returns>
	public static IWindowApi CreateWindowApi()
	{
		if (OperatingSystem.IsWindows())
		{
			return new WindowsWindowApi();
		}
		else
		{
			// Return stub implementation for non-Windows platforms
			// This prevents crashes and allows the app to run anywhere
			return new StubWindowApi();
		}
	}

	/// <summary>
	/// Creates the appropriate hotkey API implementation for the current platform.
	/// </summary>
	/// <returns>Platform-specific hotkey API implementation</returns>
	public static IHotkeyApi CreateHotkeyApi()
	{
		if (OperatingSystem.IsWindows())
		{
			return new WindowsHotkeyApi();
		}
		else
		{
			// Return stub implementation for non-Windows platforms
			// This prevents crashes and allows the app to run anywhere
			return new StubHotkeyApi();
		}
	}

	/// <summary>
	/// Gets information about the current platform and its capabilities.
	/// </summary>
	/// <returns>Platform information object</returns>
	public static PlatformInfo GetPlatformInfo()
	{
		return new PlatformInfo
		{
			OperatingSystem = GetOperatingSystemName(),
			Description = GetPlatformDescription(),
			SupportsWindowManagement = OperatingSystem.IsWindows(),
			SupportsGlobalHotkeys = OperatingSystem.IsWindows(),
			IsWindows = OperatingSystem.IsWindows(),
			IsMacOS = OperatingSystem.IsMacOS(),
			IsLinux = OperatingSystem.IsLinux()
		};
	}

	/// <summary>
	/// Gets a human-readable name for the current operating system.
	/// </summary>
	/// <returns>Operating system name</returns>
	private static string GetOperatingSystemName()
	{
		if (OperatingSystem.IsWindows())
			return "Windows";
		else if (OperatingSystem.IsMacOS())
			return "macOS";
		else if (OperatingSystem.IsLinux())
			return "Linux";
		else
			return "Unknown";
	}

	/// <summary>
	/// Gets a detailed description of the current platform and its capabilities.
	/// </summary>
	/// <returns>Platform description</returns>
	private static string GetPlatformDescription()
	{
		if (OperatingSystem.IsWindows())
		{
			return "Windows - Full window management and global hotkey support available";
		}
		else if (OperatingSystem.IsMacOS())
		{
			return "macOS - Limited functionality (stub implementations), future AppleScript support planned";
		}
		else if (OperatingSystem.IsLinux())
		{
			return "Linux - Limited functionality (stub implementations), future X11/Wayland support planned";
		}
		else
		{
			return "Unknown Platform - Limited functionality, safe defaults provided";
		}
	}
}

/// <summary>
/// Information about the current platform and its capabilities.
/// </summary>
public class PlatformInfo
{
	/// <summary>
	/// Name of the operating system.
	/// </summary>
	public string OperatingSystem { get; set; } = string.Empty;

	/// <summary>
	/// Detailed description of the platform and its capabilities.
	/// </summary>
	public string Description { get; set; } = string.Empty;

	/// <summary>
	/// Whether the platform supports window management operations.
	/// </summary>
	public bool SupportsWindowManagement { get; set; }

	/// <summary>
	/// Whether the platform supports global hotkey registration.
	/// </summary>
	public bool SupportsGlobalHotkeys { get; set; }

	/// <summary>
	/// Whether the current platform is Windows.
	/// </summary>
	public bool IsWindows { get; set; }

	/// <summary>
	/// Whether the current platform is macOS.
	/// </summary>
	public bool IsMacOS { get; set; }

	/// <summary>
	/// Whether the current platform is Linux.
	/// </summary>
	public bool IsLinux { get; set; }
}
