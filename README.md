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
