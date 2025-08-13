using System;
using System.IO;

namespace SnapDesk.Data.Configuration;

/// <summary>
/// Database configuration settings for LiteDB
/// </summary>
public class DatabaseConfiguration
{
    /// <summary>
    /// Default database filename
    /// </summary>
    public const string DefaultDatabaseName = "snapdesk.db";

    /// <summary>
    /// Application data folder name
    /// </summary>
    public const string ApplicationFolderName = ".snapdesk";

    /// <summary>
    /// Database file path
    /// </summary>
    public string DatabasePath { get; set; } = string.Empty;

    /// <summary>
    /// Connection string for LiteDB
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Whether to enable database encryption
    /// </summary>
    public bool EnableEncryption { get; set; } = false;

    /// <summary>
    /// Database password for encryption
    /// </summary>
    public string? EncryptionPassword { get; set; }

    /// <summary>
    /// Whether to enable file sharing
    /// </summary>
    public bool EnableSharing { get; set; } = true;

    /// <summary>
    /// Whether to enable read-only mode
    /// </summary>
    public bool ReadOnlyMode { get; set; } = false;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum database size in MB (0 = unlimited)
    /// </summary>
    public long MaxSizeMB { get; set; } = 0;

    /// <summary>
    /// Whether to enable journal mode
    /// </summary>
    public bool EnableJournal { get; set; } = true;

    /// <summary>
    /// Whether to enable auto-backup
    /// </summary>
    public bool EnableAutoBackup { get; set; } = true;

    /// <summary>
    /// Backup retention days
    /// </summary>
    public int BackupRetentionDays { get; set; } = 7;

    /// <summary>
    /// Creates a default database configuration
    /// </summary>
    /// <returns>Default configuration</returns>
    public static DatabaseConfiguration CreateDefault()
    {
        var userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var appDataPath = Path.Combine(userProfilePath, ApplicationFolderName);
        var databasePath = Path.Combine(appDataPath, DefaultDatabaseName);

        return new DatabaseConfiguration
        {
            DatabasePath = databasePath,
            ConnectionString = BuildConnectionString(databasePath),
            EnableEncryption = false,
            EnableSharing = true,
            ReadOnlyMode = false,
            TimeoutSeconds = 60,
            MaxSizeMB = 0,
            EnableJournal = true,
            EnableAutoBackup = true,
            BackupRetentionDays = 7
        };
    }

    /// <summary>
    /// Creates a database configuration for a specific location
    /// </summary>
    /// <param name="databasePath">Custom database path</param>
    /// <returns>Configuration for the specified path</returns>
    public static DatabaseConfiguration CreateForPath(string databasePath)
    {
        var config = CreateDefault();
        config.DatabasePath = databasePath;
        config.ConnectionString = BuildConnectionString(databasePath);
        return config;
    }

    /// <summary>
    /// Creates a database configuration with encryption
    /// </summary>
    /// <param name="password">Encryption password</param>
    /// <returns>Configuration with encryption enabled</returns>
    public static DatabaseConfiguration CreateWithEncryption(string password)
    {
        var config = CreateDefault();
        config.EnableEncryption = true;
        config.EncryptionPassword = password;
        config.ConnectionString = BuildConnectionString(config.DatabasePath, password);
        return config;
    }

    /// <summary>
    /// Builds a LiteDB connection string from configuration
    /// </summary>
    /// <param name="databasePath">Database file path</param>
    /// <param name="password">Optional encryption password</param>
    /// <returns>Connection string</returns>
    public static string BuildConnectionString(string databasePath, string? password = null)
    {
        var connectionStringBuilder = new System.Text.StringBuilder();
        connectionStringBuilder.Append($"Filename={databasePath}");

        if (!string.IsNullOrWhiteSpace(password))
        {
            connectionStringBuilder.Append($";Password={password}");
        }

        connectionStringBuilder.Append(";Connection=shared");
        connectionStringBuilder.Append(";Timeout=60");

        return connectionStringBuilder.ToString();
    }

    /// <summary>
    /// Updates the connection string based on current configuration
    /// </summary>
    public void UpdateConnectionString()
    {
        ConnectionString = BuildConnectionString(DatabasePath, EncryptionPassword);
    }

    /// <summary>
    /// Ensures the database directory exists
    /// </summary>
    public void EnsureDirectoryExists()
    {
        var directory = Path.GetDirectoryName(DatabasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <summary>
    /// Gets the backup directory path
    /// </summary>
    /// <returns>Backup directory path</returns>
    public string GetBackupDirectory()
    {
        var directory = Path.GetDirectoryName(DatabasePath);
        return Path.Combine(directory ?? string.Empty, "backups");
    }

    /// <summary>
    /// Validates the configuration
    /// </summary>
    /// <returns>True if configuration is valid</returns>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(DatabasePath))
            return false;

        if (EnableEncryption && string.IsNullOrWhiteSpace(EncryptionPassword))
            return false;

        if (TimeoutSeconds <= 0)
            return false;

        if (BackupRetentionDays < 0)
            return false;

        return true;
    }

    /// <summary>
    /// Gets the database file size in bytes
    /// </summary>
    /// <returns>File size in bytes, or 0 if file doesn't exist</returns>
    public long GetDatabaseFileSize()
    {
        if (File.Exists(DatabasePath))
        {
            return new FileInfo(DatabasePath).Length;
        }
        return 0;
    }

    /// <summary>
    /// Checks if the database file exists
    /// </summary>
    /// <returns>True if database file exists</returns>
    public bool DatabaseExists()
    {
        return File.Exists(DatabasePath);
    }
}
