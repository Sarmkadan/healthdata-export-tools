#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace HealthDataExportTools.Tests;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for
/// <see cref="TrendAnomalyDetectionServiceTests"/> instances.
/// </summary>
public static class TrendAnomalyDetectionServiceTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { MakeInternalTypesPublic } }
    };

    /// <summary>
    /// Converts a <see cref="TrendAnomalyDetectionServiceTests"/> instance to its JSON representation.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this TrendAnomalyDetectionServiceTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="TrendAnomalyDetectionServiceTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance, or <see langword="null"/> if the JSON represents a null value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static TrendAnomalyDetectionServiceTests? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return JsonSerializer.Deserialize<TrendAnomalyDetectionServiceTests>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="TrendAnomalyDetectionServiceTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string json, out TrendAnomalyDetectionServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<TrendAnomalyDetectionServiceTests>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// JsonTypeInfo modifier to make internal types public during serialization/deserialization.
    /// This allows System.Text.Json to access internal members of types in this assembly.
    /// </summary>
    private static void MakeInternalTypesPublic(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Type.Assembly == typeof(TrendAnomalyDetectionServiceTests).Assembly)
        {
            jsonTypeInfo.Options.IncludeFields = true;
        }
    }
}