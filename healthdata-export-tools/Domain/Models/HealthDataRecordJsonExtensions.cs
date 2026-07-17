#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace HealthDataExportTools.Domain.Models;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for <see cref="HealthDataRecord"/> and its derived types.
/// </summary>
public static class HealthDataRecordJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a <see cref="HealthDataRecord"/> to a JSON string with custom options.
    /// </summary>
    /// <param name="value">The health data record to serialize. Cannot be null.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <param name="options">The JSON serializer options to use. Cannot be null.</param>
    /// <returns>JSON string representation of the record.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// <para>-or-</para>
    /// <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    private static string ToJson(HealthDataRecord value, bool indented, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);

        var localOptions = indented
            ? new JsonSerializerOptions(options)
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }
            : options;

        return JsonSerializer.Serialize(value, localOptions);
    }

    /// <summary>
    /// Serializes a <see cref="HealthDataRecord"/> to a JSON string.
    /// </summary>
    /// <param name="value">The health data record to serialize. Cannot be null.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>JSON string representation of the record.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this HealthDataRecord value, bool indented = false) =>
        ToJson(value, indented, _jsonOptions);

    /// <summary>
    /// Deserializes a <see cref="HealthDataRecord"/> from a JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize. Cannot be null or empty.</param>
    /// <returns>The deserialized <see cref="HealthDataRecord"/> instance, or <see langword="null"/> if JSON is invalid.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>, empty, or consists only of white-space.</exception>
    public static HealthDataRecord? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<HealthDataRecord>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="HealthDataRecord"/> from a JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize. Cannot be null or empty.</param>
    /// <param name="value">Output parameter receiving the deserialized record.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>, empty, or consists only of white-space.</exception>
    public static bool TryFromJson(string json, out HealthDataRecord? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<HealthDataRecord>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
