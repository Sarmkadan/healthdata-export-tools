# MetricTimeSeries

`MetricTimeSeries` is an immutable data structure that represents a single metric sampled over time. It stores the metric’s identifier and a read‑only collection of its observed values, providing basic validation and a string representation for logging or display.

## API

### Constructor
```csharp
public MetricTimeSeries(string metricName, IReadOnlyList<T> values)
```
Creates a new `MetricTimeSeries` instance.  
- **metricName** – The name of the metric; must not be `null`.  
- **values** – The time‑ordered samples of the metric; must not be `null`.  
**Returns:** A new `MetricTimeSeries` with the supplied name and values.  
**Throws:**  
- `ArgumentNullException` if `metricName` is `null`.  
- `ArgumentNullException` if `values` is `null`.

### MetricName property
```csharp
public string MetricName { get; }
```
Gets the identifier of the metric.  
**Returns:** The metric name supplied to the constructor.  
**Throws:** None.

### Values property
```csharp
public IReadOnlyList<T> Values { get; }
```
Gets the read‑only list of metric samples.  
**Returns:** The sequence of values supplied to the constructor.  
**Throws:** None.

### HasSufficientData property
```csharp
public bool HasSufficientData { get; }
```
Indicates whether the series contains enough data points for meaningful statistical analysis (e.g., at least two samples).  
**Returns:** `true` if the series meets the sufficiency criterion; otherwise `false`.  
**Throws:** None.

### ToString method
```csharp
public override string ToString()
```
Returns a human‑readable representation of the metric series, typically including the metric name and the number of samples.  
**Parameters:** None.  
**Returns:** A string describing the instance.  
**Throws:** None.

### CorrelationPair nested type
```csharp
public readonly record struct CorrelationPair
```
An immutable value type used to hold the result of a correlation computation between two metrics. As a `readonly record struct`, it is safely shared across threads without external synchronization.

### LaggedCorrelationResult nested type
```csharp
public sealed record LaggedCorrelationResult
```
An immutable record type that captures the outcome of a lagged correlation analysis (e.g., correlation coefficient at a specific lag). Its members are defined by the record’s constructor and are read‑only.

## Usage

### Example 1: Basic creation and inspection
```csharp
using System.Collections.Generic;

// Assume T is double for illustration.
var temperatures = new List<double> { 21.3, 22.1, 22.8, 23.0, 22.5 };
var tempSeries = new MetricTimeSeries("RoomTemperature", (IReadOnlyList<double>)temperatures);

if (tempSeries.HasSufficientData)
{
    Console.WriteLine(tempSeries.ToString());
    // Output might be: "RoomTemperature: 5 samples"
}
else
{
    Console.WriteLine("Insufficient data for analysis.");
}
```

### Example 2: Using the nested correlation types
```csharp
// Suppose a method elsewhere returns a CorrelationPair and a LaggedCorrelationResult.
CorrelationPair pair = ComputeCorrelation(seriesA, seriesB);
LaggedCorrelationResult lagResult = ComputeLaggedCorrelation(seriesA, seriesB, maxLag: 5);

// Both types are immutable; they can be safely read from multiple threads.
Console.WriteLine($"Correlation: {pair.Correlation}");
Console.WriteLine($"Best lag: {lagResult.Lag} with value {lagResult.Correlation}");
```

## Notes
- The constructor validates that neither the metric name nor the values list is `null`; passing `null` for either argument throws an `ArgumentNullException`.  
- An empty `values` list is permitted; `HasSufficientData` will return `false` in that case, reflecting the lack of sufficient samples for analysis.  
- As a `readonly record`, `MetricTimeSeries` is immutable; all its properties return the values supplied at construction and cannot be altered after instantiation. This immutability makes the type inherently thread‑safe for concurrent read operations.  
- The nested `CorrelationPair` (a `readonly record struct`) and `LaggedCorrelationResult` (a `sealed record`) are also immutable and share the same thread‑safety guarantees. No additional synchronization is required when accessing their members from multiple threads.  
- The generic type `T` of the `Values` list is not specified in the public surface; consumers should treat it as the concrete type used when the series was created (e.g., `double`, `float`, or a custom metric value type). Mis‑matching the expected type will result in compile‑time errors.
