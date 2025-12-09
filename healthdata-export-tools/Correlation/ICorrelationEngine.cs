// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools.Configuration;
using HealthDataExportTools.DTOs;

namespace HealthDataExportTools.Correlation;

/// <summary>
/// Analyses relationships between health metrics extracted from a
/// <see cref="HealthDataCollection"/> and surfaces cross-metric insights.
/// </summary>
/// <remarks>
/// Implementations must be thread-safe; the default implementation
/// (<see cref="CorrelationEngine"/>) is safe for singleton registration.
/// </remarks>
public interface ICorrelationEngine
{
    /// <summary>
    /// Runs a full cross-metric correlation analysis over the supplied collection
    /// and returns all computed pairs together with curated health insights.
    /// </summary>
    /// <param name="collection">
    /// Source health data. The engine reads sleep, heart-rate, SpO₂, and steps records.
    /// </param>
    /// <param name="options">
    /// Tuning parameters. Defaults to <see cref="CorrelationEngineOptions.Default"/> when
    /// <see langword="null"/>.
    /// </param>
    /// <param name="cancellationToken">Token to cancel long-running parallel computations.</param>
    /// <returns>
    /// A <see cref="CorrelationAnalysisResultDto"/> containing all correlations and insights.
    /// </returns>
    Task<CorrelationAnalysisResultDto> AnalyzeAsync(
        HealthDataCollection collection,
        CorrelationEngineOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience method that returns only the insight layer from a full analysis,
    /// using a simplified single-parameter API.
    /// </summary>
    /// <param name="collection">Source health data.</param>
    /// <param name="windowDays">Look-back window in calendar days. Default: 30.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<CrossMetricInsightDto>> GetInsightsAsync(
        HealthDataCollection collection,
        int windowDays = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes the Pearson product-moment correlation coefficient between two
    /// equal-length value sequences.
    /// </summary>
    /// <param name="x">First variable — must be same length as <paramref name="y"/>.</param>
    /// <param name="y">Second variable.</param>
    /// <returns>
    /// r ∈ [-1, 1], or <see cref="double.NaN"/> when inputs are too short or collinear.
    /// </returns>
    double ComputePearsonCorrelation(IReadOnlyList<double> x, IReadOnlyList<double> y);

    /// <summary>
    /// Builds per-metric <see cref="MetricTimeSeries"/> from all data types in the
    /// collection, filtering to the most recent <paramref name="windowDays"/> days
    /// and aggregating multiple records per day to a single scalar value.
    /// </summary>
    /// <param name="collection">Source health data.</param>
    /// <param name="windowDays">Look-back window in calendar days.</param>
    /// <returns>One <see cref="MetricTimeSeries"/> per available metric.</returns>
    IReadOnlyList<MetricTimeSeries> ExtractTimeSeries(HealthDataCollection collection, int windowDays);

    /// <summary>
    /// Explores temporal lead-lag relationships between two pre-extracted time series,
    /// shifting metric B by 0–<paramref name="maxLagDays"/> days and computing the
    /// Pearson r at each offset.
    /// </summary>
    /// <param name="a">The leading (predictor) metric series.</param>
    /// <param name="b">The lagging (outcome) metric series.</param>
    /// <param name="maxLagDays">Maximum shift to explore. Default: 7.</param>
    /// <returns>
    /// Results ordered by |r| descending, with lag=0 representing the same-day correlation.
    /// </returns>
    IReadOnlyList<LaggedCorrelationResult> AnalyzeLag(
        MetricTimeSeries a,
        MetricTimeSeries b,
        int maxLagDays = 7);

    /// <summary>
    /// Computes the Pearson r between metric A and metric B shifted by <paramref name="lagDays"/>.
    /// A positive lag tests whether A predicts B in the future.
    /// </summary>
    /// <param name="x">Predictor values (metric A).</param>
    /// <param name="y">Outcome values (metric B), same length as <paramref name="x"/>.</param>
    /// <param name="lagDays">Days to shift <paramref name="y"/> forward relative to <paramref name="x"/>.</param>
    /// <returns>Pearson r for the lagged pair, or <see cref="double.NaN"/> if insufficient data.</returns>
    double ComputeLaggedCorrelation(IReadOnlyList<double> x, IReadOnlyList<double> y, int lagDays);
}
