# SleepData

Represents a single sleep session record, capturing start and end times, duration, sleep stage breakdowns, quality metrics, and optional physiological data such as heart rate. This type provides calculated properties for sleep quality classification and percentage breakdowns of deep and REM sleep relative to total duration.

## API

### `public DateTime SleepStart`

The date and time when the sleep session began.

### `public DateTime SleepEnd`

The date and time when the sleep session ended.

### `public int DurationMinutes`

Total duration of the sleep session in minutes, derived from `SleepEnd` minus `SleepStart`.

### `public int DeepSleepMinutes`

Number of minutes spent in deep sleep stage during the session.

### `public int LightSleepMinutes`

Number of minutes spent in light sleep stage during the session.

### `public int RemSleepMinutes`

Number of minutes spent in REM sleep stage during the session.

### `public int AwakeMinutes`

Number of minutes spent awake during the sleep session.

### `public SleepQuality Quality`

The sleep quality classification assigned to this session, represented by the `SleepQuality` enum.

### `public int? Score`

An optional numeric score assigned to the sleep session. Null when no score is available.

### `public int? CycleCount`

The number of complete sleep cycles detected during the session, if available. Null when cycle data is not present.

### `public bool IsNap`

Indicates whether this sleep session is classified as a nap rather than a full night's sleep.

### `public int? AverageHeartRate`

The average heart rate in beats per minute recorded during the sleep session. Null when heart rate data is unavailable.

### `public SleepQuality CalculateQuality`

Computes and returns the sleep quality classification based on the session's stage distribution and duration. This property performs the calculation each time it is accessed.

### `public override bool IsValid`

Returns `true` if the sleep session data is internally consistent and meets validity criteria (e.g., `SleepEnd` occurs after `SleepStart`, stage minutes do not exceed total duration). Returns `false` otherwise.

### `public override Dictionary<string, object> GetSummary`

Returns a dictionary containing key-value pairs summarizing the sleep session. Keys include string representations of property names, and values are their corresponding data, with nullable integers represented as either their value or `null`.

### `public double GetDeepSleepPercentage`

Calculates the percentage of total sleep duration spent in deep sleep. Returns a `double` between 0.0 and 100.0. If `DurationMinutes` is zero, the return value is 0.0.

### `public double GetRemSleepPercentage`

Calculates the percentage of total sleep duration spent in REM sleep. Returns a `double` between 0.0 and 100.0. If `DurationMinutes` is zero, the return value is 0.0.

## Usage

### Example 1: Basic Sleep Session Analysis

```csharp
var sleep = new SleepData
{
    SleepStart = new DateTime(2025, 3, 15, 23, 15, 0),
    SleepEnd = new DateTime(2025, 3, 16, 7, 5, 0),
    DeepSleepMinutes = 95,
    LightSleepMinutes = 240,
    RemSleepMinutes = 85,
    AwakeMinutes = 50,
    IsNap = false,
    Score = 87,
    CycleCount = 4,
    AverageHeartRate = 58
};

if (sleep.IsValid)
{
    Console.WriteLine($"Duration: {sleep.DurationMinutes} min");
    Console.WriteLine($"Quality: {sleep.CalculateQuality}");
    Console.WriteLine($"Deep Sleep: {sleep.GetDeepSleepPercentage():F1}%");
    Console.WriteLine($"REM Sleep: {sleep.GetRemSleepPercentage():F1}%");

    var summary = sleep.GetSummary();
    foreach (var kvp in summary)
    {
        Console.WriteLine($"{kvp.Key}: {kvp.Value ?? "N/A"}");
    }
}
```

### Example 2: Handling a Nap with Missing Data

```csharp
var nap = new SleepData
{
    SleepStart = new DateTime(2025, 3, 16, 14, 0, 0),
    SleepEnd = new DateTime(2025, 3, 16, 14, 45, 0),
    DeepSleepMinutes = 0,
    LightSleepMinutes = 30,
    RemSleepMinutes = 5,
    AwakeMinutes = 10,
    IsNap = true,
    Score = null,
    CycleCount = null,
    AverageHeartRate = null
};

if (nap.IsValid)
{
    double deepPct = nap.GetDeepSleepPercentage();
    double remPct = nap.GetRemSleepPercentage();

    Console.WriteLine($"Nap quality: {nap.CalculateQuality}");
    Console.WriteLine($"Deep sleep constitutes {deepPct:F1}% of the nap");
    Console.WriteLine($"REM sleep constitutes {remPct:F1}% of the nap");

    if (nap.Score.HasValue)
    {
        Console.WriteLine($"Score: {nap.Score.Value}");
    }
    else
    {
        Console.WriteLine("No score available for this nap.");
    }
}
```

## Notes

- `GetDeepSleepPercentage` and `GetRemSleepPercentage` guard against division by zero when `DurationMinutes` is zero, returning 0.0 in that case. No exception is thrown.
- `CalculateQuality` recomputes the quality classification on every access. For repeated use within a method, store the result in a local variable to avoid redundant computation.
- `IsValid` does not throw exceptions; it returns `false` for any inconsistent state, including negative stage minutes or `SleepEnd` earlier than or equal to `SleepStart`.
- `GetSummary` includes all public member values in its dictionary. Nullable fields appear with their underlying value or a null reference as the dictionary value.
- This type is not inherently thread-safe. If instances are shared across threads, external synchronization is required to prevent race conditions when reading or modifying properties concurrently.
