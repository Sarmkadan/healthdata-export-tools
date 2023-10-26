# DataTransformationUtilityTestsExtensions

Provides a collection of static extension methods used in unit tests to generate predictable test data and to perform fluent assertions on health‑related data structures. The methods are intended to simplify test setup and verification within the healthdata‑export‑tools test suite.

## API

### CreateTestSleepData
- **Purpose:** Returns a single `SleepData` instance populated with predefined test values.
- **Parameters:** None.
- **Return value:** A `SleepData` object ready for use in tests.
- **Exceptions:** None under normal circumstances; propagates any exceptions thrown by the `SleepData` constructor.

### CreateTestHeartRateData
- **Purpose:** Returns a single `HeartRateData` instance populated with predefined test values.
- **Parameters:** None.
- **Return value:** A `HeartRateData` object ready for use in tests.
- **Exceptions:** None under normal circumstances; propagates any exceptions thrown by the `HeartRateData` constructor.

### CreateTestStepsData
- **Purpose:** Returns a single `StepsData` instance populated with predefined test values.
- **Parameters:** None.
- **Return value:** A `StepsData` object ready for use in tests.
- **Exceptions:** None under normal circumstances; propagates any exceptions thrown by the `StepsData` constructor.

### CreateTestHealthDataRecord
- **Purpose:** Returns a single `HealthDataRecord` instance populated with predefined test values.
- **Parameters:** None.
- **Return value:** A `HealthDataRecord` object ready for use in tests.
- **Exceptions:** None under normal circumstances; propagates any exceptions thrown by the `HealthDataRecord` constructor.

### ShouldBeEquivalentTo (overload 1)
- **Purpose:** Asserts that two objects are equivalent using default comparison semantics.
- **Parameters:** `actual` (object), `expected` (object).
- **Return value:** None (throws on failure).
- **Exceptions:** Throws an assertion exception if the objects are not equivalent.

### ShouldBeEquivalentTo (overload 2)
- **Purpose:** Asserts that two objects are equivalent, allowing a custom comparer for specific members.
- **Parameters:** `actual` (object), `expected` (object), `comparer` (Func<object, object, bool>).
- **Return value:** None (throws on failure).
- **Exceptions:** Throws an assertion exception if the objects are not equivalent according to the supplied comparer.

### ShouldBeEquivalentTo (overload 3)
- **Purpose:** Asserts that two numeric collections are equivalent within a specified tolerance.
- **Parameters:** `actual` (IEnumerable<double>), `expected` (IEnumerable<double>), `tolerance` (double).
- **Return value:** None (throws on failure).
- **Exceptions:** Throws an assertion exception if any pair of values differs by more than the tolerance.

### CreateTestSleepDataBatch
- **Purpose:** Returns a list of `SleepData` instances populated with predefined test values.
- **Parameters:** None.
- **Return value:** A `List<SleepData>` containing a fixed set of test records.
- **Exceptions:** None under normal circumstances; propagates any exceptions thrown while creating individual items.

### CreateTestHeartRateDataBatch
- **Purpose:** Returns a list of `HeartRateData` instances populated with predefined test values.
- **Parameters:** None.
- **Return value:** A `List<HeartRateData>` containing a fixed set of test records.
- **Exceptions:** None under normal circumstances; propagates any exceptions thrown while creating individual items.

### CreateTestStepsDataBatch
- **Purpose:** Returns a list of `StepsData` instances populated with predefined test values.
- **Parameters:** None.
- **Return value:** A `List<StepsData>` containing a fixed set of test records.
- **Exceptions:** None under normal circumstances; propagates any exceptions thrown while creating individual items.

### CreateTestHealthDataBatch
- **Purpose:** Returns a list of `HealthDataRecord` instances populated with predefined test values.
- **Parameters:** None.
- **Return value:** A `List<HealthDataRecord>` containing a fixed set of test records.
- **Exceptions:** None under normal circumstances; propagates any exceptions thrown while creating individual items.

### ShouldContainRecordsInRange
- **Purpose:** Asserts that a collection of `HealthDataRecord` objects contains at least one record whose timestamp falls within a specified date range.
- **Parameters:** `records` (IEnumerable<HealthDataRecord>), `start` (DateTime), `end` (DateTime).
- **Return value:** None (throws on failure).
- **Exceptions:** Throws an assertion exception if no record matches the range; also throws if `records` is null.

### CreateTestDoubleValues
- **Purpose:** Returns a list of double values useful for testing normalization or statistical functions.
- **Parameters:** None.
- **Return value:** A `List<double>` with a predefined sequence (e.g., `{ 0.0, 0.5, 1.0, 1.5, 2.0 }`).
- **Exceptions:** None under normal circumstances.

### ShouldBeNormalized
- **Purpose:** Asserts that a list of double values is normalized (e.g., values are scaled to the interval [0, 1] or sum to 1, depending on the implementation).
- **Parameters:** `values` (IEnumerable<double>).
- **Return value:** None (throws on failure).
- **Exceptions:** Throws an assertion exception if the collection does not meet the normalization criteria; also throws if `values` is null.

## Usage

```csharp
using NUnit.Framework;
using HealthDataExportTools.Tests.Extensions;

[TestFixture]
public class SleepDataTests
{
    [Test]
    public void CreateTestSleepData_returns_valid_instance()
    {
        // Arrange & Act
        var sleep = DataTransformationUtilityTestsExtensions.CreateTestSleepData();

        // Assert
        Assert.IsNotNull(sleep);
        Assert.AreEqual(8.0, sleep.Hours); // example property from the test data
    }
}
```

```csharp
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using HealthDataExportTools.Tests.Extensions;

[TestFixture]
public class HeartRateDataTests
{
    [Test]
    public void ShouldBeEquivalentTo_with_tolerance_detects_small_differences()
    {
        // Arrange
        var actual   = DataTransformationUtilityTestsExtensions.CreateTestHeartRateDataBatch();
        var expected = actual.Select(h => new HeartRateData { Bpm = h.Bpm + 0.1, Timestamp = h.Timestamp }).ToList();

        // Act & Assert
        // The overload that accepts a tolerance should pass because the difference is within 0.2 bpm.
        DataTransformationUtilityTestsExtensions.ShouldBeEquivalentTo(actual, expected, 0.2);
    }
}
```

## Notes

- All methods in this class are **stateless** and rely only on their input parameters; therefore they are **thread‑safe** and can be invoked concurrently from multiple test threads without external synchronization.
- The factory methods (`CreateTest*`) return **immutable** test data where possible; however, if the underlying data classes contain mutable fields, callers should avoid modifying the returned instances to prevent test pollution.
- The `ShouldBeEquivalentTo` overloads differ in the way they handle comparison: the first uses default equality, the second permits a custom comparer, and the third applies a numeric tolerance. Selecting the appropriate overload ensures that the assertion matches the desired precision.
- `ShouldContainRecordsInRange` expects the `start` date to be strictly earlier than the `end` date; supplying an inverted range will cause the method to fail quickly because no record can satisfy the condition.
- `ShouldBeNormalized` assumes the normalization strategy implemented in the production code (e.g., min‑max scaling to `[0, 1]`). If the strategy changes, the test may need to be updated accordingly.
- None of the methods allocate static caches; each call creates new instances, which helps maintain test isolation but may incur minor allocation overhead in tight loops. This overhead is generally negligible in unit‑test scenarios.
