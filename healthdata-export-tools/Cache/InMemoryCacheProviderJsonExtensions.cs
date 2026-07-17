#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace HealthDataExportTools.Cache;

/// <summary>
/// Provides System.Text.Json serialization extensions for InMemoryCacheProvider
/// </summary>
public static class InMemoryCacheProviderJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        PropertyNameCaseInsensitive = true,
        // Ensure culture-invariant serialization for deterministic JSON output
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// Serializes the InMemoryCacheProvider instance to a JSON string
    /// </summary>
    /// <param name="value">The cache provider instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the cache provider</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static string ToJson(this InMemoryCacheProvider value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string back into an InMemoryCacheProvider instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>An InMemoryCacheProvider instance, or null if the JSON is null, empty, or whitespace-only</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized into an InMemoryCacheProvider</exception>
    public static InMemoryCacheProvider? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<InMemoryCacheProvider>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string back into an InMemoryCacheProvider instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Output parameter receiving the deserialized instance, or null on failure</param>
    /// <returns>True if deserialization succeeded; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static bool TryFromJson(string json, out InMemoryCacheProvider? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<InMemoryCacheProvider>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}