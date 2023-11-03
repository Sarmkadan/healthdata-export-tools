using System;
using System.Threading.Tasks;

namespace HealthData.Export.Interceptors
{
    /// <summary>
    /// Provides extension methods for <see cref="RateLimiter"/> to enhance rate limiting functionality.
    /// </summary>
    public static class RateLimiterExtensions
    {
        /// <summary>
        /// Attempts to acquire tokens from the rate limiter with a specified timeout.
        /// </summary>
        /// <param name="rateLimiter">The rate limiter instance.</param>
        /// <param name="tokens">Number of tokens to acquire. Must be positive.</param>
        /// <param name="timeout">Timeout duration.</param>
        /// <returns>True if tokens were acquired within the timeout, false otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rateLimiter"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="tokens"/> is not positive.</exception>
        public static bool TryAcquire(this RateLimiter rateLimiter, int tokens, TimeSpan timeout)
        {
            ArgumentNullException.ThrowIfNull(rateLimiter);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tokens);

            var startTime = DateTime.UtcNow;
            var endTime = startTime + timeout;

            while (DateTime.UtcNow < endTime)
            {
                if (rateLimiter.HasTokens(tokens))
                {
                    rateLimiter.ConsumeTokens(tokens);
                    return true;
                }

                // Wait for a short interval before retrying
                var remainingTime = endTime - DateTime.UtcNow;
                if (remainingTime.TotalMilliseconds > 10)
                {
                    System.Threading.Thread.Sleep(Math.Min(10, (int)remainingTime.TotalMilliseconds));
                }
                else
                {
                    break;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to acquire tokens from the rate limiter with a specified timeout asynchronously.
        /// </summary>
        /// <param name="rateLimiter">The rate limiter instance.</param>
        /// <param name="tokens">Number of tokens to acquire. Must be positive.</param>
        /// <param name="timeout">Timeout duration.</param>
        /// <returns>True if tokens were acquired within the timeout, false otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rateLimiter"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="tokens"/> is not positive.</exception>
        public static async Task<bool> TryAcquireAsync(this RateLimiter rateLimiter, int tokens, TimeSpan timeout)
        {
            ArgumentNullException.ThrowIfNull(rateLimiter);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tokens);

            var startTime = DateTime.UtcNow;
            var endTime = startTime + timeout;

            while (DateTime.UtcNow < endTime)
            {
                if (rateLimiter.HasTokens(tokens))
                {
                    rateLimiter.ConsumeTokens(tokens);
                    return true;
                }

                // Wait for a short interval before retrying
                var remainingTime = endTime - DateTime.UtcNow;
                if (remainingTime.TotalMilliseconds > 10)
                {
                    await Task.Delay(Math.Min(10, (int)remainingTime.TotalMilliseconds)).ConfigureAwait(false);
                }
                else
                {
                    break;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the current usage percentage of the rate limiter as a value between 0 and 100.
        /// </summary>
        /// <param name="rateLimiter">The rate limiter instance.</param>
        /// <returns>The current usage percentage (0-100).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rateLimiter"/> is null.</exception>
        public static double GetUsagePercentage(this RateLimiter rateLimiter)
        {
            ArgumentNullException.ThrowIfNull(rateLimiter);

            var status = rateLimiter.GetStatus(string.Empty);
            var maxTokens = status.MaxTokens;
            var currentTokens = status.CurrentTokens;

            if (maxTokens <= 0)
            {
                return 0;
            }

            return Math.Min(100, (currentTokens / maxTokens) * 100);
        }

        /// <summary>
        /// Resets the rate limiter for a specific identifier.
        /// </summary>
        /// <param name="rateLimiter">The rate limiter instance.</param>
        /// <param name="identifier">The identifier to reset rate limits for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rateLimiter"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is null or empty.</exception>
        public static void Reset(this RateLimiter rateLimiter, string identifier)
        {
            ArgumentNullException.ThrowIfNull(rateLimiter);
            ArgumentException.ThrowIfNullOrEmpty(identifier);

            rateLimiter.Reset(identifier);
        }

        /// <summary>
        /// Resets the rate limiter to its initial state.
        /// </summary>
        /// <param name="rateLimiter">The rate limiter instance.</param>
        /// <param name="clearAll">If true, clears all rate limit tracking data. If false, only resets token counters.</param>
        /// <exception cref="ArgumentNullException"><paramref name="rateLimiter"/> is null.</exception>
        public static void Reset(this RateLimiter rateLimiter, bool clearAll = false)
        {
            ArgumentNullException.ThrowIfNull(rateLimiter);

            if (clearAll)
            {
                rateLimiter.ClearAll();
            }
        }
    }
}