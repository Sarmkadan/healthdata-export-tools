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

public sealed class DataComparisonServiceTests
{
    private readonly DataComparisonService _sut;

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
}
