#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;

namespace HealthDataExportTools.DTOs;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for ImportResultDto
/// </summary>
public static class ImportResultDtoJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	/// <summary>
	/// Serializes an ImportResultDto to a JSON string
	/// </summary>
	/// <param name="value">The DTO to serialize</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability</param>
	/// <returns>A JSON string representation of the DTO</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
	public static string ToJson(this ImportResultDto value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonSerializerOptions)
			{
				WriteIndented = true
			}
			: _jsonSerializerOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to an ImportResultDto instance
	/// </summary>
	/// <param name="json">The JSON string to deserialize</param>
	/// <returns>The deserialized <see cref="ImportResultDto"/> instance if successful; otherwise null</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or consists only of whitespace</exception>
	public static ImportResultDto? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);
		ArgumentException.ThrowIfNullOrWhiteSpace(json);

		try
		{
			return JsonSerializer.Deserialize<ImportResultDto>(json, _jsonSerializerOptions);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to an ImportResultDto instance
	/// </summary>
	/// <param name="json">The JSON string to deserialize</param>
	/// <param name="value">Receives the deserialized DTO if successful</param>
	/// <returns>True if deserialization succeeded; otherwise false</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or consists only of whitespace</exception>
	public static bool TryFromJson(string json, out ImportResultDto? value)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);
		ArgumentException.ThrowIfNullOrWhiteSpace(json);

		try
		{
			value = JsonSerializer.Deserialize<ImportResultDto>(json, _jsonSerializerOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}