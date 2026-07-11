# RateLimiterExtensions

Provides extension methods for `RateLimiter` to simplify rate limit checks and monitoring in .NET applications. These methods offer both synchronous and asynchronous ways to attempt acquiring a rate limit token and to inspect the current usage state of the limiter.

## API

### `TryAcquire`

Attempts to acquire a single token from the associated `RateLimiter` synchronously.

- **Parameters**: None.
- **Return value**: `true` if a token was successfully acquired; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if the `RateLimiter` instance is `null`.

### `TryAcquireAsync`

Asynchronously attempts to acquire a single token from the associated `RateLimiter`.

- **Parameters**: None.
- **Return value**: A `Task<bool>` that resolves to `true` if a token was successfully acquired; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if the `RateLimiter` instance is `null`.

### `GetUsagePercentage`

Calculates the current usage percentage of the associated `RateLimiter` based on its available capacity and the number of tokens already issued.

- **Parameters**: None.
- **Return value**: A `double` representing the percentage of the limiter’s capacity currently in use (0.0 to 100.0).
- **Exceptions**: Throws `ArgumentNullException` if the `RateLimiter` instance is `null`.

### `Reset`

Resets the associated `RateLimiter` to its initial state, clearing any issued tokens and restoring full capacity.

- **Parameters**: None.
- **Return value**: None.
- **Exceptions**: Throws `ArgumentNullException` if the `RateLimiter` instance is `null`.

## Usage
