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

## AnalyticsService

The `AnalyticsService` provides comprehensive health data analysis capabilities, computing key metrics for sleep, heart rate, SpO2, steps, and overall health scores. It supports trend analysis, sleep quality assessment, SpO2 health monitoring, and activity intensity distribution to help users understand their health patterns over time.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using HealthDataExportTools.Domain.Models;

// Create analytics service
var analytics = new AnalyticsService();

// Analyze sleep metrics
var sleepRecords = new List<SleepData> { /* populate with sleep data */ };
double avgSleepHours = analytics.CalculateAverageSleepDuration(sleepRecords, days: 7);
double deepSleepPct = analytics.CalculateAverageDeepSleepPercentage(sleepRecords, days: 7);
double remSleepPct = analytics.CalculateAverageRemPercentage(sleepRecords, days: 7);

// Analyze cardiovascular metrics
var heartRateRecords = new List<HeartRateData> { /* populate with heart rate data */ };
int avgHeartRate = analytics.CalculateAverageHeartRate(heartRateRecords, days: 7);

// Analyze SpO2 metrics
var spo2Records = new List<SpO2Data> { /* populate with SpO2 data */ };
int avgSpO2 = analytics.CalculateAverageSpO2(spo2Records, days: 7);

// Analyze activity metrics
var stepsRecords = new List<StepsData> { /* populate with steps data */ };
int totalSteps = analytics.CalculateTotalSteps(stepsRecords, days: 7);

// Analyze trends
var heartRateTrend = analytics.AnalyzeTrend(heartRateRecords.Select(r => r.AverageBpm).ToList(), days: 7);
Console.WriteLine($"Health trend: {heartRateTrend.Status} ({heartRateTrend.PercentChange:F1}% change over {heartRateTrend.DaysAnalyzed} days)");

// Generate comprehensive reports
var sleepQuality = analytics.AnalyzeSleepQuality(sleepRecords, days: 30);
Console.WriteLine($"Sleep quality: {sleepQuality.Description}");
Console.WriteLine($"  - Average duration: {sleepQuality.AverageDuration:F1} minutes");
Console.WriteLine($"  - Excellent nights: {sleepQuality.ExcellentNights}/{sleepQuality.TotalNights} ({sleepQuality.ExcellenceRate:F1}%)");

var spo2Health = analytics.AnalyzeSpO2Health(spo2Records, days: 30);
Console.WriteLine($"SpO2 health: {spo2Health.Status}");
Console.WriteLine($"  - Average: {spo2Health.AverageSpO2}%");
Console.WriteLine($"  - Minimum: {spo2Health.MinimumSpO2}%");

var activityIntensity = analytics.AnalyzeActivityIntensity(
    new List<ActivityData> { /* populate with activity data */ },
    days: 7
);
Console.WriteLine($"Activity intensity: {activityIntensity.LowIntensity} low, {activityIntensity.MediumIntensity} medium, {activityIntensity.HighIntensity} high");

// Calculate overall health score
var healthScore = analytics.CalculateHealthScore(
    new HealthDataCollection {
        SleepRecords = sleepRecords,
        HeartRateRecords = heartRateRecords,
        SpO2Records = spo2Records,
        StepsRecords = stepsRecords
    },
    days: 7
);
Console.WriteLine($"Health score: {healthScore}/100");
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

## NotificationService

The `NotificationService` handles sending notifications about export and import operations, supporting multiple notification channels (email, webhooks, logs, etc.). It provides specialized methods for common operations like export completion, failures, import progress, and data quality warnings, with built-in logging support.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using HealthDataExportTools.Domain.Models;
using Microsoft.Extensions.Logging;

// Create notification service with logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<NotificationService>();
var notificationService = new NotificationService(logger);

// Register a log-based notification channel (always available)
var logChannel = new LogNotificationChannel(logger);
notificationService.RegisterChannel(logChannel);

// Register additional channels (e.g., email, webhook)
// notificationService.RegisterChannel(new EmailNotificationChannel(smtpConfig));
// notificationService.RegisterChannel(new WebhookNotificationChannel(webhookUrl));

// Notify about successful export
await notificationService.NotifyExportCompletedAsync(
    exportId: "export_2024_001",
    recordCount: 1542,
    outputPath: "/exports/patient_records_2024_001.json",
    duration: TimeSpan.FromSeconds(45.2)
);

// Notify about export failure
try
{
    // Export logic here
}
catch (Exception ex)
{
    await notificationService.NotifyExportFailedAsync(
        exportId: "export_2024_001",
        errorMessage: "Failed to connect to database",
        exception: ex
    );
}

// Notify about import progress
for (int i = 0; i < 1000; i += 100)
{
    await notificationService.NotifyImportProgressAsync(
        importId: "import_2024_002",
        processedRecords: i,
        totalRecords: 1000
    );
}

// Notify about data quality warnings
var warnings = new List<string>
{
    "Patient age exceeds reasonable range (150 years)",
    "Missing required field: date_of_birth",
    "Heart rate value out of normal range"
};
await notificationService.NotifyDataQualityWarningsAsync(
    operationId: "data_quality_check_001",
    warnings: warnings
);

// Check how many channels are registered
int channelCount = notificationService.GetChannelCount();
Console.WriteLine($"Registered notification channels: {channelCount}");
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

