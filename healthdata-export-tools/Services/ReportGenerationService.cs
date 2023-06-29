#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Services;

/// <summary>
/// Service for generating comprehensive health data reports
/// Produces summary reports, trend analysis, and statistics
/// </summary>
public sealed class ReportGenerationService
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
                    StartDate = records.Count > 0 ? records.Min(r => r.RecordDate) : default,
                    EndDate = records.Count > 0 ? records.Max(r => r.RecordDate) : default
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

            return await Task.FromResult(report).ConfigureAwait(false);
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

            return await Task.FromResult(report).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating daily report");
            throw;
        }
    }

    /// <summary>
    /// Generate weekly summary reports from health records
    /// </summary>
    public async Task<List<WeeklySummaryReport>> GenerateWeeklySummaryReportAsync(
        List<SleepData> sleepData,
        List<HeartRateData> heartRateData,
        List<StepsData> stepsData)
        => await GenerateWeeklySummaryReportAsync(sleepData, heartRateData, stepsData,
               new List<SpO2Data>(), new List<ActivityData>()).ConfigureAwait(false);

    /// <summary>
    /// Generate weekly summary reports including SpO2 and activity data.
    /// Each report covers one ISO calendar week and includes week-over-week
    /// percentage changes for every tracked metric.
    /// </summary>
    /// <param name="sleepData">Sleep records.</param>
    /// <param name="heartRateData">Heart rate records.</param>
    /// <param name="stepsData">Steps records.</param>
    /// <param name="spo2Data">SpO2 records.</param>
    /// <param name="activityData">Activity records.</param>
    public async Task<List<WeeklySummaryReport>> GenerateWeeklySummaryReportAsync(
        List<SleepData> sleepData,
        List<HeartRateData> heartRateData,
        List<StepsData> stepsData,
        List<SpO2Data> spo2Data,
        List<ActivityData> activityData)
    {
        try
        {
            _logger.LogInformation("Generating weekly reports...");
            var reports = new List<WeeklySummaryReport>();

            var allDates = sleepData.Select(s => s.RecordDate.Date)
                .Concat(heartRateData.Select(h => h.RecordDate.Date))
                .Concat(stepsData.Select(st => st.RecordDate.Date))
                .Concat(spo2Data.Select(sp => sp.RecordDate.Date))
                .Concat(activityData.Select(a => a.RecordDate.Date))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            if (allDates.Count == 0) return await Task.FromResult(reports).ConfigureAwait(false);

            var groupedByWeek = allDates.GroupBy(d =>
                System.Globalization.ISOWeek.GetYear(d) + "-" +
                System.Globalization.ISOWeek.GetWeekOfYear(d).ToString("D2"));

            foreach (var weekGroup in groupedByWeek.OrderBy(g => g.Key))
            {
                var datesInWeek = weekGroup.ToList();
                var startDate = datesInWeek.Min();
                var endDate   = datesInWeek.Max();

                var weeklySleep    = sleepData.Where(s => s.RecordDate.Date >= startDate && s.RecordDate.Date <= endDate).ToList();
                var weeklyHR       = heartRateData.Where(h => h.RecordDate.Date >= startDate && h.RecordDate.Date <= endDate).ToList();
                var weeklySteps    = stepsData.Where(st => st.RecordDate.Date >= startDate && st.RecordDate.Date <= endDate).ToList();
                var weeklySpO2     = spo2Data.Where(sp => sp.RecordDate.Date >= startDate && sp.RecordDate.Date <= endDate).ToList();
                var weeklyActivity = activityData.Where(a => a.RecordDate.Date >= startDate && a.RecordDate.Date <= endDate).ToList();

                var report = new WeeklySummaryReport
                {
                    WeekIdentifier  = weekGroup.Key,
                    StartDate       = startDate,
                    EndDate         = endDate,
                    GeneratedAt     = DateTime.UtcNow,
                    DaysWithData    = datesInWeek.Count
                };

                if (weeklySleep.Count > 0)
                {
                    report.AverageSleepDurationMinutes = (int)weeklySleep.Average(s => s.DurationMinutes);
                    report.AverageSleepQuality         = (int)weeklySleep.Average(s => (int)s.Quality);
                    report.TotalDeepSleepMinutes       = weeklySleep.Sum(s => s.DeepSleepMinutes);
                    report.TotalRemSleepMinutes        = weeklySleep.Sum(s => s.RemSleepMinutes);
                    report.ExcellentSleepNights        = weeklySleep.Count(s => s.Quality == Domain.Enums.SleepQuality.Excellent);
                }

                if (weeklyHR.Count > 0)
                {
                    report.AverageHeartRate  = (int)weeklyHR.Average(h => h.AverageBpm);
                    report.MinimumHeartRate  = weeklyHR.Min(h => h.MinimumBpm);
                    report.MaximumHeartRate  = weeklyHR.Max(h => h.MaximumBpm);
                    var withStress = weeklyHR.Where(h => h.StressLevel.HasValue).ToList();
                    report.AverageStressLevel = withStress.Count > 0
                        ? (int)withStress.Average(h => h.StressLevel!.Value)
                        : 0;
                }

                if (weeklySteps.Count > 0)
                {
                    report.TotalSteps       = weeklySteps.Sum(st => st.TotalSteps);
                    report.TotalDistanceKm  = weeklySteps.Sum(st => st.DistanceKm);
                    report.TotalCaloriesBurned += weeklySteps.Sum(st => st.CaloriesBurned);
                    report.GoalAchievedDays = weeklySteps.Count(st => st.GoalAchieved);
                }

                if (weeklySpO2.Count > 0)
                {
                    report.AverageSpO2        = (int)weeklySpO2.Average(sp => sp.AveragePercentage);
                    report.MinimumSpO2        = weeklySpO2.Min(sp => sp.MinimumPercentage);
                    report.TotalLowSpO2Events = weeklySpO2.Sum(sp => sp.LowSpO2Events);
                }

                if (weeklyActivity.Count > 0)
                {
                    report.TotalActivitySessions  = weeklyActivity.Count;
                    report.TotalActivityMinutes   = weeklyActivity.Sum(a => a.DurationMinutes);
                    report.TotalCaloriesBurned   += weeklyActivity.Sum(a => a.CaloriesBurned);
                    report.TotalActivityDistanceKm = weeklyActivity.Sum(a => a.DistanceKm);
                }

                report.WeeklyHealthScore = CalculateWeeklyHealthScore(report);
                reports.Add(report);
            }

            // Attach week-over-week changes
            for (int i = 1; i < reports.Count; i++)
                reports[i].Changes = ComputeWeekOverWeekChanges(reports[i - 1], reports[i]);

            return await Task.FromResult(reports).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating weekly summary report");
            throw;
        }
    }

    /// <summary>
    /// Export a list of weekly summary reports to a JSON file.
    /// </summary>
    /// <param name="reports">The weekly reports to serialize.</param>
    /// <param name="outputPath">Destination file path.</param>
    public async Task ExportWeeklySummaryToJsonAsync(List<WeeklySummaryReport> reports, string outputPath)
    {
        try
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var payload = new
            {
                ExportDate   = DateTime.UtcNow.ToString("o"),
                WeekCount    = reports.Count,
                WeeklyReports = reports
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload, options);
            await File.WriteAllTextAsync(outputPath, json, System.Text.Encoding.UTF8).ConfigureAwait(false);
            _logger.LogInformation("Weekly summary exported to {Path}", outputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting weekly summary to JSON");
            throw;
        }
    }

    /// <summary>
    /// Compute a simple 0-100 health score for a single week based on its aggregated metrics.
    /// </summary>
    private static int CalculateWeeklyHealthScore(WeeklySummaryReport report)
    {
        int score = 50;

        if (report.AverageSleepDurationMinutes > 0)
        {
            var hours = report.AverageSleepDurationMinutes / 60.0;
            if (hours >= 7 && hours <= 9)        score += 15;
            else if (hours >= 6.5)               score += 8;
        }

        if (report.AverageHeartRate > 0)
        {
            if (report.AverageHeartRate < 70)      score += 10;
            else if (report.AverageHeartRate < 85)  score += 5;
        }

        if (report.AverageSpO2 > 0)
        {
            if (report.AverageSpO2 >= 97)           score += 10;
            else if (report.AverageSpO2 >= 95)       score += 6;
        }

        if (report.TotalSteps > 0)
        {
            var dailyAvg = report.TotalSteps / Math.Max(report.DaysWithData, 1);
            if (dailyAvg >= 10000)                  score += 15;
            else if (dailyAvg >= 7000)               score += 8;
        }

        return Math.Min(score, 100);
    }

    /// <summary>
    /// Compute percentage change for each key metric between two consecutive weekly reports.
    /// </summary>
    private static WeekOverWeekChanges ComputeWeekOverWeekChanges(
        WeeklySummaryReport previous,
        WeeklySummaryReport current)
    {
        static double Pct(double prev, double curr) =>
            Math.Abs(prev) < 0.001 ? 0.0 : Math.Round((curr - prev) / prev * 100.0, 2);

        return new WeekOverWeekChanges
        {
            SleepDurationChangePercent  = Pct(previous.AverageSleepDurationMinutes, current.AverageSleepDurationMinutes),
            HeartRateChangePercent      = Pct(previous.AverageHeartRate,            current.AverageHeartRate),
            StepsChangePercent          = Pct(previous.TotalSteps,                  current.TotalSteps),
            SpO2ChangePercent           = Pct(previous.AverageSpO2,                 current.AverageSpO2),
            ActivityMinutesChangePercent = Pct(previous.TotalActivityMinutes,       current.TotalActivityMinutes),
            HealthScoreChangePoints     = current.WeeklyHealthScore - previous.WeeklyHealthScore
        };
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

            return await Task.FromResult(report).ConfigureAwait(false);
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
        // Hotfix: Standard deviation is 0.0 for 0 or 1 samples to prevent InvalidOperationException/NaN.
        if (values.Count <= 1)
        {
            return 0.0;
        }

        var average = values.Average();
        var sumOfSquares = values.Sum(v => Math.Pow(v - average, 2));
        // Hotfix: Ensure values.Count is not 0 before division to prevent DivideByZeroException.
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
public sealed class HealthSummaryReport
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
public sealed class DataTypeStatistic
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
public sealed class DailySummaryReport
{
    public DateTime Date { get; set; }
    public DateTime GeneratedAt { get; set; }
    public SleepMetrics? SleepMetrics { get; set; }
    public HeartRateMetrics? HeartRateMetrics { get; set; }
}

/// <summary>
/// Sleep metrics
/// </summary>
public sealed class SleepMetrics
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
public sealed class HeartRateMetrics
{
    public int AverageHeartRate { get; set; }
    public int MinHeartRate { get; set; }
    public int MaxHeartRate { get; set; }
    public int Records { get; set; }
}

/// <summary>
/// Trend analysis report
/// </summary>
public sealed class TrendAnalysisReport
{
    public DateTime AnalysisDate { get; set; }
    public int WindowDays { get; set; }
    public List<MetricTrend> MetricTrends { get; set; } = new();
}

/// <summary>
/// Metric trend information
/// </summary>
public sealed class MetricTrend
{
    public string MetricType { get; set; } = string.Empty;
    public double AverageValue { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
    public double VariationPercent { get; set; }
}

/// <summary>
/// Date range
/// </summary>
public sealed class DateRange
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysCovered => (EndDate - StartDate).Days;
}

/// <summary>
/// Weekly summary report
/// </summary>
public sealed class WeeklySummaryReport
{
    /// <summary>ISO year-week identifier, e.g. "2024-03".</summary>
    public string WeekIdentifier { get; set; } = string.Empty;
    /// <summary>First day of the week covered by this report.</summary>
    public DateTime StartDate { get; set; }
    /// <summary>Last day of the week covered by this report.</summary>
    public DateTime EndDate { get; set; }
    /// <summary>Timestamp when the report was generated.</summary>
    public DateTime GeneratedAt { get; set; }
    /// <summary>Number of distinct days for which at least one record exists.</summary>
    public int DaysWithData { get; set; }

    // Sleep
    /// <summary>Average sleep duration across all nights in the week (minutes).</summary>
    public int AverageSleepDurationMinutes { get; set; }
    /// <summary>Average sleep quality score (mapped from <see cref="Domain.Enums.SleepQuality"/>).</summary>
    public int AverageSleepQuality { get; set; }
    /// <summary>Total deep-sleep minutes accumulated across the week.</summary>
    public int TotalDeepSleepMinutes { get; set; }
    /// <summary>Total REM-sleep minutes accumulated across the week.</summary>
    public int TotalRemSleepMinutes { get; set; }
    /// <summary>Number of nights rated as Excellent quality.</summary>
    public int ExcellentSleepNights { get; set; }

    // Heart rate
    /// <summary>Average daily average heart rate (BPM).</summary>
    public int AverageHeartRate { get; set; }
    /// <summary>Lowest single-day minimum heart rate in the week (BPM).</summary>
    public int MinimumHeartRate { get; set; }
    /// <summary>Highest single-day maximum heart rate in the week (BPM).</summary>
    public int MaximumHeartRate { get; set; }
    /// <summary>Average stress level (0–100) for days where stress data is available.</summary>
    public int AverageStressLevel { get; set; }

    // Steps
    /// <summary>Total steps for the week.</summary>
    public int TotalSteps { get; set; }
    /// <summary>Total distance covered via steps (km).</summary>
    public double TotalDistanceKm { get; set; }
    /// <summary>Number of days the daily step goal was achieved.</summary>
    public int GoalAchievedDays { get; set; }

    // SpO2
    /// <summary>Average daily average SpO2 percentage.</summary>
    public int AverageSpO2 { get; set; }
    /// <summary>Lowest single-day minimum SpO2 reading in the week.</summary>
    public int MinimumSpO2 { get; set; }
    /// <summary>Total number of low-SpO2 events recorded across the week.</summary>
    public int TotalLowSpO2Events { get; set; }

    // Activity
    /// <summary>Number of recorded activity sessions in the week.</summary>
    public int TotalActivitySessions { get; set; }
    /// <summary>Total active minutes across all activity sessions.</summary>
    public int TotalActivityMinutes { get; set; }
    /// <summary>Total distance covered during activity sessions (km).</summary>
    public double TotalActivityDistanceKm { get; set; }
    /// <summary>Combined calories from steps and activity sessions.</summary>
    public int TotalCaloriesBurned { get; set; }

    // Aggregated score
    /// <summary>Composite health score (0–100) computed from the week's metrics.</summary>
    public int WeeklyHealthScore { get; set; }

    /// <summary>Week-over-week metric changes compared to the preceding week. Null for the first week.</summary>
    public WeekOverWeekChanges? Changes { get; set; }
}

/// <summary>
/// Percentage changes in key metrics between two consecutive weekly reports.
/// </summary>
public sealed class WeekOverWeekChanges
{
    /// <summary>Percentage change in average sleep duration.</summary>
    public double SleepDurationChangePercent { get; set; }
    /// <summary>Percentage change in average heart rate.</summary>
    public double HeartRateChangePercent { get; set; }
    /// <summary>Percentage change in total steps.</summary>
    public double StepsChangePercent { get; set; }
    /// <summary>Percentage change in average SpO2.</summary>
    public double SpO2ChangePercent { get; set; }
    /// <summary>Percentage change in total activity minutes.</summary>
    public double ActivityMinutesChangePercent { get; set; }
    /// <summary>Point difference in weekly health score (positive = improvement).</summary>
    public int HealthScoreChangePoints { get; set; }
}
