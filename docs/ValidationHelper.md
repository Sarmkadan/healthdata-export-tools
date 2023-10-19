# ValidationHelper

The `ValidationHelper` class provides a centralized set of static utility methods for validating health data primitives, file system paths, and common data formats within the `healthdata-export-tools` project. It offers boolean-based validation checks for specific physiological metrics (such as heart rate, SpO2, and sleep duration) alongside assertion-style methods that throw exceptions when data constraints are violated, ensuring data integrity before export or processing operations.

## API

### Validation Methods (Boolean Return)

These methods return `true` if the input meets the specific validation criteria for the given health metric or format, and `false` otherwise. They do not throw exceptions for invalid data.

*   **`public static bool IsValidHeartRate(double? value)`**
    Validates whether a given heart rate value is within a physiologically plausible range for active or resting states. Returns `false` for null or out-of-range values.

*   **`public static bool IsValidRestingHeartRate(double? value)`**
    Validates whether a given value represents a plausible resting heart rate. This typically enforces a stricter upper bound than general heart rate validation. Returns `false` for null or out-of-range values.

*   **`public static bool IsValidSpO2(double? value)`**
    Validates whether an Oxygen Saturation (SpO2) percentage is within the valid biological range (typically 0–100%, often constrained further to realistic measurement limits like 70–100%). Returns `false` for null or invalid percentages.

*   **`public static bool IsValidSleepDuration(TimeSpan? value)`**
    Validates whether a time span represents a plausible sleep duration. Returns `false` for null, negative values, or durations exceeding a realistic maximum (e.g., 24 hours).

*   **`public static bool IsValidActivityDuration(TimeSpan? value)`**
    Validates whether a time span represents a plausible activity duration. Returns `false` for null or negative values.

*   **`public static bool IsValidRecordDate(DateTime? value)`**
    Validates whether a date is logically valid for a health record (e.g., not in the distant future). Returns `false` for null or dates failing the logical check.

*   **`public static bool IsValidEmail(string value)`**
    Validates whether a string conforms to standard email address formatting rules. Returns `false` for null, empty, or malformed strings.

*   **`public static bool IsValidFilePath(string value)`**
    Validates whether a string represents a syntactically correct file path for the current operating system. Returns `false` for null, empty, or paths containing invalid characters.

*   **`public static bool IsValidDirectoryPath(string value)`**
    Validates whether a string represents a syntactically correct directory path. Returns `false` for null, empty, or invalid directory strings.

*   **`public static bool IsValidGpsCoordinates(double? latitude, double? longitude)`**
    Validates whether the provided latitude and longitude fall within valid geographic ranges (-90 to 90 for latitude, -180 to 180 for longitude). Returns `false` if either coordinate is null or out of range.

*   **`public static bool IsValidDistance(double? value)`**
    Validates whether a distance value is non-negative and within a reasonable magnitude. Returns `false` for null or negative values.

*   **`public static bool IsValidPercentage(double? value)`**
    Validates whether a value falls strictly within the 0.0 to 100.0 range. Returns `false` for null or values outside this interval.

*   **`public static bool IsValidPercentageExtended(double? value)`**
    Validates a percentage value that may allow for extended ranges (e.g., exceeding 100% in specific calculation contexts) while still enforcing non-negativity or other specific bounds. Returns `false` for null or invalid values.

*   **`public static bool IsValidElevation(double? value)`**
    Validates whether an elevation value is within realistic terrestrial limits. Returns `false` for null or extreme outliers.

*   **`public static bool IsValidTemperature(double? value)`**
    Validates whether a temperature reading is within a plausible range for human health data. Returns `false` for null or biologically impossible values.

*   **`public static bool IsValidCalories(double? value)`**
    Validates whether a calorie count is non-negative and within a reasonable limit for a single record. Returns `false` for null or negative values.

*   **`public static List<string> ValidateHeartRateData(double? value)`**
    Performs a detailed validation of heart rate data and returns a list of error messages. If the data is valid, an empty list is returned. If invalid, the list contains one or more strings describing the validation failures.

### Assertion Methods (Void Return)

These methods perform validation and throw an exception immediately if the condition is not met. They return `void` on success.

*   **`public static void EnsureInRange<T>(T value, T min, T max, string paramName)`**
    Ensures that a comparable value falls within the specified inclusive range `[min, max]`.
    *   **Parameters**: `value` (the value to check), `min` (lower bound), `max` (upper bound), `paramName` (name of the parameter for the exception message).
    *   **Throws**: `ArgumentOutOfRangeException` if `value` is less than `min` or greater than `max`.

*   **`public static void EnsureNotNull<T>(T value, string paramName)`**
    Ensures that a reference or nullable value is not null.
    *   **Parameters**: `value` (the object to check), `paramName` (name of the parameter for the exception message).
    *   **Throws**: `ArgumentNullException` if `value` is null.

*   **`public static void EnsureNotEmpty(string value, string paramName)`**
    Ensures that a string is neither null nor empty.
    *   **Parameters**: `value` (the string to check), `paramName` (name of the parameter for the exception message).
    *   **Throws**: `ArgumentException` (or `ArgumentNullException`) if `value` is null or `string.Empty`.

## Usage

### Example 1: Validating Health Metrics Before Export
This example demonstrates using the boolean validation methods to filter records before adding them to an export batch.

```csharp
using HealthDataExportTools;

public void ProcessHealthRecord(double? heartRate, double? spo2, DateTime recordedAt)
{
    if (!ValidationHelper.IsValidRecordDate(recordedAt))
    {
        Console.WriteLine("Skipping record due to invalid date.");
        return;
    }

    if (heartRate.HasValue && !ValidationHelper.IsValidHeartRate(heartRate))
    {
        Console.WriteLine($"Invalid heart rate detected: {heartRate}");
        // Optionally log detailed errors
        var errors = ValidationHelper.ValidateHeartRateData(heartRate);
        foreach (var error in errors)
        {
            Console.WriteLine($" - {error}");
        }
        return;
    }

    if (spo2.HasValue && !ValidationHelper.IsValidSpO2(spo2))
    {
        Console.WriteLine("Invalid SpO2 reading; excluding from export.");
        return;
    }

    // Proceed with export logic
    ExportRecord(heartRate, spo2, recordedAt);
}
```

### Example 2: Enforcing Constraints with Assertions
This example uses the `Ensure` methods to validate method arguments at the entry point of a service operation, failing fast if inputs are invalid.

```csharp
using HealthDataExportTools;
using System.IO;

public void ExportToFile(string filePath, double distanceKm, int calories)
{
    // Validate arguments immediately
    ValidationHelper.EnsureNotEmpty(filePath, nameof(filePath));
    ValidationHelper.EnsureNotNull(filePath, nameof(filePath)); // Redundant if EnsureNotEmpty handles null, but explicit for clarity
    
    if (!ValidationHelper.IsValidFilePath(filePath))
    {
        throw new ArgumentException("The provided path is not a valid file path format.", nameof(filePath));
    }

    ValidationHelper.EnsureInRange(distanceKm, 0.0, 1000.0, nameof(distanceKm));
    ValidationHelper.EnsureInRange(calories, 0, 50000, nameof(calories));

    // If no exceptions were thrown, proceed safely
    File.WriteAllText(filePath, $"Distance: {distanceKm}km, Calories: {calories}");
}
```

## Notes

*   **Nullable Handling**: All boolean validation methods accepting numeric types (`double?`, `int?`) or `DateTime?` explicitly handle `null` inputs by returning `false`. They do not throw exceptions for null values; null is treated as an invalid state.
*   **Thread Safety**: As `ValidationHelper` consists entirely of static methods that operate solely on input parameters without maintaining internal mutable state, it is inherently thread-safe and can be used concurrently across multiple threads without synchronization.
*   **Exception Behavior**: The `Ensure` methods (`EnsureInRange`, `EnsureNotNull`, `EnsureNotEmpty`) are designed to fail fast. They should be used for guarding public API inputs or critical internal assumptions, whereas the `IsValid` methods are suitable for data filtering and user feedback scenarios where exceptions would be disruptive.
*   **Culture Sensitivity**: Validation logic for numeric ranges assumes standard numerical values. File path validation (`IsValidFilePath`, `IsValidDirectoryPath`) adheres to the path format of the host operating system where the code is executed.
