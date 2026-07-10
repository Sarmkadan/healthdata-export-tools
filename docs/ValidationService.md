# ValidationService
The `ValidationService` class is designed to validate various types of health data, including sleep, heart rate, SpO2, steps, and activity data, as well as general health metrics. It provides a set of methods to perform these validations and track any errors that occur during the process.

## API
* `ValidateSleepData`: Validates sleep data and returns a `ValidationResult` object indicating the outcome of the validation.
* `ValidateHeartRateData`: Validates heart rate data and returns a `ValidationResult` object indicating the outcome of the validation.
* `ValidateSpO2Data`: Validates SpO2 data and returns a `ValidationResult` object indicating the outcome of the validation.
* `ValidateStepsData`: Validates steps data and returns a `ValidationResult` object indicating the outcome of the validation.
* `ValidateActivityData`: Validates activity data and returns a `ValidationResult` object indicating the outcome of the validation.
* `ValidateHealthMetric`: Validates a general health metric and returns a `ValidationResult` object indicating the outcome of the validation.
* `Errors`: Gets a list of error messages that occurred during validation.
* `AddError`: Adds an error message to the list of errors.
* `ToString`: Returns a string representation of the `ValidationService` instance.

## Usage
The following examples demonstrate how to use the `ValidationService` class:
```csharp
// Example 1: Validating sleep data
var validationService = new ValidationService();
var sleepData = new SleepData(); // assume SleepData is a class representing sleep data
var validationResult = validationService.ValidateSleepData(sleepData);
if (validationResult.IsValid)
{
    Console.WriteLine("Sleep data is valid");
}
else
{
    Console.WriteLine("Sleep data is invalid: " + string.Join(", ", validationService.Errors));
}

// Example 2: Validating heart rate data and adding custom error
var heartRateData = new HeartRateData(); // assume HeartRateData is a class representing heart rate data
validationService.ValidateHeartRateData(heartRateData);
validationService.AddError("Custom error message");
Console.WriteLine("Errors: " + string.Join(", ", validationService.Errors));
```

## Notes
The `ValidationService` class is not thread-safe, as it maintains a list of errors that can be modified by multiple threads. When using this class in a multi-threaded environment, it is recommended to create a new instance for each thread or to synchronize access to the `Errors` list. Additionally, the `Validate*` methods may throw exceptions if the input data is null or invalid, and the `AddError` method may throw an exception if the error message is null. The `ToString` method returns a string representation of the `ValidationService` instance, which can be useful for debugging purposes.
