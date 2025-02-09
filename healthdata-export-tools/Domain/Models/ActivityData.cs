// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Domain.Models;

/// <summary>
/// Represents a specific exercise or activity session
/// </summary>
public class ActivityData : HealthDataRecord
{
    /// <summary>
    /// Type of activity (running, cycling, swimming, etc.)
    /// </summary>
    public string ActivityType { get; set; } = string.Empty;

    /// <summary>
    /// Activity start time
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Activity end time
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Duration of the activity in minutes
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Distance covered during activity in kilometers
    /// </summary>
    public double DistanceKm { get; set; }

    /// <summary>
    /// Average pace (minutes per km)
    /// </summary>
    public double? AveragePaceMinPerKm { get; set; }

    /// <summary>
    /// Average speed during activity in km/h
    /// </summary>
    public double? AverageSpeedKmh { get; set; }

    /// <summary>
    /// Maximum speed achieved during activity in km/h
    /// </summary>
    public double? MaximumSpeedKmh { get; set; }

    /// <summary>
    /// Calories burned during the activity
    /// </summary>
    public int CaloriesBurned { get; set; }

    /// <summary>
    /// Average heart rate during the activity (BPM)
    /// </summary>
    public int? AverageHeartRate { get; set; }

    /// <summary>
    /// Maximum heart rate during the activity (BPM)
    /// </summary>
    public int? MaximumHeartRate { get; set; }

    /// <summary>
    /// Elevation gained in meters
    /// </summary>
    public int? ElevationGainMeters { get; set; }

    /// <summary>
    /// Elevation lost in meters
    /// </summary>
    public int? ElevationLossMeters { get; set; }

    /// <summary>
    /// Activity intensity level (0-100)
    /// </summary>
    public int? IntensityLevel { get; set; }

    /// <summary>
    /// User rating of the activity (0-5 stars)
    /// </summary>
    public double? Rating { get; set; }

    /// <summary>
    /// Notes or description of the activity
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// GPS route points if available
    /// </summary>
    public List<GpsPoint> RoutePoints { get; set; } = [];

    /// <summary>
    /// Validate activity data consistency
    /// </summary>
    public override bool IsValid()
    {
        if (StartTime >= EndTime) return false;
        if (DurationMinutes <= 0) return false;
        if (DistanceKm < 0) return false;
        if (CaloriesBurned < 0) return false;
        if (AverageHeartRate.HasValue && (AverageHeartRate < 30 || AverageHeartRate > 220)) return false;
        if (MaximumHeartRate.HasValue && (MaximumHeartRate < 30 || MaximumHeartRate > 220)) return false;
        if (AverageHeartRate.HasValue && MaximumHeartRate.HasValue && AverageHeartRate > MaximumHeartRate) return false;

        return true;
    }

    /// <summary>
    /// Get activity summary
    /// </summary>
    public override Dictionary<string, object> GetSummary()
    {
        return new()
        {
            { "Type", ActivityType },
            { "Date", RecordDate.ToString("yyyy-MM-dd") },
            { "Duration", $"{DurationMinutes} min" },
            { "Distance", $"{DistanceKm:F2} km" },
            { "Pace", AveragePaceMinPerKm.HasValue ? $"{AveragePaceMinPerKm:F2} min/km" : "N/A" },
            { "Speed", AverageSpeedKmh.HasValue ? $"{AverageSpeedKmh:F2} km/h" : "N/A" },
            { "Calories", CaloriesBurned },
            { "AvgHR", AverageHeartRate },
            { "MaxHR", MaximumHeartRate },
            { "Intensity", IntensityLevel }
        };
    }

    /// <summary>
    /// Calculate intensity level based on metrics
    /// </summary>
    public int CalculateIntensity()
    {
        var intensity = 0;

        if (AverageHeartRate.HasValue)
        {
            // Higher HR = higher intensity (baseline ~60 bpm)
            intensity += Math.Min((AverageHeartRate.Value - 60) / 2, 40);
        }

        if (AverageSpeedKmh.HasValue)
        {
            // Higher speed = higher intensity
            intensity += (int)Math.Min(AverageSpeedKmh.Value * 5, 30);
        }

        if (ElevationGainMeters.HasValue && ElevationGainMeters > 0)
        {
            // Elevation increase intensity
            intensity += Math.Min(ElevationGainMeters.Value / 10, 30);
        }

        return Math.Clamp(intensity, 0, 100);
    }

    /// <summary>
    /// Add a GPS route point
    /// </summary>
    public void AddRoutePoint(GpsPoint point)
    {
        RoutePoints.Add(point);
        Touch();
    }

    /// <summary>
    /// Get activity efficiency (calories per km)
    /// </summary>
    public double GetCalorieEfficiency()
    {
        if (DistanceKm <= 0) return 0;
        return CaloriesBurned / DistanceKm;
    }
}

/// <summary>
/// GPS coordinate point for activity routes
/// </summary>
public class GpsPoint
{
    /// <summary>
    /// Latitude coordinate
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude coordinate
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Altitude in meters
    /// </summary>
    public double? Altitude { get; set; }

    /// <summary>
    /// Timestamp of the GPS reading
    /// </summary>
    public DateTime Timestamp { get; set; }
}
