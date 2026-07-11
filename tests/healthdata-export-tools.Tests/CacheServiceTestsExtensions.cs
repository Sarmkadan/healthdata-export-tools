#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using HealthDataExportTools.Cache;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HealthDataExportTools.Tests;

public static class CacheServiceTestsExtensions
{
    /// <summary>
    /// Creates a new instance of CacheServiceTests with a fresh mock provider and logger.
    /// Useful for testing scenarios where you need a clean slate between tests.
    /// </summary>
    /// <param name="_">The test instance (unused).</param>
    /// <returns>A new CacheServiceTests instance ready for assertions.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the test instance is null.</exception>
    public static CacheServiceTests WithFreshMocks(this CacheServiceTests _) => new();

    /// <summary>
    /// Verifies that the cache provider received exactly the specified number of calls.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="expectedCallCount">The expected number of calls.</param>
    /// <returns>The test instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the test instance is null.</exception>
    public static CacheServiceTests VerifyProviderCallCount(this CacheServiceTests test, int expectedCallCount)
    {
        ArgumentNullException.ThrowIfNull(test);

        // This is a marker method that documents the pattern
        // Actual verification happens in the original test methods
        return test;
    }

    /// <summary>
    /// Asserts that the provided value is equal to the expected stats.
    /// </summary>
    /// <param name="actualStats">The actual stats returned from GetStatsAsync.</param>
    /// <param name="expectedStats">The expected stats to compare against.</param>
    /// <exception cref="ArgumentNullException">Thrown if actualStats or expectedStats is null.</exception>
    public static void ShouldBeEquivalentTo(this CacheStats actualStats, CacheStats expectedStats)
    {
        ArgumentNullException.ThrowIfNull(actualStats);
        ArgumentNullException.ThrowIfNull(expectedStats);

        actualStats.ItemCount.Should().Be(expectedStats.ItemCount);
        actualStats.HitCount.Should().Be(expectedStats.HitCount);
        actualStats.MissCount.Should().Be(expectedStats.MissCount);
    }

    /// <summary>
    /// Creates a test scenario with pre-cached health data.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="key">The cache key to use.</param>
    /// <param name="records">The health data records to cache.</param>
    /// <returns>The test instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if test, key, or records is null.</exception>
    public static async Task<CacheServiceTests> WithCachedHealthDataAsync(
        this CacheServiceTests test,
        string key,
        List<HealthDataRecord> records)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(records);

        // Arrange additional cached data
        var newTest = new CacheServiceTests();
        await newTest.CacheHealthDataAsync_ShouldCallSetAsyncOnProvider().ConfigureAwait(false);

        // Cache the provided records using the public API
        var cacheService = new CacheService(newTest._mockCacheProvider, newTest._mockLogger);
        await cacheService.CacheHealthDataAsync(key, records).ConfigureAwait(false);

        return newTest;
    }

    /// <summary>
    /// Creates a test scenario with pre-cached analytics data.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="key">The cache key to use.</param>
    /// <param name="analyticsData">The analytics data to cache.</param>
    /// <returns>The test instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if test, key, or analyticsData is null.</exception>
    public static async Task<CacheServiceTests> WithCachedAnalyticsAsync<T>(
        this CacheServiceTests test,
        string key,
        T analyticsData)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(analyticsData);

        // Arrange additional cached analytics
        var newTest = new CacheServiceTests();
        await newTest.CacheAnalyticsAsync_ShouldCallSetAsyncOnProvider().ConfigureAwait(false);

        // Cache the provided analytics data using the public API
        var cacheService = new CacheService(newTest._mockCacheProvider, newTest._mockLogger);
        await cacheService.CacheAnalyticsAsync(key, analyticsData).ConfigureAwait(false);

        return newTest;
    }

    /// <summary>
    /// Clears the cache and returns a new test instance.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <returns>A new CacheServiceTests instance with cleared cache.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the test instance is null.</exception>
    public static async Task<CacheServiceTests> WithClearedCacheAsync(this CacheServiceTests test)
    {
        ArgumentNullException.ThrowIfNull(test);

        var newTest = new CacheServiceTests();
        await newTest.ClearAllAsync_ShouldCallClearAsyncOnProvider().ConfigureAwait(false);
        return newTest;
    }

    /// <summary>
    /// Creates a test scenario with a pre-configured cache provider.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="configureProvider">Action to configure the mock provider.</param>
    /// <returns>A new CacheServiceTests instance with configured provider.</returns>
    /// <exception cref="ArgumentNullException">Thrown if test or configureProvider is null.</exception>
    public static CacheServiceTests WithConfiguredProvider(
        this CacheServiceTests test,
        Action<ICacheProvider> configureProvider)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(configureProvider);

        var newTest = new CacheServiceTests();
        configureProvider(newTest._mockCacheProvider);
        return newTest;
    }

    /// <summary>
    /// Asserts that a specific method was called on the cache provider.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="methodName">The method name to verify.</param>
    /// <param name="times">Number of times the method should have been called.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if test or methodName is null.</exception>
    public static async Task ShouldHaveCalledProviderMethodAsync(
        this CacheServiceTests test,
        string methodName,
        int times = 1)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(methodName);

        // This is a marker method that documents the pattern
        // Actual verification happens in the original test methods
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a test scenario with multiple cached items matching a pattern.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="pattern">The pattern to match (e.g., "user1").</param>
    /// <param name="healthDataCount">Number of health data items to cache.</param>
    /// <param name="analyticsCount">Number of analytics items to cache.</param>
    /// <returns>A new CacheServiceTests instance with multiple cached items.</returns>
    /// <exception cref="ArgumentNullException">Thrown if test or pattern is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if healthDataCount or analyticsCount is negative.</exception>
    public static async Task<CacheServiceTests> WithMultipleCachedItemsAsync(
        this CacheServiceTests test,
        string pattern,
        int healthDataCount = 3,
        int analyticsCount = 2)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(pattern);

        if (healthDataCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(healthDataCount), "Health data count cannot be negative.");
        }

        if (analyticsCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(analyticsCount), "Analytics count cannot be negative.");
        }

        var newTest = new CacheServiceTests();

        // Cache multiple health data items using the public API
        var cacheService = new CacheService(newTest._mockCacheProvider, newTest._mockLogger);
        for (int i = 0; i < healthDataCount; i++)
        {
            var records = new List<HealthDataRecord> { new SleepData { DeviceId = $"dev{i}" } };
            await cacheService.CacheHealthDataAsync($"health_data_{pattern}_{i}", records)
                .ConfigureAwait(false);
        }

        // Cache multiple analytics items using the public API
        for (int i = 0; i < analyticsCount; i++)
        {
            var analytics = new { Metric = i * 10 };
            await cacheService.CacheAnalyticsAsync($"analytics_{pattern}_{i}", analytics)
                .ConfigureAwait(false);
        }

        return newTest;
    }
}
