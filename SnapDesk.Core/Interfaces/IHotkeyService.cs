using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiteDB;

namespace SnapDesk.Core.Interfaces;

/// <summary>
/// Service interface for managing global hotkey operations
/// </summary>
public interface IHotkeyService
{
    /// <summary>
    /// Registers a global hotkey with a callback function
    /// </summary>
    /// <param name="hotkey">Hotkey information</param>
    /// <param name="callback">Function to execute when hotkey is pressed</param>
    /// <returns>True if registration was successful</returns>
    Task<bool> RegisterHotkeyAsync(HotkeyInfo hotkey, Func<Task> callback);

    /// <summary>
    /// Registers a global hotkey with a synchronous callback function
    /// </summary>
    /// <param name="hotkey">Hotkey information</param>
    /// <param name="callback">Function to execute when hotkey is pressed</param>
    /// <returns>True if registration was successful</returns>
    Task<bool> RegisterHotkeyAsync(HotkeyInfo hotkey, Action callback);

    /// <summary>
    /// Unregisters a previously registered hotkey
    /// </summary>
    /// <param name="hotkeyId">ID of the hotkey to unregister</param>
    /// <returns>True if unregistration was successful</returns>
    Task<bool> UnregisterHotkeyAsync(ObjectId hotkeyId);

    /// <summary>
    /// Unregisters a hotkey by its key combination
    /// </summary>
    /// <param name="keyCombination">Key combination string (e.g., "Ctrl+Alt+1")</param>
    /// <returns>True if unregistration was successful</returns>
    Task<bool> UnregisterHotkeyByKeysAsync(string keyCombination);

    /// <summary>
    /// Checks if a hotkey combination is available for registration
    /// </summary>
    /// <param name="keyCombination">Key combination to check</param>
    /// <returns>True if the hotkey is available</returns>
    Task<bool> IsHotkeyAvailableAsync(string keyCombination);

    /// <summary>
    /// Gets all currently registered hotkeys
    /// </summary>
    /// <returns>Collection of registered hotkeys</returns>
    Task<IEnumerable<HotkeyInfo>> GetRegisteredHotkeysAsync();

    /// <summary>
    /// Gets a specific hotkey by ID
    /// </summary>
    /// <param name="hotkeyId">Hotkey ID</param>
    /// <returns>Hotkey information if found, null otherwise</returns>
    Task<HotkeyInfo?> GetHotkeyAsync(ObjectId hotkeyId);

    /// <summary>
    /// Gets hotkeys by action type
    /// </summary>
    /// <param name="action">Action type to filter by</param>
    /// <returns>Collection of hotkeys for the specified action</returns>
    Task<IEnumerable<HotkeyInfo>> GetHotkeysByActionAsync(HotkeyAction action);

    /// <summary>
    /// Gets hotkeys associated with a specific layout
    /// </summary>
    /// <param name="layoutId">Layout ID</param>
    /// <returns>Collection of hotkeys for the specified layout</returns>
    Task<IEnumerable<HotkeyInfo>> GetHotkeysByLayoutAsync(ObjectId layoutId);

    /// <summary>
    /// Updates an existing hotkey
    /// </summary>
    /// <param name="hotkey">Updated hotkey information</param>
    /// <returns>True if update was successful</returns>
    Task<bool> UpdateHotkeyAsync(HotkeyInfo hotkey);

    /// <summary>
    /// Enables a previously disabled hotkey
    /// </summary>
    /// <param name="hotkeyId">Hotkey ID to enable</param>
    /// <returns>True if enable was successful</returns>
    Task<bool> EnableHotkeyAsync(ObjectId hotkeyId);

    /// <summary>
    /// Disables a hotkey (temporarily prevents it from working)
    /// </summary>
    /// <param name="hotkeyId">Hotkey ID to disable</param>
    /// <returns>True if disable was successful</returns>
    Task<bool> DisableHotkeyAsync(ObjectId hotkeyId);

    /// <summary>
    /// Changes the key combination for an existing hotkey
    /// </summary>
    /// <param name="hotkeyId">Hotkey ID</param>
    /// <param name="newKeyCombination">New key combination</param>
    /// <returns>True if change was successful</returns>
    Task<bool> ChangeHotkeyKeysAsync(ObjectId hotkeyId, string newKeyCombination);

    /// <summary>
    /// Associates a hotkey with a layout
    /// </summary>
    /// <param name="hotkeyId">Hotkey ID</param>
    /// <param name="layoutId">Layout ID to associate with</param>
    /// <returns>True if association was successful</returns>
    Task<bool> AssociateHotkeyWithLayoutAsync(ObjectId hotkeyId, ObjectId layoutId);

    /// <summary>
    /// Removes the layout association from a hotkey
    /// </summary>
    /// <param name="hotkeyId">Hotkey ID</param>
    /// <returns>True if removal was successful</returns>
    Task<bool> RemoveHotkeyLayoutAssociationAsync(ObjectId hotkeyId);

    /// <summary>
    /// Checks if a hotkey is currently active/enabled
    /// </summary>
    /// <param name="hotkeyId">Hotkey ID</param>
    /// <returns>True if hotkey is active</returns>
    Task<bool> IsHotkeyActiveAsync(ObjectId hotkeyId);

    /// <summary>
    /// Gets hotkey conflicts (multiple hotkeys with same key combination)
    /// </summary>
    /// <returns>Collection of hotkey conflicts</returns>
    Task<IEnumerable<HotkeyConflict>> GetHotkeyConflictsAsync();

    /// <summary>
    /// Resolves a hotkey conflict by choosing one hotkey over another
    /// </summary>
    /// <param name="conflict">Conflict to resolve</param>
    /// <param name="preferredHotkeyId">ID of the preferred hotkey</param>
    /// <returns>True if conflict was resolved</returns>
    Task<bool> ResolveHotkeyConflictAsync(HotkeyConflict conflict, ObjectId preferredHotkeyId);

    /// <summary>
    /// Gets hotkey usage statistics
    /// </summary>
    /// <returns>Statistics about hotkey usage</returns>
    Task<HotkeyStatistics> GetHotkeyStatisticsAsync();

    /// <summary>
    /// Records hotkey usage for statistics
    /// </summary>
    /// <param name="hotkeyId">Hotkey ID that was used</param>
    /// <returns>True if recording was successful</returns>
    Task<bool> RecordHotkeyUsageAsync(ObjectId hotkeyId);

    /// <summary>
    /// Validates hotkey configuration
    /// </summary>
    /// <param name="hotkey">Hotkey to validate</param>
    /// <returns>Validation result with details</returns>
    Task<HotkeyValidationResult> ValidateHotkeyAsync(HotkeyInfo hotkey);

    /// <summary>
    /// Gets system-wide hotkey information
    /// </summary>
    /// <returns>Information about system hotkey support</returns>
    Task<SystemHotkeyInfo> GetSystemHotkeyInfoAsync();

    /// <summary>
    /// Refreshes all hotkey registrations
    /// </summary>
    /// <returns>True if refresh was successful</returns>
    Task<bool> RefreshHotkeysAsync();

    /// <summary>
    /// Suspends all hotkey processing temporarily
    /// </summary>
    /// <returns>True if suspension was successful</returns>
    Task<bool> SuspendHotkeysAsync();

    /// <summary>
    /// Resumes hotkey processing after suspension
    /// </summary>
    /// <returns>True if resumption was successful</returns>
    Task<bool> ResumeHotkeysAsync();

    /// <summary>
    /// Resets the hotkey ID counter (useful for testing)
    /// </summary>
    void ResetHotkeyIdCounter();

    /// <summary>
    /// Clears all registered hotkeys and resets internal state (useful for testing)
    /// </summary>
    Task ClearAllHotkeysAsync();
}

/// <summary>
/// Information about hotkey conflicts
/// </summary>
public class HotkeyConflict
{
    /// <summary>
    /// Key combination that has conflicts
    /// </summary>
    public string KeyCombination { get; set; } = string.Empty;

    /// <summary>
    /// Hotkeys involved in the conflict
    /// </summary>
    public List<HotkeyInfo> ConflictingHotkeys { get; set; } = new();

    /// <summary>
    /// Whether this conflict has been resolved
    /// </summary>
    public bool IsResolved { get; set; }

    /// <summary>
    /// Resolution method used
    /// </summary>
    public string? ResolutionMethod { get; set; }

    /// <summary>
    /// When this conflict was detected
    /// </summary>
    public DateTime DetectedAt { get; set; }

    /// <summary>
    /// When this conflict was resolved (if applicable)
    /// </summary>
    public DateTime? ResolvedAt { get; set; }
}

/// <summary>
/// Statistics about hotkey usage
/// </summary>
public class HotkeyStatistics
{
    /// <summary>
    /// Total number of registered hotkeys
    /// </summary>
    public int TotalHotkeys { get; set; }

    /// <summary>
    /// Number of active hotkeys
    /// </summary>
    public int ActiveHotkeys { get; set; }

    /// <summary>
    /// Number of disabled hotkeys
    /// </summary>
    public int DisabledHotkeys { get; set; }

    /// <summary>
    /// Total number of hotkey presses recorded
    /// </summary>
    public long TotalPresses { get; set; }

    /// <summary>
    /// Most frequently used hotkey
    /// </summary>
    public string? MostUsedHotkey { get; set; }

    /// <summary>
    /// Number of hotkey conflicts detected
    /// </summary>
    public int ConflictsDetected { get; set; }

    /// <summary>
    /// Number of hotkey conflicts resolved
    /// </summary>
    public int ConflictsResolved { get; set; }

    /// <summary>
    /// Average response time for hotkey processing in milliseconds
    /// </summary>
    public double AverageResponseTimeMs { get; set; }

    /// <summary>
    /// Number of failed hotkey registrations
    /// </summary>
    public int FailedRegistrations { get; set; }

    /// <summary>
    /// Number of hotkeys associated with layouts
    /// </summary>
    public int LayoutAssociatedHotkeys { get; set; }
}

/// <summary>
/// Result of hotkey validation
/// </summary>
public class HotkeyValidationResult
{
    /// <summary>
    /// Whether the hotkey is valid
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
    /// Whether the hotkey can be registered
    /// </summary>
    public bool CanBeRegistered { get; set; }

    /// <summary>
    /// Whether the hotkey conflicts with existing hotkeys
    /// </summary>
    public bool HasConflicts { get; set; }

    /// <summary>
    /// List of conflicting hotkeys if any
    /// </summary>
    public List<ObjectId> ConflictingHotkeyIds { get; set; } = new();

    /// <summary>
    /// Whether the key combination is supported by the system
    /// </summary>
    public bool IsSystemSupported { get; set; }
}

/// <summary>
/// Information about system hotkey support
/// </summary>
public class SystemHotkeyInfo
{
    /// <summary>
    /// Whether global hotkeys are supported
    /// </summary>
    public bool GlobalHotkeysSupported { get; set; }

    /// <summary>
    /// Maximum number of hotkeys that can be registered
    /// </summary>
    public int MaxHotkeysSupported { get; set; }

    /// <summary>
    /// Whether the system supports the current hotkey registrations
    /// </summary>
    public bool CurrentRegistrationsSupported { get; set; }

    /// <summary>
    /// System-specific limitations or notes
    /// </summary>
    public string? SystemLimitations { get; set; }

    /// <summary>
    /// Whether hotkeys are currently suspended
    /// </summary>
    public bool HotkeysSuspended { get; set; }

    /// <summary>
    /// Platform-specific information
    /// </summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// Version information about hotkey support
    /// </summary>
    public string? VersionInfo { get; set; }
}
