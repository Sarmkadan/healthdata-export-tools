// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Services;

/// <summary>
/// Service for batch processing of health data with progress tracking
/// Handles large datasets efficiently with configurable batch sizes
/// </summary>
public class BatchProcessingService
{
    private readonly ILogger<BatchProcessingService> _logger;

    public BatchProcessingService(ILogger<BatchProcessingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Process items in batches with progress callback
    /// </summary>
    public async Task<BatchProcessingResult> ProcessInBatchesAsync<T>(
        List<T> items,
        Func<List<T>, Task> batchProcessor,
        int batchSize = 1000,
        Action<BatchProgress>? progressCallback = null)
    {
        if (items == null || items.Count == 0)
        {
            return new BatchProcessingResult { TotalItems = 0, ProcessedItems = 0, FailedItems = 0 };
        }

        var result = new BatchProcessingResult
        {
            TotalItems = items.Count,
            StartTime = DateTime.UtcNow
        };

        _logger.LogInformation("Starting batch processing: {Total} items in batches of {Size}",
            items.Count, batchSize);

        int batchCount = (int)Math.Ceiling((double)items.Count / batchSize);

        for (int i = 0; i < batchCount; i++)
        {
            try
            {
                var batch = items.Skip(i * batchSize).Take(batchSize).ToList();

                _logger.LogDebug("Processing batch {Current}/{Total} ({Count} items)",
                    i + 1, batchCount, batch.Count);

                await batchProcessor(batch);

                result.ProcessedItems += batch.Count;

                // Call progress callback
                progressCallback?.Invoke(new BatchProgress
                {
                    CurrentBatch = i + 1,
                    TotalBatches = batchCount,
                    ProcessedItems = result.ProcessedItems,
                    TotalItems = items.Count,
                    PercentComplete = (result.ProcessedItems * 100) / items.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch {Batch}/{Total}",
                    i + 1, batchCount);

                result.FailedItems += Math.Min(batchSize, items.Count - (i * batchSize));
                result.Errors.Add(ex.Message);
            }
        }

        result.EndTime = DateTime.UtcNow;
        result.IsSuccessful = result.FailedItems == 0;

        _logger.LogInformation(
            "Batch processing completed: {Processed}/{Total} items processed ({Percent}% success rate)",
            result.ProcessedItems, result.TotalItems, result.GetSuccessRate());

        return result;
    }

    /// <summary>
    /// Process items in parallel batches
    /// </summary>
    public async Task<BatchProcessingResult> ProcessInParallelBatchesAsync<T>(
        List<T> items,
        Func<List<T>, Task> batchProcessor,
        int batchSize = 1000,
        int maxParallelism = 4)
    {
        if (items == null || items.Count == 0)
        {
            return new BatchProcessingResult { TotalItems = 0, ProcessedItems = 0, FailedItems = 0 };
        }

        var result = new BatchProcessingResult
        {
            TotalItems = items.Count,
            StartTime = DateTime.UtcNow
        };

        int batchCount = (int)Math.Ceiling((double)items.Count / batchSize);
        var batches = new List<List<T>>();

        for (int i = 0; i < batchCount; i++)
        {
            batches.Add(items.Skip(i * batchSize).Take(batchSize).ToList());
        }

        _logger.LogInformation(
            "Starting parallel batch processing: {Total} items in {Batches} batches with {Parallelism} parallelism",
            items.Count, batchCount, maxParallelism);

        var options = new ParallelOptions { MaxDegreeOfParallelism = maxParallelism };
        var lockObj = new object();

        try
        {
            await Parallel.ForEachAsync(batches, options, async (batch, ct) =>
            {
                try
                {
                    await batchProcessor(batch);

                    lock (lockObj)
                    {
                        result.ProcessedItems += batch.Count;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in parallel batch processing");

                    lock (lockObj)
                    {
                        result.FailedItems += batch.Count;
                        result.Errors.Add(ex.Message);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in parallel batch processing");
            result.Errors.Add($"Fatal error: {ex.Message}");
        }

        result.EndTime = DateTime.UtcNow;
        result.IsSuccessful = result.FailedItems == 0;

        return result;
    }

    /// <summary>
    /// Partition items into batches
    /// </summary>
    public List<List<T>> PartitionIntoBatches<T>(List<T> items, int batchSize)
    {
        var batches = new List<List<T>>();

        for (int i = 0; i < items.Count; i += batchSize)
        {
            batches.Add(items.Skip(i).Take(batchSize).ToList());
        }

        return batches;
    }
}

/// <summary>
/// Result of batch processing
/// </summary>
public class BatchProcessingResult
{
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public int FailedItems { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsSuccessful { get; set; }
    public List<string> Errors { get; set; } = new();

    public TimeSpan GetDuration() => EndTime - StartTime;
    public double GetSuccessRate() => TotalItems > 0 ? (ProcessedItems * 100.0) / TotalItems : 0;
    public double GetThroughput() => GetDuration().TotalSeconds > 0
        ? ProcessedItems / GetDuration().TotalSeconds
        : 0;
}

/// <summary>
/// Batch processing progress information
/// </summary>
public class BatchProgress
{
    public int CurrentBatch { get; set; }
    public int TotalBatches { get; set; }
    public int ProcessedItems { get; set; }
    public int TotalItems { get; set; }
    public int PercentComplete { get; set; }

    public override string ToString()
    {
        return $"Batch {CurrentBatch}/{TotalBatches}: {ProcessedItems}/{TotalItems} items ({PercentComplete}%)";
    }
}
