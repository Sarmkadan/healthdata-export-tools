using FluentAssertions;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Utilities;
using Xunit;

namespace HealthDataExportTools.Tests;

/// <summary>
/// Extension methods for DataTransformationUtilityTests to provide additional test utilities
/// </summary>
public static class DataTransformationUtilityTestsExtensions
{
    /// <summary>
    /// Creates a test sleep record with specified parameters
    /// </summary>
    public static SleepData CreateTestSleepData(this DataTransformationUtilityTests _, DateTime date, int durationMinutes, SleepQuality quality, int deepSleepMinutes, int remSleepMinutes)
    {
        return new SleepData
        {
            RecordDate = date,
            DurationMinutes = durationMinutes,
            Quality = quality,
            DeepSleepMinutes = deepSleepMinutes,
            RemSleepMinutes = remSleepMinutes
        };
    }

    /// <summary>
    /// Creates a test heart rate record with specified parameters
    /// </summary>
    public static HeartRateData CreateTestHeartRateData(this DataTransformationUtilityTests _, DateTime date, int averageBpm, int minimumBpm, int maximumBpm)
    {
        return new HeartRateData
        {
            RecordDate = date,
            AverageBpm = averageBpm,
            MinimumBpm = minimumBpm,
            MaximumBpm = maximumBpm
        };
    }

    /// <summary>
    /// Creates a test steps record with specified parameters
    /// </summary>
    public static StepsData CreateTestStepsData(this DataTransformationUtilityTests _, DateTime date, int totalSteps, double distanceKm, int caloriesBurned)
    {
        return new StepsData
        {
            RecordDate = date,
            TotalSteps = totalSteps,
            DistanceKm = distanceKm,
            CaloriesBurned = caloriesBurned
        };
    }

    /// <summary>
    /// Creates a test health data record with specified date
    /// </summary>
    public static HealthDataRecord CreateTestHealthDataRecord(this DataTransformationUtilityTests _, DateTime date)
    {
        return new StepsData
        {
            RecordDate = date
        };
    }

    /// <summary>
    /// Asserts that two dictionaries of aggregated sleep data are equal
    /// </summary>
    public static void ShouldBeEquivalentTo(this Dictionary<DateTime, AggregatedSleepData> actual, Dictionary<DateTime, AggregatedSleepData> expected)
    {
        actual.Should().NotBeNull();
        expected.Should().NotBeNull();
        actual.Count.Should().Be(expected.Count);

        foreach (var kvp in expected)
        {
            actual.Should().ContainKey(kvp.Key);
            var actualValue = actual[kvp.Key];

            actualValue.TotalDurationMinutes.Should().Be(kvp.Value.TotalDurationMinutes);
            actualValue.AverageDurationMinutes.Should().BeApproximately(kvp.Value.AverageDurationMinutes, 0.01);
            actualValue.AverageQuality.Should().Be(kvp.Value.AverageQuality);
            actualValue.TotalDeepSleepMinutes.Should().Be(kvp.Value.TotalDeepSleepMinutes);
            actualValue.TotalRemoSleepMinutes.Should().Be(kvp.Value.TotalRemoSleepMinutes);
            actualValue.Count.Should().Be(kvp.Value.Count);
        }
    }

    /// <summary>
    /// Asserts that two dictionaries of aggregated heart rate data are equal
    /// </summary>
    public static void ShouldBeEquivalentTo(this Dictionary<DateTime, AggregatedHeartRateData> actual, Dictionary<DateTime, AggregatedHeartRateData> expected)
    {
        actual.Should().NotBeNull();
        expected.Should().NotBeNull();
        actual.Count.Should().Be(expected.Count);

        foreach (var kvp in expected)
        {
            actual.Should().ContainKey(kvp.Key);
            var actualValue = actual[kvp.Key];

            actualValue.AverageHeartRate.Should().Be(kvp.Value.AverageHeartRate);
            actualValue.MinHeartRate.Should().Be(kvp.Value.MinHeartRate);
            actualValue.MaxHeartRate.Should().Be(kvp.Value.MaxHeartRate);
            actualValue.Count.Should().Be(kvp.Value.Count);
        }
    }

    /// <summary>
    /// Asserts that two dictionaries of aggregated steps data are equal
    /// </summary>
    public static void ShouldBeEquivalentTo(this Dictionary<DateTime, AggregatedStepsData> actual, Dictionary<DateTime, AggregatedStepsData> expected)
    {
        actual.Should().NotBeNull();
        expected.Should().NotBeNull();
        actual.Count.Should().Be(expected.Count);

        foreach (var kvp in expected)
        {
            actual.Should().ContainKey(kvp.Key);
            var actualValue = actual[kvp.Key];

            actualValue.TotalSteps.Should().Be(kvp.Value.TotalSteps);
            actualValue.TotalDistance.Should().BeApproximately(kvp.Value.TotalDistance, 0.01);
            actualValue.TotalCalories.Should().BeApproximately(kvp.Value.TotalCalories, 0.01);
            actualValue.AverageSteps.Should().Be(kvp.Value.AverageSteps);
            actualValue.Count.Should().Be(kvp.Value.Count);
        }
    }

    /// <summary>
    /// Creates a list of test sleep records for batch testing
    /// </summary>
    public static List<SleepData> CreateTestSleepDataBatch(this DataTransformationUtilityTests _, int count, Func<int, SleepData> recordFactory)
    {
        var records = new List<SleepData>();
        for (int i = 0; i < count; i++)
        {
            records.Add(recordFactory(i));
        }
        return records;
    }

    /// <summary>
    /// Creates a list of test heart rate records for batch testing
    /// </summary>
    public static List<HeartRateData> CreateTestHeartRateDataBatch(this DataTransformationUtilityTests _, int count, Func<int, HeartRateData> recordFactory)
    {
        var records = new List<HeartRateData>();
        for (int i = 0; i < count; i++)
        {
            records.Add(recordFactory(i));
        }
        return records;
    }

    /// <summary>
    /// Creates a list of test steps records for batch testing
    /// </summary>
    public static List<StepsData> CreateTestStepsDataBatch(this DataTransformationUtilityTests _, int count, Func<int, StepsData> recordFactory)
    {
        var records = new List<StepsData>();
        for (int i = 0; i < count; i++)
        {
            records.Add(recordFactory(i));
        }
        return records;
    }

    /// <summary>
    /// Creates a list of test health data records for batch testing
    /// </summary>
    public static List<HealthDataRecord> CreateTestHealthDataBatch(this DataTransformationUtilityTests _, int count, Func<int, HealthDataRecord> recordFactory)
    {
        var records = new List<HealthDataRecord>();
        for (int i = 0; i < count; i++)
        {
            records.Add(recordFactory(i));
        }
        return records;
    }

    /// <summary>
    /// Asserts that a list of health data records contains records within a specific date range
    /// </summary>
    public static void ShouldContainRecordsInRange(this List<HealthDataRecord> records, DateTime startDate, DateTime endDate)
    {
        records.Should().NotBeNull();
        records.Should().NotBeEmpty();

        foreach (var record in records)
        {
            record.RecordDate.Should().BeOnOrAfter(startDate);
            record.RecordDate.Should().BeOnOrBefore(endDate);
        }
    }

    /// <summary>
    /// Creates a list of test double values for statistical testing
    /// </summary>
    public static List<double> CreateTestDoubleValues(this DataTransformationUtilityTests _, int count, Func<int, double> valueFactory)
    {
        var values = new List<double>();
        for (int i = 0; i < count; i++)
        {
            values.Add(valueFactory(i));
        }
        return values;
    }

    /// <summary>
    /// Asserts that a list of values has been normalized to 0-100 range
    /// </summary>
    public static void ShouldBeNormalized(this List<double> values)
    {
        values.Should().NotBeNull();
        values.Should().NotBeEmpty();

        foreach (var value in values)
        {
            value.Should().BeGreaterOrEqualTo(0);
            value.Should().BeLessOrEqualTo(100);
        }
    }
}