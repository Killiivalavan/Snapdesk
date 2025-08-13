using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SnapDesk.Core;
using SnapDesk.Core.Interfaces;
using SnapDesk.Data.Services;

namespace SnapDesk.Data.Repositories;

/// <summary>
/// Repository for managing HotkeyInfo entities
/// </summary>
public class HotkeyRepository : RepositoryBase<HotkeyInfo>, IHotkeyRepository
{
    public HotkeyRepository(IDatabaseService databaseService, ILogger<HotkeyRepository> logger)
        : base(databaseService, logger, "hotkeys")
    {
    }

    /// <summary>
    /// Gets a hotkey by its key combination
    /// </summary>
    /// <param name="keys">Key combination string</param>
    /// <returns>Hotkey if found, null otherwise</returns>
    public async Task<HotkeyInfo?> GetByKeysAsync(string keys)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keys))
                return null;

            var hotkeys = await GetAsync(h => h.Keys == keys);
            return hotkeys.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hotkey by keys: {Keys}", keys);
            throw;
        }
    }

    /// <summary>
    /// Gets hotkeys by action type
    /// </summary>
    /// <param name="action">Action type</param>
    /// <returns>Collection of hotkeys for the specified action</returns>
    public async Task<IEnumerable<HotkeyInfo>> GetByActionAsync(HotkeyAction action)
    {
        try
        {
            return await GetAsync(h => h.Action == action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hotkeys by action: {Action}", action);
            throw;
        }
    }

    /// <summary>
    /// Gets hotkeys associated with a specific layout
    /// </summary>
    /// <param name="layoutId">Layout ID</param>
    /// <returns>Collection of hotkeys for the specified layout</returns>
    public async Task<IEnumerable<HotkeyInfo>> GetByLayoutIdAsync(string layoutId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(layoutId))
                return new List<HotkeyInfo>();

            return await GetAsync(h => h.LayoutId == layoutId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hotkeys by layout ID: {LayoutId}", layoutId);
            throw;
        }
    }

    /// <summary>
    /// Gets all enabled hotkeys
    /// </summary>
    /// <returns>Collection of enabled hotkeys</returns>
    public async Task<IEnumerable<HotkeyInfo>> GetEnabledHotkeysAsync()
    {
        try
        {
            return await GetAsync(h => h.IsEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get enabled hotkeys");
            throw;
        }
    }

    /// <summary>
    /// Gets all disabled hotkeys
    /// </summary>
    /// <returns>Collection of disabled hotkeys</returns>
    public async Task<IEnumerable<HotkeyInfo>> GetDisabledHotkeysAsync()
    {
        try
        {
            return await GetAsync(h => !h.IsEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get disabled hotkeys");
            throw;
        }
    }

    /// <summary>
    /// Gets hotkeys by modifier keys
    /// </summary>
    /// <param name="modifiers">List of modifier keys</param>
    /// <returns>Collection of hotkeys with the specified modifiers</returns>
    public async Task<IEnumerable<HotkeyInfo>> GetByModifiersAsync(List<ModifierKey> modifiers)
    {
        try
        {
            if (modifiers == null || modifiers.Count == 0)
                return new List<HotkeyInfo>();

            var allHotkeys = await GetAllAsync();
            return allHotkeys.Where(h => 
                h.Modifiers.Count == modifiers.Count && 
                modifiers.All(m => h.Modifiers.Contains(m))
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hotkeys by modifiers");
            throw;
        }
    }

    /// <summary>
    /// Gets hotkeys that conflict with the specified key combination
    /// </summary>
    /// <param name="keys">Key combination to check</param>
    /// <param name="excludeId">Optional ID to exclude from conflict check</param>
    /// <returns>Collection of conflicting hotkeys</returns>
    public async Task<IEnumerable<HotkeyInfo>> GetConflictingHotkeysAsync(string keys, string? excludeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keys))
                return new List<HotkeyInfo>();

            if (string.IsNullOrWhiteSpace(excludeId))
            {
                return await GetAsync(h => h.Keys == keys && h.IsEnabled);
            }
            else
            {
                return await GetAsync(h => h.Keys == keys && h.IsEnabled && h.Id != excludeId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get conflicting hotkeys for keys: {Keys}", keys);
            throw;
        }
    }

    /// <summary>
    /// Checks if a key combination is available for use
    /// </summary>
    /// <param name="keys">Key combination to check</param>
    /// <param name="excludeId">Optional ID to exclude from availability check</param>
    /// <returns>True if key combination is available, false otherwise</returns>
    public async Task<bool> IsKeyCombinationAvailableAsync(string keys, string? excludeId = null)
    {
        try
        {
            var conflicts = await GetConflictingHotkeysAsync(keys, excludeId);
            return !conflicts.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check key combination availability: {Keys}", keys);
            throw;
        }
    }

    /// <summary>
    /// Gets hotkeys created within a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Collection of hotkeys created within the date range</returns>
    public async Task<IEnumerable<HotkeyInfo>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            return await GetAsync(h => h.CreatedAt >= startDate && h.CreatedAt <= endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hotkeys by date range: {StartDate} - {EndDate}", startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// Gets hotkeys that have no layout association
    /// </summary>
    /// <returns>Collection of global hotkeys</returns>
    public async Task<IEnumerable<HotkeyInfo>> GetGlobalHotkeysAsync()
    {
        try
        {
            return await GetAsync(h => string.IsNullOrEmpty(h.LayoutId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get global hotkeys");
            throw;
        }
    }

    /// <summary>
    /// Gets hotkeys that are associated with layouts
    /// </summary>
    /// <returns>Collection of layout-specific hotkeys</returns>
    public async Task<IEnumerable<HotkeyInfo>> GetLayoutSpecificHotkeysAsync()
    {
        try
        {
            return await GetAsync(h => !string.IsNullOrEmpty(h.LayoutId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get layout-specific hotkeys");
            throw;
        }
    }

    /// <summary>
    /// Gets the most recently created hotkeys
    /// </summary>
    /// <param name="count">Number of hotkeys to return</param>
    /// <returns>Collection of most recently created hotkeys</returns>
    public async Task<IEnumerable<HotkeyInfo>> GetRecentHotkeysAsync(int count = 10)
    {
        try
        {
            var hotkeys = await GetOrderedAsync(h => h.CreatedAt, false);
            return hotkeys.Take(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recent hotkeys");
            throw;
        }
    }

    /// <summary>
    /// Removes layout associations for hotkeys when a layout is deleted
    /// </summary>
    /// <param name="layoutId">Layout ID to remove associations for</param>
    /// <returns>Number of hotkeys that had their association removed</returns>
    public async Task<int> RemoveLayoutAssociationsAsync(string layoutId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(layoutId))
                return 0;

            var associatedHotkeys = await GetByLayoutIdAsync(layoutId);
            int updatedCount = 0;

            foreach (var hotkey in associatedHotkeys)
            {
                hotkey.RemoveLayoutAssociation();
                if (await UpdateAsync(hotkey))
                {
                    updatedCount++;
                }
            }

            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove layout associations for layout: {LayoutId}", layoutId);
            throw;
        }
    }

    /// <summary>
    /// Gets hotkey statistics
    /// </summary>
    /// <returns>Hotkey statistics</returns>
    public new async Task<HotkeyRepositoryStatistics> GetStatisticsAsync()
    {
        try
        {
            var allHotkeys = await GetAllAsync();
            var hotkeyList = allHotkeys.ToList();

            var actionGroups = hotkeyList.GroupBy(h => h.Action)
                .ToDictionary(g => g.Key, g => g.Count());

            var modifierGroups = hotkeyList
                .SelectMany(h => h.Modifiers)
                .GroupBy(m => m)
                .ToDictionary(g => g.Key, g => g.Count());

            return new HotkeyRepositoryStatistics
            {
                TotalHotkeys = hotkeyList.Count,
                EnabledHotkeys = hotkeyList.Count(h => h.IsEnabled),
                DisabledHotkeys = hotkeyList.Count(h => !h.IsEnabled),
                GlobalHotkeys = hotkeyList.Count(h => string.IsNullOrEmpty(h.LayoutId)),
                LayoutSpecificHotkeys = hotkeyList.Count(h => !string.IsNullOrEmpty(h.LayoutId)),
                ActionDistribution = actionGroups,
                ModifierUsage = modifierGroups,
                MostRecentHotkey = hotkeyList.OrderByDescending(h => h.CreatedAt).FirstOrDefault()?.Keys,
                HotkeysCreatedToday = hotkeyList.Count(h => h.CreatedAt.Date == DateTime.Today),
                AverageModifiersPerHotkey = hotkeyList.Count > 0 ? hotkeyList.Average(h => h.ModifierCount) : 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hotkey statistics");
            throw;
        }
    }

    /// <summary>
    /// Searches hotkeys by key combination or layout association
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>Collection of matching hotkeys</returns>
    public async Task<IEnumerable<HotkeyInfo>> SearchAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<HotkeyInfo>();

            var allHotkeys = await GetAllAsync();
            return allHotkeys.Where(h => 
                h.Keys.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(h.LayoutId) && h.LayoutId.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search hotkeys with term: {SearchTerm}", searchTerm);
            throw;
        }
    }
}

/// <summary>
/// Interface for hotkey repository operations
/// </summary>
public interface IHotkeyRepository : IRepository<HotkeyInfo>
{
    /// <summary>
    /// Gets a hotkey by its key combination
    /// </summary>
    Task<HotkeyInfo?> GetByKeysAsync(string keys);

    /// <summary>
    /// Gets hotkeys by action type
    /// </summary>
    Task<IEnumerable<HotkeyInfo>> GetByActionAsync(HotkeyAction action);

    /// <summary>
    /// Gets hotkeys associated with a specific layout
    /// </summary>
    Task<IEnumerable<HotkeyInfo>> GetByLayoutIdAsync(string layoutId);

    /// <summary>
    /// Gets all enabled hotkeys
    /// </summary>
    Task<IEnumerable<HotkeyInfo>> GetEnabledHotkeysAsync();

    /// <summary>
    /// Gets all disabled hotkeys
    /// </summary>
    Task<IEnumerable<HotkeyInfo>> GetDisabledHotkeysAsync();

    /// <summary>
    /// Gets hotkeys by modifier keys
    /// </summary>
    Task<IEnumerable<HotkeyInfo>> GetByModifiersAsync(List<ModifierKey> modifiers);

    /// <summary>
    /// Gets hotkeys that conflict with the specified key combination
    /// </summary>
    Task<IEnumerable<HotkeyInfo>> GetConflictingHotkeysAsync(string keys, string? excludeId = null);

    /// <summary>
    /// Checks if a key combination is available for use
    /// </summary>
    Task<bool> IsKeyCombinationAvailableAsync(string keys, string? excludeId = null);

    /// <summary>
    /// Gets hotkeys created within a date range
    /// </summary>
    Task<IEnumerable<HotkeyInfo>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets hotkeys that have no layout association
    /// </summary>
    Task<IEnumerable<HotkeyInfo>> GetGlobalHotkeysAsync();

    /// <summary>
    /// Gets hotkeys that are associated with layouts
    /// </summary>
    Task<IEnumerable<HotkeyInfo>> GetLayoutSpecificHotkeysAsync();

    /// <summary>
    /// Gets the most recently created hotkeys
    /// </summary>
    Task<IEnumerable<HotkeyInfo>> GetRecentHotkeysAsync(int count = 10);

    /// <summary>
    /// Removes layout associations for hotkeys when a layout is deleted
    /// </summary>
    Task<int> RemoveLayoutAssociationsAsync(string layoutId);

    /// <summary>
    /// Gets hotkey statistics
    /// </summary>
    new Task<HotkeyRepositoryStatistics> GetStatisticsAsync();

    /// <summary>
    /// Searches hotkeys by key combination or layout association
    /// </summary>
    Task<IEnumerable<HotkeyInfo>> SearchAsync(string searchTerm);
}

/// <summary>
/// Statistics about hotkeys
/// </summary>
public class HotkeyRepositoryStatistics
{
    /// <summary>
    /// Total number of hotkeys
    /// </summary>
    public int TotalHotkeys { get; set; }

    /// <summary>
    /// Number of enabled hotkeys
    /// </summary>
    public int EnabledHotkeys { get; set; }

    /// <summary>
    /// Number of disabled hotkeys
    /// </summary>
    public int DisabledHotkeys { get; set; }

    /// <summary>
    /// Number of global hotkeys
    /// </summary>
    public int GlobalHotkeys { get; set; }

    /// <summary>
    /// Number of layout-specific hotkeys
    /// </summary>
    public int LayoutSpecificHotkeys { get; set; }

    /// <summary>
    /// Distribution of hotkeys by action
    /// </summary>
    public Dictionary<HotkeyAction, int> ActionDistribution { get; set; } = new();

    /// <summary>
    /// Usage count of modifier keys
    /// </summary>
    public Dictionary<ModifierKey, int> ModifierUsage { get; set; } = new();

    /// <summary>
    /// Key combination of the most recently created hotkey
    /// </summary>
    public string? MostRecentHotkey { get; set; }

    /// <summary>
    /// Number of hotkeys created today
    /// </summary>
    public int HotkeysCreatedToday { get; set; }

    /// <summary>
    /// Average number of modifiers per hotkey
    /// </summary>
    public double AverageModifiersPerHotkey { get; set; }
}
