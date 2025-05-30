// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Exceptions;
using HealthDataExportTools.Utilities;

namespace HealthDataExportTools.Services;

/// <summary>
/// Service for validating health data integrity and consistency
/// </summary>
public class ValidationService
{
    /// <summary>
    /// Validate a sleep data record
    /// </summary>
    public ValidationResult ValidateSleepData(SleepData data)
    {
        var result = new ValidationResult();

        if (data.SleepStart >= data.SleepEnd)
            result.AddError("SleepStart must be before SleepEnd");

        if (data.DurationMinutes <= 0)
            result.AddError("DurationMinutes must be positive");

        if (!ValidationHelper.IsValidRecordDate(data.RecordDate))
            result.AddError("RecordDate cannot be in the future");

        if (data.DeepSleepMinutes < 0 || data.LightSleepMinutes < 0 || data.RemSleepMinutes < 0)
            result.AddError("Sleep phase durations must be non-negative");

        var totalPhases = data.DeepSleepMinutes + data.LightSleepMinutes + data.RemSleepMinutes + data.AwakeMinutes;
        if (totalPhases > data.DurationMinutes)
            result.AddError("Sum of sleep phases exceeds total duration");

        if (data.AverageHeartRate.HasValue)
        {
            if (!ValidationHelper.IsValidHeartRate(data.AverageHeartRate.Value))
                result.AddError("AverageHeartRate is out of valid range");
        }

        if (data.Score.HasValue)
        {
            if (!ValidationHelper.IsValidPercentage(data.Score.Value))
                result.AddError("Score must be between 0 and 100");
        }

        if (!data.IsValid())
            result.AddError("Sleep data failed internal validation");

        return result;
    }

    /// <summary>
    /// Validate heart rate data
    /// </summary>
    public ValidationResult ValidateHeartRateData(HeartRateData data)
    {
        var result = new ValidationResult();

        if (!ValidationHelper.IsValidRecordDate(data.RecordDate))
            result.AddError("RecordDate cannot be in the future");

        var hrErrors = ValidationHelper.ValidateHeartRateData(data.MinimumBpm, data.MaximumBpm, data.AverageBpm);
        foreach (var error in hrErrors)
            result.AddError(error);

        if (data.RestingBpm.HasValue)
        {
            if (!ValidationHelper.IsValidRestingHeartRate(data.RestingBpm.Value))
                result.AddError("RestingBpm is out of valid range");
        }

        if (data.CardioZoneMinutes < 0)
            result.AddError("CardioZoneMinutes must be non-negative");

        if (data.FatBurnZoneMinutes < 0)
            result.AddError("FatBurnZoneMinutes must be non-negative");

        if (!data.IsValid())
            result.AddError("Heart rate data failed internal validation");

        return result;
    }

    /// <summary>
    /// Validate SpO2 data
    /// </summary>
    public ValidationResult ValidateSpO2Data(SpO2Data data)
    {
        var result = new ValidationResult();

        if (!ValidationHelper.IsValidRecordDate(data.RecordDate))
            result.AddError("RecordDate cannot be in the future");

        if (!ValidationHelper.IsValidSpO2(data.MinimumPercentage))
            result.AddError("MinimumPercentage must be between 0 and 100");

        if (!ValidationHelper.IsValidSpO2(data.MaximumPercentage))
            result.AddError("MaximumPercentage must be between 0 and 100");

        if (!ValidationHelper.IsValidSpO2(data.AveragePercentage))
            result.AddError("AveragePercentage must be between 0 and 100");

        if (data.MinimumPercentage > data.MaximumPercentage)
            result.AddError("MinimumPercentage cannot be greater than MaximumPercentage");

        if (data.RestingPercentage.HasValue)
        {
            if (!ValidationHelper.IsValidSpO2(data.RestingPercentage.Value))
                result.AddError("RestingPercentage must be between 0 and 100");
        }

        if (!data.IsValid())
            result.AddError("SpO2 data failed internal validation");

        return result;
    }

    /// <summary>
    /// Validate steps data
    /// </summary>
    public ValidationResult ValidateStepsData(StepsData data)
    {
        var result = new ValidationResult();

        if (!ValidationHelper.IsValidRecordDate(data.RecordDate))
            result.AddError("RecordDate cannot be in the future");

        if (data.TotalSteps < 0)
            result.AddError("TotalSteps must be non-negative");

        if (!ValidationHelper.IsValidDistance(data.DistanceKm))
            result.AddError("DistanceKm must be a valid non-negative number");

        if (!ValidationHelper.IsValidCalories(data.CaloriesBurned))
            result.AddError("CaloriesBurned must be between 0 and 100000");

        if (data.DailyGoal < 0)
            result.AddError("DailyGoal must be non-negative");

        if (!ValidationHelper.IsValidPercentageExtended(data.GoalAchievementPercentage))
            result.AddError("GoalAchievementPercentage must be non-negative");

        if (!data.IsValid())
            result.AddError("Steps data failed internal validation");

        return result;
    }

    /// <summary>
    /// Validate activity data
    /// </summary>
    public ValidationResult ValidateActivityData(ActivityData data)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(data.ActivityType))
            result.AddError("ActivityType cannot be empty");

        if (!ValidationHelper.IsValidRecordDate(data.RecordDate))
            result.AddError("RecordDate cannot be in the future");

        if (data.StartTime >= data.EndTime)
            result.AddError("StartTime must be before EndTime");

        if (data.DurationMinutes <= 0)
            result.AddError("DurationMinutes must be positive");

        if (!ValidationHelper.IsValidDistance(data.DistanceKm))
            result.AddError("DistanceKm must be a valid non-negative number");

        if (!ValidationHelper.IsValidCalories(data.CaloriesBurned))
            result.AddError("CaloriesBurned must be between 0 and 100000");

        if (data.AverageHeartRate.HasValue && !ValidationHelper.IsValidHeartRate(data.AverageHeartRate.Value))
            result.AddError("AverageHeartRate is out of valid range");

        if (data.MaximumHeartRate.HasValue && !ValidationHelper.IsValidHeartRate(data.MaximumHeartRate.Value))
            result.AddError("MaximumHeartRate is out of valid range");

        if (data.AverageHeartRate.HasValue && data.MaximumHeartRate.HasValue &&
            data.AverageHeartRate > data.MaximumHeartRate)
            result.AddError("AverageHeartRate cannot be greater than MaximumHeartRate");

        if (data.ElevationGainMeters.HasValue && !ValidationHelper.IsValidElevation(data.ElevationGainMeters.Value))
            result.AddError("ElevationGainMeters must be between 0 and 9000");

        if (!data.IsValid())
            result.AddError("Activity data failed internal validation");

        return result;
    }

    /// <summary>
    /// Validate health metric
    /// </summary>
    public ValidationResult ValidateHealthMetric(HealthMetric metric)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(metric.MetricName))
            result.AddError("MetricName cannot be empty");

        if (string.IsNullOrWhiteSpace(metric.Unit))
            result.AddError("Unit cannot be empty");

        if (metric.Value < 0)
            result.AddError("Value must be non-negative");

        if (metric.NormalRangeLow.HasValue && metric.NormalRangeHigh.HasValue &&
            metric.NormalRangeLow > metric.NormalRangeHigh)
            result.AddError("NormalRangeLow cannot be greater than NormalRangeHigh");

        if (!metric.IsValid())
            result.AddError("Health metric failed internal validation");

        return result;
    }
}

/// <summary>
/// Result of a validation operation
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// List of validation errors
    /// </summary>
    public List<string> Errors { get; } = [];

    /// <summary>
    /// Check if validation passed
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Add an error to the result
    /// </summary>
    public void AddError(string error)
    {
        Errors.Add(error);
    }

    /// <summary>
    /// Get error summary
    /// </summary>
    public override string ToString()
    {
        if (IsValid) return "Validation passed";
        return $"Validation failed with {Errors.Count} error(s):\n" +
               string.Join("\n", Errors.Select(e => $"  - {e}"));
    }
}
