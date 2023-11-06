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
    /// <param name="date">The date of the sleep record</param>
    /// <param name="durationMinutes">Total duration in minutes</param>
    /// <param name="quality">Sleep quality rating</param>
    /// <param name="deepSleepMinutes">Duration of deep sleep in minutes</param>
    /// <param name="remSleepMinutes">Duration of REM sleep in minutes</param>
    /// <returns>A new <see cref="SleepData"/> instance</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any parameter is invalid</exception>
    public static SleepData CreateTestSleepData(this DataTransformationUtilityTests _, DateTime date, int durationMinutes, SleepQuality quality, int deepSleepMinutes, int remSleepMinutes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(date, DateTime.MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(date, DateTime.MaxValue);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(durationMinutes);
        ArgumentOutOfRangeException.ThrowIfNegative(deepSleepMinutes);
        ArgumentOutOfRangeException.ThrowIfNegative(remSleepMinutes);

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
    /// <param name="date">The date of the heart rate record</param>
    /// <param name="averageBpm">Average heart rate in beats per minute</param>
    /// <param name="minimumBpm">Minimum heart rate in beats per minute</param>
    /// <param name="maximumBpm">Maximum heart rate in beats per minute</param>
    /// <returns>A new <see cref="HeartRateData"/> instance</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any parameter is invalid</exception>
    public static HeartRateData CreateTestHeartRateData(this DataTransformationUtilityTests _, DateTime date, int averageBpm, int minimumBpm, int maximumBpm)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(date, DateTime.MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(date, DateTime.MaxValue);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(averageBpm);
        ArgumentOutOfRangeException.ThrowIfNegative(minimumBpm);
        ArgumentOutOfRangeException.ThrowIfNegative(maximumBpm);
        ArgumentOutOfRangeException.ThrowIfLessThan(maximumBpm, minimumBpm);

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
    /// <param name="date">The date of the steps record</param>
    /// <param name="totalSteps">Total number of steps</param>
    /// <param name="distanceKm">Distance walked in kilometers</param>
    /// <param name="caloriesBurned">Calories burned</param>
    /// <returns>A new <see cref="StepsData"/> instance</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any parameter is invalid</exception>
    public static StepsData CreateTestStepsData(this DataTransformationUtilityTests _, DateTime date, int totalSteps, double distanceKm, int caloriesBurned)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(date, DateTime.MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(date, DateTime.MaxValue);
        ArgumentOutOfRangeException.ThrowIfNegative(totalSteps);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(distanceKm);
        ArgumentOutOfRangeException.ThrowIfNegative(caloriesBurned);

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
    /// <param name="date">The date of the health record</param>
    /// <returns>A new <see cref="StepsData"/> instance</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="date"/> is outside valid range</exception>
    public static StepsData CreateTestHealthDataRecord(this DataTransformationUtilityTests _, DateTime date)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(date, DateTime.MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(date, DateTime.MaxValue);

        return new StepsData
        {
            RecordDate = date
        };
    }

    /// <summary>
    /// Asserts that two dictionaries of aggregated sleep data are equal
    /// </summary>
    /// <param name="actual">Actual dictionary to assert</param>
    /// <param name="expected">Expected dictionary values</param>
    /// <exception cref="ArgumentNullException">Thrown when either dictionary is null</exception>
    public static void ShouldBeEquivalentTo(this Dictionary<DateTime, AggregatedSleepData> actual, Dictionary<DateTime, AggregatedSleepData> expected)
    {
        ArgumentNullException.ThrowIfNull(actual);
        ArgumentNullException.ThrowIfNull(expected);

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
    /// <param name="actual">Actual dictionary to assert</param>
    /// <param name="expected">Expected dictionary values</param>
    /// <exception cref="ArgumentNullException">Thrown when either dictionary is null</exception>
    public static void ShouldBeEquivalentTo(this Dictionary<DateTime, AggregatedHeartRateData> actual, Dictionary<DateTime, AggregatedHeartRateData> expected)
    {
        ArgumentNullException.ThrowIfNull(actual);
        ArgumentNullException.ThrowIfNull(expected);

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
    /// <param name="actual">Actual dictionary to assert</param>
    /// <param name="expected">Expected dictionary values</param>
    /// <exception cref="ArgumentNullException">Thrown when either dictionary is null</exception>
    public static void ShouldBeEquivalentTo(this Dictionary<DateTime, AggregatedStepsData> actual, Dictionary<DateTime, AggregatedStepsData> expected)
    {
        ArgumentNullException.ThrowIfNull(actual);
        ArgumentNullException.ThrowIfNull(expected);

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
    /// <param name="count">Number of records to create</param>
    /// <param name="recordFactory">Factory function to create each record</param>
    /// <returns>List of created sleep records</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when count is negative</exception>
    public static List<SleepData> CreateTestSleepDataBatch(this DataTransformationUtilityTests _, int count, Func<int, SleepData> recordFactory)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentNullException.ThrowIfNull(recordFactory);

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
    /// <param name="count">Number of records to create</param>
    /// <param name="recordFactory">Factory function to create each record</param>
    /// <returns>List of created heart rate records</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when count is negative</exception>
    /// <exception cref="ArgumentNullException">Thrown when recordFactory is null</exception>
    public static List<HeartRateData> CreateTestHeartRateDataBatch(this DataTransformationUtilityTests _, int count, Func<int, HeartRateData> recordFactory)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentNullException.ThrowIfNull(recordFactory);

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
    /// <param name="count">Number of records to create</param>
    /// <param name="recordFactory">Factory function to create each record</param>
    /// <returns>List of created steps records</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when count is negative</exception>
    /// <exception cref="ArgumentNullException">Thrown when recordFactory is null</exception>
    public static List<StepsData> CreateTestStepsDataBatch(this DataTransformationUtilityTests _, int count, Func<int, StepsData> recordFactory)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentNullException.ThrowIfNull(recordFactory);

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
    /// <param name="count">Number of records to create</param>
    /// <param name="recordFactory">Factory function to create each record</param>
    /// <returns>List of created health data records</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when count is negative</exception>
    /// <exception cref="ArgumentNullException">Thrown when recordFactory is null</exception>
    public static List<HealthDataRecord> CreateTestHealthDataBatch(this DataTransformationUtilityTests _, int count, Func<int, HealthDataRecord> recordFactory)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentNullException.ThrowIfNull(recordFactory);

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
    /// <param name="records">List of health data records to validate</param>
    /// <param name="startDate">Start date of expected range (inclusive)</param>
    /// <param name="endDate">End date of expected range (inclusive)</param>
    /// <exception cref="ArgumentNullException">Thrown when records is null</exception>
    public static void ShouldContainRecordsInRange(this List<HealthDataRecord> records, DateTime startDate, DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(records);

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
    /// <param name="count">Number of values to create</param>
    /// <param name="valueFactory">Factory function to create each value</param>
    /// <returns>List of created double values</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when count is negative</exception>
    /// <exception cref="ArgumentNullException">Thrown when valueFactory is null</exception>
    public static List<double> CreateTestDoubleValues(this DataTransformationUtilityTests _, int count, Func<int, double> valueFactory)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentNullException.ThrowIfNull(valueFactory);

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
    /// <param name="values">List of values to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when values is null</exception>
    public static void ShouldBeNormalized(this List<double> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        values.Should().NotBeNull();
        values.Should().NotBeEmpty();

        foreach (var value in values)
        {
            value.Should().BeGreaterOrEqualTo(0);
            value.Should().BeLessOrEqualTo(100);
        }
    }
}