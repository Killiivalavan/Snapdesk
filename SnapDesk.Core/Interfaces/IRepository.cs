using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SnapDesk.Core.Interfaces;

/// <summary>
/// Generic repository interface for data access operations
/// </summary>
/// <typeparam name="T">Type of entity to manage</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets an entity by its unique identifier
    /// </summary>
    /// <param name="id">Entity identifier</param>
    /// <returns>Entity if found, null otherwise</returns>
    Task<T?> GetByIdAsync(string id);

    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <returns>Collection of all entities</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Gets entities that match a predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <returns>Collection of matching entities</returns>
    Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Gets entities with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated collection of entities</returns>
    Task<PaginatedResult<T>> GetPagedAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Gets entities with pagination and filtering
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated collection of matching entities</returns>
    Task<PaginatedResult<T>> GetPagedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize);

    /// <summary>
    /// Gets entities with ordering
    /// </summary>
    /// <param name="orderBy">Ordering expression</param>
    /// <param name="ascending">Whether to order ascending (true) or descending (false)</param>
    /// <returns>Ordered collection of entities</returns>
    Task<IEnumerable<T>> GetOrderedAsync(Expression<Func<T, object>> orderBy, bool ascending = true);

    /// <summary>
    /// Gets entities with filtering and ordering
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="orderBy">Ordering expression</param>
    /// <param name="ascending">Whether to order ascending (true) or descending (false)</param>
    /// <returns>Filtered and ordered collection of entities</returns>
    Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> orderBy, bool ascending = true);

    /// <summary>
    /// Checks if an entity exists
    /// </summary>
    /// <param name="id">Entity identifier</param>
    /// <returns>True if entity exists, false otherwise</returns>
    Task<bool> ExistsAsync(string id);

    /// <summary>
    /// Checks if any entities match a predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <returns>True if any entities match, false otherwise</returns>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Counts all entities
    /// </summary>
    /// <returns>Total number of entities</returns>
    Task<int> CountAsync();

    /// <summary>
    /// Counts entities that match a predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <returns>Number of matching entities</returns>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Inserts a new entity
    /// </summary>
    /// <param name="entity">Entity to insert</param>
    /// <returns>Inserted entity with generated ID</returns>
    Task<T> InsertAsync(T entity);

    /// <summary>
    /// Inserts multiple entities
    /// </summary>
    /// <param name="entities">Entities to insert</param>
    /// <returns>Collection of inserted entities</returns>
    Task<IEnumerable<T>> InsertManyAsync(IEnumerable<T> entities);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <returns>True if update was successful</returns>
    Task<bool> UpdateAsync(T entity);

    /// <summary>
    /// Updates multiple entities
    /// </summary>
    /// <param name="entities">Entities to update</param>
    /// <returns>Number of successfully updated entities</returns>
    Task<int> UpdateManyAsync(IEnumerable<T> entities);

    /// <summary>
    /// Deletes an entity by ID
    /// </summary>
    /// <param name="id">Entity identifier</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Deletes an entity
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(T entity);

    /// <summary>
    /// Deletes entities that match a predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <returns>Number of successfully deleted entities</returns>
    Task<int> DeleteManyAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Deletes all entities
    /// </summary>
    /// <returns>Number of successfully deleted entities</returns>
    Task<int> DeleteAllAsync();

    /// <summary>
    /// Begins a transaction
    /// </summary>
    /// <returns>Transaction object</returns>
    Task<ITransaction> BeginTransactionAsync();

    /// <summary>
    /// Performs a bulk operation
    /// </summary>
    /// <param name="operation">Bulk operation to perform</param>
    /// <returns>Result of the bulk operation</returns>
    Task<BulkOperationResult> BulkOperationAsync(Func<IQueryable<T>, Task> operation);

    /// <summary>
    /// Gets a queryable interface for complex queries
    /// </summary>
    /// <returns>Queryable interface</returns>
    IQueryable<T> AsQueryable();

    /// <summary>
    /// Refreshes entity data from the database
    /// </summary>
    /// <param name="entity">Entity to refresh</param>
    /// <returns>Refreshed entity</returns>
    Task<T> RefreshAsync(T entity);

    /// <summary>
    /// Checks database connectivity
    /// </summary>
    /// <returns>True if database is accessible</returns>
    Task<bool> IsConnectedAsync();

    /// <summary>
    /// Gets database statistics
    /// </summary>
    /// <returns>Database statistics</returns>
    Task<DatabaseStatistics> GetStatisticsAsync();
}

/// <summary>
/// Paginated result wrapper
/// </summary>
/// <typeparam name="T">Type of entities</typeparam>
public class PaginatedResult<T>
{
    /// <summary>
    /// Entities in the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Whether there are more pages
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Whether there are previous pages
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
}

/// <summary>
/// Transaction interface for database operations
/// </summary>
public interface ITransaction : IDisposable
{
    /// <summary>
    /// Commits the transaction
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// Rolls back the transaction
    /// </summary>
    Task RollbackAsync();

    /// <summary>
    /// Whether the transaction is active
    /// </summary>
    bool IsActive { get; }
}

/// <summary>
/// Result of bulk operations
/// </summary>
public class BulkOperationResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Number of affected entities
    /// </summary>
    public int AffectedCount { get; set; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Execution time in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }
}

/// <summary>
/// Database statistics
/// </summary>
public class DatabaseStatistics
{
    /// <summary>
    /// Total number of entities
    /// </summary>
    public int TotalEntities { get; set; }

    /// <summary>
    /// Database size in bytes
    /// </summary>
    public long DatabaseSizeBytes { get; set; }

    /// <summary>
    /// Last backup time
    /// </summary>
    public DateTime? LastBackupTime { get; set; }

    /// <summary>
    /// Database version
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Whether database is healthy
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Health check message
    /// </summary>
    public string? HealthMessage { get; set; }
}
