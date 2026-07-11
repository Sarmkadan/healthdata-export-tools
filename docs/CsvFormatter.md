# CsvFormatter

The `CsvFormatter` type provides functionality to serialize various health‑data objects into CSV‑formatted strings. It supports single records, collections, and specialized formatters for sleep, heart‑rate, SpO₂, and step data, as well as asynchronous validation of the input before formatting.

## API

### `public CsvFormatter()`
Initializes a new instance of the `CsvFormatter` class. The constructor has no parameters and does not throw under normal circumstances.

### `public bool CanFormat { get; }`
Gets a value indicating whether the formatter is capable of producing CSV output for the data types it handles. This property always returns `true` for the supported health‑data types and does not throw.

### `public async Task<string> FormatAsync(object data)`
Asynchronously formats a single health‑data object into a CSV string.

- **Parameters**  
  - `data`: The object to format. Must not be `null` and must be of a type supported by the formatter (e.g., a concrete health‑data record).

- **Return Value**  
  A `Task<string>` that completes with the CSV representation of the supplied object, including a header row.

- **Exceptions**  
  - `ArgumentNullException` if `data` is `null`.  
  - `InvalidOperationException` if the type of `data` is not supported by the formatter.

### `public async Task<string> FormatCollectionAsync(IEnumerable<object> collection)`
Asynchronously formats a collection of health‑data objects into a CSV string, emitting a single header row followed by one row per item.

- **Parameters**  
  - `collection`: The sequence of objects to format. Must not be `null`.

- **Return Value**  
  A `Task<string>` that completes with the CSV representation of the entire collection. If the collection is empty, the result contains only the header row.

- **Exceptions**  
  - `ArgumentNullException` if `collection` is `null`.  
  - `InvalidOperationException` if any element in the collection is of an unsupported type.

### `public async Task<string> FormatSleepDataAsync(SleepData sleepData)`
Asynchronously formats a `SleepData` instance into a CSV string specific to sleep records.

- **Parameters**  
  - `sleepData`: The sleep data to format. Must not be `null`.

- **Return Value**  
  A `Task<string>` containing the CSV formatted sleep data.

- **Exceptions**  
  - `ArgumentNullException` if `sleepData` is `null`.

### `public async Task<string> FormatHeartRateDataAsync(HeartRateData heartRateData)`
Asynchronously formats a `HeartRateData` instance into a CSV string specific to heart‑rate records.

- **Parameters**  
  - `heartRateData`: The heart‑rate data to format. Must not be `null`.

- **Return Value**  
  A `Task<string>` containing the CSV formatted heart‑rate data.

- **Exceptions**  
  - `ArgumentNullException` if `heartRateData` is `null`.

### `public async Task<string> FormatSpO2DataAsync(SpO2Data spo2Data)`
Asynchronously formats a `SpO2Data` instance into a CSV string specific to blood‑oxygen saturation records.

- **Parameters**  
  - `spo2Data`: The SpO₂ data to format. Must not be `null`.

- **Return Value**  
  A `Task<string>` containing the CSV formatted SpO₂ data.

- **Exceptions**  
  - `ArgumentNullException` if `spo2Data` is `null`.

### `public async Task<string> FormatStepsDataAsync(StepsData stepsData)`
Asynchronously formats a `StepsData` instance into a CSV string specific to step‑count records.

- **Parameters**  
  - `stepsData`: The steps data to format. Must not be `null`.

- **Return Value**  
  A `Task<string>` containing the CSV formatted steps data.

- **Exceptions**  
  - `ArgumentNullException` if `stepsData` is `null`.

### `public async Task<List<string>> ValidateAsync(object data)`
Asynchronously validates a health‑data object and returns a list of validation messages.

- **Parameters**  
  - `data`: The object to validate. Must not be `null`.

- **Return Value**  
  A `Task<List<string>>` that completes with a list of error messages. An empty list indicates the object is valid.

- **Exceptions**  
  - `ArgumentNullException` if `data` is `null`.

## Usage

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using HealthDataExportTools.Formatters; // namespace containing CsvFormatter
using HealthDataExportTools.Models;    // namespace containing the data models

public class ExampleService
{
    private readonly CsvFormatter _formatter = new CsvFormatter();

    public async Task<string> ExportSleepDataAsync(SleepData sleep)
    {
        if (!_formatter.CanFormat)
        {
            throw new InvalidOperationException("Formatter is not capable of formatting data.");
        }

        // Validate before formatting (optional)
        var errors = await _formatter.ValidateAsync(sleep);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"Validation failed: {string.Join("; ", errors)}");
        }

        return await _formatter.FormatAsync(sleep);
    }

    public async Task<string> ExportHeartRateBatchAsync(IEnumerable<HeartRateData> batch)
    {
        var errors = await _formatter.ValidateAsync(batch);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"Validation failed: {string.Join("; ", errors)}");
        }

        return await _formatter.FormatCollectionAsync(batch);
    }
}
```

```csharp
using System.Threading.Tasks;
using HealthDataExportTools.Formatters;
using HealthDataExportTools.Models;

public class ExportJob
{
    public async Task RunAsync()
    {
        var formatter = new CsvFormatter();

        // Example: format a single SpO₂ reading
        var spo2 = new SpO2Data { Timestamp = System.DateTime.UtcNow, Value = 98 };
        string spo2Csv = await formatter.FormatSpO2DataAsync(spo2);
        System.Console.WriteLine(spo2Csv);

        // Example: format a collection of step records
        var steps = new List<StepsData>
        {
            new StepsData { Timestamp = System.DateTime.UtcNow.AddHours(-1), Count = 3200 },
            new StepsData { Timestamp = System.DateTime.UtcNow, Count = 4150 }
        };
        string stepsCsv = await formatter.FormatCollectionAsync(steps);
        System.Console.WriteLine(stepsCsv);
    }
}
```

## Notes

- All formatting methods are stateless; they do not retain any data between calls, making them safe to invoke concurrently from multiple threads.
- The `CanFormat` property is thread‑safe and reflects the formatter’s capability irrespective of instance state.
- Passing `null` to any method that expects a data argument results in an `ArgumentNullException`. Empty collections are permitted and produce a CSV containing only the header row.
- The CSV output uses culture‑invariant formatting for numeric values and dates (ISO 8601 for timestamps) to ensure consistency across environments.
- Validation errors returned by `ValidateAsync` are intended for informational purposes; callers may choose to treat them as warnings or as failures depending on business logic. Validation does not modify the input data.
