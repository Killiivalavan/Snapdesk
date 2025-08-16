using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SnapDesk.Core;
using SnapDesk.Core.Interfaces;
using SnapDesk.Data.Services;
using LiteDB;

namespace SnapDesk.Data.Repositories;

/// <summary>
/// Repository for managing LayoutProfile entities
/// </summary>
public class LayoutRepository : RepositoryBase<LayoutProfile>, ILayoutRepository
{
    public LayoutRepository(IDatabaseService databaseService, ILogger<LayoutRepository> logger)
        : base(databaseService, logger, "layouts")
    {
    }

    /// <summary>
    /// Gets a layout by name
    /// </summary>
    /// <param name="name">Layout name</param>
    /// <returns>Layout profile if found, null otherwise</returns>
    public async Task<LayoutProfile?> GetByNameAsync(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var layouts = await GetAsync(l => l.Name == name);
            return layouts.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get layout by name: {Name}", name);
            throw;
        }
    }

    /// <summary>
    /// Gets layouts by partial name match
    /// </summary>
    /// <param name="namePattern">Name pattern to search for</param>
    /// <returns>Collection of matching layouts</returns>
    public async Task<IEnumerable<LayoutProfile>> GetByNamePatternAsync(string namePattern)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(namePattern))
                return new List<LayoutProfile>();

            var allLayouts = await GetAllAsync();
            return allLayouts.Where(l => l.Name.Contains(namePattern, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get layouts by name pattern: {Pattern}", namePattern);
            throw;
        }
    }

    /// <summary>
    /// Gets the currently active layout
    /// </summary>
    /// <returns>Active layout if found, null otherwise</returns>
    public async Task<LayoutProfile?> GetActiveLayoutAsync()
    {
        try
        {
            var activeLayouts = await GetAsync(l => l.IsActive);
            return activeLayouts.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get active layout");
            throw;
        }
    }

    /// <summary>
    /// Sets a layout as active and deactivates all others
    /// </summary>
    /// <param name="id">Layout ID to activate</param>
    /// <returns>True if activation was successful</returns>
    public async Task<bool> SetActiveLayoutAsync(ObjectId id)
    {
        try
        {
            if (id == ObjectId.Empty)
                return false;

            // First, deactivate all layouts
            var allLayouts = await GetAllAsync();
            foreach (var layout in allLayouts)
            {
                if (layout.IsActive)
                {
                    layout.Deactivate();
                    await UpdateAsync(layout);
                }
            }

            // Then activate the specified layout
            var targetLayout = await GetByIdAsync(id);
            if (targetLayout != null)
            {
                targetLayout.Activate();
                return await UpdateAsync(targetLayout);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set active layout: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets layouts created within a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Collection of layouts created within the date range</returns>
    public async Task<IEnumerable<LayoutProfile>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            return await GetAsync(l => l.CreatedAt >= startDate && l.CreatedAt <= endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get layouts by date range: {StartDate} - {EndDate}", startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// Gets layouts updated since a specific date
    /// </summary>
    /// <param name="since">Date to check updates since</param>
    /// <returns>Collection of layouts updated since the specified date</returns>
    public async Task<IEnumerable<LayoutProfile>> GetUpdatedSinceAsync(DateTime since)
    {
        try
        {
            return await GetAsync(l => l.UpdatedAt >= since);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get layouts updated since: {Since}", since);
            throw;
        }
    }

    /// <summary>
    /// Gets layouts with specific number of windows
    /// </summary>
    /// <param name="windowCount">Number of windows</param>
    /// <returns>Collection of layouts with the specified window count</returns>
    public async Task<IEnumerable<LayoutProfile>> GetByWindowCountAsync(int windowCount)
    {
        try
        {
            var allLayouts = await GetAllAsync();
            return allLayouts.Where(l => l.WindowCount == windowCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get layouts by window count: {WindowCount}", windowCount);
            throw;
        }
    }

    /// <summary>
    /// Gets layouts that have hotkeys assigned
    /// </summary>
    /// <returns>Collection of layouts with hotkeys</returns>
    public async Task<IEnumerable<LayoutProfile>> GetLayoutsWithHotkeysAsync()
    {
        try
        {
            return await GetAsync(l => l.Hotkey != null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get layouts with hotkeys");
            throw;
        }
    }

    /// <summary>
    /// Gets the most recently created layouts
    /// </summary>
    /// <param name="count">Number of layouts to return</param>
    /// <returns>Collection of most recently created layouts</returns>
    public async Task<IEnumerable<LayoutProfile>> GetRecentLayoutsAsync(int count = 10)
    {
        try
        {
            var layouts = await GetOrderedAsync(l => l.CreatedAt, false);
            return layouts.Take(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recent layouts");
            throw;
        }
    }

    /// <summary>
    /// Gets the most frequently used layouts (based on update frequency)
    /// </summary>
    /// <param name="count">Number of layouts to return</param>
    /// <returns>Collection of most frequently used layouts</returns>
    public async Task<IEnumerable<LayoutProfile>> GetFrequentlyUsedLayoutsAsync(int count = 10)
    {
        try
        {
            var layouts = await GetOrderedAsync(l => l.UpdatedAt, false);
            return layouts.Take(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get frequently used layouts");
            throw;
        }
    }

    /// <summary>
    /// Checks if a layout name already exists
    /// </summary>
    /// <param name="name">Layout name to check</param>
    /// <param name="excludeId">Optional ID to exclude from the check</param>
    /// <returns>True if name exists, false otherwise</returns>
    public async Task<bool> NameExistsAsync(string name, ObjectId? excludeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (excludeId == null || excludeId == ObjectId.Empty)
            {
                return await AnyAsync(l => l.Name == name);
            }
            else
            {
                return await AnyAsync(l => l.Name == name && l.Id != excludeId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if layout name exists: {Name}", name);
            throw;
        }
    }

    /// <summary>
    /// Gets layout statistics
    /// </summary>
    /// <returns>Layout statistics</returns>
    public new async Task<LayoutStatistics> GetStatisticsAsync()
    {
        try
        {
            var allLayouts = await GetAllAsync();
            var layoutList = allLayouts.ToList();

            return new LayoutStatistics
            {
                TotalLayouts = layoutList.Count,
                ActiveLayouts = layoutList.Count(l => l.IsActive),
                LayoutsWithHotkeys = layoutList.Count(l => l.Hotkey != null),
                AverageWindowsPerLayout = layoutList.Count > 0 ? layoutList.Average(l => l.WindowCount) : 0,
                MostRecentLayout = layoutList.OrderByDescending(l => l.CreatedAt).FirstOrDefault()?.Name,
                OldestLayout = layoutList.OrderBy(l => l.CreatedAt).FirstOrDefault()?.Name,
                TotalWindows = layoutList.Sum(l => l.WindowCount),
                LayoutsCreatedToday = layoutList.Count(l => l.CreatedAt.Date == DateTime.Today),
                LayoutsUpdatedToday = layoutList.Count(l => l.UpdatedAt.Date == DateTime.Today)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get layout statistics");
            throw;
        }
    }

    /// <summary>
    /// Searches layouts by name and description
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>Collection of matching layouts</returns>
    public async Task<IEnumerable<LayoutProfile>> SearchAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<LayoutProfile>();

            var allLayouts = await GetAllAsync();
            return allLayouts.Where(l => 
                l.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(l.Description) && l.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search layouts with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Gets layouts by tags
    /// </summary>
    /// <param name="tags">Tags to search for</param>
    /// <returns>Collection of layouts matching the tags</returns>
    public Task<IEnumerable<LayoutProfile>> GetByTagsAsync(IEnumerable<string> tags)
    {
        try
        {
            // Tags feature not yet implemented in LayoutProfile model
            // Return empty collection for now
            return Task.FromResult<IEnumerable<LayoutProfile>>(new List<LayoutProfile>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get layouts by tags");
            throw;
        }
    }

    /// <summary>
    /// Gets all available tags from all layouts
    /// </summary>
    /// <returns>Collection of unique tags</returns>
    public Task<IEnumerable<string>> GetAllTagsAsync()
    {
        try
        {
            // Tags feature not yet implemented in LayoutProfile model
            // Return empty collection for now
            return Task.FromResult<IEnumerable<string>>(new List<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all tags");
            throw;
        }
    }
}


