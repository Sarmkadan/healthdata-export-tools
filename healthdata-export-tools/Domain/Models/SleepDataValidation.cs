#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;
using HealthDataExportTools.Domain.Enums;

namespace HealthDataExportTools.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="SleepData"/> instances
/// </summary>
public static class SleepDataValidation
{
    /// <summary>
    /// Validates a SleepData instance and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The SleepData instance to validate</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this SleepData value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate SleepStart and SleepEnd
        if (value.SleepStart == default)
        {
            errors.Add("SleepStart must be set to a valid date and time");
        }

        if (value.SleepEnd == default)
        {
            errors.Add("SleepEnd must be set to a valid date and time");
        }
        else if (value.SleepStart != default && value.SleepStart >= value.SleepEnd)
        {
            errors.Add("SleepStart must be before SleepEnd");
        }

        // Validate DurationMinutes
        if (value.DurationMinutes <= 0)
        {
            errors.Add("DurationMinutes must be greater than 0");
        }

        // Validate sleep stage durations
        if (value.DeepSleepMinutes < 0)
        {
            errors.Add("DeepSleepMinutes cannot be negative");
        }

        if (value.LightSleepMinutes < 0)
        {
            errors.Add("LightSleepMinutes cannot be negative");
        }

        if (value.RemSleepMinutes < 0)
        {
            errors.Add("RemSleepMinutes cannot be negative");
        }

        if (value.AwakeMinutes < 0)
        {
            errors.Add("AwakeMinutes cannot be negative");
        }

        // Validate that sleep stages don't exceed total duration (allow small floating point tolerance)
        var totalSleepStages = value.DeepSleepMinutes + value.LightSleepMinutes + value.RemSleepMinutes + value.AwakeMinutes;
        if (totalSleepStages > value.DurationMinutes + 0.01)
        {
            errors.Add("Sum of sleep stage durations (Deep + Light + REM + Awake) cannot exceed DurationMinutes");
        }

        // Validate Score range (0-100)
        if (value.Score is < 0 or > 100)
        {
            errors.Add("Score must be between 0 and 100 when set");
        }

        // Validate CycleCount (reasonable range for sleep cycles)
        if (value.CycleCount is <= 0 or > 10)
        {
            errors.Add("CycleCount must be between 1 and 10 when set");
        }

        // Validate AverageHeartRate
        if (value.AverageHeartRate is < 30 or > 200)
        {
            errors.Add("AverageHeartRate must be between 30 and 200 when set");
        }

        // Validate Quality enum value
        if (!Enum.IsDefined(typeof(SleepQuality), value.Quality))
        {
            errors.Add("Quality must be a valid SleepQuality enum value");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if a SleepData instance is valid
    /// </summary>
    /// <param name="value">The SleepData instance to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static bool IsValid(this SleepData value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a SleepData instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The SleepData instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value contains validation errors</exception>
    public static void EnsureValid(this SleepData value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"SleepData validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}