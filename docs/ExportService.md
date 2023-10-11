# ExportService

`ExportService` provides a unified interface for exporting health data records into structured file formats. It supports JSON and CSV output for sleep, heart rate, steps, and combined datasets, with optional heart-rate zone classification. The service exposes both single-type and multi-type export methods, along with a set of properties that represent the most recent data snapshot used during export operations.

## API

### Methods

#### `ExportToJsonAsync`
Exports the current health data snapshot to a single JSON file. The output includes all available record types in one structured document.

- **Parameters:** None (operates on internal state).
- **Returns:** `Task` representing the asynchronous write operation.
- **Throws:** `InvalidOperationException` when no data has been loaded or the internal state is empty. `IOException` when the target file path is inaccessible or write permissions are insufficient.

#### `ExportSleepToCsvAsync`
Exports sleep-specific records to a CSV file. Columns correspond to the sleep-related properties of the service.

- **Parameters:** None.
- **Returns:** `Task` representing the asynchronous write operation.
- **Throws:** `InvalidOperationException` when sleep data is absent. `IOException` on file write failures.

#### `ExportHeartRateToCsvAsync`
Exports heart-rate records to a CSV file without zone classification. Output includes minimum, maximum, and average BPM values.

- **Parameters:** None.
- **Returns:** `Task` representing the asynchronous write operation.
- **Throws:** `InvalidOperationException` when heart-rate data is absent. `IOException` on file write failures.

#### `ExportHeartRateWithZonesToCsvAsync`
Exports heart-rate records to a CSV file with additional zone columns derived from BPM thresholds. Zones are calculated based on configurable or default ranges.

- **Parameters:** None.
- **Returns:** `Task` representing the asynchronous write operation.
- **Throws:** `InvalidOperationException` when heart-rate data is absent. `IOException` on file write failures.

#### `ExportStepsToCsvAsync`
Exports step-count records to a CSV file.

- **Parameters:** None.
- **Returns:** `Task` representing the asynchronous write operation.
- **Throws:** `InvalidOperationException` when steps data is absent. `IOException` on file write failures.

#### `ExportToJsonPerTypeAsync`
Exports each health data type (sleep, heart rate, steps) into separate JSON files, one per type, in a single operation.

- **Parameters:** None.
- **Returns:** `Task` representing the asynchronous write operation.
- **Throws:** `InvalidOperationException` when no data types are populated. `IOException` on file write failures.

#### `ExportCompleteAsync`
Performs a full export of all available health data types to their respective formats in one batch operation. This combines the behaviour of the individual CSV exports and the per-type JSON export.

- **Parameters:** None.
- **Returns:** `Task` representing the asynchronous write operation.
- **Throws:** `InvalidOperationException` when no data is loaded. `IOException` on file write failures.

### Properties

#### `Date` (string?)
The date associated with the current data snapshot. Can be `null` when no data has been set. Represented as a string to accommodate various date formats.

#### `Duration` (int)
Total sleep duration in minutes. Relevant to sleep exports.

#### `DeepSleep` (int)
Deep sleep duration in minutes.

#### `LightSleep` (int)
Light sleep duration in minutes.

#### `REM` (int)
REM sleep duration in minutes.

#### `Awake` (int)
Time spent awake during the sleep period, in minutes.

#### `Quality` (string?)
A qualitative descriptor of sleep quality. May be `null` when not assessed.

#### `Score` (int?)
A numeric sleep score. Nullable when no score is available.

#### `AvgHeartRate` (int?)
Average heart rate across the recorded period. Nullable when heart-rate data is absent.

#### `MinBpm` (int)
Minimum recorded beats per minute.

#### `MaxBpm` (int)
Maximum recorded beats per minute.

#### `AvgBpm` (int)
Average beats per minute computed from the heart-rate dataset.

## Usage

### Example 1: Export sleep data to CSV after loading a nightly snapshot

```csharp
var exportService = new ExportService();
// Assume data loading has populated the sleep properties
exportService.Date = "2025-03-15";
exportService.Duration = 460;
exportService.DeepSleep = 120;
exportService.LightSleep = 260;
exportService.REM = 60;
exportService.Awake = 20;
exportService.Quality = "Good";
exportService.Score = 85;

try
{
    await exportService.ExportSleepToCsvAsync();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Export failed: {ex.Message}");
}
```

### Example 2: Run a complete multi-format export for all available health metrics

```csharp
var exportService = new ExportService();
// Populate heart-rate and steps in addition to sleep
exportService.Date = "2025-03-15";
exportService.MinBpm = 48;
exportService.MaxBpm = 142;
exportService.AvgBpm = 72;
// ... other properties set accordingly

try
{
    await exportService.ExportCompleteAsync();
    Console.WriteLine("Full export completed successfully.");
}
catch (IOException ex)
{
    Console.WriteLine($"I/O error during export: {ex.Message}");
}
```

## Notes

- All export methods assume that the target file paths are pre-configured or derived from internal settings. The service does not accept path parameters; configuration must occur before invocation.
- Properties such as `Date`, `Quality`, `Score`, and `AvgHeartRate` are nullable. Export methods must handle `null` values gracefully in their output formatting; consumers should ensure meaningful defaults or explicit null checks when reading exported files.
- `ExportHeartRateWithZonesToCsvAsync` relies on zone boundaries that are either hardcoded or injected via configuration. If zone thresholds are not set, the method may produce empty zone columns or fall back to default ranges.
- The service is not thread-safe by default. Concurrent calls to export methods while properties are being mutated may produce inconsistent output files. Synchronisation is the caller’s responsibility when sharing an instance across threads.
- `ExportCompleteAsync` internally calls the individual type-specific exports. A partial failure (e.g., one CSV export throws) may leave some files written and others missing. The method does not wrap operations in a transaction; callers should implement compensating logic if atomicity is required.
- `InvalidOperationException` is thrown when the data required for a specific export is absent. Callers should verify that the relevant properties have been populated before invoking the corresponding export method.
