#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace HealthDataExportTools.Domain.Models;

/// <summary>
/// Extension methods for ActivityData providing additional functionality
/// </summary>
public static class ActivityDataExtensions
{
    /// <summary>
    /// Gets the activity pace as a TimeSpan for better formatting and comparison
    /// </summary>
    /// <param name="activity">The activity data</param>
    /// <returns>TimeSpan representing the pace per kilometer</returns>
    public static TimeSpan GetPacePerKm(this ActivityData activity)
    {
        if (!activity.AveragePaceMinPerKm.HasValue || activity.AveragePaceMinPerKm.Value <= 0)
        {
            return TimeSpan.Zero;
        }

        var minutes = (int)activity.AveragePaceMinPerKm.Value;
        var seconds = (int)((activity.AveragePaceMinPerKm.Value - minutes) * 60);
        return new TimeSpan(0, 0, minutes, seconds);
    }

    /// <summary>
    /// Calculates the average heart rate zone (1-5) based on maximum heart rate
    /// </summary>
    /// <param name="activity">The activity data</param>
    /// <returns>Heart rate zone from 1 (very light) to 5 (maximum effort)</returns>
    public static int GetHeartRateZone(this ActivityData activity)
    {
        if (!activity.AverageHeartRate.HasValue || !activity.MaximumHeartRate.HasValue)
        {
            return 0;
        }

        var percentage = (double)activity.AverageHeartRate.Value / activity.MaximumHeartRate.Value;

        if (percentage < 0.5) return 1; // Very light
        if (percentage < 0.6) return 2; // Light
        if (percentage < 0.7) return 3; // Moderate
        if (percentage < 0.85) return 4; // Hard
        return 5; // Maximum effort
    }

    /// <summary>
    /// Gets a formatted string representing the activity intensity level
    /// </summary>
    /// <param name="activity">The activity data</param>
    /// <returns>Localized intensity level description</returns>
    public static string GetIntensityDescription(this ActivityData activity)
    {
        var intensity = activity.IntensityLevel ?? activity.CalculateIntensity();

        return intensity switch
        {
            <= 20 => "Very Light",
            <= 40 => "Light",
            <= 60 => "Moderate",
            <= 80 => "Hard",
            _ => "Maximum Effort"
        };
    }

    /// <summary>
    /// Calculates the total elevation gain per kilometer for hilly terrain assessment
    /// </summary>
    /// <param name="activity">The activity data</param>
    /// <returns>Elevation gain in meters per kilometer</returns>
    public static double GetElevationGainPerKm(this ActivityData activity)
    {
        if (!activity.ElevationGainMeters.HasValue || activity.ElevationGainMeters.Value <= 0 || activity.DistanceKm <= 0)
        {
            return 0;
        }

        return activity.ElevationGainMeters.Value / activity.DistanceKm;
    }
}