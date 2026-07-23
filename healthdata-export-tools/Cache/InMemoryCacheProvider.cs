#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace HealthDataExportTools.Cache;

/// <summary>
/// In-memory cache provider with expiration, size limits, and statistics tracking
/// Thread-safe implementation using ConcurrentDictionary with per-key SemaphoreSlim locking
/// Implements single-flight pattern to prevent cache stampede on concurrent misses
/// </summary>
public sealed class InMemoryCacheProvider : ICacheProvider
{
    /// <summary>
    /// Maximum number of entries allowed in cache before eviction begins
    /// </summary>
    private const int MaxCacheSize = 10000;

    /// <summary>
    /// Default expiration time for cache entries (24 hours)
    /// </summary>
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(24);

    /// <summary>
    /// Cache storage using ConcurrentDictionary for built-in thread-safety
    /// </summary>
    private readonly ConcurrentDictionary<string, CacheEntry> _cache;

    private readonly ILogger<InMemoryCacheProvider> _logger;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _keyLocks;
    private int _hitCount;
    private int _missCount;

    /// <summary>
    /// Initializes a new instance of the InMemoryCacheProvider
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null</exception>
    public InMemoryCacheProvider(ILogger<InMemoryCacheProvider> logger)
    {
        _cache = new ConcurrentDictionary<string, CacheEntry>();
        _keyLocks = new ConcurrentDictionary<string, SemaphoreSlim>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieve value from cache, updating access statistics
    /// Performs lazy expiration check on read
    /// </summary>
    /// <typeparam name="T">Type of value to retrieve</typeparam>
    /// <param name="key">Cache key</param>
    /// <returns>Cached value or null if not found or expired</returns>
    /// <exception cref="ArgumentException">Thrown when key is null or empty</exception>
    public Task<T?> GetAsync<T>(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

        if (_cache.TryGetValue(key, out var entry))
        {
            // Check if expired and remove if so
            if (entry.IsExpired)
            {
                _cache.TryRemove(key, out _);
                Interlocked.Increment(ref _missCount);
                _logger.LogDebug("Cache entry expired and removed for key: {Key}", key);
                return Task.FromResult<T?>(default);
            }

            // Update access metadata
            entry.AccessCount++;
            entry.LastAccessAt = DateTime.UtcNow;
            Interlocked.Increment(ref _hitCount);

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return Task.FromResult<T?>((T?)entry.Value);
        }

        Interlocked.Increment(ref _missCount);
        _logger.LogDebug("Cache miss for key: {Key}", key);
        return Task.FromResult<T?>(default);
    }

    /// <summary>
    /// Get value from cache, or create and store it if it doesn't exist
    /// Uses single-flight pattern to prevent cache stampede on concurrent misses
    /// </summary>
    /// <typeparam name="T">The type of value to get or create</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">The factory function to create the value if it doesn't exist</param>
    /// <param name="expiration">Optional expiration time span</param>
    /// <returns>The cached or newly created value</returns>
    /// <exception cref="ArgumentException">Thrown when key is null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when factory is null</exception>
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        // Get or create the semaphore for this key (single-flight pattern)
        var keyLock = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        try
        {
            // Wait for the lock to ensure only one thread computes the value
            await keyLock.WaitAsync().ConfigureAwait(false);

            try
            {
                // Try to get from cache first (check after acquiring lock to avoid race)
                var cachedValue = await GetAsync<T>(key).ConfigureAwait(false);
                if (cachedValue is not null)
                {
                    return cachedValue;
                }

                // Cache miss - compute the value
                var value = await factory().ConfigureAwait(false);

                // Store in cache
                await SetAsync(key, value, expiration).ConfigureAwait(false);

                return value;
            }
            finally
            {
                // Release the lock - keep the semaphore in dictionary for other waiting threads
                keyLock.Release();
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error in GetOrCreateAsync for key: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Store value in cache with optional expiration
    /// Enforces size limits and performs LRU-based eviction when necessary
    /// </summary>
    /// <typeparam name="T">Type of value to store</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to store</param>
    /// <param name="expiration">Optional expiration time span</param>
    /// <returns>Task representing the async operation</returns>
    /// <exception cref="ArgumentException">Thrown when key is null or empty</exception>
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

        var expiresAt = expiration.HasValue
            ? DateTime.UtcNow.Add(expiration.Value)
            : DateTime.MaxValue;

        var entry = new CacheEntry
        {
            Value = value,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            AccessCount = 0,
            LastAccessAt = null
        };

        // Try to add the entry
        if (!_cache.TryAdd(key, entry))
        {
            // Key already exists, update it
            _cache[key] = entry;
        }

        // Enforce size limit - remove least recently used entries if necessary
        EnforceSizeLimit();

        _logger.LogDebug("Cache set for key: {Key}, expires: {Expires}",
            key, expiresAt == DateTime.MaxValue ? "never" : expiresAt.ToString("O"));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Remove entry from cache
    /// </summary>
    /// <param name="key">Cache key to remove</param>
    /// <returns>Task representing the async operation</returns>
    public Task RemoveAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return Task.CompletedTask;
        }

        if (_cache.TryRemove(key, out _))
        {
            // Clean up the key lock if it exists
            if (_keyLocks.TryRemove(key, out var semaphore))
            {
                try
                {
                    semaphore.Dispose();
                }
                catch
                {
                    // Best effort disposal
                }
            }

            _logger.LogDebug("Cache entry removed: {Key}", key);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Check if key exists and is not expired
    /// </summary>
    /// <param name="key">Cache key to check</param>
    /// <returns>True if key exists and is not expired, false otherwise</returns>
    /// <exception cref="ArgumentException">Thrown when key is null or empty</exception>
    public Task<bool> ExistsAsync(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

        if (_cache.TryGetValue(key, out var entry) && !entry.IsExpired)
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// Clear all cache entries
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    public Task ClearAsync()
    {
        var count = _cache.Count;
        _cache.Clear();
        _keyLocks.Clear();

        _logger.LogInformation("Cache cleared. Removed {Count} entries", count);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Get all cache keys, removing expired entries in the process
    /// </summary>
    /// <returns>List of valid cache keys</returns>
    public Task<List<string>> GetKeysAsync()
    {
        // ConcurrentDictionary doesn't support direct LINQ operations in a thread-safe way
        // We need to snapshot the keys and filter expired ones
        var expiredKeys = new List<string>();
        var validKeys = new List<string>();

        foreach (var kvp in _cache)
        {
            if (kvp.Value.IsExpired)
            {
                expiredKeys.Add(kvp.Key);
            }
            else
            {
                validKeys.Add(kvp.Key);
            }
        }

        // Remove expired keys
        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }

        return Task.FromResult(validKeys);
    }

    /// <summary>
    /// Get cache statistics for monitoring
    /// </summary>
    /// <returns>Cache statistics</returns>
    public Task<CacheStats> GetStatsAsync()
    {
        // Count non-expired entries
        var validCount = _cache.Count(kvp => !kvp.Value.IsExpired);

        var stats = new CacheStats
        {
            ItemCount = validCount,
            HitCount = _hitCount,
            MissCount = _missCount,
            TotalSize = EstimateCacheSize()
        };

        return Task.FromResult(stats);
    }

    /// <summary>
    /// Estimate cache size in bytes
    /// </summary>
    /// <returns>Approximate size in bytes</returns>
    private long EstimateCacheSize()
    {
        long size = 0;
        foreach (var kvp in _cache)
        {
            size += kvp.Key.Length * sizeof(char);
            if (kvp.Value.Value is not null)
            {
                // Safely estimate size by checking if the type is blittable
                var type = kvp.Value.Value.GetType();
                if (type.IsValueType && !type.IsGenericType)
                {
                    try
                    {
                        size += System.Runtime.InteropServices.Marshal.SizeOf(kvp.Value.Value);
                    }
                    catch
                    {
                        size += 256;
                    }
                }
                else
                {
                    size += 256;
                }
            }
        }
        return size;
    }

    /// <summary>
    /// Enforce maximum cache size by removing least recently used entries
    /// </summary>
    private void EnforceSizeLimit()
    {
        // Only enforce limit if we're over the threshold
        if (_cache.Count <= MaxCacheSize)
        {
            return;
        }

        // Sort entries by last access time (oldest first) and remove excess
        var entriesToRemove = _cache
            .Where(kvp => !kvp.Value.IsExpired)
            .OrderBy(kvp => kvp.Value.LastAccessAt ?? kvp.Value.CreatedAt)
            .Take(_cache.Count - MaxCacheSize)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in entriesToRemove)
        {
            _cache.TryRemove(key, out _);
            if (_keyLocks.TryRemove(key, out var semaphore))
            {
                try
                {
                    semaphore.Dispose();
                }
                catch
                {
                    // Best effort disposal
                }
            }
        }

        _logger.LogInformation("Cache size limit enforced. Removed {Count} least recently used entries", entriesToRemove.Count);
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        // Dispose all semaphores
        foreach (var semaphore in _keyLocks.Values)
        {
            try
            {
                semaphore.Dispose();
            }
            catch
            {
                // Best effort disposal
            }
        }
        _keyLocks.Clear();
    }

    /// <summary>
    /// Internal cache entry structure
    /// </summary>
    /// <typeparam name="T">Type of cached value</typeparam>
    private sealed class CacheEntry
    {
        public object? Value { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AccessCount { get; set; }
        public DateTime? LastAccessAt { get; set; }

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }
}