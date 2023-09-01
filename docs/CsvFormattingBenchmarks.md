# CsvFormattingBenchmarks

A benchmarking class for measuring the performance of CSV formatting operations for sleep, heart rate, and step data. This type is used to evaluate the efficiency of different formatting strategies under test conditions.

## API

### `Setup`

Initializes the benchmarking environment before each benchmark run. This method ensures that any required test data or state is prepared prior to executing the formatting operations.

- **Parameters**: None
- **Return value**: `void`
- **Exceptions**: May throw if initialization fails (e.g., missing test data).

---

### `FormatSleepCsv`

Formats sleep data into a CSV string asynchronously. The output follows a standardized schema for sleep records.

- **Parameters**: None
- **Return value**: `Task<string>` – A task representing the asynchronous operation, containing the formatted CSV string.
- **Exceptions**: May throw if the underlying data source is unavailable or if formatting fails.

---

### `FormatHeartRateCsv`

Formats heart rate data into a CSV string asynchronously. The output adheres to a predefined schema for heart rate records.

- **Parameters**: None
- **Return value**: `Task<string>` – A task representing the asynchronous operation, containing the formatted CSV string.
- **Exceptions**: May throw if the heart rate data cannot be retrieved or formatted.

---
### `FormatStepsCsv`

Formats step count data into a CSV string asynchronously. The output conforms to a consistent schema for step records.

- **Parameters**: None
- **Return value**: `Task<string>` – A task representing the asynchronous operation, containing the formatted CSV string.
- **Exceptions**: May throw if the step data is inaccessible or if formatting encounters an error.

## Usage

```csharp
// Example 1: Benchmarking sleep CSV formatting
var benchmarks = new CsvFormattingBenchmarks();
benchmarks.Setup();

string sleepCsv = await benchmarks.FormatSleepCsv();
Console.WriteLine($"Sleep CSV length: {sleepCsv.Length}");

// Example 2: Benchmarking heart rate and steps CSV formatting
var benchmarks = new CsvFormattingBenchmarks();
benchmarks.Setup();

string heartRateCsv = await benchmarks.FormatHeartRateCsv();
string stepsCsv = await benchmarks.FormatStepsCsv();

Console.WriteLine($"Heart rate records: {heartRateCsv.Split('\n').Length}");
Console.WriteLine($"Step records: {stepsCsv.Split('\n').Length}");
```

## Notes

- **Thread safety**: This type is not designed for concurrent use. Each benchmark operation should be executed on a single instance, and `Setup` should be called before each operation.
- **Data consistency**: The formatted output depends on the state of the data source at the time of formatting. Ensure data is not modified during benchmark runs.
- **Error handling**: Exceptions thrown by these methods indicate unrecoverable failures in formatting or data access. Callers should handle these cases appropriately.
- **Performance impact**: Asynchronous operations may introduce overhead. Benchmark results should account for this when comparing with synchronous alternatives.
