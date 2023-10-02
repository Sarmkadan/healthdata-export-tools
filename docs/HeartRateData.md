# HeartRateData

Represents aggregated heart rate data for a given time period, combining raw measurements with derived metrics such as zone minutes, stress level estimates, and heart rate variability. This type serves as the primary container for heart rate analysis results within the health data export pipeline.

## API

### `public int MinimumBpm`

Gets the lowest beats-per-minute value recorded across all measurements in the current instance.

### `public int MaximumBpm`

Gets the highest beats-per-minute value recorded across all measurements in the current instance.

### `public int AverageBpm`

Gets the arithmetic mean of all beats-per-minute values in the measurement set.

### `public int? RestingBpm`

Gets the resting heart rate estimate, if one could be determined from the data. Null when insufficient measurements exist to produce a reliable resting value.

### `public int MeasurementCount`

Gets the total number of individual heart rate measurements contained in this instance.

### `public List<HeartRateMeasurement> Measurements`

Gets the ordered list of raw heart rate measurements that form the basis for all aggregated values. Modifications to the returned list affect the internal state of this instance.

### `public double? HeartRateVariability`

Gets the calculated heart rate variability in milliseconds, or null if variability could not be computed (e.g., fewer than two measurements available).

### `public int? StressLevel`

Gets an integer stress level estimate derived from heart rate patterns, or null when stress assessment is not possible with the available data.

### `public int CardioZoneMinutes`

Gets the total minutes spent in the cardio heart rate zone, calculated from zone boundaries and measurement durations.

### `public int FatBurnZoneMinutes`

Gets the total minutes spent in the fat-burn heart rate zone, calculated from zone boundaries and measurement durations.

### `public int[] ZoneMinutes`

Gets an array of zone minute totals indexed by zone identifier. The array length and zone mapping correspond to the heart rate zone classification scheme used by the system.

### `public override bool IsValid`

Returns `true` when the instance contains at least one measurement and all required derived fields are consistent; returns `false` otherwise. Overrides the base validation logic to include heart-rate-specific integrity checks.

### `public override Dictionary<string, object> GetSummary`

Returns a dictionary containing key-value pairs for all public summary metrics (minimum, maximum, average, resting BPM, zone minutes, stress level, heart rate variability, and measurement count). Keys use camelCase naming. Overrides the base summary method to include heart-rate-specific fields.

### `public int? CalculateHeartRateReserve(int age, int? maxHeartRate = null)`

Computes heart rate reserve as the difference between maximum heart rate and resting heart rate. Accepts an `age` parameter used to estimate maximum heart rate via the standard 220-minus-age formula when `maxHeartRate` is not explicitly provided. Returns null when `RestingBpm` is null or age is non-positive.

**Parameters:**
- `age`: The individual's age in years. Must be greater than zero.
- `maxHeartRate`: An optional known maximum heart rate. When null, the formula 220 - age is applied.

**Returns:** The calculated reserve as beats per minute, or null if inputs are insufficient.

### `public int AssessStressLevel()`

Evaluates stress level from the current heart rate data using a combination of resting heart rate elevation, heart rate variability, and sustained elevated readings. Returns an integer on a scale where higher values indicate greater stress. Returns zero when data is insufficient for assessment.

### `public void AddMeasurement(HeartRateMeasurement measurement)`

Appends a single heart rate measurement to the internal measurement list and updates all dependent aggregate values (minimum, maximum, average, zone minutes, variability, and stress level). Throws `ArgumentNullException` when `measurement` is null. Throws `InvalidOperationException` when the measurement timestamp is earlier than the most recently added measurement, as chronological ordering is required.

**Parameters:**
- `measurement`: The heart rate measurement to add. Must not be null and must have a timestamp equal to or later than the last added measurement.

### `public static HeartRateZone ClassifyZone(int bpm, int age, int? restingBpm = null)`

Classifies a given beats-per-minute value into a heart rate zone based on age-predicted maximum heart rate and, when available, resting heart rate for more precise zone boundaries. Returns a `HeartRateZone` enum value.

**Parameters:**
- `bpm`: The heart rate value to classify.
- `age`: The individual's age in years.
- `restingBpm`: Optional resting heart rate for Karvonen-based zone calculation.

**Returns:** The corresponding `HeartRateZone` enum member.

### `public void SetZoneMinutesFromSeconds(int[] zoneSeconds)`

Overwrites zone minute totals by converting an array of per-zone second values into minutes. Each array element is divided by 60 and rounded to the nearest integer. Throws `ArgumentNullException` when `zoneSeconds` is null. Throws `ArgumentException` when the array length does not match the expected number of heart rate zones.

**Parameters:**
- `zoneSeconds`: An array of accumulated seconds per zone, indexed by zone identifier.

### `public DateTime Timestamp`

Gets the timestamp associated with the measurement. This property is inherited from the base measurement type and reflects the time of the individual reading.

### `public int Bpm`

Gets the beats-per-minute value of the measurement. This property is inherited from the base measurement type.

## Usage

### Example 1: Building HeartRateData from Individual Measurements

```csharp
var heartRateData = new HeartRateData();

// Add measurements in chronological order
heartRateData.AddMeasurement(new HeartRateMeasurement(
    timestamp: DateTime.UtcNow.AddMinutes(-10),
    bpm: 72
));
heartRateData.AddMeasurement(new HeartRateMeasurement(
    timestamp: DateTime.UtcNow.AddMinutes(-5),
    bpm: 78
));
heartRateData.AddMeasurement(new HeartRateMeasurement(
    timestamp: DateTime.UtcNow,
    bpm: 145
));

// Access aggregated results
Console.WriteLine($"Average BPM: {heartRateData.AverageBpm}");
Console.WriteLine($"Minutes in cardio zone: {heartRateData.CardioZoneMinutes}");
Console.WriteLine($"Stress level: {heartRateData.AssessStressLevel()}");

// Check validity before persisting
if (heartRateData.IsValid)
{
    var summary = heartRateData.GetSummary();
    // Serialize or store summary dictionary
}
```

### Example 2: Zone Classification and Reserve Calculation

```csharp
var heartRateData = new HeartRateData();
// ... populate with measurements ...

int age = 34;
int currentBpm = 152;

// Classify current heart rate into a zone
HeartRateZone zone = HeartRateData.ClassifyZone(
    bpm: currentBpm,
    age: age,
    restingBpm: heartRateData.RestingBpm
);

Console.WriteLine($"Current zone: {zone}");

// Calculate heart rate reserve for fitness assessment
int? reserve = heartRateData.CalculateHeartRateReserve(age);
if (reserve.HasValue)
{
    Console.WriteLine($"Heart rate reserve: {reserve.Value} BPM");
    // Use reserve for training intensity calculations
}

// Set zone minutes from externally tracked seconds
int[] trackedZoneSeconds = new[] { 1200, 900, 300, 0, 0 };
heartRateData.SetZoneMinutesFromSeconds(trackedZoneSeconds);
Console.WriteLine($"Fat burn minutes: {heartRateData.FatBurnZoneMinutes}");
```

## Notes

- **Measurement ordering:** `AddMeasurement` enforces chronological ordering. Attempting to add a measurement with a timestamp earlier than the last entry throws `InvalidOperationException`. Sort measurements before adding if they originate from an unordered source.
- **Nullability of derived metrics:** `RestingBpm`, `HeartRateVariability`, and `StressLevel` are nullable and will be null when the measurement count is too low to produce statistically meaningful results. Always null-check these properties before use.
- **Zone minutes consistency:** `ZoneMinutes`, `CardioZoneMinutes`, and `FatBurnZoneMinutes` derive from the same underlying zone data. Calling `SetZoneMinutesFromSeconds` overwrites all zone minute values and recalculates the convenience properties accordingly.
- **Thread safety:** This type is not thread-safe. Concurrent calls to `AddMeasurement` or simultaneous reads during a write will produce inconsistent state. External synchronization is required for multi-threaded scenarios.
- **Validity state:** `IsValid` may return `false` immediately after construction (zero measurements) or after `SetZoneMinutesFromSeconds` if the provided array produces an inconsistent zone distribution relative to the raw measurements. Validate before serializing or exporting.
- **`GetSummary` dictionary keys:** The returned dictionary uses camelCase string keys suitable for JSON serialization. Do not rely on key ordering; iterate the dictionary or access by known key names.
