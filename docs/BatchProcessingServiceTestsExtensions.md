# BatchProcessingServiceTestsExtensions

The `BatchProcessingServiceTestsExtensions` static class provides factory methods and assertion helpers designed to simplify unit testing of batch processing workflows. It enables the creation of mock or instrumented `BatchProcessingService` instances, batch processor delegates that track or time item processing, progress trackers, and a dedicated assertion method to verify that all items were processed. These utilities are intended for use in test projects where the behavior of batch processing components must be verified without relying on production implementations.

## API

### `public static BatchProcessingService CreateBatchProcessingService`

Creates a default test instance of `BatchProcessingService`. The returned service is pre-configured with no custom batch processor or progress tracker; these can be supplied later via the service's properties or constructor if needed.

- **Parameters**: None.
- **Returns**: A new `BatchProcessingService` instance suitable for testing.
- **Throws**: Never throws.

### `public static Func<List<T>, Task> CreateTrackingBatchProcessor<T>()`

Creates a batch processor delegate that records each invocation and the items it receives. The recorded data can later be inspected or used by `ShouldHaveProcessedAllItems<T>()`.

- **Type parameters**: `T` – the type of items in the batch.
- **Parameters**: None.
- **Returns**: A `Func<List<T>, Task>` that, when invoked, stores the provided list for subsequent assertions.
- **Throws**: Never throws.

### `public static Func<List<T>, Task> CreateTimedBatchProcessor<T>()`

Creates a batch processor delegate that simulates a fixed processing delay (typically a small, constant duration) before returning. This is useful for testing timeout or concurrency scenarios.

- **Type parameters**: `T` – the type of items in the batch.
- **Parameters**: None.
- **Returns**: A `Func<List<T>, Task>` that introduces a delay and then completes.
- **Throws**: Never throws.

### `public static Action<BatchProgress> CreateProgressTracker()`

Creates an action delegate that records progress updates reported by a `BatchProcessingService`. The captured progress data can be examined after processing completes.

- **Parameters**: None.
- **Returns**: An `Action<BatchProgress>` that stores each progress report internally.
- **Throws**: Never throws.

### `public static void ShouldHaveProcessedAllItems<T>()`

Asserts that every item submitted to a previously created tracking batch processor has been processed. This method relies on internal state accumulated by `CreateTrackingBatchProcessor<T>()` and must be called after the batch processing has finished.

- **Type parameters**: `T` – the type of items that were processed.
- **Parameters**: None.
- **Returns**: Nothing.
- **Throws**: `AssertionException` (or equivalent test framework exception) if any items remain unprocessed or if no tracking processor was used.

### `public static Func<List<T>, Task> CreateCustomBatchProcessor<T>()`

Creates a batch processor delegate that can be customized by the test author. The returned delegate initially performs no operation; its behavior can be replaced or augmented by assigning a new delegate to the returned reference before it is used.

- **Type parameters**: `T` – the type of items in the batch.
- **Parameters**: None.
- **Returns**: A `Func<List<T>, Task>` that, by default, does nothing and returns a completed task.
- **Throws**: Never throws.

## Usage

### Example 1: Verifying that all items are processed

```csharp
[Fact]
public async Task ProcessAllItems_ShouldProcessAll()
{
    // Arrange
    var service = BatchProcessingServiceTestsExtensions.CreateBatchProcessingService();
    var processor = BatchProcessingServiceTestsExtensions.CreateTrackingBatchProcessor<int>();
    service.BatchProcessor = processor;
    var items = new List<int> { 1, 2, 3 };

    // Act
    await service.ProcessBatchAsync(items);

    // Assert
    BatchProcessingServiceTestsExtensions.ShouldHaveProcessedAllItems<int>();
}
```

### Example 2: Testing progress reporting with a timed processor

```csharp
[Fact]
public async Task ProgressTracker_ReceivesUpdates()
{
    // Arrange
    var service = BatchProcessingServiceTestsExtensions.CreateBatchProcessingService();
    service.BatchProcessor = BatchProcessingServiceTestsExtensions.CreateTimedBatchProcessor<string>();
    var progressTracker = BatchProcessingServiceTestsExtensions.CreateProgressTracker();
    service.ProgressReporter = progressTracker;
    var items = new List<string> { "A", "B" };

    // Act
    await service.ProcessBatchAsync(items);

    // Assert
    // (progressTracker now contains the recorded progress reports)
    Assert.NotEmpty(progressTracker.Reports); // hypothetical property – actual API may differ
}
```

## Notes

- **Stateful helpers**: `CreateTrackingBatchProcessor<T>()` and `CreateProgressTracker()` maintain internal state. Each call creates a new, independent tracker. `ShouldHaveProcessedAllItems<T>()` uses the most recently created tracking processor; if multiple tracking processors are created, only the last one is considered. To avoid confusion, create only one tracking processor per test.
- **Empty lists**: All batch processor delegates handle an empty input list gracefully – they return a completed task without error. `ShouldHaveProcessedAllItems<T>()` will pass if no items were submitted.
- **Null items**: The delegates do not perform null checks on the input list. Passing `null` will result in a `NullReferenceException` at invocation time.
- **Thread safety**: The tracking and progress delegates are not thread-safe. They are intended for single-threaded test execution. Concurrent invocations may produce inconsistent recorded state.
- **Timed processor delay**: The delay introduced by `CreateTimedBatchProcessor<T>()` is fixed and typically short (e.g., 10 milliseconds). It is not configurable through the public API.
- **Custom processor**: The delegate returned by `CreateCustomBatchProcessor<T>()` is a mutable reference. Reassigning it after it has been passed to a `BatchProcessingService` will not affect the service; the service holds its own copy of the delegate.
