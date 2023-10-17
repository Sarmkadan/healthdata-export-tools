# CorrelationEngine

Provides statistical correlation analysis for health metric time series data. The engine computes Pearson and lagged correlations, extracts time series from raw data, and generates cross-metric insights. Designed for use within the healthdata-export-tools pipeline, it operates on structured metric collections and returns analysis results as Data Transfer Objects (DTOs).

## API

### `public sealed class CorrelationEngine`

A sealed class that cannot be inherited. Instances are created directly and hold no mutable state between method calls, but are not inherently thread-safe for concurrent use of instance methods.

---

### `public async Task<CorrelationAnalysisResultDto> AnalyzeAsync(…)`

Initiates a full correlation analysis on the provided data set.

- **Parameters**  
  `data` – The input data source (e.g., a stream, file path, or in-memory collection) containing health metric records.  
  `options` – (Optional) Analysis configuration such as time window, metric selection, or aggregation granularity.

- **Returns**  
  A `Task<CorrelationAnalysisResultDto>` that resolves to a DTO containing pairwise correlation matrices, summary statistics, and metadata.

- **Throws**  
  `ArgumentNullException` if required input is null.  
  `InvalidOperationException` if the data cannot be parsed or contains insufficient metrics for analysis.

---

### `public async Task<IReadOnlyList<CrossMetricInsightDto>> GetInsightsAsync(…)`

Asynchronously derives actionable insights by comparing correlation patterns across multiple metrics.

- **Parameters**  
  `analysisResult` – A `CorrelationAnalysisResultDto` previously obtained from `AnalyzeAsync`.  
  `threshold` – (Optional) A minimum correlation magnitude (e.g., 0.7) to consider a relationship noteworthy.

- **Returns**  
  A `Task<IReadOnlyList<CrossMetricInsightDto>>` containing a list of insights, each describing a pair of metrics, their correlation strength, and a human-readable interpretation.

- **Throws**  
  `ArgumentNullException` if `analysisResult` is null.  
  `ArgumentException` if `threshold` is outside the valid range [-1, 1].

---

### `public IReadOnlyList<MetricTimeSeries> ExtractTimeSeries(…)`

Extracts one or more time series from a raw data source without performing correlation.

- **Parameters**  
  `data` – The raw data (e.g., a `Stream`, `string` path, or `IEnumerable<MetricRecord>`).  
  `metricNames` – A collection of metric identifiers to extract. If empty, all available metrics are returned.

- **Returns**  
  An `IReadOnlyList<MetricTimeSeries>` where each element contains a metric name, a sorted list of timestamps, and corresponding values.

- **Throws**  
  `ArgumentNullException` if `data` is null.  
  `InvalidDataException` if the data format is unrecognized or required fields are missing.

---

### `public double ComputePearsonCorrelation(…)`

Calculates the Pearson product-moment correlation coefficient between two equal-length numeric sequences.

- **Parameters**  
  `series1` – First sequence of `double` values.  
  `series2` – Second sequence of `double` values.

- **Returns**  
  A `double` between -1 and 1 inclusive. Returns `double.NaN` if either sequence has zero variance or contains fewer than two data points.

- **Throws**  
  `ArgumentNullException` if either sequence is null.  
  `ArgumentException` if the sequences have different lengths.

---

### `public double ComputeLaggedCorrelation(…)`

Computes the Pearson correlation between two series after shifting one by a specified lag.

- **Parameters**  
  `series1` – First sequence of `double` values (the reference).  
  `series2` – Second sequence of `double` values (shifted by `lag`).  
  `lag` – An integer number of time steps to shift `series2` forward (positive) or backward (negative). The effective length of the overlapping portion is used.

- **Returns**  
  A `double` representing the correlation at the given lag. Returns `double.NaN` if the overlapping segment has fewer than two points or zero variance.

- **Throws**  
  `ArgumentNullException` if either series is null.  
  `ArgumentOutOfRangeException` if the absolute lag exceeds the length of either series.

---

### `public IReadOnlyList<LaggedCorrelationResult> AnalyzeLag(…)`

Evaluates correlation across a range of lags and returns the results for each lag.

- **Parameters**  
  `series1` – First sequence of `double` values.  
  `series2` – Second sequence of `double` values.  
  `maxLag` – Maximum absolute lag to examine (non-negative integer).  
  `minLag` – (Optional) Minimum absolute lag; defaults to 0.

- **Returns**  
  An `IReadOnlyList<LaggedCorrelationResult>`, each containing the lag value, the computed correlation, and the number of overlapping data points used.

- **Throws**  
  `ArgumentNullException` if either series is null.  
  `ArgumentOutOfRangeException` if `maxLag` is negative or `minLag` > `maxLag`.  
  `ArgumentException` if the series have different lengths.

## Usage

### Example 1: Basic correlation analysis with insights

```csharp
using HealthDataExportTools.Correlation;

var engine = new CorrelationEngine();

// Load raw data (e.g., from a CSV file)
using var stream = File.OpenRead("metrics_2024.csv");
var analysis = await engine.AnalyzeAsync(stream);

// Get insights for correlations above 0.8
var insights = await engine.GetInsightsAsync(analysis, threshold: 0.8);

foreach (var insight in insights)
{
    Console.WriteLine($"{insight.MetricA} ↔ {insight.MetricB}: {insight.Correlation:F3} ({insight.Description})");
}
```

### Example 2: Manual lag analysis on extracted time series

```csharp
using HealthDataExportTools.Correlation;

var engine = new CorrelationEngine();
var records = new List<MetricRecord>
{
    new MetricRecord { Timestamp = DateTime.UtcNow.AddHours(-2), Metric = "heart_rate", Value = 72 },
    new MetricRecord { Timestamp = DateTime.UtcNow.AddHours(-1), Metric = "heart_rate", Value = 78 },
    new MetricRecord { Timestamp = DateTime.UtcNow, Metric = "heart_rate", Value = 74 },
    // ... additional records for "steps" metric
};

var series = engine.ExtractTimeSeries(records, new[] { "heart_rate", "steps" });
var hrSeries = series.First(s => s.MetricName == "heart_rate").Values;
var stepsSeries = series.First(s => s.MetricName == "steps").Values;

// Check if steps correlate with heart rate at a 1-hour lag
var lagResult = engine.ComputeLaggedCorrelation(hrSeries, stepsSeries, lag: 1);
Console.WriteLine($"Correlation at lag 1 hour: {lagResult:F4}");

// Full lag profile
var lagProfile = engine.AnalyzeLag(hrSeries, stepsSeries, maxLag: 3);
foreach (var entry in lagProfile)
{
    Console.WriteLine($"Lag {entry.Lag}: r = {entry.Correlation:F4} (n = {entry.Count})");
}
```

## Notes

- **Thread safety**: `CorrelationEngine` instances are not thread-safe. Concurrent calls to any instance method from multiple threads may produce undefined behavior. Use separate instances or external synchronization when operating in parallel.
- **Empty or constant series**: `ComputePearsonCorrelation`, `ComputeLaggedCorrelation`, and `AnalyzeLag` return `double.NaN` when either series has zero variance (all values identical) or the overlapping segment contains fewer than two points. Callers should check for `double.IsNaN` before interpreting results.
- **Missing data**: The engine does not impute missing values. Time series extracted via `ExtractTimeSeries` contain only the timestamps and values present in the source. For correlation methods, series must be aligned in time; the caller is responsible for alignment (e.g., by resampling or interpolation) before passing sequences to the engine.
- **Large datasets**: `AnalyzeAsync` and `GetInsightsAsync` are asynchronous and designed for I/O-bound operations. CPU-bound correlation computations (`ComputePearsonCorrelation`, `ComputeLaggedCorrelation`, `AnalyzeLag`) are synchronous and may block the calling thread for long series.
- **Lag interpretation**: In `ComputeLaggedCorrelation` and `AnalyzeLag`, a positive lag shifts `series2` forward in time relative to `series1`. A correlation at lag +1 means `series2` at time *t* correlates with `series1` at time *t-1*.
