#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Services;

/// <summary>
/// Service for analyzing health data and computing metrics
/// </summary>
public sealed class AnalyticsService
{
    /// <summary>
    /// Calculate average sleep duration over a period
    /// </summary>
    /// <param name="records">List of sleep data records.</param>
    /// <param name="days">Number of days to analyze.</param>
    /// <returns>Average sleep duration in hours.</returns>
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

        var avgFirst = recent.Take(recent.Count / 2).ToList();
        var avgSecond = recent.Skip(recent.Count / 2).ToList();

        // Hotfix: Handle cases where segments might be empty to prevent InvalidOperationException
        double firstHalfAverage = avgFirst.Any() ? avgFirst.Average() : 0.0;
        double secondHalfAverage = avgSecond.Any() ? avgSecond.Average() : 0.0;

        double percentChange = 0;
        // Hotfix: Prevent division by zero if firstHalfAverage is zero
        if (Math.Abs(firstHalfAverage) > double.Epsilon)
        {
            percentChange = ((secondHalfAverage - firstHalfAverage) / firstHalfAverage) * 100;
        }

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

        double totalDuration = 0, totalDeep = 0, totalRem = 0;
        int excellentNights = 0, count = 0;

        foreach (var r in records)
        {
            if (r.RecordDate < cutoffDate) continue;
            totalDuration += r.DurationMinutes;
            totalDeep     += r.DeepSleepMinutes;
            totalRem      += r.RemSleepMinutes;
            if (r.Quality == Domain.Enums.SleepQuality.Excellent) excellentNights++;
            count++;
        }

        if (count == 0)
            return new SleepQualityReport { Description = "No sleep data available" };

        var report = new SleepQualityReport
        {
            AverageDuration  = totalDuration / count,
            AverageDeepSleep = totalDeep / count,
            AverageRemSleep  = totalRem / count,
            ExcellentNights  = excellentNights,
            TotalNights      = count,
            ExcellenceRate   = (excellentNights / (double)count) * 100
        };

        report.Description = report.ExcellenceRate switch
        {
            > 60 => "Excellent sleep quality",
            > 40 => "Good sleep quality",
            > 20 => "Average sleep quality",
            _    => "Poor sleep quality"
        };

        return report;
    }

    /// <summary>
    /// Identify low SpO2 events and patterns
    /// </summary>
    public SpO2HealthReport AnalyzeSpO2Health(List<SpO2Data> records, int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        long totalSpO2 = 0;
        int minSpO2 = int.MaxValue;
        int totalLowEvents = 0, daysWithLowEvents = 0, count = 0;

        foreach (var r in records)
        {
            if (r.RecordDate < cutoffDate) continue;
            totalSpO2 += r.AveragePercentage;
            if (r.MinimumPercentage < minSpO2) minSpO2 = r.MinimumPercentage;
            totalLowEvents += r.LowSpO2Events;
            if (r.LowSpO2Events > 0) daysWithLowEvents++;
            count++;
        }

        if (count == 0)
            return new SpO2HealthReport { Status = "No data" };

        var status = minSpO2 switch
        {
            < 85 => "Alert - Critical",
            < 90 => "Alert - Concerning",
            < 95 => "Caution - Monitor",
            _    => "Normal"
        };

        return new SpO2HealthReport
        {
            AverageSpO2    = (int)(totalSpO2 / count),
            MinimumSpO2    = minSpO2,
            TotalLowEvents = totalLowEvents,
            DaysWithEvents = daysWithLowEvents,
            Status         = status
        };
    }

    /// <summary>
    /// Calculate activity intensity distribution
    /// </summary>
    public ActivityIntensityDistribution AnalyzeActivityIntensity(List<ActivityData> records, int days = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        int low = 0, medium = 0, high = 0, totalCalories = 0, count = 0;

        foreach (var a in records)
        {
            if (a.RecordDate < cutoffDate) continue;
            if      (a.IntensityLevel <= 33)  low++;
            else if (a.IntensityLevel <= 66)  medium++;
            else                              high++;
            totalCalories += a.CaloriesBurned;
            count++;
        }

        if (count == 0)
            return new ActivityIntensityDistribution();

        return new ActivityIntensityDistribution
        {
            LowIntensity    = low,
            MediumIntensity = medium,
            HighIntensity   = high,
            TotalActivities = count,
            TotalCalories   = totalCalories
        };
    }

    /// <summary>
    /// Get health score (0-100) based on all metrics
    /// </summary>
    public int CalculateHealthScore(HealthDataCollection collection, int days = 7)
    {
        var score = 50;
        var cutoff = DateTime.UtcNow.AddDays(-days);

        if (collection.SleepRecords.Count > 0)
        {
            double total = 0; int n = 0;
            foreach (var r in collection.SleepRecords)
            {
                if (r.RecordDate >= cutoff) { total += r.DurationMinutes; n++; }
            }
            if (n > 0)
            {
                var hours = total / n / 60.0;
                if (hours >= 7 && hours <= 9)       score += 15;
                else if (hours >= 6.5 && hours <= 9.5) score += 10;
            }
        }

        if (collection.HeartRateRecords.Count > 0)
        {
            long total = 0; int n = 0;
            foreach (var r in collection.HeartRateRecords)
            {
                if (r.RecordDate >= cutoff) { total += r.AverageBpm; n++; }
            }
            if (n > 0)
            {
                var avg = (int)(total / n);
                if (avg < 80)       score += 15;
                else if (avg < 100) score += 10;
            }
        }

        if (collection.SpO2Records.Count > 0)
        {
            long total = 0; int n = 0;
            foreach (var r in collection.SpO2Records)
            {
                if (r.RecordDate >= cutoff) { total += r.AveragePercentage; n++; }
            }
            if (n > 0)
            {
                var avg = (int)(total / n);
                if (avg >= 95)      score += 15;
                else if (avg >= 90) score += 10;
            }
        }

        if (collection.StepsRecords.Count > 0)
        {
            long total = 0; int n = 0;
            foreach (var r in collection.StepsRecords)
            {
                if (r.RecordDate >= cutoff) { total += r.TotalSteps; n++; }
            }
            if (n > 0)
            {
                var avg = (int)(total / n);
                if (avg >= 10000)      score += 15;
                else if (avg >= 7000)  score += 10;
            }
        }

        return Math.Min(score, 100);
    }

    /// <summary>
    /// Compute specified percentiles of a numeric data set using linear interpolation.
    /// </summary>
    /// <param name="values">The collection of numeric values.</param>
    /// <param name="percentiles">Percentile values to compute (0‑100 inclusive).</param>
    /// <returns>A read‑only dictionary mapping each requested percentile to its computed value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> or <paramref name="percentiles"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any percentile is outside the 0‑100 range.</exception>
    public IReadOnlyDictionary<double, double> ComputePercentiles(IEnumerable<double> values, params double[] percentiles)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));
        if (percentiles == null) throw new ArgumentNullException(nameof(percentiles));

        var valueList = values.ToList();
        if (!valueList.Any())
            throw new ArgumentException("Values collection cannot be empty.", nameof(values));

        foreach (var p in percentiles)
        {
            if (p < 0.0 || p > 100.0)
                throw new ArgumentOutOfRangeException(nameof(percentiles), "Percentile values must be between 0 and 100 inclusive.");
        }

        var sorted = valueList.OrderBy(v => v).ToArray();
        int n = sorted.Length;
        var result = new Dictionary<double, double>(percentiles.Length);

        foreach (var p in percentiles)
        {
            // Position using the linear interpolation method (R6)
            double rank = p / 100.0 * (n - 1);
            int lower = (int)Math.Floor(rank);
            int upper = (int)Math.Ceiling(rank);

            double interpolated;
            if (lower == upper)
            {
                interpolated = sorted[lower];
            }
            else
            {
                double weight = rank - lower;
                interpolated = sorted[lower] + weight * (sorted[upper] - sorted[lower]);
            }

            result[p] = interpolated;
        }

        return result;
    }
}

/// <summary>Result of trend analysis</summary>
public sealed class TrendAnalysis
{
    public string Status { get; set; } = "Unknown";
    public double PercentChange { get; set; }
    public int DaysAnalyzed { get; set; }
}

/// <summary>Sleep quality analysis report</summary>
public sealed class SleepQualityReport
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
public sealed class SpO2HealthReport
{
    public int AverageSpO2 { get; set; }
    public int MinimumSpO2 { get; set; }
    public int TotalLowEvents { get; set; }
    public int DaysWithEvents { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>Activity intensity distribution</summary>
public sealed class ActivityIntensityDistribution
{
    public int LowIntensity { get; set; }
    public int MediumIntensity { get; set; }
    public int HighIntensity { get; set; }
    public int TotalActivities { get; set; }
    public int TotalCalories { get; set; }
}
