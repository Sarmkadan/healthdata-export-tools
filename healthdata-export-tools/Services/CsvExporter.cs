#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using CsvHelper;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Exceptions;

namespace HealthDataExportTools.Services;

/// <summary>
/// Exports a <see cref="HealthDataCollection"/> to per-data-type CSV files.
/// Supports column selection via <see cref="CsvExportOptions"/> and consistent
/// ISO 8601 date formatting.
/// </summary>
public sealed class CsvExporter : IHealthDataExporter
{
    private readonly ILogger<CsvExporter> _logger;

    public CsvExporter(ILogger<CsvExporter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task ExportToCsvAsync(
        HealthDataCollection collection,
        string outputDirectory,
        CsvExportOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        options ??= new CsvExportOptions();

        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        var tasks = new List<Task>();

        if (options.IncludeSleep && collection.SleepRecords.Count > 0)
        {
            var path = Path.Combine(outputDirectory, "sleep.csv");
            tasks.Add(ExportSleepAsync(collection.SleepRecords, path, options));
        }

        if (options.IncludeHeartRate && collection.HeartRateRecords.Count > 0)
        {
            var path = Path.Combine(outputDirectory, "heart_rate.csv");
            tasks.Add(ExportHeartRateAsync(collection.HeartRateRecords, path, options));
        }

        if (options.IncludeSpO2 && collection.SpO2Records.Count > 0)
        {
            var path = Path.Combine(outputDirectory, "spo2.csv");
            tasks.Add(ExportSpO2Async(collection.SpO2Records, path, options));
        }

        if (options.IncludeSteps && collection.StepsRecords.Count > 0)
        {
            var path = Path.Combine(outputDirectory, "steps.csv");
            tasks.Add(ExportStepsAsync(collection.StepsRecords, path, options));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        _logger.LogInformation(
            "CSV export complete: {FileCount} file(s) written to {OutputDirectory}",
            tasks.Count, outputDirectory);
    }

    private async Task ExportSleepAsync(
        List<SleepData> records,
        string outputPath,
        CsvExportOptions options)
    {
        var columns = new HashSet<string>(
            options.SleepColumns ?? Array.Empty<string>(),
            StringComparer.OrdinalIgnoreCase);

        bool all = columns.Count == 0;

        try
        {
            using var fs = File.Create(outputPath);
            using var writer = new StreamWriter(fs, Encoding.UTF8);
            using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

            WriteFieldIf(csv, all || columns.Contains("Date"),         "Date");
            WriteFieldIf(csv, all || columns.Contains("Duration"),     "Duration");
            WriteFieldIf(csv, all || columns.Contains("DeepSleep"),    "DeepSleep");
            WriteFieldIf(csv, all || columns.Contains("LightSleep"),   "LightSleep");
            WriteFieldIf(csv, all || columns.Contains("REM"),          "REM");
            WriteFieldIf(csv, all || columns.Contains("Awake"),        "Awake");
            WriteFieldIf(csv, all || columns.Contains("Quality"),      "Quality");
            WriteFieldIf(csv, all || columns.Contains("Score"),        "Score");
            WriteFieldIf(csv, all || columns.Contains("AvgHeartRate"), "AvgHeartRate");
            await csv.NextRecordAsync().ConfigureAwait(false);

            foreach (var r in records)
            {
                WriteFieldIf(csv, all || columns.Contains("Date"),         r.RecordDate.ToString(options.DateFormat, CultureInfo.InvariantCulture));
                WriteFieldIf(csv, all || columns.Contains("Duration"),     r.DurationMinutes);
                WriteFieldIf(csv, all || columns.Contains("DeepSleep"),    r.DeepSleepMinutes);
                WriteFieldIf(csv, all || columns.Contains("LightSleep"),   r.LightSleepMinutes);
                WriteFieldIf(csv, all || columns.Contains("REM"),          r.RemSleepMinutes);
                WriteFieldIf(csv, all || columns.Contains("Awake"),        r.AwakeMinutes);
                WriteFieldIf(csv, all || columns.Contains("Quality"),      r.Quality.ToString());
                WriteFieldIf(csv, all || columns.Contains("Score"),        r.Score.HasValue ? (object)r.Score.Value : null);
                WriteFieldIf(csv, all || columns.Contains("AvgHeartRate"), r.AverageHeartRate.HasValue ? (object)r.AverageHeartRate.Value : null);
                await csv.NextRecordAsync().ConfigureAwait(false);
            }
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write sleep CSV", outputPath, "CSV", ex);
        }
    }

    private async Task ExportHeartRateAsync(
        List<HeartRateData> records,
        string outputPath,
        CsvExportOptions options)
    {
        var columns = new HashSet<string>(
            options.HeartRateColumns ?? Array.Empty<string>(),
            StringComparer.OrdinalIgnoreCase);

        bool all = columns.Count == 0;

        try
        {
            using var fs = File.Create(outputPath);
            using var writer = new StreamWriter(fs, Encoding.UTF8);
            using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

            WriteFieldIf(csv, all || columns.Contains("Date"),         "Date");
            WriteFieldIf(csv, all || columns.Contains("MinBpm"),       "MinBpm");
            WriteFieldIf(csv, all || columns.Contains("MaxBpm"),       "MaxBpm");
            WriteFieldIf(csv, all || columns.Contains("AvgBpm"),       "AvgBpm");
            WriteFieldIf(csv, all || columns.Contains("RestingBpm"),   "RestingBpm");
            WriteFieldIf(csv, all || columns.Contains("Measurements"), "Measurements");
            WriteFieldIf(csv, all || columns.Contains("StressLevel"),  "StressLevel");
            WriteFieldIf(csv, all || columns.Contains("CardioZone"),   "CardioZone");
            WriteFieldIf(csv, all || columns.Contains("Zone1Minutes"), "Zone1Minutes");
            WriteFieldIf(csv, all || columns.Contains("Zone2Minutes"), "Zone2Minutes");
            WriteFieldIf(csv, all || columns.Contains("Zone3Minutes"), "Zone3Minutes");
            WriteFieldIf(csv, all || columns.Contains("Zone4Minutes"), "Zone4Minutes");
            WriteFieldIf(csv, all || columns.Contains("Zone5Minutes"), "Zone5Minutes");
            await csv.NextRecordAsync().ConfigureAwait(false);

            foreach (var r in records)
            {
                WriteFieldIf(csv, all || columns.Contains("Date"),         r.RecordDate.ToString(options.DateFormat, CultureInfo.InvariantCulture));
                WriteFieldIf(csv, all || columns.Contains("MinBpm"),       r.MinimumBpm);
                WriteFieldIf(csv, all || columns.Contains("MaxBpm"),       r.MaximumBpm);
                WriteFieldIf(csv, all || columns.Contains("AvgBpm"),       r.AverageBpm);
                WriteFieldIf(csv, all || columns.Contains("RestingBpm"),   r.RestingBpm.HasValue ? (object)r.RestingBpm.Value : null);
                WriteFieldIf(csv, all || columns.Contains("Measurements"), r.MeasurementCount);
                WriteFieldIf(csv, all || columns.Contains("StressLevel"),  r.StressLevel.HasValue ? (object)r.StressLevel.Value : null);
                WriteFieldIf(csv, all || columns.Contains("CardioZone"),   r.CardioZoneMinutes);
                WriteFieldIf(csv, all || columns.Contains("Zone1Minutes"), r.ZoneMinutes[0]);
                WriteFieldIf(csv, all || columns.Contains("Zone2Minutes"), r.ZoneMinutes[1]);
                WriteFieldIf(csv, all || columns.Contains("Zone3Minutes"), r.ZoneMinutes[2]);
                WriteFieldIf(csv, all || columns.Contains("Zone4Minutes"), r.ZoneMinutes[3]);
                WriteFieldIf(csv, all || columns.Contains("Zone5Minutes"), r.ZoneMinutes[4]);
                await csv.NextRecordAsync().ConfigureAwait(false);
            }
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write heart rate CSV", outputPath, "CSV", ex);
        }
    }

    private async Task ExportSpO2Async(
        List<SpO2Data> records,
        string outputPath,
        CsvExportOptions options)
    {
        var columns = new HashSet<string>(
            options.SpO2Columns ?? Array.Empty<string>(),
            StringComparer.OrdinalIgnoreCase);

        bool all = columns.Count == 0;

        try
        {
            using var fs = File.Create(outputPath);
            using var writer = new StreamWriter(fs, Encoding.UTF8);
            using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

            WriteFieldIf(csv, all || columns.Contains("Date"),           "Date");
            WriteFieldIf(csv, all || columns.Contains("MinPercentage"),  "MinPercentage");
            WriteFieldIf(csv, all || columns.Contains("MaxPercentage"),  "MaxPercentage");
            WriteFieldIf(csv, all || columns.Contains("AvgPercentage"),  "AvgPercentage");
            WriteFieldIf(csv, all || columns.Contains("Measurements"),   "Measurements");
            await csv.NextRecordAsync().ConfigureAwait(false);

            foreach (var r in records)
            {
                WriteFieldIf(csv, all || columns.Contains("Date"),           r.RecordDate.ToString(options.DateFormat, CultureInfo.InvariantCulture));
                WriteFieldIf(csv, all || columns.Contains("MinPercentage"),  r.MinimumPercentage);
                WriteFieldIf(csv, all || columns.Contains("MaxPercentage"),  r.MaximumPercentage);
                WriteFieldIf(csv, all || columns.Contains("AvgPercentage"),  r.AveragePercentage);
                WriteFieldIf(csv, all || columns.Contains("Measurements"),   r.MeasurementCount);
                await csv.NextRecordAsync().ConfigureAwait(false);
            }
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write SpO2 CSV", outputPath, "CSV", ex);
        }
    }

    private async Task ExportStepsAsync(
        List<StepsData> records,
        string outputPath,
        CsvExportOptions options)
    {
        var columns = new HashSet<string>(
            options.StepsColumns ?? Array.Empty<string>(),
            StringComparer.OrdinalIgnoreCase);

        bool all = columns.Count == 0;

        try
        {
            using var fs = File.Create(outputPath);
            using var writer = new StreamWriter(fs, Encoding.UTF8);
            using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

            WriteFieldIf(csv, all || columns.Contains("Date"),            "Date");
            WriteFieldIf(csv, all || columns.Contains("Steps"),           "Steps");
            WriteFieldIf(csv, all || columns.Contains("DistanceKm"),      "DistanceKm");
            WriteFieldIf(csv, all || columns.Contains("Calories"),        "Calories");
            WriteFieldIf(csv, all || columns.Contains("GoalAchievement"), "GoalAchievement");
            WriteFieldIf(csv, all || columns.Contains("ActiveMinutes"),   "ActiveMinutes");
            WriteFieldIf(csv, all || columns.Contains("Walking"),         "Walking");
            WriteFieldIf(csv, all || columns.Contains("Running"),         "Running");
            await csv.NextRecordAsync().ConfigureAwait(false);

            foreach (var r in records)
            {
                WriteFieldIf(csv, all || columns.Contains("Date"),            r.RecordDate.ToString(options.DateFormat, CultureInfo.InvariantCulture));
                WriteFieldIf(csv, all || columns.Contains("Steps"),           r.TotalSteps);
                WriteFieldIf(csv, all || columns.Contains("DistanceKm"),      r.DistanceKm);
                WriteFieldIf(csv, all || columns.Contains("Calories"),        r.CaloriesBurned);
                WriteFieldIf(csv, all || columns.Contains("GoalAchievement"), r.GoalAchievementPercentage);
                WriteFieldIf(csv, all || columns.Contains("ActiveMinutes"),   r.ActiveMinutes);
                WriteFieldIf(csv, all || columns.Contains("Walking"),         r.WalkingMinutes);
                WriteFieldIf(csv, all || columns.Contains("Running"),         r.RunningMinutes);
                await csv.NextRecordAsync().ConfigureAwait(false);
            }
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write steps CSV", outputPath, "CSV", ex);
        }
    }

    private static void WriteFieldIf(CsvWriter csv, bool condition, object? value)
    {
        if (condition)
            csv.WriteField(value);
    }
}
