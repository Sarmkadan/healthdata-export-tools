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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="activity"/> is null</exception>
    /// <returns>TimeSpan representing the pace per kilometer</returns>
    public static TimeSpan GetPacePerKm(this ActivityData activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="activity"/> is null</exception>
    /// <returns>Heart rate zone from 1 (very light) to 5 (maximum effort)</returns>
    public static int GetHeartRateZone(this ActivityData activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        if (!activity.AverageHeartRate.HasValue || !activity.MaximumHeartRate.HasValue)
        {
            return 0;
        }

        var percentage = (double)activity.AverageHeartRate.Value / activity.MaximumHeartRate.Value;

        return percentage switch
        {
            < 0.5 => 1, // Very light
            < 0.6 => 2, // Light
            < 0.7 => 3, // Moderate
            < 0.85 => 4, // Hard
            _ => 5 // Maximum effort
        };
    }

    /// <summary>
    /// Gets a formatted string representing the activity intensity level
    /// </summary>
    /// <param name="activity">The activity data</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="activity"/> is null</exception>
    /// <returns>Localized intensity level description</returns>
    public static string GetIntensityDescription(this ActivityData activity)
    {
        ArgumentNullException.ThrowIfNull(activity);
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="activity"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="activity"/> has invalid distance</exception>
    /// <returns>Elevation gain in meters per kilometer</returns>
    public static double GetElevationGainPerKm(this ActivityData activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        if (!activity.ElevationGainMeters.HasValue || activity.ElevationGainMeters.Value <= 0 || activity.DistanceKm <= 0)
        {
            return 0;
        }

        return activity.ElevationGainMeters.Value / activity.DistanceKm;
    }
}