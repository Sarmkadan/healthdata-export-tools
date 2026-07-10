# MetricsCollector

`MetricsCollector` tracks execution metrics for named operations, recording successes, failures, durations, and item counts. It provides per-operation statistics and aggregated summaries across all tracked operations.

## API

### Constructor

```csharp
public MetricsCollector(string operationName)
```

Creates a new collector for the specified operation. The `operationName` parameter identifies the operation being measured and cannot be null.

### Properties

#### `OperationName`
```csharp
public string OperationName { get; }
```
Returns the name of the operation this collector tracks.

#### `SuccessCount`
```csharp
public int SuccessCount { get; }
```
The number of successful executions recorded for this operation.

#### `FailureCount`
```csharp
public int FailureCount { get; }
```
The number of failed executions recorded for this operation.

#### `TotalDurationMs`
```csharp
public long TotalDurationMs { get; }
```
Cumulative duration in milliseconds across all recorded executions.

#### `TotalItemsProcessed`
```csharp
public long TotalItemsProcessed { get; }
```
Cumulative count of items processed across all recorded executions.

#### `MinDurationMs`
```csharp
public long MinDurationMs { get; }
```
The shortest execution duration recorded, in milliseconds. Returns `long.MaxValue` if no executions have been recorded.

#### `MaxDurationMs`
```csharp
public long MaxDurationMs { get; }
```
The longest execution duration recorded, in milliseconds. Returns `0` if no executions have been recorded.

#### `FirstExecutionTime`
```csharp
public DateTime FirstExecutionTime { get; }
```
The timestamp of the first recorded execution. Returns `DateTime.MinValue` if no executions have been recorded.

#### `LastExecutionTime`
```csharp
public DateTime LastExecutionTime { get; }
```
The timestamp of the most recent recorded execution. Returns `DateTime.MinValue` if no executions have been recorded.

#### `TotalOperations`
```csharp
public int TotalOperations { get; }
```
The total number of operations tracked across all collectors managed by this instance.

### Methods

#### `RecordSuccess`
```csharp
public void RecordSuccess(long durationMs, long itemsProcessed = 0)
```
Records a successful execution. Updates `SuccessCount`, `TotalDurationMs`, `TotalItemsProcessed`, `MinDurationMs`, `MaxDurationMs`, and timestamps. The `durationMs` parameter must be non-negative. The optional `itemsProcessed` parameter defaults to zero.

#### `RecordFailure`
```csharp
public void RecordFailure()
```
Records a failed execution. Increments `FailureCount` and updates `LastExecutionTime`. Does not affect duration or item-count statistics.

#### `IncrementSuccess`
```csharp
public void IncrementSuccess()
```
Increments `SuccessCount` by one without recording duration or item-count data. Updates `LastExecutionTime`. Useful for counting successes where timing is not relevant.

#### `IncrementFailure`
```csharp
public void IncrementFailure()
```
Increments `FailureCount` by one without recording duration data. Updates `LastExecutionTime`.

#### `GetMetrics`
```csharp
public OperationMetrics? GetMetrics(string operationName)
```
Returns the `OperationMetrics` snapshot for the named operation, or `null` if no such operation has been registered. The returned object is a point-in-time copy of the current metrics.

#### `GetAllMetrics`
```csharp
public List<OperationMetrics> GetAllMetrics()
```
Returns a list of `OperationMetrics` snapshots for all tracked operations. The list is a copy; modifications do not affect the collector.

#### `GetSummary`
```csharp
public MetricsSummary GetSummary()
```
Returns a `MetricsSummary` aggregating metrics across all tracked operations, including total successes, failures, duration, items processed, and operation count.

#### `Reset`
```csharp
public void Reset()
```
Resets all metrics for all tracked operations to their initial state. All counters, durations, and timestamps are cleared.

#### `ResetOperation`
```csharp
public void ResetOperation(string operationName)
```
Resets metrics for the specified operation only. If the operation does not exist, no action is taken and no exception is thrown.

## Usage

### Example 1: Timing a batch processing operation

```csharp
var collector = new MetricsCollector("BatchImport");

foreach (var batch in batches)
{
    var sw = Stopwatch.StartNew();
    try
    {
        ProcessBatch(batch);
        sw.Stop();
        collector.RecordSuccess(sw.ElapsedMilliseconds, batch.Items.Count);
    }
    catch (Exception)
    {
        sw.Stop();
        collector.RecordFailure();
    }
}

var metrics = collector.GetMetrics("BatchImport");
Console.WriteLine($"Successes: {metrics.SuccessCount}, Failures: {metrics.FailureCount}");
Console.WriteLine($"Avg duration: {metrics.TotalDurationMs / Math.Max(1, metrics.SuccessCount)} ms");
```

### Example 2: Aggregating metrics across multiple operations

```csharp
var collector = new MetricsCollector("DataExport");

// Track export phases
collector.RecordSuccess(150, 1000);   // Phase 1
collector.RecordSuccess(200, 800);    // Phase 2
collector.IncrementFailure();         // Phase 3 failed without timing

// Also track a separate operation
var otherCollector = new MetricsCollector("IndexRebuild");
otherCollector.RecordSuccess(3000, 50000);

var summary = collector.GetSummary();
Console.WriteLine($"Total operations: {summary.TotalOperations}");
Console.WriteLine($"Overall success rate: {(double)summary.SuccessCount / summary.TotalExecutions:P}");

// Reset only the current operation
collector.ResetOperation("DataExport");
```

## Notes

- Properties such as `MinDurationMs` and `MaxDurationMs` return sentinel values (`long.MaxValue` and `0` respectively) when no executions have been recorded. Callers should check `SuccessCount > 0` before interpreting these values.
- `FirstExecutionTime` and `LastExecutionTime` return `DateTime.MinValue` when no executions exist. Use `DateTime.MinValue` checks to guard against displaying uninitialized timestamps.
- `RecordSuccess` expects a non-negative `durationMs`. Negative values will produce incorrect minimum-duration tracking.
- `IncrementSuccess` and `IncrementFailure` bypass duration and item-count statistics entirely. They are suitable for counting outcomes where measurement overhead is undesirable or timing data is unavailable.
- `GetMetrics` returns `null` for unregistered operation names. Callers must null-check the result.
- Thread safety is not guaranteed by the signatures. If multiple threads concurrently call `RecordSuccess`, `RecordFailure`, `IncrementSuccess`, `IncrementFailure`, or reset methods on the same collector instance, external synchronization is required to avoid race conditions on counters and duration accumulators.
- `GetMetrics`, `GetAllMetrics`, and `GetSummary` return snapshot copies. The returned objects do not reflect subsequent updates to the collector.
- `ResetOperation` silently ignores unknown operation names rather than throwing, allowing safe cleanup without pre-checking registration.
