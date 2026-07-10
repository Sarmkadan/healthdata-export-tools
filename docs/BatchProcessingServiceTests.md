# BatchProcessingServiceTests

Unit test suite for the `BatchProcessingService` class, verifying batch processing behavior across sequential and parallel execution modes. The tests cover successful item processing, error handling within batch processors, progress callback invocation, empty input handling, and the correctness of the `PartitionIntoBatches` helper method.

## API

### BatchProcessingServiceTests

```csharp
public BatchProcessingServiceTests()
```

Default parameterless constructor. Initializes the test class and any shared test infrastructure required by the individual test methods. Does not throw.

---

### ProcessInBatchesAsync_ShouldProcessAllItemsSuccessfully

```csharp
public async Task ProcessInBatchesAsync_ShouldProcessAllItemsSuccessfully()
```

**Purpose:** Verifies that `ProcessInBatchesAsync` processes every item in the input collection when the batch processor completes without errors.

**Parameters:** None (parameterless test method).

**Return Value:** A `Task` representing the asynchronous test operation.

**Throws:** Assertion failures if any item is skipped or the final result does not match the expected count of processed items.

---

### ProcessInBatchesAsync_ShouldHandleErrorsInBatchProcessor

```csharp
public async Task ProcessInBatchesAsync_ShouldHandleErrorsInBatchProcessor()
```

**Purpose:** Confirms that `ProcessInBatchesAsync` gracefully handles exceptions thrown by the batch processor delegate, without crashing or losing track of successfully processed items from other batches.

**Parameters:** None.

**Return Value:** A `Task` representing the asynchronous test operation.

**Throws:** Assertion failures if the method propagates an unhandled exception or reports an incorrect success count.

---

### ProcessInBatchesAsync_ShouldInvokeProgressCallback

```csharp
public async Task ProcessInBatchesAsync_ShouldInvokeProgressCallback()
```

**Purpose:** Ensures that the progress callback supplied to `ProcessInBatchesAsync` is invoked at the expected intervals (typically once per completed batch) with accurate progress values.

**Parameters:** None.

**Return Value:** A `Task` representing the asynchronous test operation.

**Throws:** Assertion failures if the callback is never called, called an incorrect number of times, or receives invalid progress arguments.

---

### ProcessInBatchesAsync_ShouldReturnZeroForEmptyList

```csharp
public async Task ProcessInBatchesAsync_ShouldReturnZeroForEmptyList()
```

**Purpose:** Validates that `ProcessInBatchesAsync` returns zero when given an empty item collection, without invoking the batch processor or progress callback unnecessarily.

**Parameters:** None.

**Return Value:** A `Task` representing the asynchronous test operation.

**Throws:** Assertion failures if the return value is non-zero or if side effects (processor invocation) occur.

---

### ProcessInParallelBatchesAsync_ShouldProcessAllItemsSuccessfully

```csharp
public async Task ProcessInParallelBatchesAsync_ShouldProcessAllItemsSuccessfully()
```

**Purpose:** Verifies that `ProcessInParallelBatchesAsync` processes every item in the input collection when executing batches concurrently, with no items lost due to parallel access patterns.

**Parameters:** None.

**Return Value:** A `Task` representing the asynchronous test operation.

**Throws:** Assertion failures if the total processed count does not match the input size.

---

### ProcessInParallelBatchesAsync_ShouldHandleErrorsInParallelBatches

```csharp
public async Task ProcessInParallelBatchesAsync_ShouldHandleErrorsInParallelBatches()
```

**Purpose:** Confirms that `ProcessInParallelBatchesAsync` correctly handles exceptions thrown within individual parallel batch executions, aggregating errors without disrupting other concurrent batches.

**Parameters:** None.

**Return Value:** A `Task` representing the asynchronous test operation.

**Throws:** Assertion failures if an unhandled exception escapes the method or if the error aggregation logic is incorrect.

---

### PartitionIntoBatches_ShouldPartitionCorrectly

```csharp
public void PartitionIntoBatches_ShouldPartitionCorrectly()
```

**Purpose:** Tests the `PartitionIntoBatches` helper method to ensure it splits a collection into batches of the specified size, with the final batch containing the remainder when the total count is not evenly divisible.

**Parameters:** None.

**Return Value:** `void`.

**Throws:** Assertion failures if batch sizes or item distribution do not match expectations.

---

### PartitionIntoBatches_ShouldHandleEmptyList

```csharp
public void PartitionIntoBatches_ShouldHandleEmptyList()
```

**Purpose:** Verifies that `PartitionIntoBatches` returns an empty partition result when given an empty source collection, without throwing or producing a single empty batch.

**Parameters:** None.

**Return Value:** `void`.

**Throws:** Assertion failures if the result is non-empty or an exception is thrown.

## Usage

### Example 1: Testing Sequential Batch Processing with Error Injection

```csharp
[TestClass]
public class BatchProcessingServiceTests
{
    private BatchProcessingService _service;

    [TestInitialize]
    public void Setup()
    {
        _service = new BatchProcessingService();
    }

    [TestMethod]
    public async Task MySequentialBatchTest_WithPartialFailures()
    {
        // Arrange
        var items = Enumerable.Range(1, 25).ToList();
        var processed = new ConcurrentBag<int>();
        var errors = new List<Exception>();

        Func<List<int>, Task> batchProcessor = async batch =>
        {
            await Task.Delay(10);
            if (batch.Contains(13))
                throw new InvalidOperationException("Simulated failure");
            foreach (var item in batch)
                processed.Add(item);
        };

        // Act
        var result = await _service.ProcessInBatchesAsync(
            items, batchSize: 5, batchProcessor, errors);

        // Assert
        Assert.AreEqual(24, result); // 25 minus the failing item
        Assert.AreEqual(1, errors.Count);
    }
}
```

### Example 2: Testing Parallel Batch Processing with Progress Tracking

```csharp
[TestMethod]
public async Task MyParallelBatchTest_WithProgressCallback()
{
    // Arrange
    var items = Enumerable.Range(1, 100).ToList();
    var progressValues = new List<int>();
    var processedCount = 0;

    Func<List<int>, Task> batchProcessor = async batch =>
    {
        await Task.Delay(5);
        Interlocked.Add(ref processedCount, batch.Count);
    };

    Action<int> progressCallback = progress =>
    {
        lock (progressValues)
            progressValues.Add(progress);
    };

    // Act
    var result = await _service.ProcessInParallelBatchesAsync(
        items, batchSize: 10, batchProcessor, progressCallback);

    // Assert
    Assert.AreEqual(100, result);
    Assert.AreEqual(100, processedCount);
    Assert.IsTrue(progressValues.Count >= 1);
    Assert.AreEqual(100, progressValues.Last());
}
```

## Notes

- **Empty Collections:** Both `ProcessInBatchesAsync` and `ProcessInParallelBatchesAsync` must return zero immediately for empty inputs. `PartitionIntoBatches` must return an empty enumerable, not a collection containing a single empty batch.
- **Batch Size Edge Cases:** When the total item count is less than the batch size, `PartitionIntoBatches` should produce exactly one batch containing all items. When the count is an exact multiple, all batches should be of equal size.
- **Error Aggregation:** In error-handling tests, exceptions thrown by the batch processor must be captured and surfaced through the designated error collection parameter. The method under test must not throw them directly, and other batches must continue processing.
- **Progress Callback Semantics:** The progress callback should report cumulative progress (total items processed so far), not per-batch counts. The final invocation must equal the total number of items successfully processed.
- **Thread Safety:** Tests for `ProcessInParallelBatchesAsync` implicitly validate that the underlying implementation uses appropriate synchronization or concurrent-safe data structures. Test assertions that rely on shared state (e.g., `ConcurrentBag`, `Interlocked`) must be structured to avoid false positives from race conditions in the test itself.
- **Test Isolation:** Each test method is independent and should not rely on state persisted from other tests. Setup and teardown logic, if present, ensures a clean `BatchProcessingService` instance per test.
