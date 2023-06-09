// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Interceptors;

/// <summary>
/// Collects and tracks metrics for operations
/// Provides insights into performance and usage patterns
/// </summary>
public class MetricsCollector
{
    private readonly Dictionary<string, OperationMetrics> _metrics;
    private readonly ReaderWriterLockSlim _metricsLock;
    private readonly ILogger<MetricsCollector> _logger;

    public MetricsCollector(ILogger<MetricsCollector> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metrics = new Dictionary<string, OperationMetrics>();
        _metricsLock = new ReaderWriterLockSlim();
    }

    /// <summary>
    /// Record successful operation
    /// </summary>
    public void RecordSuccess(string operationName, TimeSpan duration, long itemsProcessed = 1)
    {
        _metricsLock.EnterUpgradeableReadLock();
        try
        {
            if (!_metrics.TryGetValue(operationName, out var metrics))
            {
                _metricsLock.EnterWriteLock();
                try
                {
                    metrics = new OperationMetrics { OperationName = operationName };
                    _metrics[operationName] = metrics;
                }
                finally
                {
                    _metricsLock.ExitWriteLock();
                }
            }

            metrics.IncrementSuccess(duration, itemsProcessed);
        }
        finally
        {
            _metricsLock.ExitUpgradeableReadLock();
        }
    }

    /// <summary>
    /// Record failed operation
    /// </summary>
    public void RecordFailure(string operationName, Exception ex)
    {
        _metricsLock.EnterUpgradeableReadLock();
        try
        {
            if (!_metrics.TryGetValue(operationName, out var metrics))
            {
                _metricsLock.EnterWriteLock();
                try
                {
                    metrics = new OperationMetrics { OperationName = operationName };
                    _metrics[operationName] = metrics;
                }
                finally
                {
                    _metricsLock.ExitWriteLock();
                }
            }

            metrics.IncrementFailure(ex);
        }
        finally
        {
            _metricsLock.ExitUpgradeableReadLock();
        }
    }

    /// <summary>
    /// Get metrics for a specific operation
    /// </summary>
    public OperationMetrics? GetMetrics(string operationName)
    {
        _metricsLock.EnterReadLock();
        try
        {
            return _metrics.TryGetValue(operationName, out var metrics) ? metrics : null;
        }
        finally
        {
            _metricsLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Get all metrics
    /// </summary>
    public List<OperationMetrics> GetAllMetrics()
    {
        _metricsLock.EnterReadLock();
        try
        {
            return _metrics.Values.ToList();
        }
        finally
        {
            _metricsLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Get summary of all metrics
    /// </summary>
    public MetricsSummary GetSummary()
    {
        _metricsLock.EnterReadLock();
        try
        {
            var allMetrics = _metrics.Values.ToList();

            return new MetricsSummary
            {
                TotalOperations = allMetrics.Sum(m => m.SuccessCount + m.FailureCount),
                TotalSuccessful = allMetrics.Sum(m => m.SuccessCount),
                TotalFailed = allMetrics.Sum(m => m.FailureCount),
                SuccessRate = allMetrics.Sum(m => m.SuccessCount) > 0
                    ? (double)allMetrics.Sum(m => m.SuccessCount) /
                      (allMetrics.Sum(m => m.SuccessCount + m.FailureCount)) * 100
                    : 0,
                AverageDuration = allMetrics.Where(m => m.SuccessCount > 0)
                    .Average(m => m.AverageDurationMs),
                TotalItemsProcessed = allMetrics.Sum(m => m.TotalItemsProcessed)
            };
        }
        finally
        {
            _metricsLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Reset all metrics
    /// </summary>
    public void Reset()
    {
        _metricsLock.EnterWriteLock();
        try
        {
            _metrics.Clear();
            _logger.LogInformation("All metrics reset");
        }
        finally
        {
            _metricsLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Reset metrics for specific operation
    /// </summary>
    public void ResetOperation(string operationName)
    {
        _metricsLock.EnterWriteLock();
        try
        {
            if (_metrics.Remove(operationName))
            {
                _logger.LogInformation("Metrics reset for operation: {OperationName}", operationName);
            }
        }
        finally
        {
            _metricsLock.ExitWriteLock();
        }
    }
}

/// <summary>
/// Metrics for a single operation
/// </summary>
public class OperationMetrics
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
public class MetricsSummary
{
    public int TotalOperations { get; set; }
    public int TotalSuccessful { get; set; }
    public int TotalFailed { get; set; }
    public double SuccessRate { get; set; }
    public double AverageDuration { get; set; }
    public long TotalItemsProcessed { get; set; }
}
