#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace HealthDataExportTools.Formatters;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="CsvFormatter"/>.
/// Enables serialization and deserialization of CSV formatter configuration and state.
/// </summary>
public static class CsvFormatterJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the <see cref="CsvFormatter"/> to a JSON string.
    /// </summary>
    /// <param name="value">The CSV formatter to serialize. Cannot be null.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON representation of the CSV formatter.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this CsvFormatter value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="CsvFormatter"/> from a JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize. Cannot be null or empty.</param>
    /// <returns>The deserialized <see cref="CsvFormatter"/> instance, or null if JSON is invalid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    public static CsvFormatter? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json.Trim());

        return JsonSerializer.Deserialize<CsvFormatter>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="CsvFormatter"/> from a JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize. Cannot be null.</param>
    /// <param name="value">Receives the deserialized <see cref="CsvFormatter"/> if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out CsvFormatter? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<CsvFormatter>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
