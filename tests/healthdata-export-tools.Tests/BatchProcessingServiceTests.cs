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

public sealed class BatchProcessingServiceTests
{
    private readonly ILogger<BatchProcessingService> _mockLogger;
    private readonly BatchProcessingService _batchProcessingService;

    public BatchProcessingServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<BatchProcessingService>>();
        _batchProcessingService = new BatchProcessingService(_mockLogger);
    }

    [Fact]
    public async Task ProcessInBatchesAsync_ShouldProcessAllItemsSuccessfully()
    {
        // Arrange
        var processedItems = new List<int>();
        var itemsToProcess = Enumerable.Range(1, 100).ToList();
        var batchProcessor = new Func<List<int>, Task>(async batch =>
        {
            await Task.Delay(1); // Simulate async work
            lock (processedItems)
            {
                processedItems.AddRange(batch);
            }
        });

        // Act
        var result = await _batchProcessingService.ProcessInBatchesAsync(itemsToProcess, batchProcessor, batchSize: 10);

        // Assert
        result.TotalItems.Should().Be(100);
        result.ProcessedItems.Should().Be(100);
        result.FailedItems.Should().Be(0);
        result.IsSuccessful.Should().BeTrue();
        processedItems.Should().HaveCount(100);
        processedItems.Should().BeEquivalentTo(itemsToProcess);
        _mockLogger.Received(1).LogInformation(
            Arg.Is<string>(s => s.Contains("Starting batch processing")), itemsToProcess.Count, 10);
        _mockLogger.Received(1).LogInformation(
            Arg.Is<string>(s => s.Contains("Batch processing completed")), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<double>());
    }

    [Fact]
    public async Task ProcessInBatchesAsync_ShouldHandleErrorsInBatchProcessor()
    {
        // Arrange
        var processedItems = new List<int>();
        var itemsToProcess = Enumerable.Range(1, 10).ToList();
        var batchProcessor = new Func<List<int>, Task>(async batch =>
        {
            await Task.Delay(1); // Simulate async work
            if (batch.Contains(5))
            {
                throw new Exception("Error processing item 5");
            }

            processedItems.AddRange(batch);
        });

        // Act
        var result = await _batchProcessingService.ProcessInBatchesAsync(itemsToProcess, batchProcessor, batchSize: 2);

        // Assert
        result.TotalItems.Should().Be(10);
        // Depending on how items are processed, if an error in batch 5 (items 5,6) happens, 
        // 4 items (1,2,3,4) would be processed successfully. Then items 5,6 fail.
        // Then items 7,8 are processed. Then 9,10 are processed.
        result.ProcessedItems.Should().Be(8); // 1-4, 7-10
        result.FailedItems.Should().Be(2); // 5,6
        result.IsSuccessful.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Contain("Error processing item 5");
        processedItems.Should().HaveCount(8);
        processedItems.Should().NotContain(5).And.NotContain(6);
        _mockLogger.Received(1).LogError(
            Arg.Any<Exception>(), Arg.Is<string>(s => s.Contains("Error processing batch")), Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public async Task ProcessInBatchesAsync_ShouldInvokeProgressCallback()
    {
        // Arrange
        var itemsToProcess = Enumerable.Range(1, 10).ToList();
        var progressUpdates = new List<BatchProgress>();
        var batchProcessor = new Func<List<int>, Task>(async batch =>
        {
            await Task.Delay(1);
        });
        var progressCallback = new Action<BatchProgress>(progress =>
        {
            progressUpdates.Add(progress);
        });

        // Act
        await _batchProcessingService.ProcessInBatchesAsync(itemsToProcess, batchProcessor, batchSize: 2, progressCallback);

        // Assert
        progressUpdates.Should().HaveCount(5); // 10 items, batch size 2, so 5 batches
        progressUpdates.Last().PercentComplete.Should().Be(100);
        progressUpdates.First().CurrentBatch.Should().Be(1);
    }

    [Fact]
    public async Task ProcessInBatchesAsync_ShouldReturnZeroForEmptyList()
    {
        // Arrange
        var itemsToProcess = new List<string>();
        var batchProcessor = new Func<List<string>, Task>(batch => Task.CompletedTask);

        // Act
        var result = await _batchProcessingService.ProcessInBatchesAsync(itemsToProcess, batchProcessor);

        // Assert
        result.TotalItems.Should().Be(0);
        result.ProcessedItems.Should().Be(0);
        result.FailedItems.Should().Be(0);
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessInParallelBatchesAsync_ShouldProcessAllItemsSuccessfully()
    {
        // Arrange
        var processedItems = new List<int>();
        var itemsToProcess = Enumerable.Range(1, 100).ToList();
        var batchProcessor = new Func<List<int>, Task>(async batch =>
        {
            await Task.Delay(1); // Simulate async work
            lock (processedItems)
            {
                processedItems.AddRange(batch);
            }
        });

        // Act
        var result = await _batchProcessingService.ProcessInParallelBatchesAsync(itemsToProcess, batchProcessor, batchSize: 10, maxParallelism: 4);

        // Assert
        result.TotalItems.Should().Be(100);
        result.ProcessedItems.Should().Be(100);
        result.FailedItems.Should().Be(0);
        result.IsSuccessful.Should().BeTrue();
        processedItems.Should().HaveCount(100);
        processedItems.Should().BeEquivalentTo(itemsToProcess);
        _mockLogger.Received(1).LogInformation(
            Arg.Is<string>(s => s.Contains("Starting parallel batch processing")), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public async Task ProcessInParallelBatchesAsync_ShouldHandleErrorsInParallelBatches()
    {
        // Arrange
        var processedItems = new List<int>();
        var itemsToProcess = Enumerable.Range(1, 10).ToList();
        var batchProcessor = new Func<List<int>, Task>(async batch =>
        {
            await Task.Delay(1); // Simulate async work
            if (batch.Contains(5) || batch.Contains(6))
            {
                throw new Exception($"Error processing items in batch containing {batch.First()}");
            }
            lock (processedItems)
            {
                processedItems.AddRange(batch);
            }
        });

        // Act
        var result = await _batchProcessingService.ProcessInParallelBatchesAsync(itemsToProcess, batchProcessor, batchSize: 2, maxParallelism: 2);

        // Assert
        result.TotalItems.Should().Be(10);
        result.ProcessedItems.Should().Be(8); // Items 1-4, 7-10 should pass
        result.FailedItems.Should().Be(2); // Items 5,6 in one batch should fail
        result.IsSuccessful.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().ContainMatch("Error processing items in batch containing 5");
        processedItems.Should().HaveCount(8);
        processedItems.Should().NotContain(5).And.NotContain(6);
        _mockLogger.Received(1).LogError(
            Arg.Any<Exception>(), Arg.Is<string>(s => s.Contains("Error in parallel batch processing")));
    }

    [Fact]
    public void PartitionIntoBatches_ShouldPartitionCorrectly()
    {
        // Arrange
        var items = Enumerable.Range(1, 10).ToList();

        // Act
        var batches = _batchProcessingService.PartitionIntoBatches(items, 3);

        // Assert
        batches.Should().HaveCount(4); // 3, 3, 3, 1
        batches[0].Should().ContainInOrder(1, 2, 3);
        batches[1].Should().ContainInOrder(4, 5, 6);
        batches[2].Should().ContainInOrder(7, 8, 9);
        batches[3].Should().ContainInOrder(10);
    }

    [Fact]
    public void PartitionIntoBatches_ShouldHandleEmptyList()
    {
        // Arrange
        var items = new List<int>();

        // Act
        var batches = _batchProcessingService.PartitionIntoBatches(items, 5);

        // Assert
        batches.Should().BeEmpty();
    }
}
