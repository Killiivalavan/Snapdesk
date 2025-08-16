using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;
using SnapDesk.Core.Interfaces;
using SnapDesk.Data.Services;

namespace SnapDesk.Data.Repositories;

/// <summary>
/// Base repository implementation for LiteDB operations
/// </summary>
/// <typeparam name="T">Type of entity to manage</typeparam>
public abstract class RepositoryBase<T> : IRepository<T> where T : class
{
    protected readonly IDatabaseService _databaseService;
    protected readonly ILogger _logger;
    protected readonly string _collectionName;

    /// <summary>
    /// Gets the LiteDB collection for this repository
    /// </summary>
    protected ILiteCollection<T> Collection => _databaseService.GetCollection<T>(_collectionName);

    /// <summary>
    /// Constructor for repository base
    /// </summary>
    /// <param name="databaseService">Database service</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="collectionName">Name of the collection</param>
    protected RepositoryBase(IDatabaseService databaseService, ILogger logger, string collectionName)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _collectionName = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
    }

    /// <summary>
    /// Gets an entity by its unique identifier
    /// </summary>
    /// <param name="id">Entity identifier</param>
    /// <returns>Entity if found, null otherwise</returns>
    public virtual async Task<T?> GetByIdAsync(ObjectId id)
    {
        try
        {
            return await Task.Run(() =>
            {
                if (id == ObjectId.Empty)
                    return null;

                return Collection.FindById(id);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get entity by ID: {Id} from collection: {Collection}", id, _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <returns>Collection of all entities</returns>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            return await Task.Run(() =>
            {
                return Collection.FindAll().ToList();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all entities from collection: {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Gets entities that match a predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <returns>Collection of matching entities</returns>
    public virtual async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await Task.Run(() =>
            {
                return Collection.Find(predicate).ToList();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get entities with predicate from collection: {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Gets entities with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated collection of entities</returns>
    public virtual async Task<PaginatedResult<T>> GetPagedAsync(int pageNumber, int pageSize)
    {
        try
        {
            return await Task.Run(() =>
            {
                var skip = (pageNumber - 1) * pageSize;
                var totalItems = Collection.Count();
                var items = Collection.FindAll().Skip(skip).Take(pageSize).ToList();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                return new PaginatedResult<T>
                {
                    Items = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get paged entities from collection: {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Gets entities with pagination and filtering
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated collection of matching entities</returns>
    public virtual async Task<PaginatedResult<T>> GetPagedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize)
    {
        try
        {
            return await Task.Run(() =>
            {
                var skip = (pageNumber - 1) * pageSize;
                var totalItems = Collection.Count(predicate);
                var items = Collection.Find(predicate).Skip(skip).Take(pageSize).ToList();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                return new PaginatedResult<T>
                {
                    Items = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get paged entities with predicate from collection: {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Gets entities with ordering
    /// </summary>
    /// <param name="orderBy">Ordering expression</param>
    /// <param name="ascending">Whether to order ascending (true) or descending (false)</param>
    /// <returns>Ordered collection of entities</returns>
    public virtual async Task<IEnumerable<T>> GetOrderedAsync(Expression<Func<T, object>> orderBy, bool ascending = true)
    {
        try
        {
            return await Task.Run(() =>
            {
                var query = Collection.Query();
                
                if (ascending)
                {
                    return query.OrderBy(orderBy).ToList();
                }
                else
                {
                    return query.OrderByDescending(orderBy).ToList();
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get ordered entities from collection: {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Gets entities with filtering and ordering
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="orderBy">Ordering expression</param>
    /// <param name="ascending">Whether to order ascending (true) or descending (false)</param>
    /// <returns>Filtered and ordered collection of entities</returns>
    public virtual async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> orderBy, bool ascending = true)
    {
        try
        {
            return await Task.Run(() =>
            {
                var query = Collection.Query().Where(predicate);
                
                if (ascending)
                {
                    return query.OrderBy(orderBy).ToList();
                }
                else
                {
                    return query.OrderByDescending(orderBy).ToList();
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get filtered and ordered entities from collection: {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Checks if an entity exists
    /// </summary>
    /// <param name="id">Entity identifier</param>
    /// <returns>True if entity exists, false otherwise</returns>
    public virtual async Task<bool> ExistsAsync(ObjectId id)
    {
        try
        {
            return await Task.Run(() =>
            {
                if (id == ObjectId.Empty)
                    return false;

                return Collection.Exists(Query.EQ("_id", id));
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check entity existence: {Id} in collection: {Collection}", id, _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Checks if any entities match a predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <returns>True if any entities match, false otherwise</returns>
    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await Task.Run(() =>
            {
                return Collection.Count(predicate) > 0;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check entity existence with predicate in collection: {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Counts all entities
    /// </summary>
    /// <returns>Total number of entities</returns>
    public virtual async Task<int> CountAsync()
    {
        try
        {
            return await Task.Run(() =>
            {
                return Collection.Count();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count entities in collection: {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Counts entities that match a predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <returns>Number of matching entities</returns>
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await Task.Run(() =>
            {
                return Collection.Count(predicate);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count entities with predicate in collection: {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Inserts a new entity
    /// </summary>
    /// <param name="entity">Entity to insert</param>
    /// <returns>Inserted entity with generated ID</returns>
    public virtual async Task<T> InsertAsync(T entity)
    {
        try
        {
            return await Task.Run(() =>
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                Collection.Insert(entity);
                return entity;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert entity in collection: {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Inserts multiple entities
    /// </summary>
    /// <param name="entities">Entities to insert</param>
    /// <returns>Collection of inserted entities</returns>
    public virtual async Task<IEnumerable<T>> InsertManyAsync(IEnumerable<T> entities)
    {
        try
        {
            return await Task.Run(() =>
            {
                if (entities == null)
                    throw new ArgumentNullException(nameof(entities));

                var entityList = entities.ToList();
                Collection.InsertBulk(entityList);
                return entityList;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert multiple entities in collection: {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <returns>True if update was successful</returns>
    public virtual async Task<bool> UpdateAsync(T entity)
    {
        try
        {
            return await Task.Run(() =>
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                return Collection.Update(entity);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update entity in collection: {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Updates multiple entities
    /// </summary>
    /// <param name="entities">Entities to update</param>
    /// <returns>Number of successfully updated entities</returns>
    public virtual async Task<int> UpdateManyAsync(IEnumerable<T> entities)
    {
        try
        {
            return await Task.Run(() =>
            {
                if (entities == null)
                    throw new ArgumentNullException(nameof(entities));

                int updated = 0;
                foreach (var entity in entities)
                {
                    if (Collection.Update(entity))
                        updated++;
                }
                return updated;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update multiple entities in collection: {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Deletes an entity by ID
    /// </summary>
    /// <param name="id">Entity identifier</param>
    /// <returns>True if deletion was successful</returns>
    public virtual async Task<bool> DeleteAsync(ObjectId id)
    {
        try
        {
            return await Task.Run(() =>
            {
                if (id == ObjectId.Empty)
                    return false;

                return Collection.Delete(id);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete entity by ID: {Id} from collection: {Collection}", id, _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Deletes an entity
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    /// <returns>True if deletion was successful</returns>
    public virtual async Task<bool> DeleteAsync(T entity)
    {
        try
        {
            return await Task.Run(() =>
            {
                if (entity == null)
                    return false;

                // Get entity ID through LiteDB's built-in methods
                var mapper = BsonMapper.Global;
                var entityDoc = mapper.ToDocument(entity);
                var id = entityDoc["_id"];
                
                if (id == null || id.IsNull)
                    return false;

                return Collection.Delete(id);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete entity from collection: {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Deletes entities that match a predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <returns>Number of successfully deleted entities</returns>
    public virtual async Task<int> DeleteManyAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await Task.Run(() =>
            {
                return Collection.DeleteMany(predicate);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete multiple entities from collection: {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Deletes all entities
    /// </summary>
    /// <returns>Number of successfully deleted entities</returns>
    public virtual async Task<int> DeleteAllAsync()
    {
        try
        {
            return await Task.Run(() =>
            {
                return Collection.DeleteAll();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete all entities from collection: {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Begins a transaction
    /// </summary>
    /// <returns>Transaction object</returns>
    public virtual async Task<ITransaction> BeginTransactionAsync()
    {
        // LiteDB doesn't support explicit transactions, so we return a no-op implementation
        return await Task.FromResult(new LiteDbTransaction());
    }

    /// <summary>
    /// Performs a bulk operation
    /// </summary>
    /// <param name="operation">Bulk operation to perform</param>
    /// <returns>Result of the bulk operation</returns>
    public virtual async Task<BulkOperationResult> BulkOperationAsync(Func<IQueryable<T>, Task> operation)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            var queryable = AsQueryable();
            await operation(queryable);

            var executionTime = DateTime.UtcNow - startTime;
            return new BulkOperationResult
            {
                IsSuccess = true,
                AffectedCount = -1, // Cannot determine affected count without specific operation details
                ExecutionTimeMs = (long)executionTime.TotalMilliseconds
            };
        }
        catch (Exception ex)
        {
            var executionTime = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Bulk operation failed in collection: {Collection}", _collectionName);
            
            return new BulkOperationResult
            {
                IsSuccess = false,
                AffectedCount = 0,
                ErrorMessage = ex.Message,
                ExecutionTimeMs = (long)executionTime.TotalMilliseconds
            };
        }
    }

    /// <summary>
    /// Gets a queryable interface for complex queries
    /// </summary>
    /// <returns>Queryable interface</returns>
    public virtual IQueryable<T> AsQueryable()
    {
        return Collection.FindAll().AsQueryable();
    }

    /// <summary>
    /// Refreshes entity data from the database
    /// </summary>
    /// <param name="entity">Entity to refresh</param>
    /// <returns>Refreshed entity</returns>
    public virtual async Task<T> RefreshAsync(T entity)
    {
        try
        {
            return await Task.Run(() =>
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var mapper = BsonMapper.Global;
                var entityDoc = mapper.ToDocument(entity);
                var id = entityDoc["_id"];
                
                if (id == null || id.IsNull)
                    throw new InvalidOperationException("Entity does not have a valid ID");

                var refreshed = Collection.FindById(id);
                return refreshed ?? entity;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh entity from collection: {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Checks database connectivity
    /// </summary>
    /// <returns>True if database is accessible</returns>
    public virtual async Task<bool> IsConnectedAsync()
    {
        try
        {
            return await _databaseService.TestConnectionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check database connectivity");
            return false;
        }
    }

    /// <summary>
    /// Gets database statistics
    /// </summary>
    /// <returns>Database statistics</returns>
    public virtual async Task<DatabaseStatistics> GetStatisticsAsync()
    {
        try
        {
            return await _databaseService.GetStatisticsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get database statistics");
            throw;
        }
    }
}

/// <summary>
/// No-op transaction implementation for LiteDB
/// </summary>
internal class LiteDbTransaction : ITransaction
{
    public bool IsActive { get; private set; } = true;

    public Task CommitAsync()
    {
        IsActive = false;
        return Task.CompletedTask;
    }

    public Task RollbackAsync()
    {
        IsActive = false;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        IsActive = false;
    }
}
