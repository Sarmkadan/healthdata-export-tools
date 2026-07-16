// Health Data Export Tools

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
