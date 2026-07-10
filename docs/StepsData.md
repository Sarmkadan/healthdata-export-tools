# StepsData

`StepsData` represents a daily summary of step tracking data, including metrics such as total steps, distance, calories burned, and goal achievement. It provides methods to calculate derived statistics and validate the data integrity.

## API

### `public int TotalSteps`
Gets or sets the total number of steps taken during the day. Must be a non-negative value.

### `public double DistanceKm`
Gets or sets the estimated distance traveled in kilometers. Must be a non-negative value.

### `public int CaloriesBurned`
Gets or sets the estimated calories burned during activity. Must be a non-negative value.

### `public int DailyGoal`
Gets or sets the daily step goal target. Must be a positive value.

### `public int GoalAchievementPercentage`
Gets the percentage of the daily goal achieved (0–100). Read-only.

### `public int? AverageCadence`
Gets or sets the average walking/running cadence in steps per minute. `null` indicates no data available.

### `public int? PeakStepsPerHour`
Gets or sets the highest hourly step count recorded. `null` indicates no data available.

### `public int ActiveMinutes`
Gets or sets the total minutes of active movement (walking or running).

### `public int WalkingMinutes`
Gets or sets the total minutes spent walking.

### `public int RunningMinutes`
Gets or sets the total minutes spent running.

### `public Dictionary<int, int> HourlySteps`
Gets or sets a dictionary mapping hour of day (0–23) to step count for that hour. Invalid hours (outside 0–23) are ignored during `SetHourlySteps`.

### `public bool GoalAchieved`
Gets a value indicating whether the daily step goal was achieved. Read-only.

### `public override bool IsValid`
Validates the data integrity. Returns `true` if all required fields are set and values are within valid ranges (e.g., non-negative steps, positive goal). Throws `InvalidOperationException` if `DailyGoal` is zero or negative.

### `public override Dictionary<string, object> GetSummary`
Generates a summary dictionary of key metrics for reporting. Includes `TotalSteps`, `DistanceKm`, `CaloriesBurned`, `DailyGoal`, `GoalAchievementPercentage`, `ActiveMinutes`, `WalkingMinutes`, `RunningMinutes`, and `HourlySteps`.

### `public void UpdateGoalAchievement()`
Recalculates `GoalAchievementPercentage` and `GoalAchieved` based on current `TotalSteps` and `DailyGoal`. No return value; updates internal state.

### `public void SetHourlySteps(Dictionary<int, int> stepsByHour)`
Replaces the `HourlySteps` dictionary with the provided values. Invalid hours (outside 0–23) are discarded. No return value.

### `public double GetAverageStepsPerActiveHour()`
Calculates the average steps per active hour (hours with recorded steps). Returns `0.0` if no active hours are present. Throws `InvalidOperationException` if `HourlySteps` is empty.

### `public (int Hour, int Steps)? GetMostActiveHour()`
Returns the hour with the highest step count and its step value. Returns `null` if `HourlySteps` is empty or all hours have zero steps. In case of ties, returns the earliest hour.

### `public int EstimateCalories()`
Estimates total calories burned based on activity duration and step count. Uses a heuristic combining `ActiveMinutes`, `WalkingMinutes`, `RunningMinutes`, and `TotalSteps`. Returns `0` if no activity data is available.

## Usage

```csharp
// Example 1: Creating and validating step data
var stepsData = new StepsData
{
    TotalSteps = 8500,
    DistanceKm = 6.2,
    CaloriesBurned = 310,
    DailyGoal = 10000,
    ActiveMinutes = 45,
    WalkingMinutes = 35,
    RunningMinutes = 10,
    HourlySteps = new Dictionary<int, int>
    {
        { 8, 1200 }, { 9, 1500 }, { 10, 800 },
        { 12, 2000 }, { 13, 1800 }
    }
};

stepsData.UpdateGoalAchievement();
var summary = stepsData.GetSummary();

Console.WriteLine($"Goal achieved: {stepsData.GoalAchieved}"); // False
Console.WriteLine($"Most active hour: {stepsData.GetMostActiveHour()?.Hour}"); // 12
```

```csharp
// Example 2: Updating hourly data dynamically
var data = new StepsData
{
    DailyGoal = 5000,
    HourlySteps = new Dictionary<int, int> { { 14, 300 } }
};

data.SetHourlySteps(new Dictionary<int, int>
{
    { 8, 500 }, { 9, 700 }, { 10, 600 },
    { 11, 800 }, { 12, 900 }
});

data.TotalSteps = data.HourlySteps.Values.Sum();
data.UpdateGoalAchievement();

Console.WriteLine($"Average steps per active hour: {data.GetAverageStepsPerActiveHour():F1}"); // 700.0
```

## Notes

- **Thread safety**: This type is not thread-safe. Concurrent reads/writes require external synchronization.
- **Hour validation**: `SetHourlySteps` silently discards invalid hours (outside 0–23). Ensure input data is validated before calling if strict enforcement is required.
- **Empty data**: Methods like `GetAverageStepsPerActiveHour` and `GetMostActiveHour` return default values (`0.0` or `null`) when no data is present rather than throwing. Handle these cases explicitly in calling code.
- **Goal validation**: `IsValid` enforces `DailyGoal > 0`; ensure this is set before validation.
