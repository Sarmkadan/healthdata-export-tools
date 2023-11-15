#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Services;
using Xunit;

namespace HealthDataExportTools.Tests;

/// <summary>
/// Provides unit tests for the <see cref="DataComparisonService"/> class.
/// Tests various comparison scenarios between health data periods including sleep, heart rate,
/// steps, SpO2, activity data, and generates comparison reports.
/// </summary>
public sealed class DataComparisonServiceTests
{
	private readonly DataComparisonService _sut;

	/// <summary>
	/// Initializes a new instance of the <see cref="DataComparisonServiceTests"/> class.
	/// </summary>
	public DataComparisonServiceTests()
	{
		_sut = new DataComparisonService();
	}

	[Fact]
	public async Task ComparePeriodsAsync_ShouldCalculatePercentageCorrectly()
	{
		// Arrange
		var period1 = new HealthDataCollection();
		period1.SleepRecords.Add(new SleepData { RecordDate = DateTime.UtcNow, DurationMinutes = 480 }); // New value (8 hours)
		period1.HeartRateRecords.Add(new HeartRateData { RecordDate = DateTime.UtcNow, AverageBpm = 70 });
		period1.StepsRecords.Add(new StepsData { RecordDate = DateTime.UtcNow, TotalSteps = 10000 });

		var period2 = new HealthDataCollection();
		period2.SleepRecords.Add(new SleepData { RecordDate = DateTime.UtcNow.AddDays(-7), DurationMinutes = 400 }); // Old value (6.6 hours)
		period2.HeartRateRecords.Add(new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-7), AverageBpm = 60 });
		period2.StepsRecords.Add(new StepsData { RecordDate = DateTime.UtcNow.AddDays(-7), TotalSteps = 8000 });

		// Act
		var result = await _sut.ComparePeriodsAsync(period1, period2).ConfigureAwait(false);

		// Assert
		result.Should().NotBeNull();
		result.Period1RecordCount.Should().Be(3);
		result.Period2RecordCount.Should().Be(3);

		// (400 - 480) / 480 * 100 = -16.666
		result.SleepDurationChangePercentage.Should().BeApproximately(-16.666, 0.01);

		// (60 - 70) / 70 * 100 = -14.28
		result.HeartRateChangePercentage.Should().BeApproximately(-14.28, 0.01);

		// (8000 - 10000) / 10000 * 100 = -20
		result.StepsChangePercentage.Should().BeApproximately(-20.0, 0.01);
	}

	[Fact]
	public async Task ComparePeriodsAsync_WithEmptyPeriods_ShouldReturnZeroes()
	{
		// Arrange
		var period1 = new HealthDataCollection();
		var period2 = new HealthDataCollection();

		// Act
		var result = await _sut.ComparePeriodsAsync(period1, period2).ConfigureAwait(false);

		// Assert
		result.Should().NotBeNull();
		result.Period1RecordCount.Should().Be(0);
		result.Period2RecordCount.Should().Be(0);
		result.SleepDurationChangePercentage.Should().Be(0);
		result.HeartRateChangePercentage.Should().Be(0);
		result.StepsChangePercentage.Should().Be(0);
	}

	[Fact]
	public async Task ComparePeriodsAsync_WithSpO2Data_ShouldCalculateSpO2Change()
	{
		// Arrange
		var period1 = new HealthDataCollection();
		period1.SpO2Records.Add(new SpO2Data { RecordDate = DateTime.UtcNow, AveragePercentage = 95, MinimumPercentage = 92, MaximumPercentage = 99 });

		var period2 = new HealthDataCollection();
		period2.SpO2Records.Add(new SpO2Data { RecordDate = DateTime.UtcNow.AddDays(-7), AveragePercentage = 97, MinimumPercentage = 94, MaximumPercentage = 100 });

		// Act
		var result = await _sut.ComparePeriodsAsync(period1, period2).ConfigureAwait(false);

		// Assert
		result.Period1AverageSpO2.Should().Be(95);
		result.Period2AverageSpO2.Should().Be(97);
		// (97 - 95) / 95 * 100 = 2.105...
		result.SpO2ChangePercentage.Should().BeApproximately(2.11, 0.01);
	}

	[Fact]
	public async Task ComparePeriodsAsync_WithActivityData_ShouldCalculateActivityChange()
	{
		// Arrange
		var period1 = new HealthDataCollection();
		period1.ActivityRecords.Add(new ActivityData
		{
			RecordDate = DateTime.UtcNow,
			DurationMinutes = 60,
			CaloriesBurned = 500,
			StartTime = DateTime.UtcNow,
			EndTime = DateTime.UtcNow.AddMinutes(60)
		});

		var period2 = new HealthDataCollection();
		period2.ActivityRecords.Add(new ActivityData
		{
			RecordDate = DateTime.UtcNow.AddDays(-7),
			DurationMinutes = 30,
			CaloriesBurned = 250,
			StartTime = DateTime.UtcNow.AddDays(-7),
			EndTime = DateTime.UtcNow.AddDays(-7).AddMinutes(30)
		});

		// Act
		var result = await _sut.ComparePeriodsAsync(period1, period2).ConfigureAwait(false);

		// Assert
		result.Period1TotalActivityMinutes.Should().Be(60);
		result.Period2TotalActivityMinutes.Should().Be(30);
		// (30 - 60) / 60 * 100 = -50
		result.ActivityMinutesChangePercentage.Should().BeApproximately(-50.0, 0.01);
		result.Period1TotalCalories.Should().Be(500);
		result.Period2TotalCalories.Should().Be(250);
	}

	[Fact]
	public async Task ComparePeriodsAsync_ShouldPopulateNarrativeSummary()
	{
		// Arrange
		var period1 = new HealthDataCollection();
		period1.StepsRecords.Add(new StepsData { RecordDate = DateTime.UtcNow, TotalSteps = 10000 });

		var period2 = new HealthDataCollection();
		period2.StepsRecords.Add(new StepsData { RecordDate = DateTime.UtcNow.AddDays(-7), TotalSteps = 8000 });

		// Act
		var result = await _sut.ComparePeriodsAsync(period1, period2).ConfigureAwait(false);

		// Assert
		result.NarrativeSummary.Should().NotBeNullOrEmpty();
		result.NarrativeSummary.Should().Contain("steps");
	}

	[Fact]
	public async Task CompareByDateRangeAsync_ShouldPartitionRecordsCorrectly()
	{
		// Arrange
		var collection = new HealthDataCollection();
		var baseDate = new DateTime(2024, 2, 1);

		// Period 1: 2024-01-01 – 2024-01-07
		collection.StepsRecords.Add(new StepsData { RecordDate = new DateTime(2024, 1, 3), TotalSteps = 8000 });
		collection.StepsRecords.Add(new StepsData { RecordDate = new DateTime(2024, 1, 5), TotalSteps = 9000 });

		// Period 2: 2024-01-15 – 2024-01-21
		collection.StepsRecords.Add(new StepsData { RecordDate = new DateTime(2024, 1, 16), TotalSteps = 11000 });
		collection.StepsRecords.Add(new StepsData { RecordDate = new DateTime(2024, 1, 18), TotalSteps = 12000 });

		// Act
		var result = await _sut.CompareByDateRangeAsync(
			collection,
			new DateTime(2024, 1, 1), new DateTime(2024, 1, 7),
			new DateTime(2024, 1, 15), new DateTime(2024, 1, 21)).ConfigureAwait(false);

		// Assert
		result.Period1RecordCount.Should().Be(2);
		result.Period2RecordCount.Should().Be(2);
		result.Period1AverageSteps.Should().Be(8500); // (8000 + 9000) / 2
		result.Period2AverageSteps.Should().Be(11500); // (11000 + 12000) / 2
		// (11500 - 8500) / 8500 * 100 ≈ 35.29
		result.StepsChangePercentage.Should().BeApproximately(35.29, 0.01);
	}

	[Fact]
	public async Task ExportToJsonAsync_ShouldWriteValidJsonFile()
	{
		// Arrange
		var period1 = new HealthDataCollection();
		period1.SleepRecords.Add(new SleepData { RecordDate = DateTime.UtcNow.AddDays(-1), DurationMinutes = 480 });
		period1.HeartRateRecords.Add(new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-1), AverageBpm = 65, MinimumBpm = 50, MaximumBpm = 120 });

		var period2 = new HealthDataCollection();
		period2.SleepRecords.Add(new SleepData { RecordDate = DateTime.UtcNow.AddDays(-8), DurationMinutes = 420 });
		period2.HeartRateRecords.Add(new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-8), AverageBpm = 70, MinimumBpm = 52, MaximumBpm = 125 });

		var comparisonResult = await _sut.ComparePeriodsAsync(period1, period2).ConfigureAwait(false);
		var tmpFile = Path.Combine(Path.GetTempPath(), $"comparison_{Guid.NewGuid()}.json");

		try
		{
			// Act
			await _sut.ExportToJsonAsync(comparisonResult, tmpFile).ConfigureAwait(false);

			// Assert
			File.Exists(tmpFile).Should().BeTrue();
			var json = await File.ReadAllTextAsync(tmpFile).ConfigureAwait(false);
			json.Should().Contain("NarrativeSummary");
			json.Should().Contain("SleepDurationChangePercentage");
			json.Should().Contain("HeartRateChangePercentage");
		}
		finally
		{
			if (File.Exists(tmpFile)) File.Delete(tmpFile);
		}
	}

	[Fact]
	public async Task ComparePeriodsAsync_WithDeepSleepData_ShouldCalculateDeepSleepChange()
	{
		// Arrange
		var period1 = new HealthDataCollection();
		period1.SleepRecords.Add(new SleepData
		{
			RecordDate = DateTime.UtcNow.AddDays(-1),
			DurationMinutes = 480,
			DeepSleepMinutes = 96
		});

		var period2 = new HealthDataCollection();
		period2.SleepRecords.Add(new SleepData
		{
			RecordDate = DateTime.UtcNow.AddDays(-8),
			DurationMinutes = 450,
			DeepSleepMinutes = 80
		});

		// Act
		var result = await _sut.ComparePeriodsAsync(period1, period2).ConfigureAwait(false);

		// Assert
		result.Period1AverageDeepSleepMinutes.Should().Be(96);
		result.Period2AverageDeepSleepMinutes.Should().Be(80);
		// (80 - 96) / 96 * 100 = -16.667
		result.DeepSleepChangePercentage.Should().BeApproximately(-16.67, 0.01);
	}
}