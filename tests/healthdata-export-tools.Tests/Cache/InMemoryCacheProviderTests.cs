/// <summary>
/// Contains tests for the InMemoryCacheProvider class.
/// </summary>
using HealthDataExportTools.Cache;
using Microsoft.Extensions.Logging;
using NSubstitute;
using FluentAssertions;
using Xunit;

namespace HealthDataExportTools.Tests.Cache;

/// <summary>
/// Tests for the InMemoryCacheProvider class.
/// </summary>
public class InMemoryCacheProviderTests
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryCacheProviderTests"/> class.
    /// </summary>
    private readonly InMemoryCacheProvider _sut;
    private readonly ILogger<InMemoryCacheProvider> _logger;

    public InMemoryCacheProviderTests()
    {
        _logger = Substitute.For<ILogger<InMemoryCacheProvider>>();
        _sut = new InMemoryCacheProvider(_logger);
    }

    /// <summary>
    /// Tests that setting and getting a value from the cache returns the stored value.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task SetAndGet_ShouldReturnStoredValue()
    {
        // Arrange
        string key = "testKey";
        string value = "testValue";
        _logger.LogInformation("Starting test: SetAndGet_ShouldReturnStoredValue with Key={Key}, Value={Value}", key, value);

        // Act
        await _sut.SetAsync(key, value);
        var result = await _sut.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
        _logger.LogInformation("Finished test: SetAndGet_ShouldReturnStoredValue. Result={Result}", result);
    }

    /// <summary>
    /// Tests that a value expires from the cache after the specified time.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task Expiry_ShouldReturnNullAfterExpiration()
    {
        // Arrange
        string key = "expiredKey";
        string value = "value";
        TimeSpan expiry = TimeSpan.FromMilliseconds(100);
        _logger.LogInformation("Starting test: Expiry_ShouldReturnNullAfterExpiration with Key={Key}, ExpiryMs={ExpiryMs}", key, expiry.TotalMilliseconds);

        // Act
        await _sut.SetAsync(key, value, expiry);
        await Task.Delay(200);
        var result = await _sut.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
        (await _sut.ExistsAsync(key)).Should().BeFalse();
        _logger.LogInformation("Finished test: Expiry_ShouldReturnNullAfterExpiration. IsNull={IsNull}", result == null);
    }

    /// <summary>
    /// Tests that overwriting a value in the cache replaces the existing value.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task Overwrite_ShouldReplaceExistingValue()
    {
        // Arrange
        string key = "overwriteKey";
        string value1 = "value1";
        string value2 = "value2";
        _logger.LogInformation("Starting test: Overwrite_ShouldReplaceExistingValue with Key={Key}, Value1={Value1}, Value2={Value2}", key, value1, value2);

        // Act
        await _sut.SetAsync(key, value1);
        await _sut.SetAsync(key, value2);
        var result = await _sut.GetAsync<string>(key);

        // Assert
        result.Should().Be(value2);
        _logger.LogInformation("Finished test: Overwrite_ShouldReplaceExistingValue. FinalValue={Result}", result);
    }

    /// <summary>
    /// Tests that removing a missing key from the cache does not throw an exception.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task RemoveMissingKey_ShouldNotThrow()
    {
        // Arrange
        string key = "nonExistentKey";
        _logger.LogInformation("Starting test: RemoveMissingKey_ShouldNotThrow with Key={Key}", key);

        // Act
        Func<Task> act = async () => await _sut.RemoveAsync(key);

        // Assert
        await act.Should().NotThrowAsync();
        _logger.LogInformation("Finished test: RemoveMissingKey_ShouldNotThrow. No exception thrown.");
    }

    /// <summary>
    /// Performs a smoke test for concurrent access to the cache.
    /// </summary>
    [Fact]
    public void ConcurrentAccess_SmokeTest()
    {
        // Arrange
        int iterations = 100;
        string keyPrefix = "key_";
        _logger.LogInformation("Starting test: ConcurrentAccess_SmokeTest with Iterations={Iterations}", iterations);

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
        _logger.LogInformation("Finished test: ConcurrentAccess_SmokeTest. Completed successfully.");
    }
}
