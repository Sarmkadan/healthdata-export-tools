#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using HealthDataExportTools.Domain.Enums;

namespace HealthDataExportTools.Domain.Models;

/// <summary>
/// Extension methods for <see cref="SleepData"/> providing additional functionality and convenience methods
/// for calculating sleep metrics, quality assessments, and formatting.
/// </summary>
public static class SleepDataExtensions
{
    /// <summary>
    /// Calculates the percentage of light sleep from total sleep duration.
    /// </summary>
    /// <param name="sleepData">The sleep data instance. Cannot be <see langword="null"/>.</param>
    /// <returns>Percentage of light sleep (0-100).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sleepData"/> is <see langword="null"/>.</exception>
    public static double GetLightSleepPercentage(this SleepData sleepData)
    {
        ArgumentNullException.ThrowIfNull(sleepData);

        if (sleepData.DurationMinutes <= 0)
            return 0;

        return (double)sleepData.LightSleepMinutes / sleepData.DurationMinutes * 100;
    }


    /// <summary>
    /// Calculates the percentage of awake time from total sleep duration.
    /// </summary>
    /// <param name="sleepData">The sleep data instance. Cannot be <see langword="null"/>.</param>
    /// <returns>Percentage of awake time (0-100).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sleepData"/> is <see langword="null"/>.</exception>
    public static double GetAwakePercentage(this SleepData sleepData)
    {
        ArgumentNullException.ThrowIfNull(sleepData);

        if (sleepData.DurationMinutes <= 0)
            return 0;

        return (double)sleepData.AwakeMinutes / sleepData.DurationMinutes * 100;
    }

    /// <summary>
    /// Calculates sleep efficiency score (ratio of actual sleep time to total time in bed).
    /// </summary>
    /// <param name="sleepData">The sleep data instance. Cannot be <see langword="null"/>.</param>
    /// <returns>Sleep efficiency as percentage (0-100), <see langword="null"/> if duration is invalid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sleepData"/> is <see langword="null"/>.</exception>
    public static double? GetSleepEfficiency(this SleepData sleepData)
    {
        ArgumentNullException.ThrowIfNull(sleepData);

        if (sleepData.DurationMinutes <= 0 || sleepData.AwakeMinutes < 0)
            return null;


        var sleepTime = sleepData.DurationMinutes - sleepData.AwakeMinutes;
        return (double)sleepTime / sleepData.DurationMinutes * 100;
    }

    /// <summary>
    /// Determines if this sleep session was restorative based on quality and duration.
    /// </summary>
    /// <param name="sleepData">The sleep data instance. Cannot be <see langword="null"/>.</param>
    /// <returns>True if sleep was restorative (Excellent/Good quality and sufficient duration).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sleepData"/> is <see langword="null"/>.</exception>
    public static bool IsRestorativeSleep(this SleepData sleepData)
    {
        ArgumentNullException.ThrowIfNull(sleepData);

        return sleepData.Quality switch
        {
            SleepQuality.Excellent => sleepData.DurationMinutes >= 420,
            SleepQuality.Good => sleepData.DurationMinutes >= 480,
            _ => false
        };
    }

    /// <summary>
    /// Gets sleep debt in minutes (difference between actual sleep and recommended 8 hours).
    /// </summary>
    /// <param name="sleepData">The sleep data instance. Cannot be <see langword="null"/>.</param>
    /// <returns>Sleep debt in minutes (negative if overslept), <see langword="null"/> if duration is invalid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sleepData"/> is <see langword="null"/>.</exception>
    public static int? GetSleepDebt(this SleepData sleepData)
    {
        ArgumentNullException.ThrowIfNull(sleepData);

        if (sleepData.DurationMinutes <= 0)
            return null;


        const int recommendedHours = 8;
        const int recommendedMinutes = recommendedHours * 60;

        return sleepData.DurationMinutes - recommendedMinutes;
    }

    /// <summary>
    /// Calculates the ratio of deep sleep to REM sleep.
    /// </summary>
    /// <param name="sleepData">The sleep data instance. Cannot be <see langword="null"/>.</param>
    /// <returns>Ratio of deep sleep to REM sleep, <see langword="null"/> if REM sleep is 0.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sleepData"/> is <see langword="null"/>.</exception>
    public static double? GetDeepToRemRatio(this SleepData sleepData)
    {
        ArgumentNullException.ThrowIfNull(sleepData);

        if (sleepData.RemSleepMinutes <= 0)
            return null;


        return (double)sleepData.DeepSleepMinutes / sleepData.RemSleepMinutes;
    }

    /// <summary>
    /// Gets formatted sleep duration as "HH:MM" string.
    /// </summary>
    /// <param name="sleepData">The sleep data instance. Cannot be <see langword="null"/>.</param>
    /// <returns>Formatted time string (e.g., "07:30" for 7 hours 30 minutes).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sleepData"/> is <see langword="null"/>.</exception>
    public static string GetFormattedDuration(this SleepData sleepData)
    {
        ArgumentNullException.ThrowIfNull(sleepData);

        var hours = sleepData.DurationMinutes / 60;
        var minutes = sleepData.DurationMinutes % 60;
        return $"{hours:D2}:{minutes:D2}";
    }

    /// <summary>
    /// Checks if sleep duration meets minimum recommended duration for adults (7 hours).
    /// </summary>
    /// <param name="sleepData">The sleep data instance. Cannot be <see langword="null"/>.</param>
    /// <returns>True if sleep duration is at least 7 hours.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sleepData"/> is <see langword="null"/>.</exception>
    public static bool MeetsMinimumDuration(this SleepData sleepData)
    {
        ArgumentNullException.ThrowIfNull(sleepData);
        return sleepData.DurationMinutes >= 420; // 7 hours = 420 minutes
    }

    /// <summary>
    /// Calculates a weighted sleep score combining duration, quality, and consistency metrics.
    /// </summary>
    /// <param name="sleepData">The sleep data instance. Cannot be <see langword="null"/>.</param>
    /// <returns>Weighted sleep score (0-100), <see langword="null"/> if duration is invalid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sleepData"/> is <see langword="null"/>.</exception>
    public static double? CalculateWeightedSleepScore(this SleepData sleepData)
    {
        ArgumentNullException.ThrowIfNull(sleepData);

        if (sleepData.DurationMinutes <= 0)
            return null;

        // Base score from duration (50% weight)
        var durationScore = Math.Min(100, (double)sleepData.DurationMinutes / 8); // 8 hours max

        // Quality score (30% weight)
        var qualityScore = sleepData.Quality switch
        {
            SleepQuality.Excellent => 100,
            SleepQuality.Good => 80,
            SleepQuality.Average => 60,
            SleepQuality.Poor => 30,
            _ => 50
        };

        // Consistency score (20% weight) - based on sleep efficiency
        var efficiencyScore = sleepData.GetSleepEfficiency() ?? 70;

        // Calculate weighted average
        var weightedScore = durationScore * 0.5 + qualityScore * 0.3 + efficiencyScore * 0.2;

        return Math.Round(weightedScore, 1);
    }
}