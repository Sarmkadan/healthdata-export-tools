# ChartExportServiceTests

Unit tests for `ChartExportService`, verifying HTML chart generation behavior across various data scenarios and configuration options.

## API

### `ChartExportServiceTests`
Constructor for the test class. Initializes the test fixture with required dependencies and test data.

### `Dispose`
Releases any unmanaged resources used by the test class. Inherited from `IDisposable`.

### `ExportToHtmlChartsAsync_ShouldGenerateValidHtml`
Verifies that the `ExportToHtmlChartsAsync` method produces valid HTML output when provided with standard data.

- **Parameters**: None
- **Return value**: `Task` (asynchronous completion)
- **Throws**: Does not throw under normal test conditions.

### `ExportToHtmlChartsAsync_ShouldHandleEmptyCollection`
Ensures that the service handles empty data collections gracefully without throwing exceptions.

- **Parameters**: None
- **Return value**: `Task` (asynchronous completion)
- **Throws**: Does not throw when the input collection is empty.

### `ExportToHtmlChartsAsync_WithSpO2Records_ShouldIncludeSpO2Chart`
Confirms that SpO2 records are correctly rendered as a chart in the output when present.

- **Parameters**: None
- **Return value**: `Task` (asynchronous completion)
- **Throws**: Fails if the SpO2 chart is missing from the output.

### `ExportToHtmlChartsAsync_WithOptions_ShouldRespectDisabledCharts`
Validates that chart generation respects the `ExportOptions` configuration, omitting charts marked as disabled.

- **Parameters**: None
- **Return value**: `Task` (asynchronous completion)
- **Throws**: Fails if a disabled chart is present in the output.

### `ExportToHtmlChartsAsync_WithSleepData_ShouldIncludeSleepCompositionChart`
Asserts that sleep data is rendered as a composition chart when provided.

- **Parameters**: None
- **Return value**: `Task` (asynchronous completion)
- **Throws**: Fails if the sleep composition chart is missing.

### `ExportToHtmlChartsAsync_WithAllData_ShouldIncludeSummaryTable`
Checks that a summary table is included in the output when all data types are present.

- **Parameters**: None
- **Return value**: `Task` (asynchronous completion)
- **Throws**: Fails if the summary table is absent from the generated HTML.

## Usage

### Basic Test Setup
