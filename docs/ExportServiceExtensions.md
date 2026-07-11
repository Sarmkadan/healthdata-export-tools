# ExportServiceExtensions

The `ExportServiceExtensions` class provides static asynchronous methods for exporting health data summaries to CSV format, and exposes instance properties that capture aggregated metrics from the most recent export operation. These properties are populated after a successful call to one of the export methods and reflect the data that was written to the CSV output. The class is designed to be used within the `healthdata-export-tools` project to facilitate reporting and data quality analysis.

## API

### Static Methods

#### `public static async Task ExportSleepSummaryToCsvAsync()`

Exports a sleep summary report to CSV. The method writes aggregated sleep metrics (duration, stages, quality) for a defined period.  
**Parameters:** None.  
**Returns:** A `Task` representing the asynchronous export operation.  
**Throws:** `IOException` if the output file cannot be written; `InvalidOperationException` if no sleep data is available.

#### `public static async Task ExportHeartRateAnalyticsToCsvAsync()`

Exports heart rate analytics (e.g., resting heart rate, variability, zone durations) to CSV.  
**Parameters:** None.  
**Returns:** A `Task` representing the asynchronous export operation.  
**Throws:** `IOException` on file write failure; `InvalidOperationException` if heart rate data is missing.

#### `public static async Task ExportHealthDashboardToCsvAsync()`

Exports a comprehensive health dashboard summary (combining sleep, heart rate, activity, etc.) to CSV.  
**Parameters:** None.  
**Returns:** A `Task` representing the asynchronous export operation.  
**Throws:** `IOException` on file write failure; `InvalidOperationException` if required data sources are unavailable.

#### `public static async Task ExportDataQualityReportToCsvAsync()`

Exports a data quality report detailing completeness, consistency, and anomalies across health data streams.  
**Parameters:** None.  
**Returns:** A `Task` representing the asynchronous export operation.  
**Throws:** `IOException` on file write failure; `InvalidOperationException` if data quality metrics cannot be computed.

### Instance Properties

The following properties are populated after a successful export call and reflect the metrics of the exported data set.

| Property | Type | Description |
|----------|------|-------------|
| `TotalRecords` | `int` | Total number of records exported. |
| `TotalDurationMinutes` | `int` | Total duration in minutes covered by the export. |
| `TotalDeepSleepMinutes` | `int` | Total minutes of deep sleep across the exported period. |
| `TotalLightSleepMinutes` | `int` | Total minutes of light sleep. |
| `TotalRemMinutes` | `int` | Total minutes of REM sleep. |
| `TotalAwakeMinutes` | `int` | Total minutes of awake time. |
| `AvgQuality` | `double?` | Average sleep quality score (nullable). |
| `AvgScore` | `int?` | Average sleep score (nullable). |
| `BestSleepDate` | `string?` | Date of the best sleep session (nullable). |
| `WorstSleepDate` | `string?` | Date of the worst sleep session (nullable). |
| `DeepSleepPercentage` | `double` | Percentage of total sleep time spent in deep sleep. |
| `RemPercentage` | `double` | Percentage of total sleep time spent in REM. |
| `EfficiencyPercentage` | `double` | Sleep efficiency (time asleep / time in bed) as a percentage. |
| `Date` | `string?` | Date associated with the exported data (nullable). |
| `DurationMinutes` | `int` | Duration in minutes of the exported data set. |
| `DeepSleepMinutes` | `int` | Deep sleep minutes for the exported data set. |

## Usage

### Example 1: Export sleep summary and inspect aggregated metrics

```csharp
using HealthDataExportTools;

var exporter = new ExportServiceExtensions();

// Export sleep summary to CSV (file path is configured internally)
await ExportServiceExtensions.ExportSleepSummaryToCsvAsync();

// Access aggregated metrics from the export
Console.WriteLine($"Total records: {exporter.TotalRecords}");
Console.WriteLine($"Deep sleep percentage: {exporter.DeepSleepPercentage:F1}%");
Console.WriteLine($"Efficiency: {exporter.EfficiencyPercentage:F1}%");
Console.WriteLine($"Best sleep date: {exporter.BestSleepDate ?? "N/A"}");
```

### Example 2: Export multiple reports and handle errors

```csharp
using HealthDataExportTools;

var exporter = new ExportServiceExtensions();

try
{
    await ExportServiceExtensions.ExportHeartRateAnalyticsToCsvAsync();
    Console.WriteLine($"Heart rate export completed. Duration: {exporter.DurationMinutes} min");

    await ExportServiceExtensions.ExportDataQualityReportToCsvAsync();
    Console.WriteLine($"Data quality report exported. Total records: {exporter.TotalRecords}");
}
catch (IOException ex)
{
    Console.Error.WriteLine($"File I/O error: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Data unavailable: {ex.Message}");
}
```

## Notes

- **Edge cases:** If no data is available for the requested export, the method throws `InvalidOperationException`. Properties remain at their default values (0, `null`, etc.) until a successful export completes. The `AvgQuality`, `AvgScore`, `BestSleepDate`, `WorstSleepDate`, and `Date` properties are nullable and will be `null` when no data is present.
- **Thread safety:** The static export methods are safe to call concurrently from multiple threads, but the instance properties are not synchronized. If multiple threads share the same `ExportServiceExtensions` instance, access to properties should be serialized (e.g., via a lock) to avoid reading inconsistent state. For thread‑safe usage, consider creating a separate instance per thread or per export operation.
- **Property population:** Properties are overwritten on each successful export call. They are not cumulative; they reflect only the most recent export. Calling an export method that fails does not modify the properties.
