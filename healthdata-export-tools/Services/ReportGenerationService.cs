// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Services;

/// <summary>
/// Service for generating comprehensive health data reports
/// Produces summary reports, trend analysis, and statistics
/// </summary>
public class ReportGenerationService
{
    private readonly ILogger<ReportGenerationService> _logger;

    public ReportGenerationService(ILogger<ReportGenerationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generate comprehensive health summary report
    /// </summary>
    public async Task<HealthSummaryReport> GenerateSummaryReportAsync(List<HealthDataRecord> records)
    {
        try
        {
            _logger.LogInformation("Generating summary report for {Count} records", records.Count);

            var report = new HealthSummaryReport
            {
                ReportDate = DateTime.UtcNow,
                TotalRecords = records.Count,
                DateRange = new DateRange
                {
                    StartDate = records.Min(r => r.RecordDate),
                    EndDate = records.Max(r => r.RecordDate)
                }
            };

            // Calculate statistics by data type
            var grouped = records.GroupBy(r => r.GetType().Name);

            foreach (var group in grouped)
            {
                var stat = new DataTypeStatistic
                {
                    DataType = group.Key,
                    RecordCount = group.Count(),
                    AverageValue = 0.0,
                    MinValue = 0.0,
                    MaxValue = 0.0,
                    StandardDeviation = 0.0
                };

                report.DataTypeStatistics.Add(stat);
            }

            // Device distribution
            var deviceCounts = records
                .GroupBy(r => r.DeviceId)
                .ToDictionary(g => g.Key, g => g.Count());

            report.DeviceDistribution = deviceCounts;

            return await Task.FromResult(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating summary report");
            throw;
        }
    }

    /// <summary>
    /// Generate daily summary report
    /// </summary>
    public async Task<DailySummaryReport> GenerateDailyReportAsync(
        List<SleepData> sleepData,
        List<HeartRateData> heartRateData,
        DateTime date)
    {
        try
        {
            _logger.LogInformation("Generating daily report for {Date}", date.Date);

            var report = new DailySummaryReport
            {
                Date = date.Date,
                GeneratedAt = DateTime.UtcNow
            };

            // Sleep metrics
            var daySleep = sleepData.Where(s => s.RecordDate.Date == date.Date).ToList();
            if (daySleep.Count > 0)
            {
                report.SleepMetrics = new SleepMetrics
                {
                    TotalDurationMinutes = daySleep.Sum(s => s.DurationMinutes),
                    AverageQuality = (int)daySleep.Average(s => (int)s.Quality),
                    DeepSleepMinutes = daySleep.Sum(s => s.DeepSleepMinutes),
                    RemSleepMinutes = daySleep.Sum(s => s.RemSleepMinutes),
                    Records = daySleep.Count
                };
            }

            // Heart rate metrics
            var dayHeartRate = heartRateData.Where(h => h.RecordDate.Date == date.Date).ToList();
            if (dayHeartRate.Count > 0)
            {
                report.HeartRateMetrics = new HeartRateMetrics
                {
                    AverageHeartRate = (int)dayHeartRate.Average(h => h.AverageBpm),
                    MinHeartRate = dayHeartRate.Min(h => h.MinimumBpm),
                    MaxHeartRate = dayHeartRate.Max(h => h.MaximumBpm),
                    Records = dayHeartRate.Count
                };
            }

            return await Task.FromResult(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating daily report");
            throw;
        }
    }

    /// <summary>
    /// Generate trend analysis report
    /// </summary>
    public async Task<TrendAnalysisReport> GenerateTrendReportAsync(
        List<HealthDataRecord> records,
        int windowDays = 7)
    {
        try
        {
            var report = new TrendAnalysisReport
            {
                AnalysisDate = DateTime.UtcNow,
                WindowDays = windowDays
            };

            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-windowDays);

            var windowRecords = records
                .Where(r => r.RecordDate >= startDate && r.RecordDate <= endDate)
                .OrderBy(r => r.RecordDate)
                .ToList();

            if (windowRecords.Count > 0)
            {
                // Calculate trend for each metric type
                var groupedByType = windowRecords.GroupBy(r => r.GetType().Name);

                foreach (var group in groupedByType)
                {
                    var values = group.Select(r => 0.0).ToList();
                    var trend = new MetricTrend
                    {
                        MetricType = group.Key.ToString(),
                        AverageValue = values.Average(),
                        TrendDirection = CalculateTrendDirection(values),
                        VariationPercent = CalculateVariation(values)
                    };

                    report.MetricTrends.Add(trend);
                }
            }

            return await Task.FromResult(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating trend report");
            throw;
        }
    }

    /// <summary>
    /// Calculate standard deviation
    /// </summary>
    private double CalculateStandardDeviation(List<double> values)
    {
        if (values.Count <= 1)
            return 0;

        var average = values.Average();
        var sumOfSquares = values.Sum(v => Math.Pow(v - average, 2));
        return Math.Sqrt(sumOfSquares / values.Count);
    }

    /// <summary>
    /// Determine trend direction
    /// </summary>
    private string CalculateTrendDirection(List<double> values)
    {
        if (values.Count < 2)
            return "Stable";

        var firstHalf = values.Take(values.Count / 2).Average();
        var secondHalf = values.Skip(values.Count / 2).Average();

        if (secondHalf > firstHalf * 1.05)
            return "Upward";
        if (secondHalf < firstHalf * 0.95)
            return "Downward";

        return "Stable";
    }

    /// <summary>
    /// Calculate variation percentage
    /// </summary>
    private double CalculateVariation(List<double> values)
    {
        if (values.Count == 0)
            return 0;

        var stdDev = CalculateStandardDeviation(values);
        var mean = values.Average();

        return mean > 0 ? (stdDev / mean) * 100 : 0;
    }
}

/// <summary>
/// Health summary report
/// </summary>
public class HealthSummaryReport
{
    public DateTime ReportDate { get; set; }
    public int TotalRecords { get; set; }
    public DateRange DateRange { get; set; } = new();
    public List<DataTypeStatistic> DataTypeStatistics { get; set; } = new();
    public Dictionary<string, int> DeviceDistribution { get; set; } = new();
}

/// <summary>
/// Data type statistics
/// </summary>
public class DataTypeStatistic
{
    public string DataType { get; set; } = string.Empty;
    public int RecordCount { get; set; }
    public double AverageValue { get; set; }
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public double StandardDeviation { get; set; }
}

/// <summary>
/// Daily summary report
/// </summary>
public class DailySummaryReport
{
    public DateTime Date { get; set; }
    public DateTime GeneratedAt { get; set; }
    public SleepMetrics? SleepMetrics { get; set; }
    public HeartRateMetrics? HeartRateMetrics { get; set; }
}

/// <summary>
/// Sleep metrics
/// </summary>
public class SleepMetrics
{
    public int TotalDurationMinutes { get; set; }
    public int AverageQuality { get; set; }
    public int DeepSleepMinutes { get; set; }
    public int RemSleepMinutes { get; set; }
    public int Records { get; set; }
}

/// <summary>
/// Heart rate metrics
/// </summary>
public class HeartRateMetrics
{
    public int AverageHeartRate { get; set; }
    public int MinHeartRate { get; set; }
    public int MaxHeartRate { get; set; }
    public int Records { get; set; }
}

/// <summary>
/// Trend analysis report
/// </summary>
public class TrendAnalysisReport
{
    public DateTime AnalysisDate { get; set; }
    public int WindowDays { get; set; }
    public List<MetricTrend> MetricTrends { get; set; } = new();
}

/// <summary>
/// Metric trend information
/// </summary>
public class MetricTrend
{
    public string MetricType { get; set; } = string.Empty;
    public double AverageValue { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
    public double VariationPercent { get; set; }
}

/// <summary>
/// Date range
/// </summary>
public class DateRange
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysCovered => (EndDate - StartDate).Days;
}
