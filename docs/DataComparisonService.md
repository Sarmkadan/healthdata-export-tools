# DataComparisonService

The `DataComparisonService` class provides asynchronous methods to compare health‑data metrics between two distinct time periods or over a custom date range. After a successful comparison, the service populates a set of read‑only properties that expose aggregated statistics (record counts, averages, and percentage changes) for sleep duration, deep sleep, heart rate, steps, and SpO₂. The class is designed to be used in a single‑comparison workflow: instantiate, configure the periods or range (via constructor or external setup), call one of the comparison methods, and then read the results or export them to JSON.

## API

### Methods

#### `public Task<DataComparisonResult> ComparePeriodsAsync()`

- **Purpose**: Compares health data between two predefined periods (e.g., “last week” vs. “previous week”). The periods are determined by the service’s configuration, not by parameters on this method.
- **Parameters**: None.
- **Returns**: A `Task<DataComparisonResult>` that resolves to a result object containing detailed per‑metric comparisons. After this method completes successfully, the service’s result properties (listed below) are also populated.
- **Throws**: `InvalidOperationException` if the periods have not been configured or if the underlying data source is unavailable. May also throw `TaskCanceledException` if the operation is cancelled.

#### `public Task<DataComparisonResult> CompareByDateRangeAsync()`

- **Purpose**: Compares health data over a custom date range. The start and end dates are defined externally (e.g., via constructor arguments or a prior configuration step).
- **Parameters**: None.
- **Returns**: A `Task<DataComparisonResult>` that resolves to a result object containing per‑metric comparisons. The service’s result properties are updated after a successful call.
- **Throws**: `InvalidOperationException` if the date range has not been set or is invalid. May also throw `TaskCanceledException`.

#### `public async Task ExportToJsonAsync()`

- **Purpose**: Exports the current comparison results (the values of all result properties) to a JSON file. The output file path is determined by the service’s configuration.
- **Parameters**: None.
- **Returns**: A `Task` representing the asynchronous export operation.
- **Throws**: `InvalidOperationException` if no comparison has been performed yet. `IOException` if the file cannot be written.

### Properties

All properties are read‑only and are populated after a successful call to `ComparePeriodsAsync` or `CompareByDateRangeAsync`. Accessing them before a comparison returns default values (zero for numeric types, `DateTime.MinValue` for `GeneratedAt`).

| Property | Type | Description |
|----------|------|-------------|
| `GeneratedAt` | `DateTime` | The timestamp when the last comparison was completed. |
| `Period1RecordCount` | `int` | Number of data records in the first period. |
| `Period2RecordCount` | `int` | Number of data records in the second period. |
| `Period1AverageSleepMinutes` | `double` | Average total sleep duration (minutes) for period 1. |
| `Period2AverageSleepMinutes` | `double` | Average total sleep duration (minutes) for period 2. |
| `SleepDurationChangePercentage` | `double` | Percentage change in average sleep duration from period 1 to period 2. |
| `Period1AverageDeepSleepMinutes` | `double` | Average deep sleep duration (minutes) for period 1. |
| `Period2AverageDeepSleepMinutes` | `double` | Average deep sleep duration (minutes) for period 2. |
| `DeepSleepChangePercentage` | `double` | Percentage change in average deep sleep from period 1 to period 2. |
| `Period1AverageHeartRate` | `double` | Average heart rate (bpm) for period 1. |
| `Period2AverageHeartRate` | `double` | Average heart rate (bpm) for period 2. |
| `HeartRateChangePercentage` | `double` | Percentage change in average heart rate from period 1 to period 2. |
| `Period1AverageSteps` | `double` | Average daily step count for period 1. |
| `Period2AverageSteps` | `double` | Average daily step count for period 2. |
| `StepsChangePercentage` | `double` | Percentage change in average steps from period 1 to period 2. |
| `Period1AverageSpO2` | `double` | Average blood oxygen saturation (%) for period 1. |
| `Period2AverageSpO2` | `double` | Average blood oxygen saturation (%) for period 2. |

## Usage

### Example 1: Compare two predefined periods and display results

```csharp
// Assume the service is configured with two periods (e.g., via constructor)
var service = new DataComparisonService(/* configuration */);

DataComparisonResult result = await service.ComparePeriodsAsync();

Console.WriteLine($"Comparison generated at: {service.GeneratedAt}");
Console.WriteLine($"Period 1 records: {service.Period1RecordCount}");
Console.WriteLine($"Period 2 records: {service.Period2RecordCount}");
Console.WriteLine($"Sleep change: {service.SleepDurationChangePercentage:F2}%");
Console.WriteLine($"Heart rate change: {service.HeartRateChangePercentage:F2}%");
```

### Example 2: Compare a custom date range and export to JSON

```csharp
// Service configured with a specific date range
var service = new DataComparisonService(/* startDate, endDate */);

DataComparisonResult result = await service.CompareByDateRangeAsync();

Console.WriteLine($"Deep sleep change: {service.DeepSleepChangePercentage:F2}%");
Console.WriteLine($"Steps change: {service.StepsChangePercentage:F2}%");

await service.ExportToJsonAsync();
// JSON file written to the configured output path
```

## Notes

- **Property validity**: All result properties are only meaningful after a successful call to one of the comparison methods. Accessing them before a comparison returns default values (zero or `DateTime.MinValue`). The `DataComparisonResult` object returned by the methods contains the same data and can be used independently.
- **Thread safety**: The service is not thread‑safe for concurrent calls to `ComparePeriodsAsync`, `CompareByDateRangeAsync`, or `ExportToJsonAsync`. If multiple threads attempt to perform comparisons simultaneously, the internal state may become inconsistent. After a comparison completes, reading the properties from a single thread is safe. For concurrent scenarios, instantiate separate service instances.
- **Edge cases**: If a period contains no records, the corresponding average properties will be zero and the change percentage will be `double.NaN` (division by zero). The `RecordCount` properties will be zero. The service does not throw in this case; callers should check `RecordCount` values before relying on averages or percentages.
- **Export**: `ExportToJsonAsync` serializes only the properties listed above. It does not include the full `DataComparisonResult` object. The export will fail if no comparison has been performed.
