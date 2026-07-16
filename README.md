// existing content ...

## MetricCorrelationDto

The `MetricCorrelationDto` class represents a correlation between two metrics, providing detailed information about the relationship between them, including the correlation coefficient, strength, direction, and sample count.

### Usage Example

```csharp
using HealthDataExportTools.DTOs;

// Create a new MetricCorrelationDto instance
var correlation = new MetricCorrelationDto
{
    Pair = new CorrelationPair("HeartRate", "Steps"),
    Coefficient = 0.75,
    Strength = CorrelationStrength.Strong,
    Direction = CorrelationDirection.Direct,
    SampleCount = 100,
    Interpretation = "Strong direct relationship between HeartRate and Steps",
    AnalysisPeriodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-30)),
    AnalysisPeriodEnd = DateOnly.FromDateTime(DateTime.Now)
};

// Check if the correlation is significant
bool isSignificant = correlation.IsSignificant();
Console.WriteLine($"Is significant: {isSignificant}");

// Get the title and description of the correlation
string title = correlation.Title;
string description = correlation.Description;
Console.WriteLine($"Title: {title}");
Console.WriteLine($"Description: {description}");
```

## HealthDataExportDto

`HealthDataExportDto` is the top‑level DTO used when exporting a complete health‑data package.  
It contains a unique export identifier, the export timestamp, a summary of the data set, detailed collections for sleep, heart‑rate, SpO₂ and steps records, and metadata describing the export (version, schema, timezone, etc.).

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using HealthDataExportTools.DTOs;

var exportDto = new HealthDataExportDto
{
    // Basic export information
    ExportId = Guid.NewGuid().ToString(),
    ExportDate = DateTime.UtcNow,

    // Summary information
    Summary = new ExportSummary
    {
        TotalRecords = 4,
        DateRange = new DateRangeDto
        {
            Start = new DateTime(2023, 01, 01),
            End   = new DateTime(2023, 01, 07)
        },
        RecordCounts = new RecordCountSummary
        {
            Sleep = 2,
            HeartRate = 1,
            SpO2 = 0,
            Steps = 1,
            Activity = 0
        },
        DeviceTypes = new List<string> { "Fitbit", "AppleWatch" }
    },

    // Detailed data collections
    SleepData = new List<SleepExportDto>
    {
        new SleepExportDto
        {
            Date = new DateTime(2023, 01, 01),
            DurationMinutes = 420,
            Quality = 85,
            DeepSleepMinutes = 180,
            RemSleepMinutes = 90,
            AwakeMinutes = 150,
            DeviceType = "Fitbit",
            Notes = "Good night"
        }
    },

    HeartRateData = new List<HeartRateExportDto>
    {
        new HeartRateExportDto
        {
            Timestamp = new DateTime(2023, 01, 01, 08, 30, 00),
            HeartRate = 72,
            Zone = "Rest",
            DeviceType = "AppleWatch"
        }
    },

    SpO2Data = new List<SpO2ExportDto>(),
    StepsData = new List<StepsExportDto>
    {
        new StepsExportDto
        {
            Date = new DateTime(2023, 01, 01),
            Steps = 10500,
            Distance = 8.2,
            Calories = 420,
            DeviceType = "Fitbit"
        }
    },

    // Export metadata
    Metadata = new ExportMetadata
    {
        Version = "1.0.0",
        Schema = "HealthDataExport",
        Timezone = TimeZoneInfo.Local.Id,
        IsCompressed = false,
        EncryptionEnabled = false,
        Source = "UserDeviceSync",
        Tags = new List<string> { "2023", "January" }
    }
};

// Accessing some nested values
Console.WriteLine($"Export ID: {exportDto.ExportId}");
Console.WriteLine($"Total records: {exportDto.Summary.TotalRecords}");
Console.WriteLine($"Date range: {exportDto.Summary.DateRange.Start:d} – {exportDto.Summary.DateRange.End:d}");
Console.WriteLine($"Sleep records count: {exportDto.Summary.RecordCounts.Sleep}");
Console.WriteLine($"First sleep entry quality: {exportDto.SleepData[0].Quality}");
Console.WriteLine($"Export timezone: {exportDto.Metadata.Timezone}");
```
