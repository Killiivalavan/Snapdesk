using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using System.Linq;
using Avalonia.Markup.Xaml;
using SnapDesk.UI.ViewModels;
using SnapDesk.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SnapDesk.Core.Services;
using SnapDesk.Core.Interfaces;
using SnapDesk.Data.Services;
using SnapDesk.Data.Configuration;
using SnapDesk.Data.Repositories;
using SnapDesk.Platform;
using SnapDesk.Platform.Interfaces;
using SnapDesk.Platform.Windows;
using SnapDesk.Platform.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SnapDesk.UI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private TrayIcon? _trayIcon;
    private MainWindow? _mainWindow;
    private ILogger<App>? _logger;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            // Create main window immediately for testing
            _mainWindow = new MainWindow();
            desktop.MainWindow = _mainWindow;
            
            // Show a loading state initially
            var loadingViewModel = new MainWindowViewModel();
            loadingViewModel.StatusMessage = "Initializing services...";
            loadingViewModel.IsLoading = true;
            _mainWindow.DataContext = loadingViewModel;
            
            // Show the window immediately
            _mainWindow.Show();
            Console.WriteLine("Main window shown with loading state");
            
            // Initialize services and then create the real ViewModel
            _ = Task.Run(async () =>
            {
                try
                {
                    Console.WriteLine("Starting background service initialization...");
                    await InitializeServicesAsync();
                    
                    // Update the window's DataContext with the real service-based ViewModel
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        try
                        {
                            if (_serviceProvider != null)
                            {
                                // Get the services directly to ensure they're available
                                var layoutService = _serviceProvider.GetRequiredService<ILayoutService>();
                                var hotkeyService = _serviceProvider.GetRequiredService<IHotkeyService>();
                                var logger = _serviceProvider.GetRequiredService<ILogger<MainWindowViewModel>>();
                                
                                // Create the ViewModel manually with services to ensure proper injection
                                var viewModel = new MainWindowViewModel(layoutService, hotkeyService, logger);
                                
                                // Set the DataContext
                                _mainWindow.DataContext = viewModel;
                                
                                // Force a UI refresh to ensure the binding is updated
                                viewModel.StatusMessage = "Services initialized successfully - Connected to database";
                                
                                // Test if the binding is working by checking the UI
                                var currentContext = _mainWindow.DataContext;
                                Console.WriteLine($"Current DataContext type: {currentContext?.GetType().Name}");
                                Console.WriteLine($"Current DataContext status: {((MainWindowViewModel?)currentContext)?.StatusMessage}");
                                
                                // Show service status in the UI for debugging
                                var layoutServiceType = viewModel.GetType().GetField("_layoutService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(viewModel)?.GetType().Name ?? "NULL";
                                var hotkeyServiceType = viewModel.GetType().GetField("_hotkeyService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(viewModel)?.GetType().Name ?? "NULL";
                                viewModel.StatusMessage = $"Services OK - Layout: {layoutServiceType}, Hotkey: {hotkeyServiceType}";
                                
                                Console.WriteLine("Real ViewModel set successfully with services");
                                Console.WriteLine($"ViewModel status: {viewModel.StatusMessage}");
                                Console.WriteLine($"ViewModel has layout service: {viewModel.GetType().GetField("_layoutService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(viewModel) != null}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to set real ViewModel: {ex.Message}");
                            // Fallback: show error state
                            var errorViewModel = new MainWindowViewModel();
                            errorViewModel.StatusMessage = $"Service initialization failed: {ex.Message}";
                            errorViewModel.IsLoading = false;
                            _mainWindow.DataContext = errorViewModel;
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Background service initialization failed: {ex.Message}");
                    // Show error state in UI
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        var errorViewModel = new MainWindowViewModel();
                        errorViewModel.StatusMessage = $"Service initialization failed: {ex.Message}";
                        errorViewModel.IsLoading = false;
                        _mainWindow.DataContext = errorViewModel;
                    });
                }
            });
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task InitializeServicesAsync()
    {
        try
        {
            Console.WriteLine("Starting service initialization...");
            
            // Create configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Database:ConnectionString"] = GetDefaultDatabasePath(),
                    ["Database:EncryptionKey"] = "default-encryption-key-32-chars-long!!",
                    ["Database:BackupPath"] = "./backups",
                    ["Database:EnableLogging"] = "true"
                })
                .Build();

            // Setup services
            var services = new ServiceCollection();
            
            // Add configuration
            services.AddSingleton<IConfiguration>(configuration);
            
            // Add logging
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
            });
            
            // Create database configuration
            var dbPath = GetDefaultDatabasePath();
            var dbConfig = DatabaseConfiguration.CreateForPath(dbPath);
            services.AddSingleton<DatabaseConfiguration>(dbConfig);
            
            // Register database and repository services
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<ILayoutRepository, LayoutRepository>();
            services.AddSingleton<IHotkeyRepository, HotkeyRepository>();
            services.AddSingleton<ISettingsRepository, SettingsRepository>();
            
            // Register platform services FIRST (they have no dependencies)
            // TEMPORARILY: Force use of stub services to get the app working
            // TODO: Fix Windows platform services later
            services.AddSingleton<IWindowApi, StubWindowApi>();
            services.AddSingleton<IHotkeyApi, StubHotkeyApi>();
            Console.WriteLine("Using stub platform services for now");
            
            // Register core services (they depend on platform services)
            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<IHotkeyService, HotkeyService>();
            services.AddSingleton<ILayoutService, LayoutService>();
            
            // Register lifecycle services LAST (they depend on core services)
            services.AddSingleton<IStartupService, StartupService>();
            services.AddSingleton<IApplicationLifecycleService, ApplicationLifecycleService>();
            
            // Register ViewModels
            services.AddTransient<MainWindowViewModel>();
            
            // Add logging factory for ViewModels
            services.AddLogging();
            
            // Test service registration before building
            Console.WriteLine("Testing service registration...");
            var tempServices = services.BuildServiceProvider();
            try
            {
                var testLayoutService = tempServices.GetService<ILayoutService>();
                var testHotkeyService = tempServices.GetService<IHotkeyService>();
                var testWindowService = tempServices.GetService<IWindowService>();
                Console.WriteLine($"Service registration test: Layout={testLayoutService != null}, Hotkey={testHotkeyService != null}, Window={testWindowService != null}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Service registration test failed: {ex.Message}");
            }
            finally
            {
                tempServices.Dispose();
            }
            
            Console.WriteLine("Building service provider...");
            // Build service provider
            _serviceProvider = services.BuildServiceProvider();
            
            Console.WriteLine("Initializing database...");
            // Initialize database
            var dbService = _serviceProvider.GetRequiredService<IDatabaseService>();
            await dbService.InitializeAsync();
            Console.WriteLine("Database initialized successfully");
            
            Console.WriteLine("Initializing hotkey service...");
            // Initialize hotkey service
            var hotkeyService = _serviceProvider.GetRequiredService<IHotkeyService>();
            await InitializeHotkeysAsync(hotkeyService);
            Console.WriteLine("Hotkey service initialized successfully");
            
            Console.WriteLine("Initializing application lifecycle service...");
            // Initialize application lifecycle service
            var lifecycleService = _serviceProvider.GetRequiredService<IApplicationLifecycleService>();
            await lifecycleService.StartAsync();
            Console.WriteLine("Application lifecycle service initialized successfully");
            
            Console.WriteLine("Getting logger service...");
            _logger = _serviceProvider.GetRequiredService<ILogger<App>>();
            _logger.LogInformation("Services initialized successfully");
            
            Console.WriteLine("=== SERVICES INITIALIZED SUCCESSFULLY ===");
            // Debug output
            System.Diagnostics.Debug.WriteLine("=== SERVICES INITIALIZED SUCCESSFULLY ===");
            System.Diagnostics.Debug.WriteLine($"Database Service: {_serviceProvider.GetService<IDatabaseService>() != null}");
            System.Diagnostics.Debug.WriteLine($"Layout Service: {_serviceProvider.GetService<LayoutService>() != null}");
            System.Diagnostics.Debug.WriteLine($"Layout Repository: {_serviceProvider.GetService<ILayoutRepository>() != null}");
            System.Diagnostics.Debug.WriteLine($"Hotkey Service: {_serviceProvider.GetService<IHotkeyService>() != null}");
            System.Diagnostics.Debug.WriteLine($"Window Service: {_serviceProvider.GetService<IWindowService>() != null}");
            System.Diagnostics.Debug.WriteLine($"Logger Factory: {_serviceProvider.GetService<ILoggerFactory>() != null}");
            System.Diagnostics.Debug.WriteLine("=========================================");
            
            // Test platform services specifically
            Console.WriteLine("Testing platform services...");
            try
            {
                var windowApi = _serviceProvider.GetService<IWindowApi>();
                var hotkeyApi = _serviceProvider.GetService<IHotkeyApi>();
                Console.WriteLine($"Platform services: WindowApi={windowApi?.GetType().Name ?? "NULL"}, HotkeyApi={hotkeyApi?.GetType().Name ?? "NULL"}");
                
                if (windowApi != null)
                {
                    // Test basic window API functionality
                    var desktopWindow = windowApi.GetDesktopWindow();
                    Console.WriteLine($"Desktop window handle: {desktopWindow}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Platform service test failed: {ex.Message}");
            }
            
            // Test each service in the dependency chain
            Console.WriteLine("Testing service dependency chain...");
            try
            {
                // Test repositories
                var layoutRepo = _serviceProvider.GetService<ILayoutRepository>();
                var hotkeyRepo = _serviceProvider.GetService<IHotkeyRepository>();
                Console.WriteLine($"Repositories: Layout={layoutRepo?.GetType().Name ?? "NULL"}, Hotkey={hotkeyRepo?.GetType().Name ?? "NULL"}");
                
                // Test core services
                var windowServiceTest = _serviceProvider.GetService<IWindowService>();
                var hotkeyServiceTest = _serviceProvider.GetService<IHotkeyService>();
                var layoutServiceTest = _serviceProvider.GetService<ILayoutService>();
                Console.WriteLine($"Core services: Window={windowServiceTest?.GetType().Name ?? "NULL"}, Hotkey={hotkeyServiceTest?.GetType().Name ?? "NULL"}, Layout={layoutServiceTest?.GetType().Name ?? "NULL"}");
                
                // Test if any service is null and why
                if (layoutServiceTest == null)
                {
                    Console.WriteLine("LayoutService is NULL - checking dependencies...");
                    if (layoutRepo == null) Console.WriteLine("  - LayoutRepository is NULL");
                    if (windowServiceTest == null) Console.WriteLine("  - WindowService is NULL");
                    
                    // Try to create LayoutService manually to see the exact error
                    try
                    {
                        var logger = _serviceProvider.GetService<ILogger<LayoutService>>();
                        var manualLayoutService = new LayoutService(layoutRepo!, logger!, windowServiceTest!);
                        Console.WriteLine("Manual LayoutService creation succeeded");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Manual LayoutService creation failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Service dependency chain test failed: {ex.Message}");
            }
            
            // Test creating a ViewModel to see if services are properly injected
            try
            {
                var testViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
                Console.WriteLine($"Test ViewModel created successfully. Services: Layout={testViewModel.GetType().GetField("_layoutService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(testViewModel) != null}, Hotkey={testViewModel.GetType().GetField("_hotkeyService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(testViewModel) != null}");
                
                // Also test direct service resolution
                var layoutServiceTest = _serviceProvider.GetService<ILayoutService>();
                var hotkeyServiceTest = _serviceProvider.GetService<IHotkeyService>();
                Console.WriteLine($"Direct service resolution: Layout={layoutServiceTest != null}, Hotkey={hotkeyServiceTest != null}");
                
                // Test ViewModel creation with explicit service injection
                var manualViewModel = new MainWindowViewModel(layoutServiceTest, hotkeyServiceTest, _serviceProvider.GetService<ILogger<MainWindowViewModel>>());
                Console.WriteLine($"Manual ViewModel created. Services: Layout={manualViewModel.GetType().GetField("_layoutService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(manualViewModel) != null}, Hotkey={manualViewModel.GetType().GetField("_hotkeyService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(manualViewModel) != null}");
                
                // Test if the manual ViewModel shows the correct status
                Console.WriteLine($"Manual ViewModel status: {manualViewModel.StatusMessage}");
                if (manualViewModel.StatusMessage.Contains("Services available"))
                {
                    Console.WriteLine("✅ Manual ViewModel shows services are available!");
                }
                else
                {
                    Console.WriteLine($"❌ Manual ViewModel shows: {manualViewModel.StatusMessage}");
                }
                
                // Test if the DI container creates ViewModels with services
                var diViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
                Console.WriteLine($"DI ViewModel created. Services: Layout={diViewModel.GetType().GetField("_layoutService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(diViewModel) != null}, Hotkey={diViewModel.GetType().GetField("_hotkeyService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(diViewModel) != null}");
                Console.WriteLine($"DI ViewModel status: {diViewModel.StatusMessage}");
                
                // Temporarily show the manual ViewModel in the UI to test service injection
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _mainWindow.DataContext = manualViewModel;
                    Console.WriteLine("Test ViewModel set in UI to verify service injection");
                    
                    // Test the binding
                    var currentContext = _mainWindow.DataContext;
                    Console.WriteLine($"Test binding - Current DataContext type: {currentContext?.GetType().Name}");
                    Console.WriteLine($"Test binding - Current DataContext status: {((MainWindowViewModel?)currentContext)?.StatusMessage}");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create test ViewModel: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Failed to create test ViewModel: {ex.Message}");
                
                // Show the error in the UI
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var errorViewModel = new MainWindowViewModel();
                    errorViewModel.StatusMessage = $"Service test failed: {ex.Message}";
                    errorViewModel.IsLoading = false;
                    _mainWindow.DataContext = errorViewModel;
                });
            }
        }
        catch (Exception ex)
        {
            // Fallback to basic initialization if services fail
            Console.WriteLine($"Service initialization failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Service initialization failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Show the error in the UI
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var errorViewModel = new MainWindowViewModel();
                errorViewModel.StatusMessage = $"Service initialization failed: {ex.Message}";
                errorViewModel.IsLoading = false;
                _mainWindow.DataContext = errorViewModel;
            });
            
            // Try to create a minimal service provider for basic functionality
            try
            {
                var minimalServices = new ServiceCollection();
                minimalServices.AddLogging();
                minimalServices.AddTransient<MainWindowViewModel>();
                _serviceProvider = minimalServices.BuildServiceProvider();
                Console.WriteLine("Created minimal service provider");
                System.Diagnostics.Debug.WriteLine("Created minimal service provider");
            }
            catch (Exception fallbackEx)
            {
                Console.WriteLine($"Fallback initialization also failed: {fallbackEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Fallback initialization also failed: {fallbackEx.Message}");
            }
        }
    }

    private string GetDefaultDatabasePath()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SnapDesk"
        );
        
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }
        
        return Path.Combine(appDataPath, "snapdesk.db");
    }

    private async Task InitializeHotkeysAsync(IHotkeyService hotkeyService)
    {
        try
        {
            // Register default hotkeys
            var saveLayoutHotkey = new SnapDesk.Core.HotkeyInfo("Ctrl+Shift+S", SnapDesk.Core.HotkeyAction.SaveLayout);
            var restoreLayoutHotkey = new SnapDesk.Core.HotkeyInfo("Ctrl+Shift+R", SnapDesk.Core.HotkeyAction.RestoreLayout);
            
            await hotkeyService.RegisterHotkeyAsync(saveLayoutHotkey, async () => 
            {
                // Handle save layout hotkey
                await Task.CompletedTask;
            });
            
            await hotkeyService.RegisterHotkeyAsync(restoreLayoutHotkey, async () => 
            {
                // Handle restore layout hotkey
                await Task.CompletedTask;
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Hotkey initialization failed: {ex.Message}");
        }
    }

    private void InitializeSystemTray()
    {
        try
        {
            // Create system tray icon
            _trayIcon = new TrayIcon
            {
                Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://SnapDesk.UI/Assets/avalonia-logo.ico"))),
                ToolTipText = "SnapDesk - Desktop Layout Manager"
            };

            // Create context menu
            var contextMenu = new NativeMenu();
            
            var showWindowItem = new NativeMenuItem("Show Window");
            showWindowItem.Click += (sender, e) => ShowMainWindow();
            contextMenu.Add(showWindowItem);
            
            var saveLayoutItem = new NativeMenuItem("Save Current Layout");
            saveLayoutItem.Click += async (sender, e) => await SaveCurrentLayoutAsync();
            contextMenu.Add(saveLayoutItem);
            
            contextMenu.Add(new NativeMenuItemSeparator());
            
            var exitItem = new NativeMenuItem("Exit");
            exitItem.Click += (sender, e) => Environment.Exit(0);
            contextMenu.Add(exitItem);

            _trayIcon.Menu = contextMenu;
            _trayIcon.IsVisible = true;
            
            // Handle tray icon click
            _trayIcon.Clicked += (sender, e) => ShowMainWindow();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"System tray initialization failed: {ex.Message}");
        }
    }

    private void ShowMainWindow()
    {
        if (_mainWindow != null)
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        }
    }

    private async Task SaveCurrentLayoutAsync()
    {
        try
        {
            if (_serviceProvider != null)
            {
                var layoutService = _serviceProvider.GetRequiredService<LayoutService>();
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var layout = await layoutService.SaveCurrentLayoutAsync($"Auto-Save {timestamp}");
                
                // Show success message (tray notifications not supported in this version)
                System.Diagnostics.Debug.WriteLine($"Layout '{layout.Name}' saved successfully");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save current layout");
            System.Diagnostics.Debug.WriteLine($"Failed to save layout: {ex.Message}");
        }
    }

    private void OnMainWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Window.WindowStateProperty && _mainWindow != null)
        {
            var newState = _mainWindow.WindowState;
            if (newState == WindowState.Minimized)
            {
                // Hide window when minimized
                _mainWindow.Hide();
            }
        }
    }

    private async void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        try
        {
            // Stop lifecycle service
            if (_serviceProvider != null)
            {
                var lifecycleService = _serviceProvider.GetService<IApplicationLifecycleService>();
                if (lifecycleService != null)
                {
                    await lifecycleService.ShutdownAsync();
                }
            }
            
            // Clean up resources
            _trayIcon?.Dispose();
            
            // Dispose services
            _serviceProvider?.Dispose();
            
            _logger?.LogInformation("Application shutdown completed");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during application shutdown");
        }
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
    

}