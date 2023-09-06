#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    public static BatchProcessingService CreateBatchProcessingService(this BatchProcessingServiceTests _)
    {
        var mockLogger = Substitute.For<ILogger<BatchProcessingService>>();
        return new BatchProcessingService(mockLogger);
    }

    /// <summary>
    /// Creates a batch processor that tracks processed items and can simulate errors
    /// </summary>
    /// <param name="itemsToFail">Items that should fail when processed</param>
    /// <param name="trackProcessedItems">Optional list to track processed items</param>
    public static Func<List<T>, Task> CreateTrackingBatchProcessor<T>(
        this BatchProcessingServiceTests _,
        IEnumerable<T> itemsToFail,
        List<T>? trackProcessedItems = null) where T : notnull
    {
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
    /// <param name="trackProcessingTimes">List to store processing times</param>
    /// <param name="delayMs">Optional delay per item in milliseconds</param>
    public static Func<List<T>, Task> CreateTimedBatchProcessor<T>(
        this BatchProcessingServiceTests _,
        List<TimeSpan> trackProcessingTimes,
        int delayMs = 1) where T : notnull
    {
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
    public static Action<BatchProgress> CreateProgressTracker(this BatchProcessingServiceTests _,
        List<BatchProgress> trackProgressUpdates)
    {
        return progress =>
        {
            trackProgressUpdates.Add(progress);
        };
    }

    /// <summary>
    /// Verifies that all items in a collection were processed successfully
    /// </summary>
    /// <param name="processedItems">List of items that were processed</param>
    /// <param name="expectedItems">Expected items to be processed</param>
    public static void ShouldHaveProcessedAllItems<T>(
        this BatchProcessingServiceTests _,
        List<T> processedItems,
        List<T> expectedItems) where T : notnull
    {
        processedItems.Should().HaveCount(expectedItems.Count);
        processedItems.Should().BeEquivalentTo(expectedItems);
    }

    /// <summary>
    /// Creates a batch processor that processes items with a specific pattern
    /// </summary>
    /// <param name="processorAction">Action to apply to each batch</param>
    public static Func<List<T>, Task> CreateCustomBatchProcessor<T>(
        this BatchProcessingServiceTests _,
        Func<List<T>, Task> processorAction) where T : notnull
    {
        return processorAction;
    }
}