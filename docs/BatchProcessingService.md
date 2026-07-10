# BatchProcessingService
The `BatchProcessingService` class is designed to facilitate the processing of large datasets in batches, providing a flexible and efficient way to handle data processing tasks. It offers both synchronous and asynchronous methods for processing data in batches, allowing for customization and control over the processing workflow.

## API
### Constructors
* `public BatchProcessingService`: Initializes a new instance of the `BatchProcessingService` class.

### Methods
* `public async Task<BatchProcessingResult> ProcessInBatchesAsync<T>`: Processes a dataset of type `T` in batches asynchronously. Returns a `BatchProcessingResult` object containing information about the processing outcome.
* `public async Task<BatchProcessingResult> ProcessInParallelBatchesAsync<T>`: Processes a dataset of type `T` in parallel batches asynchronously. Returns a `BatchProcessingResult` object containing information about the processing outcome.
* `public List<List<T>> PartitionIntoBatches<T>`: Partitions a dataset of type `T` into batches. Returns a list of lists, where each inner list represents a batch.

### Properties
* `public int TotalItems`: Gets the total number of items to be processed.
* `public int ProcessedItems`: Gets the number of items that have been successfully processed.
* `public int FailedItems`: Gets the number of items that failed processing.
* `public DateTime StartTime`: Gets the start time of the processing operation.
* `public DateTime EndTime`: Gets the end time of the processing operation.
* `public bool IsSuccessful`: Gets a boolean indicating whether the processing operation was successful.
* `public List<string> Errors`: Gets a list of error messages encountered during processing.
* `public TimeSpan GetDuration`: Gets the duration of the processing operation.
* `public double GetSuccessRate`: Gets the success rate of the processing operation as a percentage.
* `public double GetThroughput`: Gets the throughput of the processing operation.
* `public int CurrentBatch`: Gets the current batch being processed.
* `public int TotalBatches`: Gets the total number of batches.
* `public int PercentComplete`: Gets the percentage of completion of the processing operation.

### Override
* `public override string ToString`: Returns a string representation of the `BatchProcessingService` instance.

## Usage
The following examples demonstrate how to use the `BatchProcessingService` class to process datasets in batches:
```csharp
// Example 1: Processing a dataset in batches asynchronously
var service = new BatchProcessingService();
var dataset = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
var result = await service.ProcessInBatchesAsync(dataset);
Console.WriteLine($"Processed {result.ProcessedItems} items successfully.");

// Example 2: Processing a dataset in parallel batches asynchronously
var service = new BatchProcessingService();
var dataset = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
var result = await service.ProcessInParallelBatchesAsync(dataset);
Console.WriteLine($"Processed {result.ProcessedItems} items successfully in parallel.");
```

## Notes
When using the `BatchProcessingService` class, consider the following edge cases and thread-safety remarks:
* The `ProcessInBatchesAsync` and `ProcessInParallelBatchesAsync` methods are asynchronous and may throw exceptions if the processing operation fails.
* The `PartitionIntoBatches` method may return an empty list if the input dataset is empty.
* The `GetDuration`, `GetSuccessRate`, and `GetThroughput` properties may return default values if the processing operation has not completed.
* The `BatchProcessingService` class is not thread-safe by default. If you need to use it in a multi-threaded environment, consider implementing synchronization mechanisms to ensure thread safety.
