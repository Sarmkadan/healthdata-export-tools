#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools.Domain.Enums;

namespace HealthDataExportTools.Domain.Models;

/// <summary>
/// Heart rate measurements collected throughout a day
/// </summary>
public sealed class HeartRateData : HealthDataRecord
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
    /// Minutes spent in each heart rate zone (index 0 = Zone1 … index 4 = Zone5).
    /// Populated from device zone data (e.g. Garmin FIT hr_zone_0 – hr_zone_4 fields)
    /// or calculated via <see cref="ClassifyZone"/>.
    /// </summary>
    public int[] ZoneMinutes { get; set; } = new int[5];

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
            { "StressLevel", (object?)StressLevel },
            { "CardioZone", $"{CardioZoneMinutes} min" },
            { "FatBurnZone", $"{FatBurnZoneMinutes} min" },
            { "Zone1Min", ZoneMinutes[0] },
            { "Zone2Min", ZoneMinutes[1] },
            { "Zone3Min", ZoneMinutes[2] },
            { "Zone4Min", ZoneMinutes[3] },
            { "Zone5Min", ZoneMinutes[4] }
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

    /// <summary>
    /// Classify a BPM reading into a heart rate zone using the standard five-zone model.
    /// </summary>
    /// <param name="bpm">The heart rate reading in beats per minute.</param>
    /// <param name="maxHeartRate">The user's maximum heart rate (e.g. 220 minus age).</param>
    /// <returns>The corresponding <see cref="HeartRateZone"/>.</returns>
    public static HeartRateZone ClassifyZone(int bpm, int maxHeartRate)
    {
        if (maxHeartRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxHeartRate), "Max heart rate must be greater than zero.");

        var pct = bpm / (double)maxHeartRate;
        return pct switch
        {
            < 0.60 => HeartRateZone.Zone1,
            < 0.70 => HeartRateZone.Zone2,
            < 0.80 => HeartRateZone.Zone3,
            < 0.90 => HeartRateZone.Zone4,
            _      => HeartRateZone.Zone5
        };
    }

    /// <summary>
    /// Populate <see cref="ZoneMinutes"/> from Garmin FIT-style zone fields.
    /// Garmin exports provide time-in-zone in seconds for zones indexed 0–4;
    /// this method converts them to whole minutes and stores them correctly.
    /// </summary>
    /// <param name="zoneSeconds">
    /// Array of five elements where index 0 corresponds to zone 1 (lowest intensity)
    /// and index 4 corresponds to zone 5 (highest intensity), each value in seconds.
    /// </param>
    public void SetZoneMinutesFromSeconds(int[] zoneSeconds)
    {
        if (zoneSeconds is null || zoneSeconds.Length != 5)
            throw new ArgumentException("Expected exactly 5 zone values (indices 0–4).", nameof(zoneSeconds));

        for (int i = 0; i < 5; i++)
            ZoneMinutes[i] = zoneSeconds[i] / 60;
    }
}

/// <summary>
/// Single heart rate measurement with timestamp
/// </summary>
public sealed class HeartRateMeasurement
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
    /// Heart rate zone for this measurement (1–5).
    /// Populated by <see cref="HeartRateData.ClassifyZone"/> when max HR is known,
    /// or mapped from device zone index fields (e.g. Garmin FIT hr_zone_X).
    /// </summary>
    public HeartRateZone? Zone { get; set; }

    /// <summary>
    /// Optional accuracy indicator for the measurement
    /// </summary>
    public int? Accuracy { get; set; }
}

