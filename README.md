// existing content ...

// IDataFormatter

## XmlFormatter

The `XmlFormatter` class provides functionality for formatting health data records into XML format. It implements the `IDataFormatter` interface and supports formatting various types of health data including general health records, sleep data, heart rate data, SpO2 data, and steps data. The formatter generates well-structured, indented XML documents with proper XML declarations and namespaces.

### Usage Example

```csharp
using HealthDataExportTools.Formatters;
using HealthDataExportTools.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Create formatter with logger (typically injected via DI)
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<XmlFormatter>();
var formatter = new XmlFormatter(logger);

// Example 1: Format a single health record
var healthRecord = new HealthDataRecord
{
    RecordDate = DateTime.UtcNow,
    DeviceId = "Fitbit-12345",
    // ... other properties
};

string singleRecordXml = await formatter.FormatAsync(healthRecord);
Console.WriteLine(singleRecordXml);

// Example 2: Format a collection of health records
var records = new List<HealthDataRecord>
{
    new HealthDataRecord { RecordDate = DateTime.UtcNow.AddHours(-1), DeviceId = "Fitbit-12345" },
    new HealthDataRecord { RecordDate = DateTime.UtcNow.AddHours(-2), DeviceId = "Fitbit-67890" }
};

string collectionXml = await formatter.FormatCollectionAsync(records);
Console.WriteLine(collectionXml);

// Example 3: Format sleep data
var sleepRecords = new List<SleepData>
{
    new SleepData
    {
        RecordDate = DateTime.UtcNow.Date,
        DurationMinutes = 480,
        DeepSleepMinutes = 90,
        RemSleepMinutes = 80,
        AwakeMinutes = 20,
        Quality = "Good",
        DeviceId = "Fitbit-Sleep"
    }
};

string sleepXml = await formatter.FormatSleepDataAsync(sleepRecords);
Console.WriteLine(sleepXml);

// Example 4: Format heart rate data
var heartRateRecords = new List<HeartRateData>
{
    new HeartRateData
    {
        RecordDate = DateTime.UtcNow,
        AverageBpm = 72,
        DeviceId = "Polar-H10"
    }
};

string heartRateXml = await formatter.FormatHeartRateDataAsync(heartRateRecords);
Console.WriteLine(heartRateXml);

// Example 5: Validate records before formatting
var validationErrors = await formatter.ValidateAsync(records);
if (validationErrors.Count > 0)
{
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}
```

The `IDataFormatter` interface defines a contract for data formatters that are responsible for transforming health data into a specific output format. A data formatter provides metadata about the format, such as its name, description, file extension, and whether it supports compression. 

### Usage Example

```csharp
using HealthDataExportTools.Formatters;

// Assume a concrete formatter class implementing IDataFormatter
public class JsonFormatter : IDataFormatter
{
    public string FileExtension => ".json";
    public string FormatName => "JSON";
    public string Description => "Formats health data in JSON.";
    public bool IsCompressible => true;
    public int MaxRecordsPerFile => 10000;

    // ... other IDataFormatter members ...
}

// Usage
IDataFormatter formatter = new JsonFormatter();
Console.WriteLine($"Formatter Name: {formatter.FormatName}");
Console.WriteLine($"File Extension: {formatter.FileExtension}");
Console.WriteLine($"Description: {formatter.Description}");
Console.WriteLine($"Is Compressible: {formatter.IsCompressible}");
Console.WriteLine($"Max Records Per File: {formatter.MaxRecordsPerFile}");
```
