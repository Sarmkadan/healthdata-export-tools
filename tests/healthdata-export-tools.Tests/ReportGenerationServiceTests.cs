#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Logging;
using HealthDataExportTools.Services;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Domain.Enums;

namespace HealthDataExportTools.Tests;

public sealed class ReportGenerationServiceTests
{
    private readonly ILogger<ReportGenerationService> _mockLogger;
    private readonly ReportGenerationService _sut;

    public ReportGenerationServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<ReportGenerationService>>();
        _sut = new ReportGenerationService(_mockLogger);
    }

    [Fact]
    public async Task GenerateSummaryReportAsync_WithValidRecords_ReturnsCorrectReport()
    {
        // Arrange
        var records = new List<HealthDataRecord>
        {
            new SleepData { RecordDate = DateTime.UtcNow.AddDays(-2), DurationMinutes = 480, Quality = SleepQuality.Good, DeviceId = "DeviceA" },
            new HeartRateData { RecordDate = DateTime.UtcNow.AddDays(-1), AverageBpm = 70, DeviceId = "DeviceA" },
            new StepsData { RecordDate = DateTime.UtcNow, TotalSteps = 10000, DeviceId = "DeviceB" }
        };

        // Act
        var report = await _sut.GenerateSummaryReportAsync(records).ConfigureAwait(false);

        // Assert
        report.Should().NotBeNull();
        report.TotalRecords.Should().Be(3);
        report.DeviceDistribution.Should().ContainKey("DeviceA").And.ContainKey("DeviceB");
        report.DeviceDistribution["DeviceA"].Should().Be(2);
        report.DeviceDistribution["DeviceB"].Should().Be(1);
        report.DataTypeStatistics.Should().HaveCount(3);
        report.DataTypeStatistics.Should().Contain(s => s.DataType == nameof(SleepData) && s.RecordCount == 1);
    }

    [Fact]
    public async Task GenerateSummaryReportAsync_WithNoRecords_ReturnsEmptyReport()
    {
        // Arrange
        var records = new List<HealthDataRecord>();

        // Act
        var report = await _sut.GenerateSummaryReportAsync(records).ConfigureAwait(false);

        // Assert
        report.Should().NotBeNull();
        report.TotalRecords.Should().Be(0);
        report.DeviceDistribution.Should().BeEmpty();
        report.DataTypeStatistics.Should().BeEmpty();
        report.DateRange.StartDate.Should().Be(default(DateTime)); // Default DateTime for empty list
        report.DateRange.EndDate.Should().Be(default(DateTime));   // Default DateTime for empty list
    }

    [Fact]
    public async Task GenerateDailyReportAsync_WithValidData_ReturnsCorrectReport()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var sleepData = new List<SleepData>
        {
            new SleepData { RecordDate = today.AddHours(1), DurationMinutes = 400, Quality = SleepQuality.Good, DeepSleepMinutes = 100, RemSleepMinutes = 80 },
            new SleepData { RecordDate = today.AddHours(2), DurationMinutes = 500, Quality = SleepQuality.Excellent, DeepSleepMinutes = 120, RemSleepMinutes = 90 }
        };
        var heartRateData = new List<HeartRateData>
        {
            new HeartRateData { RecordDate = today.AddHours(3), AverageBpm = 65, MinimumBpm = 50, MaximumBpm = 80 },
            new HeartRateData { RecordDate = today.AddHours(4), AverageBpm = 75, MinimumBpm = 60, MaximumBpm = 90 }
        };

        // Act
        var report = await _sut.GenerateDailyReportAsync(sleepData, heartRateData, today).ConfigureAwait(false);

        // Assert
        report.Should().NotBeNull();
        report.Date.Should().Be(today);

        report.SleepMetrics.Should().NotBeNull();
        report.SleepMetrics!.Records.Should().Be(2);
        report.SleepMetrics.TotalDurationMinutes.Should().Be(900);
        report.SleepMetrics.AverageQuality.Should().Be(((int)SleepQuality.Good + (int)SleepQuality.Excellent) / 2);
        report.SleepMetrics.DeepSleepMinutes.Should().Be(220);
        report.SleepMetrics.RemSleepMinutes.Should().Be(170);

        report.HeartRateMetrics.Should().NotBeNull();
        report.HeartRateMetrics!.Records.Should().Be(2);
        report.HeartRateMetrics.AverageHeartRate.Should().Be(70);
        report.HeartRateMetrics.MinHeartRate.Should().Be(50);
        report.HeartRateMetrics.MaxHeartRate.Should().Be(90);
    }

    [Fact]
    public async Task GenerateDailyReportAsync_WithNoSleepData_ReturnsReportWithoutSleepMetrics()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var sleepData = new List<SleepData>();
        var heartRateData = new List<HeartRateData>
        {
            new HeartRateData { RecordDate = today.AddHours(3), AverageBpm = 65, MinimumBpm = 50, MaximumBpm = 80 }
        };

        // Act
        var report = await _sut.GenerateDailyReportAsync(sleepData, heartRateData, today).ConfigureAwait(false);

        // Assert
        report.Should().NotBeNull();
        report.SleepMetrics.Should().BeNull();
        report.HeartRateMetrics.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateDailyReportAsync_WithNoHeartRateData_ReturnsReportWithoutHeartRateMetrics()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var sleepData = new List<SleepData>
        {
            new SleepData { RecordDate = today.AddHours(1), DurationMinutes = 400, Quality = SleepQuality.Good }
        };
        var heartRateData = new List<HeartRateData>();

        // Act
        var report = await _sut.GenerateDailyReportAsync(sleepData, heartRateData, today).ConfigureAwait(false);

        // Assert
        report.Should().NotBeNull();
        report.SleepMetrics.Should().NotBeNull();
        report.HeartRateMetrics.Should().BeNull();
    }

    [Fact]
    public async Task GenerateTrendReportAsync_WithSufficientData_CalculatesTrend()
    {
        // Arrange
        var records = new List<HealthDataRecord>();
        var today = DateTime.UtcNow.Date;
        for (int i = 0; i < 10; i++)
        {
            records.Add(new StepsData { RecordDate = today.AddDays(-i), TotalSteps = 5000 + (i * 100), DeviceId = "DeviceX" });
        }

        // Act
        var report = await _sut.GenerateTrendReportAsync(records, 7).ConfigureAwait(false);

        // Assert
        report.Should().NotBeNull();
        report.WindowDays.Should().Be(7);
        report.MetricTrends.Should().NotBeEmpty();
        report.MetricTrends.Should().ContainSingle(t => t.MetricType == nameof(StepsData));
        // More specific assertions on trend direction/variation would require exposing internal calculation methods or more complex test setup.
        // For now, checking for presence and basic properties is sufficient.
    }

    [Fact]
    public async Task GenerateTrendReportAsync_WithInsufficientData_ReturnsEmptyMetricTrends()
    {
        // Arrange
        var records = new List<HealthDataRecord>
        {
            new StepsData { RecordDate = DateTime.UtcNow.AddDays(-10), TotalSteps = 5000, DeviceId = "DeviceX" }
        };

        // Act
        var report = await _sut.GenerateTrendReportAsync(records, 7).ConfigureAwait(false);

        // Assert
        report.Should().NotBeNull();
        report.MetricTrends.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateWeeklySummaryReportAsync_WithValidData_ReturnsWeeklyReports()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var sleepData = new List<SleepData>
        {
            new SleepData { RecordDate = today, DurationMinutes = 480, Quality = SleepQuality.Good },
            new SleepData { RecordDate = today.AddDays(-1), DurationMinutes = 500, Quality = SleepQuality.Excellent },
            new SleepData { RecordDate = today.AddDays(-7), DurationMinutes = 420, Quality = SleepQuality.Average } // Previous week
        };
        var heartRateData = new List<HeartRateData>
        {
            new HeartRateData { RecordDate = today, AverageBpm = 65, StressLevel = 4 },
            new HeartRateData { RecordDate = today.AddDays(-7), AverageBpm = 70, StressLevel = 5 } // Previous week
        };
        var stepsData = new List<StepsData>
        {
            new StepsData { RecordDate = today, TotalSteps = 10000, DistanceKm = 7.0 },
            new StepsData { RecordDate = today.AddDays(-7), TotalSteps = 8000, DistanceKm = 5.5 } // Previous week
        };

        // Act
        var reports = await _sut.GenerateWeeklySummaryReportAsync(sleepData, heartRateData, stepsData).ConfigureAwait(false);

        // Assert
        reports.Should().NotBeNull();
        reports.Count.Should().BeGreaterThan(0);
        
        var latestWeekReport = reports.Last();
        latestWeekReport.AverageSleepDurationMinutes.Should().Be(490); // (480 + 500) / 2
        latestWeekReport.AverageHeartRate.Should().Be(65);
        latestWeekReport.TotalSteps.Should().Be(10000);
        latestWeekReport.TotalDistanceKm.Should().Be(7.0);
    }

    [Fact]
    public async Task GenerateWeeklySummaryReportAsync_WithEmptyData_ReturnsEmptyList()
    {
        // Act
        var reports = await _sut.GenerateWeeklySummaryReportAsync(new List<SleepData>(), new List<HeartRateData>(), new List<StepsData>()).ConfigureAwait(false);

        // Assert
        reports.Should().NotBeNull();
        reports.Should().BeEmpty();
    }
}
