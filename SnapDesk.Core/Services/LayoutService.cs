using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SnapDesk.Core;
using SnapDesk.Core.Interfaces;
using SnapDesk.Platform.Interfaces;
using SnapDesk.Core.Exceptions;
using LiteDB;

namespace SnapDesk.Core.Services;

/// <summary>
/// Service for managing desktop layout operations.
/// This service coordinates between the UI layer and data access layer.
/// </summary>
public class LayoutService : ILayoutService
{
    private readonly ILayoutRepository _layoutRepository;
    private readonly ILogger<LayoutService> _logger;
    private readonly IWindowService _windowService;

    /// <summary>
    /// Initializes a new instance of the LayoutService.
    /// </summary>
    /// <param name="layoutRepository">Repository for layout data access</param>
    /// <param name="logger">Logger for service operations</param>
    public LayoutService(
        ILayoutRepository layoutRepository,
        ILogger<LayoutService> logger,
        IWindowService windowService)
    {
        _layoutRepository = layoutRepository ?? throw new ArgumentNullException(nameof(layoutRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
    }

    // TODO: Implement ILayoutService methods incrementally
    // We'll start with the simplest methods first:
    // 1. GetAllLayoutsAsync
    // 2. GetLayoutAsync  
    // 3. DeleteLayoutAsync

    /// <summary>
    /// Gets all layouts
    /// </summary>
    /// <returns>Collection of all layouts</returns>
    public async Task<IEnumerable<LayoutProfile>> GetAllLayoutsAsync()
    {
        try
        {
            _logger.LogDebug("Getting all layouts");
            var layouts = await _layoutRepository.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} layouts", layouts.Count());
            return layouts;
        }
        catch (DatabaseOperationException ex)
        {
            _logger.LogError(ex, "Database operation failed while getting all layouts: {Operation} on {Collection}", ex.Operation, ex.Collection);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while getting all layouts");
            throw new LayoutOperationException("Failed to retrieve layouts due to an unexpected error", ObjectId.Empty, ex);
        }
    }

    /// <summary>
    /// Gets a layout by ID
    /// </summary>
    /// <param name="id">Layout ID</param>
    /// <returns>Layout profile if found, null otherwise</returns>
    public async Task<LayoutProfile?> GetLayoutAsync(ObjectId id)
    {
        try
        {
            if (id == ObjectId.Empty)
            {
                _logger.LogWarning("GetLayoutAsync called with empty ID");
                return null;
            }

            _logger.LogDebug("Getting layout with ID: {Id}", id);
            var layout = await _layoutRepository.GetByIdAsync(id);
            
            if (layout == null)
            {
                _logger.LogWarning("Layout not found with ID: {Id}", id);
                return null;
            }

            _logger.LogDebug("Successfully retrieved layout: {Name} with ID: {Id}", layout.Name, id);
            return layout;
        }
        catch (DatabaseOperationException ex)
        {
            _logger.LogError(ex, "Database operation failed while getting layout {Id}: {Operation} on {Collection}", id, ex.Operation, ex.Collection);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while getting layout {Id}", id);
            throw new LayoutOperationException($"Failed to retrieve layout {id} due to an unexpected error", id, ex);
        }
    }

    /// <summary>
    /// Deletes a saved layout
    /// </summary>
    /// <param name="id">Layout ID to delete</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteLayoutAsync(ObjectId id)
    {
        try
        {
            if (id == ObjectId.Empty)
            {
                _logger.LogWarning("DeleteLayoutAsync called with empty ID");
                return false;
            }

            _logger.LogInformation("Deleting layout with ID: {Id}", id);
            var result = await _layoutRepository.DeleteAsync(id);
            _logger.LogInformation("Layout deletion result: {Success}", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete layout with ID: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Gets layouts by name (partial match)
    /// </summary>
    /// <param name="name">Layout name to search for</param>
    /// <returns>Collection of matching layouts</returns>
    public async Task<IEnumerable<LayoutProfile>> GetLayoutsByNameAsync(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("GetLayoutsByNameAsync called with null or empty name");
                return Enumerable.Empty<LayoutProfile>();
            }

            _logger.LogDebug("Getting layouts with name containing: {Name}", name);
            var layouts = await _layoutRepository.GetAsync(l => l.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            _logger.LogInformation("Found {Count} layouts matching name: {Name}", layouts.Count(), name);
            return layouts;
        }
        catch (DatabaseOperationException ex)
        {
            _logger.LogError(ex, "Database operation failed while searching layouts by name '{Name}': {Operation} on {Collection}", name, ex.Operation, ex.Collection);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while searching layouts by name '{Name}'", name);
            throw new LayoutOperationException($"Failed to search layouts by name '{name}' due to an unexpected error", ObjectId.Empty, ex);
        }
    }

    /// <summary>
    /// Updates an existing layout
    /// </summary>
    /// <param name="layout">Layout to update</param>
    /// <returns>Updated layout profile</returns>
    public async Task<LayoutProfile> UpdateLayoutAsync(LayoutProfile layout)
    {
        try
        {
            if (layout == null)
            {
                _logger.LogWarning("UpdateLayoutAsync called with null layout");
                throw new ArgumentNullException(nameof(layout));
            }

            _logger.LogInformation("Updating layout with ID: {Id}", layout.Id);
            layout.UpdatedAt = DateTime.UtcNow;
            var result = await _layoutRepository.UpdateAsync(layout);
            _logger.LogInformation("Layout update result: {Success}", result);
            return layout;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update layout with ID: {Id}", layout.Id);
            throw;
        }
    }

    /// <summary>
    /// Saves an existing layout profile
    /// </summary>
    /// <param name="layout">Layout to save</param>
    /// <returns>Saved layout profile</returns>
    public async Task<LayoutProfile> SaveLayoutAsync(LayoutProfile layout)
    {
        try
        {
            if (layout == null)
            {
                _logger.LogWarning("SaveLayoutAsync called with null layout");
                throw new ArgumentNullException(nameof(layout));
            }

            _logger.LogInformation("Saving layout with ID: {Id}", layout.Id);
            var result = await _layoutRepository.UpdateAsync(layout);
            _logger.LogInformation("Layout save result: {Success}", result);
            return layout;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save layout with ID: {Id}", layout.Id);
            throw;
        }
    }

    /// <summary>
    /// Gets the currently active layout
    /// </summary>
    /// <returns>Active layout profile if any, null otherwise</returns>
    public async Task<LayoutProfile?> GetActiveLayoutAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving active layout");
            var layout = await _layoutRepository.GetActiveLayoutAsync();
            _logger.LogInformation("Active layout result: {Found}", layout != null);
            return layout;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve active layout");
            throw;
        }
    }



    // TODO: Implement remaining methods in next increment:
    // - SaveCurrentLayoutAsync (requires WindowService)
    // - RestoreLayoutAsync (requires WindowService)
    // - ExportLayoutAsync (file operations)
    // - ImportLayoutAsync (file operations)
    // - DuplicateLayoutAsync (business logic)
    // - ValidateLayoutAsync (validation logic)

    /// <summary>
    /// Saves the current desktop layout
    /// </summary>
    /// <param name="name">Layout name</param>
    /// <param name="description">Optional layout description</param>
    /// <returns>Saved layout profile</returns>
    public async Task<LayoutProfile> SaveCurrentLayoutAsync(string name, string? description = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                _logger.LogWarning("SaveCurrentLayoutAsync called with null or empty name");
                throw new ValidationException("Layout name cannot be null or empty", "name", name);
            }

            _logger.LogInformation("Saving current desktop layout with name: {Name}", name);

            // Capture current desktop windows and monitors
            var windows = await _windowService.CaptureDesktopLayoutAsync();
            var monitors = await _windowService.GetMonitorConfigurationAsync();

            var layout = new LayoutProfile(name, description)
            {
                Windows = windows.ToList(), // Convert to List only when assigning to property
                MonitorConfiguration = monitors.ToList() // Convert to List only when assigning to property
            };

            // Persist as a new layout
            await _layoutRepository.InsertAsync(layout);
            _logger.LogInformation("Saved layout with ID: {Id}, Windows: {Count}", layout.Id, layout.Windows.Count);
            return layout;
        }
        catch (ValidationException)
        {
            // Re-throw validation exceptions as-is
            throw;
        }
        catch (DatabaseOperationException ex)
        {
            _logger.LogError(ex, "Database operation failed while saving layout '{Name}': {Operation} on {Collection}", name, ex.Operation, ex.Collection);
            throw new LayoutOperationException($"Failed to save layout '{name}' due to database error", ObjectId.Empty, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving layout '{Name}'", name);
            throw new LayoutOperationException($"Failed to save layout '{name}' due to an unexpected error", ObjectId.Empty, ex);
        }
    }

    /// <summary>
    /// Restores a saved layout to the current desktop
    /// </summary>
    /// <param name="id">Layout ID to restore</param>
    /// <param name="options">Restoration options</param>
    /// <returns>True if restoration was successful</returns>
    public async Task<bool> RestoreLayoutAsync(ObjectId id, RestoreOptions? options = null)
    {
        try
        {
            if (id == ObjectId.Empty)
            {
                _logger.LogWarning("RestoreLayoutAsync called with empty ID");
                return false;
            }

            _logger.LogInformation("Restoring layout with ID: {Id}", id);

            var layout = await _layoutRepository.GetByIdAsync(id);
            if (layout == null)
            {
                _logger.LogWarning("Layout not found for restoration: {Id}", id);
                return false;
            }

            // Optionally move windows to their target monitors first
            // Then set bounds and state
            var restored = await _windowService.RestoreWindowsAsync(layout.Windows);
            var success = restored > 0;
            _logger.LogInformation("Restored {Count} windows for layout {Id}", restored, id);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore layout with ID: {Id}", id);
            throw;
        }
    }



    /// <summary>
    /// Imports a layout from a file
    /// </summary>
    /// <param name="filePath">Import file path</param>
    /// <returns>Imported layout profile</returns>
    public async Task<LayoutProfile> ImportLayoutAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.LogWarning("ImportLayoutAsync called with null or empty file path");
                throw new ValidationException("File path cannot be null or empty", "filePath", filePath);
            }

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Import file does not exist: {FilePath}", filePath);
                throw new FileOperationException($"Import file not found: {filePath}", "Import", filePath);
            }

            _logger.LogInformation("Importing layout from file: {FilePath}", filePath);
            
            // Read and deserialize the layout file
            var jsonContent = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new FileOperationException("Import file is empty or contains no valid content", "Import", filePath);
            }

            var importOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            var importedLayout = System.Text.Json.JsonSerializer.Deserialize<LayoutProfile>(jsonContent, importOptions);
            if (importedLayout == null)
            {
                throw new FileOperationException("Failed to deserialize layout from import file", "Import", filePath);
            }

            // Validate the imported layout
            if (string.IsNullOrWhiteSpace(importedLayout.Name))
            {
                throw new ValidationException("Imported layout must have a valid name", "Name", importedLayout.Name);
            }

            // Generate new ID and timestamps for the imported layout
            importedLayout.Id = ObjectId.NewObjectId();
            importedLayout.CreatedAt = DateTime.UtcNow;
            importedLayout.UpdatedAt = DateTime.UtcNow;
            importedLayout.IsActive = false; // Imported layouts are not active by default

            // Ensure collections are initialized
            importedLayout.Windows ??= new List<WindowInfo>();
            importedLayout.MonitorConfiguration ??= new List<MonitorInfo>();

            // Generate new IDs for all child objects to avoid conflicts
            foreach (var window in importedLayout.Windows)
            {
                window.WindowId = ObjectId.NewObjectId();
            }

            _logger.LogInformation("Successfully imported layout '{Name}' with {WindowCount} windows from {FilePath}", 
                importedLayout.Name, importedLayout.Windows.Count, filePath);

            return importedLayout;
        }
        catch (ValidationException)
        {
            // Re-throw validation exceptions as-is
            throw;
        }
        catch (FileOperationException)
        {
            // Re-throw file operation exceptions as-is
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON from import file: {FilePath}", filePath);
            throw new FileOperationException($"Invalid JSON format in import file: {ex.Message}", "Import", filePath, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import layout from file: {FilePath}", filePath);
            throw new FileOperationException($"Failed to import layout from file: {filePath}", "Import", filePath, ex);
        }
    }



    /// <summary>
    /// Validates if a layout can be restored
    /// </summary>
    /// <param name="id">Layout ID to validate</param>
    /// <returns>Validation result with details</returns>
    public async Task<LayoutValidationResult> ValidateLayoutAsync(ObjectId id)
    {
        try
        {
            if (id == ObjectId.Empty)
            {
                _logger.LogWarning("ValidateLayoutAsync called with empty ID");
                return new LayoutValidationResult
                {
                    IsValid = false,
                    CanBeRestored = false,
                    Errors = { "Layout ID cannot be empty" }
                };
            }

            _logger.LogInformation("Validating layout with ID: {Id}", id);
            
            // Check if layout exists
            var layout = await _layoutRepository.GetByIdAsync(id);
            if (layout == null)
            {
                _logger.LogWarning("Layout not found for validation: {Id}", id);
                return new LayoutValidationResult
                {
                    IsValid = false,
                    CanBeRestored = false,
                    Errors = { $"Layout with ID '{id}' not found" }
                };
            }
            
            // Basic validation checks
            var errors = new List<string>();
            var warnings = new List<string>();
            
            if (string.IsNullOrWhiteSpace(layout.Name))
                errors.Add("Layout name is missing");
            
            if (layout.Windows == null || layout.Windows.Count == 0)
                warnings.Add("Layout has no windows");
            
            var isValid = errors.Count == 0;
            var canBeRestored = isValid && layout.Windows != null && layout.Windows.Count > 0;
            
            var result = new LayoutValidationResult
            {
                IsValid = isValid,
                CanBeRestored = canBeRestored,
                Errors = errors,
                Warnings = warnings,
                RestorableWindowCount = layout.Windows?.Count ?? 0,
                RestorableMonitorCount = layout.MonitorConfiguration?.Count ?? 0
            };
            
            _logger.LogInformation("Layout validation result: {IsValid}, CanBeRestored: {CanBeRestored} for ID: {Id}", 
                result.IsValid, result.CanBeRestored, id);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate layout with ID: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Activates a layout (sets it as the current active layout)
    /// </summary>
    /// <param name="id">Layout ID to activate</param>
    /// <returns>True if activation was successful</returns>
    public async Task<bool> ActivateLayoutAsync(ObjectId id)
    {
        try
        {
            if (id == ObjectId.Empty)
            {
                _logger.LogWarning("ActivateLayoutAsync called with empty ID");
                return false;
            }

            _logger.LogInformation("Activating layout with ID: {Id}", id);
            
            // Deactivate all other layouts first
            var allLayouts = await _layoutRepository.GetAllAsync();
            foreach (var layout in allLayouts)
            {
                if (layout.IsActive)
                {
                    layout.IsActive = false;
                    await _layoutRepository.UpdateAsync(layout);
                }
            }
            
            // Activate the specified layout
            var targetLayout = await _layoutRepository.GetByIdAsync(id);
            if (targetLayout == null)
            {
                _logger.LogWarning("Layout not found for activation: {Id}", id);
                return false;
            }
            
            targetLayout.IsActive = true;
            await _layoutRepository.UpdateAsync(targetLayout);
            
            _logger.LogInformation("Successfully activated layout with ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate layout with ID: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// Exports a layout to a file
    /// </summary>
    /// <param name="id">Layout ID to export</param>
    /// <param name="filePath">Export file path</param>
    /// <returns>True if export was successful</returns>
    public async Task<bool> ExportLayoutAsync(ObjectId id, string filePath)
    {
        try
        {
            if (id == ObjectId.Empty)
            {
                _logger.LogWarning("ExportLayoutAsync called with empty ID");
                throw new ValidationException("Layout ID cannot be empty", "id", id);
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.LogWarning("ExportLayoutAsync called with empty file path");
                throw new ValidationException("Export file path cannot be empty", "filePath", filePath);
            }

            _logger.LogInformation("Exporting layout with ID: {Id} to file: {FilePath}", id, filePath);
            
            var layout = await _layoutRepository.GetByIdAsync(id);
            if (layout == null)
            {
                _logger.LogWarning("Layout not found for export: {Id}", id);
                throw new ResourceNotFoundException($"Layout with ID '{id}' not found for export", "Layout", id);
            }

            // Ensure the directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create export data (clone to avoid modifying original)
            var exportData = new LayoutProfile(layout.Name, layout.Description)
            {
                Windows = new List<WindowInfo>(layout.Windows),
                MonitorConfiguration = new List<MonitorInfo>(layout.MonitorConfiguration),
                Hotkey = layout.Hotkey != null ? new HotkeyInfo
                {
                    Keys = layout.Hotkey.Keys,
                    Key = layout.Hotkey.Key,
                    Action = layout.Hotkey.Action,
                    IsEnabled = layout.Hotkey.IsEnabled
                } : null
            };

            // Serialize to JSON with proper formatting
            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };

            var jsonContent = System.Text.Json.JsonSerializer.Serialize(exportData, jsonOptions);
            
            // Write to file
            await File.WriteAllTextAsync(filePath, jsonContent);
            
            _logger.LogInformation("Successfully exported layout '{Name}' with {WindowCount} windows to {FilePath}", 
                layout.Name, layout.Windows.Count, filePath);
            
            return true;
        }
        catch (ValidationException)
        {
            // Re-throw validation exceptions as-is
            throw;
        }
        catch (ResourceNotFoundException)
        {
            // Re-throw resource not found exceptions as-is
            throw;
        }
        catch (DatabaseOperationException ex)
        {
            _logger.LogError(ex, "Database operation failed while exporting layout {Id}: {Operation} on {Collection}", id, ex.Operation, ex.Collection);
            throw new LayoutOperationException($"Failed to export layout {id} due to database error", id, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export layout with ID: {Id}", id);
            throw new LayoutOperationException($"Failed to export layout {id} due to an unexpected error", id, ex);
        }
    }

    /// <summary>
    /// Duplicates an existing layout
    /// </summary>
    /// <param name="id">Layout ID to duplicate</param>
    /// <param name="newName">Name for the duplicated layout</param>
    /// <returns>Duplicated layout profile</returns>
    public async Task<LayoutProfile> DuplicateLayoutAsync(ObjectId id, string newName)
    {
        try
        {
            if (id == ObjectId.Empty)
            {
                _logger.LogWarning("DuplicateLayoutAsync called with empty ID");
                throw new ArgumentException("Layout ID cannot be empty", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                _logger.LogWarning("DuplicateLayoutAsync called with empty new name");
                throw new ArgumentException("New name cannot be empty", nameof(newName));
            }

            _logger.LogInformation("Duplicating layout with ID: {Id} to new name: {NewName}", id, newName);
            
            // Get the original layout
            var originalLayout = await _layoutRepository.GetByIdAsync(id);
            if (originalLayout == null)
            {
                _logger.LogWarning("Original layout not found for duplication: {Id}", id);
                throw new InvalidOperationException($"Layout with ID '{id}' not found");
            }
            
            // Create duplicate with new name
            var duplicateLayout = new LayoutProfile(newName, originalLayout.Description)
            {
                Windows = new List<WindowInfo>(originalLayout.Windows),
                MonitorConfiguration = new List<MonitorInfo>(originalLayout.MonitorConfiguration),
                IsActive = false // Duplicates are not active by default
            };
            
            _logger.LogInformation("Created duplicate layout with ID: {Id}", duplicateLayout.Id);
            return duplicateLayout;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to duplicate layout with ID: {Id} to new name: {NewName}", id, newName);
            throw;
        }
    }
}
