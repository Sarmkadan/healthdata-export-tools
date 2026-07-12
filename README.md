# Health Data Export Tools

![Build](https://github.com/sarmkadan/healthdata-export-tools/actions/workflows/build.yml/badge.svg)
![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)

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
- **Chart/Graph Export**: Generate interactive HTML charts to visualize health data trends
- **Weekly Summary Reports**: Automatically aggregate and report weekly health trends and statistics
- **Data Comparison Tool**: Compare different periods of health data to determine exact percentage changes
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
dotnet add package healthdata-export-tools --version 2.0.2
```

Or via Package Manager Console:

```powershell
Install-Package healthdata-export-tools -Version 2.0.2
```

### From Source

Clone the repository and build locally:

```bash
git clone https://github.com/Sarmkadan/healthdata-export-tools.git
cd healthdata-export-tools
git checkout v2.0.2
dotnet build -c Release
```

### Using Docker

Run the Health Data Export Tools in a Docker container:

#### Build and run with Docker

```bash
# Build the Docker image
docker build -t healthdata-export-tools:latest .

# Run the container with default arguments
# The application will process sample data and export to the output directory
docker run --rm \
  -v $(pwd)/exports:/data/exports:ro \
  -v $(pwd)/output:/data/output \
  -v $(pwd)/healthdata.db:/data/healthdata.db \
  healthdata-export-tools:latest
```

#### Using docker-compose

```bash
# Start the service
docker-compose up -d

# View logs
docker-compose logs -f healthdata-exporter

# Stop the service
docker-compose down
```

#### Custom configuration

You can override default paths and settings via environment variables:

```bash
docker run --rm \
  -v $(pwd)/my-data:/data/exports:ro \
  -v $(pwd)/reports:/data/output \
  -v $(pwd)/my-health.db:/data/healthdata.db \
  -e EXPORT_FORMAT=Json \
  -e VERBOSE_LOGGING=true \
  healthdata-export-tools:latest
```

#### Available environment variables

| Variable | Description | Default |
|----------|-------------|---------|
| `INPUT_PATH` | Path to input data directory | `/data/exports` |
| `OUTPUT_PATH` | Path to output directory | `/data/output` |
| `DATABASE_PATH` | Path to SQLite database file | `/data/healthdata.db` |
| `EXPORT_FORMAT` | Export format (Json, Csv, Html, All) | `All` |
| `VERBOSE_LOGGING` | Enable verbose logging | `false` |

#### Health monitoring

The container includes a health check that verifies the application is running:

```bash
# Check container health status
docker inspect --format='{{json .State.Health}}' healthdata-export-tools
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

For comprehensive, production-ready examples, see the **complete runnable examples** in the [`/examples`](examples/) directory. Each example is a self-contained application demonstrating specific features:

- **BasicUsage.cs** - Minimal setup and first call
- **AdvancedUsage.cs** - Configuration, custom options, and error handling  
- **IntegrationExample.cs** - ASP.NET Dependency Injection integration


These examples show real-world usage patterns, best practices, and complete implementations you can adapt for your projects.

### Quick Start Examples

For immediate integration, here are common usage patterns:


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

### Example 10: Generate Interactive HTML Charts

```csharp
var collection = new HealthDataCollection
{
    HeartRateRecords = heartRateData,
    StepsRecords     = stepsData,
    SleepRecords     = sleepData,
    SpO2Records      = spo2Data,
    ActivityRecords  = activityData
};

var chartService = new ChartExportService();
await chartService.ExportToHtmlChartsAsync(collection, "health_dashboard.html");

// Or with fine-grained control
var opts = new ChartExportOptions { Title = "Q1 2024 Health Report", IncludeActivityChart = true };
await chartService.ExportToHtmlChartsAsync(collection, "q1_report.html", opts);
```

### Example 11: Generate Weekly Summary Reports

```csharp
var reportService = new ReportGenerationService(logger);

var weeklyReports = await reportService.GenerateWeeklySummaryReportAsync(
    sleepData, heartRateData, stepsData, spo2Data, activityData);

foreach (var week in weeklyReports)
{
    Console.WriteLine($"Week {week.WeekIdentifier}: score={week.WeeklyHealthScore}/100, steps={week.TotalSteps:N0}");
    if (week.Changes is not null)
        Console.WriteLine($"  ▲▼ Steps: {week.Changes.StepsChangePercent:+0.0;-0.0}% | Sleep: {week.Changes.SleepDurationChangePercent:+0.0;-0.0}%");
}

await reportService.ExportWeeklySummaryToJsonAsync(weeklyReports, "weekly_summary.json");
```

### Example 12: Compare Two Health Periods

```csharp
var comparisonService = new DataComparisonService();

// Option A: compare by pre-built collections
var result = await comparisonService.ComparePeriodsAsync(thisWeekCollection, lastWeekCollection);
Console.WriteLine(result.NarrativeSummary);

// Option B: compare two date ranges within one collection
var result2 = await comparisonService.CompareByDateRangeAsync(
    allData,
    new DateTime(2024, 1,  1), new DateTime(2024, 1,  7),   // baseline
    new DateTime(2024, 1, 15), new DateTime(2024, 1, 21));   // comparison

Console.WriteLine($"Sleep change: {result2.SleepDurationChangePercentage:+0.0;-0.0}%");
Console.WriteLine($"Steps change: {result2.StepsChangePercentage:+0.0;-0.0}%");

await comparisonService.ExportToJsonAsync(result2, "period_comparison.json");
```

### Example 13: Async Event Handling

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

#### ChartExportService

**Purpose**: Export health data to interactive HTML charts rendered with Chart.js.

```csharp
public class ChartExportService
{
    /// Export all available metrics to a self-contained HTML file (default options)
    public Task ExportToHtmlChartsAsync(HealthDataCollection collection, string outputPath);

    /// Export with configurable chart selection and layout options
    public Task ExportToHtmlChartsAsync(HealthDataCollection collection, string outputPath, ChartExportOptions options);
}

public class ChartExportOptions
{
    public string Title { get; set; }              // Report title (default: "Health Data Charts")
    public bool IncludeSummaryTable { get; set; }  // Statistics table above the charts (default: true)
    public bool IncludeSpO2Chart { get; set; }     // SpO2 line chart (default: true)
    public bool IncludeActivityChart { get; set; } // Activity dual-axis chart (default: true)
    public bool IncludeSleepCompositionChart { get; set; } // Stacked sleep stages chart (default: true)
}
```

**Example**:

```csharp
var service = new ChartExportService();

// Export with all charts
await service.ExportToHtmlChartsAsync(collection, "health_report.html");

// Export with custom options
var options = new ChartExportOptions
{
    Title = "My Weekly Health Overview",
    IncludeSpO2Chart = true,
    IncludeSleepCompositionChart = true
};
await service.ExportToHtmlChartsAsync(collection, "weekly.html", options);
```

Generated HTML is self-contained and loads Chart.js from a CDN. Charts included:
- **Heart Rate**: line chart with avg / min / max BPM per day
- **Daily Steps**: bar chart
- **Sleep Duration**: bar chart (hours per night)
- **Sleep Composition**: stacked bar chart (deep / REM / light / awake minutes)
- **SpO2**: line chart with avg and min values, y-axis capped at 85–100 %
- **Activity**: dual-axis bar+line chart (calories burned and session duration)

---

#### ReportGenerationService — Weekly Summary Reports

**Purpose**: Aggregate health records into ISO-week reports with week-over-week trend deltas and a composite health score.

```csharp
public class ReportGenerationService
{
    /// Generate weekly reports from core metrics
    public Task<List<WeeklySummaryReport>> GenerateWeeklySummaryReportAsync(
        List<SleepData> sleepData,
        List<HeartRateData> heartRateData,
        List<StepsData> stepsData);

    /// Generate weekly reports including SpO2 and activity data
    public Task<List<WeeklySummaryReport>> GenerateWeeklySummaryReportAsync(
        List<SleepData> sleepData,
        List<HeartRateData> heartRateData,
        List<StepsData> stepsData,
        List<SpO2Data> spo2Data,
        List<ActivityData> activityData);

    /// Persist the list of weekly reports to a JSON file
    public Task ExportWeeklySummaryToJsonAsync(List<WeeklySummaryReport> reports, string outputPath);
}
```

**Key fields on `WeeklySummaryReport`**:

| Field | Description |
|---|---|
| `WeekIdentifier` | ISO year-week key, e.g. `"2024-03"` |
| `AverageSleepDurationMinutes` | Mean nightly sleep across the week |
| `TotalDeepSleepMinutes` / `TotalRemSleepMinutes` | Accumulated deep/REM sleep |
| `AverageHeartRate` / `MinimumHeartRate` / `MaximumHeartRate` | Heart-rate stats |
| `TotalSteps` / `TotalDistanceKm` | Step totals |
| `AverageSpO2` / `MinimumSpO2` / `TotalLowSpO2Events` | Blood oxygen stats |
| `TotalActivitySessions` / `TotalActivityMinutes` / `TotalCaloriesBurned` | Activity totals |
| `WeeklyHealthScore` | Composite score 0–100 based on sleep, HR, SpO2, and steps |
| `Changes` | `WeekOverWeekChanges` with percentage deltas vs. prior week (null for first week) |

**Example**:

```csharp
var reportService = new ReportGenerationService(logger);

var reports = await reportService.GenerateWeeklySummaryReportAsync(
    sleepRecords, heartRateRecords, stepsRecords, spo2Records, activityRecords);

foreach (var week in reports)
{
    Console.WriteLine($"{week.WeekIdentifier}: score={week.WeeklyHealthScore}, steps={week.TotalSteps}");
    if (week.Changes is not null)
        Console.WriteLine($"  Steps Δ {week.Changes.StepsChangePercent:+0.0;-0.0}%");
}

// Persist to JSON
await reportService.ExportWeeklySummaryToJsonAsync(reports, "weekly_summary.json");
```

---

#### DataComparisonService

**Purpose**: Compare two distinct periods of health data and quantify percentage changes across all metrics.

```csharp
public class DataComparisonService
{
    /// Compare two pre-built HealthDataCollection periods
    public Task<DataComparisonResult> ComparePeriodsAsync(
        HealthDataCollection period1,
        HealthDataCollection period2);

    /// Compare two date ranges extracted from a single collection
    public Task<DataComparisonResult> CompareByDateRangeAsync(
        HealthDataCollection collection,
        DateTime period1Start, DateTime period1End,
        DateTime period2Start, DateTime period2End);

    /// Persist a DataComparisonResult to a JSON file
    public Task ExportToJsonAsync(DataComparisonResult result, string outputPath);
}
```

**Key fields on `DataComparisonResult`**:

| Field | Description |
|---|---|
| `SleepDurationChangePercentage` | % change in average nightly sleep |
| `DeepSleepChangePercentage` | % change in average deep-sleep duration |
| `HeartRateChangePercentage` | % change in average heart rate |
| `StepsChangePercentage` | % change in average daily steps |
| `SpO2ChangePercentage` | % change in average SpO2 |
| `ActivityMinutesChangePercentage` | % change in total activity minutes |
| `CaloriesChangePercentage` | % change in total calories burned |
| `NarrativeSummary` | Human-readable text with directional arrows (`▲ / ▼ / →`) per metric |

**Example**:

```csharp
var comparisonService = new DataComparisonService();

// Compare by pre-built collections
var result = await comparisonService.ComparePeriodsAsync(thisWeekCollection, lastWeekCollection);
Console.WriteLine(result.NarrativeSummary);
// e.g. "Sleep duration: ▲ 8.3% (min/night) | Daily steps: ▼ 12.5% (steps)"

// Compare by date ranges within one collection
var result2 = await comparisonService.CompareByDateRangeAsync(
    allData,
    new DateTime(2024, 1, 1), new DateTime(2024, 1, 7),   // Period 1
    new DateTime(2024, 1, 15), new DateTime(2024, 1, 21)); // Period 2

// Export result
await comparisonService.ExportToJsonAsync(result2, "comparison.json");
```

---

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

### Benchmark Results

| Method | Mean | Error | StdDev | Gen0 | Allocated |
|:---|:---|:---|:---|:---|:---|
| `Parse40Records` | 70.14 us | 1.402 us | 3.438 us | 2.0752 | 17.56 KB |
| `Parse200Records` | 331.03 us | 6.523 us | 11.253 us | 10.2539 | 84.81 KB |
| `FormatSleepCsv` | 26.90 us | 0.883 us | 2.549 us | 4.3945 | 37.12 KB |
| `FormatHeartRateCsv` | 11.82 us | 0.236 us | 0.522 us | 3.1433 | 25.71 KB |
| `FormatStepsCsv` | 23.10 us | 0.460 us | 1.320 us | 4.3945 | 36.05 KB |

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

## CacheServiceTestsExtensions

The `CacheServiceTestsExtensions` class provides extension methods for `CacheServiceTests` that enable fluent, readable test assertions and setup scenarios for testing the `CacheService` class. These extensions simplify test authoring by providing methods to create test scenarios with pre-cached data, verify provider interactions, and assert cache statistics.

### Usage Examples

```csharp
using HealthDataExportTools.Tests;
using FluentAssertions;

// Create a test instance with fresh mocks
var test = CacheServiceTests.WithFreshMocks();

// Test caching health data
var healthData = new List<HealthDataRecord>
{
    new SleepData { DeviceId = "test-device", SleepStart = DateTime.UtcNow.AddHours(-8) },
    new HeartRateData { AverageBpm = 72 }
};

var healthTest = await test.WithCachedHealthDataAsync("health_data_2024", healthData);

// Test caching analytics
var analyticsData = new { SleepQualityScore = 85, ActivityLevel = "High" };
var analyticsTest = await test.WithCachedAnalyticsAsync("analytics_2024", analyticsData);

// Test clearing cache
var clearedTest = await test.WithClearedCacheAsync();

// Test with configured provider
var configuredTest = test.WithConfiguredProvider(provider =>
{
    provider.GetAsync("test_key", Arg.Any<CancellationToken>())
        .Returns(Task.FromResult<CacheItem?>(null));
});

// Verify provider method calls
await configuredTest.ShouldHaveCalledProviderMethodAsync("GetAsync", 2);

// Test with multiple cached items
var multiTest = await test.WithMultipleCachedItemsAsync("user1", 3, 2);

// Compare cache statistics
var stats = new CacheStats { ItemCount = 5, HitCount = 10, MissCount = 2 };
var expectedStats = new CacheStats { ItemCount = 5, HitCount = 10, MissCount = 2 };
stats.ShouldBeEquivalentTo(expectedStats);
```

## BatchProcessingServiceTestsExtensions

The `BatchProcessingServiceTestsExtensions` class provides extension methods for `BatchProcessingServiceTests` that simplify testing batch processing scenarios. These extensions enable the creation of batch processors with tracking capabilities, timing measurements, and progress monitoring, making it easier to test batch operations with various configurations and error conditions.

### Key Features

- **Batch Service Creation**: Create configured `BatchProcessingService` instances for testing
- **Tracking Processors**: Create batch processors that track processed items and can simulate errors
- **Timing Measurements**: Create processors that measure and track processing times
- **Progress Tracking**: Create callbacks to monitor batch processing progress
- **Assertion Helpers**: Verify that expected items were processed successfully
- **Custom Processors**: Create processors with custom processing logic

### Usage Examples

```csharp
using HealthDataExportTools.Tests;
using HealthDataExportTools.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;

// Create a batch processing service with logger
var logger = Substitute.For<ILogger<BatchProcessingService>>();
var batchService = BatchProcessingServiceTestsExtensions.CreateBatchProcessingService(null, logger);

// Create a tracking batch processor that fails specific items
var itemsToFail = new List<string> { "error-item-1", "error-item-2" };
var processedItems = new List<string>();
var trackingProcessor = BatchProcessingServiceTestsExtensions.CreateTrackingBatchProcessor(
    null, 
    itemsToFail, 
    processedItems
);

// Process a batch with some items that should fail
var batch = new List<string> { "item-1", "error-item-1", "item-2", "error-item-2", "item-3" };

try
{
    await trackingProcessor(batch);
}
catch (Exception ex)
{
    // Expected to fail on error items
    ex.Message.Should().Contain("Simulated error");
}

// Verify only successful items were tracked
processedItems.Should().HaveCount(3);
processedItems.Should().BeEquivalentTo(new[] { "item-1", "item-2", "item-3" });

// Create a timed batch processor to measure performance
var processingTimes = new List<TimeSpan>();
var timedProcessor = BatchProcessingServiceTestsExtensions.CreateTimedBatchProcessor(
    null, 
    processingTimes,
    delayMs: 10
);

// Process a batch and measure time
var testBatch = Enumerable.Range(1, 100).Select(i => $"item-{i}").ToList();
await timedProcessor(testBatch);

// Verify timing was recorded
processingTimes.Should().HaveCount(1);
processingTimes[0].TotalMilliseconds.Should().BeGreaterThan(0);

// Create a progress tracker to monitor batch processing
var progressUpdates = new List<BatchProgress>();
var progressTracker = BatchProcessingServiceTestsExtensions.CreateProgressTracker(null, progressUpdates);

// Process a batch with progress tracking
var batchWithProgress = new List<int> { 1, 2, 3, 4, 5 };
await timedProcessor(batchWithProgress);

// Verify progress was tracked
progressUpdates.Should().NotBeEmpty();

// Create a custom batch processor with specific logic
var customProcessor = BatchProcessingServiceTestsExtensions.CreateBatchProcessor<int>(
    null,
    async batch => 
    {
        // Custom processing logic
        foreach (var item in batch)
        {
            await Task.Delay(5);
        }
    }
);

// Use the processor
await customProcessor(batchWithProgress);

// Verify all expected items were processed
BatchProcessingServiceTestsExtensions.ShouldHaveProcessedAllItems(
    null,
    processedItems,
    new List<string> { "item-1", "item-2", "item-3" }
);
```

### Key Features

- **Fluent Test Setup**: Chain multiple setup operations for complex test scenarios
- **Pre-cached Data**: Create test instances with health data, analytics, or both already cached
- **Provider Verification**: Assert that specific cache provider methods were called
- **Cache Statistics**: Compare cache statistics using FluentAssertions
- **Multiple Items**: Create scenarios with multiple cached items matching patterns

### Usage Examples

```csharp
using HealthDataExportTools.Tests;
using FluentAssertions;

// Create a test instance with fresh mocks
var test = CacheServiceTests.WithFreshMocks();

// Test caching health data
var healthData = new List<HealthDataRecord> 
{
    new SleepData { DeviceId = "test-device", SleepStart = DateTime.UtcNow.AddHours(-8) },
    new HeartRateData { AverageBpm = 72 }
};

var healthTest = await test.WithCachedHealthDataAsync("health_data_2024", healthData);

// Test caching analytics
var analyticsData = new { SleepQualityScore = 85, ActivityLevel = "High" };
var analyticsTest = await test.WithCachedAnalyticsAsync("analytics_2024", analyticsData);

// Test clearing cache
var clearedTest = await test.WithClearedCacheAsync();

// Test with configured provider
var configuredTest = test.WithConfiguredProvider(provider => 
{
    provider.GetAsync("test_key", Arg.Any<CancellationToken>()) 
        .Returns(Task.FromResult<CacheItem?>(null));
});

// Verify provider method calls
await configuredTest.ShouldHaveCalledProviderMethodAsync("GetAsync", 2);

// Test with multiple cached items
var multiTest = await test.WithMultipleCachedItemsAsync("user1", 3, 2);

// Compare cache statistics
var stats = new CacheStats { ItemCount = 5, HitCount = 10, MissCount = 2 };
var expectedStats = new CacheStats { ItemCount = 5, HitCount = 10, MissCount = 2 };
stats.ShouldBeEquivalentTo(expectedStats);
```

## DataTransformationUtilityTestsExtensions

The `DataTransformationUtilityTestsExtensions` class provides extension methods for `DataTransformationUtilityTests` that facilitate testing data transformation utilities. These extensions simplify test authoring by providing methods to create test data records (sleep, heart rate, steps), verify data transformations, and validate batch processing scenarios.

### Key Features

- **Test Data Creation**: Generate realistic test instances of sleep, heart rate, and steps data records
- **Data Validation**: Assert that transformed data matches expected values and ranges
- **Batch Testing**: Create collections of test records for batch processing scenarios
- **Range Validation**: Verify records fall within expected date ranges
- **Statistical Testing**: Generate test data for statistical analysis and normalization tests

### Usage Examples

```csharp
using HealthDataExportTools.Tests;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Domain.Enums;
using FluentAssertions;

// Create a test instance
var test = new DataTransformationUtilityTests();

// Example 1: Create individual test records
var sleepRecord = test.CreateTestSleepData(
    DateTime.Today.AddDays(-1),
    480, // 8 hours
    SleepQuality.Good,
    120, // 2 hours deep sleep
    90   // 1.5 hours REM sleep
);

var heartRateRecord = test.CreateTestHeartRateData(
    DateTime.Today,
    72,  // average BPM
    55,  // minimum BPM
    110  // maximum BPM
);

var stepsRecord = test.CreateTestStepsData(
    DateTime.Today,
    12500, // 12,500 steps
    8.2,    // 8.2 km distance
    650     // 650 calories burned
);

var healthRecord = test.CreateTestHealthDataRecord(DateTime.Today.AddDays(-2));

// Example 2: Create batch of test records
var sleepRecords = test.CreateTestSleepDataBatch(7, i => 
    test.CreateTestSleepData(
        DateTime.Today.AddDays(-i),
        420 + (i * 30), // increasing duration
        i % 2 == 0 ? SleepQuality.Good : SleepQuality.Fair,
        90 + (i * 5),
        60 + (i * 3)
    )
);

// Example 3: Validate transformed data
var aggregatedSleep = new Dictionary<DateTime, AggregatedSleepData>
{
    [DateTime.Today.AddDays(-1)] = new AggregatedSleepData
    {
        TotalDurationMinutes = 480,
        AverageDurationMinutes = 480,
        AverageQuality = SleepQuality.Good,
        TotalDeepSleepMinutes = 120,
        TotalRemoSleepMinutes = 90,
        Count = 1
    }
};

var expectedSleep = new Dictionary<DateTime, AggregatedSleepData>
{
    [DateTime.Today.AddDays(-1)] = new AggregatedSleepData
    {
        TotalDurationMinutes = 480,
        AverageDurationMinutes = 480,
        AverageQuality = SleepQuality.Good,
        TotalDeepSleepMinutes = 120,
        TotalRemoSleepMinutes = 90,
        Count = 1
    }
};

aggregatedSleep.ShouldBeEquivalentTo(expectedSleep);

// Example 4: Validate date ranges
var recordsInRange = new List<HealthDataRecord>
{
    new StepsData { RecordDate = DateTime.Today.AddDays(-3) },
    new StepsData { RecordDate = DateTime.Today.AddDays(-2) },
    new StepsData { RecordDate = DateTime.Today.AddDays(-1) }
};

recordsInRange.ShouldContainRecordsInRange(
    DateTime.Today.AddDays(-4),
    DateTime.Today.AddDays(-1)
);

// Example 5: Create test data for statistical analysis
var doubleValues = test.CreateTestDoubleValues(100, i => 
    Math.Round(Random.Shared.NextDouble() * 100, 2)
);

doubleValues.ShouldBeNormalized();

// Example 6: Batch processing with heart rate data
var heartRateRecords = test.CreateTestHeartRateDataBatch(30, i => 
    test.CreateTestHeartRateData(
        DateTime.Today.AddDays(-i),
        60 + (i % 20),
        45 + (i % 15),
        90 + (i % 30)
    )
);
```

### Available Extension Methods

| Method | Description |
|--------|-------------|
| `CreateTestSleepData` | Creates a test sleep record with specified parameters |
| `CreateTestHeartRateData` | Creates a test heart rate record with specified parameters |
| `CreateTestStepsData` | Creates a test steps record with specified parameters |
| `CreateTestHealthDataRecord` | Creates a basic test health data record |
| `ShouldBeEquivalentTo` | Asserts that two dictionaries of aggregated data are equivalent |
| `ShouldContainRecordsInRange` | Asserts that records fall within a specific date range |
| `CreateTestSleepDataBatch` | Creates a batch of test sleep records |
| `CreateTestHeartRateDataBatch` | Creates a batch of test heart rate records |
| `CreateTestStepsDataBatch` | Creates a batch of test steps records |
| `CreateTestHealthDataBatch` | Creates a batch of test health data records |
| `CreateTestDoubleValues` | Creates a list of test double values for statistical testing |
| `ShouldBeNormalized` | Asserts that values are normalized to 0-100 range |

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
