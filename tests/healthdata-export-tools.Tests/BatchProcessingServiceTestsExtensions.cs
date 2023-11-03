#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===========================================================================

using FluentAssertions;
using HealthDataExportTools.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HealthDataExportTools.Tests;

public static class BatchProcessingServiceTestsExtensions
{
    /// <summary>
    /// Creates a new BatchProcessingService instance with mocked logger for testing extensions
    /// </summary>
    /// <param name="logger">Logger instance to inject</param>
    /// <returns>Configured BatchProcessingService instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when logger is null</exception>
    public static BatchProcessingService CreateBatchProcessingService(
        this BatchProcessingServiceTests _,
        ILogger<BatchProcessingService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        return new BatchProcessingService(logger);
    }

    /// <summary>
    /// Creates a batch processor that tracks processed items and can simulate errors
    /// </summary>
    /// <typeparam name="T">Type of items being processed</typeparam>
    /// <param name="itemsToFail">Items that should fail when processed</param>
    /// <param name="trackProcessedItems">Optional list to track successfully processed items</param>
    /// <returns>Batch processor function</returns>
    /// <exception cref="ArgumentNullException">Thrown when itemsToFail is null</exception>
    public static Func<List<T>, Task> CreateTrackingBatchProcessor<T>(
        this BatchProcessingServiceTests _,
        IEnumerable<T> itemsToFail,
        List<T>? trackProcessedItems = null) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(itemsToFail);

        var failedItems = itemsToFail.ToHashSet();

        return async batch =>
        {
            await Task.Delay(1).ConfigureAwait(false);

            var itemsToProcess = new List<T>();
            foreach (var item in batch)
            {
                if (failedItems.Contains(item))
                {
                    throw new Exception($"Simulated error for item: {item}");
                }
                itemsToProcess.Add(item);
            }

            trackProcessedItems?.AddRange(itemsToProcess);
        };
    }

    /// <summary>
    /// Creates a batch processor that tracks processing time for performance testing
    /// </summary>
    /// <typeparam name="T">Type of items being processed</typeparam>
    /// <param name="trackProcessingTimes">List to store processing times</param>
    /// <param name="delayMs">Optional delay per item in milliseconds</param>
    /// <returns>Batch processor function with timing</returns>
    /// <exception cref="ArgumentNullException">Thrown when trackProcessingTimes is null</exception>
    public static Func<List<T>, Task> CreateTimedBatchProcessor<T>(
        this BatchProcessingServiceTests _,
        List<TimeSpan> trackProcessingTimes,
        int delayMs = 1) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(trackProcessingTimes);

        return async batch =>
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Simulate variable processing time based on batch size
            var totalDelay = batch.Count * delayMs;
            await Task.Delay(totalDelay).ConfigureAwait(false);

            stopwatch.Stop();
            trackProcessingTimes.Add(stopwatch.Elapsed);
        };
    }

    /// <summary>
    /// Creates a progress callback that tracks progress updates
    /// </summary>
    /// <param name="trackProgressUpdates">List to store progress updates</param>
    /// <returns>Progress callback action</returns>
    /// <exception cref="ArgumentNullException">Thrown when trackProgressUpdates is null</exception>
    public static Action<BatchProgress> CreateProgressTracker(
        this BatchProcessingServiceTests _,
        List<BatchProgress> trackProgressUpdates)
    {
        ArgumentNullException.ThrowIfNull(trackProgressUpdates);

        return progress => trackProgressUpdates.Add(progress);
    }

    /// <summary>
    /// Verifies that all items in a collection were processed successfully
    /// </summary>
    /// <typeparam name="T">Type of items being processed</typeparam>
    /// <param name="processedItems">List of items that were processed</param>
    /// <param name="expectedItems">Expected items to be processed</param>
    /// <exception cref="ArgumentNullException">Thrown when processedItems or expectedItems is null</exception>
    public static void ShouldHaveProcessedAllItems<T>(
        this BatchProcessingServiceTests _,
        List<T> processedItems,
        List<T> expectedItems) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(processedItems);
        ArgumentNullException.ThrowIfNull(expectedItems);

        processedItems.Should().HaveCount(expectedItems.Count);
        processedItems.Should().BeEquivalentTo(expectedItems);
    }

    /// <summary>
    /// Creates a batch processor that processes items with a specific pattern
    /// </summary>
    /// <typeparam name="T">Type of items being processed</typeparam>
    /// <param name="processorAction">Action to apply to each batch</param>
    /// <returns>Batch processor function</returns>
    /// <exception cref="ArgumentNullException">Thrown when processorAction is null</exception>
    public static Func<List<T>, Task> CreateBatchProcessor<T>(
        this BatchProcessingServiceTests _,
        Func<List<T>, Task> processorAction) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(processorAction);
        return processorAction;
    }
}