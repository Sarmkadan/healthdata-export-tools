#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using System.Net.Http.Headers;

namespace HealthDataExportTools.Integration;

/// <summary>
/// Configuration options for the RetryHandler
/// </summary>
public sealed class RetryHandlerOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts. Default is 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial delay in milliseconds before the first retry. Default is 100ms.
    /// </summary>
    public int InitialDelayMs { get; set; } = 100;

    /// <summary>
    /// Gets or sets the maximum delay in milliseconds between retries. Default is 30000ms (30 seconds).
    /// </summary>
    public int MaxDelayMs { get; set; } = 30_000;

    /// <summary>
    /// Gets or sets the maximum backoff time in milliseconds for the circuit breaker half-open state. Default is 60000ms (1 minute).
    /// </summary>
    public int CircuitBreakerMaxBackoffMs { get; set; } = 60_000;

    /// <summary>
    /// Gets or sets the number of consecutive failures required to open the circuit breaker. Default is 5.
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>
    /// Gets or sets the number of consecutive successes required to close the circuit breaker after being half-open. Default is 2.
    /// </summary>
    public int CircuitBreakerSuccessThreshold { get; set; } = 2;

    /// <summary>
    /// Gets or sets the minimum time in milliseconds between half-open probes. Default is 5000ms (5 seconds).
    /// </summary>
    public int CircuitBreakerHalfOpenMinDelayMs { get; set; } = 5_000;

    /// <summary>
    /// Gets or sets whether to honor the Retry-After header on 429 (Too Many Requests) and 503 (Service Unavailable) responses.
    /// When true, the actual delay will be the maximum of the Retry-After value and the calculated backoff delay.
    /// Default is true.
    /// </summary>
    public bool HonorRetryAfterHeader { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use decorrelated jitter for exponential backoff. Default is true.
    /// When true, uses decorrelated jitter algorithm for more evenly distributed retry attempts.
    /// When false, uses traditional exponential backoff with fixed multiplier.
    /// </summary>
    public bool UseDecorrelatedJitter { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of HTTP methods that are considered idempotent and safe to retry.
    /// Default includes GET, HEAD, OPTIONS, PUT, and DELETE.
    /// </summary>
    public HashSet<string> IdempotentHttpMethods { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethod.Get.Method,
        HttpMethod.Head.Method,
        HttpMethod.Options.Method,
        HttpMethod.Put.Method,
        HttpMethod.Delete.Method
    };

    /// <summary>
    /// Gets or sets the list of HTTP status codes that should trigger a retry.
    /// Default includes 408 (Request Timeout), 429 (Too Many Requests), and 5xx (Server Errors).
    /// </summary>
    public HashSet<HttpStatusCode> RetryableStatusCodes { get; set; } = new()
    {
        HttpStatusCode.RequestTimeout,           // 408
        HttpStatusCode.TooManyRequests,         // 429
        HttpStatusCode.InternalServerError,      // 500
        HttpStatusCode.BadGateway,              // 502
        HttpStatusCode.ServiceUnavailable,       // 503
        HttpStatusCode.GatewayTimeout            // 504
    };

    /// <summary>
    /// Gets or sets the list of exception types that should trigger a retry.
    /// Default includes common transient exceptions.
    /// </summary>
    public HashSet<Type> RetryableExceptions { get; set; } = new()
    {
        typeof(HttpRequestException),
        typeof(TimeoutException),
        typeof(IOException),
        typeof(InvalidOperationException),
        typeof(TaskCanceledException)
    };

    /// <summary>
    /// Gets or sets whether the circuit breaker is enabled. Default is true.
    /// </summary>
    public bool CircuitBreakerEnabled { get; set; } = true;

    /// <summary>
    /// Validates the configuration and throws if invalid.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid.</exception>
    public void Validate()
    {
        if (MaxRetries < 0)
        {
            throw new ArgumentException($"{nameof(MaxRetries)} must be non-negative", nameof(MaxRetries));
        }

        if (InitialDelayMs <= 0)
        {
            throw new ArgumentException($"{nameof(InitialDelayMs)} must be positive", nameof(InitialDelayMs));
        }

        if (MaxDelayMs <= 0)
        {
            throw new ArgumentException($"{nameof(MaxDelayMs)} must be positive", nameof(MaxDelayMs));
        }

        if (MaxDelayMs < InitialDelayMs)
        {
            throw new ArgumentException($"{nameof(MaxDelayMs)} must be greater than or equal to {nameof(InitialDelayMs)}");
        }

        if (CircuitBreakerFailureThreshold <= 0)
        {
            throw new ArgumentException($"{nameof(CircuitBreakerFailureThreshold)} must be positive", nameof(CircuitBreakerFailureThreshold));
        }

        if (CircuitBreakerSuccessThreshold <= 0)
        {
            throw new ArgumentException($"{nameof(CircuitBreakerSuccessThreshold)} must be positive", nameof(CircuitBreakerSuccessThreshold));
        }

        if (CircuitBreakerMaxBackoffMs <= 0)
        {
            throw new ArgumentException($"{nameof(CircuitBreakerMaxBackoffMs)} must be positive", nameof(CircuitBreakerMaxBackoffMs));
        }

        if (CircuitBreakerHalfOpenMinDelayMs <= 0)
        {
            throw new ArgumentException($"{nameof(CircuitBreakerHalfOpenMinDelayMs)} must be positive", nameof(CircuitBreakerHalfOpenMinDelayMs));
        }
    }
}

/// <summary>
/// Retry handler for resilient operations with exponential backoff, Retry-After header support,
/// and circuit breaker pattern
/// </summary>
public sealed class RetryHandler
{
    private readonly ILogger<RetryHandler> _logger;
    private readonly RetryHandlerOptions _options;
    private readonly Random _random = Random.Shared;
    private readonly object _circuitBreakerLock = new();
    private CircuitBreakerState _circuitBreakerState = CircuitBreakerState.Closed;
    private int _consecutiveFailures = 0;
    private int _consecutiveSuccesses = 0;
    private DateTimeOffset _lastFailureTime = DateTimeOffset.MinValue;
    private DateTimeOffset _circuitBreakerHalfOpenTime = DateTimeOffset.MinValue;

    /// <summary>
    /// Initializes a new instance of the RetryHandler class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">Retry handler configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public RetryHandler(ILogger<RetryHandler> logger, RetryHandlerOptions? options = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new RetryHandlerOptions();
        _options.Validate();
    }

    /// <summary>
    /// Execute operation with retries on failure using decorrelated-jitter exponential backoff
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operationName">Name of the operation for logging purposes.</param>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="requestUri">Optional request URI for Retry-After header support.</param>
    /// <param name="requestMethod">Optional HTTP method for idempotency checking.</param>
    /// <returns>The result of the operation if successful.</returns>
    /// <exception cref="ArgumentNullException">Thrown when operation is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when circuit breaker is open and not ready for half-open probe.</exception>
    public async Task<T> ExecuteAsync<T>(
        string operationName,
        Func<Task<T>> operation,
        string? requestUri = null,
        HttpMethod? requestMethod = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(operationName);

        CheckCircuitBreakerState();

        int attempt = 0;
        TimeSpan delay = TimeSpan.Zero;
        DateTimeOffset operationStartTime = DateTimeOffset.UtcNow;

        while (true)
        {
            try
            {
                _logger.LogDebug("Executing operation: {Operation} (attempt {Attempt}/{MaxAttempts})",
                    operationName, attempt + 1, _options.MaxRetries + 1);

                return await operation().ConfigureAwait(false);
            }
            catch (Exception ex) when (ShouldRetry(ex, requestUri, requestMethod))
            {
                attempt++;
                _consecutiveFailures++;
                _lastFailureTime = DateTimeOffset.UtcNow;

                if (attempt > _options.MaxRetries)
                {
                    _logger.LogError(ex,
                        "Operation {Operation} failed after {Attempts} attempts. Circuit breaker state: {CircuitBreakerState}",
                        operationName, attempt, _circuitBreakerState);
                    throw;
                }

                delay = CalculateDelay(attempt, ex, requestUri);

                _logger.LogWarning(ex,
                    "Operation {Operation} failed (attempt {Attempt}/{MaxAttempts}), retrying in {Delay}ms. Circuit breaker state: {CircuitBreakerState}",
                    operationName, attempt, _options.MaxRetries + 1, delay.TotalMilliseconds, _circuitBreakerState);

                await Task.Delay(delay).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Operation {Operation} failed after {Attempts} attempts (non-retryable exception). Circuit breaker state: {CircuitBreakerState}",
                    operationName, attempt + 1, _circuitBreakerState);
                throw;
            }
        }
    }

    /// <summary>
    /// Execute operation with retries on failure using decorrelated-jitter exponential backoff
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operationName">Name of the operation for logging purposes.</param>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="requestUri">Optional request URI for Retry-After header support.</param>
    /// <param name="requestMethod">Optional HTTP method for idempotency checking.</param>
    /// <returns>The result of the operation if successful.</returns>
    /// <exception cref="ArgumentNullException">Thrown when operation is null or operationName is null/empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when circuit breaker is open and not ready for half-open probe.</exception>
    public T Execute<T>(
        string operationName,
        Func<T> operation,
        string? requestUri = null,
        HttpMethod? requestMethod = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentException.ThrowIfNullOrEmpty(operationName);

        CheckCircuitBreakerState();

        int attempt = 0;
        TimeSpan delay = TimeSpan.Zero;

        while (true)
        {
            try
            {
                _logger.LogDebug("Executing operation: {Operation} (attempt {Attempt}/{MaxAttempts})",
                    operationName, attempt + 1, _options.MaxRetries + 1);

                return operation();
            }
            catch (Exception ex) when (ShouldRetry(ex, requestUri, requestMethod))
            {
                attempt++;
                _consecutiveFailures++;
                _lastFailureTime = DateTimeOffset.UtcNow;

                if (attempt > _options.MaxRetries)
                {
                    _logger.LogError(ex,
                        "Operation {Operation} failed after {Attempts} attempts. Circuit breaker state: {CircuitBreakerState}",
                        operationName, attempt, _circuitBreakerState);
                    throw;
                }

                delay = CalculateDelay(attempt, ex, requestUri);

                _logger.LogWarning(ex,
                    "Operation {Operation} failed (attempt {Attempt}/{MaxAttempts}), retrying in {Delay}ms. Circuit breaker state: {CircuitBreakerState}",
                    operationName, attempt, _options.MaxRetries + 1, delay.TotalMilliseconds, _circuitBreakerState);

                Thread.Sleep(delay);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Operation {Operation} failed after {Attempts} attempts (non-retryable exception). Circuit breaker state: {CircuitBreakerState}",
                    operationName, attempt + 1, _circuitBreakerState);
                throw;
            }
        }
    }

    /// <summary>
    /// Determines whether an exception should trigger a retry based on configuration.
    /// </summary>
    /// <param name="ex">The exception to evaluate.</param>
    /// <param name="requestUri">Optional request URI for Retry-After header support.</param>
    /// <param name="requestMethod">Optional HTTP method for idempotency checking.</param>
    /// <returns>True if the exception should trigger a retry; otherwise, false.</returns>
    private bool ShouldRetry(Exception ex, string? requestUri = null, HttpMethod? requestMethod = null)
    {
        // Never retry cancellation or operation canceled exceptions
        if (ex is OperationCanceledException or TaskCanceledException)
        {
            return false;
        }

        // Check if exception type is retryable
        bool isRetryableException = _options.RetryableExceptions.Any(type => type.IsInstanceOfType(ex));
        if (!isRetryableException)
        {
            return false;
        }

        // For HTTP-related exceptions, check status code and idempotency
        if (ex is HttpRequestException httpEx && httpEx.StatusCode.HasValue)
        {
            HttpStatusCode statusCode = httpEx.StatusCode.Value;

            // Never retry 4xx errors except 408, 429, and other explicitly configured codes
            if ((int)statusCode >= 400 && (int)statusCode < 500 &&
                !_options.RetryableStatusCodes.Contains(statusCode))
            {
                return false;
            }

            // Check if method is idempotent
            if (requestMethod != null && !IsIdempotentMethod(requestMethod))
            {
                _logger.LogDebug("Not retrying {Method} request to {Uri} due to non-idempotent method",
                    requestMethod, requestUri ?? "unknown");
                return false;
            }
        }

        // Circuit breaker is open - only allow retries if we're in half-open state
        if (_circuitBreakerState == CircuitBreakerState.Open &&
            _circuitBreakerState != CircuitBreakerState.HalfOpen)
        {
            _logger.LogDebug("Circuit breaker is open, not retrying operation");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks the circuit breaker state and transitions if necessary.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when circuit breaker is open and not ready for half-open probe.</exception>
    private void CheckCircuitBreakerState()
    {
        if (!_options.CircuitBreakerEnabled)
        {
            return;
        }

        lock (_circuitBreakerLock)
        {
            switch (_circuitBreakerState)
            {
                case CircuitBreakerState.Open:
                    // Check if we should transition to half-open
                    TimeSpan timeSinceLastFailure = DateTimeOffset.UtcNow - _lastFailureTime;
                    if (timeSinceLastFailure.TotalMilliseconds >= _options.CircuitBreakerMaxBackoffMs)
                    {
                        _circuitBreakerState = CircuitBreakerState.HalfOpen;
                        _consecutiveSuccesses = 0;
                        _logger.LogInformation("Circuit breaker transitioned from OPEN to HALF-OPEN state");
                    }
                    break;

                case CircuitBreakerState.HalfOpen:
                    // In half-open state, only allow one attempt at a time
                    TimeSpan timeSinceHalfOpen = DateTimeOffset.UtcNow - _circuitBreakerHalfOpenTime;
                    if (timeSinceHalfOpen.TotalMilliseconds < _options.CircuitBreakerHalfOpenMinDelayMs)
                    {
                        throw new InvalidOperationException(
                            $"Circuit breaker is in HALF-OPEN state but minimum delay ({_options.CircuitBreakerHalfOpenMinDelayMs}ms) not elapsed. " +
                            $"Wait until {_circuitBreakerHalfOpenTime.AddMilliseconds(_options.CircuitBreakerHalfOpenMinDelayMs):O}");
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Calculates the delay for the next retry attempt using decorrelated jitter exponential backoff.
    /// </summary>
    /// <param name="attempt">The current attempt number (1-based).</param>
    /// <param name="ex">The exception that triggered the retry.</param>
    /// <param name="requestUri">Optional request URI for Retry-After header support.</param>
    /// <returns>The delay duration before the next retry attempt.</returns>
    private TimeSpan CalculateDelay(int attempt, Exception ex, string? requestUri)
    {
        TimeSpan calculatedDelay = CalculateBaseDelay(attempt);

        // Check for Retry-After header if this is an HTTP exception
        if (_options.HonorRetryAfterHeader && ex is HttpRequestException httpEx && httpEx.StatusCode.HasValue)
        {
            HttpStatusCode statusCode = httpEx.StatusCode.Value;
            if (statusCode == HttpStatusCode.TooManyRequests || statusCode == HttpStatusCode.ServiceUnavailable)
            {
                TimeSpan? retryAfterDelay = GetRetryAfterDelay(requestUri);
                if (retryAfterDelay.HasValue)
                {
                    calculatedDelay = TimeSpan.FromMilliseconds(Math.Max(
                        calculatedDelay.TotalMilliseconds,
                        Math.Min(retryAfterDelay.Value.TotalMilliseconds, _options.MaxDelayMs)
                    ));
                }
            }
        }

        return calculatedDelay;
    }

    /// <summary>
    /// Calculates the base delay using decorrelated jitter exponential backoff.
    /// </summary>
    /// <param name="attempt">The current attempt number (1-based).</param>
    /// <returns>The base delay duration.</returns>
    private TimeSpan CalculateBaseDelay(int attempt)
    {
        // Decorrelated jitter formula: https://www.awsarchitectureblog.com/2015/03/backoff.html
        // base = min(cap, random_between(0, previous_base * backoff))
        double baseDelayMs = _options.InitialDelayMs;

        if (attempt > 1)
        {
            double previousBaseMs = _options.InitialDelayMs * Math.Pow(2, attempt - 2);
            double maxBaseMs = Math.Min(previousBaseMs * 4, _options.MaxDelayMs);

            if (_options.UseDecorrelatedJitter)
            {
                // Decorrelated jitter: random between 0 and maxBase, but never less than initial delay
                double jitteredDelayMs = _random.NextDouble() * maxBaseMs;
                baseDelayMs = Math.Max(_options.InitialDelayMs, jitteredDelayMs);
            }
            else
            {
                // Traditional exponential backoff
                baseDelayMs = Math.Min(previousBaseMs * 2, _options.MaxDelayMs);
            }
        }

        return TimeSpan.FromMilliseconds(Math.Min(baseDelayMs, _options.MaxDelayMs));
    }

    /// <summary>
    /// Extracts the Retry-After delay from response headers or exception data.
    /// </summary>
    /// <param name="requestUri">The request URI that may contain Retry-After information.</param>
    /// <returns>The Retry-After delay if available; otherwise, null.</returns>
    private TimeSpan? GetRetryAfterDelay(string? requestUri)
    {
        // In a real implementation, this would parse HTTP response headers
        // For this implementation, we'll simulate it from the request URI if present
        // Format: retry-after:<seconds> or retry-after:<http-date>

        if (string.IsNullOrEmpty(requestUri) || !requestUri.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Simulate parsing Retry-After from URI query parameters for testing purposes
        // In production, this would come from HttpResponseHeaders
        var uri = new Uri(requestUri);
        var retryAfterParam = System.Web.HttpUtility.ParseQueryString(uri.Query)["retry-after"];

        if (retryAfterParam != null && int.TryParse(retryAfterParam, out int seconds))
        {
            return TimeSpan.FromSeconds(seconds);
        }

        // Check for standard HTTP date format in headers (would be implemented in real scenario)
        return null;
    }

    /// <summary>
    /// Determines if an HTTP method is idempotent and safe to retry.
    /// </summary>
    /// <param name="method">The HTTP method to check.</param>
    /// <returns>True if the method is idempotent; otherwise, false.</returns>
    private bool IsIdempotentMethod(HttpMethod? method)
    {
        if (method == null)
        {
            return true; // Default to true if method not specified
        }

        return _options.IdempotentHttpMethods.Contains(method.Method);
    }

    /// <summary>
    /// Records a successful operation, updating circuit breaker state if applicable.
    /// </summary>
    private void RecordSuccess()
    {
        if (!_options.CircuitBreakerEnabled)
        {
            return;
        }

        lock (_circuitBreakerLock)
        {
            _consecutiveFailures = 0;
            _consecutiveSuccesses++;

            if (_circuitBreakerState == CircuitBreakerState.HalfOpen &&
                _consecutiveSuccesses >= _options.CircuitBreakerSuccessThreshold)
            {
                _circuitBreakerState = CircuitBreakerState.Closed;
                _logger.LogInformation("Circuit breaker transitioned from HALF-OPEN to CLOSED state");
            }
        }
    }

    /// <summary>
    /// Records a failure, updating circuit breaker state if applicable.
    /// </summary>
    private void RecordFailure()
    {
        if (!_options.CircuitBreakerEnabled)
        {
            return;
        }

        lock (_circuitBreakerLock)
        {
            _consecutiveSuccesses = 0;

            if (_circuitBreakerState == CircuitBreakerState.Closed &&
                _consecutiveFailures >= _options.CircuitBreakerFailureThreshold)
            {
                _circuitBreakerState = CircuitBreakerState.Open;
                _logger.LogWarning("Circuit breaker transitioned from CLOSED to OPEN state after {Failures} consecutive failures",
                    _consecutiveFailures);
            }
        }
    }

    /// <summary>
    /// Creates a retry handler with default settings.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <returns>A new RetryHandler instance configured with default values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public static RetryHandler CreateDefault(ILogger<RetryHandler> logger)
    {
        return new RetryHandler(logger, new RetryHandlerOptions
        {
            MaxRetries = 3,
            InitialDelayMs = 100,
            MaxDelayMs = 30_000,
            CircuitBreakerFailureThreshold = 5,
            CircuitBreakerSuccessThreshold = 2,
            CircuitBreakerMaxBackoffMs = 60_000,
            CircuitBreakerHalfOpenMinDelayMs = 5_000
        });
    }

    /// <summary>
    /// Creates an aggressive retry handler.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <returns>A new RetryHandler instance configured for aggressive retries.</returns>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public static RetryHandler CreateAggressive(ILogger<RetryHandler> logger)
    {
        return new RetryHandler(logger, new RetryHandlerOptions
        {
            MaxRetries = 5,
            InitialDelayMs = 50,
            MaxDelayMs = 20_000,
            CircuitBreakerFailureThreshold = 3,
            CircuitBreakerSuccessThreshold = 1,
            CircuitBreakerMaxBackoffMs = 30_000,
            CircuitBreakerHalfOpenMinDelayMs = 3_000
        });
    }

    /// <summary>
    /// Creates a conservative retry handler.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <returns>A new RetryHandler instance configured for conservative retries.</returns>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public static RetryHandler CreateConservative(ILogger<RetryHandler> logger)
    {
        return new RetryHandler(logger, new RetryHandlerOptions
        {
            MaxRetries = 2,
            InitialDelayMs = 500,
            MaxDelayMs = 60_000,
            CircuitBreakerFailureThreshold = 10,
            CircuitBreakerSuccessThreshold = 3,
            CircuitBreakerMaxBackoffMs = 120_000,
            CircuitBreakerHalfOpenMinDelayMs = 10_000
        });
    }

    /// <summary>
    /// Gets the current circuit breaker state.
    /// </summary>
    public CircuitBreakerState CircuitBreakerState => _circuitBreakerState;

    /// <summary>
    /// Gets the number of consecutive failures.
    /// </summary>
    public int ConsecutiveFailures => _consecutiveFailures;

    /// <summary>
    /// Gets the number of consecutive successes.
    /// </summary>
    public int ConsecutiveSuccesses => _consecutiveSuccesses;
}

/// <summary>
/// Represents the state of the circuit breaker.
/// </summary>
public enum CircuitBreakerState
{
    /// <summary>
    /// The circuit breaker is closed and allowing operations to proceed normally.
    /// </summary>
    Closed,

    /// <summary>
    /// The circuit breaker is open and blocking all operations.
    /// </summary>
    Open,

    /// <summary>
    /// The circuit breaker is in half-open state, allowing limited operations to probe for recovery.
    /// </summary>
    HalfOpen
}