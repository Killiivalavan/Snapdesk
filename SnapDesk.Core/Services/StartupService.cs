using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace SnapDesk.Core.Services;

/// <summary>
/// Service for managing application startup behavior and Windows integration
/// </summary>
public interface IStartupService
{
    /// <summary>
    /// Checks if the application is set to start with Windows
    /// </summary>
    /// <returns>True if the app starts with Windows</returns>
    bool IsStartupEnabled();
    
    /// <summary>
    /// Enables or disables starting the application with Windows
    /// </summary>
    /// <param name="enable">True to enable startup, false to disable</param>
    /// <returns>True if the operation was successful</returns>
    bool SetStartupEnabled(bool enable);
    
    /// <summary>
    /// Gets the startup behavior configuration
    /// </summary>
    /// <returns>Startup configuration</returns>
    StartupConfiguration GetStartupConfiguration();
    
    /// <summary>
    /// Sets the startup behavior configuration
    /// </summary>
    /// <param name="config">Startup configuration to apply</param>
    /// <returns>True if the operation was successful</returns>
    bool SetStartupConfiguration(StartupConfiguration config);
}

/// <summary>
/// Implementation of startup service for Windows
/// </summary>
public class StartupService : IStartupService
{
    private readonly ILogger<StartupService> _logger;
    private const string RegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "SnapDesk";

    public StartupService(ILogger<StartupService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool IsStartupEnabled()
    {
#if WINDOWS
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey);
            if (key != null)
            {
                var value = key.GetValue(AppName) as string;
                return !string.IsNullOrEmpty(value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check startup status");
        }
#endif
        
        return false;
    }

    public bool SetStartupEnabled(bool enable)
    {
#if WINDOWS
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
            if (key != null)
            {
                if (enable)
                {
                    var exePath = GetExecutablePath();
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        key.SetValue(AppName, $"\"{exePath}\" --startup");
                        _logger.LogInformation("Startup enabled for SnapDesk");
                        return true;
                    }
                    else
                    {
                        _logger.LogError("Could not determine executable path");
                        return false;
                    }
                }
                else
                {
                    key.DeleteValue(AppName, false);
                    _logger.LogInformation("Startup disabled for SnapDesk");
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set startup status to {Enable}", enable);
        }
#endif
        
        return false;
    }

    public StartupConfiguration GetStartupConfiguration()
    {
        return new StartupConfiguration
        {
            IsEnabled = IsStartupEnabled(),
            StartMinimized = true, // Default to starting minimized
            StartWithWindows = IsStartupEnabled()
        };
    }

    public bool SetStartupConfiguration(StartupConfiguration config)
    {
        try
        {
            var success = SetStartupEnabled(config.StartWithWindows);
            if (success)
            {
                _logger.LogInformation("Startup configuration updated: Enabled={Enabled}, StartMinimized={StartMinimized}", 
                    config.StartWithWindows, config.StartMinimized);
            }
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set startup configuration");
            return false;
        }
    }

    private string GetExecutablePath()
    {
        try
        {
            // Get the path of the current executable
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            
            // If running from development environment, try to find the actual executable
            if (exePath.Contains("bin") || exePath.Contains("obj"))
            {
                // Look for the UI project executable
                var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
                var uiProjectDir = FindUIProjectDirectory(currentDir);
                
                if (uiProjectDir != null)
                {
                    var exeFile = uiProjectDir.GetFiles("*.exe").FirstOrDefault();
                    if (exeFile != null)
                    {
                        return exeFile.FullName;
                    }
                }
            }
            
            return exePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to determine executable path");
            return string.Empty;
        }
    }

    private DirectoryInfo? FindUIProjectDirectory(DirectoryInfo startDir)
    {
        try
        {
            var current = startDir;
            while (current != null && current.Parent != null)
            {
                var uiDir = current.GetDirectories("SnapDesk.UI").FirstOrDefault();
                if (uiDir != null)
                {
                    return uiDir;
                }
                current = current.Parent;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find UI project directory");
        }
        
        return null;
    }
}

/// <summary>
/// Configuration for application startup behavior
/// </summary>
public class StartupConfiguration
{
    /// <summary>
    /// Whether the application should start with Windows
    /// </summary>
    public bool StartWithWindows { get; set; }
    
    /// <summary>
    /// Whether the application should start minimized
    /// </summary>
    public bool StartMinimized { get; set; }
    
    /// <summary>
    /// Whether startup is currently enabled
    /// </summary>
    public bool IsEnabled { get; set; }
}
