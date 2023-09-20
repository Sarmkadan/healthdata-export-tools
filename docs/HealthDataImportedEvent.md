# HealthDataImportedEvent

Represents an immutable event that captures the metadata and outcome of a health data import operation. It records the source, device type, time boundaries, record count, and the specific metric types that were imported, providing a self-contained snapshot suitable for logging, auditing, or downstream processing.

## API

### `public int RecordCount`

Gets the total number of health data records successfully imported during the operation.

**Value:** A non-negative integer. Zero indicates that the import completed but no records were processed.

---

### `public DateTime ImportStartTime`

Gets the UTC timestamp marking the moment the import operation began.

**Value:** A `DateTime` with `Kind` set to `Utc`.

---

### `public DateTime ImportEndTime`

Gets the UTC timestamp marking the moment the import operation completed.

**Value:** A `DateTime` with `Kind` set to `Utc`. This value is always equal to or later than `ImportStartTime`.

---

### `public string ImportSource`

Gets an identifier for the origin of the imported data.

**Value:** A non-null, non-empty string. Typical values include file paths, API endpoint descriptors, or database connection names.

---

### `public DeviceType DeviceType`

Gets the category of device that generated the imported health data.

**Value:** A member of the `DeviceType` enumeration (e.g., `Wearable`, `Phone`, `MedicalDevice`).

---

### `public List<string> ImportedMetricTypes`

Gets the distinct set of metric type identifiers present in the imported data.

**Value:** A non-null list of strings. Each entry corresponds to a metric classification such as `"HeartRate"`, `"StepCount"`, or `"BloodGlucose"`. The list may be empty if no metric types were recognized.

---

### `public HealthDataImportedEvent`

Constructs a new instance of `HealthDataImportedEvent`.

**Parameters:** All properties described above are supplied as constructor arguments. The constructor performs validation and sets each corresponding property.

**Exceptions:**
- `ArgumentNullException` — thrown when `ImportSource` or `ImportedMetricTypes` is `null`.
- `ArgumentException` — thrown when `ImportSource` is empty or consists only of whitespace.
- `ArgumentOutOfRangeException` — thrown when `RecordCount` is negative, or when `ImportEndTime` is earlier than `ImportStartTime`.

---

### `public TimeSpan GetImportDuration()`

Returns the elapsed wall-clock time of the import operation.

**Return Value:** A `TimeSpan` representing `ImportEndTime - ImportStartTime`.

**Parameters:** None.

**Exceptions:** None.

---

### `public double GetThroughput()`

Calculates the average processing rate of the import operation.

**Return Value:** The number of records imported per second, as a `double`. Computed as `RecordCount / GetImportDuration().TotalSeconds`.

**Parameters:** None.

**Exceptions:**
- `DivideByZeroException` — thrown when the import duration is zero (`ImportStartTime` equals `ImportEndTime`). Callers should guard against this when `RecordCount` is greater than zero but the duration is instantaneous.

---

### `public override string ToString()`

Provides a human-readable summary of the import event.

**Return Value:** A string that includes the source, device type, record count, duration, and throughput. The exact format is implementation-defined and subject to change; it is intended for diagnostics and logging, not for machine parsing.

**Parameters:** None.

**Exceptions:** None.

---

## Usage

### Example 1: Basic Construction and Logging

```csharp
var importedEvent = new HealthDataImportedEvent(
    recordCount: 15_430,
    importStartTime: new DateTime(2025, 3, 15, 8, 0, 0, DateTimeKind.Utc),
    importEndTime: new DateTime(2025, 3, 15, 8, 0, 12, DateTimeKind.Utc),
    importSource: "FitbitWebApi",
    deviceType: DeviceType.Wearable,
    importedMetricTypes: new List<string> { "HeartRate", "StepCount", "SleepStage" }
);

Console.WriteLine(importedEvent.ToString());
Console.WriteLine($"Throughput: {importedEvent.GetThroughput():F1} records/sec");
```

### Example 2: Guarding Against Zero Duration

```csharp
var instantEvent = new HealthDataImportedEvent(
    recordCount: 0,
    importStartTime: DateTime.UtcNow,
    importEndTime: DateTime.UtcNow,
    importSource: "EmptyFile.csv",
    deviceType: DeviceType.Phone,
    importedMetricTypes: new List<string>()
);

TimeSpan duration = instantEvent.GetImportDuration();
double throughput = duration.TotalSeconds > 0
    ? instantEvent.GetThroughput()
    : 0.0;

Console.WriteLine($"Import took {duration.TotalMilliseconds:F0} ms, throughput set to {throughput}");
```

---

## Notes

- **Immutability:** All property values are set at construction and cannot be modified afterward. The returned `List<string>` from `ImportedMetricTypes` is a defensive copy or read-only wrapper; mutating it will not affect the event instance.
- **Time Zone Assumption:** `ImportStartTime` and `ImportEndTime` are expected to be in UTC. Behavior with `Local` or `Unspecified` kinds is undefined and may produce misleading duration or throughput values.
- **Edge Case — Zero Records, Non-Zero Duration:** `GetThroughput()` returns `0.0` when `RecordCount` is zero, regardless of duration. This is mathematically consistent and does not throw.
- **Edge Case — Zero Duration, Zero Records:** `GetThroughput()` throws `DivideByZeroException`. Callers must explicitly check `GetImportDuration()` before calling `GetThroughput()` when there is any possibility of an instantaneous import.
- **Thread Safety:** The type is immutable and therefore safe for concurrent read access without synchronization. No internal state is mutated after construction.
- **ToString() Stability:** The output of `ToString()` is not guaranteed to remain stable across versions. It should not be parsed or used for contractual comparisons.
