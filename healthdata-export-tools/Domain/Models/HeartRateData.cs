// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Domain.Models;

/// <summary>
/// Heart rate measurements collected throughout a day
/// </summary>
public class HeartRateData : HealthDataRecord
{
    /// <summary>
    /// Minimum heart rate recorded for the day (BPM)
    /// </summary>
    public int MinimumBpm { get; set; }

    /// <summary>
    /// Maximum heart rate recorded for the day (BPM)
    /// </summary>
    public int MaximumBpm { get; set; }

    /// <summary>
    /// Average heart rate for the day (BPM)
    /// </summary>
    public int AverageBpm { get; set; }

    /// <summary>
    /// Resting heart rate measured in morning (BPM)
    /// </summary>
    public int? RestingBpm { get; set; }

    /// <summary>
    /// Number of measurements taken during the day
    /// </summary>
    public int MeasurementCount { get; set; }

    /// <summary>
    /// List of individual heart rate measurements with timestamps
    /// </summary>
    public List<HeartRateMeasurement> Measurements { get; set; } = [];

    /// <summary>
    /// Heart rate variability (HRV) score if available
    /// </summary>
    public double? HeartRateVariability { get; set; }

    /// <summary>
    /// Stress level indicator (0-100)
    /// </summary>
    public int? StressLevel { get; set; }

    /// <summary>
    /// Time spent in cardio zone (elevated HR) in minutes
    /// </summary>
    public int CardioZoneMinutes { get; set; }

    /// <summary>
    /// Time spent in fat burn zone in minutes
    /// </summary>
    public int FatBurnZoneMinutes { get; set; }

    /// <summary>
    /// Validate heart rate data is within reasonable ranges
    /// </summary>
    public override bool IsValid()
    {
        if (MinimumBpm < 30 || MinimumBpm > 200) return false;
        if (MaximumBpm < 30 || MaximumBpm > 220) return false;
        if (AverageBpm < 30 || AverageBpm > 200) return false;
        if (MinimumBpm > MaximumBpm) return false;
        if (AverageBpm < MinimumBpm || AverageBpm > MaximumBpm) return false;
        if (RestingBpm.HasValue && (RestingBpm < 30 || RestingBpm > 100)) return false;
        if (MeasurementCount < 0) return false;

        return true;
    }

    /// <summary>
    /// Get summary of heart rate statistics
    /// </summary>
    public override Dictionary<string, object> GetSummary()
    {
        return new()
        {
            { "Date", RecordDate.ToString("yyyy-MM-dd") },
            { "Minimum", $"{MinimumBpm} BPM" },
            { "Average", $"{AverageBpm} BPM" },
            { "Maximum", $"{MaximumBpm} BPM" },
            { "Resting", RestingBpm.HasValue ? $"{RestingBpm} BPM" : "N/A" },
            { "Measurements", MeasurementCount },
            { "StressLevel", StressLevel },
            { "CardioZone", $"{CardioZoneMinutes} min" },
            { "FatBurnZone", $"{FatBurnZoneMinutes} min" }
        };
    }

    /// <summary>
    /// Calculate heart rate reserve (max HR - resting HR)
    /// </summary>
    public int? CalculateHeartRateReserve()
    {
        if (!RestingBpm.HasValue) return null;
        return MaximumBpm - RestingBpm.Value;
    }

    /// <summary>
    /// Assess stress level based on heart rate patterns
    /// </summary>
    public int AssessStressLevel()
    {
        if (!RestingBpm.HasValue) return 50;

        var reserve = MaximumBpm - RestingBpm.Value;
        var avgStress = ((AverageBpm - RestingBpm.Value) / (double)reserve) * 100;

        return Math.Clamp((int)avgStress, 0, 100);
    }

    /// <summary>
    /// Add a single heart rate measurement
    /// </summary>
    public void AddMeasurement(HeartRateMeasurement measurement)
    {
        Measurements.Add(measurement);
        MeasurementCount = Measurements.Count;
        Touch();
    }
}

/// <summary>
/// Single heart rate measurement with timestamp
/// </summary>
public class HeartRateMeasurement
{
    /// <summary>
    /// Timestamp of the measurement
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Heart rate in beats per minute
    /// </summary>
    public int Bpm { get; set; }

    /// <summary>
    /// Optional accuracy indicator for the measurement
    /// </summary>
    public int? Accuracy { get; set; }
}
