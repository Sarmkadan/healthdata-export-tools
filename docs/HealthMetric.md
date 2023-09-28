# HealthMetric

`HealthMetric` represents a single quantified health observation with associated metadata, reference ranges, trend indicators, and validity logic. It is designed to capture both current and historical values, evaluate whether the measurement falls within clinically normal boundaries, and expose a structured summary for downstream processing or export.

## API

### `public string MetricName`
Gets or sets the human-readable name of the metric (e.g., "Blood Glucose", "Resting Heart Rate").

### `public double Value`
Gets or sets the current numeric reading for the metric. Must be a finite double.

### `public string Unit`
Gets or sets the unit of measurement (e.g., "mg/dL", "bpm", "kg").

### `public double? NormalRangeLow`
Gets or sets the lower bound of the normal reference range. `null` when no lower bound is defined.

### `public double? NormalRangeHigh`
Gets or sets the upper bound of the normal reference range. `null` when no upper bound is defined.

### `public double? PreviousValue`
Gets or sets the immediately preceding recorded value, if available. `null` when no prior reading exists.

### `public int? Trend`
Gets or sets a directional trend indicator encoded as an integer. The interpretation is domain-specific; `null` means no trend has been computed.

### `public double? PercentageChange`
Gets or sets the percentage change from `PreviousValue` to `Value`. `null` when `PreviousValue` is unavailable or zero-division would occur.

### `public string HealthStatus`
Gets or sets a textual assessment of the metric’s clinical standing (e.g., "Normal", "Elevated", "Low"). This property is typically set by `AssessHealthStatus`.

### `public int? ConfidenceScore`
Gets or sets an integer representing confidence in the measurement or its interpretation. `null` when confidence has not been assigned.

### `public List<string> DataSources`
Gets the list of data origin identifiers (e.g., device names, file imports, manual entry labels). The list is never null; it is initialised as an empty list on construction.

### `public DateTime LastUpdated`
Gets or sets the UTC timestamp of the most recent update to `Value` or associated metadata.

### `public int? SampleDays`
Gets or sets the number of days over which samples were aggregated to produce this metric. `null` when the metric is a single-point observation.

### `public override bool IsValid`
Returns `true` when the metric meets all internal consistency requirements (e.g., `Value` is finite, `MetricName` is non-null and non-empty, `Unit` is non-null, `LastUpdated` is a valid DateTime). Override of a base class or interface member.

### `public override Dictionary<string, object> GetSummary`
Returns a dictionary containing key-value pairs that summarise the metric’s current state. Keys typically include name, value, unit, normal range bounds, health status, and trend. Override of a base class or interface member. Never returns null.

### `public bool IsInNormalRange`
Returns `true` when `Value` lies within the inclusive bounds defined by `NormalRangeLow` and `NormalRangeHigh`. If either bound is `null`, the range on that side is considered unbounded. If both bounds are `null`, the method returns `true`.

### `public void AssessHealthStatus`
Evaluates `Value` against `NormalRangeLow` and `NormalRangeHigh` and updates `HealthStatus` accordingly. The exact status strings assigned depend on whether the value is below, within, or above the normal range. Does not throw.

### `public void UpdateValue`
Accepts a new `double` value and updates `Value`, `PreviousValue`, `PercentageChange`, `Trend` (if computable), and `LastUpdated`. Recalculates `HealthStatus` by calling `AssessHealthStatus` internally. Throws `ArgumentException` if the provided value is `double.NaN` or `double.Infinity`.

### `public void AddDataSource`
Appends a non-null, non-empty source identifier string to the `DataSources` list. Throws `ArgumentNullException` if the argument is null, and `ArgumentException` if it is empty or whitespace.

## Usage

```csharp
// Create a metric and populate it with a new reading
var glucose = new HealthMetric
{
    MetricName = "Fasting Blood Glucose",
    Unit = "mg/dL",
    NormalRangeLow = 70,
    NormalRangeHigh = 100
};

glucose.UpdateValue(108.0);
glucose.AddDataSource("LabCorp-2025-03-15");
glucose.ConfidenceScore = 95;
glucose.SampleDays = 1;

if (!glucose.IsInNormalRange)
{
    Console.WriteLine($"Alert: {glucose.MetricName} is {glucose.HealthStatus}");
}

var summary = glucose.GetSummary();
```

```csharp
// Track a metric across multiple updates and export
var hr = new HealthMetric
{
    MetricName = "Resting Heart Rate",
    Unit = "bpm",
    NormalRangeLow = 60,
    NormalRangeHigh = 100,
    DataSources = { "AppleWatch-Series9" }
};

hr.UpdateValue(72.0);
hr.UpdateValue(68.0);   // PreviousValue becomes 72, PercentageChange computed
hr.UpdateValue(74.0);

Console.WriteLine($"Current: {hr.Value} {hr.Unit}");
Console.WriteLine($"Change: {hr.PercentageChange:F1}%");
Console.WriteLine($"Trend: {hr.Trend}");
Console.WriteLine($"Valid: {hr.IsValid}");
```

## Notes

- `UpdateValue` is the primary mutator for measurement data; it chains internal state transitions and must only receive finite numeric inputs. Passing `NaN` or infinity will throw immediately.
- `IsInNormalRange` treats a missing bound as unbounded. A metric with both bounds set to `null` is always considered in range, which may not be clinically meaningful—callers should verify that at least one bound is set if range evaluation is required.
- `PercentageChange` becomes `null` when `PreviousValue` is zero or `null`, avoiding division-by-zero. Callers should null-check before displaying percentage change.
- `DataSources` is initialised as an empty list and never null. `AddDataSource` enforces non-null, non-whitespace entries.
- `GetSummary` always returns a dictionary, even when `IsValid` returns `false`. Consumers should check `IsValid` before relying on summary fields for clinical decisions.
- This type is not thread-safe. Concurrent calls to `UpdateValue`, `AddDataSource`, or property setters must be externally synchronised.
