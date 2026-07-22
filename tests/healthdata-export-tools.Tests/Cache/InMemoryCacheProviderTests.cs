using HealthDataExportTools.Cache;
using Microsoft.Extensions.Logging;
using NSubstitute;
using FluentAssertions;
using Xunit;

namespace HealthDataExportTools.Tests.Cache;

public class InMemoryCacheProviderTests
{
    private readonly InMemoryCacheProvider _sut;
    private readonly ILogger<InMemoryCacheProvider> _logger;

    public InMemoryCacheProviderTests()
    {
        _logger = Substitute.For<ILogger<InMemoryCacheProvider>>();
        _sut = new InMemoryCacheProvider(_logger);
    }

    [Fact]
    public async Task SetAndGet_ShouldReturnStoredValue()
    {
        // Arrange
        string key = "testKey";
        string value = "testValue";

        // Act
        await _sut.SetAsync(key, value);
        var result = await _sut.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public async Task Expiry_ShouldReturnNullAfterExpiration()
    {
        // Arrange
        string key = "expiredKey";
        string value = "value";
        TimeSpan expiry = TimeSpan.FromMilliseconds(100);

        // Act
        await _sut.SetAsync(key, value, expiry);
        await Task.Delay(200);
        var result = await _sut.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
        (await _sut.ExistsAsync(key)).Should().BeFalse();
    }

    [Fact]
    public async Task Overwrite_ShouldReplaceExistingValue()
    {
        // Arrange
        string key = "overwriteKey";
        string value1 = "value1";
        string value2 = "value2";

        // Act
        await _sut.SetAsync(key, value1);
        await _sut.SetAsync(key, value2);
        var result = await _sut.GetAsync<string>(key);

        // Assert
        result.Should().Be(value2);
    }

    [Fact]
    public async Task RemoveMissingKey_ShouldNotThrow()
    {
        // Arrange
        string key = "nonExistentKey";

        // Act
        Func<Task> act = async () => await _sut.RemoveAsync(key);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void ConcurrentAccess_SmokeTest()
    {
        // Arrange
        int iterations = 100;
        string keyPrefix = "key_";

        // Act
        Parallel.For(0, iterations, i =>
        {
            string key = keyPrefix + i;
            _sut.SetAsync(key, i).Wait();
            var val = _sut.GetAsync<int>(key).Result;
            val.Should().Be(i);
        });

        // Assert
        // If no exception, test passes
    }
}
