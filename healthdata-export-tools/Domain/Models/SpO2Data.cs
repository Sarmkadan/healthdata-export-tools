// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Domain.Models;

/// <summary>
/// Blood oxygen saturation (SpO2) measurements for a day
/// </summary>
public class SpO2Data : HealthDataRecord
{
    /// <summary>
    /// Minimum SpO2 percentage recorded (0-100%)
    /// </summary>
    public int MinimumPercentage { get; set; }

    /// <summary>
    /// Maximum SpO2 percentage recorded (0-100%)
    /// </summary>
    public int MaximumPercentage { get; set; }

    /// <summary>
    /// Average SpO2 percentage for the day (0-100%)
    /// </summary>
    public int AveragePercentage { get; set; }

    /// <summary>
    /// SpO2 reading at rest or morning measurement (0-100%)
    /// </summary>
    public int? RestingPercentage { get; set; }

    /// <summary>
    /// Number of SpO2 measurements taken
    /// </summary>
    public int MeasurementCount { get; set; }

    /// <summary>
    /// Detailed measurements with timestamps
    /// </summary>
    public List<SpO2Measurement> Measurements { get; set; } = [];

    /// <summary>
    /// Number of times SpO2 dropped below 95% (potential concern threshold)
    /// </summary>
    public int LowSpO2Events { get; set; }

    /// <summary>
    /// Lowest SpO2 reading if device detected concerning levels
    /// </summary>
    public int? LowestAlertValue { get; set; }

    /// <summary>
    /// Measurement reliability score (0-100) if provided by device
    /// </summary>
    public int? ReliabilityScore { get; set; }

    /// <summary>
    /// Validate SpO2 data ranges
    /// </summary>
    public override bool IsValid()
    {
        if (MinimumPercentage < 0 || MinimumPercentage > 100) return false;
        if (MaximumPercentage < 0 || MaximumPercentage > 100) return false;
        if (AveragePercentage < 0 || AveragePercentage > 100) return false;
        if (MinimumPercentage > MaximumPercentage) return false;
        if (AveragePercentage < MinimumPercentage || AveragePercentage > MaximumPercentage) return false;
        if (RestingPercentage.HasValue && (RestingPercentage < 0 || RestingPercentage > 100)) return false;
        if (MeasurementCount < 0) return false;

        return true;
    }

    /// <summary>
    /// Get summary of SpO2 data
    /// </summary>
    public override Dictionary<string, object> GetSummary()
    {
        return new()
        {
            { "Date", RecordDate.ToString("yyyy-MM-dd") },
            { "Minimum", $"{MinimumPercentage}%" },
            { "Average", $"{AveragePercentage}%" },
            { "Maximum", $"{MaximumPercentage}%" },
            { "Resting", RestingPercentage.HasValue ? $"{RestingPercentage}%" : "N/A" },
            { "Measurements", MeasurementCount },
            { "LowSpO2Events", LowSpO2Events },
            { "LowestAlert", LowestAlertValue },
            { "Reliability", $"{ReliabilityScore}%" }
        };
    }

    /// <summary>
    /// Add a SpO2 measurement
    /// </summary>
    public void AddMeasurement(SpO2Measurement measurement)
    {
        Measurements.Add(measurement);
        MeasurementCount = Measurements.Count;

        if (measurement.Percentage < 95)
            LowSpO2Events++;

        Touch();
    }

    /// <summary>
    /// Assess if SpO2 levels are concerning
    /// </summary>
    public bool HasConcerningLevels()
    {
        return MinimumPercentage < 90 || LowSpO2Events > 5;
    }

    /// <summary>
    /// Calculate percentage of measurements below threshold
    /// </summary>
    public double GetPercentageBelowThreshold(int threshold)
    {
        if (MeasurementCount == 0) return 0;
        var below = Measurements.Count(m => m.Percentage < threshold);
        return (below / (double)MeasurementCount) * 100;
    }
}

/// <summary>
/// Single SpO2 measurement with timestamp
/// </summary>
public class SpO2Measurement
{
    /// <summary>
    /// Timestamp of the measurement
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Blood oxygen saturation percentage (0-100%)
    /// </summary>
    public int Percentage { get; set; }

    /// <summary>
    /// Optional confidence score for the measurement
    /// </summary>
    public int? Confidence { get; set; }
}
