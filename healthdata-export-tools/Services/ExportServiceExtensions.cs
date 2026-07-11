#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using CsvHelper;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Exceptions;

namespace HealthDataExportTools.Services;

/// <summary>
/// Extension methods for ExportService providing additional export functionality
/// </summary>
public static class ExportServiceExtensions
{
    /// <summary>
/// <summary>
/// Exports sleep data summary statistics to CSV with calculated metrics including duration, quality scores,\n/// and efficiency percentages for each sleep record.
/// </summary>
/// <param name="exportService">The ExportService instance.</param>
/// <param name="sleepRecords">Sleep records to export. Cannot be null or empty.</param>
/// <param name="outputPath">Destination CSV file path. Cannot be null or whitespace.</param>
/// <returns>Task representing the async operation.</returns>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepRecords"/> or <paramref name="outputPath"/> is null.</exception>
/// <exception cref="ArgumentException">Thrown when <paramref name="sleepRecords"/> is empty or <paramref name="outputPath"/> is whitespace.</exception>
/// <exception cref="ExportException">Thrown when file operations fail.</exception>
    public static async Task ExportSleepSummaryToCsvAsync(
        this ExportService exportService,
        IReadOnlyList<SleepData> sleepRecords,
        string outputPath)
    {
        ArgumentNullException.ThrowIfNull(sleepRecords);
    ArgumentNullException.ThrowIfNull(outputPath);

    if (sleepRecords.Count == 0)
        throw new ArgumentException("Sleep records cannot be empty", nameof(sleepRecords));

    if (string.IsNullOrWhiteSpace(outputPath))
        throw new ArgumentException("Output path cannot be null or whitespace", nameof(outputPath));

        try
        {
            await using var fs = File.Create(outputPath);
            await using var writer = new StreamWriter(fs, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteHeader<SleepSummaryCsvRecord>();
            await csv.NextRecordAsync().ConfigureAwait(false);

            // Calculate overall sleep metrics
            var totalDuration = sleepRecords.Sum(s => s.DurationMinutes);
            var totalDeepSleep = sleepRecords.Sum(s => s.DeepSleepMinutes);
            var totalLightSleep = sleepRecords.Sum(s => s.LightSleepMinutes);
            var totalRem = sleepRecords.Sum(s => s.RemSleepMinutes);
            var totalAwake = sleepRecords.Sum(s => s.AwakeMinutes);
            var avgScore = (int)Math.Round(sleepRecords.Average(s => s.Score ?? 0));

            // Write summary record
            var summaryRecord = new SleepSummaryCsvRecord
            {
                TotalRecords = sleepRecords.Count,
                TotalDurationMinutes = totalDuration,
                TotalDeepSleepMinutes = totalDeepSleep,
                TotalLightSleepMinutes = totalLightSleep,
                TotalRemMinutes = totalRem,
                TotalAwakeMinutes = totalAwake,
                AvgQuality = Math.Round(sleepRecords.Average(s => s.Quality == SleepQuality.Good ? 1 : 0) * 100, 2),
                AvgScore = avgScore,
                BestSleepDate = sleepRecords
                    .OrderByDescending(s => s.Score)
                    .FirstOrDefault()?.RecordDate.ToString("yyyy-MM-dd"),
                WorstSleepDate = sleepRecords
                    .OrderBy(s => s.Score)
                    .FirstOrDefault()?.RecordDate.ToString("yyyy-MM-dd"),
                DeepSleepPercentage = totalDuration > 0
                    ? (double)totalDeepSleep / totalDuration * 100
                    : 0,
                RemPercentage = totalDuration > 0
                    ? (double)totalRem / totalDuration * 100
                    : 0,
                EfficiencyPercentage = totalDuration > 0
                    ? (double)(totalDuration - totalAwake) / totalDuration * 100
                    : 0
            };

            csv.WriteRecord(summaryRecord);
            await csv.NextRecordAsync().ConfigureAwait(false);

            // Write individual records
            foreach (var sleep in sleepRecords.OrderByDescending(s => s.RecordDate))
            {
                var record = new SleepSummaryCsvRecord
                {
                    Date = sleep.RecordDate.ToString("yyyy-MM-dd"),
                    DurationMinutes = sleep.DurationMinutes,
                    DeepSleepMinutes = sleep.DeepSleepMinutes,
                    LightSleepMinutes = sleep.LightSleepMinutes,
                    REMMinutes = sleep.RemSleepMinutes,
                    AwakeMinutes = sleep.AwakeMinutes,
                    Quality = sleep.Quality.ToString(),
                    Score = sleep.Score,
                    AvgHeartRate = sleep.AverageHeartRate,
                    SleepEfficiency = sleep.DurationMinutes > 0
                        ? Math.Round((double)(sleep.DurationMinutes - sleep.AwakeMinutes) / sleep.DurationMinutes * 100, 2)
                        : 0
                };

                csv.WriteRecord(record);
                await csv.NextRecordAsync().ConfigureAwait(false);
            }
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write sleep summary CSV", outputPath, "CSV", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ExportException("Access denied when writing sleep summary CSV", outputPath, "CSV", ex);
        }
        catch (ArgumentException ex)
        {
            throw new ExportException("Invalid output path for sleep summary CSV", outputPath, "CSV", ex);
        }
    }

    /// <summary>
/// <summary>
/// Exports heart rate analytics to CSV with zone distribution, stress levels, and heart rate variability metrics.
/// </summary>
/// <param name="exportService">The ExportService instance.</param>
/// <param name="records">Heart rate records to export. Cannot be null or empty.</param>
/// <param name="outputPath">Destination CSV file path. Cannot be null or whitespace.</param>
/// <param name="maxHeartRate">Users maximum heart rate for zone classification. Must be greater than zero.</param>
/// <returns>Task representing the async operation.</returns>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="records"/> or <paramref name="outputPath"/> is null.</exception>
/// <exception cref="ArgumentException">Thrown when <paramref name="records"/> is empty or <paramref name="outputPath"/> is whitespace.</exception>
/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxHeartRate"/> is less than or equal to zero.</exception>
/// <exception cref="ExportException">Thrown when file operations fail.</exception>
    public static async Task ExportHeartRateAnalyticsToCsvAsync(
        this ExportService exportService,
        IReadOnlyList<HeartRateData> records,
        string outputPath,
        int maxHeartRate)
    {
        ArgumentNullException.ThrowIfNull(records);
    ArgumentNullException.ThrowIfNull(outputPath);

    if (records.Count == 0)
        throw new ArgumentException("Heart rate records cannot be empty", nameof(records));

    if (string.IsNullOrWhiteSpace(outputPath))
        throw new ArgumentException("Output path cannot be null or whitespace", nameof(outputPath));

        if (maxHeartRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxHeartRate), "Max heart rate must be greater than zero.");

        try
        {
            await using var fs = File.Create(outputPath);
            await using var writer = new StreamWriter(fs, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteHeader<HeartRateAnalyticsCsvRecord>();
            await csv.NextRecordAsync().ConfigureAwait(false);

            // Calculate overall statistics
            var avgBpm = (int)Math.Round(records.Average(r => r.AverageBpm));
            var minBpm = records.Min(r => r.MinimumBpm);
            var maxBpm = records.Max(r => r.MaximumBpm);
            var restingBpm = records.Average(r => r.RestingBpm ?? 0) is double restingAvg ? (int)Math.Round(restingAvg) : 0;

            // Write summary record
            var summaryRecord = new HeartRateAnalyticsCsvRecord
            {
                TotalRecords = records.Count,
                AvgBpm = avgBpm,
                MinBpm = minBpm,
                MaxBpm = maxBpm,
                RestingBpm = restingBpm,
                TotalCardioZoneMinutes = records.Sum(r => r.CardioZoneMinutes),
                TotalZone1Minutes = records.Sum(r => r.ZoneMinutes[0]),
                TotalZone2Minutes = records.Sum(r => r.ZoneMinutes[1]),
                TotalZone3Minutes = records.Sum(r => r.ZoneMinutes[2]),
                TotalZone4Minutes = records.Sum(r => r.ZoneMinutes[3]),
                TotalZone5Minutes = records.Sum(r => r.ZoneMinutes[4]),
                AvgStressLevel = (int)records.Average(r => r.StressLevel ?? 0),
                BestDayDate = records
                    .OrderBy(r => r.AverageBpm)
                    .FirstOrDefault()?.RecordDate.ToString("yyyy-MM-dd"),
                WorstDayDate = records
                    .OrderByDescending(r => r.AverageBpm)
                    .FirstOrDefault()?.RecordDate.ToString("yyyy-MM-dd")
            };

            csv.WriteRecord(summaryRecord);
            await csv.NextRecordAsync().ConfigureAwait(false);

            // Write individual records
            foreach (var hr in records.OrderBy(r => r.RecordDate))
            {
                var avgZone = HeartRateData.ClassifyZone(hr.AverageBpm, maxHeartRate);
                var record = new HeartRateAnalyticsCsvRecord
                {
                    Date = hr.RecordDate.ToString("yyyy-MM-dd"),
                    MinBpm = hr.MinimumBpm,
                    MaxBpm = hr.MaximumBpm,
                    AvgBpm = hr.AverageBpm,
                    RestingBpm = hr.RestingBpm,
                    Measurements = hr.MeasurementCount,
                    StressLevel = hr.StressLevel,
                    AvgZone = (int)avgZone,
                    Zone1Minutes = hr.ZoneMinutes[0],
                    Zone2Minutes = hr.ZoneMinutes[1],
                    Zone3Minutes = hr.ZoneMinutes[2],
                    Zone4Minutes = hr.ZoneMinutes[3],
                    Zone5Minutes = hr.ZoneMinutes[4],
                    HeartRateVariability = hr.MaximumBpm - hr.MinimumBpm,
                    IsRestingDay = hr.AverageBpm <= (maxHeartRate * 0.6)
                };

                csv.WriteRecord(record);
                await csv.NextRecordAsync().ConfigureAwait(false);
            }
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write heart rate analytics CSV", outputPath, "CSV", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ExportException("Access denied when writing heart rate analytics CSV", outputPath, "CSV", ex);
        }
        catch (ArgumentException ex)
        {
            throw new ExportException("Invalid output path for heart rate analytics CSV", outputPath, "CSV", ex);
        }
    }

    /// <summary>
    /// Export combined health metrics dashboard to CSV with all data types in one file
    /// </summary>
    /// <param name="exportService">The ExportService instance</param>
    /// <param name="collection">Health data collection</param>
    /// <param name="outputPath">Destination CSV file path</param>
    /// <returns>Task representing the async operation</returns>
    public static async Task ExportHealthDashboardToCsvAsync(
        this ExportService exportService,
        HealthDataCollection collection,
        string outputPath)
    {
    ArgumentNullException.ThrowIfNull(collection);
    ArgumentNullException.ThrowIfNull(outputPath);

    if (string.IsNullOrWhiteSpace(outputPath))
        throw new ArgumentException("Output path cannot be null or whitespace", nameof(outputPath));
        try
        {
            await using var fs = File.Create(outputPath);
            await using var writer = new StreamWriter(fs, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteHeader<HealthDashboardCsvRecord>();
            await csv.NextRecordAsync().ConfigureAwait(false);

            // Create records for each date with available data
            var allDates = new HashSet<DateTime>();

            if (collection.SleepRecords != null)
                foreach (var sleep in collection.SleepRecords)
                    allDates.Add(sleep.RecordDate.Date);

            if (collection.HeartRateRecords != null)
                foreach (var hr in collection.HeartRateRecords)
                    allDates.Add(hr.RecordDate.Date);

            if (collection.StepsRecords != null)
                foreach (var steps in collection.StepsRecords)
                    allDates.Add(steps.RecordDate.Date);

            var maxHeartRate = 180; // Default max heart rate if not specified in metrics

            foreach (var date in allDates.OrderBy(d => d))
            {
                var sleepData = collection.SleepRecords?.FirstOrDefault(s => s.RecordDate.Date == date);
                var hrData = collection.HeartRateRecords?.FirstOrDefault(h => h.RecordDate.Date == date);
                var stepsData = collection.StepsRecords?.FirstOrDefault(s => s.RecordDate.Date == date);

                var record = new HealthDashboardCsvRecord
                {
                    Date = date.ToString("yyyy-MM-dd"),

                    // Sleep metrics
                    SleepDuration = sleepData?.DurationMinutes,
                    DeepSleep = sleepData?.DeepSleepMinutes,
                    LightSleep = sleepData?.LightSleepMinutes,
                    REM = sleepData?.RemSleepMinutes,
                    Awake = sleepData?.AwakeMinutes,
                    SleepQuality = sleepData?.Quality.ToString(),
                    SleepScore = sleepData?.Score,

                    // Heart rate metrics
                    AvgHeartRate = hrData?.AverageBpm,
                    MinHeartRate = hrData?.MinimumBpm,
                    MaxHeartRate = hrData?.MaximumBpm,
                    RestingHeartRate = hrData?.RestingBpm,
                    HeartRateZones = string.Join(",", hrData?.ZoneMinutes.Select((z, i) => $"Zone{i+1}:{z}") ?? Array.Empty<string>()),

                    // Steps metrics
                    Steps = stepsData?.TotalSteps,
                    DistanceKm = stepsData?.DistanceKm,
                    CaloriesBurned = stepsData?.CaloriesBurned,
                    ActiveMinutes = stepsData?.ActiveMinutes,

                    // Calculated metrics
                    HasCompleteData = sleepData != null && hrData != null && stepsData != null,
                    SleepEfficiency = sleepData?.DurationMinutes > 0 && sleepData != null
                        ? (double)(sleepData.DurationMinutes - sleepData.AwakeMinutes) / sleepData.DurationMinutes * 100
                        : null,
                    HeartRateZone = hrData != null
                        ? (int)HeartRateData.ClassifyZone(hrData.AverageBpm, maxHeartRate)
                        : null
                };

                csv.WriteRecord(record);
                await csv.NextRecordAsync().ConfigureAwait(false);
            }
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write health dashboard CSV", outputPath, "CSV", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ExportException("Access denied when writing health dashboard CSV", outputPath, "CSV", ex);
        }
        catch (ArgumentException ex)
        {
            throw new ExportException("Invalid output path for health dashboard CSV", outputPath, "CSV", ex);
        }
    }

    /// <summary>
    /// Export data quality report to CSV identifying missing or incomplete records
    /// </summary>
    /// <param name="exportService">The ExportService instance</param>
    /// <param name="collection">Health data collection</param>
    /// <param name="outputPath">Destination CSV file path</param>
    /// <returns>Task representing the async operation</returns>
    public static async Task ExportDataQualityReportToCsvAsync(
        this ExportService exportService,
        HealthDataCollection collection,
        string outputPath)
    {
    ArgumentNullException.ThrowIfNull(collection);
    ArgumentNullException.ThrowIfNull(outputPath);

    if (string.IsNullOrWhiteSpace(outputPath))
        throw new ArgumentException("Output path cannot be null or whitespace", nameof(outputPath));
        try
        {
            await using var fs = File.Create(outputPath);
            await using var writer = new StreamWriter(fs, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteHeader<DataQualityReportCsvRecord>();
            await csv.NextRecordAsync().ConfigureAwait(false);

            var allDates = new HashSet<DateTime>();
            var dateSources = new Dictionary<DateTime, List<string>>();

            // Collect all dates and their available data sources
            if (collection.SleepRecords != null)
            {
                foreach (var sleep in collection.SleepRecords)
                {
                    var date = sleep.RecordDate.Date;
                    allDates.Add(date);
                    if (!dateSources.ContainsKey(date))
                        dateSources[date] = new List<string>();
                    dateSources[date].Add("Sleep");
                }
            }

            if (collection.HeartRateRecords != null)
            {
                foreach (var hr in collection.HeartRateRecords)
                {
                    var date = hr.RecordDate.Date;
                    allDates.Add(date);
                    if (!dateSources.ContainsKey(date))
                        dateSources[date] = new List<string>();
                    dateSources[date].Add("HeartRate");
                }
            }

            if (collection.StepsRecords != null)
            {
                foreach (var steps in collection.StepsRecords)
                {
                    var date = steps.RecordDate.Date;
                    allDates.Add(date);
                    if (!dateSources.ContainsKey(date))
                        dateSources[date] = new List<string>();
                    dateSources[date].Add("Steps");
                }
            }

            // Generate quality report
            foreach (var date in allDates.OrderBy(d => d))
            {
                var hasSleep = collection.SleepRecords?.Any(s => s.RecordDate.Date == date) ?? false;
                var hasHeartRate = collection.HeartRateRecords?.Any(h => h.RecordDate.Date == date) ?? false;
                var hasSteps = collection.StepsRecords?.Any(s => s.RecordDate.Date == date) ?? false;

                var record = new DataQualityReportCsvRecord
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    HasSleepData = hasSleep,
                    HasHeartRateData = hasHeartRate,
                    HasStepsData = hasSteps,
                    DataCompleteness = dateSources.ContainsKey(date) ? $"{dateSources[date].Count}/3" : "0/3",
                    MissingDataTypes = string.Join(", ",
                        new[] { hasSleep ? null : "Sleep", hasHeartRate ? null : "HeartRate", hasSteps ? null : "Steps" }
                            .Where(x => x != null) ?? Array.Empty<string>()),
                    QualityScore = CalculateDataQualityScore(hasSleep, hasHeartRate, hasSteps)
                };

                csv.WriteRecord(record);
                await csv.NextRecordAsync().ConfigureAwait(false);
            }
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write data quality report CSV", outputPath, "CSV", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ExportException("Access denied when writing data quality report CSV", outputPath, "CSV", ex);
        }
        catch (ArgumentException ex)
        {
            throw new ExportException("Invalid output path for data quality report CSV", outputPath, "CSV", ex);
        }
    }

    private static int CalculateDataQualityScore(bool hasSleep, bool hasHeartRate, bool hasSteps)
    {
        var score = 0;
        if (hasSleep) score += 33;
        if (hasHeartRate) score += 33;
        if (hasSteps) score += 34;
        return score;
    }
}

/// <summary>CSV record for sleep summary statistics</summary>
public sealed class SleepSummaryCsvRecord
{
    public int TotalRecords { get; set; }
    public int TotalDurationMinutes { get; set; }
    public int TotalDeepSleepMinutes { get; set; }
    public int TotalLightSleepMinutes { get; set; }
    public int TotalRemMinutes { get; set; }
    public int TotalAwakeMinutes { get; set; }
    public double? AvgQuality { get; set; }
    public int? AvgScore { get; set; }
    public string? BestSleepDate { get; set; }
    public string? WorstSleepDate { get; set; }
    public double DeepSleepPercentage { get; set; }
    public double RemPercentage { get; set; }
    public double EfficiencyPercentage { get; set; }

    // Individual record properties
    public string? Date { get; set; }
    public int DurationMinutes { get; set; }
    public int DeepSleepMinutes { get; set; }
    public int LightSleepMinutes { get; set; }
    public int REMMinutes { get; set; }
    public int AwakeMinutes { get; set; }
    public string? Quality { get; set; }
    public int? Score { get; set; }
    public int? AvgHeartRate { get; set; }
    public double SleepEfficiency { get; set; }
}

/// <summary>CSV record for heart rate analytics</summary>
public sealed class HeartRateAnalyticsCsvRecord
{
    public int TotalRecords { get; set; }
    public int AvgBpm { get; set; }
    public int MinBpm { get; set; }
    public int MaxBpm { get; set; }
    public int? RestingBpm { get; set; }
    public int TotalCardioZoneMinutes { get; set; }
    public int TotalZone1Minutes { get; set; }
    public int TotalZone2Minutes { get; set; }
    public int TotalZone3Minutes { get; set; }
    public int TotalZone4Minutes { get; set; }
    public int TotalZone5Minutes { get; set; }
    public int AvgStressLevel { get; set; }
    public string? BestDayDate { get; set; }
    public string? WorstDayDate { get; set; }

    // Individual record properties
    public string? Date { get; set; }
    public int Measurements { get; set; }
    public int? StressLevel { get; set; }
    public int AvgZone { get; set; }
    public int Zone1Minutes { get; set; }
    public int Zone2Minutes { get; set; }
    public int Zone3Minutes { get; set; }
    public int Zone4Minutes { get; set; }
    public int Zone5Minutes { get; set; }
    public int HeartRateVariability { get; set; }
    public bool IsRestingDay { get; set; }
}

/// <summary>CSV record for combined health dashboard</summary>
public sealed class HealthDashboardCsvRecord
{
    public string? Date { get; set; }
    public int? SleepDuration { get; set; }
    public int? DeepSleep { get; set; }
    public int? LightSleep { get; set; }
    public int? REM { get; set; }
    public int? Awake { get; set; }
    public string? SleepQuality { get; set; }
    public int? SleepScore { get; set; }
    public int? AvgHeartRate { get; set; }
    public int? MinHeartRate { get; set; }
    public int? MaxHeartRate { get; set; }
    public int? RestingHeartRate { get; set; }
    public string? HeartRateZones { get; set; }
    public int? Steps { get; set; }
    public double? DistanceKm { get; set; }
    public int? CaloriesBurned { get; set; }
    public int? ActiveMinutes { get; set; }
    public bool? HasCompleteData { get; set; }
    public double? SleepEfficiency { get; set; }
    public int? HeartRateZone { get; set; }
}

/// <summary>CSV record for data quality report</summary>
public sealed class DataQualityReportCsvRecord
{
    public string? Date { get; set; }
    public bool HasSleepData { get; set; }
    public bool HasHeartRateData { get; set; }
    public bool HasStepsData { get; set; }
    public string? DataCompleteness { get; set; }
    public string? MissingDataTypes { get; set; }
    public int QualityScore { get; set; }
}