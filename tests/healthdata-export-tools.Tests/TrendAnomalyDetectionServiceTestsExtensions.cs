namespace HealthDataExportTools.Tests;

public static class TrendAnomalyDetectionServiceTestsExtensions
{
    /// <summary>
    /// Verifies that the given <paramref name="tests"/> instance has a valid detection result for a stable trend.
    /// </summary>
    /// <param name="tests">The <see cref="TrendAnomalyDetectionServiceTests"/> instance to verify.</param>
    /// <param name="expectedTrend">The expected trend value.</param>
    /// <param name="expectedAnomalies">The expected anomalies.</param>
    public static void AssertStableTrend(this TrendAnomalyDetectionServiceTests tests, double expectedTrend, IReadOnlyList<int> expectedAnomalies)
    {
        ArgumentNullException.ThrowIfNull(tests);
        tests.ComputeTrendAndAnomalies_ShouldDetectStableTrend();
        // TO DO: implement assertion logic
    }

    /// <summary>
    /// Verifies that the given <paramref name="tests"/> instance has a valid detection result for an improving trend.
    /// </summary>
    /// <param name="tests">The <see cref="TrendAnomalyDetectionServiceTests"/> instance to verify.</param>
    /// <param name="expectedTrend">The expected trend value.</param>
    /// <param name="expectedAnomalies">The expected anomalies.</param>
    public static void AssertImprovingTrend(this TrendAnomalyDetectionServiceTests tests, double expectedTrend, IReadOnlyList<int> expectedAnomalies)
    {
        ArgumentNullException.ThrowIfNull(tests);
        tests.ComputeTrendAndAnomalies_ShouldDetectImprovingTrend();
        // TO DO: implement assertion logic
    }

    /// <summary>
    /// Verifies that the given <paramref name="tests"/> instance has a valid detection result for a declining trend.
    /// </summary>
    /// <param name="tests">The <see cref="TrendAnomalyDetectionServiceTests"/> instance to verify.</param>
    /// <param name="expectedTrend">The expected trend value.</param>
    /// <param name="expectedAnomalies">The expected anomalies.</param>
    public static void AssertDecliningTrend(this TrendAnomalyDetectionServiceTests tests, double expectedTrend, IReadOnlyList<int> expectedAnomalies)
    {
        ArgumentNullException.ThrowIfNull(tests);
        tests.ComputeTrendAndAnomalies_ShouldDetectDecliningTrend();
        // TO DO: implement assertion logic
    }
}
