# DomainModelTestsExtensions
The `DomainModelTestsExtensions` class provides a set of extension methods for testing the domain model of the health data export tools. These methods allow developers to easily run a variety of tests on the domain model, including tests for sleep data, steps data, heart rate and SpO2 data, and cache provider tests.

## API
The `DomainModelTestsExtensions` class includes the following public members:
* `public static void RunSleepDataTests`: Runs tests on the sleep data component of the domain model. This method takes no parameters and returns no value. It may throw exceptions if the sleep data tests fail.
* `public static void RunStepsDataTests`: Runs tests on the steps data component of the domain model. This method takes no parameters and returns no value. It may throw exceptions if the steps data tests fail.
* `public static void RunHeartRateAndSpO2Tests`: Runs tests on the heart rate and SpO2 data components of the domain model. This method takes no parameters and returns no value. It may throw exceptions if the heart rate and SpO2 data tests fail.
* `public static async Task RunCacheProviderTestsAsync`: Runs tests on the cache provider component of the domain model. This method takes no parameters and returns a Task that represents the asynchronous operation. It may throw exceptions if the cache provider tests fail.

## Usage
Here are two examples of how to use the `DomainModelTestsExtensions` class:
```csharp
// Example 1: Running sleep data tests
DomainModelTestsExtensions.RunSleepDataTests();

// Example 2: Running cache provider tests asynchronously
await DomainModelTestsExtensions.RunCacheProviderTestsAsync();
```
In the first example, the `RunSleepDataTests` method is called to run the sleep data tests. In the second example, the `RunCacheProviderTestsAsync` method is called to run the cache provider tests asynchronously.

## Notes
When using the `DomainModelTestsExtensions` class, note that the `RunSleepDataTests`, `RunStepsDataTests`, and `RunHeartRateAndSpO2Tests` methods are synchronous and may block the calling thread if the tests take a long time to run. The `RunCacheProviderTestsAsync` method, on the other hand, is asynchronous and will not block the calling thread. Additionally, all of the test methods may throw exceptions if the tests fail, so it is a good idea to wrap calls to these methods in try-catch blocks to handle any exceptions that may be thrown. The thread-safety of these methods is not guaranteed, so they should not be called from multiple threads concurrently.
