using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiteDB;

namespace SnapDesk.Core.Interfaces;

/// <summary>
/// Service interface for managing desktop layout operations
/// </summary>
public interface ILayoutService
{
    /// <summary>
    /// Saves the current desktop layout with the specified name and description
    /// </summary>
    /// <param name="name">Name for the layout</param>
    /// <param name="description">Optional description</param>
    /// <returns>Saved layout profile</returns>
    Task<LayoutProfile> SaveCurrentLayoutAsync(string name, string? description = null);

    /// <summary>
    /// Saves an existing layout profile
    /// </summary>
    /// <param name="layout">Layout to save</param>
    /// <returns>Saved layout profile</returns>
    Task<LayoutProfile> SaveLayoutAsync(LayoutProfile layout);

    /// <summary>
    /// Retrieves a layout by its unique identifier
    /// </summary>
    /// <param name="id">Layout ID</param>
    /// <returns>Layout profile if found, null otherwise</returns>
    Task<LayoutProfile?> GetLayoutAsync(ObjectId id);

    /// <summary>
    /// Retrieves all saved layouts
    /// </summary>
    /// <returns>Collection of all layout profiles</returns>
    Task<IEnumerable<LayoutProfile>> GetAllLayoutsAsync();

    /// <summary>
    /// Retrieves layouts by name (partial match)
    /// </summary>
    /// <param name="name">Name to search for</param>
    /// <returns>Matching layout profiles</returns>
    Task<IEnumerable<LayoutProfile>> GetLayoutsByNameAsync(string name);

    /// <summary>
    /// Restores a saved layout to the current desktop
    /// </summary>
    /// <param name="id">Layout ID to restore</param>
    /// <param name="options">Restoration options</param>
    /// <returns>True if restoration was successful</returns>
    Task<bool> RestoreLayoutAsync(ObjectId id, RestoreOptions? options = null);

    /// <summary>
    /// Deletes a saved layout
    /// </summary>
    /// <param name="id">Layout ID to delete</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteLayoutAsync(ObjectId id);

    /// <summary>
    /// Updates an existing layout
    /// </summary>
    /// <param name="layout">Layout to update</param>
    /// <returns>Updated layout profile</returns>
    Task<LayoutProfile> UpdateLayoutAsync(LayoutProfile layout);

    /// <summary>
    /// Activates a layout (sets it as the current active layout)
    /// </summary>
    /// <param name="id">Layout ID to activate</param>
    /// <returns>True if activation was successful</returns>
    Task<bool> ActivateLayoutAsync(ObjectId id);

    /// <summary>
    /// Gets the currently active layout
    /// </summary>
    /// <returns>Active layout profile if any, null otherwise</returns>
    Task<LayoutProfile?> GetActiveLayoutAsync();

    /// <summary>
    /// Exports a layout to a file
    /// </summary>
    /// <param name="id">Layout ID to export</param>
    /// <param name="filePath">Export file path</param>
    /// <returns>True if export was successful</returns>
    Task<bool> ExportLayoutAsync(ObjectId id, string filePath);

    /// <summary>
    /// Imports a layout from a file
    /// </summary>
    /// <param name="filePath">Import file path</param>
    /// <returns>Imported layout profile</returns>
    Task<LayoutProfile> ImportLayoutAsync(string filePath);

    /// <summary>
    /// Duplicates an existing layout
    /// </summary>
    /// <param name="id">Layout ID to duplicate</param>
    /// <param name="newName">Name for the duplicate</param>
    /// <returns>New duplicated layout profile</returns>
    Task<LayoutProfile> DuplicateLayoutAsync(ObjectId id, string newName);

    /// <summary>
    /// Validates if a layout can be restored
    /// </summary>
    /// <param name="id">Layout ID to validate</param>
    /// <returns>Validation result with details</returns>
    Task<LayoutValidationResult> ValidateLayoutAsync(ObjectId id);
}

/// <summary>
/// Options for layout restoration
/// </summary>
public class RestoreOptions
{
    /// <summary>
    /// Whether to restore window positions
    /// </summary>
    public bool RestorePositions { get; set; } = true;

    /// <summary>
    /// Whether to restore window sizes
    /// </summary>
    public bool RestoreSizes { get; set; } = true;

    /// <summary>
    /// Whether to restore window states (minimized, maximized, etc.)
    /// </summary>
    public bool RestoreStates { get; set; } = true;

    /// <summary>
    /// Whether to restore window visibility
    /// </summary>
    public bool RestoreVisibility { get; set; } = true;

    /// <summary>
    /// Whether to restore window Z-order (stacking order)
    /// </summary>
    public bool RestoreZOrder { get; set; } = true;

    /// <summary>
    /// Whether to force restoration even if windows don't match
    /// </summary>
    public bool ForceRestore { get; set; } = false;

    /// <summary>
    /// Timeout for restoration operations in milliseconds
    /// </summary>
    public int TimeoutMs { get; set; } = 10000;
}

/// <summary>
/// Result of layout validation
/// </summary>
public class LayoutValidationResult
{
    /// <summary>
    /// Whether the layout is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Validation errors if any
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Validation warnings if any
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Whether the layout can be restored
    /// </summary>
    public bool CanBeRestored { get; set; }

    /// <summary>
    /// Number of windows that can be restored
    /// </summary>
    public int RestorableWindowCount { get; set; }

    /// <summary>
    /// Number of monitors that can be restored
    /// </summary>
    public int RestorableMonitorCount { get; set; }
}
