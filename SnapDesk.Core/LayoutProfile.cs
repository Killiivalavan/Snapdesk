using System;
using System.Collections.Generic;

namespace SnapDesk.Core;

/// <summary>
/// Represents a saved desktop layout configuration
/// </summary>
public class LayoutProfile
{
    /// <summary>
    /// Unique identifier for the layout
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User-friendly name for the layout (e.g., "Coding Setup", "Design Work")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of what this layout is used for
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When this layout was first created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this layout was last modified
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Whether this layout is currently active/selected
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// List of windows in this layout with their positions and states
    /// </summary>
    public List<WindowInfo> Windows { get; set; } = new();

    /// <summary>
    /// Monitor configuration when this layout was saved
    /// </summary>
    public List<MonitorInfo> MonitorConfiguration { get; set; } = new();

    /// <summary>
    /// Associated hotkey for quick restoration
    /// </summary>
    public HotkeyInfo? Hotkey { get; set; }

    /// <summary>
    /// Constructor that sets default values
    /// </summary>
    public LayoutProfile()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Constructor with name
    /// </summary>
    /// <param name="name">Name of the layout</param>
    public LayoutProfile(string name) : this()
    {
        Name = name;
    }

    /// <summary>
    /// Constructor with name and description
    /// </summary>
    /// <param name="name">Name of the layout</param>
    /// <param name="description">Description of the layout</param>
    public LayoutProfile(string name, string? description) : this(name)
    {
        Description = description;
    }
}
