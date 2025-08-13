using System;
using System.IO;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;
using SnapDesk.Data.Configuration;
using SnapDesk.Core.Interfaces;

namespace SnapDesk.Data.Services;

/// <summary>
/// Main database service for managing LiteDB connections and operations
/// </summary>
public class DatabaseService : IDatabaseService, IDisposable
{
    private readonly DatabaseConfiguration _configuration;
    private readonly ILogger<DatabaseService> _logger;
    private LiteDatabase? _database;
    private readonly object _lock = new object();
    private bool _disposed = false;

    /// <summary>
    /// Current database configuration
    /// </summary>
    public DatabaseConfiguration Configuration => _configuration;

    /// <summary>
    /// Whether the database is currently connected
    /// </summary>
    public bool IsConnected => _database != null;

    /// <summary>
    /// Database file path
    /// </summary>
    public string DatabasePath => _configuration.DatabasePath;

    public DatabaseService(DatabaseConfiguration configuration, ILogger<DatabaseService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!_configuration.IsValid())
        {
            throw new ArgumentException("Invalid database configuration", nameof(configuration));
        }
    }

    /// <summary>
    /// Initializes the database connection and schema
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Initializing database at path: {DatabasePath}", _configuration.DatabasePath);

            // Ensure directory exists
            _configuration.EnsureDirectoryExists();

            // Connect to database
            await ConnectAsync();

            // Initialize schema and indexes
            await InitializeSchemaAsync();

            // Perform any necessary migrations
            await PerformMigrationsAsync();

            _logger.LogInformation("Database initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize database");
            throw;
        }
    }

    /// <summary>
    /// Connects to the database
    /// </summary>
    public async Task ConnectAsync()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                if (_database != null)
                {
                    _logger.LogDebug("Database already connected");
                    return;
                }

                try
                {
                    _logger.LogDebug("Connecting to database with connection string: {ConnectionString}", 
                        _configuration.ConnectionString.Replace(_configuration.EncryptionPassword ?? "", "***"));

                    _database = new LiteDatabase(_configuration.ConnectionString);

                    _logger.LogInformation("Successfully connected to database");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to connect to database");
                    throw;
                }
            }
        });
    }

    /// <summary>
    /// Disconnects from the database
    /// </summary>
    public async Task DisconnectAsync()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                if (_database != null)
                {
                    _database.Dispose();
                    _database = null;
                    _logger.LogInformation("Disconnected from database");
                }
            }
        });
    }

    /// <summary>
    /// Gets the LiteDatabase instance
    /// </summary>
    /// <returns>LiteDatabase instance</returns>
    public LiteDatabase GetDatabase()
    {
        if (_database == null)
        {
            throw new InvalidOperationException("Database is not connected. Call ConnectAsync() first.");
        }

        return _database;
    }

    /// <summary>
    /// Gets a collection of the specified type
    /// </summary>
    /// <typeparam name="T">Type of documents in the collection</typeparam>
    /// <param name="collectionName">Name of the collection</param>
    /// <returns>LiteCollection instance</returns>
    public ILiteCollection<T> GetCollection<T>(string collectionName)
    {
        var database = GetDatabase();
        return database.GetCollection<T>(collectionName);
    }

    /// <summary>
    /// Checks database connectivity
    /// </summary>
    /// <returns>True if database is accessible</returns>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            if (_database == null)
            {
                await ConnectAsync();
            }

            // Test by getting a simple collection count
            var testCollection = _database!.GetCollection("_test");
            _ = testCollection.Count();

            _logger.LogDebug("Database connection test successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database connection test failed");
            return false;
        }
    }

    /// <summary>
    /// Creates a backup of the database
    /// </summary>
    /// <param name="backupPath">Optional custom backup path</param>
    /// <returns>Path to the created backup file</returns>
    public async Task<string> BackupAsync(string? backupPath = null)
    {
        try
        {
            if (!_configuration.DatabaseExists())
            {
                throw new InvalidOperationException("Cannot backup database that doesn't exist");
            }

            // Generate backup filename if not provided
            if (string.IsNullOrWhiteSpace(backupPath))
            {
                var backupDir = _configuration.GetBackupDirectory();
                Directory.CreateDirectory(backupDir);
                
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"snapdesk_backup_{timestamp}.db";
                backupPath = Path.Combine(backupDir, backupFileName);
            }

            // Ensure backup directory exists
            var backupDirectory = Path.GetDirectoryName(backupPath);
            if (!string.IsNullOrEmpty(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }

            // Disconnect temporarily for file copy
            var wasConnected = IsConnected;
            if (wasConnected)
            {
                await DisconnectAsync();
            }

            // Copy database file
            await Task.Run(() => File.Copy(_configuration.DatabasePath, backupPath, true));

            // Reconnect if was connected
            if (wasConnected)
            {
                await ConnectAsync();
            }

            _logger.LogInformation("Database backup created at: {BackupPath}", backupPath);

            // Clean up old backups
            await CleanupOldBackupsAsync();

            return backupPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database backup");
            throw;
        }
    }

    /// <summary>
    /// Restores database from a backup file
    /// </summary>
    /// <param name="backupPath">Path to the backup file</param>
    public async Task RestoreAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException($"Backup file not found: {backupPath}");
            }

            _logger.LogInformation("Restoring database from backup: {BackupPath}", backupPath);

            // Disconnect from current database
            await DisconnectAsync();

            // Create backup of current database before restoring
            if (_configuration.DatabaseExists())
            {
                var currentBackupPath = _configuration.DatabasePath + ".pre_restore_backup";
                File.Copy(_configuration.DatabasePath, currentBackupPath, true);
                _logger.LogInformation("Created backup of current database before restore");
            }

            // Copy backup file to database location
            await Task.Run(() => File.Copy(backupPath, _configuration.DatabasePath, true));

            // Reconnect to restored database
            await ConnectAsync();

            _logger.LogInformation("Database restored successfully from backup");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore database from backup");
            throw;
        }
    }

    /// <summary>
    /// Gets database statistics
    /// </summary>
    /// <returns>Database statistics</returns>
    public async Task<DatabaseStatistics> GetStatisticsAsync()
    {
        try
        {
            var stats = new DatabaseStatistics
            {
                DatabaseSizeBytes = _configuration.GetDatabaseFileSize(),
                IsHealthy = await TestConnectionAsync(),
                Version = "LiteDB 5.0"
            };

            if (IsConnected)
            {
                var database = GetDatabase();
                
                // Count total documents across all collections
                var collectionNames = database.GetCollectionNames();
                var totalEntities = 0;

                foreach (var collectionName in collectionNames)
                {
                    var collection = database.GetCollection(collectionName);
                    totalEntities += collection.Count();
                }

                stats.TotalEntities = totalEntities;
                stats.HealthMessage = "Database is healthy and accessible";
            }
            else
            {
                stats.HealthMessage = "Database is not connected";
            }

            // Check for recent backups
            var backupDir = _configuration.GetBackupDirectory();
            if (Directory.Exists(backupDir))
            {
                var backupFiles = Directory.GetFiles(backupDir, "*.db");
                if (backupFiles.Length > 0)
                {
                    var latestBackup = backupFiles
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(f => f.LastWriteTime)
                        .First();
                    
                    stats.LastBackupTime = latestBackup.LastWriteTime;
                }
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get database statistics");
            return new DatabaseStatistics
            {
                IsHealthy = false,
                HealthMessage = $"Error retrieving statistics: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Initializes the database schema and indexes
    /// </summary>
    private async Task InitializeSchemaAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                var database = GetDatabase();

                // Initialize LayoutProfiles collection with indexes
                var layoutsCollection = database.GetCollection<SnapDesk.Core.LayoutProfile>("layouts");
                layoutsCollection.EnsureIndex(x => x.Name, true); // Unique name index
                layoutsCollection.EnsureIndex(x => x.CreatedAt);
                layoutsCollection.EnsureIndex(x => x.IsActive);

                // Initialize Hotkeys collection with indexes
                var hotkeysCollection = database.GetCollection<SnapDesk.Core.HotkeyInfo>("hotkeys");
                hotkeysCollection.EnsureIndex(x => x.Keys, true); // Unique key combination
                hotkeysCollection.EnsureIndex(x => x.IsEnabled);
                hotkeysCollection.EnsureIndex(x => x.Action);

                // Initialize Settings collection with indexes
                var settingsCollection = database.GetCollection("settings");
                settingsCollection.EnsureIndex("key", true); // Unique setting key

                _logger.LogDebug("Database schema initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize database schema");
                throw;
            }
        });
    }

    /// <summary>
    /// Performs database migrations if needed
    /// </summary>
    private async Task PerformMigrationsAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                // TODO: Implement migration logic when needed
                // For now, this is a placeholder for future migrations
                
                _logger.LogDebug("Database migrations completed (none required)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform database migrations");
                throw;
            }
        });
    }

    /// <summary>
    /// Cleans up old backup files based on retention policy
    /// </summary>
    private async Task CleanupOldBackupsAsync()
    {
        if (!_configuration.EnableAutoBackup)
            return;

        await Task.Run(() =>
        {
            try
            {
                var backupDir = _configuration.GetBackupDirectory();
                if (!Directory.Exists(backupDir))
                    return;

                var cutoffDate = DateTime.Now.AddDays(-_configuration.BackupRetentionDays);
                var backupFiles = Directory.GetFiles(backupDir, "*.db");

                foreach (var backupFile in backupFiles)
                {
                    var fileInfo = new FileInfo(backupFile);
                    if (fileInfo.LastWriteTime < cutoffDate)
                    {
                        File.Delete(backupFile);
                        _logger.LogDebug("Deleted old backup file: {BackupFile}", backupFile);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup old backup files");
            }
        });
    }

    /// <summary>
    /// Disposes the database service
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _database?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Interface for database service operations
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    /// Database configuration
    /// </summary>
    DatabaseConfiguration Configuration { get; }

    /// <summary>
    /// Whether the database is connected
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Database file path
    /// </summary>
    string DatabasePath { get; }

    /// <summary>
    /// Initializes the database
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Connects to the database
    /// </summary>
    Task ConnectAsync();

    /// <summary>
    /// Disconnects from the database
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Gets the LiteDatabase instance
    /// </summary>
    LiteDatabase GetDatabase();

    /// <summary>
    /// Gets a collection
    /// </summary>
    ILiteCollection<T> GetCollection<T>(string collectionName);

    /// <summary>
    /// Tests database connectivity
    /// </summary>
    Task<bool> TestConnectionAsync();

    /// <summary>
    /// Creates a database backup
    /// </summary>
    Task<string> BackupAsync(string? backupPath = null);

    /// <summary>
    /// Restores database from backup
    /// </summary>
    Task RestoreAsync(string backupPath);

    /// <summary>
    /// Gets database statistics
    /// </summary>
    Task<DatabaseStatistics> GetStatisticsAsync();
}
