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

public class CacheServiceTests
{
    private readonly ICacheProvider _mockCacheProvider;
    private readonly ILogger<CacheService> _mockLogger;
    private readonly CacheService _cacheService;

    public CacheServiceTests()
    {
        _mockCacheProvider = Substitute.For<ICacheProvider>();
        _mockLogger = Substitute.For<ILogger<CacheService>>();
        _cacheService = new CacheService(_mockCacheProvider, _mockLogger);
    }

    [Fact]
    public async Task CacheHealthDataAsync_ShouldCallSetAsyncOnProvider()
    {
        // Arrange
        var key = "testKey";
        var records = new List<HealthDataRecord> { new SleepData { DeviceId = "dev1" } };

        // Act
        await _cacheService.CacheHealthDataAsync(key, records);

        // Assert
        await _mockCacheProvider.Received(1).SetAsync(
            Arg.Is<string>(s => s.Contains("health_data_")), records, Arg.Any<TimeSpan?>());
        _mockLogger.Received(1).LogInformation(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>());
    }

    [Fact]
    public async Task GetCachedHealthDataAsync_ShouldCallGetAsyncOnProvider()
    {
        // Arrange
        var key = "testKey";
        var expectedRecords = new List<HealthDataRecord> { new SleepData { DeviceId = "dev1" } };
        _mockCacheProvider.GetAsync<List<HealthDataRecord>>(Arg.Is<string>(s => s.Contains("health_data_")))
            .Returns(expectedRecords);

        // Act
        var result = await _cacheService.GetCachedHealthDataAsync(key);

        // Assert
        await _mockCacheProvider.Received(1).GetAsync<List<HealthDataRecord>>(Arg.Is<string>(s => s.Contains("health_data_")));
        result.Should().BeEquivalentTo(expectedRecords);
        _mockLogger.Received(1).LogInformation(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ClearAllAsync_ShouldCallClearAsyncOnProvider()
    {
        // Act
        await _cacheService.ClearAllAsync();

        // Assert
        await _mockCacheProvider.Received(1).ClearAsync();
        _mockLogger.Received(1).LogInformation(Arg.Any<string>());
    }

    [Fact]
    public async Task GetStatsAsync_ShouldReturnStatsFromProvider()
    {
        // Arrange
        var expectedStats = new CacheStats { ItemCount = 10, HitCount = 5, MissCount = 2 };
        _mockCacheProvider.GetStatsAsync().Returns(expectedStats);

        // Act
        var stats = await _cacheService.GetStatsAsync();

        // Assert
        stats.Should().BeEquivalentTo(expectedStats);
    }

    [Fact]
    public async Task IsHealthDataCachedAsync_ShouldCallExistsAsyncOnProvider()
    {
        // Arrange
        var key = "testKey";
        _mockCacheProvider.ExistsAsync(Arg.Is<string>(s => s.Contains("health_data_"))).Returns(true);

        // Act
        var isCached = await _cacheService.IsHealthDataCachedAsync(key);

        // Assert
        await _mockCacheProvider.Received(1).ExistsAsync(Arg.Is<string>(s => s.Contains("health_data_")));
        isCached.Should().BeTrue();
    }
    
    [Fact]
    public async Task CacheAnalyticsAsync_ShouldCallSetAsyncOnProvider()
    {
        // Arrange
        var key = "analyticsKey";
        var analyticsData = new { AverageSleep = 7.5 };

        // Act
        await _cacheService.CacheAnalyticsAsync(key, analyticsData);

        // Assert
        await _mockCacheProvider.Received(1).SetAsync(
            Arg.Is<string>(s => s.Contains("analytics_")), analyticsData, Arg.Any<TimeSpan?>());
        _mockLogger.Received(1).LogInformation(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task GetCachedAnalyticsAsync_ShouldCallGetAsyncOnProvider()
    {
        // Arrange
        var key = "analyticsKey";
        var expectedAnalyticsData = new { AverageSleep = 7.5 };
        _mockCacheProvider.GetAsync<dynamic>(Arg.Is<string>(s => s.Contains("analytics_")))
            .Returns(expectedAnalyticsData);

        // Act
        var result = await _cacheService.GetCachedAnalyticsAsync<dynamic>(key);

        // Assert
        await _mockCacheProvider.Received(1).GetAsync<dynamic>(Arg.Is<string>(s => s.Contains("analytics_")));
        result.Should().BeEquivalentTo(expectedAnalyticsData);
        _mockLogger.Received(1).LogInformation(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ClearPatternAsync_ShouldRemoveMatchingKeys()
    {
        // Arrange
        var keys = new List<string> { "health_data_user1", "analytics_user1", "other_data_user2" };
        _mockCacheProvider.GetKeysAsync().Returns(keys);
        
        // Act
        await _cacheService.ClearPatternAsync("user1");

        // Assert
        await _mockCacheProvider.Received(1).RemoveAsync("health_data_user1");
        await _mockCacheProvider.Received(1).RemoveAsync("analytics_user1");
        await _mockCacheProvider.DidNotReceive().RemoveAsync("other_data_user2");
        _mockLogger.Received(1).LogInformation(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>());
    }
}
