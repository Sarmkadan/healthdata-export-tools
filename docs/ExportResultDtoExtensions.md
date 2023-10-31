# ExportResultDtoExtensions

Provides convenience extension methods for inspecting and summarizing the outcome of an export operation represented by an `ExportResultDto`. These methods expose derived properties such as overall duration, success status, presence of issues, and total record counts without requiring direct manipulation of the underlying result structure.

## API

### GetDuration

```csharp
public static TimeSpan GetDuration(this ExportResultDto result)
```

Returns the total wall-clock duration of the export operation. The value is computed from the start and end timestamps present in the result. If either timestamp is unavailable or invalid, the method returns `TimeSpan.Zero`.

**Parameters:**
- `result` — The `ExportResultDto` instance to inspect. Must not be `null`.

**Return Value:**
A `TimeSpan` representing the elapsed time from export start to export completion.

**Throws:**
- `ArgumentNullException` when `result` is `null`.

---

### IsSuccessful

```csharp
public static bool IsSuccessful(this ExportResultDto result)
```

Indicates whether the export operation completed without any errors. The determination is based on the error state recorded in the result object.

**Parameters:**
- `result` — The `ExportResultDto` instance to inspect. Must not be `null`.

**Return Value:**
`true` if the export finished with no errors; otherwise `false`.

**Throws:**
- `ArgumentNullException` when `result` is `null`.

---

### HasIssues

```csharp
public static bool HasIssues(this ExportResultDto result)
```

Reports whether the export encountered any warnings, partial failures, or non-critical problems, even if the overall operation is still considered successful. This is distinct from `IsSuccessful` and may return `true` for exports that completed but with caveats.

**Parameters:**
- `result` — The `ExportResultDto` instance to inspect. Must not be `null`.

**Return Value:**
`true` if the result contains any issues or warnings; otherwise `false`.

**Throws:**
- `ArgumentNullException` when `result` is `null`.

---

### GetTotalProcessedRecords

```csharp
public static int GetTotalProcessedRecords(this ExportResultDto result)
```

Returns the total number of records processed during the export, typically aggregated across all data sources or batches included in the operation.

**Parameters:**
- `result` — The `ExportResultDto` instance to inspect. Must not be `null`.

**Return Value:**
An integer representing the total count of processed records. Returns `0` if no record count information is available.

**Throws:**
- `ArgumentNullException` when `result` is `null`.

## Usage

### Example 1: Basic Post-Export Inspection

```csharp
ExportResultDto exportResult = exportService.RunExport(config);

if (exportResult.IsSuccessful())
{
    Console.WriteLine($"Export completed in {exportResult.GetDuration():c}");
    Console.WriteLine($"Processed {exportResult.GetTotalProcessedRecords()} records");

    if (exportResult.HasIssues())
    {
        Console.WriteLine("Export finished with warnings — review the issue log.");
    }
}
else
{
    Console.WriteLine("Export failed. Check error details in the result.");
}
```

### Example 2: Aggregating Results from Multiple Exports

```csharp
var results = new List<ExportResultDto>
{
    dailyExport.Execute(),
    weeklyExport.Execute(),
    monthlyExport.Execute()
};

int totalRecords = results.Sum(r => r.GetTotalProcessedRecords());
TimeSpan totalDuration = TimeSpan.FromTicks(results.Sum(r => r.GetDuration().Ticks));
bool anyFailures = results.Any(r => !r.IsSuccessful());
bool anyIssues = results.Any(r => r.HasIssues());

Console.WriteLine($"Total records across all exports: {totalRecords}");
Console.WriteLine($"Cumulative duration: {totalDuration}");

if (anyFailures)
    Console.WriteLine("One or more exports failed.");
else if (anyIssues)
    Console.WriteLine("All exports completed, but some had warnings.");
else
    Console.WriteLine("All exports completed cleanly.");
```

## Notes

- All methods throw `ArgumentNullException` when passed a `null` reference. Callers should guard against `null` before invoking these extensions.
- `GetDuration` relies on the presence of both start and end timestamps. If the export was interrupted before recording an end time, or if timestamps are malformed, the method returns `TimeSpan.Zero` rather than throwing. This means a zero duration may indicate either an instantaneous operation or missing timing data — interpret accordingly.
- `HasIssues` and `IsSuccessful` are independent properties. A result can be successful yet still have issues (e.g., recoverable warnings), or unsuccessful with no explicitly flagged issues (e.g., a hard failure that bypasses the issue-tracking mechanism). Always check both when a complete picture of export health is required.
- `GetTotalProcessedRecords` returns `0` when the underlying record count is absent. Do not assume that zero records means an empty export; it may also mean the export metadata did not capture record counts.
- These methods perform read-only inspection of the `ExportResultDto` and do not mutate state. They are safe to call concurrently from multiple threads as long as the underlying `ExportResultDto` instance is not being modified by another thread during the call.
