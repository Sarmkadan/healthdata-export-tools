// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools.DTOs;

namespace HealthDataExportTools.Services;

/// <summary>
/// Detects statistical anomalies and computes directional trends across all tracked health
/// metrics using Z-score analysis over a configurable rolling window.
/// </summary>
public class TrendAnomalyDetectionService
{
    private readonly ILogger<TrendAnomalyDetectionService> _logger;

    /// <summary>Minimum data points required before trend or anomaly analysis is attempted.</summary>
    public const int MinimumSampleCount = 5;

    /// <summary>
    /// Initialise a new instance of <see cref="TrendAnomalyDetectionService"/>.
    /// </summary>
    /// <param name="logger">Logger instance provided by the DI container.</param>
    public TrendAnomalyDetectionService(ILogger<TrendAnomalyDetectionService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Run trend and anomaly analysis across all metrics in a <see cref="HealthDataCollection"/>.
    /// Each metric is processed concurrently and the results are collected into a single report.
    /// </summary>
    /// <param name="collection">Health data to analyse.</param>
    /// <param name="days">Rolling window in days (default 30).</param>
    /// <param name="zScoreThreshold">
    /// Z-score magnitude at or above which a data point is flagged as anomalous (default 2.0).
    /// </param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>An <see cref="AnomalyDetectionResultDto"/> summarising findings per metric.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is <c>null</c>.</exception>
    /// <exception cref="HealthDataException">Thrown when an unexpected error occurs during analysis.</exception>
    public async Task<AnomalyDetectionResultDto> AnalyzeAsync(
        HealthDataCollection collection,
        int days = 30,
        double zScoreThreshold = 2.0,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(collection);

        _logger.LogInformation(
            "Starting trend and anomaly analysis — window: {Days}d, threshold: {Threshold}",
            days, zScoreThreshold);

        var report = new AnomalyDetectionResultDto { WindowDays = days };

        try
        {
            var tasks = new[]
            {
                Task.Run(() => Analyze(collection.HeartRateRecords, "HeartRate",
                    r => (double)r.AverageBpm, days, zScoreThreshold), cancellationToken),
                Task.Run(() => Analyze(collection.SpO2Records, "SpO2",
                    r => (double)r.AveragePercentage, days, zScoreThreshold), cancellationToken),
                Task.Run(() => Analyze(collection.StepsRecords, "Steps",
                    r => (double)r.TotalSteps, days, zScoreThreshold), cancellationToken),
                Task.Run(() => Analyze(collection.SleepRecords.Where(r => !r.IsNap), "SleepDuration",
                    r => (double)r.DurationMinutes, days, zScoreThreshold), cancellationToken),
            };

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            report.Metrics.AddRange(results);

            _logger.LogInformation(
                "Analysis complete — {Anomalies} anomalies across {Metrics} metrics",
                report.TotalAnomalies, report.Metrics.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Trend and anomaly analysis was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during trend and anomaly analysis");
            throw new HealthDataException("Trend and anomaly analysis failed", "ANALYSIS_ERROR");
        }

        return report;
    }

    /// <summary>
    /// Detect anomalies and classify the trend direction for an ordered time series.
    /// Exposed as <c>public</c> to allow direct unit testing of the core algorithm.
    /// </summary>
    /// <param name="metricName">Display name used in the returned result.</param>
    /// <param name="points">Chronologically ordered (date, value) pairs within the analysis window.</param>
    /// <param name="days">Window size used to populate <see cref="MetricTrendResult.AnalysisWindowDays"/>.</param>
    /// <param name="zScoreThreshold">Z-score magnitude at or above which a point is flagged.</param>
    /// <returns>A <see cref="MetricTrendResult"/> describing trend direction and any detected anomalies.</returns>
    public MetricTrendResult ComputeTrendAndAnomalies(
        string metricName,
        IReadOnlyList<(DateTime Date, double Value)> points,
        int days,
        double zScoreThreshold)
    {
        var result = new MetricTrendResult
        {
            MetricName = metricName,
            SampleCount = points.Count,
            AnalysisWindowDays = days,
        };

        if (points.Count < MinimumSampleCount)
        {
            result.TrendStatus = "Insufficient Data";
            return result;
        }

        var values = points.Select(p => p.Value).ToList();
        var mean = values.Average();
        var stdDev = ComputeStandardDeviation(values, mean);

        result.Mean = mean;
        result.StandardDeviation = stdDev;

        if (stdDev > 0)
        {
            foreach (var (date, value) in points)
            {
                var z = (value - mean) / stdDev;
                if (Math.Abs(z) < zScoreThreshold) continue;

                result.Anomalies.Add(new AnomalyPoint
                {
                    Date = date,
                    Value = value,
                    ZScore = z,
                    DeviationFromMean = value - mean,
                    Severity = Math.Abs(z) switch
                    {
                        >= 3.0 => "Severe",
                        >= 2.5 => "Moderate",
                        _ => "Minor",
                    },
                });
            }
        }

        var mid = points.Count / 2;
        var firstHalf = points.Take(mid).Average(p => p.Value);
        var secondHalf = points.Skip(mid).Average(p => p.Value);

        if (Math.Abs(firstHalf) > double.Epsilon)
            result.PercentChange = ((secondHalf - firstHalf) / Math.Abs(firstHalf)) * 100;

        result.TrendStatus = result.PercentChange switch
        {
            > 10 => "Improving",
            < -10 => "Declining",
            _ => "Stable",
        };

        return result;
    }

    private MetricTrendResult Analyze<T>(
        IEnumerable<T> records,
        string metricName,
        Func<T, double> valueSelector,
        int days,
        double threshold) where T : HealthDataRecord
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var pts = records.Where(r => r.RecordDate >= cutoff)
            .OrderBy(r => r.RecordDate)
            .Select(r => (r.RecordDate, valueSelector(r)))
            .ToList();

        return ComputeTrendAndAnomalies(metricName, pts, days, threshold);
    }

    private static double ComputeStandardDeviation(IReadOnlyList<double> values, double mean)
    {
        if (values.Count < 2) return 0;
        return Math.Sqrt(values.Sum(v => Math.Pow(v - mean, 2)) / (values.Count - 1));
    }
}
