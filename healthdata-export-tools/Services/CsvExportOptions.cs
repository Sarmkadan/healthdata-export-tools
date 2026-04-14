#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Services;

/// <summary>
/// Configuration options for CSV export, including column selection and date formatting.
/// </summary>
public sealed class CsvExportOptions
{
    /// <summary>
    /// Include sleep records in the export. Default: true.
    /// </summary>
    public bool IncludeSleep { get; set; } = true;

    /// <summary>
    /// Include heart rate records in the export. Default: true.
    /// </summary>
    public bool IncludeHeartRate { get; set; } = true;

    /// <summary>
    /// Include SpO2 records in the export. Default: true.
    /// </summary>
    public bool IncludeSpO2 { get; set; } = true;

    /// <summary>
    /// Include steps records in the export. Default: true.
    /// </summary>
    public bool IncludeSteps { get; set; } = true;

    /// <summary>
    /// Include activity records in the export. Default: true.
    /// </summary>
    public bool IncludeActivity { get; set; } = true;

    /// <summary>
    /// Date format string used for all date columns. Defaults to ISO 8601 ("yyyy-MM-dd").
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>
    /// Subset of sleep column names to include. When null or empty, all columns are exported.
    /// Recognised names: Date, Duration, DeepSleep, LightSleep, REM, Awake, Quality, Score, AvgHeartRate.
    /// </summary>
    public IReadOnlyList<string>? SleepColumns { get; set; }

    /// <summary>
    /// Subset of heart rate column names to include. When null or empty, all columns are exported.
    /// Recognised names: Date, MinBpm, MaxBpm, AvgBpm, RestingBpm, Measurements, StressLevel, CardioZone,
    /// Zone1Minutes, Zone2Minutes, Zone3Minutes, Zone4Minutes, Zone5Minutes.
    /// </summary>
    public IReadOnlyList<string>? HeartRateColumns { get; set; }

    /// <summary>
    /// Subset of SpO2 column names to include. When null or empty, all columns are exported.
    /// Recognised names: Date, MinPercentage, MaxPercentage, AvgPercentage, Measurements.
    /// </summary>
    public IReadOnlyList<string>? SpO2Columns { get; set; }

    /// <summary>
    /// Subset of steps column names to include. When null or empty, all columns are exported.
    /// Recognised names: Date, Steps, DistanceKm, Calories, GoalAchievement, ActiveMinutes, Walking, Running.
    /// </summary>
    public IReadOnlyList<string>? StepsColumns { get; set; }
}
