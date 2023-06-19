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
}

/// <summary>
/// Cache statistics for monitoring
/// </summary>
public class CacheStats
{
    public int ItemCount { get; set; }
    public long TotalSize { get; set; }
    public int HitCount { get; set; }
    public int MissCount { get; set; }
    public double HitRate => TotalRequests > 0 ? (double)HitCount / TotalRequests : 0;
    public int TotalRequests => HitCount + MissCount;
}

/// <summary>
/// Cache entry with metadata
/// </summary>
public class CacheEntry<T>
{
    public string Key { get; set; } = string.Empty;
    public T? Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int AccessCount { get; set; }
    public DateTime? LastAccessAt { get; set; }

    public bool IsExpired => ExpiresAt != null && DateTime.UtcNow > ExpiresAt;
}
