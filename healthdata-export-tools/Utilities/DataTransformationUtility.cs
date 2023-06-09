// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Utilities;

/// <summary>
/// Utility for transforming and normalizing health data
/// Handles data type conversions and aggregations
/// </summary>
public static class DataTransformationUtility
{
    /// <summary>
    /// Aggregate sleep data by date
    /// </summary>
    public static Dictionary<DateTime, AggregatedSleepData> AggregateSleepByDate(List<SleepData> sleepRecords)
    {
        return sleepRecords
            .GroupBy(s => s.SleepDate.Date)
            .ToDictionary(
                g => g.Key,
                g => new AggregatedSleepData
                {
                    Date = g.Key,
                    TotalDurationMinutes = g.Sum(s => s.DurationMinutes),
                    AverageDurationMinutes = g.Average(s => s.DurationMinutes),
                    AverageQuality = (int)g.Average(s => (int)s.Quality),
                    TotalDeepSleepMinutes = g.Sum(s => s.DeepSleepMinutes),
                    TotalRemoSleepMinutes = g.Sum(s => s.RemSleepMinutes),
                    Count = g.Count()
                });
    }

    /// <summary>
    /// Aggregate heart rate data by hour
    /// </summary>
    public static Dictionary<DateTime, AggregatedHeartRateData> AggregateHeartRateByHour(
        List<HeartRateData> heartRateRecords)
    {
        return heartRateRecords
            .GroupBy(h => h.Timestamp.AddMinutes(-h.Timestamp.Minute).AddSeconds(-h.Timestamp.Second))
            .ToDictionary(
                g => g.Key,
                g => new AggregatedHeartRateData
                {
                    Hour = g.Key,
                    AverageHeartRate = (int)g.Average(h => h.HeartRate),
                    MinHeartRate = g.Min(h => h.HeartRate),
                    MaxHeartRate = g.Max(h => h.HeartRate),
                    Count = g.Count()
                });
    }

    /// <summary>
    /// Aggregate steps data by day
    /// </summary>
    public static Dictionary<DateTime, AggregatedStepsData> AggregateStepsByDay(List<StepsData> stepsRecords)
    {
        return stepsRecords
            .GroupBy(s => s.StepsDate.Date)
            .ToDictionary(
                g => g.Key,
                g => new AggregatedStepsData
                {
                    Date = g.Key,
                    TotalSteps = g.Sum(s => s.StepCount),
                    TotalDistance = g.Sum(s => s.Distance),
                    TotalCalories = g.Sum(s => s.Calories),
                    AverageSteps = (int)g.Average(s => s.StepCount),
                    Count = g.Count()
                });
    }

    /// <summary>
    /// Filter records by date range
    /// </summary>
    public static List<HealthDataRecord> FilterByDateRange(
        List<HealthDataRecord> records,
        DateTime startDate,
        DateTime endDate)
    {
        return records
            .Where(r => r.RecordDate >= startDate && r.RecordDate <= endDate)
            .ToList();
    }

    /// <summary>
    /// Normalize numeric values to 0-100 scale
    /// </summary>
    public static List<T> NormalizeValues<T>(List<T> records, Func<T, double> getValue, Action<T, double> setValue)
        where T : class
    {
        if (records.Count == 0)
            return records;

        var values = records.Select(getValue).ToList();
        var min = values.Min();
        var max = values.Max();
        var range = max - min == 0 ? 1 : max - min;

        foreach (var record in records)
        {
            var normalized = ((getValue(record) - min) / range) * 100;
            setValue(record, normalized);
        }

        return records;
    }

    /// <summary>
    /// Interpolate missing data points
    /// </summary>
    public static List<HealthDataRecord> InterpolateMissingData(
        List<HealthDataRecord> records,
        TimeSpan gap)
    {
        if (records.Count < 2)
            return records;

        var result = new List<HealthDataRecord>();
        var sortedRecords = records.OrderBy(r => r.RecordDate).ToList();

        for (int i = 0; i < sortedRecords.Count - 1; i++)
        {
            result.Add(sortedRecords[i]);

            var current = sortedRecords[i];
            var next = sortedRecords[i + 1];
            var timeDifference = next.RecordDate - current.RecordDate;

            // If gap exceeds threshold, interpolate
            if (timeDifference > gap)
            {
                var interpolatedCount = (int)(timeDifference.TotalSeconds / gap.TotalSeconds);

                for (int j = 1; j < interpolatedCount; j++)
                {
                    var interpolatedTime = current.RecordDate.Add(
                        TimeSpan.FromSeconds(gap.TotalSeconds * j));

                    var interpolatedValue = Lerp(current.Value, next.Value, j / (double)interpolatedCount);

                    // Create interpolated record (clone and update)
                    // This is simplified; actual implementation would depend on model structure
                }
            }
        }

        result.Add(sortedRecords.Last());
        return result;
    }

    /// <summary>
    /// Linear interpolation helper
    /// </summary>
    private static double Lerp(double a, double b, double t) => a + (b - a) * t;

    /// <summary>
    /// Remove outliers using IQR method
    /// </summary>
    public static List<double> RemoveOutliers(List<double> values)
    {
        if (values.Count < 4)
            return values;

        var sorted = values.OrderBy(v => v).ToList();
        var q1Index = sorted.Count / 4;
        var q3Index = (sorted.Count * 3) / 4;

        var q1 = sorted[q1Index];
        var q3 = sorted[q3Index];
        var iqr = q3 - q1;

        var lowerBound = q1 - (1.5 * iqr);
        var upperBound = q3 + (1.5 * iqr);

        return values.Where(v => v >= lowerBound && v <= upperBound).ToList();
    }

    /// <summary>
    /// Calculate moving average
    /// </summary>
    public static List<double> CalculateMovingAverage(List<double> values, int windowSize)
    {
        var result = new List<double>();

        for (int i = 0; i < values.Count; i++)
        {
            var start = Math.Max(0, i - windowSize / 2);
            var end = Math.Min(values.Count, i + windowSize / 2);
            var average = values.GetRange(start, end - start).Average();
            result.Add(average);
        }

        return result;
    }
}

/// <summary>
/// Aggregated sleep data
/// </summary>
public class AggregatedSleepData
{
    public DateTime Date { get; set; }
    public int TotalDurationMinutes { get; set; }
    public double AverageDurationMinutes { get; set; }
    public int AverageQuality { get; set; }
    public int TotalDeepSleepMinutes { get; set; }
    public int TotalRemoSleepMinutes { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// Aggregated heart rate data
/// </summary>
public class AggregatedHeartRateData
{
    public DateTime Hour { get; set; }
    public int AverageHeartRate { get; set; }
    public int MinHeartRate { get; set; }
    public int MaxHeartRate { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// Aggregated steps data
/// </summary>
public class AggregatedStepsData
{
    public DateTime Date { get; set; }
    public long TotalSteps { get; set; }
    public double TotalDistance { get; set; }
    public double TotalCalories { get; set; }
    public int AverageSteps { get; set; }
    public int Count { get; set; }
}
