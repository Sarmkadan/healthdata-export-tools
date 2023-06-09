// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Utilities;

/// <summary>
/// Utility class for JSON operations including parsing, validation, and transformation
/// </summary>
public static class JsonUtility
{
    private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    /// <summary>
    /// Parse JSON string into a dynamic object
    /// </summary>
    public static object? ParseJsonToDynamic(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<object>(json, DefaultOptions);
        }
        catch (JsonException ex)
        {
            throw new HealthDataException("Invalid JSON format", ex);
        }
    }

    /// <summary>
    /// Serialize object to JSON string with default formatting
    /// </summary>
    public static string SerializeToJson<T>(T obj)
    {
        try
        {
            return JsonSerializer.Serialize(obj, DefaultOptions);
        }
        catch (Exception ex)
        {
            throw new HealthDataException($"Failed to serialize object of type {typeof(T).Name}", ex);
        }
    }

    /// <summary>
    /// Deserialize JSON string to typed object
    /// </summary>
    public static T? DeserializeFromJson<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }
        catch (JsonException ex)
        {
            throw new HealthDataException($"Failed to deserialize JSON to type {typeof(T).Name}", ex);
        }
    }

    /// <summary>
    /// Pretty-print JSON string with indentation
    /// </summary>
    public static string PrettyPrint(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc, DefaultOptions);
        }
        catch (JsonException)
        {
            return json; // Return original if already invalid
        }
    }

    /// <summary>
    /// Validate JSON string structure without deserialization
    /// </summary>
    public static bool IsValidJson(string json)
    {
        try
        {
            using (JsonDocument.Parse(json))
            {
                return true;
            }
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Merge two JSON objects together, with second object overriding first
    /// </summary>
    public static string MergeJson(string json1, string json2)
    {
        try
        {
            var doc1 = JsonDocument.Parse(json1);
            var doc2 = JsonDocument.Parse(json2);

            using (var jsonWriter = new StringWriter())
            using (var writer = new Utf8JsonWriter(jsonWriter.GetStringWriter(), new JsonWriterOptions { Indented = true }))
            {
                // Deep merge logic would go here
                // For simplicity, we return the second document
                doc2.RootElement.WriteTo(writer);
                return jsonWriter.ToString();
            }
        }
        catch (JsonException ex)
        {
            throw new HealthDataException("Error merging JSON objects", ex);
        }
    }

    /// <summary>
    /// Extract a value from JSON by path (e.g., "data.records[0].value")
    /// </summary>
    public static object? GetValueByPath(string json, string path)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var pathParts = path.Split('.');

            JsonElement current = doc.RootElement;

            foreach (var part in pathParts)
            {
                if (part.Contains('['))
                {
                    // Handle array access like "records[0]"
                    var bracketIndex = part.IndexOf('[');
                    var propName = part[..bracketIndex];
                    var arrayIndex = int.Parse(part[(bracketIndex + 1)..part.LastIndexOf(']')]);

                    if (current.TryGetProperty(propName, out var arrayElement))
                    {
                        if (arrayElement.ValueKind == JsonValueKind.Array && arrayElement.GetArrayLength() > arrayIndex)
                        {
                            current = arrayElement[arrayIndex];
                        }
                    }
                }
                else if (!current.TryGetProperty(part, out current))
                {
                    return null;
                }
            }

            return current.GetRawText();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Minify JSON by removing whitespace
    /// </summary>
    public static string Minify(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var options = new JsonSerializerOptions { WriteIndented = false };
            return JsonSerializer.Serialize(doc, options);
        }
        catch
        {
            return json;
        }
    }

    /// <summary>
    /// Validate that JSON conforms to a specific schema (basic validation)
    /// </summary>
    public static List<string> ValidateJsonStructure(string json, Dictionary<string, string> requiredFields)
    {
        var errors = new List<string>();

        try
        {
            var doc = JsonDocument.Parse(json);

            foreach (var field in requiredFields)
            {
                if (!doc.RootElement.TryGetProperty(field.Key, out var element))
                {
                    errors.Add($"Missing required field: {field.Key}");
                }
                else if (element.ValueKind.ToString() != field.Value)
                {
                    errors.Add($"Field '{field.Key}' has incorrect type. Expected {field.Value}, got {element.ValueKind}");
                }
            }
        }
        catch (JsonException ex)
        {
            errors.Add($"Invalid JSON: {ex.Message}");
        }

        return errors;
    }
}
