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
public class ExportService
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

            await File.WriteAllTextAsync(outputPath, json, Encoding.UTF8);
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
            await csv.NextRecordAsync();

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
                await csv.NextRecordAsync();
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
            await csv.NextRecordAsync();

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
                    CardioZone = hr.CardioZoneMinutes
                };

                csv.WriteRecord(record);
                await csv.NextRecordAsync();
            }
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write heart rate CSV", outputPath, "CSV", ex);
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
            await csv.NextRecordAsync();

            foreach (var steps in records)
            {
                var record = new StepsCsvRecord
                {
                    Date = steps.RecordDate.ToString("yyyy-MM-dd"),
                    Steps = steps.TotalSteps,
                    DistanceKm = steps.DistanceKm,
                    Calories = steps.CaloriesBurned,
                    GoalAchievement = steps.GoalAchievementPercentage,
                    ActiveMinutes = steps.ActiveMinutes,
                    Walking = steps.WalkingMinutes,
                    Running = steps.RunningMinutes
                };

                csv.WriteRecord(record);
                await csv.NextRecordAsync();
            }
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write steps CSV", outputPath, "CSV", ex);
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
                await ExportToJsonAsync(collection, jsonPath);
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
                await ExportCompleteAsync(collection, outputDirectory, ExportFormat.Json);
                await ExportCompleteAsync(collection, outputDirectory, ExportFormat.Csv);
                break;
        }
    }
}

/// <summary>CSV record for sleep data</summary>
public class SleepCsvRecord
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
public class HeartRateCsvRecord
{
    public string? Date { get; set; }
    public int MinBpm { get; set; }
    public int MaxBpm { get; set; }
    public int AvgBpm { get; set; }
    public int? RestingBpm { get; set; }
    public int Measurements { get; set; }
    public int? StressLevel { get; set; }
    public int CardioZone { get; set; }
}

/// <summary>CSV record for steps data</summary>
public class StepsCsvRecord
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
