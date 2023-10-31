# ActivityDataExtensions

Static extension methods that derive common running‑and‑cycling metrics from activity data.

## API

### GetPacePerKm
**Purpose** – Calculates the average pace per kilometre for an activity.  
**Parameters** – `activity`: an instance containing the total distance and elapsed time.  
**Return value** – A `TimeSpan` representing the time taken to cover one kilometre.  
**Exceptions** –  
- `ArgumentNullException` if `activity` is `null`.  
- `InvalidOperationException` if the activity’s distance is zero or not set, making the calculation impossible.

### GetHeartRateZone
**Purpose** – Determines the heart‑rate zone (1‑5) based on a percentage of the user's maximum heart rate.  
**Parameters** –  
- `activity`: an instance providing the average heart rate for the activity.  
- `maxHeartRate`: the user's maximum heart rate in beats per minute.  
**Return value** – An integer from 1 to 5 indicating the zone.  
**Exceptions** –  
- `ArgumentNullException` if `activity` is `null`.  
- `ArgumentOutOfRangeException` if `maxHeartRate` is less than or equal to zero.

### GetIntensityDescription
**Purpose** – Returns a textual description of the activity's intensity (e.g., "Easy", "Moderate", "Hard").  
**Parameters** – `activity`: an instance containing the average heart rate and, optionally, the user's maximum heart rate.  
**Return value** – A string describing the intensity level.  
**Exceptions** –  
- `ArgumentNullException` if `activity` is `null`.  
- `InvalidOperationException` if the necessary heart‑rate data is missing.

### GetElevationGainPerKm
**Purpose** – Computes the average elevation gain per kilometre of the activity.  
**Parameters** – `activity`: an instance providing total distance and cumulative elevation gain.  
**Return value** – A `double` representing metres of elevation gained per kilometre.  
**Exceptions** –  
- `ArgumentNullException` if `activity` is `null`.  
- `InvalidOperationException` if the activity’s distance is zero or not set.

## Usage

```csharp
using HealthDataExportTools; // namespace containing ActivityDataExtensions
using System;

// Assume `run` is a populated ActivityData instance
ActivityData run = GetRunFromSource();

TimeSpan pace = ActivityDataExtensions.GetPacePerKm(run);
Console.WriteLine($"Pace: {pace.Minutes:00}:{pace.Seconds:00} /km");

int zone = ActivityDataExtensions.GetHeartRateZone(run, maxHeartRate: 190);
Console.WriteLine($"Heart‑rate zone: {zone}");

string intensity = ActivityDataExtensions.GetIntensityDescription(run);
Console.WriteLine($"Intensity: {intensity}");

double elevationPerKm = ActivityDataExtensions.GetElevationGainPerKm(run);
Console.WriteLine($"Elevation gain: {elevationPerKm:F1} m/km");
```

```csharp
// Example handling missing data gracefully
try
{
    TimeSpan pace = ActivityDataExtensions.GetPacePerKm(null);
}
catch (ArgumentNullException ex)
{
    Console.WriteLine("Activity data required for pace calculation.");
}
```

## Notes

- All methods are **static** and contain no mutable state; therefore they are thread‑safe and can be called concurrently from multiple threads.  
- Passing `null` for the `activity` argument will always result in an `ArgumentNullException`.  
- Calculations that involve division by distance (pace, elevation gain per kilometre) will throw an `InvalidOperationException` when the distance is zero or unavailable, preventing divide‑by‑zero errors.  
- Heart‑rate zone calculation assumes a linear scaling of zones based on the supplied `maxHeartRate`; providing an unrealistic maximum (e.g., ≤ 0) triggers an `ArgumentOutOfRangeException`.  
- The intensity description depends on the presence of both average and maximum heart‑rate data; if either is missing the method throws an `InvalidOperationException`.  
- No exceptions are thrown for successful calls; return values are always valid for the given input.
