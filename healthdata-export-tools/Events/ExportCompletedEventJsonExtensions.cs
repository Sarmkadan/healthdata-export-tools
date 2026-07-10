#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace HealthDataExportTools.Events;

/// <summary>
/// Provides System.Text.Json serialization extensions for ExportCompletedEvent
/// </summary>
public static class ExportCompletedEventJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };

    /// <summary>
    /// Serializes ExportCompletedEvent to JSON string
    /// </summary>
    /// <param name="value">The event to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this ExportCompletedEvent value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true,
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes ExportCompletedEvent from JSON string
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized event or null if JSON is invalid</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null</exception>
    /// <exception cref="ArgumentException">Thrown when json is empty or whitespace</exception>
    public static ExportCompletedEvent? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<ExportCompletedEvent>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize ExportCompletedEvent from JSON string
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Deserialized event, or null if deserialization fails</param>
    /// <returns>True if deserialization succeeds; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null</exception>
    /// <exception cref="ArgumentException">Thrown when json is empty or whitespace</exception>
    public static bool TryFromJson(string json, out ExportCompletedEvent? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<ExportCompletedEvent>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
