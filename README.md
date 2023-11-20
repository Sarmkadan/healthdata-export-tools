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

## HealthDataParserServiceTests

The `HealthDataParserServiceTests` class contains comprehensive unit tests for the `HealthDataParserService` class. It tests various parsing scenarios including JSON parsing, device type detection, and collection merging functionality to ensure robust health data processing.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using HealthDataExportTools.Domain.Enums;

// Create a parser service with validation
var validationService = new MockValidationService();
var parserService = new HealthDataParserService(validationService);

// Test JSON parsing with all health data types
var jsonContent = @"{
    ""sleep"": [{
        ""recordDate"": ""2024-01-01T00:00:00Z"",
        ""deviceId"": ""my_zepp_watch"",
        ""sleepStart"": ""2024-01-01T22:00:00Z"",
        ""sleepEnd"": ""2024-01-02T06:00:00Z"",
        ""durationMinutes"": 480,
        ""deepSleepMinutes"": 90,
        ""lightSleepMinutes"": 270,
        ""remSleepMinutes"": 60,
        ""awakeMinutes"": 60,
        ""quality"": ""3"",
        ""score"": 85
    }],
    ""heartRate"": [{
        ""recordDate"": ""2024-01-01T12:00:00Z"",
        ""deviceId"": ""my_zepp_watch"",
        ""minimumBpm"": 50,
        ""maximumBpm"": 120,
        ""averageBpm"": 70,
        ""restingBpm"": 60,
        ""measurementCount"": 100
    }],
    ""spO2"": [{
        ""recordDate"": ""2024-01-01T03:00:00Z"",
        ""deviceId"": ""my_zepp_watch"",
        ""minimumPercentage"": 95,
        ""maximumPercentage"": 99,
        ""averagePercentage"": 97,
        ""restingPercentage"": 98,
        ""measurementCount"": 50
    }],
    ""steps"": [{
        ""recordDate"": ""2024-01-01T23:59:59Z"",
        ""deviceId"": ""my_zepp_watch"",
        ""totalSteps"": 10000,
        ""distanceKm"": 7.5,
        ""caloriesBurned"": 500,
        ""dailyGoal"": 10000,
        ""activeMinutes"": 120
    }]
}";

// Parse JSON and verify results
var collection = await parserService.ParseJsonAsync(jsonContent);

// Verify all data types were parsed
collection.SleepRecords.Should().HaveCount(1);
collection.HeartRateRecords.Should().HaveCount(1);
collection.SpO2Records.Should().HaveCount(1);
collection.StepsRecords.Should().HaveCount(1);

// Test device type detection
var deviceType = parserService.DetectDeviceType("my_zepp_watch");
deviceType.Should().Be(DeviceType.Zepp);

// Test collection merging
var collection1 = new HealthDataCollection {
    SleepRecords = { new SleepData { DeviceId = "device1" } }
};
var collection2 = new HealthDataCollection {
    HeartRateRecords = { new HeartRateData { DeviceId = "device2" } }
};
var merged = parserService.MergeCollections(collection1, collection2);
merged.SleepRecords.Should().HaveCount(1);
merged.HeartRateRecords.Should().HaveCount(1);
```

## AnalyticsServiceTests

The `AnalyticsServiceTests` class contains comprehensive unit tests for the `AnalyticsService` class. It tests various analytics calculations including sleep duration, heart rate, steps, trends, health scores, and quality analysis for different health metrics like sleep, SpO2, and activity intensity.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using HealthDataExportTools.Domain.Models;
using FluentAssertions;

// Create an analytics service instance
var analyticsService = new AnalyticsService();

// Test sleep duration calculation
var sleepRecords = new List<SleepData>
{
    new SleepData { RecordDate = DateTime.UtcNow.AddDays(-1), DurationMinutes = 480 },
    new SleepData { RecordDate = DateTime.UtcNow.AddDays(-2), DurationMinutes = 540 },
    new SleepData { RecordDate = DateTime.UtcNow.AddDays(-3), DurationMinutes = 420 }
};

var avgSleepDuration = analyticsService.CalculateAverageSleepDuration(sleepRecords, 7);
avgSleepDuration.Should().BeApproximately(8.5, 0.001); // 8.5 hours average

// Test heart rate calculation
var heartRateRecords = new List<HeartRateData>
{
    new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-1), AverageBpm = 65 },
    new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-2), AverageBpm = 68 },
    new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-3), AverageBpm = 72 }
};

var avgHeartRate = analyticsService.CalculateAverageHeartRate(heartRateRecords, 7);
avgHeartRate.Should().Be(68);

// Test steps calculation
var stepsRecords = new List<StepsData>
{
    new StepsData { RecordDate = DateTime.UtcNow.AddDays(-1), TotalSteps = 12000 },
    new StepsData { RecordDate = DateTime.UtcNow.AddDays(-2), TotalSteps = 8500 },
    new StepsData { RecordDate = DateTime.UtcNow.AddDays(-3), TotalSteps = 11000 }
};

var totalSteps = analyticsService.CalculateTotalSteps(stepsRecords, 7);
totalSteps.Should().Be(31500);

// Test trend analysis
var values = new List<int> { 10, 12, 15, 18, 22, 25, 30, 35, 40, 45 };
var trendResult = analyticsService.AnalyzeTrend(values, 10);
trendResult.Status.Should().Be("Improving");
trendResult.PercentChange.Should().BeGreaterThan(10);

// Test health score calculation
var collection = new HealthDataCollection
{
    SleepRecords = { new SleepData { DurationMinutes = 450, DeepSleepMinutes = 90, RemSleepMinutes = 75 } },
    HeartRateRecords = { new HeartRateData { AverageBpm = 65 } },
    SpO2Records = { new SpO2Data { AveragePercentage = 98 } },
    StepsRecords = { new StepsData { TotalSteps = 10500 } }
};

var healthScore = analyticsService.CalculateHealthScore(collection);
healthScore.Should().BeGreaterThan(50);

// Test sleep quality analysis
var sleepQualityReport = analyticsService.AnalyzeSleepQuality(sleepRecords);
sleepQualityReport.TotalNights.Should().Be(3);
sleepQualityReport.ExcellentNights.Should().Be(0);

// Test SpO2 health analysis
var spo2Records = new List<SpO2Data>
{
    new SpO2Data { AveragePercentage = 98, MinimumPercentage = 95, LowSpO2Events = 0 },
    new SpO2Data { AveragePercentage = 92, MinimumPercentage = 88, LowSpO2Events = 2 }
};

var spo2Report = analyticsService.AnalyzeSpO2Health(spo2Records, 30);
spo2Report.AverageSpO2.Should().Be(95);
spo2Report.Status.Should().Be("Alert - Concerning");

// Test activity intensity analysis
var activityRecords = new List<ActivityData>
{
    new ActivityData { IntensityLevel = 25, CaloriesBurned = 150 }, // Low
    new ActivityData { IntensityLevel = 55, CaloriesBurned = 250 }, // Medium
    new ActivityData { IntensityLevel = 85, CaloriesBurned = 350 }  // High
};

var intensityDistribution = analyticsService.AnalyzeActivityIntensity(activityRecords, 7);
intensityDistribution.TotalActivities.Should().Be(3);
intensityDistribution.LowIntensity.Should().Be(1);
intensityDistribution.MediumIntensity.Should().Be(1);
intensityDistribution.HighIntensity.Should().Be(1);
intensityDistribution.TotalCalories.Should().Be(750);
```

## DomainModelTests

The `DomainModelTests` class contains unit tests for various domain models, including `SleepData`, `StepsData`, `HeartRateData`, and `SpO2Data`. It tests the calculation of sleep quality, deep sleep percentage, goal achievement, and other metrics. The following example demonstrates how to use the `DomainModelTests` class to test the calculation of sleep quality:

```csharp
var sleepData = new SleepData
{
    DurationMinutes = 480,
    DeepSleepMinutes = 90,
    LightSleepMinutes = 270,
    RemSleepMinutes = 60,
    AwakeMinutes = 60
};

var quality = sleepData.CalculateQuality();
quality.Should().Be(SleepQuality.Good);

var deepSleepPercentage = sleepData.GetDeepSleepPercentage();
deepSleepPercentage.Should().BeApproximately(18.75, 0.01);
```

## MockValidationService

The `MockValidationService` class provides a mock implementation of the `IValidationService` interface, allowing for easy testing and validation of health data. 

## HealthDataParserServiceTests

The `HealthDataParserServiceTests` class contains comprehensive unit tests for the `HealthDataParserService` class. 

```csharp
var parserService = new HealthDataParserService(new MockValidationService());
var deviceType = parserService.DetectDeviceType("my_zepp_watch");
deviceType.Should().Be(DeviceType.Zepp);
```