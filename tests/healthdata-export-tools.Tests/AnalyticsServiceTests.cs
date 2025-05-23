// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Services;
using Xunit;

namespace HealthDataExportTools.Tests;

public class AnalyticsServiceTests
{
    private readonly AnalyticsService _sut = new();

    [Fact]
    public void CalculateAverageSleepDuration_EmptyList_ReturnsZero()
    {
        // Arrange
        var records = new List<SleepData>();

        // Act
        var result = _sut.CalculateAverageSleepDuration(records);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CalculateAverageSleepDuration_RecentRecords_ReturnsCorrectHours()
    {
        // Arrange
        var records = new List<SleepData>
        {
            new() { RecordDate = DateTime.UtcNow.AddDays(-1), DurationMinutes = 480 },
            new() { RecordDate = DateTime.UtcNow.AddDays(-2), DurationMinutes = 420 }
        };

        // Act
        var result = _sut.CalculateAverageSleepDuration(records, 7);

        // Assert
        result.Should().BeApproximately(7.5, 0.01);
    }

    [Fact]
    public void AnalyzeTrend_SingleValue_ReturnsInsufficientData()
    {
        // Arrange
        var values = new List<int> { 75 };

        // Act
        var result = _sut.AnalyzeTrend(values);

        // Assert
        result.Status.Should().Be("Insufficient Data");
    }

    [Fact]
    public void AnalyzeTrend_StronglyIncreasingValues_ReturnsImproving()
    {
        // Arrange
        var values = new List<int> { 50, 55, 60, 65, 70, 80, 90, 100 };

        // Act
        var result = _sut.AnalyzeTrend(values, 7);

        // Assert
        result.Status.Should().Be("Improving");
        result.PercentChange.Should().BeGreaterThan(10);
    }

    [Fact]
    public void AnalyzeTrend_StronglyDecreasingValues_ReturnsDecline()
    {
        // Arrange
        var values = new List<int> { 100, 90, 80, 70, 60, 50, 40, 30 };

        // Act
        var result = _sut.AnalyzeTrend(values, 7);

        // Assert
        result.Status.Should().Be("Declining");
        result.PercentChange.Should().BeLessThan(-10);
    }

    [Fact]
    public void AnalyzeSpO2Health_MinimumBelow85_ReturnsAlertCritical()
    {
        // Arrange
        var records = new List<SpO2Data>
        {
            new()
            {
                RecordDate = DateTime.UtcNow.AddDays(-1),
                AveragePercentage = 93,
                MinimumPercentage = 82,
                MaximumPercentage = 98,
                LowSpO2Events = 3
            }
        };

        // Act
        var report = _sut.AnalyzeSpO2Health(records, 30);

        // Assert
        report.Status.Should().Be("Alert - Critical");
        report.MinimumSpO2.Should().Be(82);
    }

    [Fact]
    public void CalculateTotalSteps_RecordsOutsideWindow_ExcludesOldRecords()
    {
        // Arrange
        var records = new List<StepsData>
        {
            new() { RecordDate = DateTime.UtcNow.AddDays(-1), TotalSteps = 8500 },
            new() { RecordDate = DateTime.UtcNow.AddDays(-3), TotalSteps = 12000 },
            new() { RecordDate = DateTime.UtcNow.AddDays(-30), TotalSteps = 5000 }
        };

        // Act
        var result = _sut.CalculateTotalSteps(records, 7);

        // Assert
        result.Should().Be(20500);
    }

    [Fact]
    public void AnalyzeSleepQuality_NoRecordsInWindow_ReturnsNoDataMessage()
    {
        // Arrange
        var records = new List<SleepData>
        {
            new() { RecordDate = DateTime.UtcNow.AddDays(-60), DurationMinutes = 480 }
        };

        // Act
        var report = _sut.AnalyzeSleepQuality(records, 7);

        // Assert
        report.Description.Should().Be("No sleep data available");
        report.TotalNights.Should().Be(0);
    }
}
