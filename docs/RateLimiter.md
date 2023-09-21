# RateLimiter

A token-bucket based rate limiter that enforces a maximum request capacity and a steady refill rate. It is designed to control the rate of operations in distributed or high-throughput scenarios where consistent throttling is required, such as API calls or data export tasks.

## API

### `RateLimiter(int capacity, int refillRate, string identifier = null)`
Initializes a new instance of the `RateLimiter` with the specified capacity and refill rate.

- **Parameters**
  - `capacity` – The maximum number of tokens the bucket can hold.
  - `refillRate` – The number of tokens to add per second.
  - `identifier` – An optional string identifier for logging or debugging purposes.

- **Throws**
  - `ArgumentOutOfRangeException` – If `capacity` or `refillRate` is less than or equal to zero.

---

### `bool TryAcquire()`
Attempts to acquire a single token from the bucket without blocking. Returns `true` if a token was successfully acquired; otherwise, returns `false`.

- **Returns**
  - `bool` – `true` if a token was acquired; otherwise, `false`.

---

### `async Task AcquireAsync()`
Asynchronously waits until a token is available and then acquires it. This method blocks until a token can be consumed.

- **Returns**
  - `Task` – A task that completes when a token has been acquired.

---

### `RateLimitStatus GetStatus()`
Returns a snapshot of the current rate-limiting status, including token count and usage metrics.

- **Returns**
  - `RateLimitStatus` – A structure containing:
    - `CurrentTokens` – The current number of tokens in the bucket.
    - `MaxTokens` – The maximum bucket capacity.
    - `RefillRate` – The refill rate in tokens per second.
    - `IsRateLimited` – Whether the bucket is currently rate-limiting (i.e., `CurrentTokens == 0`).
    - `GetUsagePercentage()` – The percentage of capacity currently in use.

---

### `void Reset()`
Resets the bucket to its initial state with the full capacity and no pending refills.

---

### `void ClearAll()`
Clears all tokens from the bucket, setting the current token count to zero.

---

### `int Capacity`
Gets the maximum number of tokens the bucket can hold.

- **Returns**
  - `int` – The bucket capacity.

---

### `int RefillRate`
Gets the number of tokens added to the bucket per second.

- **Returns**
  - `int` – The refill rate in tokens per second.

---
### `double CurrentTokens`
Gets the current number of tokens in the bucket.

- **Returns**
  - `double` – The current token count.

---
### `TokenBucket TokenBucket`
Gets the underlying token bucket instance.

- **Returns**
  - `TokenBucket` – The internal token bucket used for rate limiting.

---
### `void Refill()`
Manually refills the bucket by adding tokens based on the elapsed time since the last refill.

---
### `bool HasTokens()`
Determines whether the bucket has at least one token available.

- **Returns**
  - `bool` – `true` if at least one token is available; otherwise, `false`.

---
### `void ConsumeTokens(int tokens)`
Consumes the specified number of tokens from the bucket. If insufficient tokens are available, the bucket is emptied.

- **Parameters**
  - `tokens` – The number of tokens to consume.

- **Throws**
  - `ArgumentOutOfRangeException` – If `tokens` is less than zero.

---
### `string Identifier`
Gets the optional identifier for this rate limiter.

- **Returns**
  - `string` – The identifier, or `null` if not set.

---
### `double GetUsagePercentage()`
Calculates the percentage of the bucket capacity currently in use.

- **Returns**
  - `double` – A value between `0.0` and `100.0` representing the usage percentage.

---
### `public bool IsRateLimited`
Gets a value indicating whether the bucket is currently rate-limiting (i.e., no tokens are available).

- **Returns**
  - `bool` – `true` if the bucket is rate-limiting; otherwise, `false`.

## Usage

### Example 1: Synchronous Rate-Limited Operation
