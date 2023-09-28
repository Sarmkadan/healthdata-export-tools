# ActivityData

Represents a single recorded physical activity with metrics and optional route data. Used to encapsulate exercise sessions such as running, cycling, or hiking for export, validation, and analysis.

## API

### `public string ActivityType`
The type or category of the activity (e.g., "Running", "Cycling", "Hiking"). Must not be null or empty.

### `public DateTime StartTime`
The UTC timestamp when the activity began. Used as a primary identifier and for chronological ordering.

### `public DateTime EndTime`
The UTC timestamp when the activity concluded. Must be equal to or later than `StartTime`.

### `public int DurationMinutes`
Total duration of the activity in whole minutes, calculated as the difference between `EndTime` and `StartTime`. Always non-negative.

### `public double DistanceKm`
Total distance covered during the activity in kilometers. Must be non-negative.

### `public double? AveragePaceMinPerKm`
Average pace in minutes per kilometer, or `null` if not available. If set, must be non-negative.

### `public double? AverageSpeedKmh`
Average speed in kilometers per hour, or `null` if not available. If set, must be non-negative.

### `public double? MaximumSpeedKmh`
Peak speed observed during the activity in kilometers per hour, or `null` if not available. If set, must be non-negative.

### `public int CaloriesBurned`
Estimated calories burned during the activity. Must be non-negative.

### `public int? AverageHeartRate`
Average heart rate in beats per minute during the activity, or `null` if not available. If set, must be non-negative.

### `public int? MaximumHeartRate`
Peak heart rate in beats per minute observed during the activity, or `null` if not available. If set, must be non-negative.

### `public int? ElevationGainMeters`
Total elevation gain in meters, or `null` if not available. If set, must be non-negative.

### `public int? ElevationLossMeters`
Total elevation loss in meters, or `null` if not available. If set, must be non-negative.

### `public int? IntensityLevel`
Subjective intensity level on a normalized scale (e.g., 1–10), or `null` if not available. If set, must be between 1 and 10 inclusive.

### `public double? Rating`
User-provided rating of the activity (e.g., 0.0 to 5.0), or `null` if not available. If set, must be within the valid range.

### `public string? Description`
Optional free-text description or notes about the activity.

### `public List<GpsPoint> RoutePoints`
Ordered list of GPS coordinates recorded during the activity. May be empty. Each point must have valid latitude and longitude.

### `public override bool IsValid`
Validates all required fields and constraints. Returns `true` if all invariants are satisfied; otherwise, returns `false`. Does not throw.

### `public override Dictionary<string, object> GetSummary`
Returns a dictionary containing a standardized summary of the activity’s key metrics. Includes keys such as `"ActivityType"`, `"StartTime"`, `"DurationMinutes"`, `"DistanceKm"`, `"CaloriesBurned"`, and optional fields if present. Never throws.

### `public int CalculateIntensity`
Computes a derived intensity score based on heart rate, pace, and elevation data. Returns a non-negative integer. Uses internal heuristics and may return 0 if insufficient data is available.

## Usage
