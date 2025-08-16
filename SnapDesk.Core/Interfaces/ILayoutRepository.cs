using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SnapDesk.Core;
using LiteDB;

namespace SnapDesk.Core.Interfaces;

/// <summary>
/// Interface for layout repository operations
/// </summary>
public interface ILayoutRepository : IRepository<LayoutProfile>
{
    /// <summary>
    /// Gets a layout by name
    /// </summary>
    Task<LayoutProfile?> GetByNameAsync(string name);

    /// <summary>
    /// Gets layouts by partial name match
    /// </summary>
    Task<IEnumerable<LayoutProfile>> GetByNamePatternAsync(string namePattern);

    /// <summary>
    /// Gets the currently active layout
    /// </summary>
    Task<LayoutProfile?> GetActiveLayoutAsync();

    /// <summary>
    /// Sets a layout as active
    /// </summary>
    Task<bool> SetActiveLayoutAsync(ObjectId id);

    /// <summary>
    /// Gets layouts created within a date range
    /// </summary>
    Task<IEnumerable<LayoutProfile>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets layouts updated since a specific date
    /// </summary>
    Task<IEnumerable<LayoutProfile>> GetUpdatedSinceAsync(DateTime since);

    /// <summary>
    /// Gets layouts with specific number of windows
    /// </summary>
    Task<IEnumerable<LayoutProfile>> GetByWindowCountAsync(int windowCount);

    /// <summary>
    /// Gets layouts that have hotkeys assigned
    /// </summary>
    Task<IEnumerable<LayoutProfile>> GetLayoutsWithHotkeysAsync();

    /// <summary>
    /// Gets the most recently created layouts
    /// </summary>
    Task<IEnumerable<LayoutProfile>> GetRecentLayoutsAsync(int count = 10);

    /// <summary>
    /// Gets the most frequently used layouts
    /// </summary>
    Task<IEnumerable<LayoutProfile>> GetFrequentlyUsedLayoutsAsync(int count = 10);

    /// <summary>
    /// Checks if a layout name already exists
    /// </summary>
    Task<bool> NameExistsAsync(string name, ObjectId? excludeId = null);

    /// <summary>
    /// Gets layout statistics
    /// </summary>
    new Task<LayoutStatistics> GetStatisticsAsync();

    /// <summary>
    /// Searches layouts by name and description
    /// </summary>
    Task<IEnumerable<LayoutProfile>> SearchAsync(string searchTerm);

    /// <summary>
    /// Gets layouts by tags
    /// </summary>
    Task<IEnumerable<LayoutProfile>> GetByTagsAsync(IEnumerable<string> tags);

    /// <summary>
    /// Gets all available tags
    /// </summary>
    Task<IEnumerable<string>> GetAllTagsAsync();
}

