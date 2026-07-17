namespace HealthDataExportTools.Tests;

using FluentAssertions;
using HealthDataExportTools.DTOs;
using HealthDataExportTools.Services;

public static class TrendAnomalyDetectionServiceTestsExtensions
{
    /// <summary>
    /// Executes the stable trend detection test scenario and returns the result for verification.
    /// </summary>
    /// <param name="tests">The <see cref="TrendAnomalyDetectionServiceTests"/> instance to test.</param>
    /// <param name="expectedTrend">The expected trend value (percent change).</param>
    /// <param name="expectedAnomalies">The expected anomaly values.</param>
    /// <returns>The computed <see cref="MetricTrendResult"/> for further assertions.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static MetricTrendResult AssertStableTrend(this TrendAnomalyDetectionServiceTests tests, double expectedTrend, IReadOnlyList<int> expectedAnomalies)
    {
        ArgumentNullException.ThrowIfNull(tests);

        // Get the service instance via reflection
        var serviceField = typeof(TrendAnomalyDetectionServiceTests)
            .GetField("_service", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?? throw new InvalidOperationException("Test class is missing expected private _service field");

        var service = serviceField.GetValue(tests) as TrendAnomalyDetectionService
            ?? throw new InvalidOperationException("_service field is null or not of type TrendAnomalyDetectionService");

        // Arrange test data for stable trend
        var points = new List<(DateTime Date, double Value)>();
        for (int i = 0; i < 10; i++)
        {
            points.Add((DateTime.Today.AddDays(i), 100 + i * 0.5)); // Slight increase
        }

        // Act
        var result = service.ComputeTrendAndAnomalies("TestMetric", points, 30, 2.0);

        // Assert
        result.TrendStatus.Should().Be("Stable", "Expected stable trend status");
        result.PercentChange.Should().BeInRange(-10.0, 10.0, "Percent change should be within expected range for stable trend");
        result.Anomalies.Should().BeEmpty("Stable trend should have no anomalies");

        if (expectedAnomalies.Count > 0)
        {
            result.Anomalies.Should().HaveCount(expectedAnomalies.Count, "Expected specific number of anomalies");
            foreach (var expectedValue in expectedAnomalies)
            {
                result.Anomalies.Should().Contain(a => Math.Abs(a.Value - expectedValue) < 0.01, "Expected to find anomaly with value {0}", expectedValue);
            }
        }

        return result;
    }

    /// <summary>
    /// Executes the improving trend detection test scenario and returns the result for verification.
    /// </summary>
    /// <param name="tests">The <see cref="TrendAnomalyDetectionServiceTests"/> instance to test.</param>
    /// <param name="expectedTrend">The expected trend value (percent change).</param>
    /// <param name="expectedAnomalies">The expected anomaly values.</param>
    /// <returns>The computed <see cref="MetricTrendResult"/> for further assertions.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static MetricTrendResult AssertImprovingTrend(this TrendAnomalyDetectionServiceTests tests, double expectedTrend, IReadOnlyList<int> expectedAnomalies)
    {
        ArgumentNullException.ThrowIfNull(tests);

        // Get the service instance via reflection
        var serviceField = typeof(TrendAnomalyDetectionServiceTests)
            .GetField("_service", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?? throw new InvalidOperationException("Test class is missing expected private _service field");

        var service = serviceField.GetValue(tests) as TrendAnomalyDetectionService
            ?? throw new InvalidOperationException("_service field is null or not of type TrendAnomalyDetectionService");

        // Arrange test data for improving trend
        var points = new List<(DateTime Date, double Value)>();
        for (int i = 0; i < 10; i++)
        {
            points.Add((DateTime.Today.AddDays(i), 100 + i * 5)); // Significant increase
        }

        // Act
        var result = service.ComputeTrendAndAnomalies("TestMetric", points, 30, 2.0);

        // Assert
        result.TrendStatus.Should().Be("Improving", "Expected improving trend status");
        result.PercentChange.Should().BeGreaterThan(10.0, "Improving trend should have positive percent change > 10%");
        result.Anomalies.Should().BeEmpty("Improving trend should have no anomalies");

        if (expectedAnomalies.Count > 0)
        {
            result.Anomalies.Should().HaveCount(expectedAnomalies.Count, "Expected specific number of anomalies");
            foreach (var expectedValue in expectedAnomalies)
            {
                result.Anomalies.Should().Contain(a => Math.Abs(a.Value - expectedValue) < 0.01, "Expected to find anomaly with value {0}", expectedValue);
            }
        }

        return result;
    }

    /// <summary>
    /// Executes the declining trend detection test scenario and returns the result for verification.
    /// </summary>
    /// <param name="tests">The <see cref="TrendAnomalyDetectionServiceTests"/> instance to test.</param>
    /// <param name="expectedTrend">The expected trend value (percent change).</param>
    /// <param name="expectedAnomalies">The expected anomaly values.</param>
    /// <returns>The computed <see cref="MetricTrendResult"/> for further assertions.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
    public static MetricTrendResult AssertDecliningTrend(this TrendAnomalyDetectionServiceTests tests, double expectedTrend, IReadOnlyList<int> expectedAnomalies)
    {
        ArgumentNullException.ThrowIfNull(tests);

        // Get the service instance via reflection
        var serviceField = typeof(TrendAnomalyDetectionServiceTests)
            .GetField("_service", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?? throw new InvalidOperationException("Test class is missing expected private _service field");

        var service = serviceField.GetValue(tests) as TrendAnomalyDetectionService
            ?? throw new InvalidOperationException("_service field is null or not of type TrendAnomalyDetectionService");

        // Arrange test data for declining trend
        var points = new List<(DateTime Date, double Value)>();
        for (int i = 0; i < 10; i++)
        {
            points.Add((DateTime.Today.AddDays(i), 150 - i * 5)); // Significant decrease
        }

        // Act
        var result = service.ComputeTrendAndAnomalies("TestMetric", points, 30, 2.0);

        // Assert
        result.TrendStatus.Should().Be("Declining", "Expected declining trend status");
        result.PercentChange.Should().BeLessThan(-10.0, "Declining trend should have negative percent change < -10%");
        result.Anomalies.Should().BeEmpty("Declining trend should have no anomalies");

        if (expectedAnomalies.Count > 0)
        {
            result.Anomalies.Should().HaveCount(expectedAnomalies.Count, "Expected specific number of anomalies");
            foreach (var expectedValue in expectedAnomalies)
            {
                result.Anomalies.Should().Contain(a => Math.Abs(a.Value - expectedValue) < 0.01, "Expected to find anomaly with value {0}", expectedValue);
            }
        }

        return result;
    }
}
