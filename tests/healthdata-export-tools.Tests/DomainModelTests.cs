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
using Moq;
using Xunit;

namespace HealthDataExportTools.Tests;

public class DomainModelTests
{
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

    [Fact]
    public void HealthDataParserService_DetectDeviceType_GarminIdentifier_ReturnsGarmin()
    {
        // Arrange
        var parser = new HealthDataParserService();

        // Act
        var deviceType = parser.DetectDeviceType("garmin-fenix-7");

        // Assert
        deviceType.Should().Be(DeviceType.Garmin);
    }

    [Fact]
    public async Task InMemoryCacheProvider_SetAndGet_WithMockedLogger_ReturnsStoredValue()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<InMemoryCacheProvider>>();
        var cache = new InMemoryCacheProvider(mockLogger.Object);
        const string expectedValue = "health-data-payload";

        // Act
        await cache.SetAsync("test-key", expectedValue);
        var result = await cache.GetAsync<string>("test-key");

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public async Task InMemoryCacheProvider_GetNonExistentKey_ReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<InMemoryCacheProvider>>();
        var cache = new InMemoryCacheProvider(mockLogger.Object);

        // Act
        var result = await cache.GetAsync<string>("nonexistent-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task InMemoryCacheProvider_RemoveKey_SubsequentGetReturnsNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<InMemoryCacheProvider>>();
        var cache = new InMemoryCacheProvider(mockLogger.Object);
        await cache.SetAsync("remove-key", "some-value");

        // Act
        await cache.RemoveAsync("remove-key");
        var result = await cache.GetAsync<string>("remove-key");

        // Assert
        result.Should().BeNull();
    }
}
