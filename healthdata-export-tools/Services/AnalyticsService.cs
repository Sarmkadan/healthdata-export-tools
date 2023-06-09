// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Services;

/// <summary>
/// Service for analyzing health data and computing metrics
/// </summary>
public class AnalyticsService
{
    /// <summary>
    /// Calculate average sleep duration over a period
    /// </summary>
    public double CalculateAverageSleepDuration(List<SleepData> records, int days = 7)
    {
        if (records.Count == 0) return 0;

        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var filtered = records.Where(r => r.RecordDate >= cutoffDate).ToList();

        if (filtered.Count == 0) return 0;

        return filtered.Average(r => r.DurationMinutes) / 60.0; // Convert to hours
    }

    /// <summary>
    /// Calculate average heart rate over a period
    /// </summary>
    public int CalculateAverageHeartRate(List<HeartRateData> records, int days = 7)
    {
        if (records.Count == 0) return 0;

        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var filtered = records.Where(r => r.RecordDate >= cutoffDate).ToList();

        if (filtered.Count == 0) return 0;

        return (int)filtered.Average(r => r.AverageBpm);
    }

    /// <summary>
    /// Calculate average SpO2 over a period
    /// </summary>
    public int CalculateAverageSpO2(List<SpO2Data> records, int days = 7)
    {
        if (records.Count == 0) return 0;

        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var filtered = records.Where(r => r.RecordDate >= cutoffDate).ToList();

        if (filtered.Count == 0) return 0;

        return (int)filtered.Average(r => r.AveragePercentage);
    }

    /// <summary>
    /// Calculate total steps over a period
    /// </summary>
    public int CalculateTotalSteps(List<StepsData> records, int days = 7)
    {
        if (records.Count == 0) return 0;

        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return records.Where(r => r.RecordDate >= cutoffDate)
            .Sum(r => r.TotalSteps);
    }

    /// <summary>
    /// Calculate deep sleep percentage average
    /// </summary>
    public double CalculateAverageDeepSleepPercentage(List<SleepData> records, int days = 7)
    {
        if (records.Count == 0) return 0;

        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var filtered = records.Where(r => r.RecordDate >= cutoffDate).ToList();

        if (filtered.Count == 0) return 0;

        return filtered.Average(r => r.GetDeepSleepPercentage());
    }

    /// <summary>
    /// Calculate REM sleep percentage average
    /// </summary>
    public double CalculateAverageRemPercentage(List<SleepData> records, int days = 7)
    {
        if (records.Count == 0) return 0;

        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var filtered = records.Where(r => r.RecordDate >= cutoffDate).ToList();

        if (filtered.Count == 0) return 0;

        return filtered.Average(r => r.GetRemSleepPercentage());
    }

    /// <summary>
    /// Find trends in health data (improving/declining)
    /// </summary>
    public TrendAnalysis AnalyzeTrend(List<int> values, int days = 7)
    {
        if (values.Count < 2) return new TrendAnalysis { Status = "Insufficient Data" };

        var recent = values.TakeLast(days).ToList();
        if (recent.Count < 2) return new TrendAnalysis { Status = "Insufficient Data" };

        var avgFirst = recent.Take(recent.Count / 2).Average();
        var avgSecond = recent.Skip(recent.Count / 2).Average();
        var percentChange = ((avgSecond - avgFirst) / avgFirst) * 100;

        var status = percentChange switch
        {
            > 10 => "Improving",
            < -10 => "Declining",
            _ => "Stable"
        };

        return new TrendAnalysis
        {
            Status = status,
            PercentChange = percentChange,
            DaysAnalyzed = recent.Count
        };
    }

    /// <summary>
    /// Identify sleep quality trends
    /// </summary>
    public SleepQualityReport AnalyzeSleepQuality(List<SleepData> records, int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var filtered = records.Where(r => r.RecordDate >= cutoffDate).ToList();

        if (filtered.Count == 0)
            return new SleepQualityReport { Description = "No sleep data available" };

        var avgDuration = filtered.Average(r => r.DurationMinutes);
        var avgDeep = filtered.Average(r => r.DeepSleepMinutes);
        var avgRem = filtered.Average(r => r.RemSleepMinutes);
        var excellentNights = filtered.Count(r => r.Quality == Domain.Enums.SleepQuality.Excellent);

        var report = new SleepQualityReport
        {
            AverageDuration = avgDuration,
            AverageDeepSleep = avgDeep,
            AverageRemSleep = avgRem,
            ExcellentNights = excellentNights,
            TotalNights = filtered.Count,
            ExcellenceRate = (excellentNights / (double)filtered.Count) * 100
        };

        report.Description = report.ExcellenceRate switch
        {
            > 60 => "Excellent sleep quality",
            > 40 => "Good sleep quality",
            > 20 => "Average sleep quality",
            _ => "Poor sleep quality"
        };

        return report;
    }

    /// <summary>
    /// Identify low SpO2 events and patterns
    /// </summary>
    public SpO2HealthReport AnalyzeSpO2Health(List<SpO2Data> records, int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var filtered = records.Where(r => r.RecordDate >= cutoffDate).ToList();

        if (filtered.Count == 0)
            return new SpO2HealthReport { Status = "No data" };

        var avgSpO2 = filtered.Average(r => r.AveragePercentage);
        var minSpO2 = filtered.Min(r => r.MinimumPercentage);
        var totalLowEvents = filtered.Sum(r => r.LowSpO2Events);
        var daysWithLowEvents = filtered.Count(r => r.LowSpO2Events > 0);

        var status = minSpO2 switch
        {
            < 85 => "Alert - Critical",
            < 90 => "Alert - Concerning",
            < 95 => "Caution - Monitor",
            _ => "Normal"
        };

        return new SpO2HealthReport
        {
            AverageSpO2 = (int)avgSpO2,
            MinimumSpO2 = minSpO2,
            TotalLowEvents = totalLowEvents,
            DaysWithEvents = daysWithLowEvents,
            Status = status
        };
    }

    /// <summary>
    /// Calculate activity intensity distribution
    /// </summary>
    public ActivityIntensityDistribution AnalyzeActivityIntensity(List<ActivityData> records, int days = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var filtered = records.Where(r => r.RecordDate >= cutoffDate).ToList();

        if (filtered.Count == 0)
            return new ActivityIntensityDistribution();

        var distribution = new ActivityIntensityDistribution
        {
            LowIntensity = filtered.Count(a => a.IntensityLevel <= 33),
            MediumIntensity = filtered.Count(a => a.IntensityLevel > 33 && a.IntensityLevel <= 66),
            HighIntensity = filtered.Count(a => a.IntensityLevel > 66),
            TotalActivities = filtered.Count,
            TotalCalories = filtered.Sum(a => a.CaloriesBurned)
        };

        return distribution;
    }

    /// <summary>
    /// Get health score (0-100) based on all metrics
    /// </summary>
    public int CalculateHealthScore(HealthDataCollection collection, int days = 7)
    {
        var score = 50; // Base score

        if (collection.SleepRecords.Any())
        {
            var sleepHours = CalculateAverageSleepDuration(collection.SleepRecords, days);
            if (sleepHours >= 7 && sleepHours <= 9) score += 15;
            else if (sleepHours >= 6.5 && sleepHours <= 9.5) score += 10;
        }

        if (collection.HeartRateRecords.Any())
        {
            var avgHr = CalculateAverageHeartRate(collection.HeartRateRecords, days);
            if (avgHr < 80) score += 15;
            else if (avgHr < 100) score += 10;
        }

        if (collection.SpO2Records.Any())
        {
            var avgSpO2 = CalculateAverageSpO2(collection.SpO2Records, days);
            if (avgSpO2 >= 95) score += 15;
            else if (avgSpO2 >= 90) score += 10;
        }

        if (collection.StepsRecords.Any())
        {
            var totalSteps = CalculateTotalSteps(collection.StepsRecords, days);
            var avgSteps = totalSteps / Math.Max(1, collection.StepsRecords.Count);
            if (avgSteps >= 10000) score += 15;
            else if (avgSteps >= 7000) score += 10;
        }

        return Math.Min(score, 100);
    }
}

/// <summary>Result of trend analysis</summary>
public class TrendAnalysis
{
    public string Status { get; set; } = "Unknown";
    public double PercentChange { get; set; }
    public int DaysAnalyzed { get; set; }
}

/// <summary>Sleep quality analysis report</summary>
public class SleepQualityReport
{
    public double AverageDuration { get; set; }
    public double AverageDeepSleep { get; set; }
    public double AverageRemSleep { get; set; }
    public int ExcellentNights { get; set; }
    public int TotalNights { get; set; }
    public double ExcellenceRate { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>SpO2 health analysis report</summary>
public class SpO2HealthReport
{
    public int AverageSpO2 { get; set; }
    public int MinimumSpO2 { get; set; }
    public int TotalLowEvents { get; set; }
    public int DaysWithEvents { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>Activity intensity distribution</summary>
public class ActivityIntensityDistribution
{
    public int LowIntensity { get; set; }
    public int MediumIntensity { get; set; }
    public int HighIntensity { get; set; }
    public int TotalActivities { get; set; }
    public int TotalCalories { get; set; }
}
