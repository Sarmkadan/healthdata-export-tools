# ExportResultDto

A data transfer object used to encapsulate the results of an export operation in the health data export pipeline. It contains metadata about the export process, including identifiers, timing, record counts, generated files, warnings, errors, and compression details.

## API

### Properties

#### `ExportId`
- **Purpose**: A unique identifier for the export operation.
- **Type**: `string`
- **Remarks**: Always non-null and non-empty after export initiation.

#### `Status`
- **Purpose**: Describes the final state of the export (e.g., "Completed", "Failed").
- **Type**: `string`
- **Remarks**: Populated after export completion or failure.

#### `RecordsExported`
- **Purpose**: The total number of records included in the export.
- **Type**: `int`
- **Remarks**: Non-negative; reflects the count after filtering.

#### `RecordsFiltered`
- **Purpose**: The number of records that passed any applied filters.
- **Type**: `int`
- **Remarks**: Less than or equal to `RecordsExported`; zero if no filtering occurred.

#### `OutputPath`
- **Purpose**: The filesystem path to the primary export output (if applicable).
- **Type**: `string`
- **Remarks**: May be null or empty if no single output file was generated.

#### `OutputSizeBytes`
- **Purpose**: The size in bytes of the primary export output.
- **Type**: `long`
- **Remarks**: Zero if no output file exists.

#### `GeneratedFiles`
- **Purpose**: A list of files produced by the export operation.
- **Type**: `List<ExportedFile>`
- **Remarks**: Empty if no files were generated.

#### `ExportedFormats`
- **Purpose**: The data formats included in the export (e.g., "CSV", "NDJSON").
- **Type**: `List<string>`
- **Remarks**: May be empty if format inference failed.

#### `StartTime`
- **Purpose**: The UTC timestamp when the export began.
- **Type**: `DateTime`
- **Remarks**: Always set; reflects system clock at operation start.

#### `EndTime`
- **Purpose**: The UTC timestamp when the export completed or failed.
- **Type**: `DateTime`
- **Remarks**: Always set; later than `StartTime` unless export failed immediately.

#### `DeviceTypes`
- **Purpose**: The types of devices contributing data to the export.
- **Type**: `List<string>`
- **Remarks**: May be empty if device metadata was unavailable.

#### `DataTypes`
- **Purpose**: The categories of health data included (e.g., "Observations", "Patients").
- **Type**: `List<string>`
- **Remarks**: May be empty if type inference failed.

#### `Warnings`
- **Purpose**: Non-fatal issues encountered during export.
- **Type**: `List<string>`
- **Remarks**: May be empty; safe to ignore unless investigation is needed.

#### `Errors`
- **Purpose**: Fatal issues that prevented successful export.
- **Type**: `List<string>`
- **Remarks**: Empty if export succeeded; otherwise, contains one or more error messages.

#### `IsCompressed`
- **Purpose**: Indicates whether the export output was compressed.
- **Type**: `bool`
- **Remarks**: True if compression was applied, even if no output file exists.

#### `CompressionRatio`
- **Purpose**: The ratio of compressed size to original size (if compressed).
- **Type**: `double?`
- **Remarks**: Null if not compressed or compression failed; otherwise, a positive value.

#### `GetHumanReadableSize()`
- **Purpose**: Returns a human-readable string representation of `OutputSizeBytes`.
- **Returns**: `string` – e.g., "1.2 MB".
- **Remarks**: Returns "0 B" if `OutputSizeBytes` is zero.

#### `FileName`
- **Purpose**: The name of the primary export file (without path).
- **Type**: `string`
- **Remarks**: May be null or empty if no file was generated.

#### `FilePath`
- **Purpose**: The full filesystem path to the primary export file.
- **Type**: `string`
- **Remarks**: May be null or empty if no file was generated.

#### `Format`
- **Purpose**: The primary format of the export output.
- **Type**: `string`
- **Remarks**: May be null or empty if format could not be determined.

## Usage

### Example 1: Successful Export
