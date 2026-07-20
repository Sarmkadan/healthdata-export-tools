#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using System.Text.Json;
using CsvHelper;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Exceptions;
using HealthDataExportTools.Utilities;

namespace HealthDataExportTools.Services;

/// <summary>
/// Service for exporting health data to various formats
/// </summary>
public sealed class ExportService
{
    /// <summary>
    /// Export health data collection to JSON
    /// </summary>
    public async Task ExportToJsonAsync(HealthDataCollection collection, string outputPath)
    {
        try
        {
            var data = new
            {
                ExportDate = DateTime.UtcNow.ToIso8601(),
                TotalRecords = collection.GetTotalRecordCount(),
                SleepRecords = collection.SleepRecords,
                HeartRateRecords = collection.HeartRateRecords,
                SpO2Records = collection.SpO2Records,
                StepsRecords = collection.StepsRecords,
                ActivityRecords = collection.ActivityRecords,
                Metrics = collection.Metrics
            };

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            await File.WriteAllTextAsync(outputPath, json, Encoding.UTF8).ConfigureAwait(false);
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write JSON file", outputPath, "JSON", ex);
        }
        catch (Exception ex)
        {
            throw new ExportException("Failed to export to JSON", outputPath, "JSON", ex);
        }
    }

    /// <summary>
    /// Export sleep data to CSV
    /// </summary>
    public async Task ExportSleepToCsvAsync(List<SleepData> sleepRecords, string outputPath)
    {
        try
        {
            using var fs = File.Create(outputPath);
            using var writer = new StreamWriter(fs, Encoding.UTF8);
            using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

            csv.WriteHeader<SleepCsvRecord>();
            await csv.NextRecordAsync().ConfigureAwait(false);

            foreach (var sleep in sleepRecords)
            {
                var record = new SleepCsvRecord
                {
                    Date = sleep.RecordDate.ToString("yyyy-MM-dd"),
                    Duration = sleep.DurationMinutes,
                    DeepSleep = sleep.DeepSleepMinutes,
                    LightSleep = sleep.LightSleepMinutes,
                    REM = sleep.RemSleepMinutes,
                    Awake = sleep.AwakeMinutes,
                    Quality = sleep.Quality.ToString(),
                    Score = sleep.Score,
                    AvgHeartRate = sleep.AverageHeartRate
                };

                csv.WriteRecord(record);
                await csv.NextRecordAsync().ConfigureAwait(false);
            }
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write CSV file", outputPath, "CSV", ex);
        }
    }

    /// <summary>
    /// Export heart rate data to CSV
    /// </summary>
    public async Task ExportHeartRateToCsvAsync(List<HeartRateData> records, string outputPath)
    {
        try
        {
            using var fs = File.Create(outputPath);
            using var writer = new StreamWriter(fs, Encoding.UTF8);
            using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

            csv.WriteHeader<HeartRateCsvRecord>();
            await csv.NextRecordAsync().ConfigureAwait(false);

            foreach (var hr in records)
            {
                var record = new HeartRateCsvRecord
                {
                    Date = hr.RecordDate.ToString("yyyy-MM-dd"),
                    MinBpm = hr.MinimumBpm,
                    MaxBpm = hr.MaximumBpm,
                    AvgBpm = hr.AverageBpm,
                    RestingBpm = hr.RestingBpm,
                    Measurements = hr.MeasurementCount,
                    StressLevel = hr.StressLevel,
                    CardioZone = hr.CardioZoneMinutes,
                    Zone1Minutes = hr.ZoneMinutes[0],
                    Zone2Minutes = hr.ZoneMinutes[1],
                    Zone3Minutes = hr.ZoneMinutes[2],
                    Zone4Minutes = hr.ZoneMinutes[3],
                    Zone5Minutes = hr.ZoneMinutes[4]
                };

                csv.WriteRecord(record);
                await csv.NextRecordAsync().ConfigureAwait(false);
            }
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write heart rate CSV", outputPath, "CSV", ex);
        }
    }

    /// <summary>
    /// Export heart rate data to CSV, classifying each daily record's average BPM into a
    /// heart rate zone based on the supplied maximum heart rate.
    /// Each row includes a <c>AvgZone</c> column (zone 1–5) in addition to the standard
    /// BPM columns and per-zone time totals.
    /// </summary>
    /// <param name="records">Heart rate records to export.</param>
    /// <param name="outputPath">Destination CSV file path.</param>
    /// <param name="maxHeartRate">User's maximum heart rate used for zone classification.</param>
    public async Task ExportHeartRateWithZonesToCsvAsync(
        List<HeartRateData> records,
        string outputPath,
        int maxHeartRate)
    {
        if (maxHeartRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxHeartRate), "Max heart rate must be greater than zero.");

        try
        {
            using var fs = File.Create(outputPath);
            using var writer = new StreamWriter(fs, Encoding.UTF8);
            using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

            csv.WriteHeader<HeartRateWithZonesCsvRecord>();
            await csv.NextRecordAsync().ConfigureAwait(false);

            foreach (var hr in records)
            {
                var avgZone = HeartRateData.ClassifyZone(hr.AverageBpm, maxHeartRate);

                var record = new HeartRateWithZonesCsvRecord
                {
                    Date        = hr.RecordDate.ToString("yyyy-MM-dd"),
                    MinBpm      = hr.MinimumBpm,
                    MaxBpm      = hr.MaximumBpm,
                    AvgBpm      = hr.AverageBpm,
                    RestingBpm  = hr.RestingBpm,
                    Measurements = hr.MeasurementCount,
                    StressLevel = hr.StressLevel,
                    AvgZone     = (int)avgZone,
                    Zone1Minutes = hr.ZoneMinutes[0],
                    Zone2Minutes = hr.ZoneMinutes[1],
                    Zone3Minutes = hr.ZoneMinutes[2],
                    Zone4Minutes = hr.ZoneMinutes[3],
                    Zone5Minutes = hr.ZoneMinutes[4]
                };

                csv.WriteRecord(record);
                await csv.NextRecordAsync().ConfigureAwait(false);
            }
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write heart rate with zones CSV", outputPath, "CSV", ex);
        }
    }

    /// <summary>
    /// Export steps data to CSV
    /// </summary>
    public async Task ExportStepsToCsvAsync(List<StepsData> records, string outputPath)
    {
        try
        {
            using var fs = File.Create(outputPath);
            using var writer = new StreamWriter(fs, Encoding.UTF8);
            using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);

            csv.WriteHeader<StepsCsvRecord>();
            await csv.NextRecordAsync().ConfigureAwait(false);

            foreach (var steps in records)
            {
                // GoalAchievementPercentage is only populated when the caller has explicitly
                // invoked StepsData.UpdateGoalAchievement(); export should reflect the derived
                // value regardless, so compute it directly from TotalSteps/DailyGoal here.
                var goalAchievement = steps.DailyGoal > 0
                    ? (int)(steps.TotalSteps / (double)steps.DailyGoal * 100)
                    : 0;

                var record = new StepsCsvRecord
                {
                    Date = steps.RecordDate.ToString("yyyy-MM-dd"),
                    Steps = steps.TotalSteps,
                    DistanceKm = steps.DistanceKm,
                    Calories = steps.CaloriesBurned,
                    GoalAchievement = goalAchievement,
                    ActiveMinutes = steps.ActiveMinutes,
                    Walking = steps.WalkingMinutes,
                    Running = steps.RunningMinutes
                };

                csv.WriteRecord(record);
                await csv.NextRecordAsync().ConfigureAwait(false);
            }
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write steps CSV", outputPath, "CSV", ex);
        }
    }

    /// <summary>
    /// Export health data to separate JSON files per data type:
    /// sleep.json, heart_rate.json, spo2.json, steps.json.
    /// </summary>
    public async Task ExportToJsonPerTypeAsync(HealthDataCollection collection, string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var tasks = new List<Task>();

        if (collection.SleepRecords.Count > 0)
        {
            var path = Path.Combine(outputDirectory, "sleep.json");
            tasks.Add(WriteJsonFileAsync(new
            {
                DataType = "Sleep",
                ExportDate = DateTime.UtcNow.ToIso8601(),
                RecordCount = collection.SleepRecords.Count,
                Records = collection.SleepRecords
            }, path, options));
        }

        if (collection.HeartRateRecords.Count > 0)
        {
            var path = Path.Combine(outputDirectory, "heart_rate.json");
            tasks.Add(WriteJsonFileAsync(new
            {
                DataType = "HeartRate",
                ExportDate = DateTime.UtcNow.ToIso8601(),
                RecordCount = collection.HeartRateRecords.Count,
                Records = collection.HeartRateRecords
            }, path, options));
        }

        if (collection.SpO2Records.Count > 0)
        {
            var path = Path.Combine(outputDirectory, "spo2.json");
            tasks.Add(WriteJsonFileAsync(new
            {
                DataType = "SpO2",
                ExportDate = DateTime.UtcNow.ToIso8601(),
                RecordCount = collection.SpO2Records.Count,
                Records = collection.SpO2Records
            }, path, options));
        }

        if (collection.StepsRecords.Count > 0)
        {
            var path = Path.Combine(outputDirectory, "steps.json");
            tasks.Add(WriteJsonFileAsync(new
            {
                DataType = "Steps",
                ExportDate = DateTime.UtcNow.ToIso8601(),
                RecordCount = collection.StepsRecords.Count,
                Records = collection.StepsRecords
            }, path, options));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static async Task WriteJsonFileAsync(object data, string path, JsonSerializerOptions options)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, options);
            await File.WriteAllTextAsync(path, json, Encoding.UTF8).ConfigureAwait(false);
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write JSON file", path, "JSON", ex);
        }
    }

    /// <summary>
    /// Export complete dataset
    /// </summary>
    public async Task ExportCompleteAsync(HealthDataCollection collection, string outputDirectory, ExportFormat format)
    {
        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        switch (format)
        {
            case ExportFormat.Json:
                var jsonPath = Path.Combine(outputDirectory, "health_data.json");
                await ExportToJsonAsync(collection, jsonPath).ConfigureAwait(false);
                break;

            case ExportFormat.Csv:
                if (collection.SleepRecords.Any())
                    await ExportSleepToCsvAsync(collection.SleepRecords,
                        Path.Combine(outputDirectory, "sleep.csv"));

                if (collection.HeartRateRecords.Any())
                    await ExportHeartRateToCsvAsync(collection.HeartRateRecords,
                        Path.Combine(outputDirectory, "heart_rate.csv"));

                if (collection.StepsRecords.Any())
                    await ExportStepsToCsvAsync(collection.StepsRecords,
                        Path.Combine(outputDirectory, "steps.csv"));
                break;

            case ExportFormat.All:
                await ExportCompleteAsync(collection, outputDirectory, ExportFormat.Json).ConfigureAwait(false);
                await ExportCompleteAsync(collection, outputDirectory, ExportFormat.Csv).ConfigureAwait(false);
                break;

            case ExportFormat.JsonLines:
                var jsonLinesPath = Path.Combine(outputDirectory, "health_data.jsonl");
                await ExportToJsonLinesAsync(collection, jsonLinesPath).ConfigureAwait(false);
                break;
        }
    }

    /// <summary>
    /// Export health data collection to JSON Lines format (one JSON object per line)
    /// </summary>
    public async Task ExportToJsonLinesAsync(HealthDataCollection collection, string outputPath)
    {
        if (collection.GetTotalRecordCount() == 0)
        {
            return;
        }

        try
        {
            using var fs = File.Create(outputPath);
            using var writer = new StreamWriter(fs, Encoding.UTF8);
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            // Export sleep records
            foreach (var record in collection.SleepRecords)
            {
                var json = JsonSerializer.Serialize(record, options);
                await writer.WriteLineAsync(json).ConfigureAwait(false);
            }

            // Export heart rate records
            foreach (var record in collection.HeartRateRecords)
            {
                var json = JsonSerializer.Serialize(record, options);
                await writer.WriteLineAsync(json).ConfigureAwait(false);
            }

            // Export SpO2 records
            foreach (var record in collection.SpO2Records)
            {
                var json = JsonSerializer.Serialize(record, options);
                await writer.WriteLineAsync(json).ConfigureAwait(false);
            }

            // Export steps records
            foreach (var record in collection.StepsRecords)
            {
                var json = JsonSerializer.Serialize(record, options);
                await writer.WriteLineAsync(json).ConfigureAwait(false);
            }

            // Export activity records
            foreach (var record in collection.ActivityRecords)
            {
                var json = JsonSerializer.Serialize(record, options);
                await writer.WriteLineAsync(json).ConfigureAwait(false);
            }

            await writer.FlushAsync().ConfigureAwait(false);
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write JSON Lines file", outputPath, "JSONL", ex);
        }
        catch (Exception ex)
        {
            throw new ExportException("Failed to export to JSON Lines", outputPath, "JSONL", ex);
        }
    }
}

/// <summary>CSV record for sleep data</summary>
public sealed class SleepCsvRecord
{
    public string? Date { get; set; }
    public int Duration { get; set; }
    public int DeepSleep { get; set; }
    public int LightSleep { get; set; }
    public int REM { get; set; }
    public int Awake { get; set; }
    public string? Quality { get; set; }
    public int? Score { get; set; }
    public int? AvgHeartRate { get; set; }
}

/// <summary>CSV record for heart rate data</summary>
public sealed class HeartRateCsvRecord
{
    public string? Date { get; set; }
    public int MinBpm { get; set; }
    public int MaxBpm { get; set; }
    public int AvgBpm { get; set; }
    public int? RestingBpm { get; set; }
    public int Measurements { get; set; }
    public int? StressLevel { get; set; }
    public int CardioZone { get; set; }
    public int Zone1Minutes { get; set; }
    public int Zone2Minutes { get; set; }
    public int Zone3Minutes { get; set; }
    public int Zone4Minutes { get; set; }
    public int Zone5Minutes { get; set; }
}

/// <summary>CSV record for steps data</summary>
public sealed class StepsCsvRecord
{
    public string? Date { get; set; }
    public int Steps { get; set; }
    public double DistanceKm { get; set; }
    public int Calories { get; set; }
    public int GoalAchievement { get; set; }
    public int ActiveMinutes { get; set; }
    public int Walking { get; set; }
    public int Running { get; set; }
}

/// <summary>CSV record for heart rate data with per-zone classification columns</summary>
public sealed class HeartRateWithZonesCsvRecord
{
    public string? Date { get; set; }
    public int MinBpm { get; set; }
    public int MaxBpm { get; set; }
    public int AvgBpm { get; set; }
    public int? RestingBpm { get; set; }
    public int Measurements { get; set; }
    public int? StressLevel { get; set; }
    /// <summary>Zone (1–5) that the daily average BPM falls into</summary>
    public int AvgZone { get; set; }
    public int Zone1Minutes { get; set; }
    public int Zone2Minutes { get; set; }
    public int Zone3Minutes { get; set; }
    public int Zone4Minutes { get; set; }
    public int Zone5Minutes { get; set; }
}
