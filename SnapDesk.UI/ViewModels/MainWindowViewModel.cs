using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SnapDesk.Core;
using SnapDesk.Core.Interfaces;
using LiteDB;

namespace SnapDesk.UI.ViewModels
{
    // Wrapper class for UI selection state
    public partial class LayoutProfileViewModel : ObservableObject
    {
        public LayoutProfile Layout { get; set; }
        
        [ObservableProperty]
        private bool _isSelected = false;
        
        public string Name => Layout.Name;
        public DateTime CreatedAt => Layout.CreatedAt;
        public string Description => Layout.Description ?? "No description";
        
        public LayoutProfileViewModel(LayoutProfile layout)
        {
            Layout = layout;
        }
        
        partial void OnIsSelectedChanged(bool value)
        {
            // Notify parent ViewModel that selection changed
            if (ParentViewModel != null)
            {
                ParentViewModel.UpdateButtonStates();
            }
        }
        
        public MainWindowViewModel? ParentViewModel { get; set; }
    }
    
    public partial class MainWindowViewModel : ViewModelBase
    {
        // Services
        private readonly ILayoutService? _layoutService;
        private readonly IHotkeyService? _hotkeyService;
        
        // Observable properties
        [ObservableProperty]
        private ObservableCollection<LayoutProfileViewModel> _savedLayouts = new();
        
        [ObservableProperty]
        private string _selectedLayoutName = "No layout selected";
        
        [ObservableProperty]
        private string _databaseStatus = "Unknown";
        
        [ObservableProperty]
        private string _databasePath = "Unknown";
        
        [ObservableProperty]
        private bool _isDatabaseConnected = false;
        
        [ObservableProperty]
        private string _statusMessage = "Initializing...";
        
        [ObservableProperty]
        private string _databaseStatusColor = "#DC3545"; // Default to red
        
        // Commands
        public IAsyncRelayCommand SaveCurrentLayoutCommand { get; }
        public IAsyncRelayCommand RestoreLayoutCommand { get; }
        public IAsyncRelayCommand DeleteLayoutCommand { get; }
        public IAsyncRelayCommand RefreshLayoutsCommand { get; }
        
        public MainWindowViewModel(ILayoutService? layoutService = null, IHotkeyService? hotkeyService = null)
        {
            _layoutService = layoutService;
            _hotkeyService = hotkeyService;
            
            // Initialize commands
            SaveCurrentLayoutCommand = new AsyncRelayCommand(SaveCurrentLayoutAsync, () => CanSaveLayout());
            RestoreLayoutCommand = new AsyncRelayCommand(RestoreSelectedLayoutAsync, () => CanRestoreLayout);
            DeleteLayoutCommand = new AsyncRelayCommand(DeleteSelectedLayoutAsync, () => CanDeleteLayout);
            RefreshLayoutsCommand = new AsyncRelayCommand(RefreshLayoutsAsync);
            
            // Initialize data
            _ = InitializeAsync();
        }
        
        private async Task InitializeAsync()
        {
            try
            {
                if (_layoutService != null)
                {
                    // Real service mode
                    StatusMessage = "Connected to database - Services available";
                    IsDatabaseConnected = true;
                    DatabaseStatus = "Connected";
                    DatabasePath = "Database initialized";
                    DatabaseStatusColor = "#28A745"; // Green for connected
                    
                    // Load existing layouts from database
                    await RefreshLayoutsAsync();
                }
                else
                {
                    // Demo mode
                    StatusMessage = "Demo Mode - No services available";
                    IsDatabaseConnected = false;
                    DatabaseStatus = "Demo Mode";
                    DatabasePath = "No database";
                    DatabaseStatusColor = "#DC3545"; // Red for demo mode
                    
                    // Load sample data
                    InitializeWithSampleData();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                IsDatabaseConnected = false;
                DatabaseStatus = "Error";
                DatabasePath = "Failed to initialize";
            }
        }
        
        private void InitializeWithSampleData()
        {
            // Add some sample layouts for demo purposes
            SavedLayouts.Clear();
            SavedLayouts.Add(new LayoutProfileViewModel(new LayoutProfile("Sample Layout 1")) { ParentViewModel = this });
            SavedLayouts.Add(new LayoutProfileViewModel(new LayoutProfile("Sample Layout 2")) { ParentViewModel = this });
            
            // Update button states after adding sample data
            UpdateButtonStates();
        }
        
        private async Task SaveCurrentLayoutAsync()
        {
            try
            {
                if (_layoutService != null)
                {
                    // Real service implementation
                    StatusMessage = "Saving current layout...";
                    var layoutName = $"Layout {DateTime.Now:yyyy-MM-dd HH:mm}";
                    
                    var savedLayout = await _layoutService.SaveCurrentLayoutAsync(layoutName);
                    
                    // Refresh the layouts list to show the new layout
                    await RefreshLayoutsAsync();
                    StatusMessage = $"Layout '{savedLayout.Name}' saved successfully";
                }
                else
                {
                    // Demo mode
                    var newLayout = new LayoutProfile($"Demo Layout {DateTime.Now:yyyy-MM-dd HH:mm}");
                    
                    SavedLayouts.Add(new LayoutProfileViewModel(newLayout));
                    StatusMessage = $"Demo layout '{newLayout.Name}' created";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving layout: {ex.Message}";
                // Log the error details for debugging (removed Debug.WriteLine to prevent new terminals)
            }
        }
        
        private async Task RestoreSelectedLayoutAsync()
        {
            try
            {
                var selectedLayout = SavedLayouts.FirstOrDefault(l => l.IsSelected);
                if (selectedLayout != null)
                {
                    if (_layoutService != null)
                    {
                        // Real service implementation
                        var success = await _layoutService.RestoreLayoutAsync(selectedLayout.Layout.Id);
                        if (success)
                        {
                            StatusMessage = $"Layout '{selectedLayout.Name}' restored successfully";
                        }
                        else
                        {
                            StatusMessage = $"Failed to restore layout '{selectedLayout.Name}'";
                        }
                    }
                    else
                    {
                        // Demo mode
                        StatusMessage = $"Demo: Layout '{selectedLayout.Name}' would be restored";
                    }
                    
                    SelectedLayoutName = selectedLayout.Name;
                }
                else
                {
                    StatusMessage = "Please select a layout to restore";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error restoring layout: {ex.Message}";
            }
        }
        
        private async Task DeleteSelectedLayoutAsync()
        {
            try
            {
                var selectedLayout = SavedLayouts.FirstOrDefault(l => l.IsSelected);
                if (selectedLayout != null)
                {
                    if (_layoutService != null)
                    {
                        // Real service implementation
                        var success = await _layoutService.DeleteLayoutAsync(selectedLayout.Layout.Id);
                        if (success)
                        {
                            // Refresh the layouts list to remove the deleted layout
                            await RefreshLayoutsAsync();
                            StatusMessage = $"Layout '{selectedLayout.Name}' deleted successfully";
                        }
                        else
                        {
                            StatusMessage = $"Failed to delete layout '{selectedLayout.Name}'";
                        }
                    }
                    else
                    {
                        // Demo mode
                        SavedLayouts.Remove(selectedLayout);
                        StatusMessage = $"Demo: Layout '{selectedLayout.Name}' deleted";
                    }
                    
                    SelectedLayoutName = "No layout selected";
                }
                else
                {
                    StatusMessage = "Please select a layout to delete";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting layout: {ex.Message}";
            }
        }
        
        private async Task RefreshLayoutsAsync()
        {
            try
            {
                if (_layoutService != null)
                {
                    // Real service implementation
                    StatusMessage = "Loading layouts from database...";
                    
                    var layouts = await _layoutService.GetAllLayoutsAsync();
                    
                    // Clear current list and add loaded layouts
                    SavedLayouts.Clear();
                    foreach (var layout in layouts)
                    {
                        var layoutVM = new LayoutProfileViewModel(layout) { ParentViewModel = this };
                        SavedLayouts.Add(layoutVM);
                    }
                    
                    // Update button states after loading layouts
                    UpdateButtonStates();
                    
                    StatusMessage = $"Layouts refreshed successfully - {SavedLayouts.Count} layouts loaded";
                }
                else
                {
                    // Demo mode
                    StatusMessage = "Demo layouts refreshed";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error refreshing layouts: {ex.Message}";
                // Log the error details for debugging (removed Debug.WriteLine to prevent new terminals)
            }
        }
        
        // CanExecute methods
        private bool CanSaveLayout() => true; // Always allow saving
        
        [ObservableProperty]
        private bool _canRestoreLayout = false;
        
        [ObservableProperty]
        private bool _canDeleteLayout = false;
        
        // Method to update button states when selection changes
        public void UpdateButtonStates()
        {
            CanRestoreLayout = SavedLayouts.Any(l => l.IsSelected);
            CanDeleteLayout = SavedLayouts.Any(l => l.IsSelected);

            // Force re-evaluation of command availability
            RestoreLayoutCommand.NotifyCanExecuteChanged();
            DeleteLayoutCommand.NotifyCanExecuteChanged();
        }
    }
}
