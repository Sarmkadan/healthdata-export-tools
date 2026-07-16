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