using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SnapDesk.Core;
using SnapDesk.Core.Interfaces;
using SnapDesk.Platform.Interfaces;
using LiteDB;

namespace SnapDesk.Core.Services;

/// <summary>
/// Service for managing global hotkey operations
/// </summary>
public class HotkeyService : IHotkeyService
{
    private readonly IHotkeyApi _hotkeyApi;
    private readonly IRepository<HotkeyInfo> _hotkeyRepository;
    private readonly ILogger<HotkeyService> _logger;
    private readonly Dictionary<ObjectId, Func<Task>> _asyncCallbacks = new();
    private readonly Dictionary<ObjectId, Action> _syncCallbacks = new();
    private readonly Dictionary<ObjectId, int> _hotkeyIds = new();
    private int _nextHotkeyId = 1;

    /// <summary>
    /// Resets the hotkey ID counter (useful for testing)
    /// </summary>
    public void ResetHotkeyIdCounter()
    {
        _nextHotkeyId = 1;
        _logger.LogDebug("Hotkey ID counter reset to 1");
    }

    /// <summary>
    /// Clears all registered hotkeys and resets internal state (useful for testing)
    /// </summary>
    public async Task ClearAllHotkeysAsync()
    {
        try
        {
            // Unregister all hotkeys from platform
            foreach (var platformId in _hotkeyIds.Values)
            {
                _hotkeyApi.TryUnregisterHotkey(platformId, out _);
            }

            // Clear internal collections
            _hotkeyIds.Clear();
            _asyncCallbacks.Clear();
            _syncCallbacks.Clear();
            _nextHotkeyId = 1;
            _isSuspended = false;

            // Clear all hotkeys from database
            var allHotkeys = await _hotkeyRepository.GetAllAsync();
            foreach (var hotkey in allHotkeys)
            {
                await _hotkeyRepository.DeleteAsync(hotkey.Id);
            }

            _logger.LogDebug("All hotkeys cleared from platform, memory, and database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear all hotkeys");
        }
    }
    private bool _isSuspended = false;

    public HotkeyService(
        IHotkeyApi hotkeyApi,
        IRepository<HotkeyInfo> hotkeyRepository,
        ILogger<HotkeyService> logger)
    {
        Console.WriteLine($"[CONSTRUCTOR] HotkeyService constructor called - Instance ID: {GetHashCode()}");
        Console.WriteLine($"[CONSTRUCTOR] HotkeyApi type: {hotkeyApi?.GetType().Name}");
        Console.WriteLine($"[CONSTRUCTOR] Repository type: {hotkeyRepository?.GetType().Name}");
        Console.WriteLine($"[CONSTRUCTOR] Logger type: {logger?.GetType().Name}");
        
        _hotkeyApi = hotkeyApi ?? throw new ArgumentNullException(nameof(hotkeyApi));
        _hotkeyRepository = hotkeyRepository ?? throw new ArgumentNullException(nameof(hotkeyRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        Console.WriteLine($"[CONSTRUCTOR] HotkeyService instance {GetHashCode()} initialized successfully");
    }

    /// <summary>
    /// Associates a hotkey with a specific layout
    /// </summary>
    /// <param name="hotkeyId">Hotkey ID</param>
    /// <param name="layoutId">Layout ID to associate with</param>
    /// <returns>True if association was successful</returns>
    public async Task<bool> AssociateHotkeyWithLayoutAsync(ObjectId hotkeyId, ObjectId layoutId)
    {
        try
        {
            if (hotkeyId == ObjectId.Empty || layoutId == ObjectId.Empty)
            {
                _logger.LogWarning("Hotkey ID or layout ID is null or empty");
                return false;
            }

            _logger.LogDebug("Associating hotkey {HotkeyId} with layout {LayoutId}", hotkeyId, layoutId);

            var hotkey = await _hotkeyRepository.GetByIdAsync(hotkeyId);
            if (hotkey == null)
            {
                _logger.LogWarning("Hotkey {HotkeyId} not found", hotkeyId);
                return false;
            }

            hotkey.AssociateWithLayout(layoutId);
            await _hotkeyRepository.UpdateAsync(hotkey);

            _logger.LogInformation("Successfully associated hotkey {HotkeyId} with layout {LayoutId}", hotkeyId, layoutId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to associate hotkey {HotkeyId} with layout {LayoutId}", hotkeyId, layoutId);
            return false;
        }
    }

    /// <summary>
    /// Removes the layout association from a hotkey
    /// </summary>
    /// <param name="hotkeyId">Hotkey ID</param>
    /// <returns>True if removal was successful</returns>
    public async Task<bool> RemoveHotkeyLayoutAssociationAsync(ObjectId hotkeyId)
    {
        try
        {
            if (hotkeyId == ObjectId.Empty)
            {
                _logger.LogWarning("Hotkey ID is null or empty");
                return false;
            }

            _logger.LogDebug("Removing layout association from hotkey {HotkeyId}", hotkeyId);

            var hotkey = await _hotkeyRepository.GetByIdAsync(hotkeyId);
            if (hotkey == null)
            {
                _logger.LogWarning("Hotkey {HotkeyId} not found", hotkeyId);
                return false;
            }

            hotkey.RemoveLayoutAssociation();
            await _hotkeyRepository.UpdateAsync(hotkey);

            _logger.LogInformation("Successfully removed layout association from hotkey {HotkeyId}", hotkeyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove layout association from hotkey {HotkeyId}", hotkeyId);
            return false;
        }
    }

    /// <summary>
    /// Checks if a hotkey is currently active (enabled and registered)
    /// </summary>
    /// <param name="hotkeyId">Hotkey ID</param>
    /// <returns>True if hotkey is active</returns>
    public async Task<bool> IsHotkeyActiveAsync(ObjectId hotkeyId)
    {
        try
        {
            if (hotkeyId == ObjectId.Empty)
            {
                _logger.LogWarning("Hotkey ID is empty");
                return false;
            }

            _logger.LogDebug("Checking if hotkey {HotkeyId} is active", hotkeyId);

            var hotkey = await _hotkeyRepository.GetByIdAsync(hotkeyId);
            if (hotkey == null)
            {
                _logger.LogWarning("Hotkey {HotkeyId} not found", hotkeyId);
                return false;
            }

            var isActive = hotkey.IsEnabled && _hotkeyIds.ContainsKey(hotkeyId);
            _logger.LogDebug("Hotkey {HotkeyId} active status: {IsActive}", hotkeyId, isActive);
            return isActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check active status for hotkey {HotkeyId}", hotkeyId);
            return false;
        }
    }

    /// <summary>
    /// Gets all hotkey conflicts in the system
    /// </summary>
    /// <returns>Collection of hotkey conflicts</returns>
    public async Task<IEnumerable<HotkeyConflict>> GetHotkeyConflictsAsync()
    {
        try
        {
            _logger.LogDebug("Getting hotkey conflicts");

            var allHotkeys = await _hotkeyRepository.GetAllAsync();
            var conflicts = new List<HotkeyConflict>();

            // Group hotkeys by key combination
            var hotkeyGroups = allHotkeys.GroupBy(h => h.Keys).Where(g => g.Count() > 1);

            foreach (var group in hotkeyGroups)
            {
                var conflict = new HotkeyConflict
                {
                    KeyCombination = group.Key,
                    ConflictingHotkeys = group.ToList(),
                    IsResolved = false,
                    DetectedAt = DateTime.UtcNow
                };
                conflicts.Add(conflict);
            }

            _logger.LogDebug("Found {Count} hotkey conflicts", conflicts.Count);
            return conflicts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hotkey conflicts");
            throw;
        }
    }

    /// <summary>
    /// Registers a global hotkey with a callback function
    /// </summary>
    /// <param name="hotkey">Hotkey information</param>
    /// <param name="callback">Function to execute when hotkey is pressed</param>
    /// <returns>True if registration was successful</returns>
    public async Task<bool> RegisterHotkeyAsync(HotkeyInfo hotkey, Func<Task> callback)
    {
        Console.WriteLine($"[METHOD] RegisterHotkeyAsync(Func<Task>) called - Instance ID: {GetHashCode()}");
        Console.WriteLine($"[METHOD] Hotkey: {hotkey?.Keys ?? "NULL"}");
        Console.WriteLine($"[METHOD] Callback type: {callback?.GetType().Name ?? "NULL"}");
        
        try
        {
            Console.WriteLine($"[VALIDATION] Starting validation checks...");
            Console.WriteLine($"[VALIDATION] Hotkey parameter: {(hotkey == null ? "NULL" : $"Valid - {hotkey.Keys}")}");
            Console.WriteLine($"[VALIDATION] Callback parameter: {(callback == null ? "NULL" : $"Valid - {callback.GetType().Name}")}");
            
            if (hotkey == null || callback == null)
            {
                Console.WriteLine($"[VALIDATION] ❌ Validation FAILED - null parameters detected");
                _logger.LogWarning("Hotkey or callback is null");
                return false;
            }
            
            Console.WriteLine($"[VALIDATION] ✅ Basic parameter validation passed");
            Console.WriteLine($"[VALIDATION] Checking hotkey validity...");
            Console.WriteLine($"[VALIDATION] Hotkey.IsValid(): {hotkey.IsValid()}");
            Console.WriteLine($"[VALIDATION] Hotkey ID: {hotkey.Id}");
            Console.WriteLine($"[VALIDATION] Hotkey Keys: {hotkey.Keys}");
            Console.WriteLine($"[VALIDATION] Hotkey Modifiers: {hotkey.Modifiers}");
            Console.WriteLine($"[VALIDATION] Hotkey Key: {hotkey.Key}");
            
            if (!hotkey.IsValid())
            {
                Console.WriteLine($"[VALIDATION] ❌ Validation FAILED - hotkey is not valid");
                _logger.LogWarning("Hotkey is not valid: {HotkeyId}", hotkey.Id);
                return false;
            }
            
            Console.WriteLine($"[VALIDATION] ✅ Hotkey validity check passed");
            Console.WriteLine($"[VALIDATION] Checking if service is suspended...");
            Console.WriteLine($"[VALIDATION] Service suspended: {_isSuspended}");
            
            if (_isSuspended)
            {
                Console.WriteLine($"[VALIDATION] ❌ Validation FAILED - service is suspended");
                _logger.LogInformation("Hotkey registration skipped - service is suspended");
                return false;
            }
            
            Console.WriteLine($"[VALIDATION] ✅ Service suspension check passed");

            _logger.LogDebug("Registering hotkey: {HotkeyId} with keys: {Keys}", hotkey.Id, hotkey.Keys);

            // Check if hotkey is already registered
            if (_hotkeyIds.ContainsKey(hotkey.Id))
            {
                _logger.LogWarning("Hotkey {HotkeyId} is already registered", hotkey.Id);
                return false;
            }

            // Check if key combination is available
            var isAvailable = await IsHotkeyAvailableAsync(hotkey.Keys);
            _logger.LogDebug("Hotkey availability check for {Keys}: {IsAvailable}", hotkey.Keys, isAvailable);
            Console.WriteLine($"[DEBUG] Hotkey availability check for {hotkey.Keys}: {isAvailable}");
            
            if (!isAvailable)
            {
                _logger.LogWarning("Key combination {Keys} is not available", hotkey.Keys);
                Console.WriteLine($"[WARNING] Key combination {hotkey.Keys} is not available");
                return false;
            }

            // Convert hotkey to platform format
            var platformId = _nextHotkeyId++;
            var modifiers = ConvertToPlatformModifiers(hotkey.Modifiers);
            var virtualKey = ConvertToVirtualKey(hotkey.Key);

            _logger.LogDebug("Attempting to register hotkey: ID={PlatformId}, Modifiers={Modifiers}, VirtualKey=0x{VirtualKey:X}", 
                platformId, modifiers, virtualKey);
            Console.WriteLine($"[DEBUG] Attempting to register hotkey: ID={platformId}, Modifiers={modifiers}, VirtualKey=0x{virtualKey:X}");

            // Register with platform API
            if (!_hotkeyApi.TryRegisterHotkey(platformId, modifiers, virtualKey, out var error))
            {
                _logger.LogError("Failed to register hotkey with platform: ID={PlatformId}, Modifiers={Modifiers}, VirtualKey=0x{VirtualKey:X}, Error={Error}", 
                    platformId, modifiers, virtualKey, error);
                Console.WriteLine($"[ERROR] Failed to register hotkey with platform: ID={platformId}, Modifiers={modifiers}, VirtualKey=0x{virtualKey:X}, Error={error}");
                return false;
            }

            // Store callback and mapping
            Console.WriteLine($"[REGISTRATION] Storing callback and mapping...");
            _asyncCallbacks[hotkey.Id] = callback;
            _hotkeyIds[hotkey.Id] = platformId;
            Console.WriteLine($"[REGISTRATION] ✅ Callback and mapping stored successfully");

            // Save to repository
            Console.WriteLine($"[REGISTRATION] Saving hotkey to database...");
            await _hotkeyRepository.InsertAsync(hotkey);
            Console.WriteLine($"[REGISTRATION] ✅ Hotkey saved to database successfully");

            _logger.LogInformation("Successfully registered hotkey {HotkeyId} with platform ID {PlatformId}", hotkey.Id, platformId);
            Console.WriteLine($"[REGISTRATION] 🎉 HOTKEY REGISTRATION COMPLETED SUCCESSFULLY!");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register hotkey {HotkeyId}", hotkey?.Id);
            return false;
        }
    }

    /// <summary>
    /// Registers a global hotkey with a synchronous callback function
    /// </summary>
    /// <param name="hotkey">Hotkey information</param>
    /// <param name="callback">Function to execute when hotkey is pressed</param>
    /// <returns>True if registration was successful</returns>
    public async Task<bool> RegisterHotkeyAsync(HotkeyInfo hotkey, Action callback)
    {
        Console.WriteLine($"[METHOD] RegisterHotkeyAsync(Action) called - Instance ID: {GetHashCode()}");
        Console.WriteLine($"[METHOD] Hotkey: {hotkey?.Keys ?? "NULL"}");
        Console.WriteLine($"[METHOD] Callback type: {callback?.GetType().Name ?? "NULL"}");
        
        try
        {
            Console.WriteLine($"[VALIDATION] Starting validation checks...");
            Console.WriteLine($"[VALIDATION] Hotkey parameter: {(hotkey == null ? "NULL" : $"Valid - {hotkey.Keys}")}");
            Console.WriteLine($"[VALIDATION] Callback parameter: {(callback == null ? "NULL" : $"Valid - {callback.GetType().Name}")}");
            
            if (hotkey == null || callback == null)
            {
                Console.WriteLine($"[VALIDATION] ❌ Validation FAILED - null parameters detected");
                _logger.LogWarning("Hotkey or callback is null");
                return false;
            }
            
            Console.WriteLine($"[VALIDATION] ✅ Basic parameter validation passed");
            Console.WriteLine($"[VALIDATION] Checking hotkey validity...");
            Console.WriteLine($"[VALIDATION] Hotkey.IsValid(): {hotkey.IsValid()}");
            Console.WriteLine($"[VALIDATION] Hotkey ID: {hotkey.Id}");
            Console.WriteLine($"[VALIDATION] Hotkey Keys: {hotkey.Keys}");
            Console.WriteLine($"[VALIDATION] Hotkey Modifiers: {hotkey.Modifiers}");
            Console.WriteLine($"[VALIDATION] Hotkey Key: {hotkey.Key}");
            
            if (!hotkey.IsValid())
            {
                Console.WriteLine($"[VALIDATION] ❌ Validation FAILED - hotkey is not valid");
                _logger.LogWarning("Hotkey is not valid: {HotkeyId}", hotkey.Id);
                return false;
            }
            
            Console.WriteLine($"[VALIDATION] ✅ Hotkey validity check passed");
            Console.WriteLine($"[VALIDATION] Checking if service is suspended...");
            Console.WriteLine($"[VALIDATION] Service suspended: {_isSuspended}");
            
            if (_isSuspended)
            {
                Console.WriteLine($"[VALIDATION] ❌ Validation FAILED - service is suspended");
                _logger.LogInformation("Hotkey registration skipped - service is suspended");
                return false;
            }
            
            Console.WriteLine($"[VALIDATION] ✅ Service suspension check passed");

            _logger.LogDebug("Registering hotkey: {HotkeyId} with keys: {Keys}", hotkey.Id, hotkey.Keys);

            // Check if hotkey is already registered
            if (_hotkeyIds.ContainsKey(hotkey.Id))
            {
                _logger.LogWarning("Hotkey {HotkeyId} is already registered", hotkey.Id);
                return false;
            }

            // Check if key combination is available
            var isAvailable = await IsHotkeyAvailableAsync(hotkey.Keys);
            _logger.LogDebug("Hotkey availability check for {Keys}: {IsAvailable}", hotkey.Keys, isAvailable);
            Console.WriteLine($"[DEBUG] Hotkey availability check for {hotkey.Keys}: {isAvailable}");
            
            if (!isAvailable)
            {
                _logger.LogWarning("Key combination {Keys} is not available", hotkey.Keys);
                Console.WriteLine($"[WARNING] Key combination {hotkey.Keys} is not available");
                return false;
            }

            // Convert hotkey to platform format
            var platformId = _nextHotkeyId++;
            var modifiers = ConvertToPlatformModifiers(hotkey.Modifiers);
            var virtualKey = ConvertToVirtualKey(hotkey.Key);

            _logger.LogDebug("Attempting to register hotkey: ID={PlatformId}, Modifiers={Modifiers}, VirtualKey=0x{VirtualKey:X}", 
                platformId, modifiers, virtualKey);
            Console.WriteLine($"[DEBUG] Attempting to register hotkey: ID={platformId}, Modifiers={modifiers}, VirtualKey=0x{virtualKey:X}");

            // Register with platform API
            if (!_hotkeyApi.TryRegisterHotkey(platformId, modifiers, virtualKey, out var error))
            {
                _logger.LogError("Failed to register hotkey with platform: ID={PlatformId}, Modifiers={Modifiers}, VirtualKey=0x{VirtualKey:X}, Error={Error}", 
                    platformId, modifiers, virtualKey, error);
                Console.WriteLine($"[ERROR] Failed to register hotkey with platform: ID={platformId}, Modifiers={modifiers}, VirtualKey=0x{virtualKey:X}, Error={error}");
                return false;
            }

            // Store callback and mapping
            Console.WriteLine($"[REGISTRATION] Storing callback and mapping...");
            _syncCallbacks[hotkey.Id] = callback;
            _hotkeyIds[hotkey.Id] = platformId;
            Console.WriteLine($"[REGISTRATION] ✅ Callback and mapping stored successfully");

            // Save to repository
            Console.WriteLine($"[REGISTRATION] Saving hotkey to database...");
            await _hotkeyRepository.InsertAsync(hotkey);
            Console.WriteLine($"[REGISTRATION] ✅ Hotkey saved to database successfully");

            _logger.LogInformation("Successfully registered hotkey {HotkeyId} with platform ID {PlatformId}", hotkey.Id, platformId);
            Console.WriteLine($"[REGISTRATION] 🎉 HOTKEY REGISTRATION COMPLETED SUCCESSFULLY!");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register hotkey {HotkeyId}", hotkey?.Id);
            return false;
        }
    }

    /// <summary>
    /// Unregisters a previously registered hotkey
    /// </summary>
    /// <param name="hotkeyId">ID of the hotkey to unregister</param>
    /// <returns>True if unregistration was successful</returns>
    public async Task<bool> UnregisterHotkeyAsync(ObjectId hotkeyId)
    {
        try
        {
            if (hotkeyId == ObjectId.Empty)
            {
                _logger.LogWarning("Hotkey ID is empty");
                return false;
            }

            _logger.LogDebug("Unregistering hotkey: {HotkeyId}", hotkeyId);

            if (!_hotkeyIds.TryGetValue(hotkeyId, out var platformId))
            {
                _logger.LogWarning("Hotkey {HotkeyId} is not registered", hotkeyId);
                return false;
            }

            // Unregister from platform API
            if (!_hotkeyApi.TryUnregisterHotkey(platformId, out var error))
            {
                _logger.LogError("Failed to unregister hotkey from platform: {Error}", error);
                return false;
            }

            // Remove from local tracking
            _hotkeyIds.Remove(hotkeyId);
            _asyncCallbacks.Remove(hotkeyId);
            _syncCallbacks.Remove(hotkeyId);

            // Remove from repository
            await _hotkeyRepository.DeleteAsync(hotkeyId);

            _logger.LogInformation("Successfully unregistered hotkey {HotkeyId}", hotkeyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unregister hotkey {HotkeyId}", hotkeyId);
            return false;
        }
    }

    /// <summary>
    /// Unregisters a hotkey by its key combination
    /// </summary>
    /// <param name="keyCombination">Key combination string (e.g., "Ctrl+Alt+1")</param>
    /// <returns>True if unregistration was successful</returns>
    public async Task<bool> UnregisterHotkeyByKeysAsync(string keyCombination)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keyCombination))
            {
                _logger.LogWarning("Key combination is null or empty");
                return false;
            }

            _logger.LogDebug("Unregistering hotkey by keys: {Keys}", keyCombination);

            var allHotkeys = await _hotkeyRepository.GetAllAsync();
            var hotkey = allHotkeys.FirstOrDefault(h => h.Keys == keyCombination);
            if (hotkey == null)
            {
                _logger.LogWarning("No hotkey found with keys: {Keys}", keyCombination);
                return false;
            }

            return await UnregisterHotkeyAsync(hotkey.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unregister hotkey by keys {Keys}", keyCombination);
            return false;
        }
    }

    /// <summary>
    /// Checks if a hotkey combination is available for registration
    /// </summary>
    /// <param name="keyCombination">Key combination to check</param>
    /// <returns>True if the hotkey is available</returns>
    public async Task<bool> IsHotkeyAvailableAsync(string keyCombination)
    {
        Console.WriteLine($"[AVAILABILITY] IsHotkeyAvailableAsync called for: {keyCombination}");
        Console.WriteLine($"[AVAILABILITY] Instance ID: {GetHashCode()}");
        
        try
        {
            if (string.IsNullOrWhiteSpace(keyCombination))
            {
                Console.WriteLine($"[AVAILABILITY] ❌ Key combination is null or empty");
                _logger.LogWarning("Key combination is null or empty");
                return false;
            }
            
            Console.WriteLine($"[AVAILABILITY] ✅ Key combination validation passed");

            // Check if already registered in our system
            Console.WriteLine($"[AVAILABILITY] Checking database for existing hotkeys...");
            var allHotkeys = await _hotkeyRepository.GetAllAsync();
            Console.WriteLine($"[AVAILABILITY] Found {allHotkeys.Count()} hotkeys in database");
            
            var existingHotkey = allHotkeys.FirstOrDefault(h => h.Keys == keyCombination);
            if (existingHotkey != null)
            {
                Console.WriteLine($"[AVAILABILITY] ❌ Key combination {keyCombination} is already registered in database");
                Console.WriteLine($"[AVAILABILITY] Existing hotkey ID: {existingHotkey.Id}, Action: {existingHotkey.Action}");
                _logger.LogDebug("Key combination {Keys} is already registered", keyCombination);
                return false;
            }
            
            Console.WriteLine($"[AVAILABILITY] ✅ No database conflicts found");

            // Check platform-specific limitations
            Console.WriteLine($"[AVAILABILITY] Checking platform support...");
            var systemInfo = _hotkeyApi.GetSystemInfo();
            Console.WriteLine($"[AVAILABILITY] Platform supports global hotkeys: {systemInfo.SupportsGlobalHotkeys}");
            Console.WriteLine($"[AVAILABILITY] Max hotkeys supported: {systemInfo.MaxHotkeyCount}");
            
            if (!systemInfo.SupportsGlobalHotkeys)
            {
                Console.WriteLine($"[AVAILABILITY] ❌ Platform does not support global hotkeys");
                _logger.LogDebug("Platform does not support global hotkeys");
                return false;
            }
            
            Console.WriteLine($"[AVAILABILITY] ✅ Platform support check passed");

            // Check if we've reached the platform limit
            Console.WriteLine($"[AVAILABILITY] Checking hotkey count limits...");
            Console.WriteLine($"[AVAILABILITY] Current hotkey count: {_hotkeyIds.Count}");
            Console.WriteLine($"[AVAILABILITY] Max allowed: {systemInfo.MaxHotkeyCount}");
            
            if (_hotkeyIds.Count >= systemInfo.MaxHotkeyCount)
            {
                Console.WriteLine($"[AVAILABILITY] ❌ Maximum hotkey limit reached: {_hotkeyIds.Count}/{systemInfo.MaxHotkeyCount}");
                _logger.LogDebug("Maximum hotkey limit reached: {Count}/{Max}", _hotkeyIds.Count, systemInfo.MaxHotkeyCount);
                return false;
            }
            
            Console.WriteLine($"[AVAILABILITY] ✅ Hotkey count limit check passed");
            Console.WriteLine($"[AVAILABILITY] 🎯 RESULT: Key combination {keyCombination} is AVAILABLE");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AVAILABILITY] ❌ EXCEPTION: {ex.Message}");
            _logger.LogError(ex, "Failed to check hotkey availability for {Keys}", keyCombination);
            return false;
        }
    }

    /// <summary>
    /// Gets all currently registered hotkeys
    /// </summary>
    /// <returns>Collection of registered hotkeys</returns>
    public async Task<IEnumerable<HotkeyInfo>> GetRegisteredHotkeysAsync()
    {
        try
        {
            _logger.LogDebug("Getting registered hotkeys");
            return await _hotkeyRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get registered hotkeys");
            throw;
        }
    }

    /// <summary>
    /// Gets a specific hotkey by ID
    /// </summary>
    /// <param name="hotkeyId">Hotkey ID</param>
    /// <returns>Hotkey information if found, null otherwise</returns>
    public async Task<HotkeyInfo?> GetHotkeyAsync(ObjectId hotkeyId)
    {
        try
        {
            if (hotkeyId == ObjectId.Empty)
            {
                _logger.LogWarning("Hotkey ID is empty");
                return null;
            }

            _logger.LogDebug("Getting hotkey: {HotkeyId}", hotkeyId);
            return await _hotkeyRepository.GetByIdAsync(hotkeyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hotkey {HotkeyId}", hotkeyId);
            throw;
        }
    }

    /// <summary>
    /// Gets hotkeys by action type
    /// </summary>
    /// <param name="action">Action type to filter by</param>
    /// <returns>Collection of hotkeys for the specified action</returns>
    public async Task<IEnumerable<HotkeyInfo>> GetHotkeysByActionAsync(HotkeyAction action)
    {
        try
        {
            _logger.LogDebug("Getting hotkeys by action: {Action}", action);
            var allHotkeys = await _hotkeyRepository.GetAllAsync();
            return allHotkeys.Where(h => h.Action == action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hotkeys by action {Action}", action);
            throw;
        }
    }

    /// <summary>
    /// Gets hotkeys associated with a specific layout
    /// </summary>
    /// <param name="layoutId">Layout ID</param>
    /// <returns>Collection of hotkeys for the specified layout</returns>
    public async Task<IEnumerable<HotkeyInfo>> GetHotkeysByLayoutAsync(ObjectId layoutId)
    {
        try
        {
            if (layoutId == ObjectId.Empty)
            {
                _logger.LogWarning("Layout ID is empty");
                return Enumerable.Empty<HotkeyInfo>();
            }

            _logger.LogDebug("Getting hotkeys for layout: {LayoutId}", layoutId);
            var allHotkeys = await _hotkeyRepository.GetAllAsync();
            return allHotkeys.Where(h => h.LayoutId != ObjectId.Empty && h.LayoutId == layoutId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hotkeys for layout {LayoutId}", layoutId);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing hotkey
    /// </summary>
    /// <param name="hotkey">Updated hotkey information</param>
    /// <returns>True if update was successful</returns>
    public async Task<bool> UpdateHotkeyAsync(HotkeyInfo hotkey)
    {
        try
        {
            if (hotkey == null)
            {
                _logger.LogWarning("Hotkey is null");
                return false;
            }

            if (!hotkey.IsValid())
            {
                _logger.LogWarning("Hotkey is not valid: {HotkeyId}", hotkey.Id);
                return false;
            }

            _logger.LogDebug("Updating hotkey: {HotkeyId}", hotkey.Id);

            // Check if hotkey exists
            var existingHotkey = await _hotkeyRepository.GetByIdAsync(hotkey.Id);
            if (existingHotkey == null)
            {
                _logger.LogWarning("Hotkey {HotkeyId} not found for update", hotkey.Id);
                return false;
            }

            // If keys changed, check availability
            if (existingHotkey.Keys != hotkey.Keys)
            {
                if (!await IsHotkeyAvailableAsync(hotkey.Keys))
                {
                    _logger.LogWarning("New key combination {Keys} is not available", hotkey.Keys);
                    return false;
                }

                // Re-register with platform if keys changed
                if (_hotkeyIds.TryGetValue(hotkey.Id, out var platformId))
                {
                    // Unregister old combination
                    _hotkeyApi.TryUnregisterHotkey(platformId, out _);

                    // Register new combination
                    var modifiers = ConvertToPlatformModifiers(hotkey.Modifiers);
                    var virtualKey = ConvertToVirtualKey(hotkey.Key);
                    
                    if (!_hotkeyApi.TryRegisterHotkey(platformId, modifiers, virtualKey, out var error))
                    {
                        _logger.LogError("Failed to re-register hotkey with new keys: {Error}", error);
                        return false;
                    }
                }
            }

            // Update in repository
            await _hotkeyRepository.UpdateAsync(hotkey);

            _logger.LogInformation("Successfully updated hotkey {HotkeyId}", hotkey.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update hotkey {HotkeyId}", hotkey?.Id);
            return false;
        }
    }

    /// <summary>
    /// Enables a previously disabled hotkey
    /// </summary>
    /// <param name="hotkeyId">Hotkey ID to enable</param>
    /// <returns>True if enable was successful</returns>
    public async Task<bool> EnableHotkeyAsync(ObjectId hotkeyId)
    {
        try
        {
            if (hotkeyId == ObjectId.Empty)
            {
                _logger.LogWarning("Hotkey ID is empty");
                return false;
            }

            _logger.LogDebug("Enabling hotkey: {HotkeyId}", hotkeyId);

            var hotkey = await _hotkeyRepository.GetByIdAsync(hotkeyId);
            if (hotkey == null)
            {
                _logger.LogWarning("Hotkey {HotkeyId} not found", hotkeyId);
                return false;
            }

            hotkey.Enable();
            await _hotkeyRepository.UpdateAsync(hotkey);

            _logger.LogInformation("Successfully enabled hotkey {HotkeyId}", hotkeyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enable hotkey {HotkeyId}", hotkeyId);
            return false;
        }
    }

    /// <summary>
    /// Disables a hotkey (temporarily prevents it from working)
    /// </summary>
    /// <param name="hotkeyId">Hotkey ID to disable</param>
    /// <returns>True if disable was successful</returns>
    public async Task<bool> DisableHotkeyAsync(ObjectId hotkeyId)
    {
        try
        {
            if (hotkeyId == ObjectId.Empty)
            {
                _logger.LogWarning("Hotkey ID is empty");
                return false;
            }

            _logger.LogDebug("Disabling hotkey: {HotkeyId}", hotkeyId);

            var hotkey = await _hotkeyRepository.GetByIdAsync(hotkeyId);
            if (hotkey == null)
            {
                _logger.LogWarning("Hotkey {HotkeyId} not found", hotkeyId);
                return false;
            }

            hotkey.Disable();
            await _hotkeyRepository.UpdateAsync(hotkey);

            _logger.LogInformation("Successfully disabled hotkey {HotkeyId}", hotkeyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disable hotkey {HotkeyId}", hotkeyId);
            return false;
        }
    }

    /// <summary>
    /// Changes the key combination for an existing hotkey
    /// </summary>
    /// <param name="hotkeyId">Hotkey ID</param>
    /// <param name="newKeyCombination">New key combination</param>
    /// <returns>True if change was successful</returns>
    public async Task<bool> ChangeHotkeyKeysAsync(ObjectId hotkeyId, string newKeyCombination)
    {
        try
        {
            if (hotkeyId == ObjectId.Empty || string.IsNullOrWhiteSpace(newKeyCombination))
            {
                _logger.LogWarning("Hotkey ID is empty or new key combination is null or empty");
                return false;
            }

            _logger.LogDebug("Changing hotkey {HotkeyId} keys to: {NewKeys}", hotkeyId, newKeyCombination);

            var hotkey = await _hotkeyRepository.GetByIdAsync(hotkeyId);
            if (hotkey == null)
            {
                _logger.LogWarning("Hotkey {HotkeyId} not found", hotkeyId);
                return false;
            }

            // Check if new combination is available
            if (!await IsHotkeyAvailableAsync(newKeyCombination))
            {
                _logger.LogWarning("New key combination {Keys} is not available", newKeyCombination);
                return false;
            }

            // Update keys
            hotkey.UpdateKeys(newKeyCombination);
            await _hotkeyRepository.UpdateAsync(hotkey);

            // Re-register with platform
            if (_hotkeyIds.TryGetValue(hotkeyId, out var platformId))
            {
                // Unregister old combination
                _hotkeyApi.TryUnregisterHotkey(platformId, out _);

                // Register new combination
                var modifiers = ConvertToPlatformModifiers(hotkey.Modifiers);
                var virtualKey = ConvertToVirtualKey(hotkey.Key);
                
                if (!_hotkeyApi.TryRegisterHotkey(platformId, modifiers, virtualKey, out var error))
                {
                    _logger.LogError("Failed to re-register hotkey with new keys: {Error}", error);
                    return false;
                }
            }

            _logger.LogInformation("Successfully changed hotkey {HotkeyId} keys to {NewKeys}", hotkeyId, newKeyCombination);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change hotkey {HotkeyId} keys", hotkeyId);
            return false;
        }
    }

    /// <summary>
    /// Resolves hotkey conflicts by choosing a preferred hotkey
    /// </summary>
    /// <param name="conflict">Conflict information</param>
    /// <param name="preferredHotkeyId">ID of the hotkey to keep</param>
    /// <returns>True if conflict was resolved</returns>
    public async Task<bool> ResolveHotkeyConflictAsync(HotkeyConflict conflict, ObjectId preferredHotkeyId)
    {
        try
        {
            if (conflict == null || preferredHotkeyId == ObjectId.Empty)
            {
                _logger.LogWarning("Conflict is null or preferred hotkey ID is empty");
                return false;
            }

            _logger.LogDebug("Resolving hotkey conflict for keys: {Keys}, preferred: {PreferredId}", conflict.KeyCombination, preferredHotkeyId);

            // Unregister all conflicting hotkeys except the preferred one
            foreach (var conflictingHotkey in conflict.ConflictingHotkeys)
            {
                if (conflictingHotkey.Id != preferredHotkeyId)
                {
                    await UnregisterHotkeyAsync(conflictingHotkey.Id);
                }
            }

            conflict.IsResolved = true;
            _logger.LogInformation("Successfully resolved hotkey conflict for keys: {Keys}", conflict.KeyCombination);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve hotkey conflict for keys: {Keys}", conflict.KeyCombination);
            return false;
        }
    }

    /// <summary>
    /// Gets hotkey usage statistics
    /// </summary>
    /// <returns>Statistics about hotkey usage</returns>
    public async Task<HotkeyStatistics> GetHotkeyStatisticsAsync()
    {
        try
        {
            _logger.LogDebug("Getting hotkey statistics");

            var allHotkeys = await _hotkeyRepository.GetAllAsync();
            var hotkeysList = allHotkeys.ToList();

            var statistics = new HotkeyStatistics
            {
                TotalHotkeys = hotkeysList.Count,
                ActiveHotkeys = hotkeysList.Count(h => h.IsEnabled),
                DisabledHotkeys = hotkeysList.Count(h => !h.IsEnabled),
                LayoutAssociatedHotkeys = hotkeysList.Count(h => h.LayoutId != ObjectId.Empty)
            };

            _logger.LogDebug("Hotkey statistics calculated: {TotalHotkeys} total, {ActiveHotkeys} active", 
                statistics.TotalHotkeys, statistics.ActiveHotkeys);

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get hotkey statistics");
            throw;
        }
    }

    /// <summary>
    /// Records hotkey usage for statistics
    /// </summary>
    /// <param name="hotkeyId">Hotkey ID that was used</param>
    /// <returns>True if recording was successful</returns>
    public async Task<bool> RecordHotkeyUsageAsync(ObjectId hotkeyId)
    {
        try
        {
            if (hotkeyId == ObjectId.Empty)
            {
                _logger.LogWarning("Hotkey ID is empty");
                return false;
            }

            _logger.LogDebug("Recording usage for hotkey: {HotkeyId}", hotkeyId);

            var hotkey = await _hotkeyRepository.GetByIdAsync(hotkeyId);
            if (hotkey == null)
            {
                _logger.LogWarning("Hotkey {HotkeyId} not found for usage recording", hotkeyId);
                return false;
            }

            // Update last used timestamp (if we had one in the model)
            // For now, just log the usage
            _logger.LogInformation("Hotkey {HotkeyId} was used", hotkeyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record usage for hotkey {HotkeyId}", hotkeyId);
            return false;
        }
    }

    /// <summary>
    /// Validates hotkey configuration
    /// </summary>
    /// <param name="hotkey">Hotkey to validate</param>
    /// <returns>Validation result with details</returns>
    public async Task<HotkeyValidationResult> ValidateHotkeyAsync(HotkeyInfo hotkey)
    {
        try
        {
            if (hotkey == null)
            {
                return new HotkeyValidationResult
                {
                    IsValid = false,
                    Errors = new List<string> { "Hotkey is null" },
                    Warnings = new List<string>(),
                    CanBeRegistered = false,
                    HasConflicts = false,
                    ConflictingHotkeyIds = new List<ObjectId>(),
                    IsSystemSupported = false
                };
            }

            var errors = new List<string>();
            var warnings = new List<string>();
            var conflictingIds = new List<ObjectId>();

            // Basic validation
            if (!hotkey.IsValid())
            {
                errors.Add("Hotkey basic validation failed");
            }

            // Check key combination availability
            if (!await IsHotkeyAvailableAsync(hotkey.Keys))
            {
                errors.Add($"Key combination '{hotkey.Keys}' is not available");
            }

            // Check for conflicts
            var allHotkeys = await _hotkeyRepository.GetAllAsync();
            var existingHotkey = allHotkeys.FirstOrDefault(h => h.Keys == hotkey.Keys && h.Id != hotkey.Id);
            if (existingHotkey != null)
            {
                errors.Add($"Key combination '{hotkey.Keys}' conflicts with existing hotkey '{existingHotkey.Id}'");
                conflictingIds.Add(existingHotkey.Id);
            }

            // Platform-specific validation
            var systemInfo = _hotkeyApi.GetSystemInfo();
            var isSystemSupported = systemInfo.SupportsGlobalHotkeys;

            return new HotkeyValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors,
                Warnings = warnings,
                CanBeRegistered = errors.Count == 0,
                HasConflicts = conflictingIds.Count > 0,
                ConflictingHotkeyIds = conflictingIds,
                IsSystemSupported = isSystemSupported
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate hotkey {HotkeyId}", hotkey?.Id);
            return new HotkeyValidationResult
            {
                IsValid = false,
                Errors = new List<string> { $"Validation error: {ex.Message}" },
                Warnings = new List<string>(),
                CanBeRegistered = false,
                HasConflicts = false,
                ConflictingHotkeyIds = new List<ObjectId>(),
                IsSystemSupported = false
            };
        }
    }

    /// <summary>
    /// Gets system-wide hotkey information
    /// </summary>
    /// <returns>Information about system hotkey support</returns>
    public async Task<SystemHotkeyInfo> GetSystemHotkeyInfoAsync()
    {
        try
        {
            _logger.LogDebug("Getting system hotkey information");

            var platformInfo = _hotkeyApi.GetSystemInfo();
            var statistics = await GetHotkeyStatisticsAsync();

            return new SystemHotkeyInfo
            {
                GlobalHotkeysSupported = platformInfo.SupportsGlobalHotkeys,
                MaxHotkeysSupported = platformInfo.MaxHotkeyCount,
                CurrentRegistrationsSupported = _hotkeyIds.Count <= platformInfo.MaxHotkeyCount,
                SystemLimitations = platformInfo.Limitations,
                HotkeysSuspended = _isSuspended
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get system hotkey information");
            throw;
        }
    }

    /// <summary>
    /// Refreshes all hotkey registrations
    /// </summary>
    /// <returns>True if refresh was successful</returns>
    public async Task<bool> RefreshHotkeysAsync()
    {
        try
        {
            _logger.LogDebug("Refreshing all hotkey registrations");

            // Get all hotkeys from repository
            var allHotkeys = await _hotkeyRepository.GetAllAsync();
            var enabledHotkeys = allHotkeys.Where(h => h.IsEnabled).ToList();

            // Unregister all current platform registrations
            foreach (var platformId in _hotkeyIds.Values)
            {
                _hotkeyApi.TryUnregisterHotkey(platformId, out _);
            }

            // Clear local tracking
            _hotkeyIds.Clear();
            _asyncCallbacks.Clear();
            _syncCallbacks.Clear();
            _nextHotkeyId = 1;

            // Re-register enabled hotkeys
            foreach (var hotkey in enabledHotkeys)
            {
                // Note: We can't re-register callbacks here since they're not stored
                // This is a limitation - callbacks need to be re-registered by the caller
                _logger.LogWarning("Hotkey {HotkeyId} needs callback re-registration after refresh", hotkey.Id);
            }

            _logger.LogInformation("Successfully refreshed hotkey registrations. {Count} hotkeys need callback re-registration", enabledHotkeys.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh hotkey registrations");
            return false;
        }
    }

    /// <summary>
    /// Suspends all hotkey processing temporarily
    /// </summary>
    /// <returns>True if suspension was successful</returns>
    public async Task<bool> SuspendHotkeysAsync()
    {
        try
        {
            _logger.LogDebug("Suspending hotkey processing");

            if (_isSuspended)
            {
                _logger.LogInformation("Hotkeys are already suspended");
                return true;
            }

            _isSuspended = true;

            // Unregister all platform hotkeys
            foreach (var platformId in _hotkeyIds.Values)
            {
                _hotkeyApi.TryUnregisterHotkey(platformId, out _);
            }

            _logger.LogInformation("Successfully suspended hotkey processing");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to suspend hotkey processing");
            return false;
        }
    }

    /// <summary>
    /// Resumes hotkey processing after suspension
    /// </summary>
    /// <returns>True if resumption was successful</returns>
    public async Task<bool> ResumeHotkeysAsync()
    {
        try
        {
            _logger.LogDebug("Resuming hotkey processing");

            if (!_isSuspended)
            {
                _logger.LogInformation("Hotkeys are not suspended");
                return true;
            }

            _isSuspended = false;

            // Re-register all hotkeys with platform
            var allHotkeys = await _hotkeyRepository.GetAllAsync();
            var enabledHotkeys = allHotkeys.Where(h => h.IsEnabled).ToList();

            foreach (var hotkey in enabledHotkeys)
            {
                if (_hotkeyIds.ContainsKey(hotkey.Id))
                {
                    var platformId = _hotkeyIds[hotkey.Id];
                    var modifiers = ConvertToPlatformModifiers(hotkey.Modifiers);
                    var virtualKey = ConvertToVirtualKey(hotkey.Key);

                    if (!_hotkeyApi.TryRegisterHotkey(platformId, modifiers, virtualKey, out var error))
                    {
                        _logger.LogWarning("Failed to re-register hotkey {HotkeyId} after resume: {Error}", hotkey.Id, error);
                    }
                }
            }

            _logger.LogInformation("Successfully resumed hotkey processing");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resume hotkey processing");
            return false;
        }
    }

    /// <summary>
    /// Handles a hotkey press event from the platform
    /// </summary>
    /// <param name="platformId">Platform hotkey ID</param>
    /// <returns>True if handled successfully</returns>
    public async Task<bool> HandleHotkeyPressAsync(int platformId)
    {
        try
        {
            if (_isSuspended)
            {
                _logger.LogDebug("Hotkey press ignored - service is suspended");
                return false;
            }

            // Find the hotkey by platform ID
            var hotkeyId = _hotkeyIds.FirstOrDefault(kvp => kvp.Value == platformId).Key;
            if (hotkeyId == ObjectId.Empty)
            {
                _logger.LogWarning("Unknown platform hotkey ID: {PlatformId}", platformId);
                return false;
            }

            _logger.LogDebug("Handling hotkey press for: {HotkeyId}", hotkeyId);

            // Record usage
            await RecordHotkeyUsageAsync(hotkeyId);

            // Execute callback
            if (_asyncCallbacks.TryGetValue(hotkeyId, out var asyncCallback))
            {
                await asyncCallback();
                return true;
            }
            else if (_syncCallbacks.TryGetValue(hotkeyId, out var syncCallback))
            {
                syncCallback();
                return true;
            }
            else
            {
                _logger.LogWarning("No callback found for hotkey: {HotkeyId}", hotkeyId);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle hotkey press for platform ID: {PlatformId}", platformId);
            return false;
        }
    }

    /// <summary>
    /// Converts our ModifierKey enum to platform HotkeyModifiers
    /// </summary>
    private static HotkeyModifiers ConvertToPlatformModifiers(List<ModifierKey> modifiers)
    {
        var result = HotkeyModifiers.None;

        foreach (var modifier in modifiers)
        {
            result |= modifier switch
            {
                ModifierKey.Ctrl => HotkeyModifiers.Control,
                ModifierKey.Shift => HotkeyModifiers.Shift,
                ModifierKey.Alt => HotkeyModifiers.Alt,
                ModifierKey.Win => HotkeyModifiers.Win,
                _ => HotkeyModifiers.None
            };
        }

        return result;
    }

    /// <summary>
    /// Converts a key string to virtual key code
    /// </summary>
    private static int ConvertToVirtualKey(string key)
    {
        // Extract the base key (before any underscore or special characters)
        var baseKey = key.Split('_')[0].ToUpperInvariant();
        
        // This is a simplified conversion - in a real implementation,
        // you'd want a more comprehensive mapping
        return baseKey switch
        {
            "A" => 0x41,
            "B" => 0x42,
            "C" => 0x43,
            "D" => 0x44,
            "E" => 0x45,
            "F" => 0x46,
            "G" => 0x47,
            "H" => 0x48,
            "I" => 0x49,
            "J" => 0x4A,
            "K" => 0x4B,
            "L" => 0x4C,
            "M" => 0x4D,
            "N" => 0x4E,
            "O" => 0x4F,
            "P" => 0x50,
            "Q" => 0x51,
            "R" => 0x52,
            "S" => 0x53,
            "T" => 0x54,
            "U" => 0x55,
            "V" => 0x56,
            "W" => 0x57,
            "X" => 0x58,
            "Y" => 0x59,
            "Z" => 0x5A,
            "0" => 0x30,
            "1" => 0x31,
            "2" => 0x32,
            "3" => 0x33,
            "4" => 0x34,
            "5" => 0x35,
            "6" => 0x36,
            "7" => 0x37,
            "8" => 0x38,
            "9" => 0x39,
            "F1" => 0x70,
            "F2" => 0x71,
            "F3" => 0x72,
            "F4" => 0x73,
            "F5" => 0x74,
            "F6" => 0x75,
            "F7" => 0x76,
            "F8" => 0x77,
            "F9" => 0x78,
            "F10" => 0x79,
            "F11" => 0x7A,
            "F12" => 0x7B,
            _ => 0x00 // Unknown key
        };
    }
}
