# ExportCompletedEvent

The `ExportCompletedEvent` class encapsulates the final state and metrics of a data export operation within the `healthdata-export-tools` project. It serves as an immutable result object that provides comprehensive details regarding the export format, volume of data processed, physical output characteristics, execution timeline, and any non-fatal warnings encountered during the process. This type is typically returned upon the successful completion of an export job to facilitate logging, auditing, and post-processing validation.

## API

### Properties

#### `ExportFormat`
```csharp
public ExportFormat ExportFormat { get; }
```
Gets the specific format used for the exported data (e.g., CSV, JSON, XML). This value reflects the configuration applied at the start of the export process.

#### `RecordsExported`
```csharp
public int RecordsExported { get; }
```
Gets the total count of individual data records successfully written to the output. This number represents the logical rows processed, regardless of file splitting or compression.

#### `OutputPath`
```csharp
public string OutputPath { get; }
```
Gets the primary file system path where the export result was saved. If the export generated multiple files, this typically points to the root directory or the primary manifest file.

#### `OutputSizeBytes`
```csharp
public long OutputSizeBytes { get; }
```
Gets the total size of the generated output on disk in bytes. This value accounts for all generated files and includes compression overhead if applicable.

#### `ExportStartTime`
```csharp
public DateTime ExportStartTime { get; }
```
Gets the precise timestamp marking the initiation of the export process.

#### `ExportEndTime`
```csharp
public DateTime ExportEndTime { get; }
```
Gets the precise timestamp marking the completion of the export process.

#### `WasCompressed`
```csharp
public bool WasCompressed { get; }
```
Gets a value indicating whether the output files were compressed (e.g., gzipped) during the export.

#### `GeneratedFiles`
```csharp
public List<string> GeneratedFiles { get; }
```
Gets a list of full file paths for every file created during the export operation. This list includes split files, compression artifacts, and metadata files.

#### `Warnings`
```csharp
public List<string> Warnings { get; }
```
Gets a collection of non-fatal warning messages encountered during the export. These may indicate skipped records, data truncation, or schema mismatches that did not halt the operation.

### Constructors

#### `ExportCompletedEvent`
```csharp
public ExportCompletedEvent(...)
```
Initializes a new instance of the `ExportCompletedEvent` class. This constructor accepts parameters corresponding to the public properties to populate the event state upon completion of an export task.

### Methods

#### `GetExportDuration`
```csharp
public TimeSpan GetExportDuration()
```
Calculates and returns the total time elapsed between `ExportStartTime` and `ExportEndTime`.
*   **Returns**: A `TimeSpan` representing the duration of the export.
*   **Throws**: None.

#### `GetThroughput`
```csharp
public double GetThroughput()
```
Calculates the average processing throughput of the export operation in records per second.
*   **Returns**: A `double` representing the number of records exported per second. If the duration is zero, this method returns `double.PositiveInfinity` or `0` depending on whether records were exported.
*   **Throws**: None.

#### `GetHumanReadableSize`
```csharp
public string GetHumanReadableSize()
```
Converts the `OutputSizeBytes` value into a human-readable string format (e.g., "15.4 MB", "2.1 GB").
*   **Returns**: A formatted `string` representing the file size with appropriate units.
*   **Throws**: None.

#### `ToString`
```csharp
public override string ToString()
```
Returns a string representation of the event, typically summarizing key metrics such as record count, format, and output path.
*   **Returns**: A `string` containing the summary of the export event.
*   **Throws**: None.

## Usage

### Example 1: Logging Export Metrics
The following example demonstrates how to consume an `ExportCompletedEvent` instance to log high-level statistics and verify the success of an operation.

```csharp
public void LogExportSummary(ExportCompletedEvent exportEvent)
{
    if (exportEvent.Warnings.Any())
    {
        Console.WriteLine($"Export completed with {exportEvent.Warnings.Count} warnings.");
        foreach (var warning in exportEvent.Warnings)
        {
            Console.WriteLine($"- {warning}");
        }
    }

    Console.WriteLine($"Format: {exportEvent.ExportFormat}");
    Console.WriteLine($"Records: {exportEvent.RecordsExported:N0}");
    Console.WriteLine($"Duration: {exportEvent.GetExportDuration()}");
    Console.WriteLine($"Throughput: {exportEvent.GetThroughput():F2} rec/sec");
    Console.WriteLine($"Output Size: {exportEvent.GetHumanReadableSize()}");
    Console.WriteLine($"Location: {exportEvent.OutputPath}");
}
```

### Example 2: Post-Processing Validation
This example illustrates validating the generated files and checking compression status before triggering a downstream upload process.

```csharp
public async Task UploadIfValidAsync(ExportCompletedEvent exportEvent)
{
    if (exportEvent.RecordsExported == 0)
    {
        throw new InvalidOperationException("Cannot upload: No records were exported.");
    }

    if (!exportEvent.WasCompressed && exportEvent.OutputSizeBytes > 1_000_000_000) // 1GB limit for uncompressed
    {
        Console.WriteLine("Warning: Large uncompressed file detected. Consider enabling compression.");
    }

    foreach (var filePath in exportEvent.GeneratedFiles)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Expected export file missing: {filePath}");
        }
        
        // Simulate upload
        await UploadService.UploadAsync(filePath);
    }

    Console.WriteLine($"Successfully uploaded {exportEvent.GeneratedFiles.Count} files from {exportEvent.OutputPath}.");
}
```

## Notes

*   **Immutability**: Once constructed, the properties of `ExportCompletedEvent` are intended to be read-only. The state represents a historical snapshot of a completed operation and should not be modified.
*   **Thread Safety**: This class is read-only after initialization. Instances are safe to be read concurrently by multiple threads without external synchronization, provided the underlying `List<string>` collections (`GeneratedFiles`, `Warnings`) are not modified after construction. Consumers should treat these lists as read-only; modifying them may lead to race conditions if the instance is shared across threads.
*   **Division by Zero**: The `GetThroughput` method calculates records divided by time. If `ExportStartTime` equals `ExportEndTime` (zero duration), the behavior depends on the number of records: if records are greater than zero, the result is positive infinity; if records are zero, the result is zero. Callers should handle `double.IsInfinity` if strict numeric bounds are required.
*   **File Existence**: The `GeneratedFiles` list contains paths generated at the time of the event creation. It does not guarantee that files still exist on the disk at the time of consumption, as external processes may have moved or deleted them.
*   **Size Accuracy**: `OutputSizeBytes` is calculated at the moment the export finishes. If files are subsequently modified, compressed further, or appended to by other processes, this property will not reflect those changes.
