# AnalyticsServiceTests

The `AnalyticsServiceTests` class provides a comprehensive suite of unit tests designed to validate the functional correctness of the `AnalyticsService` component within the `healthdata-export-tools` project. It ensures that the service's algorithms for processing health metrics—including sleep quality, heart rate, physical activity, and trend analysis—produce accurate results across a variety of scenarios, such as empty data sets, recent records, and diverse input values.

## API

*   `public AnalyticsServiceTests()`
    *   **Purpose:** Initializes a new instance of the `AnalyticsServiceTests` class, setting up the necessary testing environment.
*   `public void CalculateAverageSleepDuration_ShouldReturnZeroForEmptyList()`
    *   **Purpose:** Verifies that the average sleep duration calculation returns zero when provided with an empty collection of sleep records.
*   `public void CalculateAverageSleepDuration_ShouldCalculateCorrectlyForRecentRecords()`
    *   **Purpose:** Validates that the service correctly calculates the average sleep duration for a set of recent sleep records.
*   `public void CalculateAverageHeartRate_ShouldCalculateCorrectly()`
    *   **Purpose:** Verifies the accuracy of the heart rate averaging logic.
*   `public void CalculateTotalSteps_ShouldCalculateCorrectlyForRecentRecords()`
    *   **Purpose:** Validates that the service correctly calculates the total number of steps based on recent activity data.
*   `public void AnalyzeTrend_ShouldReturnImprovingForIncreasingValues()`
    *   **Purpose:** Confirms that the trend analysis correctly identifies an "improving" trend for a set of increasing health metric values.
*   `public void AnalyzeTrend_ShouldReturnDecliningForDecreasingValues()`
    *   **Purpose:** Confirms that the trend analysis correctly identifies a "declining" trend for a set of decreasing health metric values.
*   `public void AnalyzeTrend_ShouldReturnStableForMinorChanges()`
    *   **Purpose:** Verifies that the trend analysis categorizes minor changes in values as a "stable" trend.
*   `public void CalculateHealthScore_ShouldReturn50ForEmptyCollection()`
    *   **Purpose:** Validates that the health score calculation defaults to 50 when given an empty data collection.
*   `public void CalculateHealthScore_ShouldIncreaseScoreBasedOnGoodData()`
    *   **Purpose:** Ensures that the health score increases appropriately when processed against positive health indicators.
*   `public void CalculateAverageDeepSleepPercentage_ShouldReturnZeroForEmptyList()`
    *   **Purpose:** Verifies that the average deep sleep percentage returns zero for empty sleep records.
*   `public void CalculateAverageDeepSleepPercentage_ShouldCalculateCorrectly()`
    *   **Purpose:** Validates the accuracy of the deep sleep percentage averaging logic.
*   `public void CalculateAverageRemPercentage_ShouldReturnZeroForEmptyList()`
    *   **Purpose:** Verifies that the average REM sleep percentage returns zero for empty sleep records.
*   `public void CalculateAverageRemPercentage_ShouldCalculateCorrectly()`
    *   **Purpose:** Validates the accuracy of the REM sleep percentage averaging logic.
*   `public void AnalyzeSleepQuality_ShouldReturnNoDataForEmptyList()`
    *   **Purpose:** Checks that the sleep quality analysis correctly indicates "no data" when the input list is empty.
*   `public void AnalyzeSleepQuality_ShouldReturnCorrectReport()`
    *   **Purpose:** Validates that the sleep quality analysis generates the expected summary report for populated data.
*   `public void AnalyzeSpO2Health_ShouldReturnNoDataForEmptyList()`
    *   **Purpose:** Checks that the SpO2 analysis indicates "no data" for an empty input set.
*   `public void AnalyzeSpO2Health_ShouldReturnCorrectReport()`
    *   **Purpose:** Validates that the SpO2 health analysis generates the expected output based on input SpO2 data.
*   `public void AnalyzeActivityIntensity_ShouldReturnEmptyForNoActivities()`
    *   **Purpose:** Confirms that the activity intensity analysis returns an empty result when no activities are provided.
*   `public void AnalyzeActivityIntensity_ShouldCategorizeActivitiesCorrectly()`
    *   **Purpose:** Validates that the activity intensity analysis correctly categorizes activity levels (e.g., sedentary, active, high intensity).

## Usage

**Example 1: Running tests via the .NET CLI**

```bash
# Run all tests within the project, including AnalyticsServiceTests
dotnet test --filter FullyQualifiedName~AnalyticsServiceTests
```

**Example 2: Test Method Invocation within a Test Runner (e.g., xUnit)**

```csharp
public class AnalyticsServiceTests
{
    private readonly IAnalyticsService _service;

    public AnalyticsServiceTests()
    {
        _service = new AnalyticsService();
    }

    [Fact]
    public void CalculateTotalSteps_ShouldCalculateCorrectlyForRecentRecords()
    {
        // Arrange
        var records = GetSampleActivityRecords();
        
        // Act
        var totalSteps = _service.CalculateTotalSteps(records);
        
        // Assert
        Assert.Equal(15000, totalSteps);
    }
}
```

## Notes

*   **Edge Cases:** Several tests explicitly cover edge cases, such as passing empty collections or `null` equivalents, ensuring the `AnalyticsService` does not throw unhandled exceptions (e.g., `NullReferenceException` or `DivideByZeroException`) under these conditions.
*   **Thread Safety:** While individual test methods are designed to be executed independently, they are not inherently thread-safe. Execution order and parallelism are managed by the chosen test runner (e.g., xUnit, NUnit). It is recommended to configure the test runner to avoid running tests that modify shared resources concurrently, although the current `AnalyticsService` methods should ideally be stateless or read-only regarding input data.
*   **Dependencies:** These tests assume access to a properly implemented `AnalyticsService`. If the service dependencies change (e.g., addition of database context or external APIs), the constructor or individual test setups must be updated accordingly.
