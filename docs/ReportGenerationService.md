# ReportGenerationService

The `ReportGenerationService` class provides asynchronous methods for generating various health‑data summary reports and exporting them to JSON. It also exposes read‑only properties that contain metadata about the most recently generated report.

## API

### Constructor
- **ReportGenerationService()**  
  Creates a new instance of the service. No parameters are required. The instance is ready to use after construction; calling any generation method before setting required data sources may result in an exception.

### Methods
- **GenerateSummaryReportAsync()**  
  Asynchronously produces a comprehensive health summary report.  
  - **Return:** `Task<HealthSummaryReport>` – the generated report when the task completes.  
  - **Throws:** `InvalidOperationException` if required data sources are not configured; `IOException` if reading source data fails.

- **GenerateDailyReportAsync()**  
  Asynchronously generates a report summarizing data for a single day.  
  - **Return:** `Task<DailySummaryReport>` – the daily report.  
  - **Throws:** Same as `GenerateSummaryReportAsync`.

- **GenerateWeeklySummaryReportAsync()**  
  Asynchronously returns a list of weekly summary reports.  
  - **Return:** `Task<List<WeeklySummaryReport>>` – collection of weekly reports.  
  - **Throws:** Same as `GenerateSummaryReportAsync`.  
  *(Note: the service exposes this method twice; both signatures are identical.)*

- **ExportWeeklySummaryToJsonAsync()**  
  Asynchronously exports the most recently generated weekly summary data to a JSON file.  
  - **Return:** `Task` – completes when the file has been written.  
  - **Throws:** `UnauthorizedAccessException` if the target path is not writable; `IOException` on write failure.

- **GenerateTrendReportAsync()**  
  Asynchronously creates a trend analysis report over the configured date range.  
  - **Return:** `Task<TrendAnalysisReport>` – the trend report.  
  - **Throws:** Same as `GenerateSummaryReportAsync`.

### Properties
- **ReportDate** (`DateTime`)  
  The date associated with the most recently generated report. Updated after each successful generation call.

- **TotalRecords** (`int`)  
  The total number of source records processed for the last report.

- **DateRange** (`DateRange`)  
  The start and end dates that were used for the last report generation.

- **DataTypeStatistics** (`List<DataTypeStatistic>`)  
  Detailed statistics per data type included in the last report.

- **DeviceDistribution** (`Dictionary<string, int>`)  
  Mapping of device identifiers to the count of records contributed by each device in the last report.

- **DataType** (`string`)  
  The primary data type focus of the last report (e.g., "HeartRate", "Steps").

- **RecordCount** (`int`)  
  Number of records of the primary `DataType` processed in the last report.

- **AverageValue** (`double`)  
  Mean value of the primary `DataType` in the last report.

- **MinValue** (`double`)  
  Minimum observed value of the primary `DataType` in the last report.

- **MaxValue** (`double`)  
  Maximum observed value of the primary `DataType` in the last report.

- **StandardDeviation** (`double`)  
  Standard deviation of the primary `DataType` values in the last report.

- **Date** (`DateTime`)  
  The specific date that the last report pertains to (for daily reports) or the start date of the range (for weekly/trend reports).

- **GeneratedAt** (`DateTime`)  
  Timestamp indicating when the last report was generated.

## Usage

```csharp
using HealthdataExportTools;

// Create the service
var reportService = new ReportGenerationService();

// Generate a daily summary for the configured date
DailySummaryReport dailyReport = await reportService.GenerateDailyReportAsync();
Console.WriteLine($"Daily report generated on {dailyReport.GeneratedAt}");
```

```csharp
using HealthdataExportTools;
using System.IO;

var service = new ReportGenerationService();

// Produce a trend analysis and export the weekly data to JSON
TrendAnalysisReport trend = await service.GenerateTrendReportAsync();
await service.ExportWeeklySummaryToJsonAsync();

// Verify the export file exists
string exportPath = Path.Combine(Environment.CurrentDirectory, "weekly_summary.json");
if (File.Exists(exportPath))
{
    Console.WriteLine("Weekly summary exported successfully.");
}
```

## Notes
- All generation methods are asynchronous and should be awaited; calling them without `await` may lead to unobserved exceptions.
- The service is **not** thread‑safe. Concurrent calls from multiple threads on the same instance may corrupt internal state (e.g., `ReportDate`, `TotalRecords`). For parallel usage, create separate instances per thread or synchronize access externally.
- If a method is invoked before any data source has been assigned (if the class expects external configuration), it will throw an `InvalidOperationException`. Ensure the service is properly initialized via its constructor or any configuration members not listed here.
- Repeated calls to `GenerateWeeklySummaryReportAsync` will overwrite the values of the read‑only properties with the data from the most recent call.
- The `ExportWeeklySummaryToJsonAsync` method relies on the data produced by the last successful weekly report generation; exporting without first generating a weekly report may result in an empty or default file.
