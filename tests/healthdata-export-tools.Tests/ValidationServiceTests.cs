// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Services;
using Xunit;

namespace HealthDataExportTools.Tests;

public class ValidationServiceTests
{
    private readonly ValidationService _sut = new();

    [Fact]
    public void ValidateSleepData_ValidData_PassesWithNoErrors()
    {
        // Arrange
        var sleep = new SleepData
        {
            RecordDate = DateTime.UtcNow.AddDays(-1),
            SleepStart = DateTime.UtcNow.AddDays(-1).AddHours(22),
            SleepEnd = DateTime.UtcNow.AddDays(-1).AddHours(30),
            DurationMinutes = 480,
            DeepSleepMinutes = 90,
            LightSleepMinutes = 240,
            RemSleepMinutes = 100,
            AwakeMinutes = 20
        };

        // Act
        var result = _sut.ValidateSleepData(sleep);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateSleepData_SleepStartAfterEnd_ReturnsError()
    {
        // Arrange
        var baseTime = DateTime.UtcNow.AddDays(-1);
        var sleep = new SleepData
        {
            RecordDate = baseTime,
            SleepStart = baseTime.AddHours(8),
            SleepEnd = baseTime.AddHours(0),
            DurationMinutes = 480,
            DeepSleepMinutes = 90,
            LightSleepMinutes = 240,
            RemSleepMinutes = 80,
            AwakeMinutes = 20
        };

        // Act
        var result = _sut.ValidateSleepData(sleep);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("SleepStart must be before SleepEnd"));
    }

    [Fact]
    public void ValidateSleepData_SleepPhasesTotalExceedsDuration_ReturnsError()
    {
        // Arrange
        var sleep = new SleepData
        {
            RecordDate = DateTime.UtcNow.AddDays(-1),
            SleepStart = DateTime.UtcNow.AddDays(-1).AddHours(22),
            SleepEnd = DateTime.UtcNow.AddDays(-1).AddHours(30),
            DurationMinutes = 200,
            DeepSleepMinutes = 100,
            LightSleepMinutes = 100,
            RemSleepMinutes = 80,
            AwakeMinutes = 20
        };

        // Act
        var result = _sut.ValidateSleepData(sleep);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Sum of sleep phases exceeds total duration"));
    }

    [Fact]
    public void ValidateSleepData_FutureRecordDate_ReturnsError()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(2);
        var sleep = new SleepData
        {
            RecordDate = futureDate,
            SleepStart = futureDate,
            SleepEnd = futureDate.AddHours(8),
            DurationMinutes = 480,
            DeepSleepMinutes = 90,
            LightSleepMinutes = 240,
            RemSleepMinutes = 80,
            AwakeMinutes = 20
        };

        // Act
        var result = _sut.ValidateSleepData(sleep);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("RecordDate cannot be in the future"));
    }

    [Fact]
    public void ValidateHeartRateData_MinimumGreaterThanMaximum_ReturnsErrors()
    {
        // Arrange
        var hr = new HeartRateData
        {
            RecordDate = DateTime.UtcNow.AddDays(-1),
            MinimumBpm = 140,
            MaximumBpm = 80,
            AverageBpm = 100,
            MeasurementCount = 50
        };

        // Act
        var result = _sut.ValidateHeartRateData(hr);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Minimum heart rate cannot be greater than maximum"));
    }

    [Fact]
    public void ValidateSpO2Data_MinimumGreaterThanMaximum_ReturnsError()
    {
        // Arrange
        var spo2 = new SpO2Data
        {
            RecordDate = DateTime.UtcNow.AddDays(-1),
            MinimumPercentage = 98,
            MaximumPercentage = 95,
            AveragePercentage = 96,
            MeasurementCount = 20
        };

        // Act
        var result = _sut.ValidateSpO2Data(spo2);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("MinimumPercentage cannot be greater than MaximumPercentage"));
    }

    [Fact]
    public void ValidateActivityData_EmptyActivityType_ReturnsError()
    {
        // Arrange
        var activity = new ActivityData
        {
            RecordDate = DateTime.UtcNow.AddDays(-1),
            ActivityType = string.Empty,
            StartTime = DateTime.UtcNow.AddDays(-1).AddHours(7),
            EndTime = DateTime.UtcNow.AddDays(-1).AddHours(8),
            DurationMinutes = 60,
            DistanceKm = 10,
            CaloriesBurned = 450
        };

        // Act
        var result = _sut.ValidateActivityData(activity);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("ActivityType cannot be empty"));
    }

    [Fact]
    public void ValidateStepsData_NegativeTotalSteps_ReturnsError()
    {
        // Arrange
        var steps = new StepsData
        {
            RecordDate = DateTime.UtcNow.AddDays(-1),
            TotalSteps = -500,
            DistanceKm = 3.5,
            CaloriesBurned = 200,
            DailyGoal = 10000
        };

        // Act
        var result = _sut.ValidateStepsData(steps);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("TotalSteps must be non-negative"));
    }
}
