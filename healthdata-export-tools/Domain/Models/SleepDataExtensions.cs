#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using HealthDataExportTools.Domain.Enums;

namespace HealthDataExportTools.Domain.Models;

/// <summary>
/// Extension methods for SleepData providing additional functionality and convenience methods
/// </summary>
public static class SleepDataExtensions
{
    /// <summary>
    /// Calculate light sleep percentage from total sleep duration
    /// </summary>
    /// <param name="sleepData">The sleep data instance</param>
    /// <returns>Percentage of light sleep (0-100)</returns>
    public static double GetLightSleepPercentage(this SleepData sleepData)
    {
        if (sleepData.DurationMinutes <= 0)
            return 0;

        return (double)sleepData.LightSleepMinutes / sleepData.DurationMinutes * 100;
    }


    /// <summary>
    /// Calculate awake percentage from total sleep duration
    /// </summary>
    /// <param name="sleepData">The sleep data instance</param>
    /// <returns>Percentage of awake time (0-100)</returns>
    public static double GetAwakePercentage(this SleepData sleepData)
    {
        if (sleepData.DurationMinutes <= 0)
            return 0;

        return (double)sleepData.AwakeMinutes / sleepData.DurationMinutes * 100;
    }

    /// <summary>
    /// Calculate sleep efficiency score (ratio of actual sleep time to total time in bed)
    /// </summary>
    /// <param name="sleepData">The sleep data instance</param>
    /// <returns>Sleep efficiency as percentage (0-100), null if duration is invalid</returns>
    public static double? GetSleepEfficiency(this SleepData sleepData)
    {
        if (sleepData.DurationMinutes <= 0 || sleepData.AwakeMinutes < 0)
            return null;

        var sleepTime = sleepData.DurationMinutes - sleepData.AwakeMinutes;
        return (double)sleepTime / sleepData.DurationMinutes * 100;
    }

    /// <summary>
    /// Determine if this sleep session was restorative based on quality and duration
    /// </summary>
    /// <param name="sleepData">The sleep data instance</param>
    /// <returns>True if sleep was restorative (Excellent/Good quality and sufficient duration)</returns>
    public static bool IsRestorativeSleep(this SleepData sleepData)
    {
        if (sleepData.Quality == SleepQuality.Excellent && sleepData.DurationMinutes >= 420)
            return true;

        if (sleepData.Quality == SleepQuality.Good && sleepData.DurationMinutes >= 480)
            return true;

        return false;
    }

    /// <summary>
    /// Get sleep debt in minutes (difference between actual sleep and recommended 8 hours)
    /// </summary>
    /// <param name="sleepData">The sleep data instance</param>
    /// <returns>Sleep debt in minutes (negative if overslept), null if duration is invalid</returns>
    public static int? GetSleepDebt(this SleepData sleepData)
    {
        if (sleepData.DurationMinutes <= 0)
            return null;

        const int recommendedHours = 8;
        const int recommendedMinutes = recommendedHours * 60;

        return sleepData.DurationMinutes - recommendedMinutes;
    }

    /// <summary>
    /// Calculate the ratio of deep sleep to REM sleep
    /// </summary>
    /// <param name="sleepData">The sleep data instance</param>
    /// <returns>Ratio of deep sleep to REM sleep, null if REM sleep is 0</returns>
    public static double? GetDeepToRemRatio(this SleepData sleepData)
    {
        if (sleepData.RemSleepMinutes <= 0)
            return null;

        return (double)sleepData.DeepSleepMinutes / sleepData.RemSleepMinutes;
    }

    /// <summary>
    /// Get formatted sleep duration as "HH:MM" string
    /// </summary>
    /// <param name="sleepData">The sleep data instance</param>
    /// <returns>Formatted time string (e.g., "07:30" for 7 hours 30 minutes)</returns>
    public static string GetFormattedDuration(this SleepData sleepData)
    {
        var hours = sleepData.DurationMinutes / 60;
        var minutes = sleepData.DurationMinutes % 60;
        return $"{hours:D2}:{minutes:D2}";
    }

    /// <summary>
    /// Check if sleep duration meets minimum recommended duration for adults (7 hours)
    /// </summary>
    /// <param name="sleepData">The sleep data instance</param>
    /// <returns>True if sleep duration is at least 7 hours</returns>
    public static bool MeetsMinimumDuration(this SleepData sleepData)
    {
        return sleepData.DurationMinutes >= 420; // 7 hours = 420 minutes
    }

    /// <summary>
    /// Calculate weighted sleep score combining duration, quality, and consistency metrics
    /// </summary>
    /// <param name="sleepData">The sleep data instance</param>
    /// <returns>Weighted sleep score (0-100), null if duration is invalid</returns>
    public static double? CalculateWeightedSleepScore(this SleepData sleepData)
    {
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