# DataComparisonServiceTests

`DataComparisonServiceTests` is the unit test suite for the `DataComparisonService` class in the `healthdata-export-tools` project. It validates the correctness of health data comparison logic across multiple time periods, covering percentage calculations, edge cases with empty datasets, specialized metric computations (SpO₂, activity, deep sleep), narrative summary generation, date-range partitioning, and JSON export functionality.

## API

### public DataComparisonServiceTests

Default constructor. Initializes a new instance of the test class. The test framework instantiates this class before each test method execution to ensure a clean state.

### public async Task ComparePeriodsAsync_ShouldCalculatePercentageCorrectly

**Purpose:** Verifies that `ComparePeriodsAsync` computes percentage changes between two periods with standard, non-empty input data.

**Parameters:** None (parameterless test method).

**Return value:** `Task` representing the asynchronous test operation.

**Throws:** Assertion failures if the calculated percentage deviates from the expected value or if the method under test throws an unexpected exception.

### public async Task ComparePeriodsAsync_WithEmptyPeriods_ShouldReturnZeroes

**Purpose:** Ensures that `ComparePeriodsAsync` handles empty period inputs gracefully by returning zero-valued results rather than throwing exceptions or producing undefined values.

**Parameters:** None.

**Return value:** `Task`.

**Throws:** Assertion failures if the result is non-zero, null, or if an exception propagates from the service method.

### public async Task ComparePeriodsAsync_WithSpO2Data_ShouldCalculateSpO2Change

**Purpose:** Confirms that the comparison service correctly derives SpO₂ (blood oxygen saturation) change metrics when input periods contain relevant SpO₂ readings.

**Parameters:** None.

**Return value:** `Task`.

**Throws:** Assertion failures if the SpO₂ change value does not match the expected calculation or if the service fails to process valid SpO₂ data.

### public async Task ComparePeriodsAsync_WithActivityData_ShouldCalculateActivityChange

**Purpose:** Validates that activity-related metrics (e.g., steps, movement) are correctly compared and the resulting change value is accurate when activity data is present in both periods.

**Parameters:** None.

**Return value:** `Task`.

**Throws:** Assertion failures on incorrect activity change computation or unexpected service errors.

### public async Task ComparePeriodsAsync_ShouldPopulateNarrativeSummary

**Purpose:** Ensures that the comparison result includes a populated, human-readable narrative summary describing the differences between the two periods.

**Parameters:** None.

**Return value:** `Task`.

**Throws:** Assertion failures if the narrative summary is null, empty, or lacks expected descriptive content.

### public async Task CompareByDateRangeAsync_ShouldPartitionRecordsCorrectly

**Purpose:** Tests that `CompareByDateRangeAsync` correctly partitions health records into the specified date ranges, assigning each record to the appropriate period based on its timestamp.

**Parameters:** None.

**Return value:** `Task`.

**Throws:** Assertion failures if records are miscategorized across date boundaries or if the partitioning logic omits or duplicates records.

### public async Task ExportToJsonAsync_ShouldWriteValidJsonFile

**Purpose:** Verifies that `ExportToJsonAsync` produces a well-formed JSON file on disk containing the comparison results, and that the file content matches the expected serialized structure.

**Parameters:** None.

**Return value:** `Task`.

**Throws:** Assertion failures if the file is not created, contains invalid JSON, or the JSON structure does not represent the comparison output correctly.

### public async Task ComparePeriodsAsync_WithDeepSleepData_ShouldCalculateDeepSleepChange

**Purpose:** Validates that deep sleep duration changes are accurately computed when the input periods include deep sleep stage data.

**Parameters:** None.

**Return value:** `Task`.

**Throws:** Assertion failures if the deep sleep change metric is incorrect or if the service mishandles valid sleep-stage data.

## Usage

```csharp
// Example 1: Running the full test suite via a test runner
// This executes all documented test methods against DataComparisonService.
[TestClass]
public class DataComparisonServiceTestRun
{
    [TestMethod]
    public async Task RunAllComparisonTests()
    {
        var tests = new DataComparisonServiceTests();

        await tests.ComparePeriodsAsync_ShouldCalculatePercentageCorrectly();
        await tests.ComparePeriodsAsync_WithEmptyPeriods_ShouldReturnZeroes();
        await tests.ComparePeriodsAsync_WithSpO2Data_ShouldCalculateSpO2Change();
        await tests.ComparePeriodsAsync_WithActivityData_ShouldCalculateActivityChange();
        await tests.ComparePeriodsAsync_ShouldPopulateNarrativeSummary();
        await tests.CompareByDateRangeAsync_ShouldPartitionRecordsCorrectly();
        await tests.ExportToJsonAsync_ShouldWriteValidJsonFile();
        await tests.ComparePeriodsAsync_WithDeepSleepData_ShouldCalculateDeepSleepChange();
    }
}
```

```csharp
// Example 2: Selective execution focusing on export and edge-case validation
[TestClass]
public class FocusedComparisonTests
{
    [TestMethod]
    public async Task ValidateExportAndEmptyInputHandling()
    {
        var tests = new DataComparisonServiceTests();

        // Verify JSON export integrity
        await tests.ExportToJsonAsync_ShouldWriteValidJsonFile();

        // Ensure robustness against empty data sets
        await tests.ComparePeriodsAsync_WithEmptyPeriods_ShouldReturnZeroes();
    }
}
```

## Notes

- **Edge cases:** The test `ComparePeriodsAsync_WithEmptyPeriods_ShouldReturnZeroes` explicitly covers the scenario where one or both periods contain no health records. The expected behavior is a zero-valued result rather than an exception. Other tests implicitly cover boundary conditions such as single-record periods, identical periods (zero change), and periods with only one metric type present.

- **Thread safety:** All test methods are `async Task`-returning members designed for sequential execution within a test framework. They do not share mutable state across instances; the test class is stateless aside from any internal test-context setup performed by the framework. No concurrent access concerns arise under normal test runner operation. If executed in parallel via a custom harness, each test should operate on its own `DataComparisonServiceTests` instance to avoid potential interference from shared fixture state.
