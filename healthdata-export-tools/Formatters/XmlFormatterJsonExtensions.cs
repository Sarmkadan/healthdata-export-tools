#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HealthDataExportTools.Formatters;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="XmlFormatter"/>.
/// Enables serialization and deserialization of XML formatter configuration and state.
/// </summary>
public static class XmlFormatterJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the <see cref="XmlFormatter"/> to a JSON string.
    /// </summary>
    /// <param name="value">The XML formatter to serialize. Cannot be null.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON representation of the XML formatter.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this XmlFormatter value, bool indented = false) =>
        ToJsonAsync(value, indented).GetAwaiter().GetResult();

    /// <summary>
    /// Asynchronously serializes the <see cref="XmlFormatter"/> to a JSON string.
    /// </summary>
    /// <param name="value">The XML formatter to serialize. Cannot be null.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON representation of the XML formatter.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static async Task<string> ToJsonAsync(this XmlFormatter value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, value, options).ConfigureAwait(false);
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Deserializes an <see cref="XmlFormatter"/> from a JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize. Cannot be null or empty.</param>
    /// <returns>The deserialized <see cref="XmlFormatter"/> instance, or null if JSON is invalid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    public static XmlFormatter? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json.Trim());

        return JsonSerializer.Deserialize<XmlFormatter>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize an <see cref="XmlFormatter"/> from a JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize. Cannot be null.</param>
    /// <param name="value">Receives the deserialized <see cref="XmlFormatter"/> if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out XmlFormatter? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<XmlFormatter>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}