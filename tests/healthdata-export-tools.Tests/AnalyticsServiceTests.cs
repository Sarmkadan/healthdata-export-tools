#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Services;
using Xunit;

namespace HealthDataExportTools.Tests;

/// <summary>
/// Contains unit tests for the <see cref="AnalyticsService"/> class.
/// Tests various analytics calculations including sleep duration, heart rate, steps, trends, health scores,
/// and quality analysis for different health metrics.
/// </summary>
public sealed class AnalyticsServiceTests
{
    private readonly AnalyticsService _analyticsService;

    public AnalyticsServiceTests()
    {
        _analyticsService = new AnalyticsService();
    }

    /// <summary>
/// Creates a list of <see cref="SleepData"/> objects for testing purposes.
/// </summary>
/// <param name="count">The number of sleep data records to create.</param>
/// <param name="startDate">Optional start date for the data. If null, uses current date minus count days.</param>
/// <returns>A list of <see cref="SleepData"/> objects with sequential sleep metrics.</returns>
private List<SleepData> CreateSleepData(int count, DateTime? startDate = null)
    {
        var list = new List<SleepData>();
        var start = startDate ?? DateTime.UtcNow.AddDays(-count);
        for (int i = 0; i < count; i++)
        {
            list.Add(new SleepData
            {
                RecordDate = start.AddDays(i),
                DurationMinutes = 360 + (i * 10), // 6 hours + 10 min increment
                DeepSleepMinutes = 60 + (i * 2),
                RemSleepMinutes = 60 + (i * 3),
                Quality = (SleepQuality)(i % 3) + 1 // Unknown, Good, Excellent
            });
        }
        return list;
    }

    /// <summary>
/// Creates a list of <see cref="HeartRateData"/> objects for testing purposes.
/// </summary>
/// <param name="count">The number of heart rate data records to create.</param>
/// <param name="startDate">Optional start date for the data. If null, uses current date minus count days.</param>
/// <returns>A list of <see cref="HeartRateData"/> objects with sequential heart rate metrics.</returns>
private List<HeartRateData> CreateHeartRateData(int count, DateTime? startDate = null)
    {
        var list = new List<HeartRateData>();
        var start = startDate ?? DateTime.UtcNow.AddDays(-count);
        for (int i = 0; i < count; i++)
        {
            list.Add(new HeartRateData
            {
                RecordDate = start.AddDays(i),
                AverageBpm = 60 + (i * 2)
            });
        }
        return list;
    }

    /// <summary>
/// Creates a list of <see cref="SpO2Data"/> objects for testing purposes.
/// </summary>
/// <param name="count">The number of SpO2 data records to create.</param>
/// <param name="startDate">Optional start date for the data. If null, uses current date minus count days.</param>
/// <returns>A list of <see cref="SpO2Data"/> objects with sequential SpO2 metrics.</returns>
private List<SpO2Data> CreateSpO2Data(int count, DateTime? startDate = null)
    {
        var list = new List<SpO2Data>();
        var start = startDate ?? DateTime.UtcNow.AddDays(-count);
        for (int i = 0; i < count; i++)
        {
            list.Add(new SpO2Data
            {
                RecordDate = start.AddDays(i),
                AveragePercentage = 95 + (i % 5), // 95 to 99
                MinimumPercentage = 90 + (i % 5),
                LowSpO2Events = (i % 2 == 0) ? 0 : 1
            });
        }
        return list;
    }

    /// <summary>
/// Creates a list of <see cref="StepsData"/> objects for testing purposes.
/// </summary>
/// <param name="count">The number of steps data records to create.</param>
/// <param name="startDate">Optional start date for the data. If null, uses current date minus count days.</param>
/// <returns>A list of <see cref="StepsData"/> objects with sequential steps metrics.</returns>
private List<StepsData> CreateStepsData(int count, DateTime? startDate = null)
    {
        var list = new List<StepsData>();
        var start = startDate ?? DateTime.UtcNow.AddDays(-count);
        for (int i = 0; i < count; i++)
        {
            list.Add(new StepsData
            {
                RecordDate = start.AddDays(i),
                TotalSteps = 5000 + (i * 1000)
            });
        }
        return list;
    }

    /// <summary>
/// Creates a list of <see cref="ActivityData"/> objects for testing purposes.
/// </summary>
/// <param name="count">The number of activity data records to create.</param>
/// <param name="startDate">Optional start date for the data. If null, uses current date minus count days.</param>
/// <returns>A list of <see cref="ActivityData"/> objects with sequential activity metrics.</returns>
private List<ActivityData> CreateActivityData(int count, DateTime? startDate = null)
    {
        var list = new List<ActivityData>();
        var start = startDate ?? DateTime.UtcNow.AddDays(-count);
        for (int i = 0; i < count; i++)
        {
            list.Add(new ActivityData
            {
                RecordDate = start.AddDays(i),
                IntensityLevel = 30 + (i * 10), // 30, 40, 50, 60, 70, 80...
                CaloriesBurned = 100 + (i * 50)
            });
        }
        return list;
    }


/// <summary>
/// Tests that <see cref="AnalyticsService.CalculateAverageSleepDuration"/> returns 0 when given an empty list of sleep records.
/// </summary>
    [Fact]
    public void CalculateAverageSleepDuration_ShouldReturnZeroForEmptyList()
    {
        // Arrange
        var sleepRecords = new List<SleepData>();

        // Act
        var result = _analyticsService.CalculateAverageSleepDuration(sleepRecords);

        // Assert
        result.Should().Be(0);
    }

/// <summary>
/// Tests that <see cref="AnalyticsService.CalculateAverageSleepDuration"/> calculates the average sleep duration correctly for recent records within the specified time window.
/// </summary>
    [Fact]
    public void CalculateAverageSleepDuration_ShouldCalculateCorrectlyForRecentRecords()
    {
        // Arrange
        var sleepRecords = CreateSleepData(10); // Records for past 10 days
        var expectedAverage = sleepRecords.Skip(3).Average(r => r.DurationMinutes) / 60.0; // Last 7 days

        // Act
        var result = _analyticsService.CalculateAverageSleepDuration(sleepRecords, 7);

        // Assert
        result.Should().BeApproximately(expectedAverage, 0.001);
    }

    [Fact]
/// <summary>
/// Tests that <see cref="AnalyticsService.CalculateAverageHeartRate"/> calculates the average heart rate correctly from heart rate records.
/// </summary>
    public void CalculateAverageHeartRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var hrRecords = CreateHeartRateData(7);
        var expectedAverage = (int)hrRecords.Average(r => r.AverageBpm);

        // Act
        var result = _analyticsService.CalculateAverageHeartRate(hrRecords, 7);

        // Assert
        result.Should().Be(expectedAverage);
    }

    [Fact]
    public void CalculateTotalSteps_ShouldCalculateCorrectlyForRecentRecords()
/// <summary>
/// Tests that <see cref="AnalyticsService.CalculateTotalSteps"/> calculates the total steps correctly for recent records within the specified time window.
/// </summary>
    {
        // Arrange
        var stepsRecords = CreateStepsData(10);
        var expectedTotal = stepsRecords.Skip(3).Sum(r => r.TotalSteps); // Last 7 days

        // Act
        var result = _analyticsService.CalculateTotalSteps(stepsRecords, 7);

        // Assert
        result.Should().Be(expectedTotal);
    }

    [Fact]
    public void AnalyzeTrend_ShouldReturnImprovingForIncreasingValues()
    {
/// <summary>
/// Tests that <see cref="AnalyticsService.AnalyzeTrend"/> returns "Improving" status when values show an increasing trend over the specified period.
/// </summary>
        // Arrange
        var values = new List<int> { 10, 12, 15, 18, 22, 25, 30, 35, 40, 45 };

        // Act
        var result = _analyticsService.AnalyzeTrend(values, 10);

        // Assert
        result.Status.Should().Be("Improving");
        result.PercentChange.Should().BeGreaterThan(10);
    }

    [Fact]
    public void AnalyzeTrend_ShouldReturnDecliningForDecreasingValues()
    {
        // Arrange
/// <summary>
/// Tests that <see cref="AnalyticsService.AnalyzeTrend"/> returns "Declining" status when values show a decreasing trend over the specified period.
/// </summary>
        var values = new List<int> { 45, 40, 35, 30, 25, 22, 18, 15, 12, 10 };

        // Act
        var result = _analyticsService.AnalyzeTrend(values, 10);

        // Assert
        result.Status.Should().Be("Declining");
        result.PercentChange.Should().BeLessThan(-10);
    }

    [Fact]
    public void AnalyzeTrend_ShouldReturnStableForMinorChanges()
    {
        // Arrange
        var values = new List<int> { 20, 21, 20, 22, 19, 21, 20 };
/// <summary>
/// Tests that <see cref="AnalyticsService.AnalyzeTrend"/> returns "Stable" status when values show minimal changes within the specified period.
/// </summary>

        // Act
        var result = _analyticsService.AnalyzeTrend(values, 7);

        // Assert
        result.Status.Should().Be("Stable");
        result.PercentChange.Should().BeInRange(-10, 10);
    }

    [Fact]
    public void CalculateHealthScore_ShouldReturn50ForEmptyCollection()
    {
        // Arrange
        var emptyCollection = new HealthDataCollection();

/// <summary>
/// Tests that <see cref="AnalyticsService.CalculateHealthScore"/> returns 50 as the base score when given an empty health data collection.
/// </summary>
        // Act
        var score = _analyticsService.CalculateHealthScore(emptyCollection);

        // Assert
        score.Should().Be(50);
    }

    [Fact]
    public void CalculateHealthScore_ShouldIncreaseScoreBasedOnGoodData()
    {
        // Arrange
        var collection = new HealthDataCollection
        {
            SleepRecords = CreateSleepData(1, DateTime.UtcNow.AddDays(-1)).ToList(),
            HeartRateRecords = CreateHeartRateData(1, DateTime.UtcNow.AddDays(-1)).ToList(),
/// <summary>
/// Tests that <see cref="AnalyticsService.CalculateHealthScore"/> increases the health score based on good quality data across all health metrics.
/// </summary>
            SpO2Records = CreateSpO2Data(1, DateTime.UtcNow.AddDays(-1)).ToList(),
            StepsRecords = CreateStepsData(1, DateTime.UtcNow.AddDays(-1)).ToList()
        };
        collection.SleepRecords[0].DurationMinutes = 450; // 7.5 hours
        collection.HeartRateRecords[0].AverageBpm = 65;
        collection.SpO2Records[0].AveragePercentage = 98;
        collection.StepsRecords[0].TotalSteps = 10500;

        // Act
        var score = _analyticsService.CalculateHealthScore(collection);

        // Assert
        score.Should().BeGreaterThan(50);
        score.Should().Be(110); // 50 (base) + 10 (sleep) + 15 (hr) + 15 (spo2) + 15 (steps) = 105, capped at 100
    }
/// <summary>
/// Tests that <see cref="AnalyticsService.CalculateAverageDeepSleepPercentage"/> returns 0 when given an empty list of sleep records.
/// </summary>
    
    [Fact]
    public void CalculateAverageDeepSleepPercentage_ShouldReturnZeroForEmptyList()
    {
        // Arrange
        var sleepRecords = new List<SleepData>();

        // Act
        var result = _analyticsService.CalculateAverageDeepSleepPercentage(sleepRecords);

        // Assert
        result.Should().Be(0);
    }
    
    [Fact]
/// <summary>
/// Tests that <see cref="AnalyticsService.CalculateAverageDeepSleepPercentage"/> calculates the average deep sleep percentage correctly from sleep records.
/// </summary>
    public void CalculateAverageDeepSleepPercentage_ShouldCalculateCorrectly()
    {
        // Arrange
        var sleepRecords = new List<SleepData>
        {
            new SleepData { RecordDate = DateTime.UtcNow.AddDays(-1), DurationMinutes = 480, DeepSleepMinutes = 96 }, // 20%
            new SleepData { RecordDate = DateTime.UtcNow.AddDays(-2), DurationMinutes = 420, DeepSleepMinutes = 84 }  // 20%
        };

        // Act
        var result = _analyticsService.CalculateAverageDeepSleepPercentage(sleepRecords, 7);

        // Assert
        result.Should().BeApproximately(20.0, 0.001);
    }
/// <summary>
/// Tests that <see cref="AnalyticsService.CalculateAverageRemPercentage"/> returns 0 when given an empty list of sleep records.
/// </summary>

    [Fact]
    public void CalculateAverageRemPercentage_ShouldReturnZeroForEmptyList()
    {
        // Arrange
        var sleepRecords = new List<SleepData>();

        // Act
        var result = _analyticsService.CalculateAverageRemPercentage(sleepRecords);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
/// <summary>
/// Tests that <see cref="AnalyticsService.CalculateAverageRemPercentage"/> calculates the average REM sleep percentage correctly from sleep records.
/// </summary>
    public void CalculateAverageRemPercentage_ShouldCalculateCorrectly()
    {
        // Arrange
        var sleepRecords = new List<SleepData>
        {
            new SleepData { RecordDate = DateTime.UtcNow.AddDays(-1), DurationMinutes = 480, RemSleepMinutes = 72 }, // 15%
            new SleepData { RecordDate = DateTime.UtcNow.AddDays(-2), DurationMinutes = 420, RemSleepMinutes = 63 }  // 15%
        };

        // Act
        var result = _analyticsService.CalculateAverageRemPercentage(sleepRecords, 7);

        // Assert
        result.Should().BeApproximately(15.0, 0.001);
    }
/// <summary>
/// Tests that <see cref="AnalyticsService.AnalyzeSleepQuality"/> returns "No data available" when given an empty list of sleep records.
/// </summary>
    
    [Fact]
    public void AnalyzeSleepQuality_ShouldReturnNoDataForEmptyList()
    {
        // Arrange
        var sleepRecords = new List<SleepData>();

        // Act
        var report = _analyticsService.AnalyzeSleepQuality(sleepRecords);

        // Assert
        report.Description.Should().Be("No sleep data available");
        report.TotalNights.Should().Be(0);
    }

/// <summary>
/// Tests that <see cref="AnalyticsService.AnalyzeSleepQuality"/> returns a correct sleep quality report with statistics and description based on sleep data.
/// </summary>
    [Fact]
    public void AnalyzeSleepQuality_ShouldReturnCorrectReport()
    {
        // Arrange
        var sleepRecords = new List<SleepData>
        {
            new SleepData { RecordDate = DateTime.UtcNow.AddDays(-1), DurationMinutes = 480, DeepSleepMinutes = 100, RemSleepMinutes = 80, Quality = SleepQuality.Excellent },
            new SleepData { RecordDate = DateTime.UtcNow.AddDays(-2), DurationMinutes = 420, DeepSleepMinutes = 90, RemSleepMinutes = 70, Quality = SleepQuality.Good },
            new SleepData { RecordDate = DateTime.UtcNow.AddDays(-3), DurationMinutes = 500, DeepSleepMinutes = 110, RemSleepMinutes = 90, Quality = SleepQuality.Excellent }
        };

        // Act
        var report = _analyticsService.AnalyzeSleepQuality(sleepRecords, 30);

        // Assert
/// <summary>
/// Tests that <see cref="AnalyticsService.AnalyzeSpO2Health"/> returns "No data" status when given an empty list of SpO2 records.
/// </summary>
        report.TotalNights.Should().Be(3);
        report.ExcellentNights.Should().Be(2);
        report.ExcellenceRate.Should().BeApproximately((2.0 / 3.0) * 100, 0.001);
        report.Description.Should().Be("Excellent sleep quality");
    }

    [Fact]
    public void AnalyzeSpO2Health_ShouldReturnNoDataForEmptyList()
    {
        // Arrange
        var spo2Records = new List<SpO2Data>();

        // Act
        var report = _analyticsService.AnalyzeSpO2Health(spo2Records);

/// <summary>
/// Tests that <see cref="AnalyticsService.AnalyzeSpO2Health"/> returns a correct SpO2 health report with average, minimum values, and alert status based on SpO2 data.
/// </summary>
        // Assert
        report.Status.Should().Be("No data");
        report.TotalLowEvents.Should().Be(0);
    }

    [Fact]
    public void AnalyzeSpO2Health_ShouldReturnCorrectReport()
    {
        // Arrange
        var spo2Records = new List<SpO2Data>
        {
            new SpO2Data { RecordDate = DateTime.UtcNow.AddDays(-1), AveragePercentage = 98, MinimumPercentage = 95, LowSpO2Events = 0 },
            new SpO2Data { RecordDate = DateTime.UtcNow.AddDays(-2), AveragePercentage = 92, MinimumPercentage = 88, LowSpO2Events = 2 },
            new SpO2Data { RecordDate = DateTime.UtcNow.AddDays(-3), AveragePercentage = 96, MinimumPercentage = 93, LowSpO2Events = 1 }
        };
/// <summary>
/// Tests that <see cref="AnalyticsService.AnalyzeActivityIntensity"/> returns empty distribution when given an empty list of activity records.
/// </summary>

        // Act
        var report = _analyticsService.AnalyzeSpO2Health(spo2Records, 30);

        // Assert
        report.AverageSpO2.Should().Be((int)Math.Round((98 + 92 + 96) / 3.0));
        report.MinimumSpO2.Should().Be(88);
        report.TotalLowEvents.Should().Be(3);
        report.DaysWithEvents.Should().Be(2);
        report.Status.Should().Be("Alert - Concerning"); // Based on minimum 88
    }

    [Fact]
    public void AnalyzeActivityIntensity_ShouldReturnEmptyForNoActivities()
    {
/// <summary>
/// Tests that <see cref="AnalyticsService.AnalyzeActivityIntensity"/> categorizes activities correctly by intensity level (Low, Medium, High) and calculates total calories burned.
/// </summary>
        // Arrange
        var activityRecords = new List<ActivityData>();

        // Act
        var distribution = _analyticsService.AnalyzeActivityIntensity(activityRecords);

        // Assert
        distribution.TotalActivities.Should().Be(0);
        distribution.LowIntensity.Should().Be(0);
        distribution.MediumIntensity.Should().Be(0);
        distribution.HighIntensity.Should().Be(0);
    }

    [Fact]
    public void AnalyzeActivityIntensity_ShouldCategorizeActivitiesCorrectly()
    {
        // Arrange
        var activityRecords = new List<ActivityData>
        {
            new ActivityData { RecordDate = DateTime.UtcNow.AddDays(-1), IntensityLevel = 20, CaloriesBurned = 100 }, // Low
            new ActivityData { RecordDate = DateTime.UtcNow.AddDays(-2), IntensityLevel = 50, CaloriesBurned = 200 }, // Medium
            new ActivityData { RecordDate = DateTime.UtcNow.AddDays(-3), IntensityLevel = 80, CaloriesBurned = 300 }, // High
            new ActivityData { RecordDate = DateTime.UtcNow.AddDays(-4), IntensityLevel = 33, CaloriesBurned = 150 }, // Low boundary
            new ActivityData { RecordDate = DateTime.UtcNow.AddDays(-5), IntensityLevel = 66, CaloriesBurned = 250 }  // Medium boundary
        };

        // Act
        var distribution = _analyticsService.AnalyzeActivityIntensity(activityRecords, 7);

        // Assert
        distribution.TotalActivities.Should().Be(5);
        distribution.LowIntensity.Should().Be(2); // 20, 33
        distribution.MediumIntensity.Should().Be(2); // 50, 66
        distribution.HighIntensity.Should().Be(1); // 80
        distribution.TotalCalories.Should().Be(1000); // 100+200+300+150+250
    }
}
