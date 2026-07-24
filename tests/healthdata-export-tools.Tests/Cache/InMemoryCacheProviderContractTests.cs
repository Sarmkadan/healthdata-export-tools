using HealthDataExportTools.Cache;
using Microsoft.Extensions.Logging;
using NSubstitute;
using FluentAssertions;
using Xunit;
using System.Linq;

namespace HealthDataExportTools.Tests.Cache;

/// <summary>
/// Contract tests for InMemoryCacheProvider to verify ICacheProvider interface contract compliance
/// Tests edge cases and contract behaviors beyond basic happy path testing
/// </summary>
public class InMemoryCacheProviderContractTests
{
    private readonly InMemoryCacheProvider _sut;
    private readonly ILogger<InMemoryCacheProvider> _logger;

    public InMemoryCacheProviderContractTests()
    {
        _logger = Substitute.For<ILogger<InMemoryCacheProvider>>();
        _sut = new InMemoryCacheProvider(_logger);
    }

    #region GetAsync Contract Tests

    [Fact]
    public async Task GetAsync_MissingKey_ShouldReturnNullWithoutThrowing()
    {
        // Arrange
        string missingKey = "non_existent_key_12345";

        // Act
        Func<Task> act = async () => await _sut.GetAsync<string>(missingKey);

        // Assert
        await act.Should().NotThrowAsync("GetAsync should not throw for missing keys");
        var result = await _sut.GetAsync<string>(missingKey);
        result.Should().BeNull("GetAsync should return null for missing keys");
    }

    [Fact]
    public async Task GetAsync_NullKey_ShouldThrowArgumentException()
    {
        // Arrange
        string nullKey = null!;

        // Act
        Func<Task> act = async () => await _sut.GetAsync<string>(nullKey);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>("GetAsync should throw for null keys");
    }

    [Fact]
    public async Task GetAsync_EmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        string emptyKey = string.Empty;

        // Act
        Func<Task> act = async () => await _sut.GetAsync<string>(emptyKey);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>("GetAsync should throw for empty keys");
    }

    [Fact]
    public async Task GetAsync_WhitespaceKey_ShouldThrowArgumentException()
    {
        // Arrange
        string whitespaceKey = "   ";

        // Act
        Func<Task> act = async () => await _sut.GetAsync<string>(whitespaceKey);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>("GetAsync should throw for whitespace-only keys");
    }

    [Fact]
    public async Task GetAsync_ValueTypeMissingKey_ShouldReturnDefault()
    {
        // Arrange
        string missingKey = "missing_int_key";

        // Act
        var result = await _sut.GetAsync<int>(missingKey);

        // Assert
        result.Should().Be(0, "GetAsync should return default(int) for missing keys");
    }

    [Fact]
    public async Task GetAsync_ReferenceTypeMissingKey_ShouldReturnNull()
    {
        // Arrange
        string missingKey = "missing_object_key";

        // Act
        var result = await _sut.GetAsync<object>(missingKey);

        // Assert
        result.Should().BeNull("GetAsync should return null for missing reference type keys");
    }

    #endregion

    #region SetAsync Contract Tests

    [Fact]
    public async Task SetAsync_NullKey_ShouldThrowArgumentException()
    {
        // Arrange
        string nullKey = null!;
        string value = "test";

        // Act
        Func<Task> act = async () => await _sut.SetAsync(nullKey, value);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>("SetAsync should throw for null keys");
    }

    [Fact]
    public async Task SetAsync_EmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        string emptyKey = string.Empty;
        string value = "test";

        // Act
        Func<Task> act = async () => await _sut.SetAsync(emptyKey, value);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>("SetAsync should throw for empty keys");
    }

    [Fact]
    public async Task SetAsync_WhitespaceKey_ShouldThrowArgumentException()
    {
        // Arrange
        string whitespaceKey = "   ";
        string value = "test";

        // Act
        Func<Task> act = async () => await _sut.SetAsync(whitespaceKey, value);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>("SetAsync should throw for whitespace-only keys");
    }

    [Fact]
    public async Task SetAsync_NullValue_ShouldStoreNull()
    {
        // Arrange
        string key = "null_value_key";
        string? nullValue = null;

        // Act
        await _sut.SetAsync(key, nullValue);
        var result = await _sut.GetAsync<string?>(key);

        // Assert
        result.Should().BeNull("SetAsync should be able to store null values");
    }

    [Fact]
    public async Task SetAsync_ValueType_ShouldStoreAndRetrieveCorrectly()
    {
        // Arrange
        string key = "int_value_key";
        int value = 42;

        // Act
        await _sut.SetAsync(key, value);
        var result = await _sut.GetAsync<int>(key);

        // Assert
        result.Should().Be(value, "SetAsync should correctly store and retrieve value types");
    }

    [Fact]
    public async Task SetAsync_ReferenceType_ShouldStoreAndRetrieveCorrectly()
    {
        // Arrange
        string key = "object_value_key";
        var value = new { Name = "Test", Value = 123 };

        // Act
        await _sut.SetAsync(key, value);
        var result = await _sut.GetAsync<object>(key);

        // Assert
        result.Should().NotBeNull("SetAsync should store reference types");
    }

    [Fact]
    public async Task SetAsync_WithExpiration_ShouldStoreWithExpiration()
    {
        // Arrange
        string key = "expiring_key";
        string value = "test";
        TimeSpan expiration = TimeSpan.FromMilliseconds(50);

        // Act
        await _sut.SetAsync(key, value, expiration);
        var beforeExpiration = await _sut.GetAsync<string>(key);
        await Task.Delay(100); // Wait for expiration
        var afterExpiration = await _sut.GetAsync<string>(key);

        // Assert
        beforeExpiration.Should().Be(value, "Value should be retrievable before expiration");
        afterExpiration.Should().BeNull("Value should be null after expiration");
    }

    [Fact]
    public async Task SetAsync_ZeroExpiration_ShouldStoreWithNoExpiration()
    {
        // Arrange
        string key = "no_expiration_key";
        string value = "test";
        TimeSpan zeroExpiration = TimeSpan.Zero;

        // Act
        await _sut.SetAsync(key, value, zeroExpiration);
        // Wait a bit to ensure we're past any potential zero-time issues
        await Task.Delay(10);
        var result = await _sut.GetAsync<string>(key);

        // Assert
        result.Should().Be(value, "Zero expiration should not cause immediate expiration");
    }

    [Fact]
    public async Task SetAsync_OverwriteExistingKey_ShouldReplaceOldValue()
    {
        // Arrange
        string key = "overwrite_key";
        string initialValue = "initial";
        string newValue = "replacement";

        // Act
        await _sut.SetAsync(key, initialValue);
        await _sut.SetAsync(key, newValue);
        var result = await _sut.GetAsync<string>(key);

        // Assert
        result.Should().Be(newValue, "SetAsync should overwrite existing values for the same key");
    }

    #endregion

    #region ExistsAsync Contract Tests

    [Fact]
    public async Task ExistsAsync_MissingKey_ShouldReturnFalse()
    {
        // Arrange
        string missingKey = "missing_exists_key";

        // Act
        var result = await _sut.ExistsAsync(missingKey);

        // Assert
        result.Should().BeFalse("ExistsAsync should return false for missing keys");
    }

    [Fact]
    public async Task ExistsAsync_ExistingKey_ShouldReturnTrue()
    {
        // Arrange
        string key = "exists_key";
        await _sut.SetAsync(key, "value");

        // Act
        var result = await _sut.ExistsAsync(key);

        // Assert
        result.Should().BeTrue("ExistsAsync should return true for existing keys");
    }

    [Fact]
    public async Task ExistsAsync_ExpiredKey_ShouldReturnFalse()
    {
        // Arrange
        string key = "expired_exists_key";
        await _sut.SetAsync(key, "value", TimeSpan.FromMilliseconds(50));
        await Task.Delay(100); // Wait for expiration

        // Act
        var result = await _sut.ExistsAsync(key);

        // Assert
        result.Should().BeFalse("ExistsAsync should return false for expired keys");
    }

    [Fact]
    public async Task ExistsAsync_NullKey_ShouldThrowArgumentException()
    {
        // Arrange
        string nullKey = null!;

        // Act
        Func<Task> act = async () => await _sut.ExistsAsync(nullKey);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>("ExistsAsync should throw for null keys");
    }

    [Fact]
    public async Task ExistsAsync_EmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        string emptyKey = string.Empty;

        // Act
        Func<Task> act = async () => await _sut.ExistsAsync(emptyKey);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>("ExistsAsync should throw for empty keys");
    }

    #endregion

    #region RemoveAsync Contract Tests

    [Fact]
    public async Task RemoveAsync_MissingKey_ShouldNotThrow()
    {
        // Arrange
        string missingKey = "missing_remove_key";

        // Act
        Func<Task> act = async () => await _sut.RemoveAsync(missingKey);

        // Assert
        await act.Should().NotThrowAsync("RemoveAsync should not throw for missing keys");
    }

    [Fact]
    public async Task RemoveAsync_NullKey_ShouldNotThrow()
    {
        // Arrange
        string nullKey = null!;

        // Act
        Func<Task> act = async () => await _sut.RemoveAsync(nullKey);

        // Assert
        await act.Should().NotThrowAsync("RemoveAsync should not throw for null keys");
    }

    [Fact]
    public async Task RemoveAsync_EmptyKey_ShouldNotThrow()
    {
        // Arrange
        string emptyKey = string.Empty;

        // Act
        Func<Task> act = async () => await _sut.RemoveAsync(emptyKey);

        // Assert
        await act.Should().NotThrowAsync("RemoveAsync should not throw for empty keys");
    }

    [Fact]
    public async Task RemoveAsync_ExistingKey_ShouldRemoveEntry()
    {
        // Arrange
        string key = "remove_key";
        await _sut.SetAsync(key, "value");
        (await _sut.ExistsAsync(key)).Should().BeTrue("Precondition: key should exist");

        // Act
        await _sut.RemoveAsync(key);

        // Assert
        (await _sut.ExistsAsync(key)).Should().BeFalse("RemoveAsync should remove existing entries");
        var result = await _sut.GetAsync<string>(key);
        result.Should().BeNull("Removed entries should return null on GetAsync");
    }

    [Fact]
    public async Task RemoveAsync_ShouldCleanupKeyLocks()
    {
        // Arrange
        string key = "lock_cleanup_key";
        await _sut.SetAsync(key, "value");

        // Act
        await _sut.RemoveAsync(key);

        // Assert - no exception should occur
        // The key lock should be cleaned up
        var result = await _sut.GetAsync<string>(key);
        result.Should().BeNull("Removed entry should not be retrievable");
    }

    #endregion

    #region ClearAsync Contract Tests

    [Fact]
    public async Task ClearAsync_EmptyCache_ShouldNotThrow()
    {
        // Act
        Func<Task> act = async () => await _sut.ClearAsync();

        // Assert
        await act.Should().NotThrowAsync("ClearAsync should not throw on empty cache");
    }

    [Fact]
    public async Task ClearAsync_PopulatedCache_ShouldRemoveAllEntries()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            await _sut.SetAsync($"key_{i}", i);
        }
        (await _sut.GetStatsAsync()).ItemCount.Should().BeGreaterThan(0, "Precondition: cache should have entries");

        // Act
        await _sut.ClearAsync();

        // Assert
        var stats = await _sut.GetStatsAsync();
        stats.ItemCount.Should().Be(0, "ClearAsync should remove all entries");

        // Verify all keys are gone
        var keys = await _sut.GetKeysAsync();
        keys.Should().BeEmpty("ClearAsync should result in empty key list");
    }

    [Fact]
    public async Task ClearAsync_ShouldResetStatistics()
    {
        // Arrange
        await _sut.SetAsync("hit_test", "value");
        await _sut.GetAsync<string>("hit_test"); // Create a hit
        await _sut.SetAsync("miss_test", "value");
        await _sut.GetAsync<string>("nonexistent"); // Create a miss

        var preClearStats = await _sut.GetStatsAsync();
        preClearStats.ItemCount.Should().BeGreaterThan(0);
        preClearStats.HitCount.Should().BeGreaterThan(0);
        preClearStats.MissCount.Should().BeGreaterThan(0);

        // Act
        await _sut.ClearAsync();

        // Assert
        var postClearStats = await _sut.GetStatsAsync();
        postClearStats.ItemCount.Should().Be(0);
        postClearStats.HitCount.Should().Be(0);
        postClearStats.MissCount.Should().Be(0);
    }

    #endregion

    #region GetKeysAsync Contract Tests

    [Fact]
    public async Task GetKeysAsync_EmptyCache_ShouldReturnEmptyList()
    {
        // Act
        var keys = await _sut.GetKeysAsync();

        // Assert
        keys.Should().BeEmpty("GetKeysAsync should return empty list for empty cache");
    }

    [Fact]
    public async Task GetKeysAsync_PopulatedCache_ShouldReturnAllKeys()
    {
        // Arrange
        var expectedKeys = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            string key = $"key_{i}";
            await _sut.SetAsync(key, i);
            expectedKeys.Add(key);
        }

        // Act
        var actualKeys = await _sut.GetKeysAsync();

        // Assert
        actualKeys.Should().BeEquivalentTo(expectedKeys);
    }

    [Fact]
    public async Task GetKeysAsync_ShouldExcludeExpiredKeys()
    {
        // Arrange
        await _sut.SetAsync("permanent", "value1");
        await _sut.SetAsync("temporary", "value2", TimeSpan.FromMilliseconds(50));
        await Task.Delay(100); // Wait for expiration

        // Act
        var keys = await _sut.GetKeysAsync();

        // Assert
        keys.Should().NotContain("temporary", "GetKeysAsync should exclude expired keys");
        keys.Should().Contain("permanent", "GetKeysAsync should include non-expired keys");
    }

    #endregion

    #region GetStatsAsync Contract Tests

    [Fact]
    public async Task GetStatsAsync_EmptyCache_ShouldReturnZeroValues()
    {
        // Act
        var stats = await _sut.GetStatsAsync();

        // Assert
        stats.ItemCount.Should().Be(0);
        stats.HitCount.Should().Be(0);
        stats.MissCount.Should().Be(0);
        stats.TotalSize.Should().BeGreaterThanOrEqualTo(0);
        stats.HitRate.Should().Be(0);
    }

    [Fact]
    public async Task GetStatsAsync_PopulatedCache_ShouldReturnCorrectStatistics()
    {
        // Arrange
        // Create some hits
        await _sut.SetAsync("hit1", "value1");
        await _sut.GetAsync<string>("hit1");
        await _sut.GetAsync<string>("hit1");

        // Create some misses
        await _sut.GetAsync<string>("miss1");
        await _sut.GetAsync<string>("miss2");

        // Act
        var stats = await _sut.GetStatsAsync();

        // Assert
        stats.ItemCount.Should().Be(1);
        stats.HitCount.Should().Be(2);
        stats.MissCount.Should().Be(2);
        stats.TotalRequests.Should().Be(4);
        stats.HitRate.Should().Be(0.5);
    }

    [Fact]
    public async Task GetStatsAsync_ShouldIncludeNonExpiredOnly()
    {
        // Arrange
        await _sut.SetAsync("permanent", "value1");
        await _sut.SetAsync("temporary", "value2", TimeSpan.FromMilliseconds(50));
        await _sut.GetAsync<string>("permanent"); // Create a hit
        await _sut.GetAsync<string>("temporary"); // Create a hit before expiration
        await Task.Delay(100); // Wait for expiration

        // Act
        var stats = await _sut.GetStatsAsync();

        // Assert
        stats.ItemCount.Should().Be(1, "Stats should only count non-expired entries");
        stats.HitCount.Should().Be(1, "Stats should count hits for non-expired entries only");
    }

    #endregion

    #region GetOrCreateAsync Contract Tests

    [Fact]
    public async Task GetOrCreateAsync_NullKey_ShouldThrowArgumentException()
    {
        // Arrange
        string nullKey = null!;
        int factoryCallCount = 0;

        // Act
        Func<Task> act = async () => await _sut.GetOrCreateAsync(nullKey, () => Task.FromResult(42));

        // Assert
        await act.Should().ThrowAsync<ArgumentException>("GetOrCreateAsync should throw for null keys")
            .WithMessage("*ArgumentNullException*OR*ArgumentException*");
        factoryCallCount.Should().Be(0, "Factory should not be called for invalid keys");
    }

    [Fact]
    public async Task GetOrCreateAsync_EmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        string emptyKey = string.Empty;
        int factoryCallCount = 0;

        // Act
        Func<Task> act = async () => await _sut.GetOrCreateAsync(emptyKey, () => Task.FromResult(42));

        // Assert
        await act.Should().ThrowAsync<ArgumentException>("GetOrCreateAsync should throw for empty keys")
            .WithMessage("*ArgumentNullException*OR*ArgumentException*");
        factoryCallCount.Should().Be(0);
    }

    [Fact]
    public async Task GetOrCreateAsync_NullFactory_ShouldThrowArgumentNullException()
    {
        // Arrange
        string key = "null_factory_key";
        Func<Task<int>> nullFactory = null!;

        // Act
        Func<Task> act = async () => await _sut.GetOrCreateAsync(key, nullFactory);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>("GetOrCreateAsync should throw for null factory");
    }

    [Fact]
    public async Task GetOrCreateAsync_FactoryReturnsValue_ShouldCacheAndReturnValue()
    {
        // Arrange
        string key = "factory_key";
        int factoryCallCount = 0;

        async Task<int> factory()
        {
            factoryCallCount++;
            await Task.Delay(10); // Simulate work
            return 42;
        }

        // Act - first call should invoke factory
        var result1 = await _sut.GetOrCreateAsync(key, factory);

        // Second call should use cache
        var result2 = await _sut.GetOrCreateAsync(key, factory);

        // Act from another concurrent call
        var result3 = await _sut.GetOrCreateAsync(key, factory);

        // Assert
        result1.Should().Be(42);
        result2.Should().Be(42);
        result3.Should().Be(42);
        factoryCallCount.Should().Be(1, "Factory should be called only once due to single-flight pattern");
    }

    [Fact]
    public async Task GetOrCreateAsync_WithExpiration_ShouldCacheWithExpiration()
    {
        // Arrange
        string key = "factory_expiration_key";
        int factoryCallCount = 0;

        Func<Task<int>> factory = async () =>
        {
            factoryCallCount++;
            return 99;
        };

        // Act - first call
        var result1 = await _sut.GetOrCreateAsync(key, factory, TimeSpan.FromMilliseconds(50));
        result1.Should().Be(99);

        // Wait for expiration
        await Task.Delay(100);

        // Second call should invoke factory again (expired)
        var result2 = await _sut.GetOrCreateAsync(key, factory, TimeSpan.FromMilliseconds(50));
        result2.Should().Be(99);

        // Verify factory was called twice (once initially, once after expiration)
        factoryCallCount.Should().Be(2);
    }

    #endregion

    #region IDisposable Contract Tests

    [Fact]
    public void Dispose_MultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var provider = new InMemoryCacheProvider(_logger);

        // Act
        provider.Dispose();

        // Assert - should not throw on double dispose
        provider.Dispose();
    }

    [Fact]
    public async Task Dispose_ShouldCleanupAllResources()
    {
        // Arrange
        var provider = new InMemoryCacheProvider(_logger);
        await provider.SetAsync("test", "value");

        // Act
        provider.Dispose();

        // Assert - cache should be empty after dispose
        var stats = await provider.GetStatsAsync();
        stats.ItemCount.Should().Be(0);
    }

    #endregion

    #region Size Limit Contract Tests

    [Fact]
    public async Task SizeLimit_ShouldEnforceMaxCacheSize()
    {
        // Arrange
        const int maxSize = 10000; // From MaxCacheSize constant
        const int testSize = maxSize + 5000;

        // Act - fill cache beyond limit
        for (int i = 0; i < testSize; i++)
        {
            await _sut.SetAsync($"key_{i}", i, TimeSpan.FromHours(1));
        }

        // Assert - cache should be at or below max size
        var stats = await _sut.GetStatsAsync();
        stats.ItemCount.Should().BeLessThanOrEqualTo(maxSize, "Cache should enforce size limit");
    }

    [Fact]
    public async Task SizeLimit_ShouldKeepRecentlyUsedEntries()
    {
        // Arrange
        const int maxSize = 10000;
        const int testSize = maxSize + 1000;

        // Create some entries and access them to make them "recently used"
        for (int i = 0; i < 100; i++)
        {
            await _sut.SetAsync($"recent_{i}", i, TimeSpan.FromHours(1));
            await _sut.GetAsync<int>($"recent_{i}"); // Access to update LastAccessAt
        }

        // Fill cache beyond limit
        for (int i = 0; i < testSize; i++)
        {
            await _sut.SetAsync($"old_{i}", i, TimeSpan.FromHours(1));
        }

        // Act - access recent entries
        for (int i = 0; i < 100; i++)
        {
            await _sut.GetAsync<int>($"recent_{i}");
        }

        // Assert - recent entries should still exist
        var stats = await _sut.GetStatsAsync();
        var keys = await _sut.GetKeysAsync();

        foreach (int i in Enumerable.Range(0, 100))
        {
            keys.Should().Contain($"recent_{i}", "Recently accessed entries should be kept during eviction");
        }
    }

    #endregion
}