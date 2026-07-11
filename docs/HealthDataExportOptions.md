# HealthDataExportOptions

Represents the configuration for exporting health data from a local or remote source. This class encapsulates all parameters required to define the input data location, output destination, export format, validation and analysis flags, date filters, notification settings, and logging verbosity. It also provides built-in validation logic to verify that the combination of settings is consistent before the export operation begins.

## API

### Properties

- **`public string InputPath`**  
  Gets or sets the file system path or URI to the source health data file or directory.  
  *Throws*: `ArgumentNullException` if set to `null`; `ArgumentException` if the path is empty or contains invalid characters.

- **`public string OutputPath`**  
  Gets or sets the destination path where the exported data will be written.  
  *Throws*: `ArgumentNullException` if set to `null`; `ArgumentException` if the path is empty or invalid.

- **`public string DatabasePath`**  
  Gets or sets the path to a local database file (e.g., SQLite) used for intermediate processing or caching.  
  *Throws*: `ArgumentNullException` if set to `null`; `ArgumentException` if the path is empty.

- **`public ExportFormat ExportFormat`**  
  Gets or sets the desired output format (e.g., CSV, JSON, FHIR).  
  *Throws*: `ArgumentOutOfRangeException` if set to an undefined enum value.

- **`public bool ValidateData`**  
  When `true`, enables data integrity validation against the source before export. Default is `false`.

- **`public bool PerformAnalysis`**  
  When `true`, triggers a trend analysis on the exported data. Default is `false`.

- **`public int TrendAnalysisDays`**  
  Number of days to include in the trend analysis window. Must be greater than zero when `PerformAnalysis` is `true`.  
  *Throws*: `ArgumentOutOfRangeException` if set to a value less than 1.

- **`public int MaxRecordAgeDays`**  
  Maximum age (in days) of records to include in the export. Records older than this are filtered out. Must be non-negative.  
  *Throws*: `ArgumentOutOfRangeException` if set to a negative value.

- **`public bool CompressOutput`**  
  When `true`, the output file is compressed (e.g., gzip). Default is `false`.

- **`public string? TargetDeviceType`**  
  Optional filter: only export data originating from devices of this type (e.g., "watch", "glucose_monitor"). `null` means no filter.

- **`public string? TargetDeviceId`**  
  Optional filter: only export data from a specific device identified by its unique ID. `null` means no filter.

- **`public DateTime? StartDate`**  
  Optional inclusive start date for the export window. `null` means no lower bound.

- **`public DateTime? EndDate`**  
  Optional inclusive end date for the export window. `null` means no upper bound.

- **`public string? NotificationEmail`**  
  Optional email address to send completion or error notifications. `null` disables notifications.

- **`public bool VerboseLogging`**  
  When `true`, enables detailed logging output. Default is `false`.

### Methods

- **`public List<string> Validate()`**  
  Validates the current configuration and returns a list of error messages. Each message describes a specific inconsistency or missing required value.  
  *Returns*: A `List<string>` containing zero or more validation error descriptions.  
  *Throws*: Never throws; returns an empty list if the configuration is valid.

- **`public string GetValidationErrors()`**  
  Returns a single string containing all validation errors, each separated by a newline. If no errors exist, returns an empty string.  
  *Returns*: A `string` with concatenated error messages.  
  *Throws*: Never throws.

- **`public bool IsValid`**  
  Gets a value indicating whether the current configuration passes all validation checks. Equivalent to `Validate().Count == 0`.  
  *Throws*: Never throws.

## Usage

### Example 1: Basic export with validation

```csharp
var options = new HealthDataExportOptions
{
    InputPath = "/data/health_records",
    OutputPath = "/exports/patient_123.csv",
    DatabasePath = "/tmp/cache.db",
    ExportFormat = ExportFormat.Csv,
    ValidateData = true,
    MaxRecordAgeDays = 365,
    CompressOutput = true,
    VerboseLogging = true
};

if (!options.IsValid)
{
    Console.Error.WriteLine("Configuration errors:");
    Console.Error.WriteLine(options.GetValidationErrors());
    return;
}

// Pass options to export engine
var exporter = new HealthDataExporter(options);
await exporter.ExportAsync();
```

### Example 2: Filtered export with analysis and notification

```csharp
var options = new HealthDataExportOptions
{
    InputPath = "https://api.healthprovider.com/v2/records",
    OutputPath = "./exports/analysis.json",
    DatabasePath = "./temp/analysis.db",
    ExportFormat = ExportFormat.Json,
    PerformAnalysis = true,
    TrendAnalysisDays = 30,
    StartDate = new DateTime(2024, 1, 1),
    EndDate = new DateTime(2024, 12, 31),
    TargetDeviceType = "watch",
    NotificationEmail = "admin@example.com"
};

var errors = options.Validate();
if (errors.Count > 0)
{
    foreach (var err in errors)
        Console.Error.WriteLine(err);
    return;
}

var exporter = new HealthDataExporter(options);
await exporter.ExportAsync();
```

## Notes

- **Edge Cases**  
  - Setting both `StartDate` and `EndDate` to `null` exports all available records (subject to `MaxRecordAgeDays`).  
  - If `PerformAnalysis` is `true` but `TrendAnalysisDays` is not set (or set to 0), validation will fail.  
  - `TargetDeviceType` and `TargetDeviceId` are independent filters; if both are set, only records matching both criteria are exported.  
  - `NotificationEmail` is not validated for format; invalid addresses will cause a runtime error during notification dispatch.  
  - `InputPath` and `OutputPath` can be local file paths or URIs; the export engine must support the scheme (e.g., `file://`, `https://`).  
  - `DatabasePath` must point to a writable location; if the file does not exist, it will be created.

- **Thread Safety**  
  Instances of `HealthDataExportOptions` are **not thread-safe**. All properties are mutable and should not be modified while an export operation is in progress. If the same options object is used concurrently, external synchronization is required. The `Validate()`, `GetValidationErrors()`, and `IsValid` methods are safe to call from multiple threads as long as no property is being written simultaneously.
