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

/// <summary>
/// Tests for the CacheService class.
/// </summary>
public sealed class CacheServiceTests
{
    /// <summary>
    /// The mock cache provider used in these tests.
    /// </summary>
    internal readonly ICacheProvider _mockCacheProvider;
    
    /// <summary>
    /// The mock logger used in these tests.
    /// </summary>
    internal readonly ILogger<CacheService> _mockLogger;
    
    /// <summary>
    /// The instance of CacheService being tested.
    /// </summary>
    internal readonly CacheService _cacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheServiceTests"/> class.
    /// </summary>
    public CacheServiceTests()
    {
        _mockCacheProvider = Substitute.For<ICacheProvider>();
        _mockLogger = Substitute.For<ILogger<CacheService>>();
        _cacheService = new CacheService(_mockCacheProvider, _mockLogger);
    }

    /// <summary>
    /// Verifies that CacheHealthDataAsync calls SetAsync on the provider.
    /// </summary>
    [Fact]
    public async Task CacheHealthDataAsync_ShouldCallSetAsyncOnProvider()
    {
        // Arrange
        var key = "testKey";
        var records = new List<HealthDataRecord> { new SleepData { DeviceId = "dev1" } };

        // Act
        await _cacheService.CacheHealthDataAsync(key, records).ConfigureAwait(false);

        // Assert
        await _mockCacheProvider.Received(1).SetAsync(
            Arg.Is<string>(s => s.Contains("health_data_")), records, Arg.Any<TimeSpan?>());
        _mockLogger.ReceivedWithAnyArgs(1).LogInformation((string)default!, default!, default!);
    }

    /// <summary>
    /// Verifies that GetCachedHealthDataAsync calls GetAsync on the provider.
    /// </summary>
    [Fact]
    public async Task GetCachedHealthDataAsync_ShouldCallGetAsyncOnProvider()
    {
        // Arrange
        var key = "testKey";
        var expectedRecords = new List<HealthDataRecord> { new SleepData { DeviceId = "dev1" } };
        _mockCacheProvider.GetAsync<List<HealthDataRecord>>(Arg.Is<string>(s => s.Contains("health_data_")))
            .Returns(expectedRecords);

        // Act
        var result = await _cacheService.GetCachedHealthDataAsync(key).ConfigureAwait(false);

        // Assert
        await _mockCacheProvider.Received(1).GetAsync<List<HealthDataRecord>>(Arg.Is<string>(s => s.Contains("health_data_"))).ConfigureAwait(false);
        result.Should().BeEquivalentTo(expectedRecords);
        _mockLogger.ReceivedWithAnyArgs(1).LogInformation((string)default!, default!);
    }

    /// <summary>
    /// Verifies that ClearAllAsync calls ClearAsync on the provider.
    /// </summary>
    [Fact]
    public async Task ClearAllAsync_ShouldCallClearAsyncOnProvider()
    {
        // Act
        await _cacheService.ClearAllAsync().ConfigureAwait(false);

        // Assert
        await _mockCacheProvider.Received(1).ClearAsync().ConfigureAwait(false);
        _mockLogger.ReceivedWithAnyArgs(1).LogInformation((string)default!);
    }

    /// <summary>
    /// Verifies that GetStatsAsync returns the stats from the provider.
    /// </summary>
    [Fact]
    public async Task GetStatsAsync_ShouldReturnStatsFromProvider()
    {
        // Arrange
        var expectedStats = new CacheStats { ItemCount = 10, HitCount = 5, MissCount = 2 };
        _mockCacheProvider.GetStatsAsync().Returns(expectedStats);

        // Act
        var stats = await _cacheService.GetStatsAsync().ConfigureAwait(false);

        // Assert
        stats.Should().BeEquivalentTo(expectedStats);
    }

    /// <summary>
    /// Verifies that IsHealthDataCachedAsync calls ExistsAsync on the provider.
    /// </summary>
    [Fact]
    public async Task IsHealthDataCachedAsync_ShouldCallExistsAsyncOnProvider()
    {
        // Arrange
        var key = "testKey";
        _mockCacheProvider.ExistsAsync(Arg.Is<string>(s => s.Contains("health_data_"))).Returns(true);

        // Act
        var isCached = await _cacheService.IsHealthDataCachedAsync(key).ConfigureAwait(false);

        // Assert
        await _mockCacheProvider.Received(1).ExistsAsync(Arg.Is<string>(s => s.Contains("health_data_"))).ConfigureAwait(false);
        isCached.Should().BeTrue();
    }
    
    /// <summary>
    /// Verifies that CacheAnalyticsAsync calls SetAsync on the provider.
    /// </summary>
    [Fact]
    public async Task CacheAnalyticsAsync_ShouldCallSetAsyncOnProvider()
    {
        // Arrange
        var key = "analyticsKey";
        var analyticsData = new { AverageSleep = 7.5 };

        // Act
        await _cacheService.CacheAnalyticsAsync(key, analyticsData).ConfigureAwait(false);

        // Assert
        await _mockCacheProvider.Received(1).SetAsync(
            Arg.Is<string>(s => s.Contains("analytics_")), analyticsData, Arg.Any<TimeSpan?>());
        _mockLogger.ReceivedWithAnyArgs(1).LogInformation((string)default!, default!);
    }

    /// <summary>
    /// Verifies that GetCachedAnalyticsAsync calls GetAsync on the provider.
    /// </summary>
    [Fact]
    public async Task GetCachedAnalyticsAsync_ShouldCallGetAsyncOnProvider()
    {
        // Arrange
        var key = "analyticsKey";
        var expectedAnalyticsData = new { AverageSleep = 7.5 };
        _mockCacheProvider.GetAsync<dynamic>(Arg.Is<string>(s => s.Contains("analytics_")))
            .Returns(expectedAnalyticsData);

        // Act
        var result = await _cacheService.GetCachedAnalyticsAsync<dynamic>(key).ConfigureAwait(false);

        // Assert
        await _mockCacheProvider.Received(1).GetAsync<dynamic>(Arg.Is<string>(s => s.Contains("analytics_"))).ConfigureAwait(false);
        ((object)result).Should().BeEquivalentTo(expectedAnalyticsData);
        _mockLogger.ReceivedWithAnyArgs(1).LogInformation((string)default!, default!);
    }

    /// <summary>
    /// Verifies that ClearPatternAsync removes matching keys.
    /// </summary>
    [Fact]
    public async Task ClearPatternAsync_ShouldRemoveMatchingKeys()
    {
        // Arrange
        var keys = new List<string> { "health_data_user1", "analytics_user1", "other_data_user2" };
        _mockCacheProvider.GetKeysAsync().Returns(keys);
        
        // Act
        await _cacheService.ClearPatternAsync("user1").ConfigureAwait(false);

        // Assert
        await _mockCacheProvider.Received(1).RemoveAsync("health_data_user1").ConfigureAwait(false);
        await _mockCacheProvider.Received(1).RemoveAsync("analytics_user1").ConfigureAwait(false);
        await _mockCacheProvider.DidNotReceive().RemoveAsync("other_data_user2").ConfigureAwait(false);
        _mockLogger.ReceivedWithAnyArgs(1).LogInformation((string)default!, default!, default!);
    }
}
