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
