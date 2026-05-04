// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools.Domain.Enums;

namespace HealthDataExportTools.Domain.Models;

/// <summary>
/// Represents a single sleep session with detailed metrics
/// </summary>
public class SleepData : HealthDataRecord
{
    /// <summary>
    /// Time when sleep started
    /// </summary>
    public DateTime SleepStart { get; set; }

    /// <summary>
    /// Time when sleep ended
    /// </summary>
    public DateTime SleepEnd { get; set; }

    /// <summary>
    /// Total duration of sleep in minutes
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Duration of deep sleep in minutes
    /// </summary>
    public int DeepSleepMinutes { get; set; }

    /// <summary>
    /// Duration of light sleep in minutes
    /// </summary>
    public int LightSleepMinutes { get; set; }

    /// <summary>
    /// Duration of REM sleep in minutes
    /// </summary>
    public int RemSleepMinutes { get; set; }

    /// <summary>
    /// Duration of awake periods in minutes
    /// </summary>
    public int AwakeMinutes { get; set; }

    /// <summary>
    /// Quality assessment of the sleep session
    /// </summary>
    public SleepQuality Quality { get; set; } = SleepQuality.Average;

    /// <summary>
    /// Sleep score assigned by the device (typically 0-100)
    /// </summary>
    public int? Score { get; set; }

    /// <summary>
    /// Number of sleep cycles detected
    /// </summary>
    public int? CycleCount { get; set; }

    /// <summary>
    /// Indicates if this is a nap (shorter sleep session)
    /// </summary>
    public bool IsNap { get; set; }

    /// <summary>
    /// Average heart rate during sleep
    /// </summary>
    public int? AverageHeartRate { get; set; }

    /// <summary>
    /// Calculate sleep quality based on metrics
    /// </summary>
    public SleepQuality CalculateQuality()
    {
        if (DurationMinutes < 360) return SleepQuality.Poor;
        if (DurationMinutes < 420) return SleepQuality.Average;
        if (DurationMinutes > 540) return SleepQuality.Average;

        var deepPercentage = (double)DeepSleepMinutes / DurationMinutes;
        var remPercentage = (double)RemSleepMinutes / DurationMinutes;

        if (deepPercentage > 0.20 && remPercentage > 0.15) return SleepQuality.Excellent;
        if (deepPercentage > 0.15 && remPercentage > 0.12) return SleepQuality.Good;

        return SleepQuality.Average;
    }

    /// <summary>
    /// Validate sleep data integrity and plausibility
    /// </summary>
    public override bool IsValid()
    {
        if (SleepStart >= SleepEnd) return false;
        if (DurationMinutes <= 0) return false;
        if (DeepSleepMinutes < 0 || LightSleepMinutes < 0 || RemSleepMinutes < 0) return false;
        if (DeepSleepMinutes + LightSleepMinutes + RemSleepMinutes + AwakeMinutes > DurationMinutes) return false;
        if (AverageHeartRate.HasValue && (AverageHeartRate < 30 || AverageHeartRate > 200)) return false;

        return true;
    }

    /// <summary>
    /// Get summary statistics for this sleep session
    /// </summary>
    public override Dictionary<string, object> GetSummary()
    {
        return new()
        {
            { "SleepDate", RecordDate.ToString("yyyy-MM-dd") },
            { "Duration", $"{DurationMinutes} min" },
            { "Deep", $"{DeepSleepMinutes} min" },
            { "Light", $"{LightSleepMinutes} min" },
            { "REM", $"{RemSleepMinutes} min" },
            { "Awake", $"{AwakeMinutes} min" },
            { "Quality", Quality.ToString() },
            { "Score", Score },
            { "AvgHeartRate", AverageHeartRate }
        };
    }

    /// <summary>
    /// Calculate deep sleep percentage
    /// </summary>
    public double GetDeepSleepPercentage() => DurationMinutes > 0 ? (double)DeepSleepMinutes / DurationMinutes * 100 : 0;

    /// <summary>
    /// Calculate REM sleep percentage
    /// </summary>
    public double GetRemSleepPercentage() => DurationMinutes > 0 ? (double)RemSleepMinutes / DurationMinutes * 100 : 0;
}
