#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Services;
using HealthDataExportTools.Utilities; // Needed for ValidationHelper
using NSubstitute;
using Xunit;

namespace HealthDataExportTools.Tests;

/// <summary>
/// Contains unit tests for the <see cref="ValidationService"/> class.
/// Tests various validation scenarios for different health data types including sleep, heart rate, SpO2, steps, activity, and general health metrics.
/// </summary>
public sealed class ValidationServiceTests
{
	private readonly ValidationService _validationService;

	/// <summary>
	/// Initializes a new instance of the <see cref="ValidationServiceTests"/> class.
	/// </summary>
	public ValidationServiceTests()
	{
		_validationService = new ValidationService();
	}

	// --- ValidateSleepData Tests ---

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateSleepData"/> returns a valid result when provided with valid sleep data.
	/// </summary>
	public void ValidateSleepData_ShouldReturnValidResultForValidData()
	{
		// Arrange
		var sleepData = new SleepData
		{
			RecordDate = DateTime.UtcNow.Date,
			SleepStart = DateTime.UtcNow.AddHours(-8),
			SleepEnd = DateTime.UtcNow,
			DurationMinutes = 480,
			DeepSleepMinutes = 90,
			LightSleepMinutes = 270,
			RemSleepMinutes = 60,
			AwakeMinutes = 60,
			AverageHeartRate = 65,
			Score = 80,
			Quality = SleepQuality.Good
		};

		// Act
		var result = _validationService.ValidateSleepData(sleepData);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateSleepData"/> returns an invalid result when SleepStart is after SleepEnd.
	/// </summary>
	public void ValidateSleepData_ShouldReturnInvalidResultWhenSleepStartIsAfterSleepEnd()
	{
		// Arrange
		var sleepData = new SleepData
		{
			RecordDate = DateTime.UtcNow.Date,
			SleepStart = DateTime.UtcNow,
			SleepEnd = DateTime.UtcNow.AddHours(-8),
			DurationMinutes = 480,
			DeepSleepMinutes = 90,
			LightSleepMinutes = 270,
			RemSleepMinutes = 60,
			AwakeMinutes = 60
		};

		// Act
		var result = _validationService.ValidateSleepData(sleepData);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain("SleepStart must be before SleepEnd");
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateSleepData"/> returns an invalid result when DurationMinutes is zero or negative.
	/// </summary>
	public void ValidateSleepData_ShouldReturnInvalidResultWhenDurationMinutesIsZeroOrNegative()
	{
		// Arrange
		var sleepData = new SleepData
		{
			RecordDate = DateTime.UtcNow.Date,
			SleepStart = DateTime.UtcNow.AddHours(-8),
			SleepEnd = DateTime.UtcNow,
			DurationMinutes = 0,
			DeepSleepMinutes = 0,
			LightSleepMinutes = 0,
			RemSleepMinutes = 0,
			AwakeMinutes = 0
		};

		// Act
		var result = _validationService.ValidateSleepData(sleepData);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain("DurationMinutes must be positive");
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateSleepData"/> returns an invalid result when RecordDate is in the future.
	/// </summary>
	public void ValidateSleepData_ShouldReturnInvalidResultWhenRecordDateIsInFuture()
	{
		// Arrange
		var sleepData = new SleepData
		{
			RecordDate = DateTime.UtcNow.AddDays(1),
			SleepStart = DateTime.UtcNow.AddHours(-8),
			SleepEnd = DateTime.UtcNow,
			DurationMinutes = 480,
			DeepSleepMinutes = 90,
			LightSleepMinutes = 270,
			RemSleepMinutes = 60,
			AwakeMinutes = 60
		};

		// Act
		var result = _validationService.ValidateSleepData(sleepData);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain("RecordDate cannot be in the future");
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateSleepData"/> returns an invalid result when sleep phase durations are negative.
	/// </summary>
	public void ValidateSleepData_ShouldReturnInvalidResultWhenSleepPhaseDurationsAreNegative()
	{
		// Arrange
		var sleepData = new SleepData
		{
			RecordDate = DateTime.UtcNow.Date,
			SleepStart = DateTime.UtcNow.AddHours(-8),
			SleepEnd = DateTime.UtcNow,
			DurationMinutes = 480,
			DeepSleepMinutes = -10,
			LightSleepMinutes = 270,
			RemSleepMinutes = 60,
			AwakeMinutes = 60
		};

		// Act
		var result = _validationService.ValidateSleepData(sleepData);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain("Sleep phase durations must be non-negative");
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateSleepData"/> returns an invalid result when the sum of sleep phases exceeds the total duration.
	/// </summary>
	public void ValidateSleepData_ShouldReturnInvalidResultWhenSumOfSleepPhasesExceedsTotalDuration()
	{
		// Arrange
		var sleepData = new SleepData
		{
			RecordDate = DateTime.UtcNow.Date,
			SleepStart = DateTime.UtcNow.AddHours(-8),
			SleepEnd = DateTime.UtcNow,
			DurationMinutes = 400, // Sum of phases below is 480
			DeepSleepMinutes = 90,
			LightSleepMinutes = 270,
			RemSleepMinutes = 60,
			AwakeMinutes = 60
		};

		// Act
		var result = _validationService.ValidateSleepData(sleepData);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain("Sum of sleep phases exceeds total duration");
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateSleepData"/> returns an invalid result for invalid average heart rate values.
	/// </summary>
	public void ValidateSleepData_ShouldReturnInvalidResultForInvalidAverageHeartRate()
	{
		// Arrange
		var sleepData = new SleepData
		{
			RecordDate = DateTime.UtcNow.Date,
			SleepStart = DateTime.UtcNow.AddHours(-8),
			SleepEnd = DateTime.UtcNow,
			DurationMinutes = 480,
			DeepSleepMinutes = 90,
			LightSleepMinutes = 270,
			RemSleepMinutes = 60,
			AwakeMinutes = 60,
			AverageHeartRate = 30 // Too low
		};

		// Act
		var result = _validationService.ValidateSleepData(sleepData);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain("AverageHeartRate is out of valid range");
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateSleepData"/> returns an invalid result for invalid score values.
	/// </summary>
	public void ValidateSleepData_ShouldReturnInvalidResultForInvalidScore()
	{
		// Arrange
		var sleepData = new SleepData
		{
			RecordDate = DateTime.UtcNow.Date,
			SleepStart = DateTime.UtcNow.AddHours(-8),
			SleepEnd = DateTime.UtcNow,
			DurationMinutes = 480,
			DeepSleepMinutes = 90,
			LightSleepMinutes = 270,
			RemSleepMinutes = 60,
			AwakeMinutes = 60,
			Score = 150 // Too high
		};

		// Act
		var result = _validationService.ValidateSleepData(sleepData);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain("Score must be between 0 and 100");
	}

	// --- ValidateHeartRateData Tests ---

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateHeartRateData"/> returns a valid result when provided with valid heart rate data.
	/// </summary>
	public void ValidateHeartRateData_ShouldReturnValidResultForValidData()
	{
		// Arrange
		var hrData = new HeartRateData
		{
			RecordDate = DateTime.UtcNow.Date,
			MinimumBpm = 50,
			MaximumBpm = 120,
			AverageBpm = 70,
			RestingBpm = 60,
			MeasurementCount = 100
		};

		// Act
		var result = _validationService.ValidateHeartRateData(hrData);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateHeartRateData"/> returns an invalid result when RecordDate is in the future.
	/// </summary>
	public void ValidateHeartRateData_ShouldReturnInvalidResultWhenRecordDateIsInFuture()
	{
		// Arrange
		var hrData = new HeartRateData
		{
			RecordDate = DateTime.UtcNow.AddDays(1),
			MinimumBpm = 50,
			MaximumBpm = 120,
			AverageBpm = 70
		};

		// Act
		var result = _validationService.ValidateHeartRateData(hrData);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain("RecordDate cannot be in the future");
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateHeartRateData"/> returns an invalid result for invalid BPM ranges.
	/// </summary>
	public void ValidateHeartRateData_ShouldReturnInvalidResultForInvalidBpmRanges()
	{
		// Arrange
		var hrData = new HeartRateData
		{
			RecordDate = DateTime.UtcNow.Date,
			MinimumBpm = 150,
			MaximumBpm = 100,
			AverageBpm = 120 // Min > Max, Avg out of range
		};

		// Act
		var result = _validationService.ValidateHeartRateData(hrData);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain("MinimumBpm cannot be greater than MaximumBpm");
		result.Errors.Should().Contain("AverageBpm must be between MinimumBpm and MaximumBpm");
	}

	// --- ValidateSpO2Data Tests ---

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateSpO2Data"/> returns a valid result when provided with valid SpO2 data.
	/// </summary>
	public void ValidateSpO2Data_ShouldReturnValidResultForValidData()
	{
		// Arrange
		var spo2Data = new SpO2Data
		{
			RecordDate = DateTime.UtcNow.Date,
			MinimumPercentage = 95,
			MaximumPercentage = 99,
			AveragePercentage = 97,
			RestingPercentage = 98,
			MeasurementCount = 50
		};

		// Act
		var result = _validationService.ValidateSpO2Data(spo2Data);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateSpO2Data"/> returns an invalid result when MinimumPercentage is greater than MaximumPercentage.
	/// </summary>
	public void ValidateSpO2Data_ShouldReturnInvalidResultWhenMinimumPercentageIsGreaterThanMaximum()
	{
		// Arrange
		var spo2Data = new SpO2Data
		{
			RecordDate = DateTime.UtcNow.Date,
			MinimumPercentage = 98,
			MaximumPercentage = 95,
			AveragePercentage = 97
		};

		// Act
		var result = _validationService.ValidateSpO2Data(spo2Data);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain("MinimumPercentage cannot be greater than MaximumPercentage");
	}

	// --- ValidateStepsData Tests ---

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateStepsData"/> returns a valid result when provided with valid steps data.
	/// </summary>
	public void ValidateStepsData_ShouldReturnValidResultForValidData()
	{
		// Arrange
		var stepsData = new StepsData
		{
			RecordDate = DateTime.UtcNow.Date,
			TotalSteps = 10000,
			DistanceKm = 7.5,
			CaloriesBurned = 500,
			DailyGoal = 10000,
			GoalAchievementPercentage = 100,
			ActiveMinutes = 120
		};

		// Act
		var result = _validationService.ValidateStepsData(stepsData);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateStepsData"/> returns an invalid result when TotalSteps is negative.
	/// </summary>
	public void ValidateStepsData_ShouldReturnInvalidResultWhenTotalStepsIsNegative()
	{
		// Arrange
		var stepsData = new StepsData
		{
			RecordDate = DateTime.UtcNow.Date,
			TotalSteps = -100
		};

		// Act
		var result = _validationService.ValidateStepsData(stepsData);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain("TotalSteps must be non-negative");
	}

	// --- ValidateActivityData Tests ---

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateActivityData"/> returns a valid result when provided with valid activity data.
	/// </summary>
	public void ValidateActivityData_ShouldReturnValidResultForValidData()
	{
		// Arrange
		var activityData = new ActivityData
		{
			ActivityType = "Running",
			RecordDate = DateTime.UtcNow.Date,
			StartTime = DateTime.UtcNow.AddHours(-1),
			EndTime = DateTime.UtcNow,
			DurationMinutes = 60,
			DistanceKm = 10,
			CaloriesBurned = 600,
			AverageHeartRate = 140,
			MaximumHeartRate = 170
		};

		// Act
		var result = _validationService.ValidateActivityData(activityData);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateActivityData"/> returns an invalid result when ActivityType is empty.
	/// </summary>
	public void ValidateActivityData_ShouldReturnInvalidResultWhenActivityTypeIsEmpty()
	{
		// Arrange
		var activityData = new ActivityData
		{
			ActivityType = "",
			RecordDate = DateTime.UtcNow.Date,
			StartTime = DateTime.UtcNow.AddHours(-1),
			EndTime = DateTime.UtcNow,
			DurationMinutes = 60
		};

		// Act
		var result = _validationService.ValidateActivityData(activityData);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain("ActivityType cannot be empty");
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateActivityData"/> returns an invalid result when StartTime is after EndTime.
	/// </summary>
	public void ValidateActivityData_ShouldReturnInvalidResultWhenStartTimeIsAfterEndTime()
	{
		// Arrange
		var activityData = new ActivityData
		{
			ActivityType = "Running",
			RecordDate = DateTime.UtcNow.Date,
			StartTime = DateTime.UtcNow,
			EndTime = DateTime.UtcNow.AddHours(-1),
			DurationMinutes = 60
		};

		// Act
		var result = _validationService.ValidateActivityData(activityData);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain("StartTime must be before EndTime");
	}

	// --- ValidateHealthMetric Tests ---

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateHealthMetric"/> returns a valid result when provided with valid health metric data.
	/// </summary>
	public void ValidateHealthMetric_ShouldReturnValidResultForValidData()
	{
		// Arrange
		var healthMetric = new HealthMetric
		{
			MetricName = "Weight",
			RecordDate = DateTime.UtcNow.Date,
			Value = 75.5,
			Unit = "kg",
			NormalRangeLow = 60,
			NormalRangeHigh = 80
		};

		// Act
		var result = _validationService.ValidateHealthMetric(healthMetric);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateHealthMetric"/> returns an invalid result when MetricName is empty.
	/// </summary>
	public void ValidateHealthMetric_ShouldReturnInvalidResultWhenMetricNameIsEmpty()
	{
		// Arrange
		var healthMetric = new HealthMetric
		{
			MetricName = "",
			RecordDate = DateTime.UtcNow.Date,
			Value = 75.5,
			Unit = "kg"
		};

		// Act
		var result = _validationService.ValidateHealthMetric(healthMetric);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain("MetricName cannot be empty");
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateHealthMetric"/> returns an invalid result when Value is negative.
	/// </summary>
	public void ValidateHealthMetric_ShouldReturnInvalidResultWhenValueIsNegative()
	{
		// Arrange
		var healthMetric = new HealthMetric
		{
			MetricName = "Weight",
			RecordDate = DateTime.UtcNow.Date,
			Value = -10,
			Unit = "kg"
		};

		// Act
		var result = _validationService.ValidateHealthMetric(healthMetric);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain("Value must be non-negative");
	}

	[Fact]
	/// <summary>
	/// Tests that <see cref="ValidationService.ValidateHealthMetric"/> returns an invalid result when NormalRangeLow is greater than NormalRangeHigh.
	/// </summary>
	public void ValidateHealthMetric_ShouldReturnInvalidResultWhenNormalRangeLowIsGreaterThanNormalRangeHigh()
	{
		// Arrange
		var healthMetric = new HealthMetric
		{
			MetricName = "Weight",
			RecordDate = DateTime.UtcNow.Date,
			Value = 75.5,
			Unit = "kg",
			NormalRangeLow = 80,
			NormalRangeHigh = 60
		};

		// Act
		var result = _validationService.ValidateHealthMetric(healthMetric);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain("NormalRangeLow cannot be greater than NormalRangeHigh");
	}
}