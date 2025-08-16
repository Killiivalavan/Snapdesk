using Microsoft.Extensions.Logging;
using SnapDesk.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace SnapDesk.Core.Services;

/// <summary>
/// Service for managing application lifecycle and background operations
/// </summary>
public interface IApplicationLifecycleService
{
    /// <summary>
    /// Starts the application lifecycle service
    /// </summary>
    Task StartAsync();
    
    /// <summary>
    /// Stops the application lifecycle service
    /// </summary>
    Task StopAsync();
    
    /// <summary>
    /// Gets the current application state
    /// </summary>
    /// <returns>Current application state</returns>
    ApplicationState GetApplicationState();
    
    /// <summary>
    /// Suspends the application (minimize to tray)
    /// </summary>
    Task SuspendAsync();
    
    /// <summary>
    /// Resumes the application (show main window)
    /// </summary>
    Task ResumeAsync();
    
    /// <summary>
    /// Shuts down the application gracefully
    /// </summary>
    Task ShutdownAsync();
}

/// <summary>
/// Implementation of application lifecycle service
/// </summary>
public class ApplicationLifecycleService : IApplicationLifecycleService
{
    private readonly ILogger<ApplicationLifecycleService> _logger;
    private readonly IStartupService _startupService;
    private readonly IHotkeyService _hotkeyService;
    private readonly ILayoutService _layoutService;
    
    private ApplicationState _currentState = ApplicationState.Starting;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isRunning = false;

    public ApplicationLifecycleService(
        ILogger<ApplicationLifecycleService> logger,
        IStartupService startupService,
        IHotkeyService hotkeyService,
        ILayoutService layoutService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _startupService = startupService ?? throw new ArgumentNullException(nameof(startupService));
        _hotkeyService = hotkeyService ?? throw new ArgumentNullException(nameof(hotkeyService));
        _layoutService = layoutService ?? throw new ArgumentNullException(nameof(layoutService));
    }

    public async Task StartAsync()
    {
        try
        {
            _logger.LogInformation("Starting application lifecycle service");
            
            _cancellationTokenSource = new CancellationTokenSource();
            _isRunning = true;
            _currentState = ApplicationState.Starting;
            
            // Initialize startup configuration
            await InitializeStartupAsync();
            
            // Start background operations
            _ = Task.Run(BackgroundOperationsAsync, _cancellationTokenSource.Token);
            
            _currentState = ApplicationState.Running;
            _logger.LogInformation("Application lifecycle service started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start application lifecycle service");
            _currentState = ApplicationState.Error;
            throw;
        }
    }

    public async Task StopAsync()
    {
        try
        {
            _logger.LogInformation("Stopping application lifecycle service");
            
            _isRunning = false;
            _currentState = ApplicationState.Stopping;
            
            // Cancel background operations
            _cancellationTokenSource?.Cancel();
            
            // Clean up resources
            await CleanupAsync();
            
            _currentState = ApplicationState.Stopped;
            _logger.LogInformation("Application lifecycle service stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop application lifecycle service");
            _currentState = ApplicationState.Error;
            throw;
        }
    }

    public ApplicationState GetApplicationState()
    {
        return _currentState;
    }

    public async Task SuspendAsync()
    {
        try
        {
            _logger.LogInformation("Suspending application");
            
            _currentState = ApplicationState.Suspended;
            
            // Suspend hotkeys if needed
            await _hotkeyService.SuspendHotkeysAsync();
            
            _logger.LogInformation("Application suspended successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to suspend application");
            throw;
        }
    }

    public async Task ResumeAsync()
    {
        try
        {
            _logger.LogInformation("Resuming application");
            
            _currentState = ApplicationState.Running;
            
            // Resume hotkeys
            await _hotkeyService.ResumeHotkeysAsync();
            
            _logger.LogInformation("Application resumed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resume application");
            throw;
        }
    }

    public async Task ShutdownAsync()
    {
        try
        {
            _logger.LogInformation("Shutting down application");
            
            _currentState = ApplicationState.ShuttingDown;
            
            // Stop the service
            await StopAsync();
            
            _logger.LogInformation("Application shutdown completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to shutdown application");
            throw;
        }
    }

    private Task InitializeStartupAsync()
    {
        try
        {
            var startupConfig = _startupService.GetStartupConfiguration();
            _logger.LogInformation("Startup configuration: StartWithWindows={StartWithWindows}, StartMinimized={StartMinimized}", 
                startupConfig.StartWithWindows, startupConfig.StartMinimized);
            
            // Apply startup configuration if needed
            if (startupConfig.StartWithWindows && !startupConfig.IsEnabled)
            {
                var success = _startupService.SetStartupEnabled(true);
                _logger.LogInformation("Startup registration {Result}", success ? "succeeded" : "failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize startup configuration");
        }
        
        return Task.CompletedTask;
    }

    private async Task BackgroundOperationsAsync()
    {
        try
        {
            _logger.LogInformation("Background operations started");
            
            while (_isRunning && !_cancellationTokenSource?.Token.IsCancellationRequested == true)
            {
                try
                {
                    // Perform periodic background tasks
                    await PerformBackgroundTasksAsync();
                    
                    // Wait before next iteration
                    await Task.Delay(TimeSpan.FromMinutes(1), _cancellationTokenSource?.Token ?? CancellationToken.None);
                }
                catch (OperationCanceledException)
                {
                    // Normal cancellation, exit loop
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in background operations");
                    await Task.Delay(TimeSpan.FromSeconds(30), _cancellationTokenSource?.Token ?? CancellationToken.None);
                }
            }
            
            _logger.LogInformation("Background operations stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background operations failed");
        }
    }

    private async Task PerformBackgroundTasksAsync()
    {
        try
        {
            // Check for layout validation
            await ValidateLayoutsAsync();
            
            // Check for hotkey conflicts
            await CheckHotkeyConflictsAsync();
            
            // Log application health
            LogApplicationHealth();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform background tasks");
        }
    }

    private async Task ValidateLayoutsAsync()
    {
        try
        {
            var layouts = await _layoutService.GetAllLayoutsAsync();
            foreach (var layout in layouts)
            {
                var validation = await _layoutService.ValidateLayoutAsync(layout.Id);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("Layout {LayoutName} (ID: {LayoutId}) validation failed: {Errors}", 
                        layout.Name, layout.Id, string.Join(", ", validation.Errors));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate layouts");
        }
    }

    private async Task CheckHotkeyConflictsAsync()
    {
        try
        {
            var conflicts = await _hotkeyService.GetHotkeyConflictsAsync();
            if (conflicts.Any())
            {
                _logger.LogWarning("Found {ConflictCount} hotkey conflicts", conflicts.Count());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check hotkey conflicts");
        }
    }

    private void LogApplicationHealth()
    {
        try
        {
            _logger.LogDebug("Application health check - State: {State}, Running: {IsRunning}", 
                _currentState, _isRunning);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log application health");
        }
    }

    private async Task CleanupAsync()
    {
        try
        {
            // Suspend hotkeys
            await _hotkeyService.SuspendHotkeysAsync();
            
            // Dispose cancellation token source
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            
            _logger.LogInformation("Cleanup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup resources");
        }
    }
}

/// <summary>
/// Represents the current state of the application
/// </summary>
public enum ApplicationState
{
    /// <summary>
    /// Application is starting up
    /// </summary>
    Starting,
    
    /// <summary>
    /// Application is running normally
    /// </summary>
    Running,
    
    /// <summary>
    /// Application is suspended (minimized to tray)
    /// </summary>
    Suspended,
    
    /// <summary>
    /// Application is stopping
    /// </summary>
    Stopping,
    
    /// <summary>
    /// Application has stopped
    /// </summary>
    Stopped,
    
    /// <summary>
    /// Application is shutting down
    /// </summary>
    ShuttingDown,
    
    /// <summary>
    /// Application encountered an error
    /// </summary>
    Error
}
