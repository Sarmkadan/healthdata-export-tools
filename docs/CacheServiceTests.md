# CacheServiceTests

The `CacheServiceTests` class serves as the unit test suite for the `CacheService` component within the `healthdata-export-tools` project. It validates the correctness of caching operations for health data and analytics by verifying that the service correctly delegates calls to the underlying cache provider, handles data retrieval and existence checks, manages statistics, and executes pattern-based key clearing. Each test method isolates a specific public API surface of the cache service to ensure expected interactions with the dependency injection layer.

## API

### `public CacheServiceTests`
Initializes a new instance of the `CacheServiceTests` class. This constructor sets up the necessary test context, including mocking frameworks and dependency injections required for the subsequent test methods. It does not accept parameters and does not return a value.

### `public async Task CacheHealthDataAsync_ShouldCallSetAsyncOnProvider`
Verifies that invoking the health data caching method on the service results in a call to the provider's `SetAsync` method.
*   **Parameters**: None (test context is managed internally).
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an assertion exception if the provider's `SetAsync` method is not invoked with the expected arguments or if the operation fails.

### `public async Task GetCachedHealthDataAsync_ShouldCallGetAsyncOnProvider`
Ensures that requesting cached health data triggers the provider's `GetAsync` method.
*   **Parameters**: None.
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an assertion exception if the provider's `GetAsync` method is not called exactly once or if the returned data does not match the mock setup.

### `public async Task ClearAllAsync_ShouldCallClearAsyncOnProvider`
Validates that the command to clear all cache entries propagates to the provider's `ClearAsync` method.
*   **Parameters**: None.
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an assertion exception if `ClearAsync` is not invoked on the underlying provider.

### `public async Task GetStatsAsync_ShouldReturnStatsFromProvider`
Confirms that retrieving cache statistics returns the data object provided by the underlying cache provider.
*   **Parameters**: None.
*   **Return Value**: A `Task` representing the asynchronous operation. The test asserts that the returned stats object matches the mock configuration.
*   **Throws**: Throws an assertion exception if the returned statistics do not match the expected provider output.

### `public async Task IsHealthDataCachedAsync_ShouldCallExistsAsyncOnProvider`
Checks that verifying the existence of cached health data results in a call to the provider's `ExistsAsync` method.
*   **Parameters**: None.
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an assertion exception if `ExistsAsync` is not called with the correct key or if the boolean result is not handled correctly.

### `public async Task CacheAnalyticsAsync_ShouldCallSetAsyncOnProvider`
Similar to health data caching, this test ensures that caching analytics data invokes the provider's `SetAsync` method with the appropriate analytics payload.
*   **Parameters**: None.
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an assertion exception if the provider interaction is missing or incorrect.

### `public async Task GetCachedAnalyticsAsync_ShouldCallGetAsyncOnProvider`
Validates that fetching cached analytics data triggers the provider's `GetAsync` method.
*   **Parameters**: None.
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an assertion exception if the retrieval logic fails to delegate to the provider.

### `public async Task ClearPatternAsync_ShouldRemoveMatchingKeys`
Tests the functionality of clearing cache entries based on a specific key pattern, ensuring the provider receives the correct pattern string.
*   **Parameters**: None.
*   **Return Value**: A `Task` representing the asynchronous operation.
*   **Throws**: Throws an assertion exception if the provider's pattern-matching clear method is not invoked with the specified pattern.

## Usage

The following examples demonstrate how the `CacheServiceTests` class is utilized within a testing framework like xUnit or NUnit.

**Example 1: Instantiating and running a specific test logic manually**
While typically executed by a test runner, the class can be instantiated to verify setup integrity in integration scenarios.

```csharp
using System.Threading.Tasks;
using HealthDataExportTools.Tests;

public class TestRunner
{
    public async Task ExecuteCacheValidation()
    {
        var tests = new CacheServiceTests();
        
        // Execute specific validation logic
        await tests.GetCachedHealthDataAsync_ShouldCallGetAsyncOnProvider();
        
        // Verify stats retrieval
        await tests.GetStatsAsync_ShouldReturnStatsFromProvider();
    }
}
```

**Example 2: Typical xUnit Test Class Structure**
In a standard project setup, these methods are decorated with test attributes and run automatically.

```csharp
using Xunit;
using HealthDataExportTools.Tests;

public class CacheServiceIntegration
{
    private readonly CacheServiceTests _cacheTests;

    public CacheServiceIntegration()
    {
        _cacheTests = new CacheServiceTests();
    }

    [Fact]
    public async Task ValidateHealthDataCaching()
    {
        await _cacheTests.CacheHealthDataAsync_ShouldCallSetAsyncOnProvider();
        await _cacheTests.IsHealthDataCachedAsync_ShouldCallExistsAsyncOnProvider();
    }

    [Fact]
    public async Task ValidateAnalyticsAndClearing()
    {
        await _cacheTests.CacheAnalyticsAsync_ShouldCallSetAsyncOnProvider();
        await _cacheTests.ClearPatternAsync_ShouldRemoveMatchingKeys();
    }
}
```

## Notes

*   **Asynchronous Execution**: All test methods are asynchronous (`async Task`). Callers must await these methods to ensure assertions are evaluated after the asynchronous operations complete. Failure to await may result in unobserved task exceptions or premature test completion.
*   **Dependency Initialization**: The constructor `CacheServiceTests()` implicitly initializes mock objects for the cache provider. If the internal initialization logic relies on specific environment variables or configuration files not present in the test context, the constructor or subsequent methods may throw initialization exceptions.
*   **Thread Safety**: As with most unit test classes, `CacheServiceTests` is not designed to be thread-safe for concurrent execution of its methods on the same instance. Test runners typically instantiate a new class per test method to ensure isolation. Sharing a single instance across multiple threads without external synchronization may lead to race conditions regarding mock state verification.
*   **Pattern Matching**: The `ClearPatternAsync_ShouldRemoveMatchingKeys` test assumes the underlying provider supports pattern-based deletion. If the actual implementation of the provider changes to不支持 patterns, this test will fail, indicating a contract breach between the service and the provider.
