# AnalyticsBenchmarks

The `AnalyticsBenchmarks` class provides methods for calculating and analyzing health-related metrics such as sleep quality and heart rate trends. It is designed to support health data export tools by offering standardized benchmarking operations for derived analytics.

## API

### `void Setup()`

Initializes the analytics benchmarks with default or configured settings. This method must be called before any other public methods to ensure proper state.

**Parameters**
None

**Return Value**
None

**Exceptions**
- `InvalidOperationException`: Thrown if required configuration is missing or if the benchmarks have already been initialized.

---

### `int CalculateHealthScore()`

Computes an overall health score based on aggregated health metrics. The score is derived from sleep quality, heart rate trends, and other internal benchmarks.

**Parameters**
None

**Return Value**
An integer representing the computed health score, typically on a scale from 0 to 100.

**Exceptions**
- `InvalidOperationException`: Thrown if `Setup()` has not been called prior to invocation.

---

### `SleepQualityReport AnalyzeSleepQuality()`

Generates a detailed report analyzing sleep quality metrics over the recorded period. The report includes duration, efficiency, disturbances, and comparative benchmarks.

**Parameters**
None

**Return Value**
A `SleepQualityReport` object containing sleep analysis data such as:
- `Duration` (in minutes)
- `Efficiency` (percentage)
- `Disturbances` (count)
- `SleepPhases` (dictionary of phase durations)

**Exceptions**
- `InvalidOperationException`: Thrown if no sleep data is available or if `Setup()` has not been called.

---

### `TrendAnalysis AnalyzeHeartRateTrend()`

Analyzes heart rate data to identify trends such as resting heart rate stability, activity peaks, and recovery patterns over time.

**Parameters**
None

**Return Value**
A `TrendAnalysis` object containing:
- `BaselineHeartRate` (average resting rate)
- `TrendDirection` (enum: Increasing, Stable, Decreasing)
- `PeakActivityRate` (maximum observed rate)
- `RecoveryTime` (time to return to baseline after exertion)

**Exceptions**
- `InvalidOperationException`: Thrown if heart rate data is insufficient or if `Setup()` has not been called.

## Usage

```csharp
// Example 1: Basic health score calculation
var benchmarks = new AnalyticsBenchmarks();
benchmarks.Setup();

int healthScore = benchmarks.CalculateHealthScore();
Console.WriteLine($"Overall Health Score: {healthScore}");

// Example 2: Comprehensive sleep and heart rate analysis
var benchmarks = new AnalyticsBenchmarks();
benchmarks.Setup();

var sleepReport = benchmarks.AnalyzeSleepQuality();
var trendReport = benchmarks.AnalyzeHeartRateTrend();

Console.WriteLine($"Sleep Efficiency: {sleepReport.Efficiency}%");
Console.WriteLine($"Heart Rate Trend: {trendReport.TrendDirection}");
```

## Notes

- **Initialization Requirement**: All public methods require prior invocation of `Setup()`. Calling methods out of order will result in `InvalidOperationException`.
- **Data Availability**: Methods assume data has been preloaded into the class. Missing data will trigger exceptions.
- **Thread Safety**: This class is not thread-safe. Concurrent access requires external synchronization.
- **Error Handling**: Methods throw `InvalidOperationException` for operational errors; no recovery is attempted within the class.
- **Performance**: Methods may be computationally intensive depending on data volume; consider asynchronous alternatives for large datasets.
