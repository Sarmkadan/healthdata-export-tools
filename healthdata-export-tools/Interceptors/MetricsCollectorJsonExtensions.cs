#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace HealthDataExportTools.Interceptors;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for MetricsCollector
/// </summary>
public static class MetricsCollectorJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes a MetricsCollector instance to a JSON string
    /// </summary>
    /// <param name="value">The metrics collector to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the metrics collector</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this MetricsCollector value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a MetricsCollector instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A MetricsCollector instance, or null if the JSON is empty or whitespace</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized</exception>
    public static MetricsCollector? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var result = JsonSerializer.Deserialize<MetricsCollector>(json, _jsonOptions);
        return result;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a MetricsCollector instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized MetricsCollector, or null if deserialization fails</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    public static bool TryFromJson(string json, out MetricsCollector? value)
    {
        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}