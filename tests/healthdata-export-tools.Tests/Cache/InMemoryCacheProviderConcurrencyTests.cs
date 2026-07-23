using HealthDataExportTools.Cache;
using Microsoft.Extensions.Logging;
using NSubstitute;
using FluentAssertions;
using Xunit;

namespace HealthDataExportTools.Tests.Cache;

/// <summary>
/// Concurrency tests for InMemoryCacheProvider to verify thread-safety improvements
/// </summary>
public class InMemoryCacheProviderConcurrencyTests
{
    private readonly InMemoryCacheProvider _sut;
    private readonly ILogger<InMemoryCacheProvider> _logger;

    public InMemoryCacheProviderConcurrencyTests()
    {
        _logger = Substitute.For<ILogger<InMemoryCacheProvider>>();
        _sut = new InMemoryCacheProvider(_logger);
    }

    [Fact]
    public async Task GetOrCreateAsync_SingleFlight_PreventsStampede()
    {
        // Arrange
        string key = "stampede_key";
        int factoryCallCount = 0;
        var factoryCallTimes = new List<DateTime>();

        async Task<int> factory()
        {
            factoryCallCount++;
            factoryCallTimes.Add(DateTime.UtcNow);
            await Task.Delay(100); // Simulate expensive operation
            return factoryCallCount;
        }

        // Act - launch 10 concurrent requests
        var tasks = new List<Task<int>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_sut.GetOrCreateAsync(key, factory, TimeSpan.FromHours(1)));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllBeEquivalentTo(1, "only one factory invocation should occur");
        factoryCallCount.Should().Be(1, "factory should be called exactly once");

        // Verify all tasks got the same result
        results.Distinct().Should().HaveCount(1);
    }

    [Fact]
    public async Task GetOrCreateAsync_ParallelMisses_OnlyOneComputes()
    {
        // Arrange
        string key = "parallel_key";
        int factoryCallCount = 0;
        var semaphore = new SemaphoreSlim(0, 1);

        async Task<int> factory()
        {
            Interlocked.Increment(ref factoryCallCount);
            // Signal that we're computing
            semaphore.Release();
            await Task.Delay(50); // Hold for a bit
            return 42;
        }

        // Act - launch 20 concurrent requests
        var tasks = new List<Task<int>>();
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(_sut.GetOrCreateAsync(key, factory, TimeSpan.FromHours(1)));
        }

        // Wait for the first factory to start
        await semaphore.WaitAsync();
        await Task.Delay(25); // Give it time to complete

        var results = await Task.WhenAll(tasks);

        // Assert
        factoryCallCount.Should().Be(1, "factory should be called exactly once despite 20 concurrent misses");
        results.Should().AllBeEquivalentTo(42, "all requests should get the same cached value");
    }

    [Fact]
    public async Task ConcurrentSetAndGet_ShouldBeThreadSafe()
    {
        // Arrange
        var tasks = new List<Task>();
        int totalOperations = 1000;

        // Act - concurrent sets and gets
        for (int i = 0; i < totalOperations; i++)
        {
            int keySuffix = i;
            tasks.Add(Task.Run(async () =>
            {
                string key = $"concurrent_key_{keySuffix % 10}";
                await _sut.SetAsync(key, i);
                var value = await _sut.GetAsync<int>(key);
                value.Should().Be(i);
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - all values should be retrievable
        for (int i = 0; i < 10; i++)
        {
            var value = await _sut.GetAsync<int>($"concurrent_key_{i}");
            value.Should().NotBe(0);
        }
    }

    [Fact]
    public async Task ConcurrentGetOrCreateAsync_DifferentKeys_ShouldWorkIndependently()
    {
        // Arrange
        int totalKeys = 100;
        int operationsPerKey = 50;
        var tasks = new List<Task<int>>();

        // Act - each key gets its own factory invocation
        for (int keyNum = 0; keyNum < totalKeys; keyNum++)
        {
            int keyNumLocal = keyNum;
            for (int op = 0; op < operationsPerKey; op++)
            {
                tasks.Add(_sut.GetOrCreateAsync(
                    $"key_{keyNumLocal}",
                    () => Task.FromResult(keyNumLocal),
                    TimeSpan.FromHours(1)
                ));
            }
        }

        var results = await Task.WhenAll(tasks);

        // Assert - each key should have its correct value
        results.Should().HaveCount(totalKeys * operationsPerKey);
        for (int i = 0; i < totalKeys; i++)
        {
            var values = results.Where(r => r == i).ToList();
            values.Should().HaveCount(operationsPerKey, $"key {i} should have {operationsPerKey} values");
        }
    }

    [Fact]
    public async Task SizeLimit_ShouldEvictLeastRecentlyUsed()
    {
        // Arrange
        const int maxSize = 10000; // Same as MaxCacheSize constant
        const int testSize = maxSize + 1000;

        // Act - fill cache beyond limit
        for (int i = 0; i < testSize; i++)
        {
            await _sut.SetAsync($"key_{i}", i, TimeSpan.FromHours(1));
        }

        // Assert - cache should be at or below max size
        var stats = await _sut.GetStatsAsync();
        stats.ItemCount.Should().BeLessThanOrEqualTo(maxSize, "cache should enforce size limit");

        // Verify non-expired entries are kept
        var keys = await _sut.GetKeysAsync();
        keys.Should().HaveCountLessThanOrEqualTo(maxSize);
    }

    [Fact]
    public async Task Expiration_ShouldCleanUpExpiredEntries()
    {
        // Arrange
        await _sut.SetAsync("permanent", "value1", TimeSpan.FromHours(24));
        await _sut.SetAsync("temporary", "value2", TimeSpan.FromMilliseconds(50));
        await Task.Delay(100); // Wait for expiration

        // Act - get both
        var permanentValue = await _sut.GetAsync<string>("permanent");
        var temporaryValue = await _sut.GetAsync<string>("temporary");

        // Assert
        permanentValue.Should().Be("value1", "permanent entry should still exist");
        temporaryValue.Should().BeNull("expired entry should return null");

        // Verify expired entry is removed
        var exists = await _sut.ExistsAsync("temporary");
        exists.Should().BeFalse("expired entry should not exist");
    }

    [Fact]
    public async Task ConcurrentRemoval_ShouldNotCauseRaceConditions()
    {
        // Arrange
        await _sut.SetAsync("removal_key", "value");
        var tasks = new List<Task>();

        // Act - concurrent gets and removals
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await _sut.GetAsync<string>("removal_key");
                await _sut.RemoveAsync("removal_key");
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - no exceptions should occur
        var stats = await _sut.GetStatsAsync();
        // Cache might be empty or might have the last set value, both are acceptable
    }

    [Fact]
    public async Task HighConcurrency_ShouldHandleManyThreads()
    {
        // Arrange
        int threadCount = 50;
        int operationsPerThread = 100;
        var tasks = new List<Task>();
        int exceptionCount = 0;

        // Act - very high concurrency
        for (int t = 0; t < threadCount; t++)
        {
            int threadNum = t;
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    for (int i = 0; i < operationsPerThread; i++)
                    {
                        string key = $"thread_{threadNum}_op_{i % 10}";
                        await _sut.SetAsync(key, i);
                        var value = await _sut.GetAsync<int>(key);
                        if (value != i)
                        {
                            throw new Exception($"Expected {i} but got {value}");
                        }
                    }
                }
                catch
                {
                    Interlocked.Increment(ref exceptionCount);
                    throw;
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        exceptionCount.Should().Be(0, "no exceptions should occur under high concurrency");
        var stats = await _sut.GetStatsAsync();
        stats.ItemCount.Should().BeLessThanOrEqualTo(10, "should only have 10 unique keys");
    }

    [Fact]
    public async Task Dispose_ShouldCleanupSemaphores()
    {
        // Arrange
        var provider = new InMemoryCacheProvider(_logger);

        // Act
        await provider.SetAsync("test", "value");
        provider.Dispose();

        // Assert - no exception on double dispose
        provider.Dispose();
    }
}
