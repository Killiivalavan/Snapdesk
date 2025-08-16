using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SnapDesk.Core;
using SnapDesk.Core.Interfaces;
using SnapDesk.Data.Configuration;
using SnapDesk.Data.Repositories;
using SnapDesk.Data.Services;
using SnapDesk.Shared;

namespace SnapDesk.Data.Initialization;

/// <summary>
/// Service responsible for initializing the database and creating initial data
/// </summary>
public class DatabaseInitializer
{
    private readonly IDatabaseService _databaseService;
    private readonly ILayoutRepository _layoutRepository;
    private readonly IHotkeyRepository _hotkeyRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        IDatabaseService databaseService,
        ILayoutRepository layoutRepository,
        IHotkeyRepository hotkeyRepository,
        ISettingsRepository settingsRepository,
        ILogger<DatabaseInitializer> logger)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _layoutRepository = layoutRepository ?? throw new ArgumentNullException(nameof(layoutRepository));
        _hotkeyRepository = hotkeyRepository ?? throw new ArgumentNullException(nameof(hotkeyRepository));
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes the database and creates initial data
    /// </summary>
    /// <returns>Initialization result</returns>
    public async Task<InitializationResult> InitializeAsync()
    {
        var result = new InitializationResult();
        
        try
        {
            _logger.LogInformation("Starting database initialization...");

            // Initialize database service
            await _databaseService.InitializeAsync();
            result.DatabaseInitialized = true;

            // Test database connectivity
            result.DatabaseConnected = await _databaseService.TestConnectionAsync();
            if (!result.DatabaseConnected)
            {
                throw new InvalidOperationException("Database connectivity test failed");
            }

            // Initialize default settings
            var settingsCreated = await _settingsRepository.InitializeDefaultSettingsAsync();
            result.DefaultSettingsCreated = settingsCreated;
            _logger.LogInformation("Created {SettingsCount} default settings", settingsCreated);

            // Create sample data if this is a new database
            if (await IsNewDatabaseAsync())
            {
                await CreateSampleDataAsync(result);
            }

            // Verify repositories are working
            await VerifyRepositoriesAsync(result);

            result.IsSuccess = true;
            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Database initialization failed");
        }

        return result;
    }

    /// <summary>
    /// Checks if this is a new database (no layouts exist)
    /// </summary>
    /// <returns>True if database is new, false otherwise</returns>
    private async Task<bool> IsNewDatabaseAsync()
    {
        try
        {
            var layoutCount = await _layoutRepository.CountAsync();
            return layoutCount == 0;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// Creates sample data for demonstration purposes
    /// </summary>
    /// <param name="result">Initialization result to update</param>
    private async Task<bool> CreateSampleDataAsync(InitializationResult result)
    {
        try
        {
            _logger.LogInformation("Creating sample data...");

            // Create a sample layout
            var sampleLayout = new LayoutProfile("Welcome Layout", "Default layout created by SnapDesk");

            // Add sample windows
            sampleLayout.AddWindow(new WindowInfo("SnapDesk", "SnapDesk - Welcome", new Point(100, 100), new Size(1200, 800)));
            sampleLayout.AddWindow(new WindowInfo("notepad", "Untitled - Notepad", new Point(300, 200), new Size(800, 600)));

            // Add sample monitor configuration
            sampleLayout.AddMonitor(new MonitorInfo(0, true, new Rectangle(0, 0, 1920, 1080), new Rectangle(0, 0, 1920, 1040)));

            await _layoutRepository.InsertAsync(sampleLayout);
            result.SampleLayoutCreated = true;

            // Create a sample global hotkey
            var sampleHotkey = new HotkeyInfo("Ctrl+Alt+S", HotkeyAction.QuickSave);

            await _hotkeyRepository.InsertAsync(sampleHotkey);
            result.SampleHotkeyCreated = true;

            _logger.LogInformation("Sample data created successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create sample data");
            return false;
        }
    }

    /// <summary>
    /// Verifies that all repositories are working correctly
    /// </summary>
    /// <param name="result">Initialization result to update</param>
    private async Task VerifyRepositoriesAsync(InitializationResult result)
    {
        try
        {
            // Test Layout Repository
            result.LayoutRepositoryWorking = await _layoutRepository.IsConnectedAsync();
            var layoutCount = await _layoutRepository.CountAsync();
            _logger.LogDebug("Layout repository working: {Working}, Count: {Count}", 
                result.LayoutRepositoryWorking, layoutCount);

            // Test Hotkey Repository
            result.HotkeyRepositoryWorking = await _hotkeyRepository.IsConnectedAsync();
            var hotkeyCount = await _hotkeyRepository.CountAsync();
            _logger.LogDebug("Hotkey repository working: {Working}, Count: {Count}", 
                result.HotkeyRepositoryWorking, hotkeyCount);

            // Test Settings Repository
            result.SettingsRepositoryWorking = await _settingsRepository.IsConnectedAsync();
            var settingsCount = await _settingsRepository.CountAsync();
            _logger.LogDebug("Settings repository working: {Working}, Count: {Count}", 
                result.SettingsRepositoryWorking, settingsCount);

            _logger.LogInformation("Repository verification completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Repository verification failed");
            throw;
        }
    }

    /// <summary>
    /// Gets database statistics and health information
    /// </summary>
    /// <returns>Database health information</returns>
    public async Task<DatabaseHealthInfo> GetHealthInfoAsync()
    {
        try
        {
            var stats = await _databaseService.GetStatisticsAsync();
            var layoutStats = await _layoutRepository.GetStatisticsAsync();
            var hotkeyStats = await _hotkeyRepository.GetStatisticsAsync();

            return new DatabaseHealthInfo
            {
                IsHealthy = stats.IsHealthy,
                DatabasePath = _databaseService.DatabasePath,
                DatabaseSizeBytes = stats.DatabaseSizeBytes,
                TotalLayouts = layoutStats.TotalLayouts,
                TotalHotkeys = hotkeyStats.TotalHotkeys,
                TotalSettings = await _settingsRepository.CountAsync(),
                LastBackupTime = stats.LastBackupTime,
                IsConnected = await _databaseService.TestConnectionAsync()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get database health information");
            return new DatabaseHealthInfo
            {
                IsHealthy = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Creates a backup of the database
    /// </summary>
    /// <returns>Path to the backup file</returns>
    public async Task<string> CreateBackupAsync()
    {
        try
        {
            return await _databaseService.BackupAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database backup");
            throw;
        }
    }

    /// <summary>
    /// Restores the database from a backup
    /// </summary>
    /// <param name="backupPath">Path to the backup file</param>
    public async Task RestoreFromBackupAsync(string backupPath)
    {
        try
        {
            await _databaseService.RestoreAsync(backupPath);
            _logger.LogInformation("Database restored from backup: {BackupPath}", backupPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore database from backup: {BackupPath}", backupPath);
            throw;
        }
    }
}

/// <summary>
/// Result of database initialization
/// </summary>
public class InitializationResult
{
    /// <summary>
    /// Whether initialization was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Error message if initialization failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether database was initialized
    /// </summary>
    public bool DatabaseInitialized { get; set; }

    /// <summary>
    /// Whether database connection was successful
    /// </summary>
    public bool DatabaseConnected { get; set; }

    /// <summary>
    /// Number of default settings created
    /// </summary>
    public int DefaultSettingsCreated { get; set; }

    /// <summary>
    /// Whether sample layout was created
    /// </summary>
    public bool SampleLayoutCreated { get; set; }

    /// <summary>
    /// Whether sample hotkey was created
    /// </summary>
    public bool SampleHotkeyCreated { get; set; }

    /// <summary>
    /// Whether layout repository is working
    /// </summary>
    public bool LayoutRepositoryWorking { get; set; }

    /// <summary>
    /// Whether hotkey repository is working
    /// </summary>
    public bool HotkeyRepositoryWorking { get; set; }

    /// <summary>
    /// Whether settings repository is working
    /// </summary>
    public bool SettingsRepositoryWorking { get; set; }

    /// <summary>
    /// Gets a summary of the initialization result
    /// </summary>
    /// <returns>Summary string</returns>
    public string GetSummary()
    {
        if (!IsSuccess)
            return $"Initialization failed: {ErrorMessage}";

        var items = new[]
        {
            DatabaseInitialized ? "Database initialized" : null,
            DatabaseConnected ? "Database connected" : null,
            DefaultSettingsCreated > 0 ? $"{DefaultSettingsCreated} default settings created" : null,
            SampleLayoutCreated ? "Sample layout created" : null,
            SampleHotkeyCreated ? "Sample hotkey created" : null,
            LayoutRepositoryWorking ? "Layout repository working" : null,
            HotkeyRepositoryWorking ? "Hotkey repository working" : null,
            SettingsRepositoryWorking ? "Settings repository working" : null
        };

        var validItems = items.Where(item => item != null);
        return $"Initialization successful: {string.Join(", ", validItems)}";
    }
}

/// <summary>
/// Database health information
/// </summary>
public class DatabaseHealthInfo
{
    /// <summary>
    /// Whether the database is healthy
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Database file path
    /// </summary>
    public string DatabasePath { get; set; } = string.Empty;

    /// <summary>
    /// Database file size in bytes
    /// </summary>
    public long DatabaseSizeBytes { get; set; }

    /// <summary>
    /// Total number of layouts
    /// </summary>
    public int TotalLayouts { get; set; }

    /// <summary>
    /// Total number of hotkeys
    /// </summary>
    public int TotalHotkeys { get; set; }

    /// <summary>
    /// Total number of settings
    /// </summary>
    public int TotalSettings { get; set; }

    /// <summary>
    /// Last backup time
    /// </summary>
    public DateTime? LastBackupTime { get; set; }

    /// <summary>
    /// Whether database is connected
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// Error message if not healthy
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets database size in a human-readable format
    /// </summary>
    /// <returns>Formatted size string</returns>
    public string GetFormattedSize()
    {
        if (DatabaseSizeBytes < 1024)
            return $"{DatabaseSizeBytes} bytes";
        else if (DatabaseSizeBytes < 1024 * 1024)
            return $"{DatabaseSizeBytes / 1024:F1} KB";
        else
            return $"{DatabaseSizeBytes / (1024 * 1024):F1} MB";
    }
}
