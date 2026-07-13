// Health Data Export Tools

...

## SleepDataExtensions

The `SleepDataExtensions` class provides utility methods for analyzing sleep data, calculating sleep metrics, and formatting sleep-related information. It includes methods to determine sleep efficiency, awake percentage, restorative sleep status, sleep debt, and more, enabling comprehensive sleep analysis from raw sleep data.

### Usage Example

```csharp
using HealthDataExportTools.Domain.Models;

var sleepData = new SleepData
{
    TotalDuration = 360, // in minutes
    LightSleepDuration = 90,
    DeepSleepDuration = 60,
    RemSleepDuration = 30,
    Awakenings = 2
};

double efficiency = sleepData.GetSleepEfficiency() ?? 0.0;
string formattedDuration = sleepData.GetFormattedDuration();
bool isRestorative = sleepData.IsRestorativeSleep();

Console.WriteLine($"Sleep Efficiency: {efficiency:P}");
Console.WriteLine($"Total Sleep Duration: {formattedDuration}");
Console.WriteLine($"Restorative Sleep: {isRestorative}");
```

## WebhookServiceExtensions

...

## BackgroundTaskSchedulerExtensions

The `BackgroundTaskSchedulerExtensions` class provides utility methods for managing background tasks, including scheduling one-time and recurring tasks, canceling tasks by name, and retrieving lists of pending, running, and completed tasks. It enables efficient task orchestration and monitoring within the health data export tools.

### Usage Example

```csharp
using HealthDataExportTools.Tasks;

// Schedule a one-time task
string taskId1 = BackgroundTaskScheduler.ScheduleOnce("CleanCacheTask", () => Console.WriteLine("Cache cleaned"), TimeSpan.FromHours(1));

// Schedule a recurring task
string taskId2 = BackgroundTaskScheduler.ScheduleRecurring("DataBackupTask", () => Console.WriteLine("Data backed up"), "0 0 * * *"); // Daily at midnight

// Check if a task exists
bool hasTask = BackgroundTaskScheduler.TaskExists("DataBackupTask");

// Get pending tasks
List<ScheduledTask> pendingTasks = BackgroundTaskScheduler.GetPendingTasks();

// Cancel all tasks with a specific name
int canceledCount = BackgroundTaskScheduler.CancelTasksByName("OldDataCleanupTask");
```

## ValidationResultDtoExtensions

The `ValidationResultDtoExtensions` class offers a set of extension methods for `ValidationResultDto` objects, making it easy to retrieve validation statistics, error details, and summary information. These helpers simplify reporting and decision‑making based on the outcome of validation processes.

### Usage Example

```csharp
using HealthDataExportTools.DTOs;

// Assume `validationService` returns a ValidationResultDto after validating some input.
var validationResult = validationService.Validate(someInputDto);

// Basic rates
double invalidRate = validationResult.GetInvalidRate();
double validRate = validationResult.GetValidRate();

// Error analysis
bool hasCritical = validationResult.HasCriticalErrors();
var mostSevereError = validationResult.GetMostSevereError();
int specificErrorCount = validationResult.GetErrorCountByCode(42);

// Summary and warnings
string summary = validationResult.GetSummary();
bool hasWarnings = validationResult.HasWarnings();
var firstWarning = validationResult.GetFirstWarning();

// Ratio
double validToInvalidRatio = validationResult.GetValidToInvalidRatio();

Console.WriteLine($"Invalid Rate: {invalidRate:P}");
Console.WriteLine($"Valid Rate: {validRate:P}");
Console.WriteLine($"Critical Errors: {hasCritical}");
Console.WriteLine($"Most Severe Error: {mostSevereError?.Message ?? "None"}");
Console.WriteLine($"Errors with code 42: {specificErrorCount}");
Console.WriteLine($"Summary: {summary}");
Console.WriteLine($"Has Warnings: {hasWarnings}");
Console.WriteLine($"First Warning: {firstWarning?.Message ?? "None"}");
Console.WriteLine($"Valid/Invalid Ratio: {validToInvalidRatio:F2}");
```
