using System;
using System.Net;
using System.Text.Json;

namespace HealthDataExportTools.Integration
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for <see cref="RetryHandler"/> and <see cref="RetryHandlerOptions"/>
    /// </summary>
    public static class RetryHandlerJsonExtensions
    {
        private static readonly JsonSerializerOptions Options = new(JsonSerializerOptions.Default)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        /// <summary>
        /// Serializes a <see cref="RetryHandlerOptions"/> instance to JSON string.
        /// </summary>
        /// <param name="value">The retry handler options to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the retry handler options.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this RetryHandlerOptions value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var localOptions = new JsonSerializerOptions(Options)
            {
                WriteIndented = indented
            };

            return JsonSerializer.Serialize(value, localOptions);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="RetryHandlerOptions"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized retry handler options instance, or null if deserialization fails.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static RetryHandlerOptions? FromJsonToOptions(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            return JsonSerializer.Deserialize<RetryHandlerOptions>(json, Options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="RetryHandlerOptions"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized retry handler options instance if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static bool TryFromJsonToOptions(string json, out RetryHandlerOptions? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                value = JsonSerializer.Deserialize<RetryHandlerOptions>(json, Options);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Serializes a <see cref="RetryHandler"/> instance to JSON string.
        /// Note: RetryHandler contains runtime state (circuit breaker) that cannot be meaningfully serialized.
        /// This method serializes only the configuration options.
        /// </summary>
        /// <param name="value">The retry handler instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the retry handler configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this RetryHandler value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            // Serialize the options instead of the handler itself
            var options = new RetryHandlerOptions
            {
                MaxRetries = value.GetMaxRetries(),
                InitialDelayMs = value.GetInitialDelayMs(),
                MaxDelayMs = value.GetMaxDelayMs(),
                CircuitBreakerFailureThreshold = value.GetCircuitBreakerFailureThreshold(),
                CircuitBreakerSuccessThreshold = value.GetCircuitBreakerSuccessThreshold(),
                CircuitBreakerMaxBackoffMs = value.GetCircuitBreakerMaxBackoffMs(),
                CircuitBreakerHalfOpenMinDelayMs = value.GetCircuitBreakerHalfOpenMinDelayMs(),
                HonorRetryAfterHeader = value.GetHonorRetryAfterHeader(),
                UseDecorrelatedJitter = value.GetUseDecorrelatedJitter(),
                IdempotentHttpMethods = value.GetIdempotentHttpMethods(),
                RetryableStatusCodes = value.GetRetryableStatusCodes(),
                RetryableExceptions = value.GetRetryableExceptions(),
                CircuitBreakerEnabled = value.GetCircuitBreakerEnabled()
            };

            var localOptions = new JsonSerializerOptions(Options)
            {
                WriteIndented = indented
            };

            return JsonSerializer.Serialize(options, localOptions);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="RetryHandler"/> instance.
        /// Note: This creates a new handler with default state, not restoring runtime state.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="logger">The logger instance to use for the created handler.</param>
        /// <returns>The deserialized retry handler instance, or null if deserialization fails.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> or <paramref name="logger"/> is null.</exception>
        public static RetryHandler? FromJson(this RetryHandler value, string json, ILogger<RetryHandler> logger)
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentNullException.ThrowIfNull(logger);

            var options = JsonSerializer.Deserialize<RetryHandlerOptions>(json, Options);
            return options != null ? new RetryHandler(logger, options) : null;
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="RetryHandler"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="logger">The logger instance to use for the created handler.</param>
        /// <param name="value">Receives the deserialized retry handler instance if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> or <paramref name="logger"/> is null.</exception>
        public static bool TryFromJson(this RetryHandler value, string json, ILogger<RetryHandler> logger, out RetryHandler? handler)
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentNullException.ThrowIfNull(logger);

            try
            {
                var options = JsonSerializer.Deserialize<RetryHandlerOptions>(json, Options);
                handler = options != null ? new RetryHandler(logger, options) : null;
                return true;
            }
            catch (JsonException)
            {
                handler = null;
                return false;
            }
        }

        // Reflection-based property accessors for RetryHandler
        private static int GetMaxRetries(this RetryHandler handler) => handler.GetType().GetProperty("MaxRetries")?.GetValue(handler) as int? ?? 3;
        private static int GetInitialDelayMs(this RetryHandler handler) => handler.GetType().GetProperty("InitialDelayMs")?.GetValue(handler) as int? ?? 100;
        private static int GetMaxDelayMs(this RetryHandler handler) => handler.GetType().GetProperty("MaxDelayMs")?.GetValue(handler) as int? ?? 30000;
        private static int GetCircuitBreakerFailureThreshold(this RetryHandler handler) => handler.GetType().GetProperty("CircuitBreakerFailureThreshold")?.GetValue(handler) as int? ?? 5;
        private static int GetCircuitBreakerSuccessThreshold(this RetryHandler handler) => handler.GetType().GetProperty("CircuitBreakerSuccessThreshold")?.GetValue(handler) as int? ?? 2;
        private static int GetCircuitBreakerMaxBackoffMs(this RetryHandler handler) => handler.GetType().GetProperty("CircuitBreakerMaxBackoffMs")?.GetValue(handler) as int? ?? 60000;
        private static int GetCircuitBreakerHalfOpenMinDelayMs(this RetryHandler handler) => handler.GetType().GetProperty("CircuitBreakerHalfOpenMinDelayMs")?.GetValue(handler) as int? ?? 5000;
        private static bool GetHonorRetryAfterHeader(this RetryHandler handler) => (bool)(handler.GetType().GetProperty("HonorRetryAfterHeader")?.GetValue(handler) ?? true);
        private static bool GetUseDecorrelatedJitter(this RetryHandler handler) => (bool)(handler.GetType().GetProperty("UseDecorrelatedJitter")?.GetValue(handler) ?? true);
        private static HashSet<string> GetIdempotentHttpMethods(this RetryHandler handler) => handler.GetType().GetProperty("IdempotentHttpMethods")?.GetValue(handler) as HashSet<string> ?? new();
        private static HashSet<HttpStatusCode> GetRetryableStatusCodes(this RetryHandler handler) => handler.GetType().GetProperty("RetryableStatusCodes")?.GetValue(handler) as HashSet<HttpStatusCode> ?? new();
        private static HashSet<Type> GetRetryableExceptions(this RetryHandler handler) => handler.GetType().GetProperty("RetryableExceptions")?.GetValue(handler) as HashSet<Type> ?? new();
        private static bool GetCircuitBreakerEnabled(this RetryHandler handler) => (bool)(handler.GetType().GetProperty("CircuitBreakerEnabled")?.GetValue(handler) ?? true);
    }
}