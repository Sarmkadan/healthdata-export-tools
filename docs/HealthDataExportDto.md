# HealthDataExportDto

Represents a complete snapshot of exported health data from a wearable device or health platform. This DTO aggregates multiple data streams—sleep, heart rate, SpO₂, steps, and activity—alongside metadata, record counts, and device information into a single transfer object suitable for serialization, API responses, or downstream processing pipelines.

## API

### `public string ExportId`
Unique identifier for this export instance. Typically a GUID or system-generated key used to correlate the export with its source request or batch job.

### `public DateTime ExportDate`
Timestamp indicating when the export was generated or finalized. This is distinct from the data’s own observation timestamps.

### `public ExportSummary Summary`
High-level summary object containing aggregated metrics, averages, or qualitative assessments derived from the raw data streams. The exact shape is defined by the `ExportSummary` type.

### `public List<SleepExportDto> SleepData`
Collection of sleep records, each represented as a `SleepExportDto`. Contains sleep stages, duration, and quality metrics for individual sleep sessions within the export window.

### `public List<HeartRateExportDto> HeartRateData`
Collection of heart rate observations. Each `HeartRateExportDto` typically includes a timestamp, bpm value, and possibly source or confidence metadata.

### `public List<SpO2ExportDto> SpO2Data`
Collection of blood oxygen saturation readings. Each `SpO2ExportDto` carries a timestamp and percentage value, along with any device-reported context.

### `public List<StepsExportDto> StepsData`
Collection of step count records. Each `StepsExportDto` represents a time-bucketed or instantaneous step count measurement.

### `public ExportMetadata Metadata`
Additional contextual information about the export itself—source device, firmware versions, export format version, or processing flags. Defined by the `ExportMetadata` type.

### `public int TotalRecords`
Aggregate count of all individual data points across all streams (sleep, heart rate, SpO₂, steps, activity). Provides a quick size estimate without enumerating every list.

### `public DateRangeDto DateRange`
The inclusive time span covered by this export. Contains start and end boundaries that define the observation window for all contained data streams.

### `public RecordCountSummary RecordCounts`
Per-stream breakdown of record counts. Allows consumers to determine how many records exist in each category without inspecting the full lists.

### `public List<string> DeviceTypes`
Distinct device type identifiers (e.g., `"Watch"`, `"Ring"`, `"Phone"`) that contributed data to this export. Useful for filtering or multi-device reconciliation.

### `public int Sleep`
Convenience count of sleep records. Mirrors the length of `SleepData` but avoids list enumeration overhead.

### `public int HeartRate`
Convenience count of heart rate records. Mirrors the length of `HeartRateData`.

### `public int SpO2`
Convenience count of SpO₂ records. Mirrors the length of `SpO2Data`.

### `public int Steps`
Convenience count of steps records. Mirrors the length of `StepsData`.

### `public int Activity`
Count of activity-related records (e.g., workouts, movement sessions) included in the export. The underlying activity data type is not directly exposed as a list on this DTO but is summarized here.

### `public DateTime Start`
The earliest observation timestamp present across all data streams. Derived from the actual data points, not the requested export range.

### `public DateTime End`
The latest observation timestamp present across all data streams. Derived from the actual data points.

### `public DateTime Date`
Canonical date associated with the export, typically the calendar date for which the data was requested or the midpoint of the observation window.

## Usage

### Example 1: Deserializing and inspecting an export

```csharp
string json = File.ReadAllText("export_2025-03-15.json");
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
HealthDataExportDto export = JsonSerializer.Deserialize<HealthDataExportDto>(json, options);

Console.WriteLine($"Export {export.ExportId} covers {export.DateRange.Start:d} to {export.DateRange.End:d}");
Console.WriteLine($"Total records: {export.TotalRecords}");
Console.WriteLine($"  Sleep: {export.Sleep}");
Console.WriteLine($"  HeartRate: {export.HeartRate}");
Console.WriteLine($"  SpO2: {export.SpO2}");
Console.WriteLine($"  Steps: {export.Steps}");
Console.WriteLine($"  Activity: {export.Activity}");
Console.WriteLine($"Devices: {string.Join(", ", export.DeviceTypes)}");

// Process sleep data
foreach (var sleep in export.SleepData)
{
    Console.WriteLine($"Sleep session: {sleep.StartTime:t} - {sleep.EndTime:t}, Duration: {sleep.DurationMinutes} min");
}
```

### Example 2: Merging multiple exports into a unified view

```csharp
List<HealthDataExportDto> exports = LoadExportsFromStorage();

// Combine all heart rate data across exports, sorted by timestamp
var allHeartRates = exports
    .SelectMany(e => e.HeartRateData)
    .OrderBy(hr => hr.Timestamp)
    .ToList();

// Compute daily step totals
var dailySteps = exports
    .SelectMany(e => e.StepsData)
    .GroupBy(s => s.Timestamp.Date)
    .ToDictionary(g => g.Key, g => g.Sum(s => s.StepCount));

// Validate record count consistency
foreach (var export in exports)
{
    bool consistent = export.Sleep == export.SleepData.Count
                      && export.HeartRate == export.HeartRateData.Count
                      && export.SpO2 == export.SpO2Data.Count
                      && export.Steps == export.StepsData.Count;

    if (!consistent)
        Console.WriteLine($"WARNING: Export {export.ExportId} has mismatched convenience counts.");
}
```

## Notes

- **Convenience count fields** (`Sleep`, `HeartRate`, `SpO2`, `Steps`) are integer mirrors of their corresponding list lengths. Consumers should not assume these are independently populated; they exist to avoid materializing or enumerating large collections solely for count retrieval. Inconsistencies between a count field and its list length indicate data corruption or a serialization error.
- **`Activity`** provides a count of activity records but no corresponding `List<ActivityExportDto>` is exposed on this DTO. Activity data may be embedded within `Summary`, `Metadata`, or handled by a separate retrieval path.
- **`TotalRecords`** should equal the sum of all per-stream record counts plus activity records. If this invariant is violated, the export may be incomplete.
- **`Start` and `End`** are derived from actual observation timestamps, not from `DateRange`. They may represent a narrower window if no data exists near the requested boundaries. Always validate that `Start <= End` and that both fall within or equal `DateRange` bounds.
- **Thread safety**: This DTO is a plain data container with no synchronization mechanisms. It is safe for concurrent reads after construction and full population. Concurrent mutation of lists (e.g., adding items to `SleepData` while another thread enumerates it) is not safe. Treat instances as immutable once built and shared across threads.
- **Serialization**: All public members are exposed for serialization. When round-tripping through JSON or other formats, ensure that `DateRange`, `Summary`, and `Metadata` types are themselves serializable and that `DateTime` values include appropriate offset or kind information to prevent timezone ambiguity.
- **Empty exports**: When no data exists for the requested period, lists will be empty, convenience counts will be zero, and `TotalRecords` will be zero. `Start` and `End` may default to `DateTime.MinValue` or match `DateRange` boundaries depending on implementation conventions—consumers should guard against uninitialized DateTime values before display.
