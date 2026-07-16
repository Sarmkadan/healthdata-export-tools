// existing content ...


## ICacheProvider

The `ICacheProvider` interface defines the contract for cache providers in HealthData Export Tools, supporting asynchronous get, set, remove, and expiration operations. It provides a standardized way to cache data with optional expiration and includes methods for cache management and statistics.

### Usage Example

```csharp
using HealthDataExportTools.Cache;
using HealthDataExportTools.DTOs;

// Get cache statistics
var stats = await cacheProvider.GetStatsAsync();
Console.WriteLine($"Cache items: {stats.ItemCount}");
Console.WriteLine($"Total size: {stats.TotalSize} bytes");
Console.WriteLine($"Hit rate: {stats.HitRate:P2}");
Console.WriteLine($"Hits: {stats.HitCount}, Misses: {stats.MissCount}");

// Get a cache entry with metadata
var entry = await cacheProvider.GetAsync<PatientRecord>("patient_12345");
if (entry != null)
{
    Console.WriteLine($"Key: {entry.Key}");
    Console.WriteLine($"Value: {entry.Value}");
    Console.WriteLine($"Created: {entry.CreatedAt}");
    Console.WriteLine($"Expires: {entry.ExpiresAt}");
    Console.WriteLine($"Access count: {entry.AccessCount}");
    Console.WriteLine($"Last accessed: {entry.LastAccessAt}");
}

// Set a value in cache with 5 minute expiration
var patientRecord = new PatientRecord { /* populate record */ };
await cacheProvider.SetAsync("patient_12345", patientRecord, TimeSpan.FromMinutes(5));

// Check if key exists
bool exists = await cacheProvider.ExistsAsync("patient_12345");
Console.WriteLine($"Key exists: {exists}");

// Remove from cache
await cacheProvider.RemoveAsync("patient_12345");

// Clear entire cache
await cacheProvider.ClearAsync();
```

## AnalyticsService

The `AnalyticsService` provides comprehensive health data analysis capabilities, computing key metrics for sleep, heart rate, SpO2, steps, and overall health scores. It supports trend analysis, sleep quality assessment, SpO2 health monitoring, and activity intensity distribution to help users understand their health patterns over time.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using HealthDataExportTools.Domain.Models;

// Create analytics service
var analytics = new AnalyticsService();

// Analyze sleep metrics
var sleepRecords = new List<SleepData> { /* populate with sleep data */ };
double avgSleepHours = analytics.CalculateAverageSleepDuration(sleepRecords, days: 7);
double deepSleepPct = analytics.CalculateAverageDeepSleepPercentage(sleepRecords, days: 7);
double remSleepPct = analytics.CalculateAverageRemPercentage(sleepRecords, days: 7);

// Analyze cardiovascular metrics
var heartRateRecords = new List<HeartRateData> { /* populate with heart rate data */ };
int avgHeartRate = analytics.CalculateAverageHeartRate(heartRateRecords, days: 7);

// Analyze SpO2 metrics
var spo2Records = new List<SpO2Data> { /* populate with SpO2 data */ };
int avgSpO2 = analytics.CalculateAverageSpO2(spo2Records, days: 7);

// Analyze activity metrics
var stepsRecords = new List<StepsData> { /* populate with steps data */ };
int totalSteps = analytics.CalculateTotalSteps(stepsRecords, days: 7);

// Analyze trends
var heartRateTrend = analytics.AnalyzeTrend(heartRateRecords.Select(r => r.AverageBpm).ToList(), days: 7);
Console.WriteLine($"Health trend: {heartRateTrend.Status} ({heartRateTrend.PercentChange:F1}% change over {heartRateTrend.DaysAnalyzed} days)");

// Generate comprehensive reports
var sleepQuality = analytics.AnalyzeSleepQuality(sleepRecords, days: 30);
Console.WriteLine($"Sleep quality: {sleepQuality.Description}");
Console.WriteLine($"  - Average duration: {sleepQuality.AverageDuration:F1} minutes");
Console.WriteLine($"  - Excellent nights: {sleepQuality.ExcellentNights}/{sleepQuality.TotalNights} ({sleepQuality.ExcellenceRate:F1}%)");

var spo2Health = analytics.AnalyzeSpO2Health(spo2Records, days: 30);
Console.WriteLine($"SpO2 health: {spo2Health.Status}");
Console.WriteLine($"  - Average: {spo2Health.AverageSpO2}%");
Console.WriteLine($"  - Minimum: {spo2Health.MinimumSpO2}%");

var activityIntensity = analytics.AnalyzeActivityIntensity(
    new List<ActivityData> { /* populate with activity data */ },
    days: 7
);
Console.WriteLine($"Activity intensity: {activityIntensity.LowIntensity} low, {activityIntensity.MediumIntensity} medium, {activityIntensity.HighIntensity} high");

// Calculate overall health score
var healthScore = analytics.CalculateHealthScore(
    new HealthDataCollection {
        SleepRecords = sleepRecords,
        HeartRateRecords = heartRateRecords,
        SpO2Records = spo2Records,
        StepsRecords = stepsRecords
    },
    days: 7
);
Console.WriteLine($"Health score: {healthScore}/100");
```
## AnomalyPoint

The `AnomalyPoint` class represents a single data point that was flagged as anomalous during Z-score analysis, providing detailed information about the anomaly including its date, value, statistical deviation, and severity classification.

### Usage Example

```csharp
using HealthDataExportTools.DTOs;

// Create a new AnomalyPoint instance
var anomaly = new AnomalyPoint
{
    Date = DateTime.UtcNow.AddHours(-2),
    Value = 145.5,
    ZScore = 3.2,
    DeviationFromMean = 22.3,
    Severity = "Severe"
};

// Accessing properties
Console.WriteLine($"Anomaly Date: {anomaly.Date}");
Console.WriteLine($"Measured Value: {anomaly.Value}");
Console.WriteLine($"Z-Score: {anomaly.ZScore}");
Console.WriteLine($"Deviation from Mean: {anomaly.DeviationFromMean}");
Console.WriteLine($"Severity: {anomaly.Severity}");
```

## ValidationResultDto

The `ValidationResultDto` class represents the result of a validation operation, providing detailed information about the validation process, including the validation ID, timestamp, and statistics about the validation outcome.

### Usage Example

```csharp
using HealthDataExportTools.DTOs;

// Create a new ValidationResultDto instance
var result = new ValidationResultDto
{
    ValidationId = Guid.NewGuid().ToString(),
    Timestamp = DateTime.UtcNow,
    IsValid = true,
    TotalRecords = 100,
    ValidRecords = 90,
    InvalidRecords = 10,
    ValidationErrors = new List<ValidationErrorDetail>
    {
        ValidationErrorDetail.Create(1, "Field1", "Error message 1"),
        ValidationErrorDetail.Create(2, "Field2", "Error message 2")
    },
    Warnings = new List<ValidationWarning>
    {
        ValidationWarning.Create(3, "Field3", "Warning message 1"),
        ValidationWarning.Create(4, "Field4", "Warning message 2")
    },
    Statistics = new ValidationStatistics
    {
        DataTypeBreakdown = new Dictionary<string, DataTypeValidationStats>
        {
            {"DataType1", new DataTypeValidationStats {TotalRecords = 50, ValidRecords = 40, InvalidRecords = 10}},
            {"DataType2", new DataTypeValidationStats {TotalRecords = 50, ValidRecords = 40, InvalidRecords = 10}}
        },
        ErrorsByType = new Dictionary<string, int>
        {
            {"ErrorType1", 5},
            {"ErrorType2", 5}
        },
        MostCommonErrors = new List<CommonError>
        {
            new CommonError {ErrorCode = "ErrorType1", Message = "Error message 1", Occurrences = 5, AffectedFields = new List<string> {"Field1", "Field2"}},
            new CommonError {ErrorCode = "ErrorType2", Message = "Error message 2", Occurrences = 5, AffectedFields = new List<string> {"Field3", "Field4"}}
        },
        AffectedDevices = new List<string> {"Device1", "Device2"},
        DateRange = new DateRangeInfo {Start = DateTime.Now.AddDays(-30), End = DateTime.Now}
    },
    DurationMs = 1000
};

// Accessing some properties
Console.WriteLine($"Validation ID: {result.ValidationId}");
Console.WriteLine($"Is valid: {result.IsValid}");
Console.WriteLine($"Total records: {result.TotalRecords}");
Console.WriteLine($"Valid records: {result.ValidRecords}");
Console.WriteLine($"Invalid records: {result.InvalidRecords}");
Console.WriteLine($"Validation errors count: {result.GetErrorCount()}");
Console.WriteLine($"Validation warnings count: {result.GetWarningCount()}");
Console.WriteLine($"Validation duration (ms): {result.DurationMs}");
```

## CacheService

The `CacheService` provides a high-level interface for caching health data, analytics results, and file parse results with configurable time-to-live (TTL). It abstracts the underlying cache provider and adds domain-specific caching methods for common health data scenarios, including automatic key prefixing and comprehensive error handling.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Cache;
using Microsoft.Extensions.Logging;

// Create logger and cache provider
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<CacheService>();
var cacheProvider = new InMemoryCacheProvider(logger);

// Create cache service with 2-hour default TTL
var cacheService = new CacheService(cacheProvider, logger, TimeSpan.FromHours(2));

// Sample health data records (using SleepData as concrete implementation)
var healthRecords = new List<HealthDataRecord>
{
    new SleepData
    {
        RecordDate = DateTime.UtcNow.AddDays(-1),
        DeviceId = "device_001",
        DurationMinutes = 480,
        DeepSleepMinutes = 120,
        FirmwareVersion = "1.2.3"
    },
    new SleepData
    {
        RecordDate = DateTime.UtcNow.AddDays(-2),
        DeviceId = "device_001",
        DurationMinutes = 510,
        DeepSleepMinutes = 135,
        FirmwareVersion = "1.2.3"
    }
};

// Cache health data with custom key
await cacheService.CacheHealthDataAsync("patient_12345_sleep", healthRecords);

// Retrieve cached health data
var cachedRecords = await cacheService.GetCachedHealthDataAsync("patient_12345_sleep");
if (cachedRecords != null)
{
    Console.WriteLine($"Retrieved {cachedRecords.Count} cached health records");
    foreach (var record in cachedRecords)
    {
        Console.WriteLine($" - Record date: {record.RecordDate:yyyy-MM-dd}, Device: {record.DeviceId}");
    }
}

// Cache analytics data (using anonymous object as example)
var analyticsData = new { TotalRecords = 1500, AvgSleepDuration = 495.5, AnalysisDate = DateTime.UtcNow };
await cacheService.CacheAnalyticsAsync("sleep_analytics_q2_2024", analyticsData);

// Retrieve cached analytics
var cachedAnalytics = await cacheService.GetCachedAnalyticsAsync<object>("sleep_analytics_q2_2024");
if (cachedAnalytics != null)
{
    Console.WriteLine($"Cached analytics retrieved: {cachedAnalytics.TotalRecords} records");
}

// Cache file parse results
var filePath = "/data/patient_records_2024.json";
await cacheService.CacheParseResultAsync(filePath, healthRecords);

// Retrieve cached parse result
var cachedParseResult = await cacheService.GetCachedParseResultAsync(filePath);
if (cachedParseResult != null)
{
    Console.WriteLine($"Parse result cached for file: {filePath}");
}

// Check if health data is cached
bool isCached = await cacheService.IsHealthDataCachedAsync("patient_12345_sleep");
Console.WriteLine($"Health data cached: {isCached}");

// Get cache statistics
var stats = await cacheService.GetStatsAsync();
if (stats != null)
{
    Console.WriteLine($"Cache items: {stats.ItemCount}");
    Console.WriteLine($"Total size: {stats.TotalSize} bytes");
    Console.WriteLine($"Hit rate: {stats.HitRate:P2}");
}

// Remove specific cache entry
await cacheService.RemoveAsync("patient_12345_sleep");

// Clear all cache entries
await cacheService.ClearAllAsync();

// Clear cache entries matching a pattern
await cacheService.ClearPatternAsync("analytics_");
```

## ValidationService

The `ValidationService` provides comprehensive validation capabilities for health data records, ensuring data quality and consistency across all health metrics including sleep, heart rate, SpO2, steps, activity, and other health measurements. It validates individual records and provides detailed error information for debugging and data cleaning purposes.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using HealthDataExportTools.Domain.Models;

// Create validation service
var validationService = new ValidationService();

// Sample sleep data to validate
var sleepData = new SleepData
{
    RecordDate = DateTime.UtcNow.AddDays(-1),
    DeviceId = "device_001",
    DurationMinutes = 480,
    DeepSleepMinutes = 120,
    FirmwareVersion = "1.2.3"
};

// Validate sleep data
var sleepValidation = validationService.ValidateSleepData(sleepData);
if (!sleepValidation.IsValid)
{
    Console.WriteLine("Sleep data validation failed:");
    foreach (var error in sleepValidation.Errors)
    {
        Console.WriteLine($" - {error}");
    }
}

// Sample heart rate data to validate
var heartRateData = new HeartRateData
{
    RecordDate = DateTime.UtcNow.AddDays(-1),
    DeviceId = "device_001",
    AverageBpm = 75,
    MinimumBpm = 50,
    MaximumBpm = 120,
    FirmwareVersion = "1.2.3"
};

// Validate heart rate data
var heartRateValidation = validationService.ValidateHeartRateData(heartRateData);
if (!heartRateValidation.IsValid)
{
    Console.WriteLine("Heart rate data validation failed:");
    foreach (var error in heartRateValidation.Errors)
    {
        Console.WriteLine($" - {error}");
    }
}

// Sample SpO2 data to validate
var spo2Data = new SpO2Data
{
    RecordDate = DateTime.UtcNow.AddDays(-1),
    DeviceId = "device_001",
    AveragePercentage = 97,
    MinimumPercentage = 95,
    FirmwareVersion = "1.2.3"
};

// Validate SpO2 data
var spo2Validation = validationService.ValidateSpO2Data(spo2Data);
if (!spo2Validation.IsValid)
{
    Console.WriteLine("SpO2 data validation failed:");
    foreach (var error in spo2Validation.Errors)
    {
        Console.WriteLine($" - {error}");
    }
}

// Sample steps data to validate
var stepsData = new StepsData
{
    RecordDate = DateTime.UtcNow.AddDays(-1),
    DeviceId = "device_001",
    TotalSteps = 8500,
    FirmwareVersion = "1.2.3"
};

// Validate steps data
var stepsValidation = validationService.ValidateStepsData(stepsData);
if (!stepsValidation.IsValid)
{
    Console.WriteLine("Steps data validation failed:");
    foreach (var error in stepsValidation.Errors)
    {
        Console.WriteLine($" - {error}");
    }
}

// Sample activity data to validate
var activityData = new ActivityData
{
    RecordDate = DateTime.UtcNow.AddDays(-1),
    DeviceId = "device_001",
    DurationMinutes = 60,
    CaloriesBurned = 300,
    FirmwareVersion = "1.2.3"
};

// Validate activity data
var activityValidation = validationService.ValidateActivityData(activityData);
if (!activityValidation.IsValid)
{
    Console.WriteLine("Activity data validation failed:");
    foreach (var error in activityValidation.Errors)
    {
        Console.WriteLine($" - {error}");
    }
}

// Validate multiple metrics at once
var healthMetric = new HealthMetric
{
    RecordDate = DateTime.UtcNow.AddDays(-1),
    DeviceId = "device_001",
    MetricType = "BloodPressure",
    Systolic = 120,
    Diastolic = 80,
    FirmwareVersion = "1.2.3"
};

// Validate health metric
var healthMetricValidation = validationService.ValidateHealthMetric(healthMetric);
if (!healthMetricValidation.IsValid)
{
    Console.WriteLine("Health metric validation failed:");
    foreach (var error in healthMetricValidation.Errors)
    {
        Console.WriteLine($" - {error}");
    }
}

// Add custom error to validation result
var customValidation = new ValidationResult();
customValidation.AddError("Custom validation failed: field value is out of range");
Console.WriteLine(customValidation.ToString());

// Check validation result properties
Console.WriteLine($"Validation result: {(validationService.ValidateSleepData(sleepData).IsValid ? "Valid" : "Invalid")}");
Console.WriteLine($"Error count: {validationService.ValidateSleepData(sleepData).Errors.Count}");
```

## ReportGenerationService

The `ReportGenerationService` provides comprehensive report generation capabilities for health data, producing summary reports, trend analysis, and detailed statistics across multiple health metrics including sleep, heart rate, SpO2, steps, and activity data. It supports daily, weekly, and overall health summaries with statistical analysis and export functionality.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using HealthDataExportTools.Domain.Models;
using Microsoft.Extensions.Logging;

// Create logger and service
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<ReportGenerationService>();
var reportService = new ReportGenerationService(logger);

// Sample health data records
var healthRecords = new List<HealthDataRecord>
{
    new SleepData { RecordDate = DateTime.UtcNow.AddDays(-1), DeviceId = "device_001", DurationMinutes = 480, DeepSleepMinutes = 120 },
    new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-1), DeviceId = "device_001", AverageBpm = 68 },
    new StepsData { RecordDate = DateTime.UtcNow.AddDays(-1), DeviceId = "device_001", TotalSteps = 8500 },
    new SpO2Data { RecordDate = DateTime.UtcNow.AddDays(-1), DeviceId = "device_001", AveragePercentage = 97 },
    new ActivityData { RecordDate = DateTime.UtcNow.AddDays(-1), DeviceId = "device_001", DurationMinutes = 60, CaloriesBurned = 300 }
};

// Generate comprehensive summary report
var summaryReport = await reportService.GenerateSummaryReportAsync(healthRecords);
Console.WriteLine($"Summary Report - Date: {summaryReport.ReportDate:yyyy-MM-dd}");
Console.WriteLine($"Total Records: {summaryReport.TotalRecords}");
Console.WriteLine($"Date Range: {summaryReport.DateRange.StartDate:yyyy-MM-dd} to {summaryReport.DateRange.EndDate:yyyy-MM-dd}");
Console.WriteLine($"Data Types: {string.Join(", ", summaryReport.DataTypeStatistics.Select(d => d.DataType))}");
Console.WriteLine($"Devices: {string.Join(", ", summaryReport.DeviceDistribution.Select(d => $"{d.Key} ({d.Value} records)"))}");

// Generate daily summary report for a specific date
var dailyReport = await reportService.GenerateDailyReportAsync(
    new List<SleepData> { new SleepData { RecordDate = DateTime.UtcNow.AddDays(-1), DurationMinutes = 480, DeepSleepMinutes = 120, RemSleepMinutes = 90, Quality = Domain.Enums.SleepQuality.Good } },
    new List<HeartRateData> { new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-1), AverageBpm = 68, MinimumBpm = 55, MaximumBpm = 85 } },
    DateTime.UtcNow.AddDays(-1)
);
Console.WriteLine($"\nDaily Report - {dailyReport.Date:yyyy-MM-dd}");
Console.WriteLine($"Sleep: {dailyReport.SleepMetrics?.TotalDurationMinutes} minutes, Quality: {dailyReport.SleepMetrics?.AverageQuality}");
Console.WriteLine($"Heart Rate: Avg {dailyReport.HeartRateMetrics?.AverageHeartRate} BPM (Min: {dailyReport.HeartRateMetrics?.MinHeartRate}, Max: {dailyReport.HeartRateMetrics?.MaxHeartRate})");

// Generate weekly summary reports
var weeklyReports = await reportService.GenerateWeeklySummaryReportAsync(
    new List<SleepData> { new SleepData { RecordDate = DateTime.UtcNow.AddDays(-7), DurationMinutes = 450, DeepSleepMinutes = 105, Quality = Domain.Enums.SleepQuality.Excellent } },
    new List<HeartRateData> { new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-7), AverageBpm = 70 } },
    new List<StepsData> { new StepsData { RecordDate = DateTime.UtcNow.AddDays(-7), TotalSteps = 9200 } }
);
Console.WriteLine($"\nWeekly Reports Generated: {weeklyReports.Count}");
foreach (var weeklyReport in weeklyReports)
{
    Console.WriteLine($"Week {weeklyReport.WeekIdentifier}: Health Score {weeklyReport.WeeklyHealthScore}/100");
    Console.WriteLine($"  - Sleep: {weeklyReport.AverageSleepDurationMinutes} minutes avg, Quality: {weeklyReport.AverageSleepQuality}");
    Console.WriteLine($"  - Steps: {weeklyReport.TotalSteps} total, Goal days: {weeklyReport.GoalAchievedDays}");
    Console.WriteLine($"  - SpO2: {weeklyReport.AverageSpO2}% avg, Min: {weeklyReport.MinimumSpO2}%");
    if (weeklyReport.Changes != null)
    {
        Console.WriteLine($"  - Changes vs previous week:");
        Console.WriteLine($"    Sleep: {weeklyReport.Changes.SleepDurationChangePercent}%");
        Console.WriteLine($"    Steps: {weeklyReport.Changes.StepsChangePercent}%");
        Console.WriteLine($"    Health Score: {weeklyReport.Changes.HealthScoreChangePoints} points");
    }
}

// Export weekly reports to JSON
await reportService.ExportWeeklySummaryToJsonAsync(weeklyReports, "/tmp/weekly_reports.json");
Console.WriteLine("\nWeekly reports exported to JSON");

// Generate trend analysis report
var trendReport = await reportService.GenerateTrendReportAsync(healthRecords, windowDays: 7);
Console.WriteLine($"\nTrend Analysis - Last {trendReport.WindowDays} days");
Console.WriteLine($"Analysis Date: {trendReport.AnalysisDate:yyyy-MM-dd}");
foreach (var metric in trendReport.MetricTrends)
{
    Console.WriteLine($"  {metric.MetricType}: {metric.AverageValue:F2} avg, Trend: {metric.TrendDirection}, Variation: {metric.VariationPercent:F1}%");
}
```

## ChartExportOptions

The `ChartExportOptions` class configures chart export behavior for health data visualizations, allowing fine-grained control over which charts are generated and whether summary tables are included in the export. It provides properties to enable/disable specific chart types and methods to export health data to interactive HTML charts.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using HealthDataExportTools.Domain.Models;

// Create export options with custom configuration
var exportOptions = new ChartExportOptions
{
    Title = "Patient Health Metrics - Q2 2024",
    IncludeSummaryTable = true,
    IncludeSpO2Chart = true,
    IncludeActivityChart = true,
    IncludeSleepCompositionChart = false
};

// Sample health data collection
var healthData = new HealthDataCollection
{
    SleepRecords = new List<SleepData>
    {
        new SleepData { RecordDate = DateTime.UtcNow.AddDays(-1), DurationMinutes = 450, DeepSleepMinutes = 95 },
        new SleepData { RecordDate = DateTime.UtcNow.AddDays(-2), DurationMinutes = 480, DeepSleepMinutes = 105 }
    },
    HeartRateRecords = new List<HeartRateData>
    {
        new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-1), AverageBpm = 68 },
        new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-2), AverageBpm = 70 }
    },
    SpO2Records = new List<SpO2Data>
    {
        new SpO2Data { RecordDate = DateTime.UtcNow.AddDays(-1), AveragePercentage = 97 },
        new SpO2Data { RecordDate = DateTime.UtcNow.AddDays(-2), AveragePercentage = 96 }
    },
    StepsRecords = new List<StepsData>
    {
        new StepsData { RecordDate = DateTime.UtcNow.AddDays(-1), TotalSteps = 8500 },
        new StepsData { RecordDate = DateTime.UtcNow.AddDays(-2), TotalSteps = 9200 }
    },
    ActivityRecords = new List<ActivityData>
    {
        new ActivityData { RecordDate = DateTime.UtcNow.AddDays(-1), DurationMinutes = 60, CaloriesBurned = 300 },
        new ActivityData { RecordDate = DateTime.UtcNow.AddDays(-2), DurationMinutes = 75, CaloriesBurned = 375 }
    }
};

// Export to HTML charts
var chartService = new ChartExportService();
await chartService.ExportToHtmlChartsAsync(
    healthData,
    exportOptions,
    outputDirectory: "/exports/patient_charts",
    fileNamePrefix: "patient_123_q2_2024"
);
```

## DataComparisonService

The `DataComparisonService` provides functionality to compare two distinct sets of health data records across multiple metrics including sleep, heart rate, steps, SpO2, and activity data. It supports both direct comparison of pre-built periods and comparison by date ranges within a single collection, calculating percentage changes and generating narrative summaries.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using HealthDataExportTools.Domain.Models;

// Create sample health data collections
var period1 = new HealthDataCollection
{
    SleepRecords = new List<SleepData>
    {
        new SleepData { RecordDate = DateTime.UtcNow.AddDays(-14), DurationMinutes = 420, DeepSleepMinutes = 95 },
        new SleepData { RecordDate = DateTime.UtcNow.AddDays(-13), DurationMinutes = 450, DeepSleepMinutes = 105 },
        new SleepData { RecordDate = DateTime.UtcNow.AddDays(-12), DurationMinutes = 480, DeepSleepMinutes = 110 }
    },
    HeartRateRecords = new List<HeartRateData>
    {
        new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-14), AverageBpm = 68 },
        new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-13), AverageBpm = 70 },
        new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-12), AverageBpm = 69 }
    },
    StepsRecords = new List<StepsData>
    {
        new StepsData { RecordDate = DateTime.UtcNow.AddDays(-14), TotalSteps = 8500 },
        new StepsData { RecordDate = DateTime.UtcNow.AddDays(-13), TotalSteps = 9200 },
        new StepsData { RecordDate = DateTime.UtcNow.AddDays(-12), TotalSteps = 8800 }
    },
    SpO2Records = new List<SpO2Data>
    {
        new SpO2Data { RecordDate = DateTime.UtcNow.AddDays(-14), AveragePercentage = 97 },
        new SpO2Data { RecordDate = DateTime.UtcNow.AddDays(-13), AveragePercentage = 96 },
        new SpO2Data { RecordDate = DateTime.UtcNow.AddDays(-12), AveragePercentage = 98 }
    },
    ActivityRecords = new List<ActivityData>
    {
        new ActivityData { RecordDate = DateTime.UtcNow.AddDays(-14), DurationMinutes = 60, CaloriesBurned = 300 },
        new ActivityData { RecordDate = DateTime.UtcNow.AddDays(-13), DurationMinutes = 75, CaloriesBurned = 375 },
        new ActivityData { RecordDate = DateTime.UtcNow.AddDays(-12), DurationMinutes = 45, CaloriesBurned = 225 }
    }
};

var period2 = new HealthDataCollection
{
    SleepRecords = new List<SleepData>
    {
        new SleepData { RecordDate = DateTime.UtcNow.AddDays(-7), DurationMinutes = 405, DeepSleepMinutes = 85 },
        new SleepData { RecordDate = DateTime.UtcNow.AddDays(-6), DurationMinutes = 435, DeepSleepMinutes = 95 },
        new SleepData { RecordDate = DateTime.UtcNow.AddDays(-5), DurationMinutes = 465, DeepSleepMinutes = 100 }
    },
    HeartRateRecords = new List<HeartRateData>
    {
        new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-7), AverageBpm = 72 },
        new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-6), AverageBpm = 71 },
        new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-5), AverageBpm = 70 }
    },
    StepsRecords = new List<StepsData>
    {
        new StepsData { RecordDate = DateTime.UtcNow.AddDays(-7), TotalSteps = 7200 },
        new StepsData { RecordDate = DateTime.UtcNow.AddDays(-6), TotalSteps = 6800 },
        new StepsData { RecordDate = DateTime.UtcNow.AddDays(-5), TotalSteps = 7500 }
    },
    SpO2Records = new List<SpO2Data>
    {
        new SpO2Data { RecordDate = DateTime.UtcNow.AddDays(-7), AveragePercentage = 95 },
        new SpO2Data { RecordDate = DateTime.UtcNow.AddDays(-6), AveragePercentage = 94 },
        new SpO2Data { RecordDate = DateTime.UtcNow.AddDays(-5), AveragePercentage = 96 }
    },
    ActivityRecords = new List<ActivityData>
    {
        new ActivityData { RecordDate = DateTime.UtcNow.AddDays(-7), DurationMinutes = 30, CaloriesBurned = 150 },
        new ActivityData { RecordDate = DateTime.UtcNow.AddDays(-6), DurationMinutes = 45, CaloriesBurned = 225 },
        new ActivityData { RecordDate = DateTime.UtcNow.AddDays(-5), DurationMinutes = 60, CaloriesBurned = 300 }
    }
};

// Create comparison service and compare periods
var comparisonService = new DataComparisonService();
var result = await comparisonService.ComparePeriodsAsync(period1, period2);

// Access comparison results
Console.WriteLine($"Generated at: {result.GeneratedAt}");
Console.WriteLine($"Period 1 records: {result.Period1RecordCount}");
Console.WriteLine($"Period 2 records: {result.Period2RecordCount}");
Console.WriteLine($"Sleep change: {result.SleepDurationChangePercentage}%");
Console.WriteLine($"Deep sleep change: {result.DeepSleepChangePercentage}%");
Console.WriteLine($"Heart rate change: {result.HeartRateChangePercentage}%");
Console.WriteLine($"Steps change: {result.StepsChangePercentage}%");
Console.WriteLine($"SpO2 change: {result.SpO2ChangePercentage}%");
Console.WriteLine($"Activity minutes change: {result.ActivityMinutesChangePercentage}%");
Console.WriteLine($"Calories change: {result.CaloriesChangePercentage}%");
Console.WriteLine($"Summary: {result.NarrativeSummary}");

// Export to JSON
await comparisonService.ExportToJsonAsync(result, "/tmp/comparison_result.json");

// Compare by date ranges
var fullCollection = new HealthDataCollection
{
    SleepRecords = period1.SleepRecords.Concat(period2.SleepRecords).ToList(),
    HeartRateRecords = period1.HeartRateRecords.Concat(period2.HeartRateRecords).ToList(),
    StepsRecords = period1.StepsRecords.Concat(period2.StepsRecords).ToList(),
    SpO2Records = period1.SpO2Records.Concat(period2.SpO2Records).ToList(),
    ActivityRecords = period1.ActivityRecords.Concat(period2.ActivityRecords).ToList()
};

var dateRangeResult = await comparisonService.CompareByDateRangeAsync(
    fullCollection,
    DateTime.UtcNow.AddDays(-14),
    DateTime.UtcNow.AddDays(-12),
    DateTime.UtcNow.AddDays(-7),
    DateTime.UtcNow.AddDays(-5)
);
```

## ErrorHandlingMiddleware

The `ErrorHandlingMiddleware` class provides centralized error handling and exception transformation in the HTTP request pipeline. It implements the `IMiddleware` interface and catches exceptions, converting them into structured error responses with consistent error IDs, status codes, and diagnostic information.

### Usage Example

```csharp
using HealthDataExportTools.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

// In your Program.cs or Startup.cs:
// builder.Services.AddTransient<ErrorHandlingMiddleware>();
// app.UseMiddleware<ErrorHandlingMiddleware>();

// Example usage in a controller or endpoint:
public class PatientController : ControllerBase
{
    private readonly ErrorHandlingMiddleware _errorHandler;
    private readonly ILogger<PatientController> _logger;
    
    public PatientController(ErrorHandlingMiddleware errorHandler, ILogger<PatientController> logger)
    {
        _errorHandler = errorHandler;
        _logger = logger;
    }
    
    [HttpGet("patients/{id}")]
    public async Task<IActionResult> GetPatient(string id)
    {
        var context = new MiddlewareContext
        {
            RequestId = Guid.NewGuid().ToString(),
            // Other context setup
        };
        
        // Process request through error handling middleware
        await _errorHandler.ProcessAsync(context, async ctx =>
        {
            // Your business logic here
            var patient = await _patientService.GetPatientAsync(id);
            if (patient == null)
            {
                throw new HealthDataException("Patient not found");
            }
            
            return Results.Ok(patient);
        });
        
        if (!context.ContinueProcessing)
        {
            // The error was handled by middleware
            var errorResponse = context.Result as ErrorResponse;
            return Results.Problem(
                statusCode: errorResponse?.StatusCode ?? 500,
                title: errorResponse?.Message ?? "An error occurred",
                detail: errorResponse?.Details,
                extensions: new Dictionary<string, object?>
                {
                    ["errorId"] = errorResponse?.ErrorId,
                    ["requestId"] = errorResponse?.RequestId,
                    ["timestamp"] = errorResponse?.Timestamp
                }
            );
        }
        
        return Results.Ok(context.Result);
    }
}

// The ErrorResponse class contains the following properties:
// - ErrorId: Unique identifier for the error instance
// - RequestId: Correlation ID for tracing the request
// - StatusCode: HTTP status code (400, 404, 409, 500, etc.)
// - Message: Human-readable error message
// - Details: Detailed error information
// - ExceptionType: Type of the original exception (optional)
// - Timestamp: When the error occurred
```

## InMemoryCacheProvider

The `InMemoryCacheProvider` class provides a thread-safe, in-memory cache implementation with expiration support and comprehensive statistics tracking. It implements the `ICacheProvider` interface and uses `ReaderWriterLockSlim` for concurrent access, making it suitable for multi-threaded applications.

### Usage Example

```csharp
using HealthDataExportTools.Cache;
using Microsoft.Extensions.Logging;

// Create cache provider with logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<InMemoryCacheProvider>();
var cacheProvider = new InMemoryCacheProvider(logger);

// Set a value in cache with 10 minute expiration
var patientData = new { Id = 123, Name = "John Doe", Age = 45 };
await cacheProvider.SetAsync("patient_123", patientData, TimeSpan.FromMinutes(10));

// Get a value from cache
var cachedData = await cacheProvider.GetAsync<object>("patient_123");
if (cachedData != null)
{
    Console.WriteLine("Cache hit!");
}
else
{
    Console.WriteLine("Cache miss - value not found or expired");
}

// Check if key exists
bool exists = await cacheProvider.ExistsAsync("patient_123");
Console.WriteLine($"Key exists: {exists}");

// Get cache statistics
var stats = await cacheProvider.GetStatsAsync();
Console.WriteLine($"Cache items: {stats.ItemCount}");
Console.WriteLine($"Hit rate: {(stats.HitCount + stats.MissCount > 0 ? (double)stats.HitCount / (stats.HitCount + stats.MissCount) : 0):P2}");
Console.WriteLine($"Total size: {stats.TotalSize} bytes");

// Remove a specific key
await cacheProvider.RemoveAsync("patient_123");

// Clear entire cache
await cacheProvider.ClearAsync();

// Get all cache keys
var keys = await cacheProvider.GetKeysAsync();
Console.WriteLine($"Cache contains {keys.Count} keys");

// Dispose when done
cacheProvider.Dispose();
```

## NotificationService

The `NotificationService` handles sending notifications about export and import operations, supporting multiple notification channels (email, webhooks, logs, etc.). It provides specialized methods for common operations like export completion, failures, import progress, and data quality warnings, with built-in logging support.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using HealthDataExportTools.Domain.Models;
using Microsoft.Extensions.Logging;

// Create notification service with logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<NotificationService>();
var notificationService = new NotificationService(logger);

// Register a log-based notification channel (always available)
var logChannel = new LogNotificationChannel(logger);
notificationService.RegisterChannel(logChannel);

// Register additional channels (e.g., email, webhook)
// notificationService.RegisterChannel(new EmailNotificationChannel(smtpConfig));
// notificationService.RegisterChannel(new WebhookNotificationChannel(webhookUrl));

// Notify about successful export
await notificationService.NotifyExportCompletedAsync(
    exportId: "export_2024_001",
    recordCount: 1542,
    outputPath: "/exports/patient_records_2024_001.json",
    duration: TimeSpan.FromSeconds(45.2)
);

// Notify about export failure
try
{
    // Export logic here
}
catch (Exception ex)
{
    await notificationService.NotifyExportFailedAsync(
        exportId: "export_2024_001",
        errorMessage: "Failed to connect to database",
        exception: ex
    );
}

// Notify about import progress
for (int i = 0; i < 1000; i += 100)
{
    await notificationService.NotifyImportProgressAsync(
        importId: "import_2024_002",
        processedRecords: i,
        totalRecords: 1000
    );
}

// Notify about data quality warnings
var warnings = new List<string>
{
    "Patient age exceeds reasonable range (150 years)",
    "Missing required field: date_of_birth",
    "Heart rate value out of normal range"
};
await notificationService.NotifyDataQualityWarningsAsync(
    operationId: "data_quality_check_001",
    warnings: warnings
);

// Check how many channels are registered
int channelCount = notificationService.GetChannelCount();
Console.WriteLine($"Registered notification channels: {channelCount}");
```

## IMiddleware

The `IMiddleware` interface defines the contract for middleware components in the request processing pipeline. It provides properties for tracking request context, metadata, and processing state, allowing middleware to handle requests and responses consistently.

### Usage Example

```csharp
using HealthDataExportTools.Middleware;

// Implement a custom middleware
public class CustomMiddleware : IMiddleware
{
    public string RequestId { get; set; }
    public DateTime StartTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public object? Data { get; set; }
    public Exception? Exception { get; set; }
    public bool ContinueProcessing { get; set; } = true;
    public object? Result { get; set; }
}

// Usage in a request pipeline
var middleware = new CustomMiddleware
{
    RequestId = Guid.NewGuid().ToString(),
    StartTime = DateTime.UtcNow,
    Metadata = new Dictionary<string, object>
    {
        {"UserId", "user_123"},
        {"RequestType", "export"}
    }
};

// Process request through middleware
Console.WriteLine($"Processing request: {middleware.RequestId}");
Console.WriteLine($"Started at: {middleware.StartTime}");

// Set result or exception
middleware.Result = new { Status = "Completed", Data = new { Records = 100 } };
// OR
middleware.Exception = new InvalidOperationException("Invalid data format");
middleware.ContinueProcessing = false;
```

