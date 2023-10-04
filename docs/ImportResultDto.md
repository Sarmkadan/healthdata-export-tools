# ImportResultDto

DTO representing the result of an import operation, containing metadata, statistics, validation outcomes, and error information for downstream processing or display.

## API

### Properties

- **`ImportId`** (string)
  Unique identifier for the import operation. Used to correlate results with the originating request or job.

- **`Status`** (string)
  Human-readable status of the import (e.g., "Completed", "Failed", "PartiallyCompleted"). Not guaranteed to be machine-parsable; use structured fields like `RecordsImported` for programmatic checks.

- **`RecordsImported`** (int)
  Total number of records processed during the import. Always non-negative. May exceed `RecordsValidated` if validation occurs post-import.

- **`RecordsValidated`** (int)
  Number of records subjected to validation logic. Will not exceed `RecordsImported`. Useful for gauging validation coverage.

- **`RecordsRejected`** (int)
  Count of records explicitly rejected due to validation failures or other criteria. Derived from `ValidationErrors`; may be zero even if errors exist if rejection logic is deferred.

- **`ImportSource`** (string)
  Origin of the import payload (e.g., file path, URI, database table name). Intended for logging and audit trails.

- **`DeviceType`** (string)
  Type or model identifier of the source device generating the import data. Optional; may be null or empty if not applicable.

- **`DataTypes`** (List<string>)
  List of data type identifiers present in the import (e.g., "Patient", "Observation"). Never null; empty list indicates no recognized types.

- **`StartTime`** (DateTime)
  Timestamp marking the beginning of the import operation. Must be a valid DateTime in UTC or local time as per system configuration.

- **`EndTime`** (DateTime)
  Timestamp marking the conclusion of the import operation. Must be equal to or later than `StartTime`.

- **`ValidationErrors`** (List<ValidationError>)
  Collection of structured validation failures encountered during import. Never null; empty list indicates no validation errors. Each `ValidationError` contains `RecordIndex`, `ErrorType`, `Message`, and `Severity`.

- **`Warnings`** (List<string>)
  Non-fatal advisories generated during import (e.g., schema deviations, deprecated fields). Never null; empty list indicates no warnings.

- **`DateRange`** (DateRangeInfo)
  Temporal bounds of the data contained within the import. May be null if the dataset lacks date information.

- **`Statistics`** (ImportStatistics)
  Aggregated metrics and analytics about the import (e.g., record counts by type, processing duration). Never null; initialized with default values if no statistics are available.

- **`GetSuccessRate()`** (double)
  Computes the success rate as `(RecordsImported - RecordsRejected) / RecordsImported`, clamped to [0.0, 1.0]. Returns 1.0 if `RecordsImported` is zero. No exceptions raised.

- **`RecordIndex`** (int)
  Zero-based index of a specific record within the import, used when referencing individual records in `ValidationErrors` or `Warnings`. Negative values indicate absence or invalid index.

- **`ErrorType`** (string)
  Category of error (e.g., "SchemaViolation", "DataFormatError"). Used alongside `Message` and `Severity` in `ValidationError` entries.

- **`Message`** (string)
  Human-readable description of a validation error or warning. May be empty if context is implicit.

- **`Severity`** (string)
  Criticality level of a message (e.g., "Error", "Warning", "Info"). Used to prioritize user attention.

- **`StartDate`** (DateTime?)
  Earliest date present in the imported data, if applicable. Null if no date information is available or the dataset is empty.

## Usage

```csharp
// Example 1: Successful import with warnings
var result = new ImportResultDto
{
    ImportId = "imp-2024-001",
    Status = "Completed",
    RecordsImported = 1500,
    RecordsValidated = 1500,
    RecordsRejected = 0,
    ImportSource = "/data/patients_20240501.ndjson",
    DataTypes = new List<string> { "Patient", "Observation" },
    StartTime = DateTime.Parse("2024-05-01T08:00:00Z"),
    EndTime = DateTime.Parse("2024-05-01T08:05:33Z"),
    ValidationErrors = new List<ValidationError>(),
    Warnings = new List<string> { "Patient ID format differs from standard" },
    DateRange = new DateRangeInfo
    {
        Start = DateTime.Parse("2024-01-01T00:00:00Z"),
        End = DateTime.Parse("2024-05-01T00:00:00Z")
    },
    Statistics = new ImportStatistics
    {
        RecordCountsByType = new Dictionary<string, int> { ["Patient"] = 1200, ["Observation"] = 300 }
    }
};

double successRate = result.GetSuccessRate(); // 1.0
```

```csharp
// Example 2: Failed import with detailed errors
var error = new ValidationError
{
    RecordIndex = 42,
    ErrorType = "SchemaViolation",
    Message = "Missing required field 'birthDate'",
    Severity = "Error"
};

var failedResult = new ImportResultDto
{
    ImportId = "imp-2024-002",
    Status = "Failed",
    RecordsImported = 100,
    RecordsValidated = 100,
    RecordsRejected = 1,
    ImportSource = "https://api.example.com/imports/1002",
    DataTypes = new List<string> { "Patient" },
    StartTime = DateTime.Parse("2024-05-02T09:15:00Z"),
    EndTime = DateTime.Parse("2024-05-02T09:16:14Z"),
    ValidationErrors = new List<ValidationError> { error },
    Warnings = new List<string>(),
    DateRange = null,
    Statistics = new ImportStatistics()
};

double failedRate = failedResult.GetSuccessRate(); // 0.99
```

## Notes

- All properties are thread-safe for concurrent reads. Concurrent modifications are not supported; treat the DTO as immutable after construction.
- `ValidationErrors`, `Warnings`, and `DataTypes` are initialized as empty lists to avoid null references.
- `StartTime` and `EndTime` are expected to be set by the import pipeline; invalid temporal relationships (e.g., `EndTime < StartTime`) may indicate system clock issues but are not enforced by this type.
- `GetSuccessRate()` performs a simple arithmetic operation and will not overflow under normal usage (assumes `RecordsImported` ≤ 2,147,483,647).
- `RecordIndex` is informational; it may refer to a logical position rather than a physical one depending on the import source.
