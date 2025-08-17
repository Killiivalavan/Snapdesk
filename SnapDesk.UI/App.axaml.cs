using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using SnapDesk.UI.ViewModels;
using SnapDesk.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SnapDesk.Core.Services;
using SnapDesk.Core.Interfaces;
using SnapDesk.Data.Services;
using SnapDesk.Data.Configuration;
using SnapDesk.Data.Repositories;
using SnapDesk.Platform.Interfaces;
using SnapDesk.Platform.Common;
using SnapDesk.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SnapDesk.UI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private static readonly string LogFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SnapDesk",
        "snapdesk-ui.log"
    );

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
            
            // Initialize services
            _ = Task.Run(async () => await InitializeServicesAsync(desktop));
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void LogToFile(string message)
    {
        try
        {
            var logDir = Path.GetDirectoryName(LogFilePath);
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir!);

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logMessage = $"[{timestamp}] {message}{Environment.NewLine}";
            File.AppendAllText(LogFilePath, logMessage);
        }
        catch
        {
            // If logging fails, we can't do much about it
        }
    }

    private async Task InitializeServicesAsync(IClassicDesktopStyleApplicationLifetime desktop)
    {
        try
        {
            LogToFile("=== Starting service initialization ===");
            
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
            LogToFile("✓ Configuration created");

            // Setup services
            var services = new ServiceCollection();
            
            // Add configuration
            services.AddSingleton<IConfiguration>(configuration);
            LogToFile("✓ Configuration service registered");
            
            // Add logging
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
            });
            LogToFile("✓ Logging service registered");
            
            // Create database configuration
            var dbPath = GetDefaultDatabasePath();
            var dbConfig = DatabaseConfiguration.CreateForPath(dbPath);
            services.AddSingleton<DatabaseConfiguration>(dbConfig);
            LogToFile("✓ Database configuration created");
            
            // Register database and repository services
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<ILayoutRepository, LayoutRepository>();
            services.AddSingleton<IHotkeyRepository, HotkeyRepository>();
            services.AddSingleton<SnapDesk.Core.Interfaces.IRepository<HotkeyInfo>, HotkeyRepository>();  // Add this line for HotkeyService
            services.AddSingleton<ISettingsRepository, SettingsRepository>();
            LogToFile("✓ Database and repository services registered");
            
            // Register platform services (using real Windows API implementations)
            services.AddSingleton<SnapDesk.Platform.Interfaces.IWindowApi, SnapDesk.Platform.Windows.WindowsWindowApi>();
            services.AddSingleton<SnapDesk.Platform.Interfaces.IHotkeyApi, SnapDesk.Platform.Windows.WindowsHotkeyApi>();
            LogToFile("✓ Platform services registered");
            
            // Register core services
            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<IHotkeyService, HotkeyService>();
            services.AddSingleton<ILayoutService, LayoutService>();
            LogToFile("✓ Core services registered");
            
            // Test service registration before building
            LogToFile("Testing service registration...");
            var tempServices = services.BuildServiceProvider();
            try
            {
                var testLayoutService = tempServices.GetService<ILayoutService>();
                var testHotkeyService = tempServices.GetService<IHotkeyService>();
                var testWindowService = tempServices.GetService<IWindowService>();
                LogToFile($"✓ Service test: Layout={testLayoutService != null}, Hotkey={testHotkeyService != null}, Window={testWindowService != null}");
            }
            catch (Exception ex)
            {
                LogToFile($"❌ Service test failed: {ex.Message}");
                throw;
            }
            finally
            {
                tempServices.Dispose();
            }
            
            // Build service provider
            LogToFile("Building service provider...");
            _serviceProvider = services.BuildServiceProvider();
            LogToFile("✓ Service provider built");
            
            // Initialize database
            LogToFile("Initializing database...");
            var dbService = _serviceProvider.GetRequiredService<IDatabaseService>();
            await dbService.InitializeAsync();
            LogToFile("✓ Database initialized");
            
            // Create main window with services
            LogToFile("Creating main window with services...");
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                var layoutService = _serviceProvider.GetService<ILayoutService>();
                var hotkeyService = _serviceProvider.GetService<IHotkeyService>();
                
                LogToFile($"Services resolved: Layout={layoutService != null}, Hotkey={hotkeyService != null}");
                
                var viewModel = new MainWindowViewModel(layoutService, hotkeyService);
                var mainWindow = new MainWindow { DataContext = viewModel };
                
                desktop.MainWindow = mainWindow;
                mainWindow.Show();
                LogToFile("✓ Main window created and shown with services");
            });
        }
        catch (Exception ex)
        {
            LogToFile($"❌ Service initialization failed: {ex.Message}");
            LogToFile($"Stack trace: {ex.StackTrace}");
            
            // Fallback to demo mode if services fail
            LogToFile("Falling back to demo mode...");
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                var viewModel = new MainWindowViewModel(); // Demo mode
                var mainWindow = new MainWindow { DataContext = viewModel };
                
                desktop.MainWindow = mainWindow;
                mainWindow.Show();
                LogToFile("✓ Demo mode window created");
            });
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