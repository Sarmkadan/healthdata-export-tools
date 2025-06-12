// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.DTOs;

/// <summary>
/// A single data point that was flagged as anomalous during Z-score analysis.
/// </summary>
public class AnomalyPoint
{
    /// <summary>Date of the anomalous measurement.</summary>
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    /// <summary>Measured value at the time of the anomaly.</summary>
    [JsonPropertyName("value")]
    public double Value { get; set; }

    /// <summary>Z-score indicating how many standard deviations the value sits from the mean.</summary>
    [JsonPropertyName("zScore")]
    public double ZScore { get; set; }

    /// <summary>Absolute deviation of the value from the window mean.</summary>
    [JsonPropertyName("deviationFromMean")]
    public double DeviationFromMean { get; set; }

    /// <summary>Severity classification: <c>Minor</c> (|z|&lt;2.5), <c>Moderate</c> (|z|&lt;3.0), or <c>Severe</c> (|z|≥3.0).</summary>
    [JsonPropertyName("severity")]
    public string Severity { get; set; } = "Minor";
}

/// <summary>
/// Trend direction and anomaly summary for a single health metric over a rolling window.
/// </summary>
public class MetricTrendResult
{
    /// <summary>Name of the health metric (e.g. <c>HeartRate</c>, <c>SpO2</c>, <c>Steps</c>, <c>SleepDuration</c>).</summary>
    [JsonPropertyName("metricName")]
    public string MetricName { get; set; } = string.Empty;

    /// <summary>Trend direction: <c>Improving</c>, <c>Declining</c>, <c>Stable</c>, or <c>Insufficient Data</c>.</summary>
    [JsonPropertyName("trendStatus")]
    public string TrendStatus { get; set; } = "Stable";

    /// <summary>Percentage change between the first and second halves of the analysis window.</summary>
    [JsonPropertyName("percentChange")]
    public double PercentChange { get; set; }

    /// <summary>Mean value over the analysis window.</summary>
    [JsonPropertyName("mean")]
    public double Mean { get; set; }

    /// <summary>Sample standard deviation over the analysis window.</summary>
    [JsonPropertyName("standardDeviation")]
    public double StandardDeviation { get; set; }

    /// <summary>Number of data points included in the analysis.</summary>
    [JsonPropertyName("sampleCount")]
    public int SampleCount { get; set; }

    /// <summary>Number of days covered by the analysis window.</summary>
    [JsonPropertyName("analysisWindowDays")]
    public int AnalysisWindowDays { get; set; }

    /// <summary>Anomalous data points identified within the window.</summary>
    [JsonPropertyName("anomalies")]
    public List<AnomalyPoint> Anomalies { get; set; } = new();

    /// <summary>UTC timestamp when this metric result was computed.</summary>
    [JsonPropertyName("analysedAt")]
    public DateTime AnalysedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Whether at least one anomaly was detected for this metric.</summary>
    [JsonPropertyName("hasAnomalies")]
    public bool HasAnomalies => Anomalies.Count > 0;
}

/// <summary>
/// Full anomaly detection report produced by <see cref="HealthDataExportTools.Services.TrendAnomalyDetectionService"/>,
/// covering all tracked health metrics for a given rolling window.
/// </summary>
public class AnomalyDetectionResultDto
{
    /// <summary>Unique identifier for this detection report.</summary>
    [JsonPropertyName("reportId")]
    public string ReportId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>UTC timestamp when the report was generated.</summary>
    [JsonPropertyName("generatedAt")]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Analysis window in days passed to the service.</summary>
    [JsonPropertyName("windowDays")]
    public int WindowDays { get; set; }

    /// <summary>Per-metric trend and anomaly results.</summary>
    [JsonPropertyName("metrics")]
    public List<MetricTrendResult> Metrics { get; set; } = new();

    /// <summary>Total number of anomalies found across all metrics.</summary>
    [JsonPropertyName("totalAnomalies")]
    public int TotalAnomalies => Metrics.Sum(m => m.Anomalies.Count);

    /// <summary>
    /// Overall health status derived from anomaly count:
    /// <c>Healthy</c> (0), <c>Monitor</c> (1–3), or <c>Alert</c> (&gt;3).
    /// </summary>
    [JsonPropertyName("overallStatus")]
    public string OverallStatus => TotalAnomalies switch
    {
        0 => "Healthy",
        <= 3 => "Monitor",
        _ => "Alert",
    };

    /// <summary>Convenience flag indicating whether any anomalies were found.</summary>
    [JsonPropertyName("requiresAttention")]
    public bool RequiresAttention => TotalAnomalies > 0;
}
