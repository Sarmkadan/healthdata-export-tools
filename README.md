// existing content ...

## CsvFormatter

The `CsvFormatter` class provides functionality for formatting health data records into CSV (Comma-Separated Values) format. It implements the `IDataFormatter` interface and supports formatting various types of health data including general health records, sleep data, heart rate data, SpO2 data, and steps data.

### Usage Example

```csharp
using HealthDataExportTools.Formatters;
using HealthDataExportTools.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Create logger (in a real app this would be injected via DI)
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<CsvFormatter>();
var csvFormatter = new CsvFormatter(logger);

// Example 1: Check if formatter can handle a record type
var canFormat = csvFormatter.CanFormat(typeof(HeartRateData));
Console.WriteLine($"Can format HeartRateData: {canFormat}");

// Example 2: Format a collection of generic health records
var records = new List<HealthDataRecord>
{
    new HeartRateData
    {
        RecordDate = DateTime.UtcNow.AddMinutes(-10),
        DeviceId = "Polar-H10",
        AverageBpm = 68,
    },
    new StepsData
    {
        RecordDate = DateTime.UtcNow.AddHours(-1),
        DeviceId = "Fitbit-123",
        TotalSteps = 3500,
    }
};

string csvCollection = await csvFormatter.FormatCollectionAsync(records);
Console.WriteLine(csvCollection);

// Example 3: Format sleep data
var sleepRecords = new List<SleepData>
{
    new SleepData
    {
        RecordDate = DateTime.UtcNow.Date,
        DurationMinutes = 420,
        DeepSleepMinutes = 100,
        RemSleepMinutes = 80,
        AwakeMinutes = 30,
        Quality = "Good",
        DeviceId = "Fitbit-Sleep"
    }
};

string csvSleep = await csvFormatter.FormatSleepDataAsync(sleepRecords);
Console.WriteLine(csvSleep);

// Example 4: Validate records before formatting
var validationErrors = await csvFormatter.ValidateAsync(records);
if (validationErrors.Count > 0)
{
    foreach (var err in validationErrors)
    {
        Console.WriteLine($"Validation error: {err}");
    }
}
```

## RateLimiterExtensions

The `RateLimiterExtensions` static class provides extension methods for the `RateLimiter` class to enhance rate limiting functionality. It offers synchronous and asynchronous methods for token acquisition with timeout, usage tracking, and rate limiter state management.

### Usage Example

```csharp
using HealthData.Export.Interceptors;
using System;
using System.Threading.Tasks;

// Create a rate limiter instance
var rateLimiter = new RateLimiter(
    maxTokens: 100,
    refillRate: TimeSpan.FromSeconds(1),
    maxBurst: 50);

// Example 1: Try to acquire tokens synchronously with timeout
bool acquired = rateLimiter.TryAcquire(10, TimeSpan.FromSeconds(5));
Console.WriteLine($"Acquired tokens synchronously: {acquired}");

// Example 2: Try to acquire tokens asynchronously with timeout
bool acquiredAsync = await rateLimiter.TryAcquireAsync(15, TimeSpan.FromSeconds(2));
Console.WriteLine($"Acquired tokens asynchronously: {acquiredAsync}");

// Example 3: Get current usage percentage
var usagePercentage = rateLimiter.GetUsagePercentage();
Console.WriteLine($"Current usage: {usagePercentage:F2}%");

// Example 4: Reset rate limiter for a specific identifier
rateLimiter.Reset("user-123");

// Example 5: Reset rate limiter completely
rateLimiter.Reset(clearAll: true);
```

## MockValidationServiceValidation

The `MockValidationServiceValidation` static class provides a set of extension methods that enable comprehensive validation of a `MockValidationService` instance and the health‑data models it works with. It offers methods to retrieve validation errors, check validity, and enforce validity by throwing an exception when the service or a model is invalid.

### Usage Example

```csharp
using HealthDataExportTools.Benchmarks;
using HealthDataExportTools.Domain.Models;

// Create a mock validation service (assume it has a parameterless constructor)
var mockService = new MockValidationService();

// Validate the whole service
var serviceErrors = mockService.Validate();
if (!mockService.IsValid())
{
    // Throws an ArgumentException with detailed errors
    mockService.EnsureValid();
}

// Validate individual health data records
var sleep = new SleepData
{
    RecordDate = DateTime.UtcNow,
    SleepStart = DateTime.UtcNow.AddHours(-8),
    SleepEnd = DateTime.UtcNow,
    DurationMinutes = 480,
    DeepSleepMinutes = 120,
    LightSleepMinutes = 200,
    RemSleepMinutes = 80,
    AwakeMinutes = 80
};

var sleepErrors = sleep.Validate();

var heartRate = new HeartRateData
{
    RecordDate = DateTime.UtcNow,
    MinimumBpm = 55,
    MaximumBpm = 150,
    AverageBpm = 80,
    MeasurementCount = 30
};

var hrErrors = heartRate.Validate();
```

## CorrelationEngineOptionsExtensions

The `CorrelationEngineOptionsExtensions` static class provides extension methods for `CorrelationEngineOptions` that simplify correlation analysis configuration and validation. It offers methods to validate analysis windows, compute effective lag periods, determine parallel computation strategies, retrieve computation pairs, validate significance thresholds, and configure minimum sample requirements.


### Usage Example

```csharp
using HealthDataExportTools.Configuration;
using HealthDataExportTools.Domain.Models;
using System;
using System.Collections.Generic;

// Create correlation engine options with typical configuration
var options = new CorrelationEngineOptions
{
    AnalysisWindowDays = 30,
    MaxLagDays = 15,
    EnableParallelComputation = true,
    MaxDegreeOfParallelism = 4,
    AdditionalMetricPairs = new List<CorrelationPair>
    {
        new CorrelationPair("SleepDuration", "RestingHeartRate"),
        new CorrelationPair("Steps", "CaloriesBurned")
    },
    SignificanceThreshold = 0.1,
    MinimumSampleCount = 50,
    IncludeWeakCorrelations = true
};

// Example 1: Validate that the analysis window is within acceptable bounds
bool isWindowValid = options.IsAnalysisWindowValid();
Console.WriteLine($"Analysis window valid: {isWindowValid}");

// Example 2: Get the effective maximum lag days (capped at 50% of analysis window)
int effectiveMaxLag = options.GetEffectiveMaxLagDays();
Console.WriteLine($"Effective max lag days: {effectiveMaxLag}");

// Example 3: Determine if parallel computation should be used
bool useParallel = options.ShouldUseParallelComputation();
Console.WriteLine($"Use parallel computation: {useParallel}");

// Example 4: Get the list of correlation pairs to compute
IReadOnlyList<CorrelationPair> pairs = options.GetComputationPairs();
Console.WriteLine($"Computation pairs count: {pairs.Count}");

// Example 5: Validate significance threshold configuration
IEnumerable<string> thresholdErrors = options.ValidateSignificanceThreshold();
foreach (var error in thresholdErrors)
{
    Console.WriteLine($"Validation error: {error}");
}

// Example 6: Get minimum sample count (ensures at least 10 samples)
int minSampleCount = options.GetMinimumSampleCount();
Console.WriteLine($"Minimum sample count: {minSampleCount}");

// Example 7: Determine if weak correlations should be included
bool includeWeak = options.ShouldIncludeWeakCorrelations();
Console.WriteLine($"Include weak correlations: {includeWeak}");
```

## HealthDataExportOptionsExtensions

The `HealthDataExportOptionsExtensions` static class provides extension methods for the `HealthDataExportOptions` class that simplify common operations like cloning, path resolution, date range extraction, and configuration validation. These methods help standardize how health data export options are manipulated throughout the application.


### Usage Example

```csharp
using HealthDataExportTools.Configuration;
using System;

// Create export options
var options = new HealthDataExportOptions
{
    InputPath = "/data/health_records",
    OutputPath = "/exports",
    DatabasePath = "/data/health.db",
    ExportFormat = ExportFormat.Csv,
    ValidateData = true,
    PerformAnalysis = true,
    TrendAnalysisDays = 30,
    MaxRecordAgeDays = 90,
    CompressOutput = true,
    TargetDeviceType = "Fitbit",
    TargetDeviceId = "FB-12345",
    StartDate = new DateTime(2024, 1, 1),
    EndDate = new DateTime(2024, 12, 31),
    NotificationEmail = "admin@example.com"
};

// Example 1: Clone options to create a modified configuration
var clonedOptions = options.Clone();
clonedOptions.MaxRecordAgeDays = 60; // Modify the clone

// Example 2: Get effective input path (falls back to DatabasePath if InputPath is empty)
var effectiveInputPath = options.GetEffectiveInputPath();
Console.WriteLine($"Effective input path: {effectiveInputPath}");

// Example 3: Ensure output directory exists
var outputDir = options.EnsureOutputDirectoryExists();
Console.WriteLine($"Output directory ensured: {outputDir}");

// Example 4: Get date range as a tuple
var dateRange = options.GetDateRange();
Console.WriteLine($"Date range: {dateRange.StartDate?.ToShortDateString()} to {dateRange.EndDate?.ToShortDateString()}");

// Example 5: Get file extension based on export format
var fileExtension = options.GetFileExtension();
Console.WriteLine($"File extension: {fileExtension}");

// Example 6: Get device filter description for logging
var deviceFilter = options.GetDeviceFilterDescription();
Console.WriteLine($"Device filter: {deviceFilter}");

// Example 7: Get analysis period as TimeSpan
var analysisPeriod = options.GetAnalysisPeriod();
Console.WriteLine($"Analysis period: {analysisPeriod.TotalDays} days");

// Example 8: Get maximum record age as TimeSpan
var maxRecordAge = options.GetMaxRecordAge();
Console.WriteLine($"Max record age: {maxRecordAge.TotalDays} days");

// Example 9: Generate output file path with timestamp
var outputFile = options.GetOutputFilePath("health_export");
Console.WriteLine($"Output file path: {outputFile}");

// Example 10: Get all validation rules
var validationRules = options.GetValidationRules();
foreach (var rule in validationRules)
{
    Console.WriteLine($"Validation rule: {rule}");
}
```
