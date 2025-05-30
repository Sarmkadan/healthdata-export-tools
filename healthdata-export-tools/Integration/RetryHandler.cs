// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Integration;

/// <summary>
/// Retry handler for resilient operations with exponential backoff
/// </summary>
public class RetryHandler
{
    private readonly ILogger<RetryHandler> _logger;
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay;
    private readonly double _backoffMultiplier;

    public RetryHandler(
        ILogger<RetryHandler> logger,
        int maxRetries = 3,
        int initialDelayMs = 100,
        double backoffMultiplier = 2.0)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maxRetries = maxRetries;
        _initialDelay = TimeSpan.FromMilliseconds(initialDelayMs);
        _backoffMultiplier = backoffMultiplier;
    }

    /// <summary>
    /// Execute operation with retries on failure
    /// </summary>
    public async Task<T> ExecuteAsync<T>(
        string operationName,
        Func<Task<T>> operation,
        Func<Exception, bool>? shouldRetry = null)
    {
        shouldRetry ??= DefaultShouldRetry;

        int attempt = 0;
        TimeSpan delay = _initialDelay;

        while (true)
        {
            try
            {
                _logger.LogDebug("Executing operation: {Operation} (attempt {Attempt}/{MaxAttempts})",
                    operationName, attempt + 1, _maxRetries + 1);

                return await operation();
            }
            catch (Exception ex)
            {
                attempt++;

                if (attempt > _maxRetries || !shouldRetry(ex))
                {
                    _logger.LogError(ex,
                        "Operation {Operation} failed after {Attempts} attempts",
                        operationName, attempt);
                    throw;
                }

                _logger.LogWarning(ex,
                    "Operation {Operation} failed (attempt {Attempt}/{MaxAttempts}), retrying in {Delay}ms",
                    operationName, attempt, _maxRetries + 1, delay.TotalMilliseconds);

                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * _backoffMultiplier);
            }
        }
    }

    /// <summary>
    /// Execute operation with retries (non-async version)
    /// </summary>
    public T Execute<T>(
        string operationName,
        Func<T> operation,
        Func<Exception, bool>? shouldRetry = null)
    {
        shouldRetry ??= DefaultShouldRetry;

        int attempt = 0;
        TimeSpan delay = _initialDelay;

        while (true)
        {
            try
            {
                _logger.LogDebug("Executing operation: {Operation} (attempt {Attempt}/{MaxAttempts})",
                    operationName, attempt + 1, _maxRetries + 1);

                return operation();
            }
            catch (Exception ex)
            {
                attempt++;

                if (attempt > _maxRetries || !shouldRetry(ex))
                {
                    _logger.LogError(ex,
                        "Operation {Operation} failed after {Attempts} attempts",
                        operationName, attempt);
                    throw;
                }

                _logger.LogWarning(
                    "Operation {Operation} failed (attempt {Attempt}/{MaxAttempts}), retrying in {Delay}ms",
                    operationName, attempt, _maxRetries + 1, delay.TotalMilliseconds);

                Thread.Sleep(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * _backoffMultiplier);
            }
        }
    }

    /// <summary>
    /// Default retry predicate - retry on transient errors
    /// </summary>
    private bool DefaultShouldRetry(Exception ex)
    {
        return ex is HttpRequestException ||
               ex is TimeoutException ||
               ex is InvalidOperationException ||
               ex is IOException;
    }

    /// <summary>
    /// Create retry handler with custom configuration
    /// </summary>
    public static RetryHandler CreateDefault(ILogger<RetryHandler> logger)
    {
        return new RetryHandler(logger, maxRetries: 3, initialDelayMs: 100);
    }

    /// <summary>
    /// Create aggressive retry handler
    /// </summary>
    public static RetryHandler CreateAggressive(ILogger<RetryHandler> logger)
    {
        return new RetryHandler(logger, maxRetries: 5, initialDelayMs: 50);
    }

    /// <summary>
    /// Create conservative retry handler
    /// </summary>
    public static RetryHandler CreateConservative(ILogger<RetryHandler> logger)
    {
        return new RetryHandler(logger, maxRetries: 1, initialDelayMs: 500);
    }
}

/// <summary>
/// Retry policy configuration
/// </summary>
public class RetryPolicy
{
    public int MaxRetries { get; set; } = 3;
    public int InitialDelayMs { get; set; } = 100;
    public double BackoffMultiplier { get; set; } = 2.0;
    public int MaxDelayMs { get; set; } = 10000;
    public List<Type> RetryableExceptions { get; set; } = new();

    public bool ShouldRetry(Exception ex)
    {
        if (RetryableExceptions.Count == 0)
        {
            return ex is HttpRequestException ||
                   ex is TimeoutException ||
                   ex is IOException;
        }

        return RetryableExceptions.Any(type => type.IsInstanceOfType(ex));
    }
}
