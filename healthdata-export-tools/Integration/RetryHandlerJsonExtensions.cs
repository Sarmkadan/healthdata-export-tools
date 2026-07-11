using System;
using System.Text.Json;

namespace HealthDataExportTools.Integration
{
    /// <summary>
    /// Provides JSON serialization and deserialization extensions for <see cref="RetryHandler"/>
    /// </summary>
    public static class RetryHandlerJsonExtensions
    {
        private static readonly JsonSerializerOptions Options = new(JsonSerializerOptions.Default)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Serializes a <see cref="RetryHandler"/> instance to JSON string.
        /// </summary>
        /// <param name="value">The retry handler instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the retry handler.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this RetryHandler value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            return indented
                ? JsonSerializer.Serialize(value, new JsonSerializerOptions(Options) { WriteIndented = true })
                : JsonSerializer.Serialize(value, Options);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="RetryHandler"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized retry handler instance, or null if deserialization fails.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static RetryHandler? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            return JsonSerializer.Deserialize<RetryHandler>(json, Options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="RetryHandler"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized retry handler instance if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static bool TryFromJson(string json, out RetryHandler? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                value = JsonSerializer.Deserialize<RetryHandler>(json, Options);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}