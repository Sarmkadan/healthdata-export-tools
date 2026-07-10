# AnomalyPoint

`AnomalyPoint` represents a single detected anomaly within a health data metric time series. It captures the timestamp, observed value, statistical deviation metrics (Z‑score, deviation from mean, percent change), contextual baseline statistics (mean, standard deviation, sample count), severity classification, trend status, and metadata linking it to a specific analysis report. Instances are typically produced as part of a batch analysis run and may also contain nested anomaly collections and associated metric trend results.

## API

### `public DateTime Date`
The timestamp of the data point that was flagged as anomalous. This reflects the original observation time in the source health data series.

### `public double Value`
The raw numeric value recorded at `Date` for the associated metric.

### `public double ZScore`
The number of standard deviations the `Value` lies from the baseline `Mean`. Positive values indicate the observation is above the mean; negative values indicate it is below. Commonly used to rank anomaly strength.

### `public double DeviationFromMean`
The absolute arithmetic difference between `Value` and `Mean` (`Value - Mean`). Retains the sign of the deviation.

### `public string Severity`
A label describing the severity level of the anomaly. Typical values are implementation‑defined (e.g. `"Low"`, `"Medium"`, `"High"`, `"Critical"`) and are derived from thresholds applied to `ZScore` or `PercentChange`.

### `public string MetricName`
The name of the health metric to which this anomaly belongs (e.g. `"HeartRate"`, `"StepCount"`, `"BloodPressureSystolic"`). Used to correlate anomalies across different metrics within the same report.

### `public string TrendStatus`
A string indicating the directional trend classification at the time of the anomaly. May contain values such as `"Increasing"`, `"Decreasing"`, `"Stable"`, or `"Unknown"`, depending on the trend analysis logic applied to the metric’s recent window.

### `public double PercentChange`
The percentage change of `Value` relative to the baseline `Mean`, typically computed as `((Value - Mean) / Mean) * 100`. May be zero or negative. Behaviour when `Mean` is zero is implementation‑defined (callers should guard against division by zero if constructing manually).

### `public double Mean`
The arithmetic mean of the baseline sample used for anomaly detection. This is the central value against which `Value` is compared.

### `public double StandardDeviation`
The standard deviation of the baseline sample. Used together with `Mean` to compute `ZScore`. A value of zero indicates no variance in the baseline window, which may affect the interpretability of `ZScore`.

### `public int SampleCount`
The number of data points included in the baseline window from which `Mean` and `StandardDeviation` were calculated.

### `public int AnalysisWindowDays`
The size of the lookback window, in days, that defined the baseline period for detecting this anomaly. This governs the temporal context of `SampleCount`, `Mean`, and `StandardDeviation`.

### `public List<AnomalyPoint> Anomalies`
A nested list of additional `AnomalyPoint` instances. This can be used to group sub‑anomalies or related anomalies detected within the same analysis scope. May be `null` or empty when no sub‑anomalies are present.

### `public DateTime AnalysedAt`
The timestamp when the anomaly detection analysis was executed for this point. Distinct from `Date`, which is the observation time of the source data.

### `public string ReportId`
A unique identifier for the analysis report that produced this anomaly. Used to correlate all anomalies and metric results belonging to a single analytical run.

### `public DateTime GeneratedAt`
The timestamp when the enclosing report was generated. This may differ from `AnalysedAt` if the report assembly occurs after individual metric analyses complete.

### `public int WindowDays`
The total window size, in days, used for the overall report analysis. This may match `AnalysisWindowDays` for a given anomaly but represents the report‑level configuration.

### `public List<MetricTrendResult> Metrics`
A list of `MetricTrendResult` objects representing trend analysis outcomes for metrics evaluated in the same report. Provides broader context beyond the single anomaly, allowing consumers to correlate the anomaly with the overall trend of its metric and other metrics.

## Usage

### Example 1: Iterating over anomalies in a report and logging high‑severity points

```csharp
AnomalyPoint anomaly = GetAnomalyFromReport(); // obtained from a report deserialization or API

if (anomaly.Severity == "High" || anomaly.Severity == "Critical")
{
    Console.WriteLine(
        $"High‑severity anomaly in {anomaly.MetricName} on {anomaly.Date:yyyy-MM-dd}: " +
        $"Value = {anomaly.Value:F2}, ZScore = {anomaly.ZScore:F2}, " +
        $"Deviation = {anomaly.DeviationFromMean:F2}, Trend = {anomaly.TrendStatus}");
}

// Also inspect nested anomalies
if (anomaly.Anomalies != null)
{
    foreach (var subAnomaly in anomaly.Anomalies)
    {
        Console.WriteLine(
            $"  Sub‑anomaly: {subAnomaly.MetricName} at {subAnomaly.Date}, Severity = {subAnomaly.Severity}");
    }
}
```

### Example 2: Building a summary view across all metrics in a report

```csharp
AnomalyPoint reportAnomaly = LoadReportAnomaly("report-123");

Console.WriteLine($"Report {reportAnomaly.ReportId} generated at {reportAnomaly.GeneratedAt}");
Console.WriteLine($"Analysis window: {reportAnomaly.WindowDays} days");

if (reportAnomaly.Metrics != null)
{
    foreach (var trend in reportAnomaly.Metrics)
    {
        Console.WriteLine(
            $"Metric: {trend.MetricName}, Trend: {trend.TrendStatus}, " +
            $"Anomaly count: {trend.Anomalies?.Count ?? 0}");
    }
}

// Focus on the primary anomaly
Console.WriteLine(
    $"Primary anomaly: {reportAnomaly.MetricName}, Date: {reportAnomaly.Date}, " +
    $"ZScore: {reportAnomaly.ZScore:F2}, PercentChange: {reportAnomaly.PercentChange:F1}%");
```

## Notes

- **Nested anomalies**: The `Anomalies` list can contain further `AnomalyPoint` instances, potentially forming a tree. Recursive processing should guard against infinite loops if circular references are inadvertently introduced during construction (though normal report generation produces acyclic structures).
- **Statistical edge cases**: When `StandardDeviation` is zero, `ZScore` becomes effectively infinite or undefined depending on the calculation implementation. Consumers should test `StandardDeviation` before relying on `ZScore` for ranking. Similarly, `PercentChange` is undefined when `Mean` is zero; callers constructing instances manually must handle this to avoid division‑by‑zero exceptions.
- **Nullability**: Reference‑type members (`Severity`, `MetricName`, `TrendStatus`, `ReportId`, `Anomalies`, `Metrics`) may be `null` if not populated by the analysis pipeline. Defensive null checks are recommended before iterating over collections or calling string operations.
- **DateTime precision**: All `DateTime` members use the standard .NET `DateTime` precision (ticks). Timestamps may be expressed in UTC or local time depending on the data source; consumers should not assume a specific `Kind` without inspecting the value.
- **Thread safety**: `AnomalyPoint` is a plain data object and provides no intrinsic synchronisation. Concurrent reads from multiple threads are safe provided no thread mutates the instance or its list members (`Anomalies`, `Metrics`) after publication. Concurrent modification of shared lists requires external locking.
- **Immutability considerations**: The type exposes public setters (implied by the public property signatures). In practice, instances are typically populated once during analysis and then treated as read‑only snapshots. Modifying properties on a shared instance after it has been distributed to consumers can lead to inconsistent views; prefer replacing the instance or creating a copy when changes are needed.
