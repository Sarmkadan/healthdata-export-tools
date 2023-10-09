# CacheService

The `CacheService` class provides an asynchronous interface for managing the caching lifecycle of health data records, analytics results, and parse operations within the `healthdata-export-tools` project. It abstracts the underlying storage mechanism to allow efficient retrieval, storage, and invalidation of cached entities, while also exposing statistical metrics and pattern-based clearing capabilities to maintain cache hygiene and performance.

## API

### `public CacheService`
Initializes a new instance of the `CacheService` class. This constructor typically injects necessary dependencies such as the underlying distributed cache client or database context required to perform asynchronous operations.

### `public async Task CacheHealthDataAsync`
Persists health data records into the cache.
*   **Purpose**: Stores raw or processed health data for quick subsequent retrieval.
*   **Parameters**: Accepts the specific health data payload and a unique identifier (key) to associate with the data.
*   **Return Value**: Returns a `Task` that completes when the write operation is finished.
*   **Throws**: May throw exceptions related to serialization failures or underlying connectivity issues with the cache provider.

### `public async Task<List<HealthDataRecord>?> GetCachedHealthDataAsync`
Retrieves a list of cached health data records.
*   **Purpose**: Fetches previously stored health data based on a specific query or key.
*   **Parameters**: Requires a key or filter criteria to locate the relevant records.
*   **Return Value**: Returns a `Task` containing a `List<HealthDataRecord>` if data exists, or `null` if the cache miss occurs.
*   **Throws**: May throw deserialization exceptions if the cached data format is incompatible with `HealthDataRecord`.

### `public async Task CacheAnalyticsAsync`
Stores computed analytics results in the cache.
*   **Purpose**: Caches aggregated or analytical data to avoid recomputing expensive metrics.
*   **Parameters**: Accepts the analytics payload and an associated cache key.
*   **Return Value**: Returns a `Task` representing the asynchronous write operation.
*   **Throws**: Propagates errors from the underlying storage layer if the write fails.

### `public async Task<T?> GetCachedAnalyticsAsync<T>`
Retrieves a generic typed analytics object from the cache.
*   **Purpose**: Fetches specific analytics results, allowing the caller to define the expected return type.
*   **Parameters**: Requires a cache key and implicitly uses the generic type `T` for deserialization.
*   **Return Value**: Returns a `Task` containing an instance of `T` if found, or `null` if the key does not exist or deserialization fails.
*   **Throws**: May throw invalid cast exceptions if the stored data does not match the requested type `T`.

### `public async Task CacheParseResultAsync`
Caches the result of a data parsing operation.
*   **Purpose**: Stores intermediate or final results from parsing raw health data files to prevent redundant processing.
*   **Parameters**: Accepts the parse result object and a unique identifier for the source data.
*   **Return Value**: Returns a `Task` that completes upon successful storage.
*   **Throws**: Throws if the serialization of the parse result fails or the cache is unavailable.

### `public async Task<List<HealthDataRecord>?> GetCachedParseResultAsync`
Retrieves cached results from a previous parse operation.
*   **Purpose**: Returns health data records generated during a prior parsing step.
*   **Parameters**: Requires the identifier associated with the original parse job.
*   **Return Value**: Returns a `Task` containing a `List<HealthDataRecord>` or `null` if no result is cached.
*   **Throws**: May throw exceptions if the cached content is corrupted or cannot be deserialized.

### `public async Task ClearAllAsync`
Removes all entries from the cache.
*   **Purpose**: Performs a global invalidation of the cache, useful for maintenance or critical data consistency resets.
*   **Parameters**: None.
*   **Return Value**: Returns a `Task` that completes when all keys have been purged.
*   **Throws**: May throw if the underlying cache provider rejects bulk deletion operations.

### `public async Task ClearPatternAsync`
Removes cache entries matching a specific key pattern.
*   **Purpose**: Invalidates a subset of the cache (e.g., all keys starting with "user_123_") without affecting unrelated data.
*   **Parameters**: Accepts a string pattern (often supporting wildcards) to match against cache keys.
*   **Return Value**: Returns a `Task` representing the asynchronous deletion process.
*   **Throws**: Throws if the pattern syntax is invalid or unsupported by the cache provider.

### `public async Task<CacheStats?> GetStatsAsync`
Retrieves current statistics about the cache state.
*   **Purpose**: Provides metrics such as hit/miss ratios, memory usage, or item counts.
*   **Parameters**: None.
*   **Return Value**: Returns a `Task` containing a `CacheStats` object, or `null` if statistics are unavailable.
*   **Throws**: Generally does not throw unless the monitoring endpoint of the cache provider is unreachable.

### `public async Task<bool> IsHealthDataCachedAsync`
Checks for the existence of specific health data in the cache without retrieving it.
*   **Purpose**: Determines if a cache entry exists to optimize logic flow (e.g., deciding whether to trigger a fetch or a computation).
*   **Parameters**: Requires the key of the health data to check.
*   **Return Value**: Returns a `Task` containing a `bool` (`true` if present, `false` otherwise).
*   **Throws**: Unlikely to throw unless the connection to the cache is lost.

### `public async Task RemoveAsync`
Removes a specific entry from the cache.
*   **Purpose**: Deletes a single item identified by its key.
*   **Parameters**: Accepts the specific cache key to remove.
*   **Return Value**: Returns a `Task` that completes when the deletion is confirmed.
*   **Throws**: May throw if the key format is invalid or the cache provider encounters an error during deletion.

## Usage

### Example 1: Caching and Retrieving Health Data
This example demonstrates storing health data and subsequently retrieving it, falling back to a generation method if the cache miss occurs.

```csharp
public class HealthDataProcessor
{
    private readonly CacheService _cacheService;

    public HealthDataProcessor(CacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<List<HealthDataRecord>> ProcessUserDataAsync(string userId, Func<Task<List<HealthDataRecord>>> generateDataFunc)
    {
        // Check if data exists first
        if (await _cacheService.IsHealthDataCachedAsync(userId))
        {
            var cachedData = await _cacheService.GetCachedHealthDataAsync(userId);
            if (cachedData != null)
            {
                return cachedData;
            }
        }

        // Cache miss: generate new data
        var freshData = await generateDataFunc();
        
        // Store in cache for future requests
        await _cacheService.CacheHealthDataAsync(userId, freshData);
        
        return freshData;
    }
}
```

### Example 2: Managing Analytics and Selective Clearing
This example shows caching generic analytics results and invalidating only the analytics related to a specific report type using a pattern.

```csharp
public class AnalyticsManager
{
    private readonly CacheService _cacheService;

    public AnalyticsManager(CacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<DailyReport> GetDailyReportAsync(string dateKey)
    {
        // Attempt to retrieve typed analytics
        var report = await _cacheService.GetCachedAnalyticsAsync<DailyReport>($"report:{dateKey}");
        
        if (report != null)
        {
            return report;
        }

        // Compute report logic here...
        var newReport = new DailyReport { Date = dateKey, /* ... */ };

        // Cache the result
        await _cacheService.CacheAnalyticsAsync($"report:{dateKey}", newReport);
        
        return newReport;
    }

    public async Task InvalidateReportsForMonthAsync(string monthPrefix)
    {
        // Clear all keys matching the pattern "report:2023-10-*"
        await _cacheService.ClearPatternAsync($"report:{monthPrefix}*");
    }
}
```

## Notes

*   **Null Handling**: Methods returning data (`GetCachedHealthDataAsync`, `GetCachedAnalyticsAsync<T>`, etc.) explicitly return `null` on cache misses rather than throwing exceptions. Callers must handle nullability appropriately.
*   **Thread Safety**: As all public members are asynchronous and likely rely on an underlying thread-safe cache client (e.g., Redis or IMemoryCache), `CacheService` is designed to be thread-safe for concurrent read/write operations. However, race conditions between `IsHealthDataCachedAsync` and `GetCached...Async` are possible in high-concurrency scenarios; the "Check-Then-Act" pattern should ideally be handled with atomic operations if strict consistency is required, though the provided API separates these concerns.
*   **Pattern Matching**: The behavior of `ClearPatternAsync` depends heavily on the underlying cache provider's support for key scanning or wildcard deletion. In environments where pattern scanning is expensive, this operation may have higher latency than single-key removals.
*   **Serialization**: Since the service handles generic types (`T`) and complex objects (`HealthDataRecord`), failures in serialization (e.g., due to type changes between deployments) will manifest as exceptions during retrieval or `null` returns if the deserializer fails silently.
*   **Expiration**: While not explicitly exposed in the method signatures, cached items likely have implicit expiration policies configured in the service's internal setup. Relying on `ClearAllAsync` or `RemoveAsync` for immediate consistency is recommended if time-sensitive data updates are required.
