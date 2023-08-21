// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Correlation;

/// <summary>
/// Categorical strength of a Pearson correlation coefficient.
/// Values are ordered so that ≥ comparisons work correctly.
/// </summary>
public enum CorrelationStrength
{
    /// <summary>|r| &lt; 0.10 — effectively random noise.</summary>
    Negligible = 0,

    /// <summary>0.10 ≤ |r| &lt; 0.30 — small but present relationship.</summary>
    Weak = 1,

    /// <summary>0.30 ≤ |r| &lt; 0.50 — meaningful, practically notable.</summary>
    Moderate = 2,

    /// <summary>0.50 ≤ |r| &lt; 0.70 — robust, clearly present.</summary>
    Strong = 3,

    /// <summary>|r| ≥ 0.70 — dominant linear relationship.</summary>
    VeryStrong = 4,
}

/// <summary>Sign of the correlation coefficient.</summary>
public enum CorrelationDirection
{
    /// <summary>Metrics move in opposite directions.</summary>
    Negative = -1,

    /// <summary>No meaningful linear relationship detected.</summary>
    None = 0,

    /// <summary>Metrics move in the same direction.</summary>
    Positive = 1,
}

/// <summary>
/// Importance level of a cross-metric health insight.
/// Ordered so that ≥ comparisons work correctly.
/// </summary>
public enum InsightSeverity
{
    /// <summary>Interesting but does not require action.</summary>
    Informational = 0,

    /// <summary>Pattern worth monitoring over the coming weeks.</summary>
    Moderate = 1,

    /// <summary>Clinically relevant pattern; actionable recommendation included.</summary>
    Significant = 2,
}

/// <summary>Controls how many insights the engine emits beyond the curated knowledge base.</summary>
public enum InsightGenerationMode
{
    /// <summary>Only emit insights from the curated pattern library.</summary>
    Minimal,

    /// <summary>Curated patterns plus generic commentary on strong correlations.</summary>
    Standard,

    /// <summary>All significant correlations receive at least a generic insight.</summary>
    Comprehensive,
}

/// <summary>
/// An immutable, value-typed identifier for an ordered pair of metrics to correlate.
/// The canonical form has MetricA ≤ MetricB lexicographically to avoid duplicates.
/// </summary>
public readonly record struct CorrelationPair(string MetricA, string MetricB)
{
    /// <summary>Human-readable label used in logs and insight titles.</summary>
    public override string ToString() => $"{MetricA} ↔ {MetricB}";
}

/// <summary>
/// A date-ordered sequence of scalar values for a single named health metric,
/// pre-aggregated to one value per calendar day.
/// </summary>
/// <param name="MetricName">Stable identifier matching the engine's internal metric names.</param>
/// <param name="DataPoints">Chronologically ordered (date, value) pairs.</param>
public sealed record MetricTimeSeries(string MetricName, IReadOnlyList<(DateOnly Date, double Value)> DataPoints)
{
    /// <summary>Number of days in the series.</summary>
    public int Count => DataPoints.Count;

    /// <summary>Returns <see langword="true"/> when the series has at least <paramref name="minimum"/> days.</summary>
    public bool HasSufficientData(int minimum) => Count >= minimum;

    /// <summary>Flattened value sequence, preserving chronological order.</summary>
    public IReadOnlyList<double> Values => DataPoints.Select(p => p.Value).ToArray();
}

/// <summary>
/// Result of a lagged Pearson correlation: how strongly does metric A on day N
/// predict metric B <paramref name="LagDays"/> days later.
/// </summary>
/// <param name="MetricA">The leading (predictor) metric.</param>
/// <param name="MetricB">The lagging (outcome) metric.</param>
/// <param name="LagDays">Number of days B is shifted relative to A.</param>
/// <param name="Coefficient">Pearson r for the lagged pair.</param>
/// <param name="SampleCount">Effective sample size after applying the lag.</param>
/// <param name="Interpretation">Plain-language description of the lagged relationship.</param>
public sealed record LaggedCorrelationResult(
    string MetricA,
    string MetricB,
    int LagDays,
    double Coefficient,
    int SampleCount,
    string Interpretation)
{
    /// <summary>Absolute value of the correlation coefficient.</summary>
    public double AbsoluteCoefficient => Math.Abs(Coefficient);

    /// <summary><see langword="true"/> when this is a same-day (zero-lag) result.</summary>
    public bool IsSameDay => LagDays == 0;
}
