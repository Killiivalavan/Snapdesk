using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SnapDesk.UI.ViewModels;

/// <summary>
/// Simplified main window view model that demonstrates basic MVVM concepts.
/// This will be enhanced in later phases when we add real services.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    // Observable collections automatically update the UI when data changes
    private ObservableCollection<string> _layoutNames;
    private string? _selectedLayout;
    private bool _isLoading;
    private string _statusMessage = "Ready";

    /// <summary>
    /// Collection of layout names that will be displayed in the UI
    /// </summary>
    public ObservableCollection<string> LayoutNames
    {
        get => _layoutNames;
        set => SetProperty(ref _layoutNames, value);
    }

    /// <summary>
    /// Currently selected layout in the UI
    /// </summary>
    public string? SelectedLayout
    {
        get => _selectedLayout;
        set => SetProperty(ref _selectedLayout, value);
    }

    /// <summary>
    /// Whether the application is currently loading data
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// Status message to display to the user
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// Command to save the current desktop layout
    /// </summary>
    public ICommand SaveCurrentLayoutCommand { get; }

    /// <summary>
    /// Command to restore a selected layout
    /// </summary>
    public ICommand RestoreLayoutCommand { get; }

    /// <summary>
    /// Command to delete a selected layout
    /// </summary>
    public ICommand DeleteLayoutCommand { get; }

    /// <summary>
    /// Command to refresh the layout list
    /// </summary>
    public ICommand RefreshLayoutsCommand { get; }

    /// <summary>
    /// Constructor that initializes the view model with sample data
    /// </summary>
    public MainWindowViewModel()
    {
        // Initialize collections with sample data
        _layoutNames = new ObservableCollection<string>
        {
            "Coding Setup",
            "Design Work",
            "Gaming Layout",
            "Productivity Mode"
        };

        // Initialize commands
        SaveCurrentLayoutCommand = new RelayCommand(SaveCurrentLayout);
        RestoreLayoutCommand = new RelayCommand(RestoreSelectedLayout, () => !string.IsNullOrEmpty(SelectedLayout));
        DeleteLayoutCommand = new RelayCommand(DeleteSelectedLayout, () => !string.IsNullOrEmpty(SelectedLayout));
        RefreshLayoutsCommand = new RelayCommand(RefreshLayouts);
    }

    /// <summary>
    /// Simulates saving the current desktop layout
    /// </summary>
    private void SaveCurrentLayout()
    {
        IsLoading = true;
        StatusMessage = "Capturing current layout...";

        // Simulate work
        System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ =>
        {
            // Add a new layout name
            var newLayoutName = $"Layout {DateTime.Now:yyyy-MM-dd HH:mm}";
            LayoutNames.Add(newLayoutName);
            
            // Update UI on main thread
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                IsLoading = false;
                StatusMessage = $"Layout '{newLayoutName}' saved successfully";
            });
        });
    }

    /// <summary>
    /// Simulates restoring the selected layout
    /// </summary>
    private void RestoreSelectedLayout()
    {
        if (string.IsNullOrEmpty(SelectedLayout)) return;

        IsLoading = true;
        StatusMessage = $"Restoring layout '{SelectedLayout}'...";

        // Simulate work
        System.Threading.Tasks.Task.Delay(1500).ContinueWith(_ =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                IsLoading = false;
                StatusMessage = $"Layout '{SelectedLayout}' restored successfully";
            });
        });
    }

    /// <summary>
    /// Simulates deleting the selected layout
    /// </summary>
    private void DeleteSelectedLayout()
    {
        if (string.IsNullOrEmpty(SelectedLayout)) return;

        IsLoading = true;
        StatusMessage = $"Deleting layout '{SelectedLayout}'...";

        // Simulate work
        System.Threading.Tasks.Task.Delay(800).ContinueWith(_ =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                // Remove from collection
                LayoutNames.Remove(SelectedLayout);
                SelectedLayout = null;
                
                IsLoading = false;
                StatusMessage = "Layout deleted successfully";
            });
        });
    }

    /// <summary>
    /// Simulates refreshing the layout list
    /// </summary>
    private void RefreshLayouts()
    {
        IsLoading = true;
        StatusMessage = "Refreshing layouts...";

        // Simulate work
        System.Threading.Tasks.Task.Delay(500).ContinueWith(_ =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                IsLoading = false;
                StatusMessage = $"Refreshed {LayoutNames.Count} layouts";
            });
        });
    }
}
