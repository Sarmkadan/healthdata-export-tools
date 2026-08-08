public enum CorrelationStrength
{
    /// <summary>|r| < 0.10 — effectively random noise.</summary>
    Negligible = 0,
    /// <summary>0.10 ≤ |r| < 0.30 — small but present relationship.</summary>
    Weak = 1,
    /// <summary>0.30 ≤ |r| < 0.50 — meaningful, practically notable.</summary>
    Moderate = 2,
    /// <summary>0.50 ≤ |r| < 0.70 — robust, clearly present.</summary>
    Strong = 3,
    /// <summary>|r| ≥ 0.70 — dominant linear relationship.</summary>
    VeryStrong = 4,
}

public enum CorrelationDirection
{
    /// <summary>Metrics move in opposite directions.</summary>
    Negative = -1,
    /// <summary>No meaningful linear relationship detected.</summary>
    None = 0,
    /// <summary>Metrics move in the same direction.</summary>
    Positive = 1,
}

public enum InsightSeverity
{
    /// <summary>Interesting but does not require action.</summary>
    Informational = 0,
    /// <summary>Pattern worth monitoring over the coming weeks.</summary>
    Moderate = 1,
    /// <summary>Clinically relevant pattern; actionable recommendation included.</summary>
    Significant = 2,
}

public enum InsightGenerationMode
{
    /// <summary>Only emit insights from the curated pattern library.</summary>
    Minimal,
    /// <summary>Curated patterns plus generic commentary on strong correlations.</summary>
    Standard,
    /// <summary>All significant correlations receive at least a generic insight.</summary>
    Comprehensive,
}

public readonly record struct CorrelationPair(string MetricA, string MetricB)
{
    /// <summary>Human-readable label used in logs and insight titles.</summary>
    public override string ToString() => $"{MetricA} ↔ {MetricB}";
}

public sealed record MetricTimeSeries(string MetricName, IReadOnlyList<(DateOnly Date, double Value)> DataPoints)
{
    /// <summary>Number of days in the series.</summary>
    public int Count => DataPoints.Count;
    /// <summary>Returns <see langword="true"/> when the series has at least <paramref name="minimum"/> days.</summary>
    public bool HasSufficientData(int minimum)
    {
        if (minimum == null)
        {
            throw new ArgumentNullException(nameof(minimum));
        }
        return Count >= minimum;
    }
    /// <summary>Flattened value sequence, preserving chronological order.</summary>
    public IReadOnlyList<double> Values => DataPoints.Select(p => p.Value).ToArray();
}

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