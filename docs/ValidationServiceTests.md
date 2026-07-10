# ValidationServiceTests

The `ValidationServiceTests` class contains a comprehensive suite of unit tests designed to verify the correct functioning of the `ValidationService`. It ensures that all health data processing—including sleep, heart rate, oxygen saturation (SpO2), steps, and general activity metrics—adheres to defined business rules. These tests validate both expected successful outcomes and various failure modes, ensuring robustness when handling edge cases such as future dates, mismatched timestamps, or out-of-range metric values.

## API

### Constructors
*   `public ValidationServiceTests()`
    *   **Purpose**: Initializes a new instance of the test class.
    *   **Parameters**: None.
    *   **Returns**: N/A.
    *   **Throws**: None.

### Test Methods
*(Note: All test methods return `void`, accept no parameters, and do not throw exceptions under normal execution conditions; failure of assertions within the tests results in a test failure reported by the test runner.)*

*   `public void ValidateSleepData_ShouldReturnValidResultForValidData()`
    *   **Purpose**: Verifies that correctly structured sleep data passes validation.
*   `public void ValidateSleepData_ShouldReturnInvalidResultWhenSleepStartIsAfterSleepEnd()`
    *   **Purpose**: Verifies that validation fails if the sleep start timestamp is later than the sleep end timestamp.
*   `public void ValidateSleepData_ShouldReturnInvalidResultWhenDurationMinutesIsZeroOrNegative()`
    *   **Purpose**: Verifies that validation fails if the calculated duration is non-positive.
*   `public void ValidateSleepData_ShouldReturnInvalidResultWhenRecordDateIsInFuture()`
    *   **Purpose**: Verifies that validation fails if the record date is set in the future.
*   `public void ValidateSleepData_ShouldReturnInvalidResultWhenSleepPhaseDurationsAreNegative()`
    *   **Purpose**: Verifies that validation fails if any sleep phase duration is negative.
*   `public void ValidateSleepData_ShouldReturnInvalidResultWhenSumOfSleepPhasesExceedsTotalDuration()`
    *   **Purpose**: Verifies that validation fails if the total sum of sleep phase durations is greater than the total reported sleep duration.
*   `public void ValidateSleepData_ShouldReturnInvalidResultForInvalidAverageHeartRate()`
    *   **Purpose**: Verifies that validation fails if the average heart rate value is outside acceptable bounds.
*   `public void ValidateSleepData_ShouldReturnInvalidResultForInvalidScore()`
    *   **Purpose**: Verifies that validation fails if the sleep score is outside the valid range.
*   `public void ValidateHeartRateData_ShouldReturnValidResultForValidData()`
    *   **Purpose**: Verifies that valid heart rate data passes validation.
*   `public void ValidateHeartRateData_ShouldReturnInvalidResultWhenRecordDateIsInFuture()`
    *   **Purpose**: Verifies that validation fails if the heart rate record date is in the future.
*   `public void ValidateHeartRateData_ShouldReturnInvalidResultForInvalidBpmRanges()`
    *   **Purpose**: Verifies that validation fails if the heart rate BPM (beats per minute) values are invalid.
*   `public void ValidateSpO2Data_ShouldReturnValidResultForValidData()`
    *   **Purpose**: Verifies that valid SpO2 data passes validation.
*   `public void ValidateSpO2Data_ShouldReturnInvalidResultWhenMinimumPercentageIsGreaterThanMaximum()`
    *   **Purpose**: Verifies that validation fails if the minimum SpO2 percentage exceeds the maximum percentage.
*   `public void ValidateStepsData_ShouldReturnValidResultForValidData()`
    *   **Purpose**: Verifies that valid steps data passes validation.
*   `public void ValidateStepsData_ShouldReturnInvalidResultWhenTotalStepsIsNegative()`
    *   **Purpose**: Verifies that validation fails if the total step count is a negative value.
*   `public void ValidateActivityData_ShouldReturnValidResultForValidData()`
    *   **Purpose**: Verifies that valid activity data passes validation.
*   `public void ValidateActivityData_ShouldReturnInvalidResultWhenActivityTypeIsEmpty()`
    *   **Purpose**: Verifies that validation fails if the activity type string is empty or null.
*   `public void ValidateActivityData_ShouldReturnInvalidResultWhenStartTimeIsAfterEndTime()`
    *   **Purpose**: Verifies that validation fails if the activity start time is after the end time.
*   `public void ValidateHealthMetric_ShouldReturnValidResultForValidData()`
    *   **Purpose**: Verifies that valid generic health metric data passes validation.

## Usage

### Example 1: Instantiating the test class
```csharp
[TestClass]
public class MyTestRunner
{
    [TestMethod]
    public void SetupTest()
    {
        // Tests are typically invoked by the test runner (e.g., MSTest, NUnit, xUnit)
        var testSuite = new ValidationServiceTests();
        Assert.IsNotNull(testSuite);
    }
}
```

### Example 2: Running a specific test manually
```csharp
public void ManualTestExecution()
{
    var tester = new ValidationServiceTests();
    // Invoke a validation test case directly
    // If no assertion exception is thrown, the test passes
    tester.ValidateStepsData_ShouldReturnValidResultForValidData();
    Console.WriteLine("Test passed successfully.");
}
```

## Notes

### Thread Safety
The `ValidationServiceTests` class is not inherently thread-safe. While individual test methods are typically executed sequentially or in isolated parallel threads by modern test runners (e.g., MSTest or xUnit), they should not be concurrently executed within the same instance without proper synchronization.

### Edge Cases
*   **Future Dates:** Several tests explicitly cover the "future date" edge case, ensuring the service correctly rejects data with timestamps occurring after the current system time.
*   **Boundary Conditions:** Tests for numerical inputs (e.g., heart rate, steps, SpO2) focus on boundary conditions, such as negative values or inverted range limits (e.g., Minimum > Maximum).
*   **Inverted Intervals:** Tests for sleep and activity data verify that start/end timestamps are logically ordered and that durations are calculated accurately.
