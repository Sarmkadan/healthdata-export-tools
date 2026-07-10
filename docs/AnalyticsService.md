# AnalyticsService

The `AnalyticsService` class provides a centralized interface for computing health metrics and generating analytical reports from exported health data. It exposes methods to calculate averages, totals, and composite scores, as well as methods that produce specialized report objects for sleep quality, SpO2 health, activity intensity distribution, and trend analysis. The service also maintains a set of read-only properties that reflect the most recent analysis results, such as average duration, sleep stage percentages, and excellence rates.

## API

### Methods

#### `double CalculateAverageSleepDuration()`
Computes the mean sleep duration across all recorded nights.  
**Returns:** The average sleep duration in hours.  
**Throws:** `InvalidOperationException` if no sleep data is available.

#### `int CalculateAverageHeartRate()`
Computes the mean heart rate over the entire observation period.  
**Returns:** The average heart rate in beats per minute, rounded to the nearest integer.  
**Throws:** `InvalidOperationException` if no heart rate data is present.

#### `int CalculateAverageSpO2()`
Computes the mean peripheral capillary oxygen saturation (SpO2) level.  
**Returns:** The average SpO2 percentage, rounded to the nearest integer.  
**Throws:** `InvalidOperationException` if no SpO2 readings are available.

#### `int CalculateTotalSteps()`
Sums the total number of steps recorded.  
**Returns:** The total step count.  
**Throws:** `InvalidOperationException` if step data is missing.

#### `double CalculateAverageDeepSleepPercentage()`
Calculates the average percentage of sleep time spent in deep sleep.  
**Returns:** A value between 0 and 100 representing the mean deep sleep percentage.  
**Throws:** `InvalidOperationException` if sleep stage data is unavailable.

#### `double CalculateAverageRemPercentage()`
Calculates the average percentage of sleep time spent in REM sleep.  
**Returns:** A value between 0 and 100 representing the mean REM sleep percentage.  
**Throws:** `InvalidOperationException` if sleep stage data is unavailable.

#### `TrendAnalysis AnalyzeTrend()`
Performs a trend analysis on the health data over time.  
**Returns:** A `TrendAnalysis` object containing trend direction, magnitude, and supporting statistics.  
**Throws:** `InvalidOperationException` if insufficient data points exist for trend calculation.

#### `SleepQualityReport AnalyzeSleepQuality()`
Generates a comprehensive sleep quality report.  
**Returns:** A `SleepQualityReport` object containing metrics such as duration, interruptions, and sleep stage breakdowns.  
**Throws:** `InvalidOperationException` if sleep data is insufficient or missing.

#### `SpO2HealthReport AnalyzeSpO2Health()`
Generates a report on SpO2 health indicators, including variability and time below threshold.  
**Returns:** An `SpO2HealthReport` object summarizing oxygen saturation patterns.  
**Throws:** `InvalidOperationException` if SpO2 data is unavailable.

#### `ActivityIntensityDistribution AnalyzeActivityIntensity()`
Analyzes the distribution of activity across intensity levels (sedentary, light, moderate, vigorous).  
**Returns:** An `ActivityIntensityDistribution` object with time spent in each intensity zone.  
**Throws:** `InvalidOperationException` if activity data is missing.

#### `int CalculateHealthScore()`
Computes a composite health score based on multiple metrics (e.g., sleep, heart rate, steps, SpO2).  
**Returns:** An integer score, typically in a range such as 0–100.  
**Throws:** `InvalidOperationException` if any required metric is unavailable.

### Properties

#### `string Status`
Gets the current status of the service (e.g., `"Ready"`, `"DataLoaded"`, `"Error"`).  
**Throws:** Never.

#### `double PercentChange`
Gets the percentage change in the primary health metric (e.g., average sleep duration) compared to a baseline period.  
**Throws:** `InvalidOperationException` if baseline data has not been established.

#### `int DaysAnalyzed`
Gets the number of days included in the most recent analysis.  
**Throws:** Never.

#### `double AverageDuration`
Gets the average sleep duration (in hours) from the most recent analysis.  
**Throws:** `InvalidOperationException` if no analysis has been performed.

#### `double AverageDeepSleep`
Gets the average deep sleep percentage from the most recent analysis.  
**Throws:** `InvalidOperationException` if no analysis has been performed.

#### `double AverageRemSleep`
Gets the average REM sleep percentage from the most recent analysis.  
**Throws:** `InvalidOperationException` if no analysis has been performed.

#### `int ExcellentNights`
Gets the count of nights that meet the “excellent” quality threshold (e.g., duration ≥ 7 hours, deep sleep ≥ 20%).  
**Throws:** `InvalidOperationException` if no analysis has been performed.

#### `int TotalNights`
Gets the total number of nights considered in the most recent analysis.  
**Throws:** Never.

#### `double ExcellenceRate`
Gets the ratio of excellent nights to total nights, expressed as a decimal (0.0–1.0).  
**Throws:** `InvalidOperationException` if no analysis has been performed.

## Usage

### Example 1: Basic sleep metrics and health score

```csharp
var analytics = new AnalyticsService();
analytics.LoadData("export_2025-03-01.json"); // hypothetical data loading method

double avgSleep = analytics.CalculateAverageSleepDuration();
int avgHeartRate = analytics.CalculateAverageHeartRate();
int healthScore = analytics.CalculateHealthScore();

Console.WriteLine($"Average sleep: {avgSleep:F1} hours");
Console.WriteLine($"Average heart rate: {avgHeartRate} bpm");
Console.WriteLine($"Health score: {healthScore}");
```

### Example 2: Generating and inspecting a sleep quality report

```csharp
var analytics = new AnalyticsService();
analytics.LoadData("export_2025-03-01.json");

SleepQualityReport report = analytics.AnalyzeSleepQuality();
Console.WriteLine($"Sleep quality: {report.OverallRating}");
Console.WriteLine($"Average duration: {analytics.AverageDuration} hours");
Console.WriteLine($"Excellent nights: {analytics.ExcellentNights} / {analytics.TotalNights}");
Console.WriteLine($"Excellence rate: {analytics.ExcellenceRate:P1}");
```

## Notes

- **Empty or missing data:** All calculation methods throw `InvalidOperationException` when the required data is absent. Ensure data is loaded and validated before calling these methods.
- **Property access order:** Properties such as `AverageDuration`, `AverageDeepSleep`, and `ExcellentNights` are populated only after an analysis method (e.g., `AnalyzeSleepQuality`) has been invoked. Accessing them before analysis will throw `InvalidOperationException`.
- **Thread safety:** `AnalyticsService` is not thread-safe. Concurrent calls to methods that modify internal state (e.g., loading new data) while other methods are executing may produce inconsistent results. External synchronization is required if the instance is shared across threads.
- **Precision:** Double-returning methods use standard floating-point arithmetic. Rounding behavior for integer-returning methods follows midpoint rounding away from zero.
- **Excellence threshold:** The definition of an “excellent night” is determined by internal configuration and may vary between versions of the library. Refer to the project documentation for the exact criteria.
