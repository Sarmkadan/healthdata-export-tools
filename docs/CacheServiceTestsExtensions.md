# CacheServiceTestsExtensions

The `CacheServiceTestsExtensions` class provides a set of static extension methods for the `CacheServiceTests` type. These methods simplify the arrangement, assertion, and teardown phases of unit tests that involve a cache service, its underlying data providers, and cached analytics. They allow test authors to fluently configure mock providers, populate or clear cache entries, verify provider call counts, and assert equivalence of cached data without repeating boilerplate code.

## API

### `WithFreshMocks`
```csharp
public static CacheServiceTests WithFreshMocks(this CacheServiceTests tests)
```
Resets all mock objects (providers, cache store, etc.) associated with the `CacheServiceTests` instance to a clean state. Returns the same instance for chaining.  
**Throws:** `InvalidOperationException` if the instance has already been disposed or if mock initialization fails.

### `VerifyProviderCallCount`
```csharp
public static CacheServiceTests VerifyProviderCallCount(this CacheServiceTests tests, int expectedCount)
```
Asserts that the underlying data provider was called exactly `expectedCount` times during the test. Returns the `CacheServiceTests` instance for chaining.  
**Throws:** `Xunit.Sdk.XunitException` (or equivalent assertion exception) when the actual call count does not match.

### `ShouldBeEquivalentTo`
```csharp
public static void ShouldBeEquivalentTo<T>(this T actual, T expected)
```
Performs a deep structural comparison between `actual` and `expected` objects of the same type. Throws if the two objects are not equivalent.  
**Throws:** `Xunit.Sdk.AssertActualExpectedException` when equivalence fails.

### `WithCachedHealthDataAsync`
```csharp
public static async Task<CacheServiceTests> WithCachedHealthDataAsync(this CacheServiceTests tests, HealthData data)
```
Asynchronously inserts the provided `HealthData` object into the cache as if it had been stored by the health data provider. Returns the `CacheServiceTests` instance after the operation completes.  
**Throws:** `ArgumentNullException` if `data` is `null`.

### `WithCachedAnalyticsAsync<T>`
```csharp
public static async Task<CacheServiceTests> WithCachedAnalyticsAsync<T>(this CacheServiceTests tests, T analyticsData)
```
Asynchronously caches an analytics object of type `T` (e.g., `AnalyticsSummary`, `AnalyticsReport`). Returns the `CacheServiceTests` instance after the operation completes.  
**Throws:** `ArgumentNullException` if `analyticsData` is `null`.

### `WithClearedCacheAsync`
```csharp
public static async Task<CacheServiceTests> WithClearedCacheAsync(this CacheServiceTests tests)
```
Asynchronously removes all entries from the cache. Returns the `CacheServiceTests` instance after the cache is cleared.  
**Throws:** `InvalidOperationException` if the cache is not accessible or has been disposed.

### `WithConfiguredProvider`
```csharp
public static CacheServiceTests WithConfiguredProvider(this CacheServiceTests tests, Action<Mock<IProvider>> configure)
```
Applies the specified `configure` action to the mock provider instance. This allows setting up return values, callbacks, or exceptions. Returns the `CacheServiceTests` instance for chaining.  
**Throws:** `ArgumentNullException` if `configure` is `null`.

### `ShouldHaveCalledProviderMethodAsync`
```csharp
public static async Task ShouldHaveCalledProviderMethodAsync(this CacheServiceTests tests, string methodName, Times times)
```
Asynchronously verifies that a specific method on the provider mock was called the number of times specified by `times`. The method is identified by its name.  
**Throws:** `ArgumentException` if `methodName` is `null` or empty; `MockException` if the invocation count does not match.

### `WithMultipleCachedItemsAsync`
```csharp
public static async Task<CacheServiceTests> WithMultipleCachedItemsAsync(this CacheServiceTests tests, IEnumerable<object> items)
```
Asynchronously caches multiple items (of heterogeneous types) into the cache. Each item is stored under a generated key. Returns the `CacheServiceTests` instance after all items are inserted.  
**Throws:** `ArgumentNullException` if `items` is `null`.

## Usage

### Example 1: Testing health data retrieval with caching
```csharp
[Fact]
public async Task GetHealthData_WhenCached_ReturnsCachedValue()
{
    var expected = new HealthData { Id = 1, Value = "cached" };

    var sut = await new CacheServiceTests()
        .WithFreshMocks()
        .WithCachedHealthDataAsync(expected)
        .WithConfiguredProvider(provider =>
            provider.Setup(p => p.FetchHealthDataAsync(It.IsAny<int>()))
                    .ReturnsAsync(new HealthData { Id = 1, Value = "from provider" }));

    var result = await sut.Service.GetHealthDataAsync(1);

    result.ShouldBeEquivalentTo(expected);
    sut.VerifyProviderCallCount(0); // provider should not be called
}
```

### Example 2: Verifying provider call count after cache miss
```csharp
[Fact]
public async Task GetAnalytics_WhenCacheEmpty_CallsProvider()
{
    var analytics = new AnalyticsSummary { Total = 42 };

    var sut = await new CacheServiceTests()
        .WithFreshMocks()
        .WithClearedCacheAsync()
        .WithConfiguredProvider(provider =>
            provider.Setup(p => p.FetchAnalyticsAsync())
                    .ReturnsAsync(analytics));

    var result = await sut.Service.GetAnalyticsAsync();

    await sut.ShouldHaveCalledProviderMethodAsync(nameof(IProvider.FetchAnalyticsAsync), Times.Once());
    result.ShouldBeEquivalentTo(analytics);
}
```

## Notes

- All methods that return `CacheServiceTests` are designed for fluent chaining. They modify the internal state of the provided instance and are **not thread-safe**. Concurrent calls on the same instance from multiple threads will produce undefined behavior.
- The `ShouldBeEquivalentTo` method performs a recursive comparison that respects collection ordering and ignores reference identity. It will throw if the types of `actual` and `expected` differ or if any nested property does not match.
- Async methods (`WithCachedHealthDataAsync`, `WithCachedAnalyticsAsync<T>`, `WithClearedCacheAsync`, `WithMultipleCachedItemsAsync`, `ShouldHaveCalledProviderMethodAsync`) must be awaited to ensure the cache operation completes before proceeding.
- When using `WithConfiguredProvider`, the `configure` action receives the mock object for the default provider. If multiple providers are registered, only the primary one is configured. Use the returned `CacheServiceTests` instance to access additional mocks if needed.
- `VerifyProviderCallCount` and `ShouldHaveCalledProviderMethodAsync` both rely on the mock framework’s verification capabilities. They will throw if the mock has not been set up or if the provider has not been invoked at all.
- Edge case: calling `WithClearedCacheAsync` on an already empty cache is a no-op and does not throw. Calling it on a disposed cache instance will throw `InvalidOperationException`.
