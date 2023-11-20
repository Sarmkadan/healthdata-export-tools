// Health Data Export Tools

...

## ExportServiceExtensions

The `ExportServiceExtensions` class provides a set of static methods for exporting various health data summaries to CSV files. It offers methods for exporting sleep summary, heart rate analytics, health dashboard, and data quality report. These methods allow for easy and efficient data export, making it convenient for users to access and analyze their health data.

### Usage Example

```csharp
using HealthDataExportTools.Services;

// Export sleep summary to CSV
await ExportServiceExtensions.ExportSleepSummaryToCsvAsync("sleep_summary.csv");

// Export heart rate analytics to CSV
await ExportServiceExtensions.ExportHeartRateAnalyticsToCsvAsync("heart_rate_analytics.csv");

// Export health dashboard to CSV
await ExportServiceExtensions.ExportHealthDashboardToCsvAsync("health_dashboard.csv");

// Export data quality report to CSV
await ExportServiceExtensions.ExportDataQualityReportToCsvAsync("data_quality_report.csv");

// Get total records
int totalRecords = ExportServiceExtensions.TotalRecords;

// Get total duration in minutes
int totalDurationMinutes = ExportServiceExtensions.TotalDurationMinutes;

// Get total deep sleep minutes
int totalDeepSleepMinutes = ExportServiceExtensions.TotalDeepSleepMinutes;

// Get total light sleep minutes
int totalLightSleepMinutes = ExportServiceExtensions.TotalLightSleepMinutes;

// Get total REM minutes
int totalRemMinutes = ExportServiceExtensions.TotalRemMinutes;

// Get total awake minutes
int totalAwakeMinutes = ExportServiceExtensions.TotalAwakeMinutes;

// Get average quality
double? avgQuality = ExportServiceExtensions.AvgQuality;

// Get average score
int? avgScore = ExportServiceExtensions.AvgScore;

// Get best sleep date
string? bestSleepDate = ExportServiceExtensions.BestSleepDate;

// Get worst sleep date
string? worstSleepDate = ExportServiceExtensions.WorstSleepDate;

// Get deep sleep percentage
double deepSleepPercentage = ExportServiceExtensions.DeepSleepPercentage;

// Get REM percentage
double remPercentage = ExportServiceExtensions.RemPercentage;

// Get efficiency percentage
double efficiencyPercentage = ExportServiceExtensions.EfficiencyPercentage;

// Get date
string? date = ExportServiceExtensions.Date;

// Get duration in minutes
int durationMinutes = ExportServiceExtensions.DurationMinutes;

// Get deep sleep minutes
int deepSleepMinutes = ExportServiceExtensions.DeepSleepMinutes;
```

## InMemoryHealthDataRepositoryExtensions

The `InMemoryHealthDataRepositoryExtensions` class provides in-memory data access methods for retrieving health metrics like sleep, heart rate, and steps. It offers efficient, lightweight access to health data stored in memory, ideal for scenarios requiring fast lookups or temporary data processing.

### Usage Example

```csharp
using HealthDataExportTools.Data;

// Retrieve the most recent sleep record
var mostRecentSleep = await InMemoryHealthDataRepositoryExtensions.GetMostRecentSleepAsync();

// Check if data exists for a specific date
bool hasData = await InMemoryHealthDataRepositoryExtensions.HasDataForDateAsync("2024-03-20");

// Get total steps and average heart rate
int? totalSteps = await InMemoryHealthDataRepositoryExtensions.GetTotalStepsAsync();
int? avgHeartRate = await InMemoryHealthDataRepositoryExtensions.GetAverageHeartRateAsync();

// Get the latest record of any type
var latestRecord = await InMemoryHealthDataRepositoryExtensions.GetLatestRecordAsync();

// Group all records by date
var recordsByDate = await InMemoryHealthDataRepositoryExtensions.GetRecordsByDateGroupedAsync();
```

## MockValidationService

The `MockValidationService` class provides a mock implementation of the `IValidationService` interface, allowing for easy testing and validation of health data. It offers methods for validating sleep data, heart rate data, SpO2 data, steps data, activity data, and health metrics.

### Usage Example

```csharp
using HealthDataExportTools.Services;

// Create a new instance of MockValidationService
var mockValidationService = new MockValidationService();

// Validate sleep data
var sleepData = new SleepData();
var validationResult = mockValidationService.ValidateSleepData(sleepData);

// Validate heart rate data
var heartRateData = new HeartRateData();
var heartRateValidationResult = mockValidationService.ValidateHeartRateData(heartRateData);

// Validate SpO2 data
var spo2Data = new SpO2Data();
var spo2ValidationResult = mockValidationService.ValidateSpO2Data(spo2Data);

// Validate steps data
var stepsData = new StepsData();
var stepsValidationResult = mockValidationService.ValidateStepsData(stepsData);

// Validate activity data
var activityData = new ActivityData();
var activityValidationResult = mockValidationService.ValidateActivityData(activityData);

// Validate health metric
var healthMetric = new HealthMetric();
var healthValidationResult = mockValidationService.ValidateHealthMetric(healthMetric);
```
```