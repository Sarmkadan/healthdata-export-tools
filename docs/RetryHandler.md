# RetryHandler

A utility class for executing operations with configurable retry policies, handling transient failures by automatically retrying failed operations with exponential backoff, Retry-After header support, idempotency checking, and circuit breaker pattern.

## Features

- **Decorrelated-jitter exponential backoff**: More evenly distributed retry attempts compared to traditional exponential backoff
- **Retry-After header support**: Honors Retry-After headers on 429 (Too Many Requests) and 503 (Service Unavailable) responses
- **Idempotent method filtering**: Only retries idempotent HTTP methods (GET, HEAD, OPTIONS, PUT, DELETE) by default
- **Circuit breaker pattern**: Fails fast after consecutive failures, probes for recovery in half-open state
- **Configurable options**: All thresholds and behaviors are configurable via `RetryHandlerOptions`
- **Transient error detection**: Retries on 408, 429, 5xx status codes and common transient exceptions

## API

### `RetryHandlerOptions`

Configuration class for RetryHandler with the following properties:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `MaxRetries` | int | 3 | Maximum number of retry attempts |
| `InitialDelayMs` | int | 100 | Initial delay in milliseconds before first retry |
| `MaxDelayMs` | int | 30000 | Maximum delay in milliseconds between retries |
| `CircuitBreakerMaxBackoffMs` | int | 60000 | Maximum backoff time for circuit breaker half-open state |
| `CircuitBreakerFailureThreshold` | int | 5 | Consecutive failures to open circuit breaker |
| `CircuitBreakerSuccessThreshold` | int | 2 | Consecutive successes to close circuit breaker |
| `CircuitBreakerHalfOpenMinDelayMs` | int | 5000 | Minimum time between half-open probes |
| `HonorRetryAfterHeader` | bool | true | Whether to honor Retry-After headers |
| `UseDecorrelatedJitter` | bool | true | Whether to use decorrelated jitter for backoff |
| `IdempotentHttpMethods` | HashSet<string> | GET, HEAD, OPTIONS, PUT, DELETE | Idempotent HTTP methods to retry |
| `RetryableStatusCodes` | HashSet<HttpStatusCode> | 408, 429, 500, 502, 503, 504 | HTTP status codes that trigger retries |
| `RetryableExceptions` | HashSet<Type> | HttpRequestException, TimeoutException, IOException, InvalidOperationException, TaskCanceledException | Exception types that trigger retries |
| `CircuitBreakerEnabled` | bool | true | Whether circuit breaker is enabled |

### `RetryHandler`

The main retry handler class that implements resilient operation execution.

#### Constructors

```csharp
public RetryHandler(ILogger<RetryHandler> logger, RetryHandlerOptions? options = null)
```

- **logger**: The logger instance
- **options**: Optional configuration options (defaults to sensible defaults if null)

#### Methods

##### `ExecuteAsync<T>`

```csharp
public async Task<T> ExecuteAsync<T>(
    string operationName,
    Func<Task<T>> operation,
    string? requestUri = null,
    HttpMethod? requestMethod = null)
```

Executes the provided asynchronous operation with retry logic.

- **operationName**: Name of the operation for logging purposes
- **operation**: The asynchronous operation to execute
- **requestUri**: Optional request URI for Retry-After header support
- **requestMethod**: Optional HTTP method for idempotency checking
- **Return value**: The result of the operation if successful
- **Throws**: `ArgumentNullException` if `operation` or `operationName` is null
- **Throws**: `InvalidOperationException` if circuit breaker is open and not ready for half-open probe
- **Throws**: Any exception thrown by `operation` if all retries are exhausted

##### `Execute<T>`

```csharp
public T Execute<T>(
    string operationName,
    Func<T> operation,
    string? requestUri = null,
    HttpMethod? requestMethod = null)
```

Executes the provided synchronous operation with retry logic.

- **operationName**: Name of the operation for logging purposes
- **operation**: The synchronous operation to execute
- **requestUri**: Optional request URI for Retry-After header support
- **requestMethod**: Optional HTTP method for idempotency checking
- **Return value**: The result of the operation if successful
- **Throws**: `ArgumentNullException` if `operation` or `operationName` is null/empty
- **Throws**: `InvalidOperationException` if circuit breaker is open and not ready for half-open probe
- **Throws**: Any exception thrown by `operation` if all retries are exhausted

##### Factory Methods

```csharp
public static RetryHandler CreateDefault(ILogger<RetryHandler> logger)
```

Creates a retry handler with default settings (3 retries, 100ms initial delay, 30s max delay, circuit breaker with 5 failure threshold).

- **logger**: The logger instance
- **Return value**: A new `RetryHandler` instance configured with default values

```csharp
public static RetryHandler CreateAggressive(ILogger<RetryHandler> logger)
```

Creates a retry handler with aggressive settings (5 retries, 50ms initial delay, 20s max delay, circuit breaker with 3 failure threshold).

- **logger**: The logger instance
- **Return value**: A new `RetryHandler` instance configured for aggressive retries

```csharp
public static RetryHandler CreateConservative(ILogger<RetryHandler> logger)
```

Creates a retry handler with conservative settings (2 retries, 500ms initial delay, 60s max delay, circuit breaker with 10 failure threshold).

- **logger**: The logger instance
- **Return value**: A new `RetryHandler` instance configured for conservative retries

##### Properties

```csharp
public CircuitBreakerState CircuitBreakerState { get; }
```

Gets the current circuit breaker state (Closed, Open, or HalfOpen).

```csharp
public int ConsecutiveFailures { get; }
```

Gets the number of consecutive failures.

```csharp
public int ConsecutiveSuccesses { get; }
```

Gets the number of consecutive successes.

## Usage

### Example 1: Basic Retry with Default Settings

```csharp
var logger = new Logger<RetryHandler>(...);
var retryHandler = RetryHandler.CreateDefault(logger);

var result = await retryHandler.ExecuteAsync("GetHealthData", async () =>
{
    var response = await httpClient.GetAsync("https://api.example.com/health");
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
});
```

### Example 2: Custom Configuration with Circuit Breaker

```csharp
var options = new RetryHandlerOptions
{
    MaxRetries = 5,
    InitialDelayMs = 200,
    MaxDelayMs = 60000,
    CircuitBreakerFailureThreshold = 3,
    CircuitBreakerSuccessThreshold = 2,
    HonorRetryAfterHeader = true,
    UseDecorrelatedJitter = true
};

var retryHandler = new RetryHandler(logger, options);

try
{
    var data = await retryHandler.ExecuteAsync("ApiCall", async () =>
    {
        var response = await httpClient.GetAsync("https://api.example.com/data");
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            // Retry-After header will be honored automatically
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }, "https://api.example.com/data", HttpMethod.Get);
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
{
    // Handle rate limited response
}
```

### Example 3: Idempotent Method Filtering

```csharp
var retryHandler = RetryHandler.CreateDefault(logger);

// This will retry because GET is idempotent
var result = await retryHandler.ExecuteAsync("GetData", async () =>
{
    var response = await httpClient.GetAsync("https://api.example.com/data");
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
}, "https://api.example.com/data", HttpMethod.Get);

// This will NOT retry because POST is not idempotent by default
// (unless explicitly added to IdempotentHttpMethods)
var postResult = await retryHandler.ExecuteAsync("PostData", async () =>
{
    var response = await httpClient.PostAsync("https://api.example.com/data", content);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
}, "https://api.example.com/data", HttpMethod.Post);
```

### Example 4: Monitoring Circuit Breaker State

```csharp
var retryHandler = RetryHandler.CreateDefault(logger);

if (retryHandler.CircuitBreakerState == CircuitBreakerState.Open)
{
    Console.WriteLine("Circuit breaker is open! Upstream service may be unavailable.");
    Console.WriteLine($"Consecutive failures: {retryHandler.ConsecutiveFailures}");
}

// After successful operations, circuit breaker may close
if (retryHandler.CircuitBreakerState == CircuitBreakerState.Closed)
{
    Console.WriteLine("Circuit breaker is closed. Operations are proceeding normally.");
}
```

### Example 5: Custom Status Codes and Exceptions

```csharp
var options = new RetryHandlerOptions
{
    RetryableStatusCodes = new HashSet<HttpStatusCode>
    {
        HttpStatusCode.RequestTimeout,      // 408
        HttpStatusCode.TooManyRequests,    // 429
        HttpStatusCode.InternalServerError,  // 500
        HttpStatusCode.BadGateway           // 502
    },
    RetryableExceptions = new HashSet<Type>
    {
        typeof(HttpRequestException),
        typeof(TimeoutException),
        typeof(IOException)
    }
};

var retryHandler = new RetryHandler(logger, options);
```

## Circuit Breaker States

### Closed State
- Normal operation
- All requests are allowed
- Failures are counted
- When failure threshold is reached, transitions to Open state

### Open State
- All requests fail fast (no retries)
- After backoff period, transitions to HalfOpen state
- Allows limited probing for recovery

### HalfOpen State
- Limited number of requests are allowed
- If successful, transitions to Closed state
- If failed, transitions back to Open state

## Backoff Strategies

### Decorrelated Jitter (Default)
The decorrelated jitter algorithm provides more evenly distributed retry attempts:

```
delay = random_between(0, min(cap, previous_base * 4))
```

This prevents the "thundering herd" problem where many clients retry at the same time after a failure.

### Traditional Exponential Backoff (Optional)
Can be enabled by setting `UseDecorrelatedJitter = false`:

```
delay = min(cap, previous_base * 2)
```

## Retry-After Header Support

When `HonorRetryAfterHeader` is true (default), the handler will:

1. Check for 429 (Too Many Requests) or 503 (Service Unavailable) responses
2. Parse the Retry-After header (seconds or HTTP-date format)
3. Use the maximum of the Retry-After delay and the calculated backoff delay
4. Cap at `MaxDelayMs` to prevent excessive waits

## Idempotency

By default, only the following HTTP methods are considered idempotent:
- GET
- HEAD
- OPTIONS
- PUT
- DELETE

Non-idempotent methods (POST, PATCH) will not be retried unless explicitly added to the `IdempotentHttpMethods` set.

## Logging

The RetryHandler logs at the following levels:

- **Debug**: Operation execution attempts
- **Warning**: Retry attempts with delay information
- **Error**: Final failures after all retries exhausted
- **Information**: Circuit breaker state transitions

## Thread Safety

The RetryHandler is thread-safe for concurrent use. The circuit breaker state and failure counters are protected by locks.

## Best Practices

1. **Use appropriate retry settings**: Aggressive for internal services, conservative for external APIs
2. **Monitor circuit breaker state**: Log state transitions to detect upstream issues
3. **Honor Retry-After headers**: Prevents overwhelming rate-limited services
4. **Use idempotent methods**: Ensures safe retry behavior
5. **Set reasonable max delays**: Prevents long waits for unresponsive services
6. **Configure circuit breaker thresholds**: Balance between fast failure and resilience