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

## ValidationServiceTests

The `ValidationServiceTests` class contains comprehensive unit tests for the `ValidationService` class, covering validation scenarios for various health data types including sleep, heart rate, SpO2, steps, activity, and general health metrics. It tests both valid and invalid data scenarios to ensure robust validation logic.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using HealthDataExportTools.Domain.Models;
using FluentAssertions;

// Create validation service instance
var validationService = new ValidationService();

// Validate sleep data with valid values
var validSleepData = new SleepData
{
    RecordDate = DateTime.UtcNow.Date,
    SleepStart = DateTime.UtcNow.AddHours(-8),
    SleepEnd = DateTime.UtcNow,
    DurationMinutes = 480,
    DeepSleepMinutes = 90,
    LightSleepMinutes = 270,
    RemSleepMinutes = 60,
    AwakeMinutes = 60,
    AverageHeartRate = 65,
    Score = 80,
    Quality = SleepQuality.Good
};

var sleepValidationResult = validationService.ValidateSleepData(validSleepData);
sleepValidationResult.IsValid.Should().BeTrue();
sleepValidationResult.Errors.Should().BeEmpty();

// Validate heart rate data with invalid BPM ranges
var invalidHeartRateData = new HeartRateData
{
    RecordDate = DateTime.UtcNow.Date,
    MinimumBpm = 150,
    MaximumBpm = 100, // Invalid: min > max
    AverageBpm = 120
};

var hrValidationResult = validationService.ValidateHeartRateData(invalidHeartRateData);
hrValidationResult.IsValid.Should().BeFalse();
hrValidationResult.Errors.Should().Contain("Minimum heart rate cannot be greater than maximum");

// Validate SpO2 data with valid values
var validSpO2Data = new SpO2Data
{
    RecordDate = DateTime.UtcNow.Date,
    MinimumPercentage = 95,
    MaximumPercentage = 99,
    AveragePercentage = 97,
    RestingPercentage = 98,
    MeasurementCount = 50
};

var spo2ValidationResult = validationService.ValidateSpO2Data(validSpO2Data);
spo2ValidationResult.IsValid.Should().BeTrue();

// Validate steps data with negative total steps
var invalidStepsData = new StepsData
{
    RecordDate = DateTime.UtcNow.Date,
    TotalSteps = -100 // Invalid: negative steps
};

var stepsValidationResult = validationService.ValidateStepsData(invalidStepsData);
stepsValidationResult.IsValid.Should().BeFalse();
stepsValidationResult.Errors.Should().Contain("TotalSteps must be non-negative");

// Validate activity data with empty activity type
var invalidActivityData = new ActivityData
{
    ActivityType = "", // Invalid: empty
    RecordDate = DateTime.UtcNow.Date,
    StartTime = DateTime.UtcNow.AddHours(-1),
    EndTime = DateTime.UtcNow,
    DurationMinutes = 60
};

var activityValidationResult = validationService.ValidateActivityData(invalidActivityData);
activityValidationResult.IsValid.Should().BeFalse();
activityValidationResult.Errors.Should().Contain("ActivityType cannot be empty");

// Validate health metric with valid values
var validHealthMetric = new HealthMetric
{
    MetricName = "Weight",
    RecordDate = DateTime.UtcNow.Date,
    Value = 75.5,
    Unit = "kg",
    NormalRangeLow = 60,
    NormalRangeHigh = 80
};

var healthMetricValidationResult = validationService.ValidateHealthMetric(validHealthMetric);
healthMetricValidationResult.IsValid.Should().BeTrue();
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


## ChartExportServiceTests

The `ChartExportServiceTests` class contains comprehensive unit tests for the `ChartExportService` class. It tests various chart export scenarios including HTML generation, chart rendering, data handling, and configuration options for different health data types like SpO2 records, sleep data, and summary tables.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using HealthDataExportTools.Domain.Models;
using FluentAssertions;

// Create a chart export service instance
var chartExportService = new ChartExportService();

// Export charts to HTML with SpO2 records
var spo2Records = new List<SpO2Data>
{
    new SpO2Data { RecordDate = DateTime.UtcNow.Date.AddDays(-1), AveragePercentage = 98, MinimumPercentage = 95, MaximumPercentage = 99 },
    new SpO2Data { RecordDate = DateTime.UtcNow.Date.AddDays(-2), AveragePercentage = 96, MinimumPercentage = 93, MaximumPercentage = 98 },
    new SpO2Data { RecordDate = DateTime.UtcNow.Date.AddDays(-3), AveragePercentage = 97, MinimumPercentage = 94, MaximumPercentage = 99 }
};

var htmlResult = await chartExportService.ExportToHtmlChartsAsync(spo2Records);
htmlResult.Should().NotBeNullOrEmpty();
htmlResult.Should().Contain("SpO2");

// Export charts with sleep data
var sleepRecords = new List<SleepData>
{
    new SleepData { RecordDate = DateTime.UtcNow.Date.AddDays(-1), DurationMinutes = 480, DeepSleepMinutes = 90, RemSleepMinutes = 75, AwakeMinutes = 60 },
    new SleepData { RecordDate = DateTime.UtcNow.Date.AddDays(-2), DurationMinutes = 540, DeepSleepMinutes = 105, RemSleepMinutes = 90, AwakeMinutes = 75 }
};

var sleepHtmlResult = await chartExportService.ExportToHtmlChartsAsync(sleepRecords);
sleepHtmlResult.Should().NotBeNullOrEmpty();
sleepHtmlResult.Should().Contain("Sleep Composition");

// Export charts with all data types
var collection = new HealthDataCollection
{
    SpO2Records = { new SpO2Data { AveragePercentage = 98 } },
    SleepRecords = { new SleepData { DurationMinutes = 480 } },
    HeartRateRecords = { new HeartRateData { AverageBpm = 70 } },
    StepsRecords = { new StepsData { TotalSteps = 10000 } }
};

var allDataHtmlResult = await chartExportService.ExportToHtmlChartsAsync(collection);
allDataHtmlResult.Should().NotBeNullOrEmpty();
allDataHtmlResult.Should().Contain("Summary Table");

// Export charts with disabled charts option
var options = new ChartExportOptions
{
    IncludeSpO2Chart = false,
    IncludeSleepCompositionChart = false,
    IncludeSummaryTable = true
};

var optionsHtmlResult = await chartExportService.ExportToHtmlChartsAsync(spo2Records, options);
optionsHtmlResult.Should().NotBeNullOrEmpty();
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

## InMemoryHealthDataRepositoryTests

The `InMemoryHealthDataRepositoryTests` class contains comprehensive unit tests for the `InMemoryHealthDataRepository` class. It tests various CRUD operations and data management scenarios for sleep, heart rate, and steps data stored in memory.

### Usage Example

```csharp
using HealthDataExportTools.Data;
using HealthDataExportTools.Domain.Models;
using FluentAssertions;

// Create an in-memory repository instance
var repository = new InMemoryHealthDataRepository();

// Add sleep data
var sleepData = new SleepData
{
    Id = Guid.NewGuid().ToString(),
    RecordDate = DateTime.UtcNow.Date,
    DeviceId = "TestDevice",
    DurationMinutes = 480,
    Quality = SleepQuality.Good,
    DeepSleepMinutes = 100,
    RemSleepMinutes = 80
};

await repository.AddSleepAsync(sleepData);

// Retrieve sleep data by ID
var retrievedSleep = await repository.GetSleepByIdAsync(sleepData.Id);
retrievedSleep.Should().NotBeNull();

// Update sleep data
sleepData.DurationMinutes = 500;
await repository.UpdateSleepAsync(sleepData);
var updatedSleep = await repository.GetSleepByIdAsync(sleepData.Id);
updatedSleep!.DurationMinutes.Should().Be(500);

// Add heart rate data
var hrData = new HeartRateData
{
    Id = Guid.NewGuid().ToString(),
    RecordDate = DateTime.UtcNow.Date,
    DeviceId = "TestDevice",
    AverageBpm = 70,
    MinimumBpm = 60,
    MaximumBpm = 80
};

await repository.AddHeartRateAsync(hrData);

// Retrieve heart rate data by ID
var retrievedHr = await repository.GetHeartRateByIdAsync(hrData.Id);
retrievedHr.Should().NotBeNull();

// Get total record count
var totalRecords = await repository.GetTotalRecordCountAsync();
totalRecords.Should().Be(2);

// Get sleep data within a date range
var sleepRange = await repository.GetSleepRangeAsync(
    DateTime.UtcNow.AddDays(-2),
    DateTime.UtcNow.AddDays(-1)
);

// Delete sleep data
await repository.DeleteSleepAsync(sleepData.Id);
var deletedSleep = await repository.GetSleepByIdAsync(sleepData.Id);
deletedSleep.Should().BeNull();

// Delete old records (older than 30 days)
await repository.DeleteOldRecordsAsync(DateTime.UtcNow.AddDays(-30));
```

## BatchProcessingServiceTests

The `BatchProcessingServiceTests` class contains comprehensive unit tests for the `BatchProcessingService` class. It tests batch processing functionality including sequential and parallel batch processing, error handling, progress tracking, and batch partitioning scenarios.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using FluentAssertions;

// Create a batch processing service
var logger = Substitute.For<ILogger<BatchProcessingService>>();
var batchProcessingService = new BatchProcessingService(logger);

// Test sequential batch processing
var items = Enumerable.Range(1, 100).ToList();
var processedItems = new List<int>();

var result = await batchProcessingService.ProcessInBatchesAsync(
    items,
    async batch => 
    {
        await Task.Delay(1);
        lock (processedItems)
        {
            processedItems.AddRange(batch);
        }
    },
    batchSize: 10
);

result.TotalItems.Should().Be(100);
result.ProcessedItems.Should().Be(100);
result.IsSuccessful.Should().BeTrue();

// Test parallel batch processing
var parallelResult = await batchProcessingService.ProcessInParallelBatchesAsync(
    items,
    async batch => 
    {
        await Task.Delay(1);
        lock (processedItems)
        {
            processedItems.AddRange(batch);
        }
    },
    batchSize: 10,
    maxParallelism: 4
);

parallelResult.TotalItems.Should().Be(100);
parallelResult.ProcessedItems.Should().Be(100);
parallelResult.IsSuccessful.Should().BeTrue();

// Test batch partitioning
var batches = batchProcessingService.PartitionIntoBatches(items, 3);
batches.Should().HaveCount(34); // 33 batches of 3 items + 1 batch of 1 item
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

## DataTransformationUtilityTests

The `DataTransformationUtilityTests` class provides comprehensive unit tests for the `DataTransformationUtility` class, covering data transformation operations including aggregation, filtering, outlier removal, and statistical calculations. It tests various scenarios for transforming health data to support analytics and reporting workflows.

### Usage Example

```csharp
using HealthDataExportTools.Utilities;
using HealthDataExportTools.Domain.Models;
using FluentAssertions;

// Create sample health data for testing
var sleepRecords = new List<SleepData>
{
    new SleepData { RecordDate = DateTime.UtcNow.Date, DurationMinutes = 400, Quality = SleepQuality.Good },
    new SleepData { RecordDate = DateTime.UtcNow.Date, DurationMinutes = 200, Quality = SleepQuality.Average }
};

// Test sleep aggregation by date
var sleepByDate = DataTransformationUtility.AggregateSleepByDate(sleepRecords);
sleepByDate.Should().ContainKey(DateTime.UtcNow.Date);

// Test heart rate aggregation by hour
var heartRateRecords = new List<HeartRateData>
{
    new HeartRateData { RecordDate = DateTime.UtcNow.AddHours(-2), AverageBpm = 80 },
    new HeartRateData { RecordDate = DateTime.UtcNow.AddHours(-1), AverageBpm = 100 }
};
var heartRateByHour = DataTransformationUtility.AggregateHeartRateByHour(heartRateRecords);

// Test steps aggregation by day
var stepsRecords = new List<StepsData>
{
    new StepsData { RecordDate = DateTime.UtcNow.Date, TotalSteps = 5000 },
    new StepsData { RecordDate = DateTime.UtcNow.Date, TotalSteps = 3000 }
};
var stepsByDay = DataTransformationUtility.AggregateStepsByDay(stepsRecords);

// Test date range filtering
var allRecords = new List<HealthDataRecord>();
var filteredRecords = DataTransformationUtility.FilterByDateRange(allRecords, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

// Test outlier removal
var values = new List<double> { 10, 12, 11, 13, 100, 11, 12, 10 };
var cleanedValues = DataTransformationUtility.RemoveOutliers(values);

// Test moving average calculation
var movingAvg = DataTransformationUtility.CalculateMovingAverage(values, 3);
```

## CacheServiceTests

The `CacheServiceTests` class contains comprehensive unit tests for the `CacheService` class. It tests caching operations including storing and retrieving health data, analytics, clearing cache entries, checking cache existence, and managing cache patterns.

### Usage Example

```csharp
using HealthDataExportTools.Cache;
using HealthDataExportTools.Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

// Create a mock cache provider and logger
var mockCacheProvider = Substitute.For<ICacheProvider>();
var mockLogger = Substitute.For<ILogger<CacheService>>();

// Create the cache service instance
var cacheService = new CacheService(mockCacheProvider, mockLogger);

// Cache health data
var healthDataKey = "user1_health_data";
var healthData = new List<HealthDataRecord> { new SleepData { DeviceId = "device1" } };
await cacheService.CacheHealthDataAsync(healthDataKey, healthData);

// Retrieve cached health data
var cachedHealthData = await cacheService.GetCachedHealthDataAsync(healthDataKey);
cachedHealthData.Should().NotBeNull();

// Check if health data is cached
var isCached = await cacheService.IsHealthDataCachedAsync(healthDataKey);
isCached.Should().BeTrue();

// Cache analytics data
var analyticsKey = "user1_analytics";
var analyticsData = new { AverageSleep = 7.5, StepsGoalAchieved = true };
await cacheService.CacheAnalyticsAsync(analyticsKey, analyticsData);

// Retrieve cached analytics
var cachedAnalytics = await cacheService.GetCachedAnalyticsAsync<dynamic>(analyticsKey);
cachedAnalytics.Should().NotBeNull();

// Get cache statistics
var stats = await cacheService.GetStatsAsync();
stats.ItemCount.Should().BeGreaterOrEqualTo(2);

// Clear all cache entries
await cacheService.ClearAllAsync();

// Clear cache entries matching a pattern
await cacheService.ClearPatternAsync("user1");
```

## SqliteConnectionManagerTests

The `SqliteConnectionManagerTests` class contains comprehensive unit tests for the `SqliteConnectionManager` class, covering database connection management, initialization, verification, and file operations. It tests scenarios for opening connections, creating database files, verifying connection states, and managing database files including existence checks and deletion.

### Usage Example

```csharp
using HealthDataExportTools.Data;
using FluentAssertions;

// Create a connection manager instance
var connectionManager = new SqliteConnectionManager("test_database.db");

// Verify non-existent database returns false
bool existsBefore = connectionManager.DatabaseExists();
existsBefore.Should().BeFalse();

// Initialize database and verify tables are created
await connectionManager.InitializeDatabaseAsync();

// Verify connection is open
bool isConnected = await connectionManager.VerifyConnectionAsync();
isConnected.Should().BeTrue();

// Get an open connection
var connection = await connectionManager.GetConnection();
connection.Should().NotBeNull();

// Verify database now exists
bool existsAfter = connectionManager.DatabaseExists();
existsAfter.Should().BeTrue();

// Get database file size
long size = await connectionManager.GetDatabaseSize();
size.Should().BeGreaterThan(0);

// Delete database file
await connectionManager.DeleteDatabase();

// Verify database no longer exists
bool existsAfterDelete = connectionManager.DatabaseExists();
existsAfterDelete.Should().BeFalse();

// Verify connection returns false after database deletion
bool isConnectedAfterDelete = await connectionManager.VerifyConnectionAsync();
isConnectedAfterDelete.Should().BeFalse();
```

## ValidationServiceTests

The `ValidationServiceTests` class contains comprehensive unit tests for the `ValidationService` class, covering validation scenarios for various health data types including sleep, heart rate, SpO2, steps, activity, and general health metrics. It tests both valid and invalid data scenarios to ensure robust validation logic.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using HealthDataExportTools.Domain.Models;
using FluentAssertions;

// Create validation service instance
var validationService = new ValidationService();

// Validate sleep data with valid values
var validSleepData = new SleepData
{
    RecordDate = DateTime.UtcNow.Date,
    SleepStart = DateTime.UtcNow.AddHours(-8),
    SleepEnd = DateTime.UtcNow,
    DurationMinutes = 480,
    DeepSleepMinutes = 90,
    LightSleepMinutes = 270,
    RemSleepMinutes = 60,
    AwakeMinutes = 60,
    AverageHeartRate = 65,
    Score = 80,
    Quality = SleepQuality.Good
};

var sleepValidationResult = validationService.ValidateSleepData(validSleepData);
sleepValidationResult.IsValid.Should().BeTrue();
sleepValidationResult.Errors.Should().BeEmpty();

// Validate heart rate data with invalid BPM ranges
var invalidHeartRateData = new HeartRateData
{
    RecordDate = DateTime.UtcNow.Date,
    MinimumBpm = 150,
    MaximumBpm = 100, // Invalid: min > max
    AverageBpm = 120
};

var hrValidationResult = validationService.ValidateHeartRateData(invalidHeartRateData);
hrValidationResult.IsValid.Should().BeFalse();
hrValidationResult.Errors.Should().Contain("Minimum heart rate cannot be greater than maximum");

// Validate SpO2 data with valid values
var validSpO2Data = new SpO2Data
{
    RecordDate = DateTime.UtcNow.Date,
    MinimumPercentage = 95,
    MaximumPercentage = 99,
    AveragePercentage = 97,
    RestingPercentage = 98,
    MeasurementCount = 50
};

var spo2ValidationResult = validationService.ValidateSpO2Data(validSpO2Data);
spo2ValidationResult.IsValid.Should().BeTrue();

// Validate steps data with negative total steps
var invalidStepsData = new StepsData
{
    RecordDate = DateTime.UtcNow.Date,
    TotalSteps = -100 // Invalid: negative steps
};

var stepsValidationResult = validationService.ValidateStepsData(invalidStepsData);
stepsValidationResult.IsValid.Should().BeFalse();
stepsValidationResult.Errors.Should().Contain("TotalSteps must be non-negative");

// Validate activity data with empty activity type
var invalidActivityData = new ActivityData
{
    ActivityType = "", // Invalid: empty
    RecordDate = DateTime.UtcNow.Date,
    StartTime = DateTime.UtcNow.AddHours(-1),
    EndTime = DateTime.UtcNow,
    DurationMinutes = 60
};

var activityValidationResult = validationService.ValidateActivityData(invalidActivityData);
activityValidationResult.IsValid.Should().BeFalse();
activityValidationResult.Errors.Should().Contain("ActivityType cannot be empty");

// Validate health metric with valid values
var validHealthMetric = new HealthMetric
{
    MetricName = "Weight",
    RecordDate = DateTime.UtcNow.Date,
    Value = 75.5,
    Unit = "kg",
    NormalRangeLow = 60,
    NormalRangeHigh = 80
};

var healthMetricValidationResult = validationService.ValidateHealthMetric(validHealthMetric);
healthMetricValidationResult.IsValid.Should().BeTrue();
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

## ReportGenerationServiceTests

The `ReportGenerationServiceTests` class contains comprehensive unit tests for the `ReportGenerationService` class. It tests various report generation scenarios including summary reports, daily reports, weekly reports, trend analysis, and JSON export functionality to ensure robust report generation across different health data types.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Logging;

// Create mock logger
var mockLogger = Substitute.For<ILogger<ReportGenerationService>>();

// Create report generation service instance
var reportService = new ReportGenerationService(mockLogger);

// Test summary report generation with valid records
var healthRecords = new List<HealthDataRecord>
{
    new SleepData { RecordDate = DateTime.UtcNow.AddDays(-2), DurationMinutes = 480, Quality = SleepQuality.Good, DeviceId = "DeviceA" },
    new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-1), AverageBpm = 70, DeviceId = "DeviceA" },
    new StepsData { RecordDate = DateTime.UtcNow, TotalSteps = 10000, DeviceId = "DeviceB" }
};

var summaryReport = await reportService.GenerateSummaryReportAsync(healthRecords);
summaryReport.TotalRecords.Should().Be(3);
summaryReport.DeviceDistribution.Should().ContainKey("DeviceA").And.ContainKey("DeviceB");

// Test daily report generation
var today = DateTime.UtcNow.Date;
var sleepData = new List<SleepData>
{
    new SleepData { RecordDate = today.AddHours(1), DurationMinutes = 400, Quality = SleepQuality.Good, DeepSleepMinutes = 100, RemSleepMinutes = 80 },
    new SleepData { RecordDate = today.AddHours(2), DurationMinutes = 500, Quality = SleepQuality.Excellent, DeepSleepMinutes = 120, RemSleepMinutes = 90 }
};

var heartRateData = new List<HeartRateData>
{
    new HeartRateData { RecordDate = today.AddHours(3), AverageBpm = 65, MinimumBpm = 50, MaximumBpm = 80 },
    new HeartRateData { RecordDate = today.AddHours(4), AverageBpm = 75, MinimumBpm = 60, MaximumBpm = 90 }
};

var dailyReport = await reportService.GenerateDailyReportAsync(sleepData, heartRateData, today);
dailyReport.Date.Should().Be(today);
dailyReport.SleepMetrics.Should().NotBeNull();
dailyReport.SleepMetrics!.TotalDurationMinutes.Should().Be(900);
dailyReport.HeartRateMetrics.Should().NotBeNull();
dailyReport.HeartRateMetrics!.AverageHeartRate.Should().Be(70);

// Test weekly summary report generation
var weeklyReports = await reportService.GenerateWeeklySummaryReportAsync(
    new List<SleepData> { new SleepData { RecordDate = today.AddDays(-1), DurationMinutes = 460, Quality = SleepQuality.Good } },
    new List<HeartRateData> { new HeartRateData { RecordDate = today.AddDays(-1), AverageBpm = 68, MinimumBpm = 52, MaximumBpm = 110 } },
    new List<StepsData> { new StepsData { RecordDate = today.AddDays(-1), TotalSteps = 9500 } }
);

weeklyReports.Should().NotBeEmpty();
weeklyReports[0].AverageSleepDurationMinutes.Should().Be(460);
weeklyReports[0].AverageHeartRate.Should().Be(68);
weeklyReports[0].TotalSteps.Should().Be(9500);

// Test trend report generation
var trendRecords = new List<HealthDataRecord>();
for (int i = 0; i < 10; i++)
{
    trendRecords.Add(new StepsData { RecordDate = today.AddDays(-i), TotalSteps = 5000 + (i * 100), DeviceId = "DeviceX" });
}

var trendReport = await reportService.GenerateTrendReportAsync(trendRecords, 7);
trendReport.Should().NotBeNull();
trendReport.WindowDays.Should().Be(7);
trendReport.MetricTrends.Should().NotBeEmpty();

// Test JSON export
var tmpFile = Path.Combine(Path.GetTempPath(), $"weekly_{Guid.NewGuid()}.json");
try
{
    await reportService.ExportWeeklySummaryToJsonAsync(weeklyReports, tmpFile);
    File.Exists(tmpFile).Should().BeTrue();
}
finally
{
    if (File.Exists(tmpFile)) File.Delete(tmpFile);
}
```
