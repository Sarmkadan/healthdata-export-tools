// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.DTOs;
using HealthDataExportTools.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HealthDataExportTools.Tests;

public class TrendAnomalyDetectionServiceTests
{
    private readonly TrendAnomalyDetectionService _sut =
        new(NullLogger<TrendAnomalyDetectionService>.Instance);

    // -------------------------------------------------------------------------
    // ComputeTrendAndAnomalies — trend direction
    // -------------------------------------------------------------------------

    [Fact]
    public void ComputeTrendAndAnomalies_FewerThanMinimumPoints_ReturnsInsufficientData()
    {
        // Arrange
        var points = BuildPoints(75, 80, 78); // 3 points < MinimumSampleCount (5)

        // Act
        var result = _sut.ComputeTrendAndAnomalies("HeartRate", points, 30, 2.0);

        // Assert
        result.TrendStatus.Should().Be("Insufficient Data");
        result.Anomalies.Should().BeEmpty();
    }

    [Fact]
    public void ComputeTrendAndAnomalies_EmptyPoints_ReturnsInsufficientData()
    {
        // Arrange
        var points = Array.Empty<(DateTime, double)>();

        // Act
        var result = _sut.ComputeTrendAndAnomalies("SpO2", points, 30, 2.0);

        // Assert
        result.TrendStatus.Should().Be("Insufficient Data");
        result.SampleCount.Should().Be(0);
    }

    [Fact]
    public void ComputeTrendAndAnomalies_StronglyIncreasingValues_ReturnsImproving()
    {
        // Arrange — second half substantially higher than first half
        var points = BuildPoints(50, 52, 54, 56, 70, 80, 90, 100);

        // Act
        var result = _sut.ComputeTrendAndAnomalies("Steps", points, 30, 2.0);

        // Assert
        result.TrendStatus.Should().Be("Improving");
        result.PercentChange.Should().BeGreaterThan(10);
    }

    [Fact]
    public void ComputeTrendAndAnomalies_StronglyDecreasingValues_ReturnsDecling()
    {
        // Arrange — second half substantially lower than first half
        var points = BuildPoints(100, 95, 90, 85, 60, 50, 40, 30);

        // Act
        var result = _sut.ComputeTrendAndAnomalies("HeartRate", points, 30, 2.0);

        // Assert
        result.TrendStatus.Should().Be("Declining");
        result.PercentChange.Should().BeLessThan(-10);
    }

    [Fact]
    public void ComputeTrendAndAnomalies_FlatValues_ReturnsStable()
    {
        // Arrange — all values identical → 0 % change
        var points = BuildPoints(75, 75, 75, 75, 75, 75, 75, 75);

        // Act
        var result = _sut.ComputeTrendAndAnomalies("HeartRate", points, 30, 2.0);

        // Assert
        result.TrendStatus.Should().Be("Stable");
        result.PercentChange.Should().BeApproximately(0, 0.001);
    }

    // -------------------------------------------------------------------------
    // ComputeTrendAndAnomalies — anomaly detection
    // -------------------------------------------------------------------------

    [Fact]
    public void ComputeTrendAndAnomalies_OneStrongOutlier_DetectsAnomaly()
    {
        // Arrange — 7 values near 75 BPM, one spike to 160 (clearly anomalous)
        var values = new double[] { 74, 75, 76, 74, 75, 76, 74, 160 };
        var points = BuildPoints(values);

        // Act
        var result = _sut.ComputeTrendAndAnomalies("HeartRate", points, 30, 2.0);

        // Assert
        result.HasAnomalies.Should().BeTrue();
        result.Anomalies.Should().HaveCount(1);
        result.Anomalies[0].Value.Should().Be(160);
        result.Anomalies[0].ZScore.Should().BeGreaterThan(2.0);
    }

    [Fact]
    public void ComputeTrendAndAnomalies_OutlierAbove3StdDev_ClassifiedAsSevere()
    {
        // Arrange — 20 tightly clustered values + one extreme outlier.
        // With many normal points the outlier's influence on the mean/stddev is small,
        // ensuring |z| > 3.0 and a "Severe" classification.
        var values = Enumerable.Repeat(70.0, 20).Append(300.0).ToArray();
        var points = BuildPoints(values);

        // Act
        var result = _sut.ComputeTrendAndAnomalies("HeartRate", points, 30, 2.0);

        // Assert
        var anomaly = result.Anomalies.Should().ContainSingle().Subject;
        anomaly.Severity.Should().Be("Severe");
        anomaly.DeviationFromMean.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ComputeTrendAndAnomalies_NoOutliers_NoAnomaliesDetected()
    {
        // Arrange — tightly clustered values, no outlier
        var points = BuildPoints(74, 75, 76, 75, 74, 76, 75, 74);

        // Act
        var result = _sut.ComputeTrendAndAnomalies("HeartRate", points, 30, 2.0);

        // Assert
        result.HasAnomalies.Should().BeFalse();
        result.Anomalies.Should().BeEmpty();
    }

    [Fact]
    public void ComputeTrendAndAnomalies_MeanAndStdDevPopulated()
    {
        // Arrange
        var points = BuildPoints(60, 70, 80, 90, 100, 110, 120, 130);

        // Act
        var result = _sut.ComputeTrendAndAnomalies("Steps", points, 30, 2.0);

        // Assert
        result.Mean.Should().BeApproximately(95, 0.01);
        result.StandardDeviation.Should().BeGreaterThan(0);
        result.SampleCount.Should().Be(8);
    }

    // -------------------------------------------------------------------------
    // AnalyzeAsync — integration
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AnalyzeAsync_EmptyCollection_AllMetricsReturnInsufficientData()
    {
        // Arrange
        var collection = new HealthDataCollection();

        // Act
        var report = await _sut.AnalyzeAsync(collection, days: 30);

        // Assert
        report.Metrics.Should().HaveCount(4);
        report.Metrics.Should().OnlyContain(m => m.TrendStatus == "Insufficient Data");
        report.TotalAnomalies.Should().Be(0);
        report.OverallStatus.Should().Be("Healthy");
    }

    [Fact]
    public async Task AnalyzeAsync_CollectionWithHeartRateData_PopulatesHeartRateMetric()
    {
        // Arrange — 10 heart rate records with one spike
        var collection = new HealthDataCollection();
        var baseValues = new[] { 68, 70, 69, 71, 70, 68, 71, 70, 69, 150 };
        for (var i = 0; i < baseValues.Length; i++)
        {
            collection.HeartRateRecords.Add(new HeartRateData
            {
                RecordDate = DateTime.UtcNow.AddDays(-(baseValues.Length - i)),
                MinimumBpm = baseValues[i] - 5,
                AverageBpm = baseValues[i],
                MaximumBpm = baseValues[i] + 10,
                MeasurementCount = 48,
            });
        }

        // Act
        var report = await _sut.AnalyzeAsync(collection, days: 30);

        // Assert
        var hrMetric = report.Metrics.Should().Contain(m => m.MetricName == "HeartRate").Subject;
        hrMetric.SampleCount.Should().Be(10);
        hrMetric.HasAnomalies.Should().BeTrue();
        report.RequiresAttention.Should().BeTrue();
    }

    [Fact]
    public async Task AnalyzeAsync_NullCollection_ThrowsArgumentNullException()
    {
        // Act
        var act = async () => await _sut.AnalyzeAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AnalyzeAsync_CancelledToken_ThrowsOperationCancelledException()
    {
        // Arrange
        var collection = new HealthDataCollection();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await _sut.AnalyzeAsync(collection, cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -------------------------------------------------------------------------
    // AnomalyDetectionResultDto — computed properties
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(0, "Healthy")]
    [InlineData(2, "Monitor")]
    [InlineData(3, "Monitor")]
    [InlineData(4, "Alert")]
    public void AnomalyDetectionResultDto_OverallStatus_ReflectsAnomalyCount(
        int anomalyCount, string expectedStatus)
    {
        // Arrange
        var dto = new AnomalyDetectionResultDto();
        var metric = new MetricTrendResult { MetricName = "Test" };
        for (var i = 0; i < anomalyCount; i++)
            metric.Anomalies.Add(new AnomalyPoint { Value = 999 + i, Severity = "Minor" });

        dto.Metrics.Add(metric);

        // Act & Assert
        dto.OverallStatus.Should().Be(expectedStatus);
        dto.RequiresAttention.Should().Be(anomalyCount > 0);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static List<(DateTime Date, double Value)> BuildPoints(params double[] values)
    {
        return values.Select((v, i) => (DateTime.UtcNow.AddDays(i - values.Length), v)).ToList();
    }
}
