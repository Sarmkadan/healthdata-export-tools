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

## ErrorHandlingMiddleware

The `ErrorHandlingMiddleware` class provides centralized error handling and exception transformation in the HTTP request pipeline. It implements the `IMiddleware` interface and catches exceptions, converting them into structured error responses with consistent error IDs, status codes, and diagnostic information.

### Usage Example

```csharp
using HealthDataExportTools.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

// In your Program.cs or Startup.cs:
// builder.Services.AddTransient<ErrorHandlingMiddleware>();
// app.UseMiddleware<ErrorHandlingMiddleware>();

// Example usage in a controller or endpoint:
public class PatientController : ControllerBase
{
    private readonly ErrorHandlingMiddleware _errorHandler;
    private readonly ILogger<PatientController> _logger;
    
    public PatientController(ErrorHandlingMiddleware errorHandler, ILogger<PatientController> logger)
    {
        _errorHandler = errorHandler;
        _logger = logger;
    }
    
    [HttpGet("patients/{id}")]
    public async Task<IActionResult> GetPatient(string id)
    {
        var context = new MiddlewareContext
        {
            RequestId = Guid.NewGuid().ToString(),
            // Other context setup
        };
        
        // Process request through error handling middleware
        await _errorHandler.ProcessAsync(context, async ctx =>
        {
            // Your business logic here
            var patient = await _patientService.GetPatientAsync(id);
            if (patient == null)
            {
                throw new HealthDataException("Patient not found");
            }
            
            return Results.Ok(patient);
        });
        
        if (!context.ContinueProcessing)
        {
            // The error was handled by middleware
            var errorResponse = context.Result as ErrorResponse;
            return Results.Problem(
                statusCode: errorResponse?.StatusCode ?? 500,
                title: errorResponse?.Message ?? "An error occurred",
                detail: errorResponse?.Details,
                extensions: new Dictionary<string, object?>
                {
                    ["errorId"] = errorResponse?.ErrorId,
                    ["requestId"] = errorResponse?.RequestId,
                    ["timestamp"] = errorResponse?.Timestamp
                }
            );
        }
        
        return Results.Ok(context.Result);
    }
}

// The ErrorResponse class contains the following properties:
// - ErrorId: Unique identifier for the error instance
// - RequestId: Correlation ID for tracing the request
// - StatusCode: HTTP status code (400, 404, 409, 500, etc.)
// - Message: Human-readable error message
// - Details: Detailed error information
// - ExceptionType: Type of the original exception (optional)
// - Timestamp: When the error occurred
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

## IMiddleware

The `IMiddleware` interface defines the contract for middleware components in the request processing pipeline. It provides properties for tracking request context, metadata, and processing state, allowing middleware to handle requests and responses consistently.

### Usage Example

```csharp
using HealthDataExportTools.Middleware;

// Implement a custom middleware
public class CustomMiddleware : IMiddleware
{
    public string RequestId { get; set; }
    public DateTime StartTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public object? Data { get; set; }
    public Exception? Exception { get; set; }
    public bool ContinueProcessing { get; set; } = true;
    public object? Result { get; set; }
}

// Usage in a request pipeline
var middleware = new CustomMiddleware
{
    RequestId = Guid.NewGuid().ToString(),
    StartTime = DateTime.UtcNow,
    Metadata = new Dictionary<string, object>
    {
        {"UserId", "user_123"},
        {"RequestType", "export"}
    }
};

// Process request through middleware
Console.WriteLine($"Processing request: {middleware.RequestId}");
Console.WriteLine($"Started at: {middleware.StartTime}");

// Set result or exception
middleware.Result = new { Status = "Completed", Data = new { Records = 100 } };
// OR
middleware.Exception = new InvalidOperationException("Invalid data format");
middleware.ContinueProcessing = false;
```

