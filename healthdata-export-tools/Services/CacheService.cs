// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools.Cache;

namespace HealthDataExportTools.Services;

/// <summary>
/// Service for managing cache operations with configurable TTL
/// Provides high-level caching interface for health data and analytics
/// </summary>
public class CacheService
{
    private readonly ICacheProvider _cacheProvider;
    private readonly ILogger<CacheService> _logger;
    private readonly TimeSpan _defaultTtl;

    // Cache key prefixes for different data types
    private const string HealthDataKeyPrefix = "health_data_";
    private const string AnalyticsKeyPrefix = "analytics_";
    private const string ParseResultKeyPrefix = "parse_result_";

    public CacheService(
        ICacheProvider cacheProvider,
        ILogger<CacheService> logger,
        TimeSpan? defaultTtl = null)
    {
        _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _defaultTtl = defaultTtl ?? TimeSpan.FromHours(1);
    }

    /// <summary>
    /// Cache health data records with default TTL
    /// </summary>
    public async Task CacheHealthDataAsync(string key, List<HealthDataRecord> records)
    {
        try
        {
            var cacheKey = $"{HealthDataKeyPrefix}{key}";
            await _cacheProvider.SetAsync(cacheKey, records, _defaultTtl);

            _logger.LogInformation("Cached {Count} health records with key: {Key}", records.Count, cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache health data");
        }
    }

    /// <summary>
    /// Retrieve cached health data records
    /// </summary>
    public async Task<List<HealthDataRecord>?> GetCachedHealthDataAsync(string key)
    {
        try
        {
            var cacheKey = $"{HealthDataKeyPrefix}{key}";
            var cached = await _cacheProvider.GetAsync<List<HealthDataRecord>>(cacheKey);

            if (cached != null)
            {
                _logger.LogInformation("Retrieved cached health data: {Key}", cacheKey);
            }

            return cached;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve cached health data");
            return null;
        }
    }

    /// <summary>
    /// Cache analytics results
    /// </summary>
    public async Task CacheAnalyticsAsync(string key, object analyticsData)
    {
        try
        {
            var cacheKey = $"{AnalyticsKeyPrefix}{key}";
            await _cacheProvider.SetAsync(cacheKey, analyticsData, _defaultTtl);

            _logger.LogInformation("Cached analytics data with key: {Key}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache analytics data");
        }
    }

    /// <summary>
    /// Retrieve cached analytics
    /// </summary>
    public async Task<T?> GetCachedAnalyticsAsync<T>(string key) where T : class
    {
        try
        {
            var cacheKey = $"{AnalyticsKeyPrefix}{key}";
            var cached = await _cacheProvider.GetAsync<T>(cacheKey);

            if (cached != null)
            {
                _logger.LogInformation("Retrieved cached analytics: {Key}", cacheKey);
            }

            return cached;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve cached analytics");
            return null;
        }
    }

    /// <summary>
    /// Cache file parse results
    /// </summary>
    public async Task CacheParseResultAsync(string filePath, List<HealthDataRecord> records)
    {
        try
        {
            var fileHash = Path.GetFileName(filePath);
            var cacheKey = $"{ParseResultKeyPrefix}{fileHash}";
            await _cacheProvider.SetAsync(cacheKey, records, _defaultTtl);

            _logger.LogInformation("Cached parse result for file: {File}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache parse result");
        }
    }

    /// <summary>
    /// Retrieve cached parse result
    /// </summary>
    public async Task<List<HealthDataRecord>?> GetCachedParseResultAsync(string filePath)
    {
        try
        {
            var fileHash = Path.GetFileName(filePath);
            var cacheKey = $"{ParseResultKeyPrefix}{fileHash}";
            return await _cacheProvider.GetAsync<List<HealthDataRecord>>(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve cached parse result");
            return null;
        }
    }

    /// <summary>
    /// Clear all cache entries
    /// </summary>
    public async Task ClearAllAsync()
    {
        try
        {
            await _cacheProvider.ClearAsync();
            _logger.LogInformation("Cache cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear cache");
        }
    }

    /// <summary>
    /// Clear cache entries matching a pattern
    /// </summary>
    public async Task ClearPatternAsync(string pattern)
    {
        try
        {
            var keys = await _cacheProvider.GetKeysAsync();
            var matchingKeys = keys.Where(k => k.Contains(pattern)).ToList();

            foreach (var key in matchingKeys)
            {
                await _cacheProvider.RemoveAsync(key);
            }

            _logger.LogInformation("Cleared {Count} cache entries matching pattern: {Pattern}",
                matchingKeys.Count, pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear cache pattern");
        }
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    public async Task<CacheStats?> GetStatsAsync()
    {
        try
        {
            return await _cacheProvider.GetStatsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cache statistics");
            return null;
        }
    }

    /// <summary>
    /// Check if a key is cached
    /// </summary>
    public async Task<bool> IsHealthDataCachedAsync(string key)
    {
        try
        {
            var cacheKey = $"{HealthDataKeyPrefix}{key}";
            return await _cacheProvider.ExistsAsync(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check cache existence");
            return false;
        }
    }

    /// <summary>
    /// Remove specific cache entry
    /// </summary>
    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cacheProvider.RemoveAsync(key);
            _logger.LogInformation("Removed cache entry: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove cache entry");
        }
    }
}
