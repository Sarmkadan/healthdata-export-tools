#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools.Correlation;

namespace HealthDataExportTools.DTOs;

/// <summary>
/// Correlation result for a single metric pair, suitable for serialisation
/// and UI display. Immutable by design; all properties are <c>required init</c>.
/// </summary>
public sealed record MetricCorrelationDto
{
    /// <summary>The two metrics that were correlated.</summary>
    public required CorrelationPair Pair { get; init; }

    /// <summary>
    /// Pearson r coefficient, rounded to 4 decimal places.
    /// Range: [-1, 1]. Positive values indicate direct relationships,
    /// negative values indicate inverse relationships.
    /// </summary>
    public required double Coefficient { get; init; }

    /// <summary>Categorical strength derived from the absolute coefficient.</summary>
    public required CorrelationStrength Strength { get; init; }

    /// <summary>Sign of the relationship (positive, negative, or none).</summary>
    public required CorrelationDirection Direction { get; init; }

    /// <summary>Number of date-aligned data points used in the computation.</summary>
    public required int SampleCount { get; init; }

    /// <summary>Plain-language summary of the coefficient, strength, and direction.</summary>
    public required string Interpretation { get; init; }

    /// <summary>First day included in the analysis window for this pair.</summary>
    public required DateOnly AnalysisPeriodStart { get; init; }

    /// <summary>Last day included in the analysis window for this pair.</summary>
    public required DateOnly AnalysisPeriodEnd { get; init; }

    /// <summary>Absolute value of <see cref="Coefficient"/>; used for ranking.</summary>
    public double AbsoluteCoefficient => Math.Abs(Coefficient);

    /// <summary>
    /// Returns <see langword="true"/> when the absolute coefficient meets or exceeds
    /// <paramref name="threshold"/>. Default threshold aligns with <c>CorrelationEngineOptions.Default</c>.
    /// </summary>
    public bool IsSignificant(double threshold = 0.30) => AbsoluteCoefficient >= threshold;

    /// <summary>Number of days spanned by the analysis window for this pair.</summary>
    public int AnalysisSpanDays => AnalysisPeriodEnd.DayNumber - AnalysisPeriodStart.DayNumber + 1;
}

/// <summary>
/// A health insight synthesised from one or more significant correlations.
/// Pairs an evidence-based description with a concrete, actionable recommendation.
/// </summary>
public sealed record CrossMetricInsightDto
{
    /// <summary>Short headline suitable for dashboard cards or notification titles.</summary>
    public required string Title { get; init; }

    /// <summary>Evidence-backed description referencing the correlation coefficient.</summary>
    public required string Description { get; init; }

    /// <summary>The metric pair that drove this insight.</summary>
    public required CorrelationPair RelatedPair { get; init; }

    /// <summary>The Pearson r value that triggered this insight.</summary>
    public required double Coefficient { get; init; }

    /// <summary>Importance level, used for sorting and alert-threshold decisions.</summary>
    public required InsightSeverity Severity { get; init; }

    /// <summary>Concrete, actionable step the user can take in response to this pattern.</summary>
    public required string Recommendation { get; init; }

    /// <summary>Absolute coefficient; used for secondary sorting within the same severity tier.</summary>
    public double AbsoluteCoefficient => Math.Abs(Coefficient);
}

/// <summary>
/// Complete output of a <see cref="HealthDataExportTools.Correlation.ICorrelationEngine.AnalyzeAsync"/> call.
/// Contains all computed correlations, curated insights, and summary statistics.
/// </summary>
public sealed record CorrelationAnalysisResultDto
{
    /// <summary>Unique identifier for this analysis run (12-character hex).</summary>
    public required string AnalysisId { get; init; }

    /// <summary>UTC timestamp when the analysis was completed.</summary>
    public required DateTimeOffset GeneratedAt { get; init; }

    /// <summary>Look-back window used to build the time series.</summary>
    public required int WindowDays { get; init; }

    /// <summary>All computed (and optionally filtered) metric-pair correlations, ordered by |r| descending.</summary>
    public required IReadOnlyList<MetricCorrelationDto> Correlations { get; init; }

    /// <summary>Curated insights derived from the significant correlations, ordered by severity then |r|.</summary>
    public required IReadOnlyList<CrossMetricInsightDto> Insights { get; init; }

    /// <summary>Total number of metric pairs that had sufficient aligned data for computation.</summary>
    public required int TotalMetricPairsAnalyzed { get; init; }

    /// <summary>Number of correlations that met the significance threshold.</summary>
    public required int SignificantCorrelationsFound { get; init; }

    // ── Computed convenience properties ──────────────────────────────────────

    /// <summary>
    /// The single highest-|r| correlation in the result set, or <see langword="null"/>
    /// when no pairs could be computed.
    /// </summary>
    public MetricCorrelationDto? StrongestCorrelation =>
        Correlations.Count > 0 ? Correlations.MaxBy(c => c.AbsoluteCoefficient) : null;

    /// <summary>All correlations meeting the default 0.30 significance threshold.</summary>
    public IReadOnlyList<MetricCorrelationDto> SignificantCorrelations =>
        Correlations.Where(c => c.IsSignificant()).ToArray();

    /// <summary>
    /// <see langword="true"/> when at least one insight carries
    /// <see cref="InsightSeverity.Significant"/> severity.
    /// </summary>
    public bool HasActionableInsights =>
        Insights.Any(i => i.Severity >= InsightSeverity.Significant);

    /// <summary>
    /// <see langword="true"/> when the engine found no computable metric pairs
    /// (typically because the collection lacks sufficient data).
    /// </summary>
    public bool IsEmpty => TotalMetricPairsAnalyzed == 0;

    /// <summary>
    /// Average absolute correlation across all computed pairs.
    /// Returns 0 when no pairs were computed.
    /// </summary>
    public double AverageAbsoluteCoefficient =>
        Correlations.Count > 0 ? Correlations.Average(c => c.AbsoluteCoefficient) : 0.0;
}
