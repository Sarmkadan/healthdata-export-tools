# CsvExportOptionsExtensions

Provides a set of fluent extension methods for configuring `CsvExportOptions` instances used by the health data export utilities. These methods enable concise, readable construction of export settings such as toggling all data types, specifying date formats, and limiting exported columns.

## API

### EnableAllDataTypes
```csharp
public static CsvExportOptions EnableAllDataTypes(this CsvExportOptions options)
```
**Purpose** – Returns a copy of the supplied options with every supported data type enabled for export.  
**Parameters** – `options`: The `CsvExportOptions` instance to configure; must not be `null`.  
**Return value** – A new `CsvExportOptions` object with all data type flags set to `true`.  
**Exceptions** – Throws `ArgumentNullException` if `options` is `null`.

### DisableAllDataTypes
```csharp
public static CsvExportOptions DisableAllDataTypes(this CsvExportOptions options)
```
**Purpose** – Returns a copy of the supplied options with every supported data type disabled for export.  
**Parameters** – `options`: The `CsvExportOptions` instance to configure; must not be `null`.  
**Return value** – A new `CsvExportOptions` object with all data type flags set to `false`.  
**Exceptions** – Throws `ArgumentNullException` if `options` is `null`.

### WithDateFormat
```csharp
public static CsvExportOptions WithDateFormat(this CsvExportOptions options, string dateFormat)
```
**Purpose** – Returns a copy of the supplied options with the date format string applied to all date‑time fields in the exported CSV.  
**Parameters** –  
- `options`: The `CsvExportOptions` instance to configure; must not be `null`.  
- `dateFormat`: A .NET custom or standard date format string (e.g., `"yyyy-MM-dd"`); must not be `null`, empty, or consist only of whitespace.  
**Return value** – A new `CsvExportOptions` object whose `DateFormat` property is set to the supplied value.  
**Exceptions** –  
- `ArgumentNullException` if `options` or `dateFormat` is `null`.  
- `ArgumentException` if `dateFormat` is empty, whitespace, or an invalid format string.

### WithColumns
```csharp
public static CsvExportOptions WithColumns(this CsvExportOptions options, params string[] columnNames)
```
**Purpose** – Returns a copy of the supplied options that limits the exported CSV to the specified column names, preserving their order.  
**Parameters** –  
- `options`: The `CsvExportOptions` instance to configure; must not be `null`.  
- `columnNames`: One or more column names to include; each name must be a non‑null, non‑empty string. Duplicate names are allowed but will be treated as a single column.  
**Return value** – A new `CsvExportOptions` object whose `Columns` collection is set to the provided names.  
**Exceptions** –  
- `ArgumentNullException` if `options` is `null` or `columnNames` is `null`.  
- `ArgumentException` if any element in `columnNames` is `null`, empty, or consists only of whitespace.

## Usage

```csharp
// Example 1: Export all data with a custom date format and a specific column order.
var options = new CsvExportOptions()
    .EnableAllDataTypes()
    .WithDateFormat("yyyy-MM-dd")
    .WithColumns("PatientId", "EncounterDate", "ResultValue", "Units");

// Example 2: Start with no data types enabled, then enable only a subset.
var options = new CsvExportOptions()
    .DisableAllDataTypes()
    .WithColumns("PatientId", "TestName")
    .EnableAllDataTypes(); // re‑enables all types; alternatively, call specific enable methods if available
```

## Notes

- All extension methods treat the input `CsvExportOptions` as immutable; they never modify the original instance but instead return a new instance with the requested changes applied. This permits safe chaining and reuse of base configurations.
- Because the methods do not access any static or shared mutable state, they are thread‑safe when called concurrently on distinct `CsvExportOptions` instances. Supplying the same instance to multiple threads is safe as long as the instance is not mutated elsewhere; the methods themselves will not cause race conditions.
- Passing `null` for the `options` argument results in an `ArgumentNullException`. Likewise, supplying invalid arguments for `dateFormat` or `columnNames` triggers the appropriate exceptions as described.
- The order of method calls does not affect the final state except where later calls override earlier ones (e.g., calling `DisableAllDataTypes` after `EnableAllDataTypes` will result in all types being disabled). Users should arrange calls to reflect the intended final configuration.
