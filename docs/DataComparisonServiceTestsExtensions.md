# DataComparisonServiceTestsExtensions

Provides a set of static extension methods used in unit tests to build test data, compute comparison results, and assert expected changes between baseline and current health data collections.

## API

### CreateComparisonResultAsync
**Purpose:** Asynchronously compares two `HealthDataCollection` instances and produces a `DataComparisonResult` containing calculated metrics.  
**Parameters:**  
- `baseline` (`HealthDataCollection`) – the reference data set.  
- `current` (`HealthDataCollection`) – the data set to compare against the baseline.  
- `cancellationToken` (`CancellationToken`, optional) – token to observe for cancellation requests.  
**Return Value:** `Task<DataComparisonResult>` – the result of the comparison.  
**Exceptions:**  
- `ArgumentNullException` if either `baseline` or `current` is `null`.  
- `InvalidOperationException` if the collections contain incompatible or missing data types required for comparison.

### ShouldHavePercentageChanges
**Purpose:** Asserts that the percentage changes reported in a `DataComparisonResult` match expected values within a tolerance.  
**Parameters:**  
- `result` (`DataComparisonResult`) – the comparison result to inspect.  
- `expectedChanges` (`IReadOnlyDictionary<string, double>`) – mapping of metric names to expected percentage change.  
- `tolerance` (`double`, optional) – maximum allowed deviation; defaults to `0.0`.  
**Return Value:** `void`.  
**Exceptions:**  
- Throws an assertion exception (e.g., `Xunit.AssertException`) if any actual percentage change differs from the expected value by more than `tolerance`.  
- `ArgumentNullException` if `result` or `expectedChanges` is `null`.

### ShouldHaveSpO2Change
**Purpose:** Asserts that the SpO₂ change metric in the result equals the expected value within a tolerance.  
**Parameters:**  
- `result` (`DataComparisonResult`) – the comparison result.  
- `expectedChange` (`double`) – expected SpO₂ change (percentage points).  
- `tolerance` (`double`, optional) – allowed deviation; defaults to `0.0`.  
**Return Value:** `void`.  
**Exceptions:**  
- Assertion exception if the actual SpO₂ change deviates beyond `tolerance`.  
- `ArgumentNullException` if `result` is `null`.

### ShouldHaveActivityChange
**Purpose:** Asserts that the activity change metric matches the expected value within a tolerance.  
**Parameters:**  
- `result` (`DataComparisonResult`) – the comparison result.  
- `expectedChange` (`double`) – expected activity change.  
- `tolerance` (`double`, optional) – allowed deviation; defaults to `0.0`.  
**Return Value:** `void`.  
**Exceptions:**  
- Assertion exception on mismatch beyond `tolerance`.  
- `ArgumentNullException` if `result` is `null`.

### ShouldHaveDeepSleepChange
**Purpose:** Asserts that the deep sleep change metric matches the expected value within a tolerance.  
**Parameters:**  
- `result` (`DataComparisonResult`) – the comparison result.  
- `expectedChange` (`double`) – expected deep sleep change (e.g., minutes or percentage).  
- `tolerance` (`double`, optional) – allowed deviation; defaults to `0.0`.  
**Return Value:** `void`.  
**Exceptions:**  
- Assertion exception if the actual deep sleep change differs beyond `tolerance`.  
- `ArgumentNullException` if `result` is `null`.

### WithSleepData
**Purpose:** Creates a `HealthDataCollection` populated with sleep‑stage records for test scenarios.  
**Parameters:**  
- `sleepEntries` (`IEnumerable<SleepRecord>`) – sequence of sleep records to include.  
**Return Value:** `HealthDataCollection` containing the supplied sleep data.  
**Exceptions:**  
- `ArgumentNullException` if `sleepEntries` is `null`.  
- `ArgumentException` if any entry has invalid timestamps or unsupported sleep stages.

### WithHeartRateData
**Purpose:** Creates a `HealthDataCollection` populated with heart‑rate records.  
**Parameters:**  
- `heartRateEntries` (`IEnumerable<HeartRateRecord>`) – sequence of heart‑rate records.  
**Return Value:** `HealthDataCollection` containing the supplied heart‑rate data.  
**Exceptions:**  
- `ArgumentNullException` if `heartRateEntries` is `null`.  
- `ArgumentException` if any record contains a non‑positive or physiologically implausible value.

### WithStepsData
**Purpose:** Creates a `HealthDataCollection` populated with step‑count records.  
**Parameters:**  
- `stepsEntries` (`IEnumerable<StepRecord>`) – sequence of step records.  
**Return Value:** `HealthDataCollection` containing the supplied step data.  
**Exceptions:**  
- `ArgumentNullException` if `stepsEntries` is `null`.  
- `ArgumentException` if any step count is negative.

### WithSpO2Data
**Purpose:** Creates a `HealthDataCollection` populated with SpO₂ records.  
**Parameters:**  
- `spo2Entries` (`IEnumerable<SpO2Record>`) – sequence of SpO₂ records.  
**Return Value:** `HealthDataCollection` containing the supplied SpO₂ data.  
**Exceptions:**  
- `ArgumentNullException` if `spo2Entries` is `null`.  
- `ArgumentException` if any SpO₂ value lies outside the 0‑100 range.

### WithActivityData
**Purpose:** Creates a `HealthDataCollection` populated with activity records.  
**Parameters:**  
- `activityEntries` (`IEnumerable<ActivityRecord>`) – sequence of activity records.  
**Return Value:** `HealthDataCollection` containing the supplied activity data.  
**Exceptions:**  
- `ArgumentNullException` if `activityEntries` is `null`.  
- `ArgumentException` if any record contains an unsupported activity type or invalid duration.

## Usage

### Example 1: Comparing heart‑rate data and asserting percentage change
```csharp
var baseline = DataComparisonServiceTestsExtensions.WithHeartRateData(
    new[]
    {
        new HeartRateRecord(DateTime.UtcNow.AddHours(-1), 72),
        new HeartRateRecord(DateTime.UtcNow, 78)
    });

var current = DataComparisonServiceTestsExtensions.WithHeartRateData(
    new[]
    {
        new HeartRateRecord(DateTime.UtcNow.AddHours(-1), 70),
        new HeartRateRecord(DateTime.UtcNow, 80)
    });

var result = await DataComparisonServiceTestsExtensions.CreateComparisonResultAsync(baseline, current);

DataComparisonServiceTestsExtensions.ShouldHavePercentageChanges(
    result,
    new Dictionary<string, double> { { "HeartRate", 8.33 } },
    tolerance: 0.5);
```

### Example 2: Combining sleep and SpO₂ data, then verifying specific metric changes
```csharp
var sleepBaseline = DataComparisonServiceTestsExtensions.WithSleepData(
    new[] { new SleepRecord(DateTime.UtcNow.Date, SleepStage.Deep, 120) });

var sleepCurrent = DataComparisonServiceTestsExtensions.WithSleepData(
    new[] { new SleepRecord(DateTime.UtcNow.Date, SleepStage.Deep, 150) });

var spo2Baseline = DataComparisonServiceTestsExtensions.WithSpO2Data(
    new[] { new SpO2Record(DateTime.UtcNow, 96) });

var spo2Current = DataComparisonServiceTestsExtensions.WithSpO2Data(
    new[] { new SpO2Record(DateTime.UtcNow, 94) });

// Assuming HealthDataCollection provides a Merge method to combine independent data sets
var baseline = sleepBaseline.Merge(spo2Baseline);
var current = sleepCurrent.Merge(spo2Current);

var result = await DataComparisonServiceTestsExtensions.CreateComparisonResultAsync(baseline, current);

DataComparisonServiceTestsExtensions.ShouldHaveDeepSleepChange(result, expectedChange: 25.0, tolerance: 1.0);
DataComparisonServiceTestsExtensions.ShouldHaveSpO2Change(result, expectedChange: -2.0, tolerance: 0.5);
```

## Notes
- All `With*Data` methods produce new `HealthDataCollection` instances; they do not mutate the input sequences.  
- `CreateComparisonResultAsync` accesses only its parameters and has no static mutable state, making it safe for concurrent invocation from multiple threads.  
- The `ShouldHave*` assertion methods validate their arguments and throw `ArgumentNullException` for null inputs; otherwise they throw only when the assertion fails.  
- Input validation in the `With*` helpers prevents creation of malformed collections (e.g., null sequences, out‑of‑range values).  
- If the project’s `HealthDataCollection` type lacks a built‑in merge/combine capability, tests must manually aggregate data before calling `CreateComparisonResultAsync`.  
- Tolerance parameters in the assertion methods accommodate floating‑point imprecision; a tolerance of zero demands an exact match.  
- Because the type contains solely static members, there is no instance state to synchronize; thread‑safety concerns are limited to the safety of the arguments supplied by callers.
