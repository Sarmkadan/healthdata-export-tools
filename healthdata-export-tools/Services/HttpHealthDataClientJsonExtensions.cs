#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Exceptions;

namespace HealthDataExportTools.Services;

/// <summary>
/// Provides System.Text.Json serialization extensions for HttpHealthDataClient
/// </summary>
public static class HttpHealthDataClientJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes an HttpHealthDataClient instance to a JSON string
    /// </summary>
    /// <param name="value">The HttpHealthDataClient instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the HttpHealthDataClient</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this HttpHealthDataClient value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an HttpHealthDataClient instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>An HttpHealthDataClient instance populated from JSON</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null or empty</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid or cannot be deserialized</exception>
    public static HttpHealthDataClient? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<HttpHealthDataClient>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an HttpHealthDataClient instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized HttpHealthDataClient if successful</param>
    /// <returns>True if deserialization succeeded; otherwise false</returns>
    public static bool TryFromJson(string json, out HttpHealthDataClient? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<HttpHealthDataClient>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
