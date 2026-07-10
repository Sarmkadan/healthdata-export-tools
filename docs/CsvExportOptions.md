# CsvExportOptions

Configuration object that controls which health data streams are exported to CSV and how the output is formatted. Instances are passed to export routines to tailor the columns, date representation, and inclusion flags for sleep, heart rate, SpO₂, steps, and activity data.

## API

| Member | Type | Purpose | Parameters | Return Value | Exceptions |
|--------|------|---------|------------|--------------|------------|
| `IncludeSleep` | `bool` | When `true`, sleep records are included in the export; when `false`, they are omitted. | None | The current boolean value. | None |
| `IncludeHeartRate` | `bool` | When `true`, heart‑rate records are included in the export; when `false`, they are omitted. | None | The current boolean value. | None |
| `IncludeSpO2` | `bool` | When `true`, blood‑oxygen (SpO₂) records are included in the export; when `false`, they are omitted. | None | The current boolean value. | None |
| `IncludeSteps` | `bool` | When `true`, step‑count records are included in the export; when `false`, they are omitted. | None | The current boolean value. | None |
| `IncludeActivity` | `bool` | When `true`, generic activity records are included in the export; when `false`, they are omitted. | None | The current boolean value. | None |
| `DateFormat` | `string` | .NET format string applied to all timestamp columns in the generated CSV (e.g., `"yyyy-MM-dd HH:mm:ss"`). If `null` or empty, the exporter uses its internal default format. | None | The current format string. | None |
| `SleepColumns` | `IReadOnlyList<string>?` | Optional list of column names to export for sleep data. If `null`, the exporter uses its default set of sleep columns. The list is treated as read‑only; callers should not modify it after assignment. | None | The current list reference or `null`. | None |
| `HeartRateColumns` | `IReadOnlyList<string>?` | Optional list of column names to export for heart‑rate data. If `null`, the exporter uses its default set of heart‑rate columns. The list is treated as read‑only. | None | The current list reference or `null`. | None |
| `SpO2Columns` | `IReadOnlyList<string>?` | Optional list of column names to export for SpO₂ data. If `null`, the exporter uses its default set of SpO₂ columns. The list is treated as read‑only. | None | The current list reference or `null`. | None |
| `StepsColumns` | `IReadOnlyList<string>?` | Optional list of column names to export for steps data. If `null`, the exporter uses its default set of steps columns. The list is treated as read‑only. | None | The current list reference or `null`. | None |

## Usage

### Basic export with default columns
```csharp
var options = new CsvExportOptions
{
    IncludeSleep = true,
    IncludeHeartRate = true,
    IncludeSpO2 = false,
    IncludeSteps = true,
    IncludeActivity = true,
    DateFormat = "yyyy-MM-dd HH:mm:ss"
    // Column lists left null → use built‑in defaults
};

Exporter.ExportToCsv(dataSource, "export.csv", options);
```

### Custom column selection and alternative date format
```csharp
var sleepCols = new List<string> { "StartTime", "EndTime", "DurationMinutes", "Efficiency" };
var heartRateCols = new List<string> { "Timestamp", "Bpm" };

var options = new CsvExportOptions
{
    IncludeSleep = true,
    IncludeHeartRate = true,
    IncludeSpO2 = true,
    IncludeSteps = false,
    IncludeActivity = false,
    DateFormat = "MM/dd/yyyy HH:mm",
    SleepColumns = sleepCols,
    HeartRateColumns = heartRateCols,
    // SpO2Columns, StepsColumns left null → defaults
};

Exporter.ExportToCsv(dataSource, "custom_export.csv", options);
```

## Notes

- All members are mutable fields; concurrent modification from multiple threads without external synchronization can lead to race conditions. It is recommended to create a separate `CsvExportOptions` instance per thread or to guard access with a lock.
- The `*Columns` properties accept `null` to signal “use default columns”. Supplying an empty list (`new List<string>()`) will result in no columns being exported for that data type, which may produce empty or malformed CSV output.
- `DateFormat` must be a valid .NET custom or standard format string recognized by `DateTime.ToString(string)`. An invalid format will cause the underlying exporter to throw a `FormatException` when formatting timestamps.
- Changing the column lists after they have been passed to an export method does not affect an ongoing export; the exporter reads the values at the moment the method is invoked.
- The type does not inherit from any other class and does not implement any interfaces; it is a plain data container. No disposal or resource cleanup is required.
