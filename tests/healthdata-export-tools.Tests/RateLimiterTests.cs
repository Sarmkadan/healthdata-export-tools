using FluentAssertions;
using HealthDataExportTools.Interceptors;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HealthDataExportTools.Tests;

public class RateLimiterTests
{
    [Fact]
    public async Task TryAcquire_RefillsTokensOverTime()
    {
        var logger = Substitute.For<ILogger<RateLimiter>>();
        // Capacity: 10, Refill rate: 50 tokens/sec
        var rateLimiter = new RateLimiter(logger, defaultCapacity: 10, refillRate: 50);
        
        // Consume all tokens
        rateLimiter.TryAcquire("test", 10).Should().BeTrue();
        rateLimiter.TryAcquire("test", 1).Should().BeFalse();

        // Wait for 100ms. Refill should be 50 * 0.1 = 5 tokens.
        await Task.Delay(150); // Increased slightly for stability

        // Should have tokens now
        rateLimiter.TryAcquire("test", 4).Should().BeTrue();
        rateLimiter.TryAcquire("test", 1).Should().BeFalse();
    }
}
