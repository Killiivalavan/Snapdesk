using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SnapDesk.Core.Interfaces;
using SnapDesk.Data.Services;

namespace SnapDesk.Data.Repositories;

/// <summary>
/// Repository for managing application settings
/// </summary>
public class SettingsRepository : RepositoryBase<AppSetting>, ISettingsRepository
{
    public SettingsRepository(IDatabaseService databaseService, ILogger<SettingsRepository> logger)
        : base(databaseService, logger, "settings")
    {
    }

    /// <summary>
    /// Gets a setting by its key
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <returns>Setting if found, null otherwise</returns>
    public async Task<AppSetting?> GetByKeyAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            var settings = await GetAsync(s => s.Key == key);
            return settings.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get setting by key: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Gets the string value of a setting
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="defaultValue">Default value if setting doesn't exist</param>
    /// <returns>Setting value or default value</returns>
    public async Task<string> GetValueAsync(string key, string defaultValue = "")
    {
        try
        {
            var setting = await GetByKeyAsync(key);
            return setting?.Value ?? defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get setting value for key: {Key}", key);
            return defaultValue;
        }
    }

    /// <summary>
    /// Gets the boolean value of a setting
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="defaultValue">Default value if setting doesn't exist</param>
    /// <returns>Setting value or default value</returns>
    public async Task<bool> GetBoolValueAsync(string key, bool defaultValue = false)
    {
        try
        {
            var value = await GetValueAsync(key);
            if (bool.TryParse(value, out var result))
                return result;
            return defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get boolean setting value for key: {Key}", key);
            return defaultValue;
        }
    }

    /// <summary>
    /// Gets the integer value of a setting
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="defaultValue">Default value if setting doesn't exist</param>
    /// <returns>Setting value or default value</returns>
    public async Task<int> GetIntValueAsync(string key, int defaultValue = 0)
    {
        try
        {
            var value = await GetValueAsync(key);
            if (int.TryParse(value, out var result))
                return result;
            return defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get integer setting value for key: {Key}", key);
            return defaultValue;
        }
    }

    /// <summary>
    /// Gets the double value of a setting
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="defaultValue">Default value if setting doesn't exist</param>
    /// <returns>Setting value or default value</returns>
    public async Task<double> GetDoubleValueAsync(string key, double defaultValue = 0.0)
    {
        try
        {
            var value = await GetValueAsync(key);
            if (double.TryParse(value, out var result))
                return result;
            return defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get double setting value for key: {Key}", key);
            return defaultValue;
        }
    }

    /// <summary>
    /// Sets a setting value
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="value">Setting value</param>
    /// <param name="description">Optional description</param>
    /// <returns>True if setting was saved successfully</returns>
    public async Task<bool> SetValueAsync(string key, string value, string? description = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            var existingSetting = await GetByKeyAsync(key);
            
            if (existingSetting != null)
            {
                existingSetting.Value = value ?? string.Empty;
                existingSetting.UpdatedAt = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(description))
                    existingSetting.Description = description;
                
                return await UpdateAsync(existingSetting);
            }
            else
            {
                var newSetting = new AppSetting
                {
                    Key = key,
                    Value = value ?? string.Empty,
                    Description = description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await InsertAsync(newSetting);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set setting value for key: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Sets a boolean setting value
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="value">Setting value</param>
    /// <param name="description">Optional description</param>
    /// <returns>True if setting was saved successfully</returns>
    public async Task<bool> SetBoolValueAsync(string key, bool value, string? description = null)
    {
        return await SetValueAsync(key, value.ToString(), description);
    }

    /// <summary>
    /// Sets an integer setting value
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="value">Setting value</param>
    /// <param name="description">Optional description</param>
    /// <returns>True if setting was saved successfully</returns>
    public async Task<bool> SetIntValueAsync(string key, int value, string? description = null)
    {
        return await SetValueAsync(key, value.ToString(), description);
    }

    /// <summary>
    /// Sets a double setting value
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="value">Setting value</param>
    /// <param name="description">Optional description</param>
    /// <returns>True if setting was saved successfully</returns>
    public async Task<bool> SetDoubleValueAsync(string key, double value, string? description = null)
    {
        return await SetValueAsync(key, value.ToString(), description);
    }

    /// <summary>
    /// Deletes a setting by key
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <returns>True if setting was deleted successfully</returns>
    public async Task<bool> DeleteByKeyAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            var setting = await GetByKeyAsync(key);
            if (setting != null)
            {
                return await DeleteAsync(setting);
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete setting by key: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Gets settings by category (using key prefix)
    /// </summary>
    /// <param name="category">Category prefix</param>
    /// <returns>Collection of settings in the specified category</returns>
    public async Task<IEnumerable<AppSetting>> GetByCategoryAsync(string category)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(category))
                return new List<AppSetting>();

            var allSettings = await GetAllAsync();
            return allSettings.Where(s => s.Key.StartsWith(category, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get settings by category: {Category}", category);
            throw;
        }
    }

    /// <summary>
    /// Checks if a setting key exists
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <returns>True if setting exists, false otherwise</returns>
    public async Task<bool> KeyExistsAsync(string key)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            return await AnyAsync(s => s.Key == key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if setting key exists: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Gets settings updated since a specific date
    /// </summary>
    /// <param name="since">Date to check updates since</param>
    /// <returns>Collection of settings updated since the specified date</returns>
    public async Task<IEnumerable<AppSetting>> GetUpdatedSinceAsync(DateTime since)
    {
        try
        {
            return await GetAsync(s => s.UpdatedAt >= since);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get settings updated since: {Since}", since);
            throw;
        }
    }

    /// <summary>
    /// Gets all setting keys
    /// </summary>
    /// <returns>Collection of all setting keys</returns>
    public async Task<IEnumerable<string>> GetAllKeysAsync()
    {
        try
        {
            var allSettings = await GetAllAsync();
            return allSettings.Select(s => s.Key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all setting keys");
            throw;
        }
    }

    /// <summary>
    /// Initializes default settings if they don't exist
    /// </summary>
    /// <returns>Number of default settings created</returns>
    public async Task<int> InitializeDefaultSettingsAsync()
    {
        try
        {
            var defaultSettings = new[]
            {
                new { Key = "app.startup.enabled", Value = "false", Description = "Start SnapDesk with Windows" },
                new { Key = "app.startup.minimized", Value = "true", Description = "Start SnapDesk minimized to system tray" },
                new { Key = "app.backup.enabled", Value = "true", Description = "Enable automatic database backups" },
                new { Key = "app.backup.interval", Value = "24", Description = "Backup interval in hours" },
                new { Key = "app.backup.retention", Value = "7", Description = "Number of days to keep backups" },
                new { Key = "app.hotkeys.global", Value = "true", Description = "Enable global hotkey processing" },
                new { Key = "app.notifications.enabled", Value = "true", Description = "Show notification messages" },
                new { Key = "app.logging.level", Value = "Information", Description = "Logging level (Debug, Information, Warning, Error)" },
                new { Key = "ui.theme", Value = "System", Description = "UI theme (Light, Dark, System)" },
                new { Key = "ui.window.width", Value = "1200", Description = "Main window default width" },
                new { Key = "ui.window.height", Value = "800", Description = "Main window default height" },
                new { Key = "layout.auto.save", Value = "false", Description = "Automatically save layout changes" },
                new { Key = "layout.validation.strict", Value = "true", Description = "Enable strict layout validation" }
            };

            int created = 0;
            foreach (var setting in defaultSettings)
            {
                if (!await KeyExistsAsync(setting.Key))
                {
                    await SetValueAsync(setting.Key, setting.Value, setting.Description);
                    created++;
                }
            }

            return created;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize default settings");
            throw;
        }
    }
}

/// <summary>
/// Interface for settings repository operations
/// </summary>
public interface ISettingsRepository : IRepository<AppSetting>
{
    /// <summary>
    /// Gets a setting by its key
    /// </summary>
    Task<AppSetting?> GetByKeyAsync(string key);

    /// <summary>
    /// Gets the string value of a setting
    /// </summary>
    Task<string> GetValueAsync(string key, string defaultValue = "");

    /// <summary>
    /// Gets the boolean value of a setting
    /// </summary>
    Task<bool> GetBoolValueAsync(string key, bool defaultValue = false);

    /// <summary>
    /// Gets the integer value of a setting
    /// </summary>
    Task<int> GetIntValueAsync(string key, int defaultValue = 0);

    /// <summary>
    /// Gets the double value of a setting
    /// </summary>
    Task<double> GetDoubleValueAsync(string key, double defaultValue = 0.0);

    /// <summary>
    /// Sets a setting value
    /// </summary>
    Task<bool> SetValueAsync(string key, string value, string? description = null);

    /// <summary>
    /// Sets a boolean setting value
    /// </summary>
    Task<bool> SetBoolValueAsync(string key, bool value, string? description = null);

    /// <summary>
    /// Sets an integer setting value
    /// </summary>
    Task<bool> SetIntValueAsync(string key, int value, string? description = null);

    /// <summary>
    /// Sets a double setting value
    /// </summary>
    Task<bool> SetDoubleValueAsync(string key, double value, string? description = null);

    /// <summary>
    /// Deletes a setting by key
    /// </summary>
    Task<bool> DeleteByKeyAsync(string key);

    /// <summary>
    /// Gets settings by category
    /// </summary>
    Task<IEnumerable<AppSetting>> GetByCategoryAsync(string category);

    /// <summary>
    /// Checks if a setting key exists
    /// </summary>
    Task<bool> KeyExistsAsync(string key);

    /// <summary>
    /// Gets settings updated since a specific date
    /// </summary>
    Task<IEnumerable<AppSetting>> GetUpdatedSinceAsync(DateTime since);

    /// <summary>
    /// Gets all setting keys
    /// </summary>
    Task<IEnumerable<string>> GetAllKeysAsync();

    /// <summary>
    /// Initializes default settings
    /// </summary>
    Task<int> InitializeDefaultSettingsAsync();
}

/// <summary>
/// Application setting entity
/// </summary>
public class AppSetting
{
    /// <summary>
    /// Unique identifier for the setting
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Setting key (unique)
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Setting value
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the setting
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When this setting was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this setting was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this setting is user-configurable
    /// </summary>
    public bool IsUserConfigurable { get; set; } = true;

    /// <summary>
    /// Whether this setting requires application restart to take effect
    /// </summary>
    public bool RequiresRestart { get; set; } = false;
}
