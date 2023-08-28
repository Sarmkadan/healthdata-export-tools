#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Services;

/// <summary>
/// Service for comparing two distinct sets of health data records.
/// </summary>
public sealed class DataComparisonService
{
    /// <summary>
    /// Compare two periods of health data to determine trends and changes.
    /// </summary>
    public Task<DataComparisonResult> ComparePeriodsAsync(
        HealthDataCollection period1,
        HealthDataCollection period2)
    {
        var result = new DataComparisonResult
        {
            Period1RecordCount = period1.GetTotalRecordCount(),
            Period2RecordCount = period2.GetTotalRecordCount()
        };

        // Compare Sleep
        if (period1.SleepRecords.Count > 0 && period2.SleepRecords.Count > 0)
        {
            result.Period1AverageSleepMinutes = period1.SleepRecords.Average(s => s.DurationMinutes);
            result.Period2AverageSleepMinutes = period2.SleepRecords.Average(s => s.DurationMinutes);
            result.SleepDurationChangePercentage = CalculatePercentageChange(
                result.Period1AverageSleepMinutes,
                result.Period2AverageSleepMinutes);
        }

        // Compare Heart Rate
        if (period1.HeartRateRecords.Count > 0 && period2.HeartRateRecords.Count > 0)
        {
            result.Period1AverageHeartRate = period1.HeartRateRecords.Average(h => h.AverageBpm);
            result.Period2AverageHeartRate = period2.HeartRateRecords.Average(h => h.AverageBpm);
            result.HeartRateChangePercentage = CalculatePercentageChange(
                result.Period1AverageHeartRate,
                result.Period2AverageHeartRate);
        }

        // Compare Steps
        if (period1.StepsRecords.Count > 0 && period2.StepsRecords.Count > 0)
        {
            result.Period1AverageSteps = period1.StepsRecords.Average(st => st.TotalSteps);
            result.Period2AverageSteps = period2.StepsRecords.Average(st => st.TotalSteps);
            result.StepsChangePercentage = CalculatePercentageChange(
                result.Period1AverageSteps,
                result.Period2AverageSteps);
        }

        return Task.FromResult(result);
    }

    private double CalculatePercentageChange(double oldVal, double newVal)
    {
        if (Math.Abs(oldVal) < 0.001) return newVal > 0 ? 100.0 : 0.0;
        return ((newVal - oldVal) / oldVal) * 100.0;
    }
}

/// <summary>
/// Contains the result of comparing two data periods.
/// </summary>
public sealed class DataComparisonResult
{
    public int Period1RecordCount { get; set; }
    public int Period2RecordCount { get; set; }
    
    public double Period1AverageSleepMinutes { get; set; }
    public double Period2AverageSleepMinutes { get; set; }
    public double SleepDurationChangePercentage { get; set; }

    public double Period1AverageHeartRate { get; set; }
    public double Period2AverageHeartRate { get; set; }
    public double HeartRateChangePercentage { get; set; }

    public double Period1AverageSteps { get; set; }
    public double Period2AverageSteps { get; set; }
    public double StepsChangePercentage { get; set; }
}
