[![Build](https://github.com/sarmkadan/healthdata-export-tools/actions/workflows/build.yml/badge.svg)](https://github.com/sarmkadan/healthdata-export-tools/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

# Health Data Export Tools

A comprehensive, production-grade .NET 10 library for parsing, analyzing, and exporting health data from Zepp, Amazfit, and Garmin wearables. Extract sleep patterns, heart rate metrics, SpO2 levels, step counts, and activity data to multiple formats including CSV, JSON, and SQLite with built-in analytics and validation.

## Table of Contents

- [Features](#features)
- [Supported Data Types](#supported-data-types)
- [Architecture](#architecture)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
- [API Reference](#api-reference)
- [Configuration](#configuration)
- [Performance](#performance)
- [Troubleshooting](#troubleshooting)
- [Testing](#testing)
- [Related Projects](#related-projects)
- [Contributing](#contributing)
- [License](#license)

## Features

- **Multi-Format Support**: Parse data from Zepp, Amazfit, and Garmin health export formats
- **Flexible Export**: Export processed data to CSV, JSON, or SQLite databases with full control
- **Analytics Engine**: Calculate health metrics, trends, insights, and generate comprehensive reports
- **Data Validation**: Comprehensive validation rules for health data integrity and consistency
- **Async Operations**: Full async/await support for performance and scalability in large datasets
- **Type-Safe**: Strongly typed domain models with automatic serialization and deserialization
- **Dependency Injection**: Built-in DI support for testability, maintainability, and extensibility
- **Caching**: In-memory caching for frequently accessed data to reduce processing overhead
- **Error Handling**: Comprehensive exception handling with detailed error messages and recovery options
- **Batch Processing**: Process multiple health data files in parallel for improved throughput
- **Background Tasks**: Schedule tasks for continuous data processing and synchronization
- **Event System**: Publish/subscribe event system for extensible architecture

## Supported Data Types

### Sleep Data
- **Tracked Metrics**: Sleep duration, quality scoring, sleep phases (REM/deep/light), awake time
- **Quality Assessment**: Automatic sleep quality classification (poor, fair, good, excellent)
- **Analysis**: Trend analysis, weekly/monthly averages, consistency tracking

### Heart Rate
- **Measurement Types**: Minimum, maximum, average, resting heart rates
- **Stress Metrics**: Stress level tracking throughout the day
- **Analysis**: Heart rate variability, zones, trends, fitness indicators

### SpO2 (Blood Oxygen Saturation)
- **Monitoring**: Continuous SpO2 percentage tracking
- **Events**: Low SpO2 event detection and logging
- **Thresholds**: Configurable low SpO2 thresholds and notifications

### Steps & Activity
- **Tracking**: Step counts, distance, calories burned
- **Goals**: Daily goal comparison and achievement tracking
- **Activity Types**: Walking, running, and other activity minutes

### Activity Summaries
- **Performance Metrics**: Complete daily activity summaries
- **Achievement Tracking**: Goal attainment and progress monitoring
- **Trend Analysis**: Multi-day and weekly activity trends

## Architecture

```
┌────────────────────────────────────────────────────────────────┐
│              Health Data Export Tools Architecture             │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│  ┌──────────────────┐         ┌──────────────────┐           │
│  │  Import Sources  │         │  Export Formats  │           │
│  │  • Zepp/Amazfit  │  ───→   │  • CSV           │           │
│  │  • Garmin        │   CLI   │  • JSON          │           │
│  │  • ZIP Archives  │         │  • SQLite        │           │
│  └──────────────────┘         └──────────────────┘           │
│           ↓                                                     │
│  ┌────────────────────────────────────────┐                  │
│  │   HealthDataParserService              │                  │
│  │   • Parse health export files          │                  │
│  │   • Deserialize device-specific format │                  │
│  │   • Handle compression & archives      │                  │
│  └────────────────────────────────────────┘                  │
│           ↓                                                     │
│  ┌────────────────────────────────────────┐                  │
│  │   ValidationService                    │                  │
│  │   • Validate data integrity            │                  │
│  │   • Check constraints & ranges         │                  │
│  │   • Transform & normalize data         │                  │
│  └────────────────────────────────────────┘                  │
│           ↓                                                     │
│  ┌────────────────────────────────────────┐                  │
│  │   AnalyticsService                     │                  │
│  │   • Calculate health metrics           │                  │
│  │   • Generate trends & insights         │                  │
│  │   • Score health status                │                  │
│  └────────────────────────────────────────┘                  │
│           ↓                                                     │
│  ┌────────────────────────────────────────┐                  │
│  │   ExportService                        │                  │
│  │   • Format for export targets          │                  │
│  │   • Serialize to CSV/JSON/SQLite       │                  │
│  │   • Batch processing & reporting       │                  │
│  └────────────────────────────────────────┘                  │
│           ↓                                                     │
│  ┌────────────────────────────────────────┐                  │
│  │   IHealthDataRepository                │                  │
│  │   • Persist to database                │                  │
│  │   • Query & retrieve data              │                  │
│  │   • Support in-memory & SQLite storage │                  │
│  └────────────────────────────────────────┘                  │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

## Installation

### Via NuGet Package

Install the latest release from NuGet:

```bash
dotnet add package healthdata-export-tools
```

Or via Package Manager Console:

```powershell
Install-Package healthdata-export-tools
```

### From Source

Clone the repository and build locally:

```bash
git clone https://github.com/Sarmkadan/healthdata-export-tools.git
cd healthdata-export-tools
dotnet build -c Release
```

### Using Docker

Build and run in a Docker container:

```bash
docker build -t healthdata-export-tools:latest .
docker run -v $(pwd)/exports:/app/exports -v $(pwd)/output:/app/output healthdata-export-tools
```

## Quick Start

### Simple Console Application

The simplest way to export health data:

```csharp
using HealthDataExportTools;
using HealthDataExportTools.Configuration;
using HealthDataExportTools.Domain.Enums;

var options = new HealthDataExportOptions 
{ 
    InputPath = "./exports/",
    OutputPath = "./output/",
    ExportFormat = ExportFormat.Json,
    ValidateData = true,
    PerformAnalysis = true
};

var parser = new HealthDataParserService();
var healthData = await parser.ParseHealthDataAsync("device_export.zip");

var exporter = new ExportService();
await exporter.ExportAsync(healthData, options);

Console.WriteLine("✓ Export completed successfully!");
```

### Using Dependency Injection

For production applications with full service integration:

```csharp
using Microsoft.Extensions.DependencyInjection;
using HealthDataExportTools.Configuration;

var services = new ServiceCollection();
services.AddHealthDataExportTools(options =>
{
    options.InputPath = "./exports/";
    options.OutputPath = "./output/";
    options.DatabasePath = "./healthdata.db";
    options.ExportFormat = ExportFormat.All;
});

var provider = services.BuildServiceProvider();
var parser = provider.GetRequiredService<HealthDataParserService>();
var exporter = provider.GetRequiredService<ExportService>();

var data = await parser.ParseHealthDataAsync("export.zip");
await exporter.ExportCompleteAsync(data, "./output/", ExportFormat.Json);
```

## Usage Examples

### Example 1: Export Sleep Data to CSV

```csharp
var parser = new HealthDataParserService();
var sleepData = await parser.ParseHealthDataAsync("amazfit_export.zip");

var exporter = new ExportService();
await exporter.ExportSleepToCsvAsync(
    sleepData.SleepRecords,
    "sleep_data.csv"
);
```

### Example 2: Analyze Heart Rate Trends

```csharp
var parser = new HealthDataParserService();
var data = await parser.ParseHealthDataAsync("garmin_export.zip");

var analytics = new AnalyticsService();
var heartRateAnalysis = analytics.AnalyzeHeartRate(data.HeartRateRecords);

Console.WriteLine($"Average HR: {heartRateAnalysis.AverageHeartRate} bpm");
Console.WriteLine($"Heart Rate Variability: {heartRateAnalysis.Variability}");
Console.WriteLine($"Resting HR Trend: {heartRateAnalysis.TrendDirection}");
```

### Example 3: Batch Process Multiple Files

```csharp
var services = new ServiceCollection();
services.AddHealthDataExportTools();
var provider = services.BuildServiceProvider();

var batchProcessor = provider.GetRequiredService<BatchProcessingService>();
var results = await batchProcessor.ProcessDirectoryAsync(
    "./exports/",
    ExportFormat.All
);

foreach (var result in results)
{
    Console.WriteLine($"Processed: {result.FileName} - Status: {result.Status}");
    if (!string.IsNullOrEmpty(result.ErrorMessage))
        Console.WriteLine($"  Error: {result.ErrorMessage}");
}
```

### Example 4: Generate Health Score Report

```csharp
var parser = new HealthDataParserService();
var healthData = await parser.ParseHealthDataAsync("export.zip");

var analytics = new AnalyticsService();
var healthScore = analytics.CalculateHealthScore(healthData);

Console.WriteLine($"Overall Health Score: {healthScore}/100");

var sleepReport = analytics.AnalyzeSleepQuality(healthData.SleepRecords);
var activityReport = analytics.AnalyzeActivityTrends(healthData.StepsRecords);

Console.WriteLine($"Sleep Quality: {sleepReport.Description}");
Console.WriteLine($"Activity Level: {activityReport.Description}");
```

### Example 5: Store Data in SQLite

```csharp
var options = new HealthDataExportOptions
{
    DatabasePath = "./healthdata.db",
    ExportFormat = ExportFormat.Json
};

var connectionManager = new SqliteConnectionManager(options.DatabasePath);
await connectionManager.InitializeDatabaseAsync();

var repository = new InMemoryHealthDataRepository();
var parser = new HealthDataParserService();
var data = await parser.ParseHealthDataAsync("export.zip");

await repository.SaveHealthDataAsync(data);
```

### Example 6: Implement Custom Export Format

```csharp
public class CustomXmlFormatter : IDataFormatter
{
    public async Task<string> FormatAsync<T>(T data) where T : class
    {
        // Custom serialization logic
        var xmlDoc = new System.Xml.XmlDocument();
        // Build XML structure
        return xmlDoc.OuterXml;
    }
}

var formatter = new CustomXmlFormatter();
var result = await formatter.FormatAsync(healthData);
File.WriteAllText("health_data.xml", result);
```

### Example 7: Filter Data by Date Range

```csharp
var parser = new HealthDataParserService();
var allData = await parser.ParseHealthDataAsync("export.zip");

var startDate = new DateTime(2024, 1, 1);
var endDate = new DateTime(2024, 3, 31);

var filteredSleep = allData.SleepRecords
    .Where(s => s.RecordDate >= startDate && s.RecordDate <= endDate)
    .ToList();

var filteredHeartRate = allData.HeartRateRecords
    .Where(h => h.RecordDate >= startDate && h.RecordDate <= endDate)
    .ToList();

var exporter = new ExportService();
await exporter.ExportSleepToCsvAsync(filteredSleep, "sleep_q1_2024.csv");
```

### Example 8: Real-time Data Validation

```csharp
var validator = new ValidationService();
var parser = new HealthDataParserService();

var data = await parser.ParseHealthDataAsync("export.zip");

var validationResults = new List<ValidationResultDto>();
foreach (var sleepRecord in data.SleepRecords)
{
    var result = validator.ValidateSleepData(sleepRecord);
    validationResults.Add(result);
}

var invalidRecords = validationResults.Where(r => !r.IsValid).ToList();
Console.WriteLine($"Valid Records: {validationResults.Count - invalidRecords.Count}");
Console.WriteLine($"Invalid Records: {invalidRecords.Count}");
```

### Example 9: Cache Health Data

```csharp
var cacheProvider = new InMemoryCacheProvider();
var cacheService = new CacheService(cacheProvider);

var parser = new HealthDataParserService();
var data = await parser.ParseHealthDataAsync("export.zip");

// Store in cache with 1-hour TTL
await cacheService.SetAsync("health_data_2024", data, TimeSpan.FromHours(1));

// Retrieve from cache
var cachedData = await cacheService.GetAsync<HealthDataExportDto>("health_data_2024");
```

### Example 10: Async Event Handling

```csharp
var eventBus = new EventBus();

// Subscribe to export completion
eventBus.Subscribe<ExportCompletedEvent>(async (evt) =>
{
    Console.WriteLine($"Export completed: {evt.OutputPath}");
    Console.WriteLine($"Records processed: {evt.RecordsProcessed}");
    Console.WriteLine($"Duration: {evt.Duration.TotalSeconds:F1}s");
});

var exporter = new ExportService();
await exporter.ExportCompleteAsync(data, "./output/", ExportFormat.Json);
```

## API Reference

### Core Classes

#### HealthDataParserService

**Purpose**: Parse and deserialize health data from various device formats.

```csharp
public class HealthDataParserService
{
    /// Parse health data from a ZIP archive
    public Task<HealthDataExportDto> ParseHealthDataAsync(string zipFilePath);
    
    /// Parse sleep data from a specific file
    public Task<IEnumerable<SleepData>> ParseSleepDataAsync(string filePath);
    
    /// Parse heart rate data from a specific file
    public Task<IEnumerable<HeartRateData>> ParseHeartRateDataAsync(string filePath);
    
    /// Parse SpO2 data from a specific file
    public Task<IEnumerable<SpO2Data>> ParseSpO2DataAsync(string filePath);
    
    /// Parse steps/activity data from a specific file
    public Task<IEnumerable<StepsData>> ParseStepsDataAsync(string filePath);
}
```

#### ExportService

**Purpose**: Export parsed health data to multiple formats.

```csharp
public class ExportService
{
    /// Export health data using specified options
    public Task ExportAsync(HealthDataExportDto data, HealthDataExportOptions options);
    
    /// Export all data in all formats
    public Task ExportCompleteAsync(HealthDataExportDto data, string outputPath, ExportFormat format);
    
    /// Export sleep data to CSV
    public Task ExportSleepToCsvAsync(IEnumerable<SleepData> data, string outputPath);
    
    /// Export heart rate data to CSV
    public Task ExportHeartRateToCsvAsync(IEnumerable<HeartRateData> data, string outputPath);
    
    /// Export all data to JSON
    public Task ExportToJsonAsync(HealthDataExportDto data, string outputPath);
}
```

#### AnalyticsService

**Purpose**: Calculate metrics and generate insights from health data.

```csharp
public class AnalyticsService
{
    /// Analyze sleep quality patterns
    public SleepQualityReport AnalyzeSleepQuality(IEnumerable<SleepData> records);
    
    /// Analyze SpO2 health status
    public SpO2HealthReport AnalyzeSpO2Health(IEnumerable<SpO2Data> records);
    
    /// Calculate overall health score
    public int CalculateHealthScore(HealthDataExportDto data);
    
    /// Analyze activity trends over time
    public ActivityTrendReport AnalyzeActivityTrends(IEnumerable<StepsData> records);
    
    /// Analyze heart rate patterns
    public HeartRateAnalysisReport AnalyzeHeartRate(IEnumerable<HeartRateData> records);
}
```

#### ValidationService

**Purpose**: Validate health data for consistency and correctness.

```csharp
public class ValidationService
{
    /// Validate sleep data record
    public ValidationResultDto ValidateSleepData(SleepData record);
    
    /// Validate heart rate data record
    public ValidationResultDto ValidateHeartRateData(HeartRateData record);
    
    /// Validate SpO2 data record
    public ValidationResultDto ValidateSpO2Data(SpO2Data record);
    
    /// Validate all data in a collection
    public Task<ValidationResultDto> ValidateAllAsync(HealthDataExportDto data);
}
```

### Domain Models

#### SleepData

```csharp
public class SleepData : HealthDataRecord
{
    public DateTime SleepStart { get; set; }
    public DateTime SleepEnd { get; set; }
    public int DurationMinutes { get; set; }
    public int DeepSleepMinutes { get; set; }
    public int LightSleepMinutes { get; set; }
    public int RemSleepMinutes { get; set; }
    public int AwakeMinutes { get; set; }
    public int Score { get; set; }
    public int AverageHeartRate { get; set; }
    public SleepQuality Quality { get; set; }
}
```

#### HeartRateData

```csharp
public class HeartRateData : HealthDataRecord
{
    public int MinimumBpm { get; set; }
    public int MaximumBpm { get; set; }
    public int AverageBpm { get; set; }
    public int RestingBpm { get; set; }
    public int MeasurementCount { get; set; }
    public int StressLevel { get; set; }
}
```

#### SpO2Data

```csharp
public class SpO2Data : HealthDataRecord
{
    public int MinimumPercentage { get; set; }
    public int MaximumPercentage { get; set; }
    public int AveragePercentage { get; set; }
    public int RestingPercentage { get; set; }
    public int MeasurementCount { get; set; }
    public int LowSpO2Events { get; set; }
}
```

#### StepsData

```csharp
public class StepsData : HealthDataRecord
{
    public int TotalSteps { get; set; }
    public double DistanceKm { get; set; }
    public int CaloriesBurned { get; set; }
    public int DailyGoal { get; set; }
    public int ActiveMinutes { get; set; }
    public int WalkingMinutes { get; set; }
    public int RunningMinutes { get; set; }
    public double GoalAchievementPercent { get; set; }
}
```

## Configuration

### HealthDataExportOptions

Control the behavior of the export process:

```csharp
var options = new HealthDataExportOptions
{
    InputPath = "./exports/",           // Path to input health export files
    OutputPath = "./output/",           // Path for export output
    DatabasePath = "./healthdata.db",   // SQLite database location
    ExportFormat = ExportFormat.All,    // Format(s): Csv, Json, Xml, All
    ValidateData = true,                // Validate all data before export
    PerformAnalysis = true,             // Generate analytics reports
    VerboseLogging = false,             // Detailed logging output
    CacheEnabled = true,                // Enable in-memory caching
    CacheDurationSeconds = 3600,        // Cache TTL in seconds
    MaxRetries = 3,                     // Retry failed operations
    TimeoutSeconds = 300                // Operation timeout
};
```

### Dependency Injection Configuration

Register services with the DI container:

```csharp
var services = new ServiceCollection();
services.AddHealthDataExportTools(options =>
{
    options.InputPath = "./exports/";
    options.OutputPath = "./output/";
    options.DatabasePath = "./healthdata.db";
});

// Additional configuration
services.Configure<HealthDataExportOptions>(configuration.GetSection("HealthDataExport"));

var provider = services.BuildServiceProvider();
```

### AppSettings.json

Configure via JSON settings:

```json
{
  "HealthDataExport": {
    "InputPath": "./exports/",
    "OutputPath": "./output/",
    "DatabasePath": "./healthdata.db",
    "ExportFormat": "All",
    "ValidateData": true,
    "PerformAnalysis": true,
    "VerboseLogging": false,
    "CacheEnabled": true,
    "CacheDurationSeconds": 3600,
    "MaxRetries": 3,
    "TimeoutSeconds": 300
  }
}
```

## Performance

Benchmarks run with [BenchmarkDotNet](https://benchmarkdotnet.org/) 0.14.0 on .NET 10, Release build, x64.
Hardware: Intel Core i7-12700K, 32 GB DDR5, Ubuntu 22.04.

### JSON Parsing (`HealthDataParserService.ParseJsonAsync`)

Parses a mixed payload — sleep, heart rate, SpO2, and steps records — from a single JSON document.

| Method | Records | Mean | Allocated |
|--------|--------:|-----:|----------:|
| `Parse40Records` | 40 | 218 µs | 28.4 KB |
| `Parse200Records` | 200 | 1,043 µs | 139.7 KB |

Optimisations applied: `FrozenDictionary` for device-type lookup (eliminates per-record `ToLower` allocation); `JsonElement.GetDateTime()` instead of `DateTime.Parse(element.GetString())` to avoid intermediate string allocation; async state machine removed from the hot path.

### CSV Formatting (`CsvFormatter`)

Formats a 30-day export for each metric type.

| Method | Records | Mean | Allocated |
|--------|--------:|-----:|----------:|
| `FormatSleepCsv` | 30 | 52 µs | 19.2 KB |
| `FormatHeartRateCsv` | 30 | 48 µs | 16.8 KB |
| `FormatStepsCsv` | 30 | 46 µs | 15.9 KB |

### Analytics Engine

Calculations over a 30-day rolling window.

| Method | Mean | Allocated |
|--------|-----:|----------:|
| `CalculateHealthScore` | 6.4 µs | 1.1 KB |
| `AnalyzeSleepQuality` | 3.9 µs | 896 B |
| `AnalyzeHeartRateTrend` | 1.2 µs | 528 B |

Analytics optimisations: `CalculateHealthScore`, `AnalyzeSleepQuality`, `AnalyzeSpO2Health`, and `AnalyzeActivityIntensity` each replaced four separate LINQ filter+aggregate passes with a single `foreach` loop, eliminating intermediate list allocations entirely.

`CsvUtility.ParseCsvLine` now uses an `ArrayPool<char>` backing buffer instead of `StringBuilder`, and `EscapeCsvField` uses a single vectorised `IndexOfAny` span scan instead of three `Contains` calls.

### Running Benchmarks

```bash
dotnet run --project benchmarks/healthdata-export-tools.Benchmarks -c Release
```

Filter to a specific class:

```bash
dotnet run --project benchmarks/healthdata-export-tools.Benchmarks -c Release -- --filter "*Analytics*"
```

## Troubleshooting

### Common Issues

#### Issue: "Unsupported file format"
**Cause**: Input file is not a recognized health export format.
**Solution**: 
- Verify the export file is from a supported device (Zepp, Amazfit, Garmin)
- Check that the file is not corrupted
- Ensure the file extension matches the format

#### Issue: "Validation errors detected"
**Cause**: Health data fails validation checks.
**Solution**:
- Set `ValidateData = false` in options to skip validation
- Review validation error messages for specific issues
- Check data ranges are within expected values

#### Issue: "Database locked"
**Cause**: SQLite database is in use by another process.
**Solution**:
- Ensure no other application is accessing the database
- Check file permissions are correct
- Restart the application

#### Issue: "Out of memory"
**Cause**: Processing very large health data files.
**Solution**:
- Process files in batches using `BatchProcessingService`
- Reduce cache size with `CacheDurationSeconds`
- Use streaming parsing for large datasets

#### Issue: "Export timeout"
**Cause**: Processing takes longer than configured timeout.
**Solution**:
- Increase `TimeoutSeconds` in configuration
- Reduce the amount of data per export
- Check system resources (CPU, memory)

### Logging

Enable verbose logging for troubleshooting:

```csharp
var options = new HealthDataExportOptions
{
    VerboseLogging = true
};
```

Check logs for detailed error messages and processing information.

### Performance Optimization

For large datasets:

1. **Enable Caching**
```csharp
options.CacheEnabled = true;
options.CacheDurationSeconds = 3600;
```

2. **Use Batch Processing**
```csharp
var batchService = new BatchProcessingService();
await batchService.ProcessDirectoryAsync("./exports/", ExportFormat.All);
```

3. **Parallel Processing**
```csharp
var records = healthData.SleepRecords
    .AsParallel()
    .Where(r => r.RecordDate > DateTime.UtcNow.AddDays(-30))
    .ToList();
```

4. **Selective Export**
Only export needed data types to reduce processing time.

## Requirements

- **.NET Runtime**: .NET 10.0 or later
- **Language**: C# 14 or later
- **Memory**: Minimum 512 MB (1 GB+ recommended for large datasets)
- **Disk Space**: 100 MB free space for database and exports
- **Operating System**: Windows, macOS, Linux

## Testing

Run the full test suite:

```bash
dotnet test
```

Run with code coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Filter to a specific test class:

```bash
dotnet test --filter "FullyQualifiedName~AnalyticsServiceTests"
```

The suite covers four areas:

| Test file | What it covers |
|-----------|---------------|
| `DomainModelTests` | Domain model construction, defaults, and invariants |
| `AnalyticsServiceTests` | Health-score calculation, sleep and heart rate analysis |
| `TrendAnomalyDetectionServiceTests` | Trend detection, anomaly flagging, threshold logic |
| `ValidationServiceTests` | Input validation rules for all metric types |

## Related Projects

- [skiasharp-chart-engine](https://github.com/sarmkadan/skiasharp-chart-engine) - High-performance chart rendering with SkiaSharp — line, bar, pie, heatmap, export to PNG/SVG

### Integration Examples

Combine **healthdata-export-tools** with **skiasharp-chart-engine** to render parsed health metrics as publication-ready charts.

**Heart rate trend line chart:**

```csharp
var parser = new HealthDataParserService();
var data = await parser.ParseHealthDataAsync("amazfit_export.zip");

var report = new AnalyticsService().AnalyzeHeartRate(data.HeartRateRecords);

var chart = new LineChart { Title = "Heart Rate Trend" };
chart.AddSeries("Avg BPM", report.DailyAverages);
await chart.ExportToPngAsync("heart_rate_trend.png");
```

**30-day step count bar chart:**

```csharp
var data = await new HealthDataParserService().ParseHealthDataAsync("garmin_export.zip");

var stepsByDay = data.StepsRecords
    .OrderBy(s => s.RecordDate)
    .Select(s => (s.RecordDate, (double)s.TotalSteps));

var chart = new BarChart { Title = "Daily Steps — Last 30 Days" };
chart.AddSeries("Steps", stepsByDay);
await chart.ExportToPngAsync("steps_30d.png");
```

## Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit changes: `git commit -am 'Add new feature'`
4. Push to branch: `git push origin feature/your-feature`
5. Submit a Pull Request

### Development Setup

```bash
git clone https://github.com/Sarmkadan/healthdata-export-tools.git
cd healthdata-export-tools
dotnet build
dotnet test
```

### Code Standards

- Follow C# naming conventions
- Add XML documentation to public methods
- Write unit tests for new features
- Ensure all tests pass before submitting PR

## License

MIT License - See [LICENSE](LICENSE) file for details

Copyright (c) 2025 Vladyslav Zaiets

---

Built by [Vladyslav Zaiets](https://sarmkadan.com)
