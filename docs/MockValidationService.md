# MockValidationService

`MockValidationService` provides a lightweight, in-memory implementation of validation logic for health data exports, intended for use in unit testing scenarios where network-dependent or complex real-world validation services are unnecessary or undesirable. It allows developers to simulate the outcome of validation processes for various health metrics, enabling deterministic testing of data processing pipelines without the overhead of external dependencies.

## API

### `ValidationResult ValidateSleepData(...)`
Validates sleep-related data records.
- **Parameters:** Accepts data structures representing sleep duration, stages, and quality metrics.
- **Returns:** A `ValidationResult` object indicating success or failure, containing potential error messages or warnings.
- **Exceptions:** Throws `ArgumentNullException` if provided data is null.

### `ValidationResult ValidateHeartRateData(...)`
Validates heart rate telemetry records.
- **Parameters:** Accepts heart rate samples, including timestamp and beats-per-minute values.
- **Returns:** A `ValidationResult` indicating whether the heart rate data falls within physically plausible ranges.
- **Exceptions:** Throws `ArgumentException` if timestamp sequence is invalid.

### `ValidationResult ValidateSpO2Data(...)`
Validates blood oxygen saturation (SpO2) data.
- **Parameters:** Accepts SpO2 percentage readings and associated metadata.
- **Returns:** A `ValidationResult` indicating validity based on standard physiological bounds.

### `ValidationResult ValidateStepsData(...)`
Validates step count data.
- **Parameters:** Accepts total steps, time intervals, and sensor accuracy metadata.
- **Returns:** A `ValidationResult` validating the continuity and non-negative nature of the step data.

### `ValidationResult ValidateActivityData(...)`
Validates general activity logs (e.g., exercise duration, type, calories burned).
- **Parameters:** Accepts activity type definitions and duration data.
- **Returns:** A `ValidationResult` indicating whether the activity record conforms to expected schemas.

### `ValidationResult ValidateHealthMetric(...)`
A generic validator for arbitrary or custom health metrics.
- **Parameters:** Accepts a generic metric type and its corresponding data payload.
- **Returns:** A `ValidationResult` providing validation status based on the specific metric type identifier.

## Usage

```csharp
// Example 1: Basic validation in a unit test
var validator = new MockValidationService();
var sleepData = new SleepData { DurationMinutes = 480 };
var result = validator.ValidateSleepData(sleepData);

if (!result.IsValid)
{
    Console.WriteLine($"Validation failed: {result.ErrorMessage}");
}
```

```csharp
// Example 2: Using the generic validator for custom metrics
var validator = new MockValidationService();
var customMetric = new CustomMetric { Name = "Hydration", Value = 2500 };
var result = validator.ValidateHealthMetric(customMetric);

Assert.IsTrue(result.IsValid);
```

## Notes

- **Thread Safety:** `MockValidationService` is designed to be thread-safe for concurrent read operations. However, internal state management (if overridden in derived classes) must be explicitly managed to ensure thread safety during concurrent validation calls.
- **Edge Cases:** Invalid timestamps, null inputs, and values outside of realistic physiological ranges (e.g., heart rate < 0) are treated as invalid and will return a `ValidationResult` with `IsValid = false`.
- **Deterministic Behavior:** This service is primarily for deterministic testing; it will always return the same validation outcome for identical inputs within the same process execution context.
