// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Utilities;

/// <summary>
/// Utility for performance monitoring and measurement
/// </summary>
public static class PerformanceUtility
{
    /// <summary>
    /// Measure execution time of an async operation
    /// </summary>
    public static async Task<(T Result, TimeSpan Duration)> MeasureAsync<T>(
        Func<Task<T>> operation,
        string operationName = "Operation")
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await operation();
            stopwatch.Stop();
            return (result, stopwatch.Elapsed);
        }
        catch
        {
            stopwatch.Stop();
            throw;
        }
    }

    /// <summary>
    /// Measure execution time of a sync operation
    /// </summary>
    public static (T Result, TimeSpan Duration) Measure<T>(
        Func<T> operation,
        string operationName = "Operation")
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = operation();
            stopwatch.Stop();
            return (result, stopwatch.Elapsed);
        }
        catch
        {
            stopwatch.Stop();
            throw;
        }
    }

    /// <summary>
    /// Get current memory usage
    /// </summary>
    public static MemoryUsage GetMemoryUsage()
    {
        var process = Process.GetCurrentProcess();

        return new MemoryUsage
        {
            WorkingSetBytes = process.WorkingSet64,
            PrivateMemoryBytes = process.PrivateMemorySize64,
            PeakMemoryBytes = process.PeakWorkingSet64,
            ManagedMemoryBytes = GC.GetTotalMemory(false)
        };
    }

    /// <summary>
    /// Get CPU usage information
    /// </summary>
    public static CpuUsage GetCpuUsage()
    {
        var process = Process.GetCurrentProcess();

        return new CpuUsage
        {
            ProcessorCount = Environment.ProcessorCount,
            UserProcessorTime = process.UserProcessorTime.TotalMilliseconds,
            TotalProcessorTime = process.TotalProcessorTime.TotalMilliseconds,
            Threads = process.Threads.Count
        };
    }

    /// <summary>
    /// Estimate throughput (records per second)
    /// </summary>
    public static double CalculateThroughput(int recordCount, TimeSpan duration)
    {
        if (duration.TotalSeconds == 0)
            return 0;

        return recordCount / duration.TotalSeconds;
    }

    /// <summary>
    /// Estimate time remaining for operation
    /// </summary>
    public static TimeSpan EstimateTimeRemaining(
        int processedItems,
        int totalItems,
        TimeSpan elapsedTime)
    {
        if (processedItems == 0 || totalItems == 0)
            return TimeSpan.Zero;

        var itemsRemaining = totalItems - processedItems;
        var timePerItem = elapsedTime.TotalSeconds / processedItems;
        var secondsRemaining = itemsRemaining * timePerItem;

        return TimeSpan.FromSeconds(secondsRemaining);
    }

    /// <summary>
    /// Get percentage complete
    /// </summary>
    public static double CalculatePercentComplete(int processedItems, int totalItems)
    {
        if (totalItems == 0)
            return 0;

        return (processedItems * 100.0) / totalItems;
    }

    /// <summary>
    /// Format duration in human-readable format
    /// </summary>
    public static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalSeconds < 1)
            return $"{duration.TotalMilliseconds:F0}ms";

        if (duration.TotalMinutes < 1)
            return $"{duration.TotalSeconds:F2}s";

        if (duration.TotalHours < 1)
            return $"{duration.TotalMinutes:F2}m";

        return $"{duration.TotalHours:F2}h";
    }
}

/// <summary>
/// Memory usage information
/// </summary>
public class MemoryUsage
{
    public long WorkingSetBytes { get; set; }
    public long PrivateMemoryBytes { get; set; }
    public long PeakMemoryBytes { get; set; }
    public long ManagedMemoryBytes { get; set; }

    public string GetWorkingSetMB() => $"{WorkingSetBytes / 1024.0 / 1024.0:F2} MB";
    public string GetManagedMemoryMB() => $"{ManagedMemoryBytes / 1024.0 / 1024.0:F2} MB";
    public string GetPeakMemoryMB() => $"{PeakMemoryBytes / 1024.0 / 1024.0:F2} MB";

    public override string ToString()
    {
        return $"Memory - Working: {GetWorkingSetMB()}, Managed: {GetManagedMemoryMB()}, Peak: {GetPeakMemoryMB()}";
    }
}

/// <summary>
/// CPU usage information
/// </summary>
public class CpuUsage
{
    public int ProcessorCount { get; set; }
    public double UserProcessorTime { get; set; }
    public double TotalProcessorTime { get; set; }
    public int Threads { get; set; }

    public override string ToString()
    {
        return $"CPU - Processors: {ProcessorCount}, Total Time: {TotalProcessorTime:F0}ms, Threads: {Threads}";
    }
}
