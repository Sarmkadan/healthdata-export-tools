using FluentAssertions;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Utilities;
using Xunit;

namespace HealthDataExportTools.Tests;

public class DataTransformationUtilityTests
{
    [Fact]
    public void AggregateSleepByDate_ShouldCorrectlyAggregateData()
    {
        // Arrange
        var date = new DateTime(2026, 6, 27);
        var records = new List<SleepData>
        {
            new SleepData { RecordDate = date, DurationMinutes = 400, Quality = SleepQuality.Good, DeepSleepMinutes = 60, RemSleepMinutes = 60 },
            new SleepData { RecordDate = date, DurationMinutes = 200, Quality = SleepQuality.Average, DeepSleepMinutes = 30, RemSleepMinutes = 20 }
        };

        // Act
        var result = DataTransformationUtility.AggregateSleepByDate(records);

        // Assert
        result.Should().ContainKey(date.Date);
        result[date.Date].TotalDurationMinutes.Should().Be(600);
        result[date.Date].AverageDurationMinutes.Should().Be(300);
        result[date.Date].Count.Should().Be(2);
    }

    [Fact]
    public void AggregateHeartRateByHour_ShouldCorrectlyAggregateData()
    {
        // Arrange
        var date = new DateTime(2026, 6, 27, 10, 30, 0);
        var hour = new DateTime(2026, 6, 27, 10, 0, 0);
        var records = new List<HeartRateData>
        {
            new HeartRateData { RecordDate = date, AverageBpm = 80, MinimumBpm = 70, MaximumBpm = 90 },
            new HeartRateData { RecordDate = date.AddMinutes(10), AverageBpm = 100, MinimumBpm = 90, MaximumBpm = 110 }
        };

        // Act
        var result = DataTransformationUtility.AggregateHeartRateByHour(records);

        // Assert
        result.Should().ContainKey(hour);
        result[hour].AverageHeartRate.Should().Be(90);
        result[hour].MinHeartRate.Should().Be(70);
        result[hour].MaxHeartRate.Should().Be(110);
        result[hour].Count.Should().Be(2);
    }

    [Fact]
    public void AggregateStepsByDay_ShouldCorrectlyAggregateData()
    {
        // Arrange
        var date = new DateTime(2026, 6, 27);
        var records = new List<StepsData>
        {
            new StepsData { RecordDate = date, TotalSteps = 5000, DistanceKm = 4.0, CaloriesBurned = 200 },
            new StepsData { RecordDate = date, TotalSteps = 3000, DistanceKm = 2.0, CaloriesBurned = 100 }
        };

        // Act
        var result = DataTransformationUtility.AggregateStepsByDay(records);

        // Assert
        result.Should().ContainKey(date.Date);
        result[date.Date].TotalSteps.Should().Be(8000);
        result[date.Date].TotalDistance.Should().Be(6.0);
        result[date.Date].TotalCalories.Should().Be(300);
        result[date.Date].AverageSteps.Should().Be(4000);
        result[date.Date].Count.Should().Be(2);
    }

    [Fact]
    public void FilterByDateRange_ShouldReturnCorrectSubset()
    {
        // Arrange
        var startDate = new DateTime(2026, 6, 27);
        var endDate = new DateTime(2026, 6, 28);
        var records = new List<HealthDataRecord>
        {
            new StepsData { RecordDate = startDate.AddDays(-1) },
            new StepsData { RecordDate = startDate },
            new StepsData { RecordDate = endDate },
            new StepsData { RecordDate = endDate.AddDays(1) }
        };

        // Act
        var result = DataTransformationUtility.FilterByDateRange(records, startDate, endDate);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.RecordDate.Should().BeOnOrAfter(startDate).And.BeOnOrBefore(endDate));
    }

    [Fact]
    public void RemoveOutliers_ShouldRemoveExtremeValues()
    {
        // Arrange
        var values = new List<double> { 10, 12, 11, 13, 100, 11, 12, 10 }; // 100 is an outlier

        // Act
        var result = DataTransformationUtility.RemoveOutliers(values);

        // Assert
        result.Should().NotContain(100);
        result.Should().HaveCount(7);
    }

    [Fact]
    public void CalculateMovingAverage_ShouldReturnSmoothedValues()
    {
        // Arrange
        var values = new List<double> { 10, 20, 30, 40, 50 };
        var windowSize = 3;

        // Act
        var result = DataTransformationUtility.CalculateMovingAverage(values, windowSize);

        // Assert
        result.Should().HaveCount(5);
        result[0].Should().Be(10); // (10)/1
        result[1].Should().Be(15); // (10+20)/2
        result[2].Should().Be(25); // (20+30)/2
        result[3].Should().Be(35); // (30+40)/2
        result[4].Should().Be(45); // (40+50)/2
    }
}
