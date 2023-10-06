# ValidationResultDto

`ValidationResultDto` is a data transfer object that encapsulates the outcome of a validation operation performed on a set of records. It aggregates overall validity status, record counts, performance metrics, and detailed error and warning information. The type is designed to be serialized and consumed by reporting or monitoring components.

## API

### ValidationResultDto Properties

| Member | Type | Description |
|--------|------|-------------|
| `ValidationId` | `string` | A unique identifier for the validation run. |
| `Timestamp` | `DateTime` | The date and time when the validation completed. |
| `IsValid` | `bool` | Indicates whether the entire validation passed (no errors). |
| `TotalRecords` | `int` | The total number of records that were processed. |
| `ValidRecords` | `int` | The number of records that passed all validation rules. |
| `InvalidRecords` | `int` | The number of records that failed at least one validation rule. |
| `ValidationErrors` | `List<ValidationErrorDetail>` | A list of detailed error objects describing each validation failure. |
| `Warnings` | `List<ValidationWarning>` | A list of warning objects for non‑critical issues. |
| `Statistics` | `ValidationStatistics` | An object containing aggregated statistical data about the validation run. |
| `DurationMs` | `double` | The total time taken for the validation, in milliseconds. |

### ValidationResultDto Methods

| Member | Return Type | Description |
|--------|-------------|-------------|
| `GetSuccessRate` | `double` | Returns the success rate as a value between 0.0 and 1.0, calculated as `ValidRecords / TotalRecords`. Throws `DivideByZeroException` if `TotalRecords` is zero. |
| `GetErrorCount` | `int` | Returns the total number of validation errors (the count of items in `ValidationErrors`). |
| `GetWarningCount` | `int` | Returns the total number of warnings (the count of items in `Warnings`). |

### ValidationErrorDetail

Each item in `ValidationErrors` exposes the following members:

| Member | Type | Description |
|--------|------|-------------|
| `RecordIndex` | `int` | The zero‑based index of the record that caused the error. |
| `Field` | `string` | The name of the field that failed validation. |
| `Value` | `string?` | The actual value that was present in the field (may be null). |
| `ErrorCode` | `string` | A machine‑readable code identifying the validation rule that was violated. |
| `Message` | `string` | A human‑readable description of the error. |
| `Severity` | `string` | The severity level of the error (e.g., "Error", "Critical"). |
| `Create` | `static ValidationErrorDetail` | Factory method that creates a new `ValidationErrorDetail` instance. Parameters and behavior are defined by the implementation. |

### ValidationWarning

Each item in `Warnings` exposes the same members as `ValidationErrorDetail` (RecordIndex, Field, Value, ErrorCode, Message, Severity, and the static `Create` method). Warnings represent non‑fatal issues that do not cause the overall validation to fail.

## Usage

### Example 1: Inspecting validation results

```csharp
ValidationResultDto result = PerformValidation(data);

if (!result.IsValid)
{
    Console.WriteLine($"Validation failed. Errors: {result.GetErrorCount()}, Warnings: {result.GetWarningCount()}");
    Console.WriteLine($"Success rate: {result.GetSuccessRate():P1}");

    foreach (var error in result.ValidationErrors)
    {
        Console.WriteLine($"Record {error.RecordIndex}, field '{error.Field}': {error.Message} (code: {error.ErrorCode})");
    }
}
```

### Example 2: Serializing the result for logging

```csharp
var result = new ValidationResultDto
{
    ValidationId = Guid.NewGuid().ToString(),
    Timestamp = DateTime.UtcNow,
    TotalRecords = 1000,
    ValidRecords = 985,
    InvalidRecords = 15,
    DurationMs = 234.5,
    ValidationErrors = new List<ValidationErrorDetail>
    {
        ValidationErrorDetail.Create(42, "Email", "invalid@", "INVALID_EMAIL", "Email format is invalid", "Error")
    },
    Warnings = new List<ValidationWarning>()
};

string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText("validation_report.json", json);
```

## Notes

- **Edge Cases**  
  - If `TotalRecords` is zero, calling `GetSuccessRate()` throws a `DivideByZeroException`. Always check `TotalRecords > 0` before invoking this method, or handle the exception.  
  - `GetErrorCount()` and `GetWarningCount()` are safe to call even when the respective lists are null or empty; they return 0 in those cases (the lists are initialized by the constructor).  
  - The `Value` property in `ValidationErrorDetail` and `ValidationWarning` can be `null` when the field was missing or not applicable.

- **Thread Safety**  
  `ValidationResultDto` and its nested types (`ValidationErrorDetail`, `ValidationWarning`, `ValidationStatistics`) are not thread‑safe. Their properties are mutable and intended for single‑threaded construction and consumption. If instances must be shared across threads, external synchronization (e.g., locking or defensive copying) is required.

- **Immutability**  
  The type does not enforce immutability. Consumers should treat instances as read‑only after construction to avoid unintended side effects. The factory method `Create` on `ValidationErrorDetail` and `ValidationWarning` is provided to encourage consistent construction.
