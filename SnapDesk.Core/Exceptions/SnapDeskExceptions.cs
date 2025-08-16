using System;
using LiteDB;

namespace SnapDesk.Core.Exceptions;

/// <summary>
/// Base exception for all SnapDesk application exceptions
/// </summary>
public class SnapDeskException : Exception
{
    public SnapDeskException(string message) : base(message) { }
    public SnapDeskException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a layout operation fails
/// </summary>
public class LayoutOperationException : SnapDeskException
{
    public ObjectId LayoutId { get; }
    
    public LayoutOperationException(string message, ObjectId layoutId) : base(message)
    {
        LayoutId = layoutId;
    }
    
    public LayoutOperationException(string message, ObjectId layoutId, Exception innerException) : base(message, innerException)
    {
        LayoutId = layoutId;
    }
}

/// <summary>
/// Exception thrown when a hotkey operation fails
/// </summary>
public class HotkeyOperationException : SnapDeskException
{
    public ObjectId HotkeyId { get; }
    
    public HotkeyOperationException(string message, ObjectId hotkeyId) : base(message)
    {
        HotkeyId = hotkeyId;
    }
    
    public HotkeyOperationException(string message, ObjectId hotkeyId, Exception innerException) : base(message, innerException)
    {
        HotkeyId = hotkeyId;
    }
}

/// <summary>
/// Exception thrown when a window operation fails
/// </summary>
public class WindowOperationException : SnapDeskException
{
    public ObjectId WindowId { get; }
    
    public WindowOperationException(string message, ObjectId windowId) : base(message)
    {
        WindowId = windowId;
    }
    
    public WindowOperationException(string message, ObjectId windowId, Exception innerException) : base(message, innerException)
    {
        WindowId = windowId;
    }
}

/// <summary>
/// Exception thrown when a database operation fails
/// </summary>
public class DatabaseOperationException : SnapDeskException
{
    public string Operation { get; }
    public string Collection { get; }
    
    public DatabaseOperationException(string message, string operation, string collection) : base(message)
    {
        Operation = operation;
        Collection = collection;
    }
    
    public DatabaseOperationException(string message, string operation, string collection, Exception innerException) : base(message, innerException)
    {
        Operation = operation;
        Collection = collection;
    }
}

/// <summary>
/// Exception thrown when a file operation fails
/// </summary>
public class FileOperationException : SnapDeskException
{
    public string FilePath { get; }
    public string Operation { get; }
    
    public FileOperationException(string message, string operation, string filePath) : base(message)
    {
        Operation = operation;
        FilePath = filePath;
    }
    
    public FileOperationException(string message, string operation, string filePath, Exception innerException) : base(message, innerException)
    {
        Operation = operation;
        FilePath = filePath;
    }
}

/// <summary>
/// Exception thrown when a platform API operation fails
/// </summary>
public class PlatformApiException : SnapDeskException
{
    public string ApiMethod { get; }
    public string ErrorCode { get; }
    
    public PlatformApiException(string message, string apiMethod, string errorCode) : base(message)
    {
        ApiMethod = apiMethod;
        ErrorCode = errorCode;
    }
    
    public PlatformApiException(string message, string apiMethod, string errorCode, Exception innerException) : base(message, innerException)
    {
        ApiMethod = apiMethod;
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : SnapDeskException
{
    public string Field { get; }
    public object Value { get; }
    
    public ValidationException(string message, string field, object value) : base(message)
    {
        Field = field;
        Value = value;
    }
    
    public ValidationException(string message, string field, object value, Exception innerException) : base(message, innerException)
    {
        Field = field;
        Value = value;
    }
}

/// <summary>
/// Exception thrown when a resource is not found
/// </summary>
public class ResourceNotFoundException : SnapDeskException
{
    public string ResourceType { get; }
    public object ResourceId { get; }
    
    public ResourceNotFoundException(string message, string resourceType, object resourceId) : base(message)
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
    
    public ResourceNotFoundException(string message, string resourceType, object resourceId, Exception innerException) : base(message, innerException)
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}
