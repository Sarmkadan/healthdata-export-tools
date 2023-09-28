# SpO2Data

Represents a collection of oxygen saturation (SpO₂) measurements over a period, including derived statistics and reliability indicators. Used for monitoring blood oxygen levels and detecting potential hypoxia events.

## API

### Properties

#### `public int MinimumPercentage`
Gets the minimum SpO₂ percentage recorded in the measurements.
- **Type:** `int`
- **Access:** Read-only
- **Notes:** Always ≥ 0. Throws `InvalidOperationException` if `Measurements` is empty.

#### `public int MaximumPercentage`
Gets the maximum SpO₂ percentage recorded in the measurements.
- **Type:** `int`
- **Access:** Read-only
- **Notes:** Always ≤ 100. Throws `InvalidOperationException` if `Measurements` is empty.

#### `public int AveragePercentage`
Gets the average SpO₂ percentage across all measurements.
- **Type:** `int`
- **Access:** Read-only
- **Notes:** Rounded to nearest integer. Throws `InvalidOperationException` if `Measurements` is empty.

#### `public int? RestingPercentage`
Gets the SpO₂ percentage recorded during a resting state, if available.
- **Type:** `int?`
- **Access:** Read-only
- **Notes:** `null` if no resting measurement exists or if no resting state was identified.

#### `public int MeasurementCount`
Gets the total number of SpO₂ measurements in the collection.
- **Type:** `int`
- **Access:** Read-only

#### `public List<SpO2Measurement> Measurements`
Gets the list of individual SpO₂ measurements.
- **Type:** `List<SpO2Measurement>`
- **Access:** Read-only
- **Notes:** Never `null`. Empty if no measurements have been added.

#### `public int LowSpO2Events`
Gets the count of measurements where SpO₂ fell below a concerning threshold.
- **Type:** `int`
- **Access:** Read-only
- **Notes:** Threshold defined by `LowestAlertValue` if set; otherwise, system default.

#### `public int? LowestAlertValue`
Gets the lowest SpO₂ value that triggers an alert, if configured.
- **Type:** `int?`
- **Access:** Read-only
- **Notes:** `null` if no alert threshold is set.

#### `public int? ReliabilityScore`
Gets a score indicating the reliability of the data (0–100).
- **Type:** `int?`
- **Access:** Read-only
- **Notes:** `null` if reliability could not be determined.

#### `public override bool IsValid`
Indicates whether the SpO₂ data is valid based on internal checks.
- **Type:** `bool`
- **Access:** Read-only
- **Notes:** Returns `true` if data meets minimum quality criteria (e.g., sufficient measurements, plausible range).

#### `public override Dictionary<string, object> GetSummary()`
Generates a summary dictionary of key SpO₂ metrics.
- **Returns:** `Dictionary<string, object>` containing keys: `"MinimumPercentage"`, `"MaximumPercentage"`, `"AveragePercentage"`, `"RestingPercentage"`, `"MeasurementCount"`, `"LowSpO2Events"`, `"LowestAlertValue"`, `"ReliabilityScore"`, `"IsValid"`, `"Timestamp"`.
- **Throws:** `InvalidOperationException` if `Measurements` is empty.

#### `public void AddMeasurement(SpO2Measurement measurement)`
Adds a new SpO₂ measurement to the collection.
- **Parameters:**
  - `measurement` (`SpO2Measurement`): The measurement to add.
- **Throws:** `ArgumentNullException` if `measurement` is `null`.

#### `public bool HasConcerningLevels()`
Determines if the SpO₂ levels indicate a potential concern.
- **Returns:** `true` if `LowSpO2Events` > 0 or if `AveragePercentage` is below a critical threshold; otherwise, `false`.
- **Notes:** Uses internal thresholds unless overridden by `LowestAlertValue`.

#### `public double GetPercentageBelowThreshold(int threshold)`
Calculates the percentage of measurements below a specified SpO₂ threshold.
- **Parameters:**
  - `threshold` (`int`): The threshold value to compare against.
- **Returns:** Percentage of measurements with SpO₂ < `threshold` (0.0–100.0).
- **Throws:** `ArgumentOutOfRangeException` if `threshold` < 0 or > 100.

#### `public DateTime Timestamp`
Gets the timestamp when the SpO₂ data was recorded or processed.
- **Type:** `DateTime`
- **Access:** Read-only
- **Notes:** Typically set when the first measurement is added.

#### `public int Percentage`
Gets the most recent SpO₂ percentage.
- **Type:** `int`
- **Access:** Read-only
- **Notes:** Throws `InvalidOperationException` if `Measurements` is empty.

#### `public int? Confidence`
Gets a confidence score for the most recent SpO₂ reading (0–100).
- **Type:** `int?`
- **Access:** Read-only
- **Notes:** `null` if confidence could not be determined.

## Usage

### Example 1: Basic Monitoring
