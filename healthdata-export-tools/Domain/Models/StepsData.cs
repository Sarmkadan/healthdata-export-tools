// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Domain.Models;

/// <summary>
/// Daily step count and activity tracking data
/// </summary>
public class StepsData : HealthDataRecord
{
    /// <summary>
    /// Total steps recorded for the day
    /// </summary>
    public int TotalSteps { get; set; }

    /// <summary>
    /// Distance traveled in kilometers
    /// </summary>
    public double DistanceKm { get; set; }

    /// <summary>
    /// Calories burned through activity in kcal
    /// </summary>
    public int CaloriesBurned { get; set; }

    /// <summary>
    /// Daily step goal target
    /// </summary>
    public int DailyGoal { get; set; }

    /// <summary>
    /// Percentage of daily goal achieved (0-100+)
    /// </summary>
    public int GoalAchievementPercentage { get; set; }

    /// <summary>
    /// Average cadence (steps per minute) during active periods
    /// </summary>
    public int? AverageCadence { get; set; }

    /// <summary>
    /// Peak step count during an hour
    /// </summary>
    public int? PeakStepsPerHour { get; set; }

    /// <summary>
    /// Total active minutes during the day
    /// </summary>
    public int ActiveMinutes { get; set; }

    /// <summary>
    /// Walking duration in minutes
    /// </summary>
    public int WalkingMinutes { get; set; }

    /// <summary>
    /// Running duration in minutes
    /// </summary>
    public int RunningMinutes { get; set; }

    /// <summary>
    /// Hourly step breakdown for trend analysis
    /// </summary>
    public Dictionary<int, int> HourlySteps { get; set; } = [];

    /// <summary>
    /// Whether the daily goal was achieved
    /// </summary>
    public bool GoalAchieved { get; set; }

    /// <summary>
    /// Validate step data consistency
    /// </summary>
    public override bool IsValid()
    {
        if (TotalSteps < 0) return false;
        if (DistanceKm < 0) return false;
        if (CaloriesBurned < 0) return false;
        if (DailyGoal < 0) return false;
        if (GoalAchievementPercentage < 0) return false;
        if (ActiveMinutes < 0) return false;
        if (AverageCadence.HasValue && (AverageCadence < 0 || AverageCadence > 200)) return false;

        return true;
    }

    /// <summary>
    /// Get summary of daily activity
    /// </summary>
    public override Dictionary<string, object> GetSummary()
    {
        return new()
        {
            { "Date", RecordDate.ToString("yyyy-MM-dd") },
            { "Steps", TotalSteps },
            { "Distance", $"{DistanceKm:F2} km" },
            { "Calories", $"{CaloriesBurned} kcal" },
            { "Goal", $"{GoalAchievementPercentage}%" },
            { "GoalAchieved", GoalAchieved },
            { "ActiveMinutes", ActiveMinutes },
            { "Walking", $"{WalkingMinutes} min" },
            { "Running", $"{RunningMinutes} min" },
            { "PeakHourly", PeakStepsPerHour }
        };
    }

    /// <summary>
    /// Update goal achievement percentage based on total steps
    /// </summary>
    public void UpdateGoalAchievement()
    {
        if (DailyGoal <= 0)
        {
            GoalAchievementPercentage = 0;
            GoalAchieved = false;
            return;
        }

        GoalAchievementPercentage = (int)((TotalSteps / (double)DailyGoal) * 100);
        GoalAchieved = TotalSteps >= DailyGoal;
        Touch();
    }

    /// <summary>
    /// Add hourly step data
    /// </summary>
    public void SetHourlySteps(int hour, int steps)
    {
        if (hour < 0 || hour > 23) throw new ArgumentOutOfRangeException(nameof(hour));
        if (steps < 0) throw new ArgumentOutOfRangeException(nameof(steps));

        HourlySteps[hour] = steps;
        Touch();
    }

    /// <summary>
    /// Calculate average steps per active hour
    /// </summary>
    public double GetAverageStepsPerActiveHour()
    {
        var activeHours = HourlySteps.Count(h => h.Value > 0);
        if (activeHours == 0) return 0;
        return TotalSteps / (double)activeHours;
    }

    /// <summary>
    /// Get the most active hour of the day
    /// </summary>
    public (int Hour, int Steps)? GetMostActiveHour()
    {
        if (HourlySteps.Count == 0) return null;
        var max = HourlySteps.MaxBy(h => h.Value);
        return (max.Key, max.Value);
    }

    /// <summary>
    /// Estimate consumed calories for given step count
    /// </summary>
    public int EstimateCalories(double weightKg = 70)
    {
        // Approximate: 0.05 calories per step (varies by weight)
        var calPerStep = weightKg * 0.0005;
        return (int)(TotalSteps * calPerStep);
    }
}
