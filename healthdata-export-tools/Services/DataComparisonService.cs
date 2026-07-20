#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using System.Text.Json;
using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Services;

/// <summary>
/// Service for comparing two distinct sets of health data records.
/// </summary>
public sealed class DataComparisonService
{
    /// <summary>
    /// Compare two pre-built <see cref="HealthDataCollection"/> periods.
    /// Calculates percentage changes for sleep, heart rate, steps, SpO2, and activity metrics.
    /// </summary>
    /// <param name="period1">Reference (baseline) period.</param>
    /// <param name="period2">Comparison period.</param>
    /// <returns>A <see cref="DataComparisonResult"/> containing per-metric changes and a narrative summary.</returns>
    public Task<DataComparisonResult> ComparePeriodsAsync(
        HealthDataCollection period1,
        HealthDataCollection period2)
    {
        ArgumentNullException.ThrowIfNull(period1);
        ArgumentNullException.ThrowIfNull(period2);

        var result = new DataComparisonResult
        {
            Period1RecordCount = period1.GetTotalRecordCount(),
            Period2RecordCount = period2.GetTotalRecordCount(),
            GeneratedAt = DateTime.UtcNow
        };

        // Sleep
        if (period1.SleepRecords.Count > 0 && period2.SleepRecords.Count > 0)
        {
            result.Period1AverageSleepMinutes = period1.SleepRecords.Average(s => s.DurationMinutes);
            result.Period2AverageSleepMinutes = period2.SleepRecords.Average(s => s.DurationMinutes);
            result.SleepDurationChangePercentage = CalculatePercentageChange(
                result.Period1AverageSleepMinutes,
                result.Period2AverageSleepMinutes);

            result.Period1AverageDeepSleepMinutes = period1.SleepRecords.Average(s => s.DeepSleepMinutes);
            result.Period2AverageDeepSleepMinutes = period2.SleepRecords.Average(s => s.DeepSleepMinutes);
            result.DeepSleepChangePercentage = CalculatePercentageChange(
                result.Period1AverageDeepSleepMinutes,
                result.Period2AverageDeepSleepMinutes);
        }

        // Heart Rate
        if (period1.HeartRateRecords.Count > 0 && period2.HeartRateRecords.Count > 0)
        {
            result.Period1AverageHeartRate = period1.HeartRateRecords.Average(h => h.AverageBpm);
            result.Period2AverageHeartRate = period2.HeartRateRecords.Average(h => h.AverageBpm);
            result.HeartRateChangePercentage = CalculatePercentageChange(
                result.Period1AverageHeartRate,
                result.Period2AverageHeartRate);
        }

        // Steps
        if (period1.StepsRecords.Count > 0 && period2.StepsRecords.Count > 0)
        {
            result.Period1AverageSteps = period1.StepsRecords.Average(st => st.TotalSteps);
            result.Period2AverageSteps = period2.StepsRecords.Average(st => st.TotalSteps);
            result.StepsChangePercentage = CalculatePercentageChange(
                result.Period1AverageSteps,
                result.Period2AverageSteps);
        }

        // SpO2
        if (period1.SpO2Records.Count > 0 && period2.SpO2Records.Count > 0)
        {
            result.Period1AverageSpO2 = period1.SpO2Records.Average(sp => sp.AveragePercentage);
            result.Period2AverageSpO2 = period2.SpO2Records.Average(sp => sp.AveragePercentage);
            result.SpO2ChangePercentage = CalculatePercentageChange(
                result.Period1AverageSpO2,
                result.Period2AverageSpO2);
        }

        // Activity
        if (period1.ActivityRecords.Count > 0 && period2.ActivityRecords.Count > 0)
        {
            result.Period1TotalActivityMinutes = period1.ActivityRecords.Sum(a => a.DurationMinutes);
            result.Period2TotalActivityMinutes = period2.ActivityRecords.Sum(a => a.DurationMinutes);
            result.ActivityMinutesChangePercentage = CalculatePercentageChange(
                result.Period1TotalActivityMinutes,
                result.Period2TotalActivityMinutes);

            result.Period1TotalCalories = period1.ActivityRecords.Sum(a => a.CaloriesBurned);
            result.Period2TotalCalories = period2.ActivityRecords.Sum(a => a.CaloriesBurned);
            result.CaloriesChangePercentage = CalculatePercentageChange(
                result.Period1TotalCalories,
                result.Period2TotalCalories);
        }

        result.NarrativeSummary = BuildNarrative(result);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Compare two date ranges extracted from a single <see cref="HealthDataCollection"/>.
    /// Records are partitioned into Period 1 and Period 2 based on the supplied date boundaries.
    /// </summary>
    /// <param name="collection">The full collection containing all records.</param>
    /// <param name="period1Start">Inclusive start of the baseline period.</param>
    /// <param name="period1End">Inclusive end of the baseline period.</param>
    /// <param name="period2Start">Inclusive start of the comparison period.</param>
    /// <param name="period2End">Inclusive end of the comparison period.</param>
    public Task<DataComparisonResult> CompareByDateRangeAsync(
        HealthDataCollection collection,
        DateTime period1Start,
        DateTime period1End,
        DateTime period2Start,
        DateTime period2End)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var p1 = new HealthDataCollection
        {
            SleepRecords      = collection.SleepRecords.Where(r => r.RecordDate.Date >= period1Start.Date && r.RecordDate.Date <= period1End.Date).ToList(),
            HeartRateRecords  = collection.HeartRateRecords.Where(r => r.RecordDate.Date >= period1Start.Date && r.RecordDate.Date <= period1End.Date).ToList(),
            StepsRecords      = collection.StepsRecords.Where(r => r.RecordDate.Date >= period1Start.Date && r.RecordDate.Date <= period1End.Date).ToList(),
            SpO2Records       = collection.SpO2Records.Where(r => r.RecordDate.Date >= period1Start.Date && r.RecordDate.Date <= period1End.Date).ToList(),
            ActivityRecords   = collection.ActivityRecords.Where(r => r.RecordDate.Date >= period1Start.Date && r.RecordDate.Date <= period1End.Date).ToList()
        };

        var p2 = new HealthDataCollection
        {
            SleepRecords      = collection.SleepRecords.Where(r => r.RecordDate.Date >= period2Start.Date && r.RecordDate.Date <= period2End.Date).ToList(),
            HeartRateRecords  = collection.HeartRateRecords.Where(r => r.RecordDate.Date >= period2Start.Date && r.RecordDate.Date <= period2End.Date).ToList(),
            StepsRecords      = collection.StepsRecords.Where(r => r.RecordDate.Date >= period2Start.Date && r.RecordDate.Date <= period2End.Date).ToList(),
            SpO2Records       = collection.SpO2Records.Where(r => r.RecordDate.Date >= period2Start.Date && r.RecordDate.Date <= period2End.Date).ToList(),
            ActivityRecords   = collection.ActivityRecords.Where(r => r.RecordDate.Date >= period2Start.Date && r.RecordDate.Date <= period2End.Date).ToList()
        };

        return ComparePeriodsAsync(p1, p2);
    }

    /// <summary>
    /// Export a <see cref="DataComparisonResult"/> to a JSON file.
    /// </summary>
    /// <param name="result">The comparison result to serialize.</param>
    /// <param name="outputPath">Destination file path.</param>
    public async Task ExportToJsonAsync(DataComparisonResult result, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(result);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(result, options);
        await File.WriteAllTextAsync(outputPath, json, Encoding.UTF8).ConfigureAwait(false);
    }

    /// <summary>
    /// Renders a plain-text diff summary.
    /// </summary>
    /// <param name="result">The comparison result to render.</param>
    /// <returns>A formatted plain-text diff summary.</returns>
    public string GenerateTextReport(DataComparisonResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var sb = new StringBuilder();
        sb.AppendLine("Diff Summary:");
        sb.AppendLine($"Added: {result.AddedCount}");
        sb.AppendLine($"Removed: {result.RemovedCount}");
        sb.AppendLine($"Changed: {result.ChangedCount}");
        sb.AppendLine();
        sb.AppendLine("Top 10 Changed Entries:");
        
        if (result.TopChangedEntries.Count == 0)
        {
            sb.AppendLine("None.");
        }
        else
        {
            foreach (var entry in result.TopChangedEntries.Take(10))
            {
                sb.AppendLine($"- {entry}");
            }
        }
        return sb.ToString();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static double CalculatePercentageChange(double oldVal, double newVal)
    {
        if (Math.Abs(oldVal) < 0.001) return newVal > 0 ? 100.0 : 0.0;
        return Math.Round((newVal - oldVal) / oldVal * 100.0, 2);
    }

    private static string BuildNarrative(DataComparisonResult r)
    {
        var parts = new List<string>();

        if (r.Period1AverageSleepMinutes > 0)
            parts.Add(FormatChange("Sleep duration", r.SleepDurationChangePercentage, "min/night"));

        if (r.Period1AverageHeartRate > 0)
            parts.Add(FormatChange("Resting heart rate", r.HeartRateChangePercentage, "BPM"));

        if (r.Period1AverageSteps > 0)
            parts.Add(FormatChange("Daily steps", r.StepsChangePercentage, "steps"));

        if (r.Period1AverageSpO2 > 0)
            parts.Add(FormatChange("SpO2", r.SpO2ChangePercentage, "%"));

        if (r.Period1TotalActivityMinutes > 0)
            parts.Add(FormatChange("Activity minutes", r.ActivityMinutesChangePercentage, "min/period"));

        return parts.Count == 0
            ? "No comparable data found between the two periods."
            : string.Join(" | ", parts);
    }

    private static string FormatChange(string metric, double pct, string unit)
    {
        var direction = pct > 0 ? "▲" : pct < 0 ? "▼" : "→";
        var absStr    = Math.Abs(pct).ToString("F1");
        return $"{metric}: {direction} {absStr}% ({unit})";
    }
}

/// <summary>
/// Contains the result of comparing two data periods.
/// </summary>
public sealed class DataComparisonResult
{
    /// <summary>Timestamp when this comparison was generated.</summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>Total record count in Period 1.</summary>
    public int Period1RecordCount { get; set; }
    /// <summary>Total record count in Period 2.</summary>
    public int Period2RecordCount { get; set; }

    // Diff metrics
    public int AddedCount { get; set; }
    public int RemovedCount { get; set; }
    public int ChangedCount { get; set; }
    public List<string> TopChangedEntries { get; set; } = [];

    // Sleep
    /// <summary>Average nightly sleep duration in Period 1 (minutes).</summary>
    public double Period1AverageSleepMinutes { get; set; }
    /// <summary>Average nightly sleep duration in Period 2 (minutes).</summary>
    public double Period2AverageSleepMinutes { get; set; }
    /// <summary>Percentage change in average sleep duration (Period 2 vs Period 1).</summary>
    public double SleepDurationChangePercentage { get; set; }

    /// <summary>Average deep-sleep duration per night in Period 1 (minutes).</summary>
    public double Period1AverageDeepSleepMinutes { get; set; }
    /// <summary>Average deep-sleep duration per night in Period 2 (minutes).</summary>
    public double Period2AverageDeepSleepMinutes { get; set; }
    /// <summary>Percentage change in average deep-sleep duration.</summary>
    public double DeepSleepChangePercentage { get; set; }

    // Heart rate
    /// <summary>Average heart rate in Period 1 (BPM).</summary>
    public double Period1AverageHeartRate { get; set; }
    /// <summary>Average heart rate in Period 2 (BPM).</summary>
    public double Period2AverageHeartRate { get; set; }
    /// <summary>Percentage change in average heart rate.</summary>
    public double HeartRateChangePercentage { get; set; }

    // Steps
    /// <summary>Average daily steps in Period 1.</summary>
    public double Period1AverageSteps { get; set; }
    /// <summary>Average daily steps in Period 2.</summary>
    public double Period2AverageSteps { get; set; }
    /// <summary>Percentage change in average daily steps.</summary>
    public double StepsChangePercentage { get; set; }

    // SpO2
    /// <summary>Average SpO2 percentage in Period 1.</summary>
    public double Period1AverageSpO2 { get; set; }
    /// <summary>Average SpO2 percentage in Period 2.</summary>
    public double Period2AverageSpO2 { get; set; }
    /// <summary>Percentage change in average SpO2.</summary>
    public double SpO2ChangePercentage { get; set; }

    // Activity
    /// <summary>Total activity minutes in Period 1.</summary>
    public int Period1TotalActivityMinutes { get; set; }
    /// <summary>Total activity minutes in Period 2.</summary>
    public int Period2TotalActivityMinutes { get; set; }
    /// <summary>Percentage change in total activity minutes.</summary>
    public double ActivityMinutesChangePercentage { get; set; }

    /// <summary>Total calories burned via activity in Period 1.</summary>
    public int Period1TotalCalories { get; set; }
    /// <summary>Total calories burned via activity in Period 2.</summary>
    public int Period2TotalCalories { get; set; }
    /// <summary>Percentage change in total calories burned.</summary>
    public double CaloriesChangePercentage { get; set; }

    /// <summary>
    /// Human-readable narrative summarising the key changes across all metrics.
    /// </summary>
    public string NarrativeSummary { get; set; } = string.Empty;
}
