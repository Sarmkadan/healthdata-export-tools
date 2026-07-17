# HealthDataExportOptionsExtensions

Extension methods for `HealthDataExportOptions` that provide common operations, validations, and convenience methods for working with health data export configurations.

## API

### Clone

```csharp
public static HealthDataExportOptions Clone(this HealthDataExportOptions options)
```

Creates a deep copy of the `HealthDataExportOptions` instance.

**Parameters:**
- `options`: The options to copy

**Returns:**
- A new `HealthDataExportOptions` instance with the same values

**Exceptions:**
- `ArgumentNullException`: Thrown when `options` is null

**Remarks:**
The returned instance is a complete copy with all properties duplicated. Modifications to the copy will not affect the original instance.

---

### GetEffectiveInputPath

```csharp
public static string GetEffectiveInputPath(this HealthDataExportOptions options)
```

Gets the effective input path by returning `DatabasePath` if `InputPath` is empty or whitespace, otherwise returns `InputPath`.

**Parameters:**
- `options`: The options instance

**Returns:**
- The effective input path as a string

**Exceptions:**
- `ArgumentNullException`: Thrown when `options` is null

**Remarks:**
This method provides a fallback mechanism for input sources. When `InputPath` is not specified, it automatically uses `DatabasePath` as the data source.

---

### EnsureOutputDirectoryExists

```csharp
public static string EnsureOutputDirectoryExists(this HealthDataExportOptions options)
```

Gets the effective output directory and ensures it exists by creating it if necessary.

**Parameters:**
- `options`: The options instance

**Returns:**
- The effective output directory path

**Exceptions:**
- `ArgumentNullException`: Thrown when `options` is null
- `InvalidOperationException`: Thrown when `OutputPath` is not specified or cannot be created

**Remarks:**
This method will create the output directory if it does not exist. If `OutputPath` is null, empty, or whitespace, an `InvalidOperationException` is thrown.

---

### GetDateRange

```csharp
public static (DateTime? StartDate, DateTime? EndDate) GetDateRange(this HealthDataExportOptions options)
```

Gets the date range as a tuple for easy consumption.

**Parameters:**
- `options`: The options instance

**Returns:**
- A tuple containing `(StartDate, EndDate)` where null values are preserved

**Exceptions:**
- `ArgumentNullException`: Thrown when `options` is null

**Remarks:**
This method provides a convenient way to access both date boundaries simultaneously. Both values in the tuple may be null if no date range is configured.

---

### GetFileExtension

```csharp
public static string GetFileExtension(this HealthDataExportOptions options)
```

Gets the effective file extension for the configured export format.

**Parameters:**
- `options`: The options instance

**Returns:**
- The file extension including dot (e.g., `.json`, `.csv`, `.db`, `.html`, `.all`)

**Exceptions:**
- `ArgumentNullException`: Thrown when `options` is null

**Remarks:**
Returns the appropriate file extension based on the `ExportFormat` property. Defaults to `.json` for unknown or unhandled formats.

---

### GetDeviceFilterDescription

```csharp
public static string GetDeviceFilterDescription(this HealthDataExportOptions options)
```

Gets the device filter as a formatted string for logging purposes.

**Parameters:**
- `options`: The options instance

**Returns:**
- A formatted string representing the device filter (e.g., "All devices", "Device Type: Fitbit", "Device ID: device123")

**Exceptions:**
- `ArgumentNullException`: Thrown when `options` is null

**Remarks:**
This method generates a human-readable description of the device filtering configuration. It handles all combinations of `TargetDeviceType` and `TargetDeviceId` being set or null.

---

### GetAnalysisPeriod

```csharp
public static TimeSpan GetAnalysisPeriod(this HealthDataExportOptions options)
```

Gets the configured analysis period as a `TimeSpan`.

**Parameters:**
- `options`: The options instance

**Returns:**
- The analysis period as `TimeSpan`

**Exceptions:**
- `ArgumentNullException`: Thrown when `options` is null
- `InvalidOperationException`: Thrown when `TrendAnalysisDays` is negative

**Remarks:**
Converts the `TrendAnalysisDays` property to a `TimeSpan` for use in time-based operations. Validates that the value is non-negative.

---

### GetMaxRecordAge

```csharp
public static TimeSpan GetMaxRecordAge(this HealthDataExportOptions options)
```

Gets the configured maximum record age as a `TimeSpan`.

**Parameters:**
- `options`: The options instance

**Returns:**
- The maximum record age as `TimeSpan`

**Exceptions:**
- `ArgumentNullException`: Thrown when `options` is null
- `InvalidOperationException`: Thrown when `MaxRecordAgeDays` is negative

**Remarks:**
Converts the `MaxRecordAgeDays` property to a `TimeSpan` for use in time-based operations. Validates that the value is non-negative.

---

### GetOutputFilePath

```csharp
public static string GetOutputFilePath(
    this HealthDataExportOptions options,
    string baseFilename
)
```

Gets the output file path for a given base filename using the configured format.

**Parameters:**
- `options`: The options instance
- `baseFilename`: The base filename without extension

**Returns:**
- The full output file path with appropriate extension and timestamp

**Exceptions:**
- `ArgumentNullException`: Thrown when `options` or `baseFilename` is null
- `ArgumentException`: Thrown when `baseFilename` is empty or whitespace
- `InvalidOperationException`: Thrown when `OutputPath` is not specified

**Remarks:**
Constructs a complete file path by combining the output directory, base filename, current timestamp, and appropriate file extension. The timestamp format is `yyyyMMdd_HHmmss` using invariant culture.


---

### GetValidationRules

```csharp
public static IEnumerable<string> GetValidationRules(this HealthDataExportOptions options)
```

Gets all configured validation rules as an enumerable of strings.

**Parameters:**
- `options`: The options instance

**Returns:**
- An enumerable of validation rule descriptions

**Exceptions:**
- `ArgumentNullException`: Thrown when `options` is null

**Remarks:**
Returns a lazy-evaluated enumerable that yields validation rule descriptions. Each rule is a human-readable string describing a validation constraint that should be satisfied by the configuration.

## Usage

### Example 1: Cloning and modifying configuration

```csharp
var originalOptions = new HealthDataExportOptions
{
    InputPath = "/data/health",
    OutputPath = "/exports",
    ExportFormat = ExportFormat.Csv,
    MaxRecordAgeDays = 90,
    TrendAnalysisDays = 30
};

// Create a copy to modify without affecting the original
var modifiedOptions = originalOptions.Clone();
modifiedOptions.MaxRecordAgeDays = 180;
modifiedOptions.TrendAnalysisDays = 60;

// Use the modified configuration
var outputPath = modifiedOptions.GetOutputFilePath("health_export");
Console.WriteLine($"Export will be saved to: {outputPath}");
```

### Example 2: Building output file path with validation
```csharp
public string BuildExportPath(HealthDataExportOptions options, string baseName)
{
    // Ensure directory exists (creates if necessary)
    var outputDir = options.EnsureOutputDirectoryExists();
    
    // Get the full output path with timestamp and correct extension
    var filePath = options.GetOutputFilePath(baseName);
    
    // Log the configuration
    Console.WriteLine($"Exporting to: {filePath}");
    Console.WriteLine($"Device filter: {options.GetDeviceFilterDescription()}");
    Console.WriteLine($"Date range: {options.GetDateRange()}");
    
    // Check validation rules
    foreach (var rule in options.GetValidationRules())
    {
        Console.WriteLine($"Validation: {rule}");
    }
    
    return filePath;
}
```

## Notes

### Thread Safety

The extension methods are thread-safe when used with a single `HealthDataExportOptions` instance. Each method validates its input parameters and performs its operations independently. The methods do not modify the `options` parameter, only reading from it.


### Error Handling

All methods throw `ArgumentNullException` when the `options` parameter is null. Several methods throw `InvalidOperationException` when required properties are not configured or contain invalid values (e.g., negative time periods, missing output paths).


### Performance Considerations

- `Clone()`: Creates a new object with all properties copied. For configurations with many properties, this is an O(n) operation where n is the number of properties.
- `GetValidationRules()`: Returns an `IEnumerable<string>` that is lazily evaluated. The rules are only generated when iterated.
- `GetOutputFilePath()`: Includes a timestamp in the filename using `DateTime.Now`, which means multiple calls in quick succession will generate different filenames.


### Edge Cases

- When `InputPath` is empty or whitespace, `GetEffectiveInputPath()` returns `DatabasePath`
- When `TrendAnalysisDays` is 0, `GetAnalysisPeriod()` returns `TimeSpan.Zero` (valid case)
- When `MaxRecordAgeDays` is 0, `GetMaxRecordAge()` returns `TimeSpan.Zero` (valid case)
- `GetFileExtension()` returns `.json` for unknown `ExportFormat` values
- `GetDeviceFilterDescription()` returns "All devices" when both `TargetDeviceType` and `TargetDeviceId` are null or empty
- `EnsureOutputDirectoryExists()` creates parent directories as needed via `Directory.CreateDirectory()`

### Null Handling

All methods properly handle null values:
- Parameter validation with `ArgumentNullException.ThrowIfNull()`
- Null property values are handled gracefully (e.g., `GetDeviceFilterDescription()`)
- Null date values are preserved and returned (e.g., `GetDateRange()`)