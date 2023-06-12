// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Cache;

/// <summary>
/// In-memory cache provider with expiration and statistics tracking
/// Thread-safe implementation using ReaderWriterLockSlim
/// </summary>
public class InMemoryCacheProvider : ICacheProvider
{
    private readonly Dictionary<string, (object? Value, DateTime? ExpiresAt, int AccessCount, DateTime? LastAccess)> _cache;
    private readonly ReaderWriterLockSlim _lock;
    private readonly ILogger<InMemoryCacheProvider> _logger;
    private int _hitCount;
    private int _missCount;

    public InMemoryCacheProvider(ILogger<InMemoryCacheProvider> logger)
    {
        _cache = new Dictionary<string, (object?, DateTime?, int, DateTime?)>();
        _lock = new ReaderWriterLockSlim();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieve value from cache, updating access statistics
    /// </summary>
    public Task<T?> GetAsync<T>(string key)
    {
        _lock.EnterReadLock();
        try
        {
            if (string.IsNullOrEmpty(key))
                return Task.FromResult<T?>(default);

            if (_cache.TryGetValue(key, out var entry))
            {
                // Check if expired
                if (entry.ExpiresAt != null && DateTime.UtcNow > entry.ExpiresAt)
                {
                    _lock.ExitReadLock();
                    _lock.EnterWriteLock();
                    try
                    {
                        _cache.Remove(key);
                        _missCount++;
                        return Task.FromResult<T?>(default);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }

                // Update access metadata
                _lock.ExitReadLock();
                _lock.EnterWriteLock();
                try
                {
                    _cache[key] = (entry.Value, entry.ExpiresAt, entry.AccessCount + 1, DateTime.UtcNow);
                    _hitCount++;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }

                _logger.LogDebug("Cache hit for key: {Key}", key);
                return Task.FromResult((T?)entry.Value);
            }

            _missCount++;
            _logger.LogDebug("Cache miss for key: {Key}", key);
            return Task.FromResult<T?>(default);
        }
        finally
        {
            if (_lock.IsReadLockHeld)
                _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Store value in cache with optional expiration
    /// </summary>
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be empty", nameof(key));

        _lock.EnterWriteLock();
        try
        {
            DateTime? expiresAt = expiration != null ? DateTime.UtcNow.Add(expiration.Value) : null;
            _cache[key] = (value, expiresAt, 0, null);

            _logger.LogDebug("Cache set for key: {Key}, expires: {Expires}",
                key, expiresAt?.ToString("O") ?? "never");

            return Task.CompletedTask;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Remove entry from cache
    /// </summary>
    public Task RemoveAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return Task.CompletedTask;

        _lock.EnterWriteLock();
        try
        {
            if (_cache.Remove(key))
            {
                _logger.LogDebug("Cache entry removed: {Key}", key);
            }
            return Task.CompletedTask;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Check if key exists and is not expired
    /// </summary>
    public Task<bool> ExistsAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return Task.FromResult(false);

        _lock.EnterReadLock();
        try
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.ExpiresAt != null && DateTime.UtcNow > entry.ExpiresAt)
                {
                    return Task.FromResult(false);
                }
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Clear all cache entries
    /// </summary>
    public Task ClearAsync()
    {
        _lock.EnterWriteLock();
        try
        {
            var count = _cache.Count;
            _cache.Clear();
            _logger.LogInformation("Cache cleared. Removed {Count} entries", count);
            return Task.CompletedTask;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Get all cache keys, removing expired entries in the process
    /// </summary>
    public Task<List<string>> GetKeysAsync()
    {
        _lock.EnterWriteLock();
        try
        {
            var keysToRemove = _cache
                .Where(kvp => kvp.Value.ExpiresAt != null && DateTime.UtcNow > kvp.Value.ExpiresAt)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }

            var validKeys = _cache.Keys.ToList();
            return Task.FromResult(validKeys);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Get cache statistics for monitoring
    /// </summary>
    public Task<CacheStats> GetStatsAsync()
    {
        _lock.EnterReadLock();
        try
        {
            // Count non-expired entries
            var validCount = _cache.Count(kvp =>
                kvp.Value.ExpiresAt == null || DateTime.UtcNow <= kvp.Value.ExpiresAt);

            var stats = new CacheStats
            {
                ItemCount = validCount,
                HitCount = _hitCount,
                MissCount = _missCount,
                TotalSize = EstimateCacheSize()
            };

            return Task.FromResult(stats);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Estimate cache size in bytes
    /// </summary>
    private long EstimateCacheSize()
    {
        long size = 0;
        foreach (var kvp in _cache)
        {
            size += kvp.Key.Length * sizeof(char);
            if (kvp.Value.Value != null)
            {
                // Fix: Safely estimate size by checking if the type is blittable to avoid Marshal.SizeOf throwing exceptions on managed types
                var type = kvp.Value.Value.GetType();
                if (type.IsValueType && !type.IsGenericType)
                {
                    try { size += System.Runtime.InteropServices.Marshal.SizeOf(kvp.Value.Value); }
                    catch { size += 256; }
                }
                else
                {
                    size += 256;
                }
            }
        }
        return size;
    }

    public void Dispose()
    {
        _lock?.Dispose();
    }
}
