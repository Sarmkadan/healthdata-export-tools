## ActivityData

The `ActivityData` class represents a collection of activity metrics derived from wearable devices or health monitoring systems. It tracks various activity indicators such as activity type, start/end time, duration, distance, average pace, and calories burned. This data is crucial for assessing physical activity and detecting potential health risks.

### Properties

* `ActivityType`: The type of activity (e.g., walking, running, cycling)
* `StartTime` and `EndTime`: The start and end times of the activity
* `DurationMinutes`: The duration of the activity in minutes
* `DistanceKm`: The distance covered during the activity in kilometers
* `AveragePaceMinPerKm`: The average pace of the activity in minutes per kilometer
* `CaloriesBurned`: The estimated calories burned during the activity
* `AverageHeartRate` and `MaximumHeartRate`: The average and maximum heart rates during the activity
* `ElevationGainMeters` and `ElevationLossMeters`: The elevation gain and loss during the activity in meters
* `IntensityLevel`: The intensity level of the activity (e.g., low, moderate, high)
* `Rating`: A subjective rating of the activity (e.g., 1-5)
* `Description`: A brief description of the activity

### Usage Example
```csharp
using HealthDataExportTools.Domain.Models;

// Create a new ActivityData instance
var activityData = new ActivityData
{
    ActivityType = "Running",
    StartTime = DateTime.Now,
    EndTime = DateTime.Now.AddMinutes(30),
    DurationMinutes = 30,
    DistanceKm = 5.2,
    AveragePaceMinPerKm = 6.5,
    CaloriesBurned = 200,
    AverageHeartRate = 120,
    MaximumHeartRate = 140,
    ElevationGainMeters = 100,
    ElevationLossMeters = 50,
    IntensityLevel = 3,
    Rating = 4,
    Description = "Morning run"
};

// Access activity data properties
Console.WriteLine($"Activity Type: {activityData.ActivityType}");
Console.WriteLine($"Start Time: {activityData.StartTime}");
Console.WriteLine($"End Time: {activityData.EndTime}");
Console.WriteLine($"Duration: {activityData.DurationMinutes} minutes");
Console.WriteLine($"Distance: {activityData.DistanceKm} km");
Console.WriteLine($"Average Pace: {activityData.AveragePaceMinPerKm} min/km");
Console.WriteLine($"Calories Burned: {activityData.CaloriesBurned}");
Console.WriteLine($"Average Heart Rate: {activityData.AverageHeartRate} bpm");
Console.WriteLine($"Maximum Heart Rate: {activityData.MaximumHeartRate} bpm");
Console.WriteLine($"Elevation Gain: {activityData.ElevationGainMeters} meters");
Console.WriteLine($"Elevation Loss: {activityData.ElevationLossMeters} meters");
Console.WriteLine($"Intensity Level: {activityData.IntensityLevel}");
Console.WriteLine($"Rating: {activityData.Rating}");
Console.WriteLine($"Description: {activityData.Description}");
```