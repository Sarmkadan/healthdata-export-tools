#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools.Correlation;

namespace HealthDataExportTools.Configuration;

/// <summary>
/// Tuning parameters for the <see cref="HealthDataExportTools.Correlation.ICorrelationEngine"/>.
/// All properties expose sensible defaults suitable for 30-day wearable exports.
/// </summary>
public sealed class CorrelationEngineOptions
{
    /// <summary>
    /// Shared immutable default instance.
    /// Do not mutate; create a new instance when customisation is required.
    /// </summary>
    public static readonly CorrelationEngineOptions Default = new();

    /// <summary>
    /// Number of calendar days to look back when building per-metric time series.
    /// Larger windows improve statistical confidence but may span device-change boundaries.
    /// Default: <c>30</c>.
    /// </summary>
    public int AnalysisWindowDays { get; set; } = 30;

    /// <summary>
    /// Minimum absolute Pearson |r| required for a correlation to be considered statistically
    /// meaningful and included in the results (when <see cref="IncludeWeakCorrelations"/> is
    /// <see langword="false"/>). Range: [0, 1]. Default: <c>0.30</c>.
    /// </summary>
    public double SignificanceThreshold { get; set; } = 0.30;

    /// <summary>
    /// Minimum number of aligned days required before a metric pair is computed.
    /// Pairs with fewer shared dates are silently skipped. Default: <c>7</c>.
    /// </summary>
    public int MinimumSampleCount { get; set; } = 7;

    /// <summary>
    /// When <see langword="true"/>, correlations below <see cref="SignificanceThreshold"/>
    /// are retained in the output. Useful for diagnostic purposes.
    /// Default: <see langword="false"/>.
    /// </summary>
    public bool IncludeWeakCorrelations { get; set; } = false;

    /// <summary>
    /// When <see langword="true"/>, all N×(N-1)/2 metric pairs extracted from the collection
    /// are analysed. When <see langword="false"/>, only the engine's curated default pairs
    /// plus any entries in <see cref="AdditionalMetricPairs"/> are computed.
    /// Default: <see langword="true"/>.
    /// </summary>
    public bool ComputeAllPairs { get; set; } = true;

    /// <summary>
    /// When <see langword="true"/>, metric pairs are processed concurrently using
    /// <see cref="System.Threading.Tasks.Parallel.ForEachAsync{TSource}"/>.
    /// Default: <see langword="true"/>.
    /// </summary>
    public bool EnableParallelComputation { get; set; } = true;

    /// <summary>
    /// Maximum number of concurrent tasks used during parallel correlation computation.
    /// Has no effect when <see cref="EnableParallelComputation"/> is <see langword="false"/>.
    /// Default: <c>4</c>.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = 4;

    /// <summary>
    /// Controls how many insights are emitted for correlations that are not in the
    /// curated pattern library. Default: <see cref="InsightGenerationMode.Standard"/>.
    /// </summary>
    public InsightGenerationMode InsightMode { get; set; } = InsightGenerationMode.Standard;

    /// <summary>
    /// Extra metric pairs to analyse in addition to the defaults (or instead of them when
    /// <see cref="ComputeAllPairs"/> is <see langword="false"/>).
    /// </summary>
    public IList<CorrelationPair> AdditionalMetricPairs { get; set; } = [];

    /// <summary>
    /// Maximum lag (in days) to explore when calling
    /// <see cref="ICorrelationEngine.AnalyzeLag"/>. Default: <c>7</c>.
    /// </summary>
    public int MaxLagDays { get; set; } = 7;

    /// <summary>
    /// Validates the option values and returns any configuration errors.
    /// </summary>
    /// <returns>Enumeration of human-readable error messages; empty if valid.</returns>
    public IEnumerable<string> Validate()
    {
        if (AnalysisWindowDays < 1)
            yield return $"{nameof(AnalysisWindowDays)} must be at least 1.";
        if (SignificanceThreshold is < 0 or > 1)
            yield return $"{nameof(SignificanceThreshold)} must be between 0 and 1.";
        if (MinimumSampleCount < 2)
            yield return $"{nameof(MinimumSampleCount)} must be at least 2.";
        if (MaxDegreeOfParallelism < 1)
            yield return $"{nameof(MaxDegreeOfParallelism)} must be at least 1.";
        if (MaxLagDays < 0)
            yield return $"{nameof(MaxLagDays)} must be non-negative.";
        if (MinimumSampleCount > AnalysisWindowDays)
            yield return $"{nameof(MinimumSampleCount)} ({MinimumSampleCount}) exceeds {nameof(AnalysisWindowDays)} ({AnalysisWindowDays}).";
    }

    /// <summary>Returns <see langword="true"/> when no validation errors are present.</summary>
    public bool IsValid() => !Validate().Any();
}
