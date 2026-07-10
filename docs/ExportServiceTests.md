# ExportServiceTests

Unit tests for the `ExportService` class, verifying correct behavior when exporting health data to JSON and CSV formats. The test suite covers happy paths, edge cases, and directory handling scenarios to ensure the export functionality behaves as expected under various conditions.

## API

### `ExportServiceTests`
Constructor for the test class. Initializes a new instance of the `ExportServiceTests` class with required dependencies and test context.

### `Dispose`
Releases all resources used by the current test instance. Called automatically by the test framework after each test execution.

### `ExportToJsonAsync_ShouldCreateValidJsonFile`
Verifies that exporting a non-empty collection to JSON produces a valid JSON file with correct content structure.

- **Parameters**: None
- **Return value**: `Task` (asynchronous test)
- **Throws**: Standard test assertion exceptions when file content or structure is invalid

### `ExportToJsonAsync_ShouldHandleEmptyCollection`
Ensures that exporting an empty collection to JSON results in a valid, well-formed JSON file (typically an empty array).

- **Parameters**: None
- **Return value**: `Task` (asynchronous test)
- **Throws**: Standard test assertion exceptions when file content is not an empty array

### `ExportSleepToCsvAsync_ShouldCreateValidCsvFile`
Validates that exporting sleep data to CSV produces a correctly formatted CSV file with expected headers and data rows.

- **Parameters**: None
- **Return value**: `Task` (asynchronous test)
- **Throws**: Standard test assertion exceptions when CSV format or content is incorrect

### `ExportCompleteAsync_ShouldExportAllFormatsWhenSpecified`
Confirms that invoking the complete export with multiple formats specified generates all requested output files (e.g., JSON and CSV) in the correct directory.

- **Parameters**: None
- **Return value**: `Task` (asynchronous test)
- **Throws**: Standard test assertion exceptions when any expected file is missing or malformed

### `ExportCompleteAsync_ShouldCreateOutputDirectoryIfNotExists`
Ensures that the export service creates the target output directory if it does not already exist.

- **Parameters**: None
- **Return value**: `Task` (asynchronous test)
- **Throws**: Standard test assertion exceptions when directory is not created or permissions prevent creation

### `ExportHeartRateToCsvAsync_ShouldCreateValidCsvFile`
Checks that exporting heart rate data to CSV produces a valid CSV file with correct headers and data integrity.

- **Parameters**: None
- **Return value**: `Task` (asynchronous test)
- **Throws**: Standard test assertion exceptions when CSV content or structure is invalid

### `ExportStepsToCsvAsync_ShouldCreateValidCsvFile`
Validates that exporting step count data to CSV results in a properly formatted CSV file with expected headers and data rows.

- **Parameters**: None
- **Return value**: `Task` (asynchronous test)
- **Throws**: Standard test assertion exceptions when CSV format or content is incorrect

## Usage
