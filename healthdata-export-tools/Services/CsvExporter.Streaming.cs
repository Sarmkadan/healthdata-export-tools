#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using HealthDataExportTools.Domain.Models;
using Microsoft.Extensions.Logging;

namespace HealthDataExportTools.Services;

public sealed partial class CsvExporter
{
    /// <summary>
    /// Export all enabled data types from <paramref name="collection"/> to a single CSV stream asynchronously.
    /// This streaming approach avoids building the entire payload in memory, making it suitable for large datasets.
    /// </summary>
    /// <param name="collection">The health data to export.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="options">
    /// Optional export options controlling which data types and columns to include,
    /// and the date format to use. When <c>null</c>, all data types are exported
    /// with ISO 8601 date formatting.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is <c>null</c>.</exception>
    public async Task ExportToCsvStreamAsync(
        HealthDataCollection collection,
        TextWriter writer,
        CsvExportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(writer);

        options ??= new CsvExportOptions();

        var columns = new HashSet<string>(
            options.SleepColumns ?? Array.Empty<string>(),
            StringComparer.OrdinalIgnoreCase);

        bool all = columns.Count == 0;

        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        // Write sleep data if enabled
        if (options.IncludeSleep && collection.SleepRecords.Count > 0)
        {
            WriteFieldIf(csv, all || columns.Contains("Date"), "Date");
            WriteFieldIf(csv, all || columns.Contains("Duration"), "Duration");
            WriteFieldIf(csv, all || columns.Contains("DeepSleep"), "DeepSleep");
            WriteFieldIf(csv, all || columns.Contains("LightSleep"), "LightSleep");
            WriteFieldIf(csv, all || columns.Contains("REM"), "REM");
            WriteFieldIf(csv, all || columns.Contains("Awake"), "Awake");
            WriteFieldIf(csv, all || columns.Contains("Quality"), "Quality");
            WriteFieldIf(csv, all || columns.Contains("Score"), "Score");
            WriteFieldIf(csv, all || columns.Contains("AvgHeartRate"), "AvgHeartRate");
            await csv.NextRecordAsync().ConfigureAwait(false);

            foreach (var r in collection.SleepRecords)
            {
                cancellationToken.ThrowIfCancellationRequested();

                WriteFieldIf(csv, all || columns.Contains("Date"), r.RecordDate.ToString(options.DateFormat, CultureInfo.InvariantCulture));
                WriteFieldIf(csv, all || columns.Contains("Duration"), r.DurationMinutes);
                WriteFieldIf(csv, all || columns.Contains("DeepSleep"), r.DeepSleepMinutes);
                WriteFieldIf(csv, all || columns.Contains("LightSleep"), r.LightSleepMinutes);
                WriteFieldIf(csv, all || columns.Contains("REM"), r.RemSleepMinutes);
                WriteFieldIf(csv, all || columns.Contains("Awake"), r.AwakeMinutes);
                WriteFieldIf(csv, all || columns.Contains("Quality"), r.Quality.ToString());
                WriteFieldIf(csv, all || columns.Contains("Score"), r.Score.HasValue ? (object)r.Score.Value : null);
                WriteFieldIf(csv, all || columns.Contains("AvgHeartRate"), r.AverageHeartRate.HasValue ? (object)r.AverageHeartRate.Value : null);
                await csv.NextRecordAsync().ConfigureAwait(false);
            }
        }

        // Write heart rate data if enabled
        if (options.IncludeHeartRate && collection.HeartRateRecords.Count > 0)
        {
            if (collection.SleepRecords.Count > 0 || options.IncludeSleep)
            {
                await csv.NextRecordAsync().ConfigureAwait(false); // Add separator between data types
            }

            columns = new HashSet<string>(
                options.HeartRateColumns ?? Array.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
            all = columns.Count == 0;

            WriteFieldIf(csv, all || columns.Contains("Date"), "Date");
            WriteFieldIf(csv, all || columns.Contains("MinBpm"), "MinBpm");
            WriteFieldIf(csv, all || columns.Contains("MaxBpm"), "MaxBpm");
            WriteFieldIf(csv, all || columns.Contains("AvgBpm"), "AvgBpm");
            WriteFieldIf(csv, all || columns.Contains("RestingBpm"), "RestingBpm");
            WriteFieldIf(csv, all || columns.Contains("Measurements"), "Measurements");
            WriteFieldIf(csv, all || columns.Contains("StressLevel"), "StressLevel");
            WriteFieldIf(csv, all || columns.Contains("CardioZone"), "CardioZone");
            WriteFieldIf(csv, all || columns.Contains("Zone1Minutes"), "Zone1Minutes");
            WriteFieldIf(csv, all || columns.Contains("Zone2Minutes"), "Zone2Minutes");
            WriteFieldIf(csv, all || columns.Contains("Zone3Minutes"), "Zone3Minutes");
            WriteFieldIf(csv, all || columns.Contains("Zone4Minutes"), "Zone4Minutes");
            WriteFieldIf(csv, all || columns.Contains("Zone5Minutes"), "Zone5Minutes");
            await csv.NextRecordAsync().ConfigureAwait(false);

            foreach (var r in collection.HeartRateRecords)
            {
                cancellationToken.ThrowIfCancellationRequested();

                WriteFieldIf(csv, all || columns.Contains("Date"), r.RecordDate.ToString(options.DateFormat, CultureInfo.InvariantCulture));
                WriteFieldIf(csv, all || columns.Contains("MinBpm"), r.MinimumBpm);
                WriteFieldIf(csv, all || columns.Contains("MaxBpm"), r.MaximumBpm);
                WriteFieldIf(csv, all || columns.Contains("AvgBpm"), r.AverageBpm);
                WriteFieldIf(csv, all || columns.Contains("RestingBpm"), r.RestingBpm.HasValue ? (object)r.RestingBpm.Value : null);
                WriteFieldIf(csv, all || columns.Contains("Measurements"), r.MeasurementCount);
                WriteFieldIf(csv, all || columns.Contains("StressLevel"), r.StressLevel.HasValue ? (object)r.StressLevel.Value : null);
                WriteFieldIf(csv, all || columns.Contains("CardioZone"), r.CardioZoneMinutes);
                WriteFieldIf(csv, all || columns.Contains("Zone1Minutes"), r.ZoneMinutes[0]);
                WriteFieldIf(csv, all || columns.Contains("Zone2Minutes"), r.ZoneMinutes[1]);
                WriteFieldIf(csv, all || columns.Contains("Zone3Minutes"), r.ZoneMinutes[2]);
                WriteFieldIf(csv, all || columns.Contains("Zone4Minutes"), r.ZoneMinutes[3]);
                WriteFieldIf(csv, all || columns.Contains("Zone5Minutes"), r.ZoneMinutes[4]);
                await csv.NextRecordAsync().ConfigureAwait(false);
            }
        }

        // Write SpO2 data if enabled
        if (options.IncludeSpO2 && collection.SpO2Records.Count > 0)
        {
            if (collection.HeartRateRecords.Count > 0 || options.IncludeHeartRate)
            {
                await csv.NextRecordAsync().ConfigureAwait(false); // Add separator between data types
            }

            columns = new HashSet<string>(
                options.SpO2Columns ?? Array.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
            all = columns.Count == 0;

            WriteFieldIf(csv, all || columns.Contains("Date"), "Date");
            WriteFieldIf(csv, all || columns.Contains("MinPercentage"), "MinPercentage");
            WriteFieldIf(csv, all || columns.Contains("MaxPercentage"), "MaxPercentage");
            WriteFieldIf(csv, all || columns.Contains("AvgPercentage"), "AvgPercentage");
            WriteFieldIf(csv, all || columns.Contains("Measurements"), "Measurements");
            await csv.NextRecordAsync().ConfigureAwait(false);

            foreach (var r in collection.SpO2Records)
            {
                cancellationToken.ThrowIfCancellationRequested();

                WriteFieldIf(csv, all || columns.Contains("Date"), r.RecordDate.ToString(options.DateFormat, CultureInfo.InvariantCulture));
                WriteFieldIf(csv, all || columns.Contains("MinPercentage"), r.MinimumPercentage);
                WriteFieldIf(csv, all || columns.Contains("MaxPercentage"), r.MaximumPercentage);
                WriteFieldIf(csv, all || columns.Contains("AvgPercentage"), r.AveragePercentage);
                WriteFieldIf(csv, all || columns.Contains("Measurements"), r.MeasurementCount);
                await csv.NextRecordAsync().ConfigureAwait(false);
            }
        }

        // Write steps data if enabled
        if (options.IncludeSteps && collection.StepsRecords.Count > 0)
        {
            if (collection.SpO2Records.Count > 0 || options.IncludeSpO2)
            {
                await csv.NextRecordAsync().ConfigureAwait(false); // Add separator between data types
            }

            columns = new HashSet<string>(
                options.StepsColumns ?? Array.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
            all = columns.Count == 0;

            WriteFieldIf(csv, all || columns.Contains("Date"), "Date");
            WriteFieldIf(csv, all || columns.Contains("Steps"), "Steps");
            WriteFieldIf(csv, all || columns.Contains("DistanceKm"), "DistanceKm");
            WriteFieldIf(csv, all || columns.Contains("Calories"), "Calories");
            WriteFieldIf(csv, all || columns.Contains("GoalAchievement"), "GoalAchievement");
            WriteFieldIf(csv, all || columns.Contains("ActiveMinutes"), "ActiveMinutes");
            WriteFieldIf(csv, all || columns.Contains("Walking"), "Walking");
            WriteFieldIf(csv, all || columns.Contains("Running"), "Running");
            await csv.NextRecordAsync().ConfigureAwait(false);

            foreach (var r in collection.StepsRecords)
            {
                cancellationToken.ThrowIfCancellationRequested();

                WriteFieldIf(csv, all || columns.Contains("Date"), r.RecordDate.ToString(options.DateFormat, CultureInfo.InvariantCulture));
                WriteFieldIf(csv, all || columns.Contains("Steps"), r.TotalSteps);
                WriteFieldIf(csv, all || columns.Contains("DistanceKm"), r.DistanceKm);
                WriteFieldIf(csv, all || columns.Contains("Calories"), r.CaloriesBurned);
                WriteFieldIf(csv, all || columns.Contains("GoalAchievement"), r.GoalAchievementPercentage);
                WriteFieldIf(csv, all || columns.Contains("ActiveMinutes"), r.ActiveMinutes);
                WriteFieldIf(csv, all || columns.Contains("Walking"), r.WalkingMinutes);
                WriteFieldIf(csv, all || columns.Contains("Running"), r.RunningMinutes);
                await csv.NextRecordAsync().ConfigureAwait(false);
            }
        }

        await writer.FlushAsync().ConfigureAwait(false);

        _logger.LogInformation(
            "CSV stream export complete: {TotalRecords} records written",
            collection.GetTotalRecordCount());
    }
}