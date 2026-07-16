// existing content ...


## ICacheProvider

The `ICacheProvider` interface defines the contract for cache providers in HealthData Export Tools, supporting asynchronous get, set, remove, and expiration operations. It provides a standardized way to cache data with optional expiration and includes methods for cache management and statistics.

### Usage Example

```csharp
using HealthDataExportTools.Cache;
using HealthDataExportTools.DTOs;

// Get cache statistics
var stats = await cacheProvider.GetStatsAsync();
Console.WriteLine($"Cache items: {stats.ItemCount}");
Console.WriteLine($"Total size: {stats.TotalSize} bytes");
Console.WriteLine($"Hit rate: {stats.HitRate:P2}");
Console.WriteLine($"Hits: {stats.HitCount}, Misses: {stats.MissCount}");

// Get a cache entry with metadata
var entry = await cacheProvider.GetAsync<PatientRecord>("patient_12345");
if (entry != null)
{
    Console.WriteLine($"Key: {entry.Key}");
    Console.WriteLine($"Value: {entry.Value}");
    Console.WriteLine($"Created: {entry.CreatedAt}");
    Console.WriteLine($"Expires: {entry.ExpiresAt}");
    Console.WriteLine($"Access count: {entry.AccessCount}");
    Console.WriteLine($"Last accessed: {entry.LastAccessAt}");
}

// Set a value in cache with 5 minute expiration
var patientRecord = new PatientRecord { /* populate record */ };
await cacheProvider.SetAsync("patient_12345", patientRecord, TimeSpan.FromMinutes(5));

// Check if key exists
bool exists = await cacheProvider.ExistsAsync("patient_12345");
Console.WriteLine($"Key exists: {exists}");

// Remove from cache
await cacheProvider.RemoveAsync("patient_12345");

// Clear entire cache
await cacheProvider.ClearAsync();
```
## AnomalyPoint

The `AnomalyPoint` class represents a single data point that was flagged as anomalous during Z-score analysis, providing detailed information about the anomaly including its date, value, statistical deviation, and severity classification.

### Usage Example

```csharp
using HealthDataExportTools.DTOs;

// Create a new AnomalyPoint instance
var anomaly = new AnomalyPoint
{
    Date = DateTime.UtcNow.AddHours(-2),
    Value = 145.5,
    ZScore = 3.2,
    DeviationFromMean = 22.3,
    Severity = "Severe"
};

// Accessing properties
Console.WriteLine($"Anomaly Date: {anomaly.Date}");
Console.WriteLine($"Measured Value: {anomaly.Value}");
Console.WriteLine($"Z-Score: {anomaly.ZScore}");
Console.WriteLine($"Deviation from Mean: {anomaly.DeviationFromMean}");
Console.WriteLine($"Severity: {anomaly.Severity}");
```

## ValidationResultDto

The `ValidationResultDto` class represents the result of a validation operation, providing detailed information about the validation process, including the validation ID, timestamp, and statistics about the validation outcome.

### Usage Example

```csharp
using HealthDataExportTools.DTOs;

// Create a new ValidationResultDto instance
var result = new ValidationResultDto
{
    ValidationId = Guid.NewGuid().ToString(),
    Timestamp = DateTime.UtcNow,
    IsValid = true,
    TotalRecords = 100,
    ValidRecords = 90,
    InvalidRecords = 10,
    ValidationErrors = new List<ValidationErrorDetail>
    {
        ValidationErrorDetail.Create(1, "Field1", "Error message 1"),
        ValidationErrorDetail.Create(2, "Field2", "Error message 2")
    },
    Warnings = new List<ValidationWarning>
    {
        ValidationWarning.Create(3, "Field3", "Warning message 1"),
        ValidationWarning.Create(4, "Field4", "Warning message 2")
    },
    Statistics = new ValidationStatistics
    {
        DataTypeBreakdown = new Dictionary<string, DataTypeValidationStats>
        {
            {"DataType1", new DataTypeValidationStats {TotalRecords = 50, ValidRecords = 40, InvalidRecords = 10}},
            {"DataType2", new DataTypeValidationStats {TotalRecords = 50, ValidRecords = 40, InvalidRecords = 10}}
        },
        ErrorsByType = new Dictionary<string, int>
        {
            {"ErrorType1", 5},
            {"ErrorType2", 5}
        },
        MostCommonErrors = new List<CommonError>
        {
            new CommonError {ErrorCode = "ErrorType1", Message = "Error message 1", Occurrences = 5, AffectedFields = new List<string> {"Field1", "Field2"}},
            new CommonError {ErrorCode = "ErrorType2", Message = "Error message 2", Occurrences = 5, AffectedFields = new List<string> {"Field3", "Field4"}}
        },
        AffectedDevices = new List<string> {"Device1", "Device2"},
        DateRange = new DateRangeInfo {Start = DateTime.Now.AddDays(-30), End = DateTime.Now}
    },
    DurationMs = 1000
};

// Accessing some properties
Console.WriteLine($"Validation ID: {result.ValidationId}");
Console.WriteLine($"Is valid: {result.IsValid}");
Console.WriteLine($"Total records: {result.TotalRecords}");
Console.WriteLine($"Valid records: {result.ValidRecords}");
Console.WriteLine($"Invalid records: {result.InvalidRecords}");
Console.WriteLine($"Validation errors count: {result.GetErrorCount()}");
Console.WriteLine($"Validation warnings count: {result.GetWarningCount()}");
Console.WriteLine($"Validation duration (ms): {result.DurationMs}");
```

## InMemoryCacheProvider

The `InMemoryCacheProvider` class provides a thread-safe, in-memory cache implementation with expiration support and comprehensive statistics tracking. It implements the `ICacheProvider` interface and uses `ReaderWriterLockSlim` for concurrent access, making it suitable for multi-threaded applications.

### Usage Example

```csharp
using HealthDataExportTools.Cache;
using Microsoft.Extensions.Logging;

// Create cache provider with logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<InMemoryCacheProvider>();
var cacheProvider = new InMemoryCacheProvider(logger);

// Set a value in cache with 10 minute expiration
var patientData = new { Id = 123, Name = "John Doe", Age = 45 };
await cacheProvider.SetAsync("patient_123", patientData, TimeSpan.FromMinutes(10));

// Get a value from cache
var cachedData = await cacheProvider.GetAsync<object>("patient_123");
if (cachedData != null)
{
    Console.WriteLine("Cache hit!");
}
else
{
    Console.WriteLine("Cache miss - value not found or expired");
}

// Check if key exists
bool exists = await cacheProvider.ExistsAsync("patient_123");
Console.WriteLine($"Key exists: {exists}");

// Get cache statistics
var stats = await cacheProvider.GetStatsAsync();
Console.WriteLine($"Cache items: {stats.ItemCount}");
Console.WriteLine($"Hit rate: {(stats.HitCount + stats.MissCount > 0 ? (double)stats.HitCount / (stats.HitCount + stats.MissCount) : 0):P2}");
Console.WriteLine($"Total size: {stats.TotalSize} bytes");

// Remove a specific key
await cacheProvider.RemoveAsync("patient_123");

// Clear entire cache
await cacheProvider.ClearAsync();

// Get all cache keys
var keys = await cacheProvider.GetKeysAsync();
Console.WriteLine($"Cache contains {keys.Count} keys");

// Dispose when done
cacheProvider.Dispose();
```

