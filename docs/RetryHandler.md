# RetryHandler

A utility class for executing operations with configurable retry policies, handling transient failures by automatically retrying failed operations with exponential backoff and configurable delays.

## API

### `RetryHandler`
The default constructor initializes a retry handler with conservative defaults (3 retries, 100ms initial delay, 2.0 backoff multiplier, 5000ms max delay, and common retryable exceptions).

### `public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)`
Executes the provided asynchronous operation with retry logic.

- **operation**: The asynchronous operation to execute.
- **Return value**: The result of the operation if successful.
- **Throws**: `ArgumentNullException` if `operation` is `null`.
- **Throws**: Any exception thrown by `operation` if all retries are exhausted.

### `public T Execute<T>(Func<T> operation)`
Executes the provided synchronous operation with retry logic.

- **operation**: The synchronous operation to execute.
- **Return value**: The result of the operation if successful.
- **Throws**: `ArgumentNullException` if `operation` is `null`.
- **Throws**: Any exception thrown by `operation` if all retries are exhausted.

### `public static RetryHandler CreateDefault()`
Creates a retry handler with default settings (3 retries, 100ms initial delay, 2.0 backoff multiplier, 5000ms max delay, and common retryable exceptions).

- **Return value**: A new `RetryHandler` instance configured with default values.

### `public static RetryHandler CreateAggressive()`
Creates a retry handler with aggressive settings (5 retries, 50ms initial delay, 1.5 backoff multiplier, 2000ms max delay, and common retryable exceptions).

- **Return value**: A new `RetryHandler` instance configured for aggressive retries.

### `public static RetryHandler CreateConservative()`
Creates a retry handler with conservative settings (2 retries, 200ms initial delay, 3.0 backoff multiplier, 10000ms max delay, and common retryable exceptions).

- **Return value**: A new `RetryHandler` instance configured for conservative retries.

### `public int MaxRetries`
Gets or sets the maximum number of retry attempts. Default is 3.

### `public int InitialDelayMs`
Gets or sets the initial delay in milliseconds before the first retry. Default is 100.

### `public double BackoffMultiplier`
Gets or sets the multiplier applied to the delay between retries. Default is 2.0.

### `public int MaxDelayMs`
Gets or sets the maximum delay in milliseconds between retries. Default is 5000.

### `public List<Type> RetryableExceptions`
Gets the list of exception types that should trigger a retry. By default, includes common transient exceptions like `HttpRequestException`, `SocketException`, and `TimeoutException`.

### `public bool ShouldRetry(Exception exception)`
Determines whether the given exception should trigger a retry based on the configured retryable exceptions list.

- **exception**: The exception to evaluate.
- **Return value**: `true` if the exception should trigger a retry; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `exception` is `null`.

## Usage

### Example 1: Basic Retry with Default Settings
