#nullable enable
// =============================================================================
// Author: Automated Test Generation
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using HealthDataExportTools.Integration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HealthDataExportTools.Tests;

/// <summary>
/// Tests for <see cref="RetryHandler"/> covering success, retries, exhaustion,
/// cancellation handling and back‑off delay behavior.
/// </summary>
public sealed class RetryHandlerTests : IDisposable
{
    private readonly Mock<ILogger<RetryHandler>> _loggerMock;
    private readonly List<string> _logMessages = new();

    public RetryHandlerTests()
    {
        _loggerMock = new Mock<ILogger<RetryHandler>>();

        // Capture log messages for optional assertions
        _loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _loggerMock.Setup(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()))
            .Callback((LogLevel level, EventId id, object state, Exception? ex, Delegate formatter) =>
            {
                var message = formatter.DynamicInvoke(state, ex) as string;
                if (message != null)
                {
                    _logMessages.Add(message);
                }
            });
    }

    public void Dispose()
    {
        _loggerMock.Reset();
    }

    [Fact]
    public async Task ExecuteAsync_SucceedsFirstTry_ReturnsResult()
    {
        // Arrange
        var handler = new RetryHandler(_loggerMock.Object, maxRetries: 3, initialDelayMs: 10);
        var expected = 42;

        // Act
        var result = await handler.ExecuteAsync<int>(
            "FirstTry",
            () => Task.FromResult(expected));

        // Assert
        Assert.Equal(expected, result);
        // Only one debug log should be emitted (no retries)
        Assert.Contains(_logMessages, m => m.Contains("Executing operation: FirstTry"));
    }

    [Fact]
    public async Task ExecuteAsync_RetriesTransientFailures_ReturnsResult()
    {
        // Arrange
        var handler = new RetryHandler(_loggerMock.Object, maxRetries: 3, initialDelayMs: 10);
        var attempts = 0;

        // Throw HttpRequestException twice, then succeed
        Task<int> Operation()
        {
            attempts++;
            if (attempts <= 2)
                throw new HttpRequestException("Transient failure");
            return Task.FromResult(99);
        }

        // Act
        var result = await handler.ExecuteAsync<int>("TransientRetry", Operation);

        // Assert
        Assert.Equal(99, result);
        Assert.Equal(3, attempts); // two failures + one success
        // Verify that warning logs were emitted for the two retries
        var warningCount = 0;
        foreach (var msg in _logMessages)
        {
            if (msg.Contains("failed (attempt"))
                warningCount++;
        }
        Assert.Equal(2, warningCount);
    }

    [Fact]
    public async Task ExecuteAsync_ExceedsMaxRetries_Throws()
    {
        // Arrange
        var handler = new RetryHandler(_loggerMock.Object, maxRetries: 2, initialDelayMs: 10);
        int attempts = 0;

        Task<int> Operation()
        {
            attempts++;
            throw new HttpRequestException("Always failing");
        }

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await handler.ExecuteAsync<int>("ExhaustRetries", Operation));

        Assert.Equal("Always failing", ex.Message);
        // Attempts should be maxRetries + 1 (initial try + retries)
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task ExecuteAsync_CancellationException_NotRetried()
    {
        // Arrange
        var handler = new RetryHandler(_loggerMock.Object, maxRetries: 5, initialDelayMs: 10);
        int attempts = 0;

        Task<int> Operation()
        {
            attempts++;
            throw new OperationCanceledException("Cancelled");
        }

        // Act & Assert
        var ex = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await handler.ExecuteAsync<int>("Cancellation", Operation));

        Assert.Equal("Cancelled", ex.Message);
        // Should not retry; only one attempt
        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task ExecuteAsync_BackoffDelaysIncrease()
    {
        // Arrange
        var initialDelayMs = 20;
        var backoff = 2.0;
        var handler = new RetryHandler(_loggerMock.Object, maxRetries: 3, initialDelayMs: initialDelayMs, backoffMultiplier: backoff);
        var timestamps = new List<DateTime>();

        int attempts = 0;
        Task<int> Operation()
        {
            attempts++;
            timestamps.Add(DateTime.UtcNow);
            if (attempts <= 2)
                throw new HttpRequestException("Transient");
            return Task.FromResult(1);
        }

        // Act
        var sw = Stopwatch.StartNew();
        var result = await handler.ExecuteAsync<int>("BackoffTest", Operation);
        sw.Stop();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(3, attempts); // two failures + success

        // Verify that delays roughly follow exponential back‑off
        // Expected total delay ≈ initialDelay + initialDelay*backoff
        var expectedDelay = initialDelayMs + (initialDelayMs * backoff);
        // Allow some tolerance for scheduling overhead
        Assert.InRange(sw.ElapsedMilliseconds, expectedDelay, expectedDelay + 200);
    }

    [Fact]
    public void Execute_SucceedsFirstTry_ReturnsResult()
    {
        // Arrange
        var handler = new RetryHandler(_loggerMock.Object, maxRetries: 3, initialDelayMs: 10);
        var expected = "ok";

        // Act
        var result = handler.Execute<string>("SyncFirstTry", () => expected);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Execute_TransientFailures_RetriesAndSucceeds()
    {
        // Arrange
        var handler = new RetryHandler(_loggerMock.Object, maxRetries: 3, initialDelayMs: 10);
        var attempts = 0;

        string Operation()
        {
            attempts++;
            if (attempts <= 2)
                throw new HttpRequestException("Transient");
            return "done";
        }

        // Act
        var result = handler.Execute<string>("SyncTransient", Operation);

        // Assert
        Assert.Equal("done", result);
        Assert.Equal(3, attempts);
    }
}
