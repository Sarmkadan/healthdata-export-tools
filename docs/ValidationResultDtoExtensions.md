# ValidationResultDtoExtensions

The `ValidationResultDtoExtensions` class provides a set of static extension methods designed to simplify the analysis and reporting of validation outcomes within the `healthdata-export-tools` project. By operating on `ValidationResultDto` instances, these utilities enable developers to quickly calculate validity metrics, assess error severity, retrieve specific warning details, and generate human-readable summaries without implementing repetitive aggregation logic.

## API

### GetInvalidRate
Calculates the proportion of invalid items relative to the total count within the validation result.
*   **Parameters**: `this ValidationResultDto source`
*   **Returns**: `double` representing the ratio of invalid items (0.0 to 1.0). Returns 0.0 if the total count is zero.
*   **Throws**: `ArgumentNullException` if `source` is null.

### GetValidRate
Calculates the proportion of valid items relative to the total count within the validation result.
*   **Parameters**: `this ValidationResultDto source`
*   **Returns**: `double` representing the ratio of valid items (0.0 to 1.0). Returns 0.0 if the total count is zero.
*   **Throws**: `ArgumentNullException` if `source` is null.

### HasCriticalErrors
Determines whether the validation result contains any errors classified as critical.
*   **Parameters**: `this ValidationResultDto source`
*   **Returns**: `bool` indicating the presence of at least one critical error.
*   **Throws**: `ArgumentNullException` if `source` is null.

### GetMostSevereError
Retrieves the single error with the highest severity level from the validation result.
*   **Parameters**: `this ValidationResultDto source`
*   **Returns**: `ValidationErrorDetail?` containing the most severe error, or `null` if no errors exist.
*   **Throws**: `ArgumentNullException` if `source` is null.

### GetErrorCountByCode
Counts the occurrences of a specific error code within the validation result.
*   **Parameters**: 
    *   `this ValidationResultDto source`
    *   `string errorCode`
*   **Returns**: `int` representing the number of times the specified code appears. Returns 0 if the code is not found or the error list is empty.
*   **Throws**: `ArgumentNullException` if `source` is null.

### GetSummary
Generates a concise textual overview of the validation status, including counts of valid/invalid items and the presence of errors or warnings.
*   **Parameters**: `this ValidationResultDto source`
*   **Returns**: `string` containing the formatted summary.
*   **Throws**: `ArgumentNullException` if `source` is null.

### HasWarnings
Determines whether the validation result contains any non-blocking warnings.
*   **Parameters**: `this ValidationResultDto source`
*   **Returns**: `bool` indicating the presence of at least one warning.
*   **Throws**: `ArgumentNullException` if `source` is null.

### GetFirstWarning
Retrieves the first warning encountered in the validation result list.
*   **Parameters**: `this ValidationResultDto source`
*   **Returns**: `ValidationWarning?` containing the first warning, or `null` if no warnings exist.
*   **Throws**: `ArgumentNullException` if `source` is null.

### GetValidToInvalidRatio
Calculates the ratio of valid items to invalid items.
*   **Parameters**: `this ValidationResultDto source`
*   **Returns**: `double`. If there are no invalid items but there are valid items, returns `double.PositiveInfinity`. If both counts are zero, returns 0.0.
*   **Throws**: `ArgumentNullException` if `source` is null.

## Usage

### Example 1: Health Check and Critical Failure Detection
This example demonstrates how to quickly determine if a data export batch is safe to proceed by checking for critical errors and calculating the validity rate.

```csharp
using HealthDataExportTools.Models;
using HealthDataExportTools.Extensions;

public void ProcessExportBatch(ValidationResultDto result)
{
    if (result.HasCriticalErrors())
    {
        var severeError = result.GetMostSevereError();
        Console.WriteLine($"Processing halted: Critical error detected - {severeError?.Message}");
        return;
    }

    double validRate = result.GetValidRate();
    if (validRate < 0.95)
    {
        Console.WriteLine($"Warning: Low data quality. Valid rate: {validRate:P2}");
        Console.WriteLine(result.GetSummary());
    }
    else
    {
        Console.WriteLine("Batch validated successfully.");
    }
}
```

### Example 2: Detailed Error Analysis and Reporting
This example illustrates how to analyze specific error patterns and retrieve warning details for a diagnostic report.

```csharp
using HealthDataExportTools.Models;
using HealthDataExportTools.Extensions;

public void GenerateDiagnosticReport(ValidationResultDto result)
{
    int missingFieldCount = result.GetErrorCountByCode("ERR_MISSING_FIELD");
    int formatErrorCount = result.GetErrorCountByCode("ERR_FORMAT_INVALID");

    Console.WriteLine($"Missing Fields: {missingFieldCount}");
    Console.WriteLine($"Format Errors: {formatErrorCount}");

    if (result.HasWarnings())
    {
        var firstWarning = result.GetFirstWarning();
        Console.WriteLine($"Primary Warning: {firstWarning?.Message} (Code: {firstWarning?.Code})");
    }

    double ratio = result.GetValidToInvalidRatio();
    Console.WriteLine($"Valid to Invalid Ratio: {(double.IsInfinity(ratio) ? "Infinity (No errors)" : ratio.ToString("F2"))}");
}
```

## Notes

*   **Null Safety**: All extension methods will throw an `ArgumentNullException` if the `source` `ValidationResultDto` instance is null. Callers must ensure the object is instantiated before invoking these methods.
*   **Division by Zero**: 
    *   `GetInvalidRate` and `GetValidRate` handle empty datasets (total count = 0) by returning `0.0` rather than throwing an exception.
    *   `GetValidToInvalidRatio` returns `double.PositiveInfinity` if the invalid count is zero while the valid count is greater than zero. If both counts are zero, it returns `0.0`.
*   **Thread Safety**: As this class consists entirely of stateless static methods that operate only on the provided input parameters, it is inherently thread-safe. Multiple threads can safely invoke these extensions concurrently on different or same `ValidationResultDto` instances, provided the underlying `ValidationResultDto` object itself is not being modified simultaneously by another process.
*   **Return Values**: Methods returning nullable types (`ValidationErrorDetail?`, `ValidationWarning?`) will return `null` when the respective collections (errors or warnings) are empty, rather than throwing an exception.
