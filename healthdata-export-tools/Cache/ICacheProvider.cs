#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Cache;

/// <summary>
/// Defines the contract for cache providers
/// Supports get, set, remove, and expiration operations
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    /// Get value from cache by key
    /// </summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Set value in cache with optional expiration
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Remove value from cache
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Check if key exists in cache
    /// </summary>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Clear all cache entries
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// Get all cache keys
    /// </summary>
    Task<List<string>> GetKeysAsync();

    /// <summary>
    /// Get cache statistics
    /// </summary>
    Task<CacheStats> GetStatsAsync();

    /// <summary>
    /// Get value from cache, or create and store it if it doesn't exist
    /// Uses per-key locking to ensure only one thread computes the value for concurrent misses
    /// </summary>
    /// <typeparam name="T">The type of value to get or create</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">The factory function to create the value if it doesn't exist</param>
    /// <param name="expiration">Optional expiration time span</param>
    /// <returns>The cached or newly created value</returns>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
}

/// <summary>
/// Cache statistics for monitoring
/// </summary>
public sealed class CacheStats
{
    public int ItemCount { get; set; }
    public long TotalSize { get; set; }
    public int HitCount { get; set; }
    public int MissCount { get; set; }
    public double HitRate => TotalRequests > 0 ? (double)HitCount / TotalRequests : 0;
    public int TotalRequests => HitCount + MissCount;
}
