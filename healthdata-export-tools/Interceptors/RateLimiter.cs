// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Interceptors;

/// <summary>
/// Rate limiter to control the rate of operations
/// Uses token bucket algorithm for fairness and burst handling
/// </summary>
public class RateLimiter
{
    private readonly Dictionary<string, TokenBucket> _buckets;
    private readonly ReaderWriterLockSlim _bucketsLock;
    private readonly ILogger<RateLimiter> _logger;
    private readonly int _defaultCapacity;
    private readonly int _refillRate; // tokens per second

    public RateLimiter(
        ILogger<RateLimiter> logger,
        int defaultCapacity = 100,
        int refillRate = 10)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _buckets = new Dictionary<string, TokenBucket>();
        _bucketsLock = new ReaderWriterLockSlim();
        _defaultCapacity = defaultCapacity;
        _refillRate = refillRate;
    }

    /// <summary>
    /// Try to acquire tokens for an operation
    /// Returns true if operation is allowed, false if rate limit exceeded
    /// </summary>
    public bool TryAcquire(string identifier, int tokensRequired = 1)
    {
        if (string.IsNullOrEmpty(identifier))
            return false;

        _bucketsLock.EnterUpgradeableReadLock();
        try
        {
            if (!_buckets.TryGetValue(identifier, out var bucket))
            {
                _bucketsLock.EnterWriteLock();
                try
                {
                    bucket = new TokenBucket(_defaultCapacity, _refillRate);
                    _buckets[identifier] = bucket;
                }
                finally
                {
                    _bucketsLock.ExitWriteLock();
                }
            }

            bucket.Refill();

            if (bucket.HasTokens(tokensRequired))
            {
                bucket.ConsumeTokens(tokensRequired);
                _logger.LogDebug(
                    "Rate limit acquired for {Identifier}: {Tokens} tokens (remaining: {Remaining})",
                    identifier, tokensRequired, bucket.CurrentTokens);
                return true;
            }

            _logger.LogWarning(
                "Rate limit exceeded for {Identifier}: {Tokens} tokens requested, only {Available} available",
                identifier, tokensRequired, bucket.CurrentTokens);
            return false;
        }
        finally
        {
            _bucketsLock.ExitUpgradeableReadLock();
        }
    }

    /// <summary>
    /// Acquire tokens asynchronously, waiting if necessary
    /// </summary>
    public async Task AcquireAsync(string identifier, int tokensRequired = 1, TimeSpan? maxWait = null)
    {
        maxWait ??= TimeSpan.FromSeconds(30);
        var deadline = DateTime.UtcNow.Add(maxWait.Value);

        while (DateTime.UtcNow < deadline)
        {
            if (TryAcquire(identifier, tokensRequired))
                return;

            await Task.Delay(100); // Wait before retrying
        }

        throw new InvalidOperationException($"Rate limit timeout for {identifier}");
    }

    /// <summary>
    /// Get current rate limit status for an identifier
    /// </summary>
    public RateLimitStatus GetStatus(string identifier)
    {
        _bucketsLock.EnterReadLock();
        try
        {
            if (_buckets.TryGetValue(identifier, out var bucket))
            {
                bucket.Refill();
                return new RateLimitStatus
                {
                    Identifier = identifier,
                    CurrentTokens = bucket.CurrentTokens,
                    MaxTokens = bucket.Capacity,
                    RefillRate = bucket.RefillRate,
                    IsRateLimited = !bucket.HasTokens(1)
                };
            }

            return new RateLimitStatus
            {
                Identifier = identifier,
                CurrentTokens = _defaultCapacity,
                MaxTokens = _defaultCapacity,
                RefillRate = _refillRate,
                IsRateLimited = false
            };
        }
        finally
        {
            _bucketsLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Reset rate limit for an identifier
    /// </summary>
    public void Reset(string identifier)
    {
        _bucketsLock.EnterWriteLock();
        try
        {
            if (_buckets.Remove(identifier))
            {
                _logger.LogInformation("Rate limit reset for: {Identifier}", identifier);
            }
        }
        finally
        {
            _bucketsLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Clear all rate limits
    /// </summary>
    public void ClearAll()
    {
        _bucketsLock.EnterWriteLock();
        try
        {
            _buckets.Clear();
            _logger.LogInformation("All rate limits cleared");
        }
        finally
        {
            _bucketsLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Token bucket for rate limiting
    /// </summary>
    private class TokenBucket
    {
        public int Capacity { get; }
        public int RefillRate { get; }
        public double CurrentTokens { get; private set; }
        private DateTime _lastRefill;

        public TokenBucket(int capacity, int refillRate)
        {
            Capacity = capacity;
            RefillRate = refillRate;
            CurrentTokens = capacity;
            _lastRefill = DateTime.UtcNow;
        }

        public void Refill()
        {
            var now = DateTime.UtcNow;
            var timeSinceLastRefill = (now - _lastRefill).TotalSeconds;
            var tokensToAdd = timeSinceLastRefill * RefillRate;

            CurrentTokens = Math.Min(Capacity, CurrentTokens + tokensToAdd);
            _lastRefill = now;
        }

        public bool HasTokens(int required) => CurrentTokens >= required;

        public void ConsumeTokens(int amount)
        {
            CurrentTokens -= amount;
        }
    }
}

/// <summary>
/// Rate limit status information
/// </summary>
public class RateLimitStatus
{
    public string Identifier { get; set; } = string.Empty;
    public double CurrentTokens { get; set; }
    public int MaxTokens { get; set; }
    public int RefillRate { get; set; }
    public bool IsRateLimited { get; set; }

    public double GetUsagePercentage() => (CurrentTokens / MaxTokens) * 100;
}
