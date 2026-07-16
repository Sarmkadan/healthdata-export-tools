// existing content ...

## HeartRateData

The `HeartRateData` class represents a collection of heart rate measurements collected throughout a day. It tracks various heart rate indicators such as minimum, maximum, and average heart rates, as well as heart rate variability and stress levels. This data is crucial for assessing cardiovascular health and detecting potential heart risks.

### Usage Example

```csharp
using HealthDataExportTools.Domain.Models;

// Create a new HeartRateData instance
var heartRateData = new HeartRateData
{
    MinimumBpm = 60,
    MaximumBpm = 180,
    AverageBpm = 120,
    RestingBpm = 70,
    MeasurementCount = 100,
    HeartRateVariability = 50.0,
    StressLevel = 20,
    CardioZoneMinutes = 30,
    FatBurnZoneMinutes = 45,
    ZoneMinutes = new int[] { 10, 20, 30, 40, 50 }
};

// Create a new HeartRateMeasurement instance
var measurement = new HeartRateMeasurement
{
    Timestamp = DateTime.Now,
    Bpm = 120,
    Zone = HeartRateZone.Zone3
};

// Add measurement to heart rate data
heartRateData.AddMeasurement(measurement);

// Assess stress level
var stressLevel = heartRateData.AssessStressLevel();
Console.WriteLine($"Stress Level: {stressLevel}");

// Calculate heart rate reserve
var heartRateReserve = heartRateData.CalculateHeartRateReserve();
Console.WriteLine($"Heart Rate Reserve: {heartRateReserve}");

// Get summary of heart rate statistics
var summary = heartRateData.GetSummary();
foreach (var item in summary)
{
    Console.WriteLine($"{item.Key}: {item.Value}");
}

// Classify a BPM reading into a heart rate zone
var zone = HeartRateData.ClassifyZone(140, 200);
Console.WriteLine($"Heart Rate Zone: {zone}");

// Populate zone minutes from seconds
var zoneSeconds = new int[] { 600, 1200, 1800, 2400, 3000 };
heartRateData.SetZoneMinutesFromSeconds(zoneSeconds);
```

## StepsData

`StepsData` captures daily step‑count and activity metrics. It stores raw values such as total steps, distance, calories, and active minutes, and provides helper methods to validate the data, compute summaries, update goal achievement, and analyse hourly step trends.

### Usage Example

```csharp
using HealthDataExportTools.Domain.Models;

// Create a new StepsData instance and populate basic fields
var steps = new StepsData
{
    TotalSteps = 12_345,
    DistanceKm = 9.8,
    CaloriesBurned = 560,
    DailyGoal = 10_000,
    ActiveMinutes = 85,
    WalkingMinutes = 45,
    RunningMinutes = 20,
    GoalAchieved = false // will be updated by UpdateGoalAchievement()
};

// Update the goal‑achievement percentage based on the total steps
steps.UpdateGoalAchievement();
Console.WriteLine($"Goal achievement: {steps.GoalAchievementPercentage}% (Achieved: {steps.GoalAchieved})");

// Add hourly step counts
steps.SetHourlySteps(9, 1_200);   // 9 AM
steps.SetHourlySteps(12, 2_500); // noon
steps.SetHourlySteps(18, 3_000); // 6 PM

// Compute average steps per active hour
double avgPerHour = steps.GetAverageStepsPerActiveHour();
Console.WriteLine($"Average steps per active hour: {avgPerHour:F1}");

// Find the most active hour
var mostActive = steps.GetMostActiveHour();
if (mostActive.HasValue)
{
    Console.WriteLine($"Most active hour: {mostActive.Value.Hour}:00 with {mostActive.Value.Steps} steps");
}

// Estimate calories burned for a 75 kg person
int estimatedCalories = steps.EstimateCalories(weightKg: 75);
Console.WriteLine($"Estimated calories (75 kg): {estimatedCalories} kcal");

// Validate the data
bool isValid = steps.IsValid();
Console.WriteLine($"Steps data valid: {isValid}");

// Get a human‑readable summary
var summary = steps.GetSummary();
foreach (var kvp in summary)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}
```

## BackgroundTaskScheduler

The `BackgroundTaskScheduler` class provides a thread-safe mechanism for scheduling both one-time and recurring background tasks. It manages task execution, tracking task state and execution history, and supports cancellation of scheduled tasks.



### Usage Example

```csharp
using HealthDataExportTools.Tasks;
using Microsoft.Extensions.Logging;

// Create a logger (in real application, use dependency injection)
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<BackgroundTaskScheduler>();

// Initialize the scheduler
var scheduler = new BackgroundTaskScheduler(logger);

// Schedule a one-time task to run at a specific time
var oneTimeTaskId = scheduler.ScheduleOnce(
    taskName: "GenerateDailyReport",
    action: async () =>
    {
        // Your background task logic here
        Console.WriteLine("Daily report generation started...");
        await Task.Delay(1000); // Simulate work
        Console.WriteLine("Daily report completed!");
    },
    executeAt: DateTime.UtcNow.AddMinutes(5) // Run in 5 minutes
);

// Schedule a recurring task to run every hour
var recurringTaskId = scheduler.ScheduleRecurring(
    taskName: "CleanupTempFiles",
    action: async () =>
    {
        // Your cleanup logic here
        Console.WriteLine("Cleaning up temporary files...");
        await Task.Delay(500); // Simulate work
        Console.WriteLine("Cleanup completed!");
    },
    interval: TimeSpan.FromHours(1) // Run every hour
);

// Get all scheduled tasks
var allTasks = scheduler.GetScheduledTasks();
Console.WriteLine($"Total scheduled tasks: {allTasks.Count}");

// Get a specific task
var task = scheduler.GetTask(recurringTaskId);
if (task != null)
{
    Console.WriteLine($"Task: {task.Name}");
    Console.WriteLine($"Recurring: {task.IsRecurring}");
    Console.WriteLine($"Created: {task.CreatedAt}");
    Console.WriteLine($"Next execution: {task.ExecuteAt}");
    Console.WriteLine($"Interval: {task.Interval}");
    Console.WriteLine($"Executions: {task.ExecutionCount}");
}

// Cancel a task
bool cancelled = scheduler.CancelTask(oneTimeTaskId);
Console.WriteLine($"Task cancelled: {cancelled}");
```
