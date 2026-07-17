// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Services;

namespace HealthDataExportTools.Benchmarks;

/// <summary>
/// Provides validation extensions for <see cref="MockValidationService"/> to enable comprehensive
/// validation of mock validation service instances in benchmark scenarios.
/// </summary>
public static class MockValidationServiceValidation
{
    /// <summary>
    /// Validates a <see cref="MockValidationService"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The mock validation service to validate.</param>
    /// <returns>A read-only list of validation error messages (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this MockValidationService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // ValidateSleepData
        if (value.ValidateSleepData(null) is { IsValid: false } sleepResult)
        {
            errors.AddRange(sleepResult.Errors);
        }

        // ValidateHeartRateData
        if (value.ValidateHeartRateData(null) is { IsValid: false } hrResult)
        {
            errors.AddRange(hrResult.Errors);
        }

        // ValidateSpO2Data
        if (value.ValidateSpO2Data(null) is { IsValid: false } spo2Result)
        {
            errors.AddRange(spo2Result.Errors);
        }

        // ValidateStepsData
        if (value.ValidateStepsData(null) is { IsValid: false } stepsResult)
        {
            errors.AddRange(stepsResult.Errors);
        }

        // ValidateActivityData
        if (value.ValidateActivityData(null) is { IsValid: false } activityResult)
        {
            errors.AddRange(activityResult.Errors);
        }

        // ValidateHealthMetric
        if (value.ValidateHealthMetric(null) is { IsValid: false } metricResult)
        {
            errors.AddRange(metricResult.Errors);
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if a <see cref="MockValidationService"/> instance is valid.
    /// </summary>
    /// <param name="value">The mock validation service to check.</param>
    /// <returns>True if the service is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this MockValidationService? value) => value?.Validate() is { Count: 0 };

    /// <summary>
    /// Ensures that a <see cref="MockValidationService"/> instance is valid,
    /// throwing an <see cref="ArgumentException"/> with detailed error messages if not.
    /// </summary>
    /// <param name="value">The mock validation service to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the service is invalid, containing all validation errors.</exception>
    public static void EnsureValid(this MockValidationService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"MockValidationService is invalid. Validation found {errors.Count} error(s):{Environment.NewLine}{string.Join(Environment.NewLine, errors.Select((e, i) => $" {i + 1}. {e}"))}");
        }
    }

    /// <summary>
    /// Validates <see cref="SleepData"/> and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The sleep data to validate.</param>
    /// <returns>A read-only list of validation error messages (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SleepData? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Date validation
        if (value.RecordDate > DateTime.UtcNow.AddDays(1))
        {
            errors.Add("SleepData.RecordDate cannot be in the future");
        }

        if (value.RecordDate == default)
        {
            errors.Add("SleepData.RecordDate must be set (cannot be default)");
        }
        else if (value.RecordDate.Kind != DateTimeKind.Utc)
        {
            errors.Add("SleepData.RecordDate must be in UTC format");
        }

        // Time range validation
        if (value.SleepStart >= value.SleepEnd)
        {
            errors.Add("SleepData.SleepStart must be before SleepData.SleepEnd");
        }

        if (value.SleepStart == default)
        {
            errors.Add("SleepData.SleepStart must be set (cannot be default)");
        }
        else if (value.SleepStart.Kind != DateTimeKind.Utc)
        {
            errors.Add("SleepData.SleepStart must be in UTC format");
        }

        if (value.SleepEnd == default)
        {
            errors.Add("SleepData.SleepEnd must be set (cannot be default)");
        }
        else if (value.SleepEnd.Kind != DateTimeKind.Utc)
        {
            errors.Add("SleepData.SleepEnd must be in UTC format");
        }

        // Duration validation
        if (value.DurationMinutes <= 0)
        {
            errors.Add("SleepData.DurationMinutes must be positive");
        }

        // Sleep phase durations
        if (value.DeepSleepMinutes < 0)
        {
            errors.Add("SleepData.DeepSleepMinutes must be non-negative");
        }

        if (value.LightSleepMinutes < 0)
        {
            errors.Add("SleepData.LightSleepMinutes must be non-negative");
        }

        if (value.RemSleepMinutes < 0)
        {
            errors.Add("SleepData.RemSleepMinutes must be non-negative");
        }

        if (value.AwakeMinutes < 0)
        {
            errors.Add("SleepData.AwakeMinutes must be non-negative");
        }

        // Phase sum validation
        var totalPhases = value.DeepSleepMinutes + value.LightSleepMinutes + value.RemSleepMinutes + value.AwakeMinutes;
        if (totalPhases > value.DurationMinutes)
        {
            errors.Add("Sum of SleepData sleep phases exceeds total DurationMinutes");
        }

        // Heart rate validation
        if (value.AverageHeartRate.HasValue)
        {
            if (value.AverageHeartRate < 30 || value.AverageHeartRate > 200)
            {
                errors.Add("SleepData.AverageHeartRate must be between 30 and 200 BPM");
            }
        }

        // Score validation
        if (value.Score.HasValue)
        {
            if (value.Score < 0 || value.Score > 100)
            {
                errors.Add("SleepData.Score must be between 0 and 100");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates <see cref="HeartRateData"/> and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The heart rate data to validate.</param>
    /// <returns>A read-only list of validation error messages (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this HeartRateData? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Date validation
        if (value.RecordDate > DateTime.UtcNow.AddDays(1))
        {
            errors.Add("HeartRateData.RecordDate cannot be in the future");
        }

        if (value.RecordDate == default)
        {
            errors.Add("HeartRateData.RecordDate must be set (cannot be default)");
        }
        else if (value.RecordDate.Kind != DateTimeKind.Utc)
        {
            errors.Add("HeartRateData.RecordDate must be in UTC format");
        }

        // Heart rate range validation
        if (value.MinimumBpm < 30 || value.MinimumBpm > 200)
        {
            errors.Add("HeartRateData.MinimumBpm must be between 30 and 200 BPM");
        }

        if (value.MaximumBpm < 30 || value.MaximumBpm > 220)
        {
            errors.Add("HeartRateData.MaximumBpm must be between 30 and 220 BPM");
        }

        if (value.AverageBpm < 30 || value.AverageBpm > 200)
        {
            errors.Add("HeartRateData.AverageBpm must be between 30 and 200 BPM");
        }

        // Range consistency
        if (value.MinimumBpm > value.MaximumBpm)
        {
            errors.Add("HeartRateData.MinimumBpm cannot be greater than MaximumBpm");
        }

        if (value.AverageBpm < value.MinimumBpm || value.AverageBpm > value.MaximumBpm)
        {
            errors.Add("HeartRateData.AverageBpm must be between MinimumBpm and MaximumBpm");
        }

        // Resting heart rate validation
        if (value.RestingBpm.HasValue)
        {
            if (value.RestingBpm < 30 || value.RestingBpm > 100)
            {
                errors.Add("HeartRateData.RestingBpm must be between 30 and 100 BPM");
            }
        }

        // Measurement count
        if (value.MeasurementCount < 0)
        {
            errors.Add("HeartRateData.MeasurementCount must be non-negative");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates <see cref="SpO2Data"/> and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The SpO2 data to validate.</param>
    /// <returns>A read-only list of validation error messages (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SpO2Data? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Date validation
        if (value.RecordDate > DateTime.UtcNow.AddDays(1))
        {
            errors.Add("SpO2Data.RecordDate cannot be in the future");
        }

        if (value.RecordDate == default)
        {
            errors.Add("SpO2Data.RecordDate must be set (cannot be default)");
        }
        else if (value.RecordDate.Kind != DateTimeKind.Utc)
        {
            errors.Add("SpO2Data.RecordDate must be in UTC format");
        }

        // SpO2 percentage validation (0-100 range)
        if (value.MinimumPercentage < 0 || value.MinimumPercentage > 100)
        {
            errors.Add("SpO2Data.MinimumPercentage must be between 0 and 100");
        }

        if (value.MaximumPercentage < 0 || value.MaximumPercentage > 100)
        {
            errors.Add("SpO2Data.MaximumPercentage must be between 0 and 100");
        }

        if (value.AveragePercentage < 0 || value.AveragePercentage > 100)
        {
            errors.Add("SpO2Data.AveragePercentage must be between 0 and 100");
        }

        // Range consistency
        if (value.MinimumPercentage > value.MaximumPercentage)
        {
            errors.Add("SpO2Data.MinimumPercentage cannot be greater than MaximumPercentage");
        }

        if (value.AveragePercentage < value.MinimumPercentage || value.AveragePercentage > value.MaximumPercentage)
        {
            errors.Add("SpO2Data.AveragePercentage must be between MinimumPercentage and MaximumPercentage");
        }

        // Resting SpO2 validation
        if (value.RestingPercentage.HasValue)
        {
            if (value.RestingPercentage < 0 || value.RestingPercentage > 100)
            {
                errors.Add("SpO2Data.RestingPercentage must be between 0 and 100");
            }
        }

        // Measurement count
        if (value.MeasurementCount < 0)
        {
            errors.Add("SpO2Data.MeasurementCount must be non-negative");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates <see cref="StepsData"/> and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The steps data to validate.</param>
    /// <returns>A read-only list of validation error messages (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this StepsData? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Date validation
        if (value.RecordDate > DateTime.UtcNow.AddDays(1))
        {
            errors.Add("StepsData.RecordDate cannot be in the future");
        }

        if (value.RecordDate == default)
        {
            errors.Add("StepsData.RecordDate must be set (cannot be default)");
        }
        else if (value.RecordDate.Kind != DateTimeKind.Utc)
        {
            errors.Add("StepsData.RecordDate must be in UTC format");
        }

        // Steps validation
        if (value.TotalSteps < 0)
        {
            errors.Add("StepsData.TotalSteps must be non-negative");
        }

        // Distance validation
        if (value.DistanceKm < 0)
        {
            errors.Add("StepsData.DistanceKm must be non-negative");
        }

        // Calories validation
        if (value.CaloriesBurned < 0)
        {
            errors.Add("StepsData.CaloriesBurned must be non-negative");
        }

        // Goal validation
        if (value.DailyGoal < 0)
        {
            errors.Add("StepsData.DailyGoal must be non-negative");
        }

        if (value.GoalAchievementPercentage < 0)
        {
            errors.Add("StepsData.GoalAchievementPercentage must be non-negative");
        }

        // Active minutes
        if (value.ActiveMinutes < 0)
        {
            errors.Add("StepsData.ActiveMinutes must be non-negative");
        }

        // Cadence validation
        if (value.AverageCadence.HasValue)
        {
            if (value.AverageCadence < 0 || value.AverageCadence > 200)
            {
                errors.Add("StepsData.AverageCadence must be between 0 and 200 steps per minute");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates <see cref="ActivityData"/> and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The activity data to validate.</param>
    /// <returns>A read-only list of validation error messages (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ActivityData? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Date validation
        if (value.RecordDate > DateTime.UtcNow.AddDays(1))
        {
            errors.Add("ActivityData.RecordDate cannot be in the future");
        }

        if (value.RecordDate == default)
        {
            errors.Add("ActivityData.RecordDate must be set (cannot be default)");
        }
        else if (value.RecordDate.Kind != DateTimeKind.Utc)
        {
            errors.Add("ActivityData.RecordDate must be in UTC format");
        }

        // Activity type validation
        if (string.IsNullOrWhiteSpace(value.ActivityType))
        {
            errors.Add("ActivityData.ActivityType cannot be empty or whitespace");
        }

        // Time range validation
        if (value.StartTime >= value.EndTime)
        {
            errors.Add("ActivityData.StartTime must be before ActivityData.EndTime");
        }

        if (value.StartTime == default)
        {
            errors.Add("ActivityData.StartTime must be set (cannot be default)");
        }
        else if (value.StartTime.Kind != DateTimeKind.Utc)
        {
            errors.Add("ActivityData.StartTime must be in UTC format");
        }

        if (value.EndTime == default)
        {
            errors.Add("ActivityData.EndTime must be set (cannot be default)");
        }
        else if (value.EndTime.Kind != DateTimeKind.Utc)
        {
            errors.Add("ActivityData.EndTime must be in UTC format");
        }

        // Duration validation
        if (value.DurationMinutes <= 0)
        {
            errors.Add("ActivityData.DurationMinutes must be positive");
        }

        // Distance validation
        if (value.DistanceKm < 0)
        {
            errors.Add("ActivityData.DistanceKm must be non-negative");
        }

        // Calories validation
        if (value.CaloriesBurned < 0)
        {
            errors.Add("ActivityData.CaloriesBurned must be non-negative");
        }

        // Heart rate validation
        if (value.AverageHeartRate.HasValue)
        {
            if (value.AverageHeartRate < 30 || value.AverageHeartRate > 220)
            {
                errors.Add("ActivityData.AverageHeartRate must be between 30 and 220 BPM");
            }
        }

        if (value.MaximumHeartRate.HasValue)
        {
            if (value.MaximumHeartRate < 30 || value.MaximumHeartRate > 220)
            {
                errors.Add("ActivityData.MaximumHeartRate must be between 30 and 220 BPM");
            }
        }

        if (value.AverageHeartRate.HasValue && value.MaximumHeartRate.HasValue &&
            value.AverageHeartRate > value.MaximumHeartRate)
        {
            errors.Add("ActivityData.AverageHeartRate cannot be greater than MaximumHeartRate");
        }

        // Elevation validation
        if (value.ElevationGainMeters.HasValue)
        {
            if (value.ElevationGainMeters < 0 || value.ElevationGainMeters > 9000)
            {
                errors.Add("ActivityData.ElevationGainMeters must be between 0 and 9000 meters");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates <see cref="HealthMetric"/> and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The health metric to validate.</param>
    /// <returns>A read-only list of validation error messages (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this HealthMetric? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Date validation
        if (value.RecordDate > DateTime.UtcNow.AddDays(1))
        {
            errors.Add("HealthMetric.RecordDate cannot be in the future");
        }

        if (value.RecordDate == default)
        {
            errors.Add("HealthMetric.RecordDate must be set (cannot be default)");
        }
        else if (value.RecordDate.Kind != DateTimeKind.Utc)
        {
            errors.Add("HealthMetric.RecordDate must be in UTC format");
        }

        // Metric name validation
        if (string.IsNullOrWhiteSpace(value.MetricName))
        {
            errors.Add("HealthMetric.MetricName cannot be empty or whitespace");
        }

        // Unit validation
        if (string.IsNullOrWhiteSpace(value.Unit))
        {
            errors.Add("HealthMetric.Unit cannot be empty or whitespace");
        }

        // Value validation
        if (value.Value < 0)
        {
            errors.Add("HealthMetric.Value must be non-negative");
        }

        // Normal range validation
        if (value.NormalRangeLow.HasValue && value.NormalRangeHigh.HasValue &&
            value.NormalRangeLow > value.NormalRangeHigh)
        {
            errors.Add("HealthMetric.NormalRangeLow cannot be greater than NormalRangeHigh");
        }

        return errors.AsReadOnly();
    }
}