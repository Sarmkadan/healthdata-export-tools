# DomainModelTests

`DomainModelTests` is a test suite designed to validate the core business logic, data models, service behaviors, and caching mechanisms within the `healthdata-export-tools` project. It ensures that health metrics—including sleep quality, step goal achievement, heart rate reserve, and blood oxygen saturation levels—are calculated accurately, while also verifying service-level parser identification and the reliability of in-memory caching components.

## API

### Sleep Data Tests
- `SleepData_CalculateQuality_DurationUnder6Hours_ReturnsPoor()`: Verifies that `SleepData` correctly categorizes total sleep duration of less than 6 hours as "Poor".
- `SleepData_GetDeepSleepPercentage_ReturnsCorrectRatio()`: Validates that the ratio of deep sleep duration to total sleep duration is calculated accurately.
- `SleepData_GetDeepSleepPercentage_ZeroDuration_ReturnsZero()`: Ensures the deep sleep calculation handles a total sleep duration of zero without error, returning a percentage of zero.

### Steps Data Tests
- `StepsData_UpdateGoalAchievement_StepsExceedGoal_SetsGoalAchievedTrueAndCorrectPercentage()`: Confirms that when recorded steps exceed the daily goal, `StepsData` correctly updates the achievement status to `true` and calculates the accurate completion percentage.
- `StepsData_UpdateGoalAchievement_ZeroDailyGoal_SetsAchievementToZero()`: Verifies that achievement calculation gracefully handles a zero-value daily goal.
- `StepsData_SetHourlySteps_HourAbove23_ThrowsArgumentOutOfRangeException()`: Validates that attempting to set hourly steps for an invalid time (hour greater than 23) throws an `ArgumentOutOfRangeException`.

### Heart Rate Data Tests
- `HeartRateData_AddMeasurement_UpdatesMeasurementCount()`: Ensures that adding a heart rate measurement correctly increments the total internal measurement count within `HeartRateData`.
- `HeartRateData_CalculateHeartRateReserve_WithRestingBpm_ReturnsMaxMinusResting()`: Validates the heart rate reserve calculation, confirming it returns the difference between the maximum and resting heart rate values.

### SpO2 Data Tests
- `SpO2Data_AddMeasurement_ReadingBelow95_IncrementsLowSpO2Events()`: Checks that oxygen saturation readings below 95% correctly increment the internal counter for low SpO2 events.
- `SpO2Data_HasConcerningLevels_MinimumBelow90_ReturnsTrue()`: Confirms that `SpO2Data` correctly flags concerning levels when the minimum observed reading falls below 90%.

### Parser Service Tests
- `HealthDataParserService_DetectDeviceType_GarminIdentifier_ReturnsGarmin()`: Validates that the parser service accurately identifies Garmin devices based on recognized string identifiers.

### InMemoryCacheProvider Tests
- `InMemoryCacheProvider_SetAndGet_WithMockedLogger_ReturnsStoredValue()`: An asynchronous test verifying that the cache provider successfully stores and retrieves data using a mocked logger dependency.
- `InMemoryCacheProvider_GetNonExistentKey_ReturnsNull()`: An asynchronous test ensuring that retrieving a key that does not exist in the cache returns `null`.
- `InMemoryCacheProvider_RemoveKey_SubsequentGetReturnsNull()`: An asynchronous test verifying that removing a key from the cache ensures subsequent lookups for that key return `null`.

## Usage

### Synchronous Logic Validation
```csharp
// Example of running a test for StepsData logic
public void TestStepsLogic()
{
    var tester = new DomainModelTests();
    // Verify that invalid hours throw the expected exception
    Assert.Throws<ArgumentOutOfRangeException>(() => 
        tester.StepsData_SetHourlySteps_HourAbove23_ThrowsArgumentOutOfRangeException());
}
```

### Asynchronous Cache Validation
```csharp
// Example of running an async test for InMemoryCacheProvider
public async Task TestCacheLogic()
{
    var tester = new DomainModelTests();
    // Await the test method to ensure completion
    await tester.InMemoryCacheProvider_GetNonExistentKey_ReturnsNull();
}
```

## Notes

- **Test Framework**: These members constitute test methods designed for execution within a standard .NET test runner (e.g., xUnit, NUnit, or MSTest).
- **Asynchronous Execution**: Members prefixed with `async` (e.g., the `InMemoryCacheProvider` tests) must be awaited by the test runner to ensure proper execution and lifecycle management.
- **Error Conditions**: The tests covering exceptions (such as `StepsData_SetHourlySteps_HourAbove23_ThrowsArgumentOutOfRangeException`) should be wrapped in assertions that verify the specific exception type is thrown.
- **Thread Safety**: While `InMemoryCacheProvider` is designed for in-memory operations, the unit tests do not explicitly test concurrent access scenarios; ensure that thread-safety requirements for the provider itself are validated separately if high-concurrency usage is expected.
