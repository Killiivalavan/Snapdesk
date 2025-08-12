using System;
using System.Collections.Generic;

namespace SnapDesk.Core;

/// <summary>
/// Represents a keyboard shortcut (hotkey) for quick layout operations
/// </summary>
public class HotkeyInfo
{
    /// <summary>
    /// Unique identifier for the hotkey
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The key combination as a string (e.g., "Ctrl+Alt+1")
    /// </summary>
    public string Keys { get; set; } = string.Empty;

    /// <summary>
    /// Individual modifier keys (Ctrl, Shift, Alt, Win)
    /// </summary>
    public List<ModifierKey> Modifiers { get; set; } = new();

    /// <summary>
    /// The main key (e.g., "1", "F1", "A")
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Whether this hotkey is currently enabled
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// What action this hotkey performs
    /// </summary>
    public HotkeyAction Action { get; set; }

    /// <summary>
    /// Optional layout ID if this hotkey is layout-specific
    /// </summary>
    public string? LayoutId { get; set; }

    /// <summary>
    /// When this hotkey was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this hotkey was last modified
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public HotkeyInfo()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsEnabled = true;
    }

    /// <summary>
    /// Constructor with key combination
    /// </summary>
    /// <param name="keys">Key combination string</param>
    /// <param name="action">Action to perform</param>
    public HotkeyInfo(string keys, HotkeyAction action) : this()
    {
        Keys = keys;
        Action = action;
        ParseKeys(keys);
    }

    /// <summary>
    /// Constructor with key combination and layout
    /// </summary>
    /// <param name="keys">Key combination string</param>
    /// <param name="action">Action to perform</param>
    /// <param name="layoutId">Associated layout ID</param>
    public HotkeyInfo(string keys, HotkeyAction action, string layoutId) : this(keys, action)
    {
        LayoutId = layoutId;
    }

    /// <summary>
    /// Parses the key combination string into individual components
    /// </summary>
    /// <param name="keyString">Key combination string to parse</param>
    private void ParseKeys(string keyString)
    {
        var parts = keyString.Split('+');
        
        foreach (var part in parts)
        {
            var trimmedPart = part.Trim();
            
            if (Enum.TryParse<ModifierKey>(trimmedPart, true, out var modifier))
            {
                Modifiers.Add(modifier);
            }
            else
            {
                Key = trimmedPart;
            }
        }
    }

    /// <summary>
    /// Gets a display-friendly representation of the hotkey
    /// </summary>
    /// <returns>Formatted hotkey string</returns>
    public string GetDisplayString()
    {
        if (string.IsNullOrEmpty(Keys))
            return "Not Set";
        
        return Keys;
    }

    /// <summary>
    /// Checks if this hotkey conflicts with another hotkey
    /// </summary>
    /// <param name="other">Other hotkey to check</param>
    /// <returns>True if there's a conflict</returns>
    public bool ConflictsWith(HotkeyInfo other)
    {
        return Keys.Equals(other.Keys, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Possible modifier keys for hotkeys
/// </summary>
public enum ModifierKey
{
    /// <summary>
    /// Control key
    /// </summary>
    Ctrl = 0,

    /// <summary>
    /// Shift key
    /// </summary>
    Shift = 1,

    /// <summary>
    /// Alt key
    /// </summary>
    Alt = 2,

    /// <summary>
    /// Windows key
    /// </summary>
    Win = 3
}

/// <summary>
/// Actions that can be performed by hotkeys
/// </summary>
public enum HotkeyAction
{
    /// <summary>
    /// Save the current desktop layout
    /// </summary>
    SaveLayout = 0,

    /// <summary>
    /// Restore a specific saved layout
    /// </summary>
    RestoreLayout = 1,

    /// <summary>
    /// Show/hide the main SnapDesk window
    /// </summary>
    ToggleMainWindow = 2,

    /// <summary>
    /// Quick save with auto-naming
    /// </summary>
    QuickSave = 3,

    /// <summary>
    /// Cycle through saved layouts
    /// </summary>
    CycleLayouts = 4
}
