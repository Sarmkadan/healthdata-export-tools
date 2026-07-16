## ActivityData

The `ActivityData` class represents a collection of activity metrics derived from wearable devices or health monitoring systems. It tracks various activity indicators such as activity type, start/end time, duration, distance, average pace, and calories burned. This data is crucial for assessing physical activity and detecting potential health risks.


## SleepData

The `SleepData` class represents a single sleep session with detailed metrics captured from wearable devices or sleep tracking systems. It tracks various sleep indicators such as sleep start/end times, duration, sleep stages (deep, light, REM), awake periods, and quality assessments. This data is crucial for analyzing sleep patterns and identifying potential sleep issues.

### Properties

* `SleepStart` and `SleepEnd`: The start and end times of the sleep session
* `DurationMinutes`: The total duration of the sleep session in minutes
* `DeepSleepMinutes`, `LightSleepMinutes`, `RemSleepMinutes`, `AwakeMinutes`: Durations of different sleep stages and awake periods in minutes
* `Quality`: The overall quality assessment of the sleep session (Excellent, Good, Average, Poor)
* `Score`: An optional sleep score assigned by the device (typically 0-100)
* `CycleCount`: An optional count of sleep cycles detected
* `IsNap`: Indicates if this is a short nap rather than a full sleep session
* `AverageHeartRate`: The average heart rate during the sleep session (optional)

### Methods

* `CalculateQuality()`: Calculate sleep quality based on duration and sleep stage percentages
* `IsValid()`: Validate sleep data integrity and plausibility
* `GetSummary()`: Get a summary of the sleep session's key metrics
* `GetDeepSleepPercentage()`: Calculate the percentage of time spent in deep sleep
* `GetRemSleepPercentage()`: Calculate the percentage of time spent in REM sleep

### Usage Example

```csharp
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Domain.Enums;

// Create a new SleepData instance for a full night's sleep
var sleepData = new SleepData
{
    SleepStart = new DateTime(2024, 3, 15, 22, 30, 0), // 10:30 PM
    SleepEnd = new DateTime(2024, 3, 16, 6, 45, 0),   // 6:45 AM
    DurationMinutes = 495,
    DeepSleepMinutes = 90,
    LightSleepMinutes = 270,
    RemSleepMinutes = 75,
    AwakeMinutes = 60,
    Quality = SleepQuality.Good,
    Score = 82,
    CycleCount = 4,
    IsNap = false,
    AverageHeartRate = 58,
    DeviceId = "SLEEP-001",
    RecordDate = new DateTime(2024, 3, 16)
};

// Access sleep data properties
Console.WriteLine($"Sleep Date: {sleepData.RecordDate:yyyy-MM-dd}");
Console.WriteLine($"Duration: {sleepData.DurationMinutes} minutes");
Console.WriteLine($"Start Time: {sleepData.SleepStart:HH:mm}");
Console.WriteLine($"End Time: {sleepData.SleepEnd:HH:mm}");
Console.WriteLine($"Deep Sleep: {sleepData.DeepSleepMinutes} minutes ({sleepData.GetDeepSleepPercentage():F1}%)");
Console.WriteLine($"Light Sleep: {sleepData.LightSleepMinutes} minutes");
Console.WriteLine($"REM Sleep: {sleepData.RemSleepMinutes} minutes ({sleepData.GetRemSleepPercentage():F1}%)");
Console.WriteLine($"Awake Time: {sleepData.AwakeMinutes} minutes");
Console.WriteLine($"Quality: {sleepData.Quality}");
Console.WriteLine($"Score: {sleepData.Score}");
Console.WriteLine($"Cycles: {sleepData.CycleCount}");
Console.WriteLine($"Is Nap: {sleepData.IsNap}");
Console.WriteLine($"Average Heart Rate: {sleepData.AverageHeartRate} bpm");

// Calculate quality programmatically
var calculatedQuality = sleepData.CalculateQuality();
Console.WriteLine($"Calculated Quality: {calculatedQuality}");

// Validate data
if (sleepData.IsValid())
{
    Console.WriteLine("Sleep data is valid and ready for analysis");
}

// Get summary data
var summary = sleepData.GetSummary();
foreach (var item in summary)
{
    Console.WriteLine($"{item.Key}: {item.Value}");
}
```

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

## HealthDataRecord

The `HealthDataRecord` abstract base class provides common properties and methods for all health data records in the system. It serves as the foundation for tracking health metrics from wearable devices and monitoring systems, ensuring consistent data structure and validation across different record types.


### Properties

* `Id`: Unique identifier for the record (auto-generated as GUID)
* `CreatedUtc`: Record creation timestamp in UTC
* `ModifiedUtc`: Record modification timestamp in UTC
* `RecordDate`: Date this health record pertains to
* `DeviceId`: Source device that recorded this data
* `FirmwareVersion`: Firmware version of the recording device (optional)
* `IsValidated`: Indicates if record data has been validated
* `Notes`: Additional metadata or notes about the record (optional)

### Methods

* `MarkAsValidated()`: Mark this record as validated after passing quality checks
* `IsValid()`: Check if the record contains valid data for analysis (abstract - must be implemented by derived classes)
* `GetSummary()`: Get a summary of the record's key metrics (abstract - must be implemented by derived classes)
* `Touch()`: Update the modification timestamp to now

### Usage Example

```csharp
using HealthDataExportTools.Domain.Models;

// Create a concrete HealthDataRecord implementation
public class SampleHealthRecord : HealthDataRecord
{
    public double HeartRate { get; set; }
    public double Steps { get; set; }

    public override bool IsValid()
    {
        return HeartRate > 0 && Steps >= 0 && !string.IsNullOrEmpty(DeviceId);
    }

    public override Dictionary<string, object> GetSummary()
    {
        return new Dictionary<string, object>
        {
            { "DeviceId", DeviceId },
            { "RecordDate", RecordDate },
            { "HeartRate", HeartRate },
            { "Steps", Steps },
            { "IsValidated", IsValidated }
        };
    }
}

// Usage
var record = new SampleHealthRecord
{
    RecordDate = DateTime.Today,
    DeviceId = "WATCH-001",
    FirmwareVersion = "2.1.4",
    HeartRate = 75.5,
    Steps = 8423
};

// Access properties
Console.WriteLine($"Record ID: {record.Id}");
Console.WriteLine($"Created: {record.CreatedUtc}");
Console.WriteLine($"Device: {record.DeviceId}");
Console.WriteLine($"Firmware: {record.FirmwareVersion}");
Console.WriteLine($"Record Date: {record.RecordDate:yyyy-MM-dd}");

// Validate and mark as validated
if (record.IsValid())
{
    record.MarkAsValidated();
    Console.WriteLine("Record validated successfully");
}

// Get summary data
var summary = record.GetSummary();
foreach (var item in summary)
{
    Console.WriteLine($"{item.Key}: {item.Value}");
}

// Update modification timestamp
record.Touch();
```