# SleepDataExtensions
The `SleepDataExtensions` class provides a set of static methods for analyzing and processing sleep data. These methods can be used to calculate various sleep metrics, such as sleep efficiency, debt, and quality scores, as well as to format sleep duration and determine if a sleep period is restorative.

## API
* `public static double GetLightSleepPercentage`: Calculates the percentage of light sleep in a given sleep period. This method takes no parameters and returns a `double` value between 0 and 100. It does not throw any exceptions.
* `public static double GetAwakePercentage`: Calculates the percentage of time spent awake in a given sleep period. This method takes no parameters and returns a `double` value between 0 and 100. It does not throw any exceptions.
* `public static double? GetSleepEfficiency`: Calculates the sleep efficiency of a given sleep period. This method takes no parameters and returns a nullable `double` value between 0 and 100. It returns `null` if the sleep efficiency cannot be calculated.
* `public static bool IsRestorativeSleep`: Determines if a given sleep period is restorative. This method takes no parameters and returns a `bool` value indicating whether the sleep is restorative. It does not throw any exceptions.
* `public static int? GetSleepDebt`: Calculates the sleep debt of a given sleep period. This method takes no parameters and returns a nullable `int` value representing the sleep debt in minutes. It returns `null` if the sleep debt cannot be calculated.
* `public static double? GetDeepToRemRatio`: Calculates the ratio of deep sleep to REM sleep in a given sleep period. This method takes no parameters and returns a nullable `double` value representing the ratio. It returns `null` if the ratio cannot be calculated.
* `public static string GetFormattedDuration`: Formats the duration of a given sleep period. This method takes no parameters and returns a `string` value representing the formatted duration. It does not throw any exceptions.
* `public static bool MeetsMinimumDuration`: Determines if a given sleep period meets the minimum duration requirement. This method takes no parameters and returns a `bool` value indicating whether the sleep meets the minimum duration. It does not throw any exceptions.
* `public static double? CalculateWeightedSleepScore`: Calculates a weighted sleep score for a given sleep period. This method takes no parameters and returns a nullable `double` value representing the sleep score. It returns `null` if the sleep score cannot be calculated.

## Usage
```csharp
// Example 1: Calculating sleep metrics
var sleepData = new SleepData(); // assume SleepData is a class with sleep data properties
var lightSleepPercentage = SleepDataExtensions.GetLightSleepPercentage();
var sleepEfficiency = SleepDataExtensions.GetSleepEfficiency();
Console.WriteLine($"Light sleep percentage: {lightSleepPercentage}%");
Console.WriteLine($"Sleep efficiency: {sleepEfficiency}%");

// Example 2: Determining restorative sleep and formatting duration
var isRestorative = SleepDataExtensions.IsRestorativeSleep();
var formattedDuration = SleepDataExtensions.GetFormattedDuration();
Console.WriteLine($"Is restorative sleep: {isRestorative}");
Console.WriteLine($"Sleep duration: {formattedDuration}");
```

## Notes
The `SleepDataExtensions` class provides a set of static methods that can be used to analyze and process sleep data. These methods are designed to be thread-safe, as they do not rely on any instance state. However, the accuracy of the calculations depends on the quality of the input sleep data. In cases where the input data is incomplete or invalid, the methods may return `null` or throw exceptions. Additionally, the methods that calculate ratios and percentages may return values outside the expected range if the input data is extreme. It is recommended to validate the input data and handle any exceptions or edge cases accordingly.
