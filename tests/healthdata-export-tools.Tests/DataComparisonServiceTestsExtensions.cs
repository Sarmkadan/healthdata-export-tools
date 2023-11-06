#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Services;
using Xunit;

namespace HealthDataExportTools.Tests;

public static class DataComparisonServiceTestsExtensions
{
    /// <summary>
    /// Creates a comparison result with the specified percentage changes for testing purposes.
    /// </summary>
    /// <param name="service">The service instance to extend.</param>
    /// <param name="sleepChange">Sleep duration change percentage.</param>
    /// <param name="heartRateChange">Heart rate change percentage.</param>
    /// <param name="stepsChange">Steps change percentage.</param>
    /// <param name="spO2Change">SpO2 change percentage.</param>
    /// <param name="activityChange">Activity change percentage.</param>
    /// <param name="deepSleepChange">Deep sleep change percentage.</param>
    /// <returns>A comparison result with the specified changes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static async Task<DataComparisonResult> CreateComparisonResultAsync(
        this DataComparisonService service,
        double sleepChange = 0,
        double heartRateChange = 0,
        double stepsChange = 0,
        double spO2Change = 0,
        double activityChange = 0,
        double deepSleepChange = 0)
    {
        ArgumentNullException.ThrowIfNull(service);

        var period1 = new HealthDataCollection();
        var period2 = new HealthDataCollection();

        // Add basic records to both periods
        period1.SleepRecords.Add(new SleepData { RecordDate = DateTime.UtcNow, DurationMinutes = 480 });
        period1.HeartRateRecords.Add(new HeartRateData { RecordDate = DateTime.UtcNow, AverageBpm = 70 });
        period1.StepsRecords.Add(new StepsData { RecordDate = DateTime.UtcNow, TotalSteps = 10000 });
        period1.SpO2Records.Add(new SpO2Data { RecordDate = DateTime.UtcNow, AveragePercentage = 95 });
        period1.ActivityRecords.Add(new ActivityData { RecordDate = DateTime.UtcNow, DurationMinutes = 60, CaloriesBurned = 500 });
        period1.SleepRecords.Add(new SleepData { RecordDate = DateTime.UtcNow, DurationMinutes = 480, DeepSleepMinutes = 96 });

        period2.SleepRecords.Add(new SleepData { RecordDate = DateTime.UtcNow.AddDays(-7), DurationMinutes = 480 });
        period2.HeartRateRecords.Add(new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-7), AverageBpm = 70 });
        period2.StepsRecords.Add(new StepsData { RecordDate = DateTime.UtcNow.AddDays(-7), TotalSteps = 10000 });
        period2.SpO2Records.Add(new SpO2Data { RecordDate = DateTime.UtcNow.AddDays(-7), AveragePercentage = 95 });
        period2.ActivityRecords.Add(new ActivityData { RecordDate = DateTime.UtcNow.AddDays(-7), DurationMinutes = 60, CaloriesBurned = 500 });
        period2.SleepRecords.Add(new SleepData { RecordDate = DateTime.UtcNow.AddDays(-7), DurationMinutes = 480, DeepSleepMinutes = 96 });

        // Act
        var result = await service.ComparePeriodsAsync(period1, period2).ConfigureAwait(false);

        // Modify the result with the specified changes
        result.SleepDurationChangePercentage = sleepChange;
        result.HeartRateChangePercentage = heartRateChange;
        result.StepsChangePercentage = stepsChange;
        result.SpO2ChangePercentage = spO2Change;
        result.ActivityMinutesChangePercentage = activityChange;
        result.DeepSleepChangePercentage = deepSleepChange;

        return result;
    }

    /// <summary>
    /// Asserts that a comparison result has the expected percentage changes within a tolerance.
    /// </summary>
    /// <param name="result">The comparison result to assert.</param>
    /// <param name="expectedSleepChange">Expected sleep duration change percentage.</param>
    /// <param name="expectedHeartRateChange">Expected heart rate change percentage.</param>
    /// <param name="expectedStepsChange">Expected steps change percentage.</param>
    /// <param name="tolerance">Tolerance for floating point comparison.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static void ShouldHavePercentageChanges(
        this DataComparisonResult result,
        double expectedSleepChange,
        double expectedHeartRateChange,
        double expectedStepsChange,
        double tolerance = 0.01)
    {
        ArgumentNullException.ThrowIfNull(result);

        result.Should().NotBeNull();
        result.SleepDurationChangePercentage.Should().BeApproximately(expectedSleepChange, tolerance);
        result.HeartRateChangePercentage.Should().BeApproximately(expectedHeartRateChange, tolerance);
        result.StepsChangePercentage.Should().BeApproximately(expectedStepsChange, tolerance);
    }

    /// <summary>
    /// Asserts that a comparison result has the expected SpO2 changes within a tolerance.
    /// </summary>
    /// <param name="result">The comparison result to assert.</param>
    /// <param name="expectedSpO2Change">Expected SpO2 change percentage.</param>
    /// <param name="tolerance">Tolerance for floating point comparison.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static void ShouldHaveSpO2Change(
        this DataComparisonResult result,
        double expectedSpO2Change,
        double tolerance = 0.01)
    {
        ArgumentNullException.ThrowIfNull(result);

        result.Should().NotBeNull();
        result.SpO2ChangePercentage.Should().BeApproximately(expectedSpO2Change, tolerance);
    }

    /// <summary>
    /// Asserts that a comparison result has the expected activity changes within a tolerance.
    /// </summary>
    /// <param name="result">The comparison result to assert.</param>
    /// <param name="expectedActivityChange">Expected activity change percentage.</param>
    /// <param name="tolerance">Tolerance for floating point comparison.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static void ShouldHaveActivityChange(
        this DataComparisonResult result,
        double expectedActivityChange,
        double tolerance = 0.01)
    {
        ArgumentNullException.ThrowIfNull(result);

        result.Should().NotBeNull();
        result.ActivityMinutesChangePercentage.Should().BeApproximately(expectedActivityChange, tolerance);
    }

    /// <summary>
    /// Asserts that a comparison result has the expected deep sleep changes within a tolerance.
    /// </summary>
    /// <param name="result">The comparison result to assert.</param>
    /// <param name="expectedDeepSleepChange">Expected deep sleep change percentage.</param>
    /// <param name="tolerance">Tolerance for floating point comparison.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static void ShouldHaveDeepSleepChange(
        this DataComparisonResult result,
        double expectedDeepSleepChange,
        double tolerance = 0.01)
    {
        ArgumentNullException.ThrowIfNull(result);

        result.Should().NotBeNull();
        result.DeepSleepChangePercentage.Should().BeApproximately(expectedDeepSleepChange, tolerance);
    }

    /// <summary>
    /// Creates a health data collection with the specified sleep duration for testing.
    /// </summary>
    /// <param name="collection">The collection to extend.</param>
    /// <param name="sleepDurationMinutes">Sleep duration in minutes.</param>
    /// <param name="deepSleepMinutes">Deep sleep duration in minutes.</param>
    /// <returns>A health data collection with the specified sleep data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is null.</exception>
    public static HealthDataCollection WithSleepData(
        this HealthDataCollection collection,
        int sleepDurationMinutes,
        int deepSleepMinutes = 0)
    {
        ArgumentNullException.ThrowIfNull(collection);

        collection.SleepRecords.Add(new SleepData
        {
            RecordDate = DateTime.UtcNow,
            DurationMinutes = sleepDurationMinutes,
            DeepSleepMinutes = deepSleepMinutes
        });
        return collection;
    }

    /// <summary>
    /// Creates a health data collection with the specified heart rate for testing.
    /// </summary>
    /// <param name="collection">The collection to extend.</param>
    /// <param name="averageBpm">Average heart rate in BPM.</param>
    /// <param name="minBpm">Minimum heart rate.</param>
    /// <param name="maxBpm">Maximum heart rate.</param>
    /// <returns>A health data collection with the specified heart rate data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is null.</exception>
    public static HealthDataCollection WithHeartRateData(
        this HealthDataCollection collection,
        int averageBpm,
        int minBpm = 50,
        int maxBpm = 120)
    {
        ArgumentNullException.ThrowIfNull(collection);

        collection.HeartRateRecords.Add(new HeartRateData
        {
            RecordDate = DateTime.UtcNow,
            AverageBpm = averageBpm,
            MinimumBpm = minBpm,
            MaximumBpm = maxBpm
        });
        return collection;
    }

    /// <summary>
    /// Creates a health data collection with the specified steps data for testing.
    /// </summary>
    /// <param name="collection">The collection to extend.</param>
    /// <param name="totalSteps">Total steps count.</param>
    /// <returns>A health data collection with the specified steps data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is null.</exception>
    public static HealthDataCollection WithStepsData(
        this HealthDataCollection collection,
        int totalSteps)
    {
        ArgumentNullException.ThrowIfNull(collection);

        collection.StepsRecords.Add(new StepsData
        {
            RecordDate = DateTime.UtcNow,
            TotalSteps = totalSteps
        });
        return collection;
    }

    /// <summary>
    /// Creates a health data collection with the specified SpO2 data for testing.
    /// </summary>
    /// <param name="collection">The collection to extend.</param>
    /// <param name="averagePercentage">Average SpO2 percentage.</param>
    /// <param name="minPercentage">Minimum SpO2 percentage.</param>
    /// <param name="maxPercentage">Maximum SpO2 percentage.</param>
    /// <returns>A health data collection with the specified SpO2 data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is null.</exception>
    public static HealthDataCollection WithSpO2Data(
        this HealthDataCollection collection,
        int averagePercentage,
        int minPercentage = 90,
        int maxPercentage = 100)
    {
        ArgumentNullException.ThrowIfNull(collection);

        collection.SpO2Records.Add(new SpO2Data
        {
            RecordDate = DateTime.UtcNow,
            AveragePercentage = averagePercentage,
            MinimumPercentage = minPercentage,
            MaximumPercentage = maxPercentage
        });
        return collection;
    }

    /// <summary>
    /// Creates a health data collection with the specified activity data for testing.
    /// </summary>
    /// <param name="collection">The collection to extend.</param>
    /// <param name="durationMinutes">Activity duration in minutes.</param>
    /// <param name="caloriesBurned">Calories burned.</param>
    /// <returns>A health data collection with the specified activity data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is null.</exception>
    public static HealthDataCollection WithActivityData(
        this HealthDataCollection collection,
        int durationMinutes,
        int caloriesBurned)
    {
        ArgumentNullException.ThrowIfNull(collection);

        collection.ActivityRecords.Add(new ActivityData
        {
            RecordDate = DateTime.UtcNow,
            DurationMinutes = durationMinutes,
            CaloriesBurned = caloriesBurned,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMinutes(durationMinutes)
        });
        return collection;
    }
}