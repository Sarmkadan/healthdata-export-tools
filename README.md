// Health Data Export Tools

## WebhookService

The `WebhookService` manages webhook registrations and event notifications, enabling external systems to receive real-time updates when specific events occur within the HealthData Export Tools. It supports registering, unregistering, and triggering webhooks with detailed tracking of invocation history, success/failure counts, and retry attempts.



### Usage Example

```csharp
using HealthDataExportTools.Integration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Create a logger and webhook service
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<WebhookService>();
var webhookService = new WebhookService(logger);

// Register a new webhook
var webhookId = webhookService.RegisterWebhook(
    url: "https://api.example.com/webhooks/healthdata",
    eventType: "PatientDataExported",
    isActive: true);
Console.WriteLine($"Webhook registered with ID: {webhookId}");

// Get registered webhooks
var registeredWebhooks = webhookService.GetRegisteredWebhooks();
Console.WriteLine($"Total registered webhooks: {registeredWebhooks.Count}");

// Activate/deactivate a webhook
var activationResult = webhookService.SetWebhookActive(webhookId, isActive: true);
Console.WriteLine($"Webhook activation status: {activationResult}");

// Trigger webhooks for a specific event
var webhookEvent = new WebhookEvent
{
    EventType = "PatientDataExported",
    Timestamp = DateTime.UtcNow,
    Payload = new Dictionary<string, object>
    {
        { "patientId", "PT-12345" },
        { "exportFormat", "CSV" },
        { "recordsExported", 1542 },
        { "exportId", Guid.NewGuid().ToString() }
    }
};

await webhookService.TriggerWebhooksAsync(webhookEvent);

// Get detailed webhook information
var webhookDetails = webhookService.GetRegisteredWebhooks()
    .FirstOrDefault(w => w.Id == webhookId);
if (webhookDetails != null)
{
    Console.WriteLine($"Webhook URL: {webhookDetails.Url}");
    Console.WriteLine($"Event Type: {webhookDetails.EventType}");
    Console.WriteLine($"Is Active: {webhookDetails.IsActive}");
    Console.WriteLine($"Created At: {webhookDetails.CreatedAt}");
    Console.WriteLine($"Last Invoked: {webhookDetails.LastInvokedAt}");
    Console.WriteLine($"Success Count: {webhookDetails.SuccessCount}");
    Console.WriteLine($"Failure Count: {webhookDetails.FailureCount}");
}

// Unregister a webhook
var unregisterResult = webhookService.UnregisterWebhook(webhookId);
Console.WriteLine($"Webhook unregistered: {unregisterResult}");
```

## RateLimiter

The `RateLimiter` class implements a token bucket rate limiting algorithm to control the frequency of operations. It tracks available tokens, refills them at a specified rate, and provides both synchronous and asynchronous methods for acquiring rate limit permits. This is particularly useful for managing API calls, database operations, or other resource-intensive tasks to prevent overwhelming systems.

### Usage Example

```csharp
using HealthDataExportTools.Interceptors;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

// Create a logger and rate limiter with default capacity of 100 tokens and refill rate of 10 tokens per second
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<RateLimiter>();
var rateLimiter = new RateLimiter(logger, defaultCapacity: 100, refillRate: 10);

// Get current rate limit status for an identifier
var status = rateLimiter.GetStatus("api-calls");
Console.WriteLine($"Current tokens: {status.CurrentTokens}");
Console.WriteLine($"Max tokens: {status.MaxTokens}");
Console.WriteLine($"Usage: {status.GetUsagePercentage():F2}%");
Console.WriteLine($"Is rate limited: {status.IsRateLimited}");

// Try to acquire a token synchronously (returns true if operation is allowed, false if rate limit exceeded)
bool acquired = rateLimiter.TryAcquire("api-calls");
if (acquired)
{
    Console.WriteLine("Token acquired successfully");
    // Perform rate-limited operation here
}
else
{
    Console.WriteLine("Rate limit exceeded - operation not allowed");
}

// Try to acquire multiple tokens
bool acquiredMultiple = rateLimiter.TryAcquire("api-calls", tokensRequired: 5);
if (acquiredMultiple)
{
    Console.WriteLine("5 tokens acquired successfully");
}

// Acquire a token asynchronously with timeout (waits until token available or timeout)
try
{
    await rateLimiter.AcquireAsync("api-calls", tokensRequired: 1, maxWait: TimeSpan.FromSeconds(5));
    Console.WriteLine("Async token acquired successfully");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Failed to acquire token: {ex.Message}");
}

// Reset rate limit for a specific identifier
rateLimiter.Reset("api-calls");
Console.WriteLine("Rate limit reset for 'api-calls'");

// Clear all rate limits
rateLimiter.ClearAll();
Console.WriteLine("All rate limits cleared");
```

## HealthDataException

The `HealthDataException` is the base exception class for all health data processing errors in the HealthData Export Tools library. It provides structured error categorization through the `ErrorCode` property and supports additional context data via the `ContextData` dictionary, enabling detailed error tracking and debugging.

This exception serves as the foundation for more specific exception types like `ParsingException`, `ValidationException`, `ExportException`, and `DataAccessException`, all of which inherit from it.

### Usage Example

```csharp
using HealthDataExportTools.Exceptions;

// Basic usage with default error code
var exception = new HealthDataException("Failed to process health data file");
Console.WriteLine(exception.ErrorCode); // Outputs: UNKNOWN_ERROR

// Usage with specific error code
var validationException = new HealthDataException(
    "Patient age is out of valid range",
    "VALIDATION_ERROR"
);
Console.WriteLine(validationException.ErrorCode); // Outputs: VALIDATION_ERROR

// Usage with context data for detailed error tracking
var contextData = new Dictionary<string, object>
{
    { "PatientId", "PT-12345" },
    { "Field", "Age" },
    { "ExpectedRange", "18-120" },
    { "ActualValue", 16 }
};

var detailedException = new HealthDataException(
    "Data validation failed",
    "VALIDATION_ERROR",
    contextData
);

// Access context data for logging or error handling
if (detailedException.ContextData != null)
{
    foreach (var kvp in detailedException.ContextData)
    {
        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
    }
}
```

## NotificationServiceTests

The `NotificationServiceTests` class contains comprehensive unit tests for the `NotificationService` class. It tests various scenarios for sending notifications with different types (Info, Warning, Error) and registering notification channels. The tests ensure that the correct log level is used for each notification type and that registered channels receive notifications.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using HealthDataExportTools.Tests;
using NSubstitute;
using Xunit;
using Microsoft.Extensions.Logging;

var mockLogger = Substitute.For<ILogger<NotificationService>>();
var notificationService = new NotificationService(mockLogger);

var notificationMessage = new NotificationMessage
{
    Subject = "Test Subject",
    Body = "Test Message",
    Type = NotificationType.Info,
    Timestamp = DateTime.UtcNow
};

await notificationService.SendNotificationAsync(notificationMessage);

// Register a channel and verify it receives the notification
var mockChannel = Substitute.For<INotificationChannel>();
notificationService.RegisterChannel(mockChannel);

await notificationService.SendNotificationAsync(notificationMessage);

await mockChannel.Received(1).SendAsync(Arg.Is<NotificationMessage>(nm => nm.Body.Contains(notificationMessage.Body)));
```

## DataComparisonServiceTests

The `DataComparisonServiceTests` class provides the unit test suite for the `DataComparisonService`, validating its ability to analyze and compare health data collections across different time periods. It covers various scenarios including activity, sleep, heart rate, and SpO2 trends, ensuring that percentage changes and narrative summaries are calculated accurately.

### Usage Example

```csharp
using HealthDataExportTools.Tests;

// Instantiate the test suite
var testSuite = new DataComparisonServiceTests();

// Execute specific comparison test cases
await testSuite.ComparePeriodsAsync_ShouldCalculatePercentageCorrectly();
await testSuite.ComparePeriodsAsync_WithSpO2Data_ShouldCalculateSpO2Change();
await testSuite.ComparePeriodsAsync_ShouldPopulateNarrativeSummary();
```

## ExportCompletedEvent

The `ExportCompletedEvent` is raised when a health data export operation completes successfully. It contains comprehensive metadata about the export including format, timing, file information, and any warnings encountered during processing. This event is useful for tracking export operations, monitoring performance, and triggering downstream processes like notifications or data validation.


### Usage Example

```csharp
using HealthDataExportTools.Events;
using HealthDataExportTools.Domain.Enums;
using System;
using System.Collections.Generic;

// Create an ExportCompletedEvent instance
var exportId = Guid.NewGuid().ToString();
var exportStartTime = DateTime.UtcNow.AddMinutes(-5);
var exportEndTime = DateTime.UtcNow;
var generatedFiles = new List<string> { "/exports/patient_data_2024.csv", "/exports/patient_data_2024.json" };
var warnings = new List<string> { "Large patient dataset detected", "Memory usage high during export" };

var exportEvent = new ExportCompletedEvent(
    exportId: exportId,
    exportFormat: ExportFormat.Csv,
    recordsExported: 1542,
    outputPath: "/exports/patient_data_2024.csv",
    outputSizeBytes: 4523876,
    exportStartTime: exportStartTime,
    exportEndTime: exportEndTime,
    wasCompressed: true,
    generatedFiles: generatedFiles,
    warnings: warnings
);

// Access export properties
Console.WriteLine($"Export completed: {exportEvent.RecordsExported} records");
Console.WriteLine($"Format: {exportEvent.ExportFormat}");
Console.WriteLine($"Duration: {exportEvent.GetExportDuration().TotalSeconds:F2} seconds");
Console.WriteLine($"Size: {exportEvent.GetHumanReadableSize()}");
Console.WriteLine($"Throughput: {exportEvent.GetThroughput():F2} records/sec");
Console.WriteLine($"Warnings: {exportEvent.Warnings.Count}");

// Check if export had issues
if (exportEvent.HasWarnings)
{
    Console.WriteLine("Warnings encountered:");
    foreach (var warning in exportEvent.Warnings)
    {
        Console.WriteLine($"  - {warning}");
    }
}

// Use the ToString() method for quick logging
Console.WriteLine(exportEvent.ToString());
```

## EventBus

The `EventBus` class implements a thread-safe, asynchronous event bus using the publish-subscribe pattern. It enables decoupled communication between components by allowing event publishers to notify multiple subscribers without direct dependencies. The implementation is designed for high-concurrency scenarios with proper synchronization using `ReaderWriterLockSlim`.


### Usage Example

```csharp
using HealthDataExportTools.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

// Create an EventBus instance
var serviceCollection = new ServiceCollection();
serviceCollection.AddLogging(configure => configure.AddConsole());
var serviceProvider = serviceCollection.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<EventBus>>();
var eventBus = new EventBus(logger);

// Define a custom event
public class PatientDataExportedEvent : EventBase
{
    public string PatientId { get; }
    public string ExportFormat { get; }
    public int RecordCount { get; }

    public PatientDataExportedEvent(string patientId, string exportFormat, int recordCount)
        : base(patientId)
    {
        PatientId = patientId;
        ExportFormat = exportFormat;
        RecordCount = recordCount;
    }
}

// Subscribe to events
var notificationService = new NotificationService(logger);
eventBus.Subscribe<PatientDataExportedEvent>(async @event => {
    await notificationService.SendNotificationAsync(new NotificationMessage
    {
        Subject = "Patient Data Exported",
        Body = $"Patient {@event.PatientId} data exported to {@event.ExportFormat} ({@event.RecordCount} records)",
        Type = NotificationType.Info,
        Timestamp = DateTime.UtcNow
    });
});

// Publish an event
var exportEvent = new PatientDataExportedEvent("PT-12345", "PDF", 42);
await eventBus.PublishAsync(exportEvent);

// Check subscriber count
var subscriberCount = eventBus.GetSubscriberCount<PatientDataExportedEvent>();
Console.WriteLine($"Subscribers: {subscriberCount}");

// Unsubscribe when no longer needed
eventBus.Unsubscribe<PatientDataExportedEvent>(async @event => {
    await notificationService.SendNotificationAsync(new NotificationMessage
    {
        Subject = "Unsubscribed from Patient Data Exported",
        Body = $"No longer receiving export notifications for patient {@event.PatientId}",
        Type = NotificationType.Info,
        Timestamp = DateTime.UtcNow
    });
});

// Clear all subscribers
eventBus.ClearAllSubscribers();
```

## CliOptions

The `CliOptions` class defines all command-line arguments and options for the HealthData Export Tools CLI application. It handles input/output paths, database connections, export formats, device specifications, date ranges, and various operational flags for controlling the export process. This class is typically used with command-line parsing libraries like `System.CommandLine` or `Microsoft.Extensions.Configuration`.



### Usage Example

```csharp
using HealthDataExportTools.Cli;
using System;
using System.IO;

// Create CliOptions with common export parameters
var options = new CliOptions
{
    InputPath = "/data/health_records/2024",
    OutputPath = "/exports/patient_data_2024",
    DatabasePath = "/databases/healthdata.db",
    Format = "CSV",
    Device = "Zepp",
    DataType = "Activity",
    StartDate = "2024-01-01",
    EndDate = "2024-12-31",
    Validate = true,
    Analyze = true,
    Compare = true,
    Verbose = true,
    Compress = true,
    Parallel = true,
    MaxParallelism = 4,
    EnableCache = true,
    CacheDurationMinutes = 60
};

// Validate required fields
if (string.IsNullOrEmpty(options.InputPath) || !Directory.Exists(options.InputPath))
{
    Console.Error.WriteLine("Error: Input path is required and must exist");
    return;
}

if (string.IsNullOrEmpty(options.OutputPath))
{
    options.OutputPath = Path.Combine(options.InputPath, "output");
    Directory.CreateDirectory(options.OutputPath);
}

// Display configuration summary
Console.WriteLine("HealthData Export Tools CLI");
Console.WriteLine($"Input: {options.InputPath}");
Console.WriteLine($"Output: {options.OutputPath}");
Console.WriteLine($"Format: {options.Format}");
Console.WriteLine($"Device: {options.Device}");
Console.WriteLine($"Data Type: {options.DataType}");
Console.WriteLine($"Date Range: {options.StartDate} to {options.EndDate}");
Console.WriteLine($"Parallel Processing: {options.Parallel} ({options.MaxParallelism} threads)");
Console.WriteLine($"Cache Enabled: {options.EnableCache} (Duration: {options.CacheDurationMinutes} minutes)");
```

## HealthDataImportedEvent

The `HealthDataImportedEvent` is raised when health data is successfully imported from a wearable device or external source. It contains comprehensive metadata about the import operation including record count, timing information, source details, device type, and the types of metrics imported. This event is useful for triggering downstream processes like data validation, caching, indexing, or analytics pipeline updates.




### Usage Example

```csharp
using HealthDataExportTools.Events;
using HealthDataExportTools.Domain.Enums;
using System;
using System.Collections.Generic;

// Create a HealthDataImportedEvent instance
var importId = Guid.NewGuid().ToString();
var importStartTime = DateTime.UtcNow.AddMinutes(-2);
var importEndTime = DateTime.UtcNow;
var metricTypes = new List<string> { "HeartRate", "Steps", "Sleep", "SpO2" };

var importEvent = new HealthDataImportedEvent(
    importId: importId,
    recordCount: 2487,
    importStartTime: importStartTime,
    importEndTime: importEndTime,
    importSource: "zepp://user/12345/data.json",
    deviceType: DeviceType.Zepp,
    importedMetricTypes: metricTypes
);

// Access import properties
Console.WriteLine($"Import completed: {importEvent.RecordCount} records");
Console.WriteLine($"Source: {importEvent.ImportSource}");
Console.WriteLine($"Device: {importEvent.DeviceType}");
Console.WriteLine($"Metrics: {string.Join(", ", importEvent.ImportedMetricTypes)}");
Console.WriteLine($"Duration: {importEvent.GetImportDuration().TotalSeconds:F2} seconds");
Console.WriteLine($"Throughput: {importEvent.GetThroughput():F2} records/sec");

// Check if import had issues (duration > 30 seconds)
if (importEvent.GetImportDuration().TotalSeconds > 30)
{
    Console.WriteLine("Warning: Import took longer than expected");
}

// Use the ToString() method for quick logging
Console.WriteLine(importEvent.ToString());
```

## SpO2Data

The `SpO2Data` class represents blood oxygen saturation measurements collected from wearable devices or health monitoring systems. It tracks daily SpO2 statistics including minimum, maximum, and average readings, along with detailed measurement history and reliability indicators. This data is crucial for assessing respiratory health and detecting potential oxygen desaturation events.


### Usage Example

```csharp
using HealthDataExportTools.Domain.Models;
using System;
using System.Collections.Generic;

// Create SpO2Data for a day
var spo2Data = new SpO2Data
{
    RecordDate = DateTime.Today,
    MinimumPercentage = 92,
    MaximumPercentage = 98,
    AveragePercentage = 95,
    RestingPercentage = 96,
    MeasurementCount = 0,
    LowSpO2Events = 0,
    LowestAlertValue = null,
    ReliabilityScore = 95
};

// Add measurements throughout the day
spo2Data.AddMeasurement(new SpO2Measurement
{
    Timestamp = DateTime.Today.AddHours(8),
    Percentage = 95,
    Confidence = 98
});

spo2Data.AddMeasurement(new SpO2Measurement
{
    Timestamp = DateTime.Today.AddHours(14),
    Percentage = 93,
    Confidence = 97
});

spo2Data.AddMeasurement(new SpO2Measurement
{
    Timestamp = DateTime.Today.AddHours(20),
    Percentage = 97,
    Confidence = 99
});

// Check if SpO2 levels are concerning
bool hasConcerningLevels = spo2Data.HasConcerningLevels();
Console.WriteLine($"Has concerning levels: {hasConcerningLevels}");

// Calculate percentage of measurements below threshold
var below95Percent = spo2Data.GetPercentageBelowThreshold(95);
Console.WriteLine($"Percentage below 95%: {below95Percent:F2}%");

// Get summary data
var summary = spo2Data.GetSummary();
foreach (var kvp in summary)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}

// Validate data
bool isValid = spo2Data.IsValid();
Console.WriteLine($"Data is valid: {isValid}");
```

## RetryHandler

The `RetryHandler` class implements resilient operation execution with configurable retry policies and exponential backoff. It supports both synchronous and asynchronous operations, allowing you to automatically retry failed operations based on customizable retry conditions. The handler is particularly useful for transient failures like network issues, timeouts, or temporary service unavailability.

### Usage Example

```csharp
using HealthDataExportTools.Integration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

// Create a logger and retry handler with default configuration
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<RetryHandler>();
var retryHandler = new RetryHandler(logger, maxRetries: 3, initialDelayMs: 100);

// Execute an async operation with retries
var result = await retryHandler.ExecuteAsync(
    "FetchPatientData",
    async () => 
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync("https://api.example.com/patients/PT-12345");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
);
Console.WriteLine($"Operation completed successfully: {result.Length} bytes");

// Execute a sync operation with retries
var fileContent = retryHandler.Execute(
    "ReadPatientFile",
    () => System.IO.File.ReadAllText("/data/patients/PT-12345.json")
);
Console.WriteLine($"File read successfully: {fileContent.Length} characters");

// Use predefined retry configurations
var aggressiveRetry = RetryHandler.CreateAggressive(logger);
var conservativeRetry = RetryHandler.CreateConservative(logger);

// Access retry handler properties
Console.WriteLine($"Max Retries: {retryHandler.MaxRetries}");
Console.WriteLine($"Initial Delay: {retryHandler.InitialDelayMs}ms");
Console.WriteLine($"Backoff Multiplier: {retryHandler.BackoffMultiplier}");

// Custom retry policy with specific exception types
var customRetry = new RetryHandler(
    logger,
    maxRetries: 5,
    initialDelayMs: 200,
    backoffMultiplier: 1.5
);
```

## MetricsCollector

The `MetricsCollector` class collects and tracks metrics for operations, providing insights into performance and usage patterns. It maintains detailed statistics including success/failure counts, execution times, throughput, and item processing rates. This is particularly useful for monitoring performance bottlenecks, tracking operation health, and analyzing system behavior over time.


### Usage Example

```csharp
using HealthDataExportTools.Interceptors;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

// Create a logger and metrics collector
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<MetricsCollector>();
var metricsCollector = new MetricsCollector(logger);

// Record successful operations
metricsCollector.RecordSuccess("DataExport", TimeSpan.FromSeconds(2.5), itemsProcessed: 1500);
metricsCollector.RecordSuccess("DataImport", TimeSpan.FromSeconds(1.8), itemsProcessed: 800);
metricsCollector.RecordSuccess("DataValidation", TimeSpan.FromSeconds(0.7), itemsProcessed: 2000);

// Record failed operations
try
{
    // Simulate a failing operation
    throw new InvalidOperationException("Database connection failed");
}
catch (Exception ex)
{
    metricsCollector.RecordFailure("DataExport", ex);
}

// Get metrics for a specific operation
var exportMetrics = metricsCollector.GetMetrics("DataExport");
if (exportMetrics != null)
{
    Console.WriteLine($"Operation: {exportMetrics.OperationName}");
    Console.WriteLine($"Success Count: {exportMetrics.SuccessCount}");
    Console.WriteLine($"Failure Count: {exportMetrics.FailureCount}");
    Console.WriteLine($"Total Items: {exportMetrics.TotalItemsProcessed}");
    Console.WriteLine($"Total Duration: {exportMetrics.TotalDurationMs}ms");
    Console.WriteLine($"Average Duration: {exportMetrics.AverageDurationMs:F2}ms");
    Console.WriteLine($"Throughput: {exportMetrics.Throughput:F2} items/sec");
    Console.WriteLine($"Min Duration: {exportMetrics.MinDurationMs}ms");
    Console.WriteLine($"Max Duration: {exportMetrics.MaxDurationMs}ms");
    Console.WriteLine($"First Execution: {exportMetrics.FirstExecutionTime}");
    Console.WriteLine($"Last Execution: {exportMetrics.LastExecutionTime}");
}

// Get all metrics
var allMetrics = metricsCollector.GetAllMetrics();
Console.WriteLine($"Total operations tracked: {allMetrics.Count}");

// Get summary statistics
var summary = metricsCollector.GetSummary();
Console.WriteLine($"Total Operations: {summary.TotalOperations}");
Console.WriteLine($"Total Successful: {summary.TotalSuccessful}");
Console.WriteLine($"Total Failed: {summary.TotalFailed}");
Console.WriteLine($"Success Rate: {summary.SuccessRate:F2}%");
Console.WriteLine($"Average Duration: {summary.AverageDuration:F2}ms");
Console.WriteLine($"Total Items Processed: {summary.TotalItemsProcessed}");

// Reset metrics for a specific operation
metricsCollector.ResetOperation("DataValidation");
Console.WriteLine("Metrics reset for 'DataValidation'");

// Reset all metrics
metricsCollector.Reset();
Console.WriteLine("All metrics have been reset");
```
