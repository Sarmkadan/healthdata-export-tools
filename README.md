// Health Data Export Tools

...

## SleepDataExtensions

The `SleepDataExtensions` class provides utility methods for analyzing sleep data, calculating sleep metrics, and formatting sleep-related information. It includes methods to determine sleep efficiency, awake percentage, restorative sleep status, sleep debt, and more, enabling comprehensive sleep analysis from raw sleep data.

### Usage Example

```csharp
using HealthDataExportTools.Domain.Models;

var sleepData = new SleepData
{
    TotalDuration = 360, // in minutes
    LightSleepDuration = 90,
    DeepSleepDuration = 60,
    RemSleepDuration = 30,
    Awakenings = 2
};

double efficiency = sleepData.GetSleepEfficiency() ?? 0.0;
string formattedDuration = sleepData.GetFormattedDuration();
bool isRestorative = sleepData.IsRestorativeSleep();

Console.WriteLine($"Sleep Efficiency: {efficiency:P}");
Console.WriteLine($"Total Sleep Duration: {formattedDuration}");
Console.WriteLine($"Restorative Sleep: {isRestorative}");
```

## WebhookServiceExtensions

...

