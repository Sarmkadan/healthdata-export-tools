#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using HealthDataExportTools.Cache;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HealthDataExportTools.Tests;

/// <summary>
/// Contains unit tests for domain models.
/// </summary>
public sealed class DomainModelTests
{
    /// <summary>
    /// Verifies that SleepData calculates quality as Poor when duration is under 6 hours.
    /// </summary>
    [Fact]
    public void SleepData_CalculateQuality_DurationUnder6Hours_ReturnsPoor()
    {
        // Arrange
        var sleep = new SleepData
        {
            DurationMinutes = 300,
            DeepSleepMinutes = 60,
            LightSleepMinutes = 150,
            RemSleepMinutes = 60,
            AwakeMinutes = 30
        };

        // Act
        var quality = sleep.CalculateQuality();

        // Assert
        quality.Should().Be(SleepQuality.Poor);
    }

    /// <summary>
    /// Verifies that SleepData returns correct deep sleep percentage.
    /// </summary>
    [Fact]
    public void SleepData_GetDeepSleepPercentage_ReturnsCorrectRatio()
    {
        // Arrange
        var sleep = new SleepData
        {
            DurationMinutes = 480,
            DeepSleepMinutes = 96
        };

        // Act
        var percentage = sleep.GetDeepSleepPercentage();

        // Assert
        percentage.Should().BeApproximately(20.0, 0.01);
    }

    /// <summary>
    /// Verifies that SleepData returns 0 when duration is 0.
    /// </summary>
    [Fact]
    public void SleepData_GetDeepSleepPercentage_ZeroDuration_ReturnsZero()
    {
        // Arrange
        var sleep = new SleepData { DurationMinutes = 0, DeepSleepMinutes = 0 };

        // Act
        var percentage = sleep.GetDeepSleepPercentage();

        // Assert
        percentage.Should().Be(0);
    }

    /// <summary>
    /// Verifies that StepsData updates goal achievement correctly when steps exceed goal.
    /// </summary>
    [Fact]
    public void StepsData_UpdateGoalAchievement_StepsExceedGoal_SetsGoalAchievedTrueAndCorrectPercentage()
    {
        // Arrange
        var steps = new StepsData { TotalSteps = 12000, DailyGoal = 10000 };

        // Act
        steps.UpdateGoalAchievement();

        // Assert
        steps.GoalAchieved.Should().BeTrue();
        steps.GoalAchievementPercentage.Should().Be(120);
    }

    /// <summary>
    /// Verifies that StepsData sets achievement to 0 when daily goal is 0.
    /// </summary>
    [Fact]
    public void StepsData_UpdateGoalAchievement_ZeroDailyGoal_SetsAchievementToZero()
    {
        // Arrange
        var steps = new StepsData { TotalSteps = 8000, DailyGoal = 0 };

        // Act
        steps.UpdateGoalAchievement();

        // Assert
        steps.GoalAchieved.Should().BeFalse();
        steps.GoalAchievementPercentage.Should().Be(0);
    }

    /// <summary>
    /// Verifies that StepsData throws an exception when setting hourly steps for an invalid hour.
    /// </summary>
    [Fact]
    public void StepsData_SetHourlySteps_HourAbove23_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var steps = new StepsData();

        // Act
        var act = () => steps.SetHourlySteps(25, 500);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that HeartRateData adds a measurement correctly.
    /// </summary>
    [Fact]
    public void HeartRateData_AddMeasurement_UpdatesMeasurementCount()
    {
        // Arrange
        var hr = new HeartRateData();
        var measurement = new HeartRateMeasurement { Timestamp = DateTime.UtcNow, Bpm = 75 };

        // Act
        hr.AddMeasurement(measurement);

        // Assert
        hr.MeasurementCount.Should().Be(1);
        hr.Measurements.Should().ContainSingle();
    }

    /// <summary>
    /// Verifies that HeartRateData calculates heart rate reserve correctly.
    /// </summary>
    [Fact]
    public void HeartRateData_CalculateHeartRateReserve_WithRestingBpm_ReturnsMaxMinusResting()
    {
        // Arrange
        var hr = new HeartRateData { MaximumBpm = 185, RestingBpm = 55 };

        // Act
        var reserve = hr.CalculateHeartRateReserve();

        // Assert
        reserve.Should().Be(130);
    }

    /// <summary>
    /// Verifies that SpO2Data increments low SpO2 events when a reading below 95 is added.
    /// </summary>
    [Fact]
    public void SpO2Data_AddMeasurement_ReadingBelow95_IncrementsLowSpO2Events()
    {
        // Arrange
        var spo2 = new SpO2Data();
        var lowReading = new SpO2Measurement { Timestamp = DateTime.UtcNow, Percentage = 92 };

        // Act
        spo2.AddMeasurement(lowReading);

        // Assert
        spo2.LowSpO2Events.Should().Be(1);
        spo2.MeasurementCount.Should().Be(1);
    }

    /// <summary>
    /// Verifies that SpO2Data returns true when minimum percentage is below 90.
    /// </summary>
    [Fact]
    public void SpO2Data_HasConcerningLevels_MinimumBelow90_ReturnsTrue()
    {
        // Arrange
        var spo2 = new SpO2Data { MinimumPercentage = 88, LowSpO2Events = 2 };

        // Act
        var concerning = spo2.HasConcerningLevels();

        // Assert
        concerning.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HealthDataParserService detects device type correctly.
    /// </summary>
    [Fact]
    public void HealthDataParserService_DetectDeviceType_GarminIdentifier_ReturnsGarmin()
    {
        // Arrange
        var mockValidationService = Substitute.For<IValidationService>();
        var parser = new HealthDataParserService(mockValidationService);

        // Act
        var deviceType = parser.DetectDeviceType("garmin-fenix-7");

        // Assert
        deviceType.Should().Be(DeviceType.Garmin);
    }

    /// <summary>
    /// Verifies that InMemoryCacheProvider sets and gets a value correctly.
    /// </summary>
    [Fact]
    public async Task InMemoryCacheProvider_SetAndGet_WithMockedLogger_ReturnsStoredValue()
    {
        // Arrange
        var mockLogger = Substitute.For<ILogger<InMemoryCacheProvider>>();
        var cache = new InMemoryCacheProvider(mockLogger);
        const string expectedValue = "health-data-payload";

        // Act
        await cache.SetAsync("test-key", expectedValue).ConfigureAwait(false);
        var result = await cache.GetAsync<string>("test-key").ConfigureAwait(false);

        // Assert
        result.Should().Be(expectedValue);
    }

    /// <summary>
    /// Verifies that InMemoryCacheProvider returns null when getting a non-existent key.
    /// </summary>
    [Fact]
    public async Task InMemoryCacheProvider_GetNonExistentKey_ReturnsNull()
    {
        // Arrange
        var mockLogger = Substitute.For<ILogger<InMemoryCacheProvider>>();
        var cache = new InMemoryCacheProvider(mockLogger);

        // Act
        var result = await cache.GetAsync<string>("nonexistent-key").ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that InMemoryCacheProvider removes a key correctly.
    /// </summary>
    [Fact]
    public async Task InMemoryCacheProvider_RemoveKey_SubsequentGetReturnsNull()
    {
        // Arrange
        var mockLogger = Substitute.For<ILogger<InMemoryCacheProvider>>();
        var cache = new InMemoryCacheProvider(mockLogger);
        await cache.SetAsync("remove-key", "some-value").ConfigureAwait(false);

        // Act
        await cache.RemoveAsync("remove-key").ConfigureAwait(false);
        var result = await cache.GetAsync<string>("remove-key").ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }
}
