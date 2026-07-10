# TrendAnomalyDetectionServiceTests

Unit test class for the `TrendAn overview of what it is for  
`TrendAnomalyDetectionServiceTests` contains the unit‑test suite for the `TrendAnomalyDetectionService` component in the **healthdata-export-tools** project. The tests verify that the service correctly computes trends, detects anomalies, and handles various edge‑case input scenarios such as insufficient data, flat series, and error conditions.

## API  

### `public TrendAnomalyDetectionServiceTests()`  
* **Purpose** – Parameter‑less constructor used by the test framework to instantiate the test class.  
* **Parameters** – None.  
* **Return value** – None (constructor).  
* **Throws** – None under normal circumstances; any exception would indicate a test‑framework setup problem.

### `public void ComputeTrendAndAnomalies_ShouldReturnInsufficientDataForFewPoints()`  
* **Purpose** – Verifies that the service returns an *insufficient data* result when fewer than the required number of samples are supplied.  
* **Parameters** – None.  
* **Return value** – `void`.  
* **Throws** – Throws an assertion exception (e.g., `NUnit.Framework.AssertionException`) if the service does not return the expected insufficient‑data outcome.

### `public void ComputeTrendAndAnomalies_ShouldDetectStableTrend()`  
* **Purpose** – Confirms that a series with no meaningful change is classified as a stable trend.  
* **Parameters** – None.  
* **Return value** – `void`.  
* **Throws** – Throws an assertion exception if the reported trend is not `Stable`.

### `public void ComputeTrendAndAnomalies_ShouldDetectImprovingTrend()`  
* **Purpose** – Ensures an upward‑moving series is identified as an improving trend.  
* **Parameters** – None.  
* **Return value** – `void`.  
* **Throws** – Throws an assertion exception if the trend is not reported as `Improving`.

### `public void ComputeTrendAndAnomalies_ShouldDetectDecliningTrend()`  
* **Purpose** – Checks that a downward‑moving series is correctly labeled as a declining trend.  
* **Parameters** – None.  
* **Return value** – `void`.  
* **Throws** – Throws an assertion exception if the trend is not reported as `Declining`.

### `public void ComputeTrendAndAnomalies_ShouldDetectAnomalies()`  
* **Purpose** – Validates that the service flags outliers in a otherwise normal series as anomalies.  
* **Parameters** – None.  
* **Return value** – `void`.  
* **Throws** – Throws an assertion exception if anomaly detection fails to identify the injected outliers.

### `public void ComputeTrendAndAnomalies_ShouldHandleAllSameValues()`  
* **Purpose** – Tests the behavior when every sample in the input series has identical value (zero variance).  
* **Parameters** – None.  
* **Return value** – `void`.  
* **Throws** – Throws an assertion exception if the service does not treat the series as stable or does not produce a defined result.

### `public void ComputeTrendAndAnomalies_ShouldHandleSingleZeroSampleWithoutNaN()`  
* **Purpose** – Ensures a series containing a single zero sample does not produce `NaN` in trend or anomaly outputs.  
* **Parameters** – None.  
* **Return value** – `void`.  
* **Throws** – Throws an assertion exception if any `NaN` value appears in the result.

### `public async Task AnalyzeAsync_ShouldProcessAllMetricTypes()`  
* **Purpose** – Asynchronously checks that the service can analyze a collection containing all supported metric types without error.  
* **Parameters** – None.  
* **Return value** – A `Task` representing the asynchronous operation.  
* **Throws** – May throw a `HealthDataException` if the underlying service encounters an unexpected error; the test expects no exception and will fail via assertion if one occurs.

### `public async Task AnalyzeAsync_ShouldHandleEmptyCollection()`  
* **Purpose** – Verifies that supplying an empty metric collection results in a graceful, empty output rather than a crash.  
* **Parameters** – None.  
* **Return value** – A `Task`.  
* **Throws** – Should not throw; if a `HealthDataException` is propagated, the test will fail with an assertion.

### `public async Task AnalyzeAsync_ShouldThrowHealthDataExceptionOnError()`  
* **Purpose** – Confirms that the service propagates a `HealthDataException` when an internal error (e.g., invalid data) occurs during analysis.  
* **Parameters** – None.  
* **Return value** – A `Task`.Task`  
* **Throws** –` the test awaits the call and expects the specific exception type.  
* **Throws** – The test itself catches the exception; if the service does **not** throw a `HealthDataException`, the test fails with an assertion.

## Usage  

The test class is intended to be executed by a unit‑test runner (e.g., `dotnet test`, Visual Studio Test Explorer, or ReSharper). No direct instantiation is required in production code.

### Example 1 – Running all tests with the .NET CLI  

```bash
# From the repository root
dotnet test healthdata-export-tools.Tests/HealthdataExportTools.Tests.csproj \
    --filter "FullyQualifiedName~TrendAnomalyDetectionServiceTests"
```

This command builds the test project and executes every test method in `TrendAnomalyDetectionServiceTests`. The runner reports pass/fail status for each method, including the asynchronous tests.

### Example 2 – Selective execution via NUnit console runner  

```bash
nunit3-console healthdata-export-tools.Tests/bin/Debug/net6.0/healthdata-export-tools.Tests.dll \
    --where "cat == TrendAnomalyDetectionServiceTests"
```

Assuming the test class is placed in the `TrendAnTests` category (via `[Category("TrendAnTests")]` attributes), this runs only the tests belonging to that group, useful for quick feedback during development.

## Notes  

* **Edge‑case coverage** – The suite explicitly validates behavior for:
  * Too few data points (`ShouldReturnInsufficientDataForFewPoints`).
  * Uniform series (`ShouldHandleAllSameValues`).
  * Single‑value zero sample (`ShouldHandleSingleZeroSampleWithoutNaN`).
  * Empty input collections (`AnalyzeAsync_ShouldHandleEmptyCollection`).
  * Error conditions that should surface as `HealthDataException` (`AnalyzeAsync_ShouldThrowHealthDataExceptionOnError`).

* **Thread‑safety** – The test class contains no mutable static or instance state; each test method operates on locally created data or mocks. Consequently, the class is safe to run in parallel with other test classes, and the test runner may execute its methods concurrently without risk of interference.

* **Exception semantics** – As unit tests, the methods themselves do not throw domain exceptions; they use the test framework’s assertion mechanisms to signal failure. The only expected production exception is `HealthDataException`, which is asserted for in the `ShouldThrowHealthDataExceptionOnError` test.

* **Framework assumptions** – Documentation assumes the use of NUnit (or a compatible framework) for the `[Test]` attributes and `Assert` calls. Adjust the runner commands if the project utilizes xUnit or MSTest.
