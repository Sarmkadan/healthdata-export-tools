#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace HealthDataExportTools.Interceptors;

/// <summary>
/// Interface for collecting and tracking metrics for operations
/// Provides insights into performance and usage patterns
/// </summary>
public interface IMetricsCollector
{
    /// <summary>
    /// Record successful operation
    /// </summary>
    void RecordSuccess(string operationName, TimeSpan duration, long itemsProcessed = 1);

    /// <summary>
    /// Record failed operation
    /// </summary>
    void RecordFailure(string operationName, Exception ex);

    /// <summary>
    /// Get metrics for a specific operation
    /// </summary>
    OperationMetrics? GetMetrics(string operationName);

    /// <summary>
    /// Get all metrics
    /// </summary>
    List<OperationMetrics> GetAllMetrics();

    /// <summary>
    /// Get summary of all metrics
    /// </summary>
    MetricsSummary GetSummary();

    /// <summary>
    /// Reset all metrics
    /// </summary>
    void Reset();

    /// <summary>
    /// Reset metrics for specific operation
    /// </summary>
    void ResetOperation(string operationName);
}

/// <summary>
/// Metrics for a single operation
/// </summary>
public sealed class OperationMetrics
{
    public string OperationName { get; set; } = string.Empty;
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public long TotalDurationMs { get; set; }
    public long TotalItemsProcessed { get; set; }
    public long MinDurationMs { get; set; } = long.MaxValue;
    public long MaxDurationMs { get; set; }
    public DateTime FirstExecutionTime { get; set; }
    public DateTime LastExecutionTime { get; set; }

    public double AverageDurationMs => SuccessCount > 0 ? (double)TotalDurationMs / SuccessCount : 0;
    public double Throughput => TotalDurationMs > 0 ? (TotalItemsProcessed * 1000.0) / TotalDurationMs : 0;

    public void IncrementSuccess(TimeSpan duration, long itemsProcessed = 1)
    {
        SuccessCount++;
        var durationMs = (long)duration.TotalMilliseconds;
        TotalDurationMs += durationMs;
        TotalItemsProcessed += itemsProcessed;
        MinDurationMs = Math.Min(MinDurationMs, durationMs);
        MaxDurationMs = Math.Max(MaxDurationMs, durationMs);
        LastExecutionTime = DateTime.UtcNow;

        if (FirstExecutionTime == default)
            FirstExecutionTime = DateTime.UtcNow;
    }

    public void IncrementFailure(Exception ex)
    {
        FailureCount++;
        LastExecutionTime = DateTime.UtcNow;

        if (FirstExecutionTime == default)
            FirstExecutionTime = DateTime.UtcNow;
    }
}

/// <summary>
/// Summary of all metrics
/// </summary>
public sealed class MetricsSummary
{
    public int TotalOperations { get; set; }
    public int TotalSuccessful { get; set; }
    public int TotalFailed { get; set; }
    public double SuccessRate { get; set; }
    public double AverageDuration { get; set; }
    public long TotalItemsProcessed { get; set; }
}