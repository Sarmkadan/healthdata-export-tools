# API Reference

Complete reference for all public APIs in Health Data Export Tools.

## Table of Contents

- [Services](#services)
- [Domain Models](#domain-models)
- [DTOs](#dtos)
- [Enumerations](#enumerations)
- [Interfaces](#interfaces)
- [Exceptions](#exceptions)
- [Utilities](#utilities)

## Services

### HealthDataParserService

Parse and deserialize health data from export files.

```csharp
public class HealthDataParserService
{
    /// Parse health data from a ZIP archive
    /// Parameters:
    ///   zipFilePath - Path to the health export ZIP file
    /// Returns: Complete health data export DTO
    /// Throws: HealthDataException if parsing fails
    public Task<HealthDataExportDto> ParseHealthDataAsync(string zipFilePath);
    
    /// Parse sleep data from a specific file
    /// Parameters:
    ///   filePath - Path to sleep data file
    /// Returns: Enumerable of parsed sleep records
    public Task<IEnumerable<SleepData>> ParseSleepDataAsync(string filePath);
    
    /// Parse heart rate data from a specific file
    public Task<IEnumerable<HeartRateData>> ParseHeartRateDataAsync(string filePath);
    
    /// Parse SpO2 data from a specific file
    public Task<IEnumerable<SpO2Data>> ParseSpO2DataAsync(string filePath);
    
    /// Parse steps and activity data from a specific file
    public Task<IEnumerable<StepsData>> ParseStepsDataAsync(string filePath);
}
```

### ExportService

Export parsed health data to multiple formats.

```csharp
public class ExportService
{
    /// Export health data using specified options
    /// Parameters:
    ///   data - Health data to export
    ///   options - Export configuration options
    /// Returns: Task representing the asynchronous operation
    public Task ExportAsync(HealthDataExportDto data, HealthDataExportOptions options);
    
    /// Export all data in specified format
    /// Parameters:
    ///   data - Health data to export
    ///   outputPath - Directory for output files
    ///   format - Export format (Csv, Json, Xml)
    public Task ExportCompleteAsync(
        HealthDataExportDto data, 
        string outputPath, 
        ExportFormat format);
    
    /// Export sleep data to CSV
    public Task ExportSleepToCsvAsync(
        IEnumerable<SleepData> data, 
        string outputPath);
    
    /// Export heart rate data to CSV
    public Task ExportHeartRateToCsvAsync(
        IEnumerable<HeartRateData> data, 
        string outputPath);
    
    /// Export steps data to CSV
    public Task ExportStepsToCsvAsync(
        IEnumerable<StepsData> data, 
        string outputPath);
    
    /// Export SpO2 data to CSV
    public Task ExportSpO2ToCsvAsync(
        IEnumerable<SpO2Data> data, 
        string outputPath);
    
    /// Export all data to JSON
    public Task ExportToJsonAsync(
        HealthDataExportDto data, 
        string outputPath);
}
```

### AnalyticsService

Calculate metrics, trends, and health insights.

```csharp
public class AnalyticsService
{
    /// Analyze sleep quality patterns
    /// Parameters:
    ///   records - Collection of sleep data records
    /// Returns: Sleep quality analysis report
    /// Example:
    ///   var report = analytics.AnalyzeSleepQuality(sleepRecords);
    ///   Console.WriteLine($"Sleep Quality: {report.Description}");
    ///   Console.WriteLine($"Average Duration: {report.AverageDuration} min");
    public SleepQualityReport AnalyzeSleepQuality(IEnumerable<SleepData> records);
    
    /// Analyze SpO2 health status
    public SpO2HealthReport AnalyzeSpO2Health(IEnumerable<SpO2Data> records);
    
    /// Calculate overall health score
    /// Returns: Integer score from 0-100
    /// Calculation: 30% sleep + 25% HR + 20% SpO2 + 25% activity
    public int CalculateHealthScore(HealthDataExportDto data);
    
    /// Analyze activity trends over time
    public ActivityTrendReport AnalyzeActivityTrends(IEnumerable<StepsData> records);
    
    /// Analyze heart rate patterns and trends
    public HeartRateAnalysisReport AnalyzeHeartRate(IEnumerable<HeartRateData> records);
    
    /// Generate comprehensive health report
    public ComprehensiveHealthReport GenerateFullReport(HealthDataExportDto data);
}
```

### ValidationService

Validate health data for consistency and correctness.

```csharp
public class ValidationService
{
    /// Validate sleep data record
    /// Parameters:
    ///   record - Sleep data record to validate
    /// Returns: Validation result with errors/warnings
    public ValidationResultDto ValidateSleepData(SleepData record);
    
    /// Validate heart rate data record
    public ValidationResultDto ValidateHeartRateData(HeartRateData record);
    
    /// Validate SpO2 data record
    public ValidationResultDto ValidateSpO2Data(SpO2Data record);
    
    /// Validate steps data record
    public ValidationResultDto ValidateStepsData(StepsData record);
    
    /// Validate all data in collection
    public Task<ValidationResultDto> ValidateAllAsync(HealthDataExportDto data);
}
```

### CacheService

Manage in-memory caching of health data.

```csharp
public class CacheService
{
    /// Get cached value by key
    /// Parameters:
    ///   key - Cache key identifier
    /// Returns: Cached value or null if not found/expired
    public async Task<T?> GetAsync<T>(string key);
    
    /// Store value in cache with optional TTL
    /// Parameters:
    ///   key - Cache key identifier
    ///   value - Value to cache
    ///   duration - Time-to-live (default from options)
    public async Task SetAsync<T>(string key, T value, TimeSpan? duration = null);
    
    /// Remove cached value
    public async Task RemoveAsync(string key);
    
    /// Clear all cache
    public async Task ClearAsync();
}
```

### BatchProcessingService

Process multiple health data files in parallel.

```csharp
public class BatchProcessingService
{
    /// Process all ZIP files in a directory
    /// Parameters:
    ///   directory - Directory containing health export files
    ///   format - Export format for output
    /// Returns: Collection of batch result DTOs
    public Task<IEnumerable<BatchResultDto>> ProcessDirectoryAsync(
        string directory, 
        ExportFormat format);
    
    /// Process with custom options
    public Task<IEnumerable<BatchResultDto>> ProcessDirectoryAsync(
        string directory, 
        HealthDataExportOptions options);
}
```

### BackgroundTaskScheduler

Schedule background tasks for periodic operations.

```csharp
public class BackgroundTaskScheduler
{
    /// Schedule a periodic task
    /// Parameters:
    ///   name - Task identifier
    ///   interval - Time between executions
    ///   action - Async action to perform
    public void ScheduleTask(string name, TimeSpan interval, Func<Task> action);
    
    /// Stop a scheduled task
    public void StopTask(string name);
    
    /// Stop all scheduled tasks
    public void StopAll();
}
```

## Domain Models

### HealthDataRecord (Base Class)

```csharp
public abstract class HealthDataRecord
{
    /// Date of the health record
    public DateTime RecordDate { get; set; }
    
    /// Device identifier (e.g., "Amazfit-12345")
    public string DeviceId { get; set; }
    
    /// When the record was created
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// Record metadata/notes
    public string? Notes { get; set; }
}
```

### SleepData

```csharp
public class SleepData : HealthDataRecord
{
    /// Sleep start timestamp
    public DateTime SleepStart { get; set; }
    
    /// Sleep end timestamp
    public DateTime SleepEnd { get; set; }
    
    /// Total sleep duration in minutes
    public int DurationMinutes { get; set; }
    
    /// Deep sleep duration in minutes
    public int DeepSleepMinutes { get; set; }
    
    /// Light sleep duration in minutes
    public int LightSleepMinutes { get; set; }
    
    /// REM sleep duration in minutes
    public int RemSleepMinutes { get; set; }
    
    /// Awake time in minutes
    public int AwakeMinutes { get; set; }
    
    /// Sleep quality score (0-100)
    public int Score { get; set; }
    
    /// Average heart rate during sleep
    public int AverageHeartRate { get; set; }
    
    /// Quality classification
    public SleepQuality Quality { get; set; }
    
    /// Calculate quality based on duration and score
    public SleepQuality CalculateQuality();
}
```

### HeartRateData

```csharp
public class HeartRateData : HealthDataRecord
{
    /// Minimum heart rate for the day (bpm)
    public int MinimumBpm { get; set; }
    
    /// Maximum heart rate for the day (bpm)
    public int MaximumBpm { get; set; }
    
    /// Average heart rate for the day (bpm)
    public int AverageBpm { get; set; }
    
    /// Resting heart rate (bpm)
    public int RestingBpm { get; set; }
    
    /// Number of measurements taken
    public int MeasurementCount { get; set; }
    
    /// Stress level (0-100)
    public int StressLevel { get; set; }
}
```

### SpO2Data

```csharp
public class SpO2Data : HealthDataRecord
{
    /// Minimum SpO2 percentage
    public int MinimumPercentage { get; set; }
    
    /// Maximum SpO2 percentage
    public int MaximumPercentage { get; set; }
    
    /// Average SpO2 percentage
    public int AveragePercentage { get; set; }
    
    /// Resting SpO2 percentage
    public int RestingPercentage { get; set; }
    
    /// Number of measurements
    public int MeasurementCount { get; set; }
    
    /// Count of low SpO2 events (<95%)
    public int LowSpO2Events { get; set; }
}
```

### StepsData

```csharp
public class StepsData : HealthDataRecord
{
    /// Total steps for the day
    public int TotalSteps { get; set; }
    
    /// Distance traveled in kilometers
    public double DistanceKm { get; set; }
    
    /// Calories burned
    public int CaloriesBurned { get; set; }
    
    /// Daily step goal
    public int DailyGoal { get; set; }
    
    /// Active minutes during the day
    public int ActiveMinutes { get; set; }
    
    /// Walking minutes
    public int WalkingMinutes { get; set; }
    
    /// Running minutes
    public int RunningMinutes { get; set; }
    
    /// Goal achievement percentage (0-100)
    public double GoalAchievementPercent { get; set; }
    
    /// Calculate goal achievement percentage
    public void UpdateGoalAchievement();
}
```

## DTOs

### HealthDataExportDto

```csharp
public class HealthDataExportDto
{
    /// Collection of sleep records
    public List<SleepData> SleepRecords { get; set; } = new();
    
    /// Collection of heart rate records
    public List<HeartRateData> HeartRateRecords { get; set; } = new();
    
    /// Collection of SpO2 records
    public List<SpO2Data> SpO2Records { get; set; } = new();
    
    /// Collection of steps records
    public List<StepsData> StepsRecords { get; set; } = new();
    
    /// Total record count
    public int GetTotalRecordCount();
    
    /// Check if data is empty
    public bool IsEmpty { get; }
}
```

### ExportResultDto

```csharp
public class ExportResultDto
{
    /// Export completion status
    public bool Success { get; set; }
    
    /// Output file path
    public string OutputPath { get; set; }
    
    /// Number of records exported
    public int RecordsExported { get; set; }
    
    /// Duration of export in milliseconds
    public long DurationMilliseconds { get; set; }
    
    /// Error message if failed
    public string? ErrorMessage { get; set; }
}
```

### ValidationResultDto

```csharp
public class ValidationResultDto
{
    /// Whether validation passed
    public bool IsValid { get; set; }
    
    /// List of validation errors
    public List<string> Errors { get; set; } = new();
    
    /// List of validation warnings
    public List<string> Warnings { get; set; } = new();
    
    /// Number of records validated
    public int RecordsValidated { get; set; }
    
    /// Number of invalid records
    public int RecordsInvalid { get; set; }
}
```

## Enumerations

### ExportFormat

```csharp
public enum ExportFormat
{
    /// Comma-separated values format
    Csv = 0,
    
    /// JSON format
    Json = 1,
    
    /// XML format
    Xml = 2,
    
    /// All supported formats
    All = 3
}
```

### SleepQuality

```csharp
public enum SleepQuality
{
    /// Poor sleep (< 5 hours or score < 50)
    Poor = 0,
    
    /// Fair sleep (5-6 hours or score 50-70)
    Fair = 1,
    
    /// Good sleep (6-8 hours or score 70-85)
    Good = 2,
    
    /// Excellent sleep (8+ hours and score 85+)
    Excellent = 3
}
```

### DeviceType

```csharp
public enum DeviceType
{
    /// Zepp/Amazfit device
    Amazfit = 0,
    
    /// Garmin device
    Garmin = 1,
    
    /// Unknown device type
    Unknown = 2
}
```

## Interfaces

### IHealthDataRepository

```csharp
public interface IHealthDataRepository
{
    /// Save complete health data
    Task SaveHealthDataAsync(HealthDataExportDto data);
    
    /// Retrieve health data
    Task<HealthDataExportDto?> GetHealthDataAsync();
    
    /// Get sleep records for date range
    Task<IEnumerable<SleepData>> GetSleepRecordsAsync(
        DateTime? fromDate = null, 
        DateTime? toDate = null);
    
    /// Get heart rate records for date range
    Task<IEnumerable<HeartRateData>> GetHeartRateRecordsAsync(
        DateTime? fromDate = null, 
        DateTime? toDate = null);
    
    /// Delete records older than specified days
    Task DeleteOldRecordsAsync(int daysToKeep);
}
```

### IDataFormatter

```csharp
public interface IDataFormatter
{
    /// Format data to string representation
    /// Type parameter T: Data type to format
    /// Returns: Formatted string
    Task<string> FormatAsync<T>(T data) where T : class;
}
```

### IEventPublisher

```csharp
public interface IEventPublisher
{
    /// Subscribe to event type
    void Subscribe<T>(Func<T, Task> handler) where T : IEvent;
    
    /// Unsubscribe from event type
    void Unsubscribe<T>(Func<T, Task> handler) where T : IEvent;
    
    /// Publish event to subscribers
    Task PublishAsync<T>(T evt) where T : IEvent;
}
```

### IEvent

```csharp
public interface IEvent
{
    /// Event identifier
    string EventId { get; }
    
    /// When event occurred
    DateTime Timestamp { get; }
}
```

## Exceptions

### HealthDataException

Base exception for health data errors.

```csharp
public class HealthDataException : Exception
{
    public HealthDataException(string message) : base(message) { }
    public HealthDataException(string message, Exception inner) 
        : base(message, inner) { }
}
```

## Utilities

### Constants

```csharp
public class Constants
{
    // HR constraints
    public const int MinHeartRate = 30;
    public const int MaxHeartRate = 220;
    
    // SpO2 constraints
    public const int MinSpO2 = 70;
    public const int MaxSpO2 = 100;
    
    // Sleep constraints
    public const int MinSleepDurationMinutes = 0;
    public const int MaxSleepDurationMinutes = 720; // 12 hours
    
    // File extensions
    public const string ZipExtension = ".zip";
    public const string JsonExtension = ".json";
    public const string CsvExtension = ".csv";
    public const string XmlExtension = ".xml";
}
```

### DateTimeExtensions

```csharp
public static class DateTimeExtensions
{
    /// Convert to Unix timestamp
    public static long ToUnixTimestamp(this DateTime dateTime);
    
    /// Convert from Unix timestamp
    public static DateTime FromUnixTimestamp(long timestamp);
    
    /// Get start of day
    public static DateTime StartOfDay(this DateTime dateTime);
    
    /// Get end of day
    public static DateTime EndOfDay(this DateTime dateTime);
}
```

### FileUtility

```csharp
public static class FileUtility
{
    /// Check if file path is valid
    public static bool IsValidFilePath(string path);
    
    /// Get file size in MB
    public static double GetFileSizeMB(string path);
    
    /// Safe file write with backup
    public static async Task SafeWriteAsync(string path, string content);
}
```

## Configuration Classes

### HealthDataExportOptions

```csharp
public class HealthDataExportOptions
{
    /// Path to input health export files
    public string InputPath { get; set; } = "./exports/";
    
    /// Path for export output
    public string OutputPath { get; set; } = "./output/";
    
    /// SQLite database file path
    public string DatabasePath { get; set; } = "./healthdata.db";
    
    /// Export format(s)
    public ExportFormat ExportFormat { get; set; } = ExportFormat.Json;
    
    /// Validate data before export
    public bool ValidateData { get; set; } = true;
    
    /// Perform analytics
    public bool PerformAnalysis { get; set; } = true;
    
    /// Enable verbose logging
    public bool VerboseLogging { get; set; } = false;
    
    /// Enable caching
    public bool CacheEnabled { get; set; } = true;
    
    /// Cache TTL in seconds
    public int CacheDurationSeconds { get; set; } = 3600;
    
    /// Max operation retries
    public int MaxRetries { get; set; } = 3;
    
    /// Operation timeout in seconds
    public int TimeoutSeconds { get; set; } = 300;
    
    /// Validate configuration
    public List<string> Validate();
}
```

## Next Steps

For examples using these APIs, see:
- [Getting Started](getting-started.md) - Basic usage guide
- [README.md](../README.md) - 10+ code examples
- [examples/](../examples/) directory - Ready-to-run applications
