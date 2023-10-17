# JsonUtility

The `JsonUtility` class provides a centralized, static interface for common JSON manipulation tasks within the `healthdata-export-tools` project. It abstracts serialization, deserialization, validation, and transformation logic, offering utilities to parse JSON into dynamic objects, enforce schema structures, merge payloads, and format output for both human readability and minimal transmission size. This class is designed to streamline data handling workflows without requiring external dependencies for basic operations.

## API

### `ParseJsonToDynamic`
```csharp
public static object? ParseJsonToDynamic(string json)
```
Parses a JSON string into a dynamic object structure (typically `ExpandoObject` or `Dictionary<string, object>`).
- **Parameters**: `json` – The JSON string to parse.
- **Returns**: An `object?` representing the dynamic structure, or `null` if the input is null or represents a JSON `null`.
- **Throws**: Throws a parsing exception if the input string is malformed JSON.

### `SerializeToJson<T>`
```csharp
public static string SerializeToJson<T>(T value)
```
Serializes a generic object of type `T` into a JSON string.
- **Parameters**: `value` – The object instance to serialize.
- **Returns**: A JSON-formatted string representation of the object.
- **Throws**: Throws an exception if the object contains circular references or types that are not serializable.

### `DeserializeFromJson<T>`
```csharp
public static T? DeserializeFromJson<T>(string json)
```
Deserializes a JSON string into an object of type `T`.
- **Parameters**: `json` – The JSON string to deserialize.
- **Returns**: An instance of `T`, or `null` if the JSON represents a null value or if `T` is a reference type and deserialization yields no result.
- **Throws**: Throws an exception if the JSON structure does not match the expected schema of `T` or if the JSON is invalid.

### `PrettyPrint`
```csharp
public static string PrettyPrint(string json)
```
Formats a compact JSON string into an indented, human-readable format.
- **Parameters**: `json` – The raw JSON string.
- **Returns**: A formatted string with indentation and line breaks.
- **Throws**: Throws an exception if the input is not valid JSON.

### `IsValidJson`
```csharp
public static bool IsValidJson(string json)
```
Validates whether a given string constitutes well-formed JSON.
- **Parameters**: `json` – The string to validate.
- **Returns**: `true` if the string is valid JSON; otherwise, `false`.
- **Throws**: Does not throw exceptions; returns `false` on failure.

### `MergeJson`
```csharp
public static string MergeJson(string json1, string json2)
```
Merges two JSON objects into a single JSON string. In case of key conflicts, values from the second JSON typically overwrite the first.
- **Parameters**: `json1` – The base JSON string. `json2` – The JSON string containing updates or additional data.
- **Returns**: A new JSON string representing the merged result.
- **Throws**: Throws an exception if either input is not a valid JSON object or if merging logic fails due to type mismatches (e.g., merging an array into an object).

### `GetValueByPath`
```csharp
public static object? GetValueByPath(string json, string path)
```
Extracts a specific value from a JSON string using a dot-notated path (e.g., "patient.id").
- **Parameters**: `json` – The source JSON string. `path` – The dot-separated path to the desired value.
- **Returns**: The value found at the path as an `object?`, or `null` if the path does not exist.
- **Throws**: Throws an exception if the JSON is invalid or if the path traversal encounters a type mismatch (e.g., attempting to traverse a property on a primitive value).

### `Minify`
```csharp
public static string Minify(string json)
```
Removes all unnecessary whitespace from a JSON string to reduce payload size.
- **Parameters**: `json` – The JSON string to minify.
- **Returns**: A compact JSON string without whitespace.
- **Throws**: Throws an exception if the input is not valid JSON.

### `ValidateJsonStructure`
```csharp
public static List<string> ValidateJsonStructure(string json, IEnumerable<string> requiredPaths)
```
Validates that a JSON string contains all specified required paths.
- **Parameters**: `json` – The JSON string to validate. `requiredPaths` – A collection of dot-notated paths that must exist in the JSON.
- **Returns**: A `List<string>` containing error messages for any missing paths. If the list is empty, the structure is valid.
- **Throws**: Throws an exception if the input JSON is malformed.

## Usage

### Example 1: Deserialization and Validation
This example demonstrates deserializing a patient record and ensuring critical fields exist before processing.

```csharp
using HealthDataExportTools;

string patientJson = "{\"id\": 101, \"name\": \"Doe\", \"vitals\": {\"bp\": \"120/80\"}}";
var requiredFields = new List<string> { "id", "name", "vitals.bp" };

// Validate structure first
var errors = JsonUtility.ValidateJsonStructure(patientJson, requiredFields);
if (errors.Any())
{
    Console.WriteLine($"Invalid structure: {string.Join(", ", errors)}");
    return;
}

// Deserialize into strong type
var patient = JsonUtility.DeserializeFromJson<PatientRecord>(patientJson);
if (patient != null)
{
    Console.WriteLine($"Processing patient: {patient.Name}");
}
```

### Example 2: Merging and Formatting
This example shows how to merge an update payload into an existing record and output a pretty-printed result for logging.

```csharp
using HealthDataExportTools;

string baseRecord = "{\"patientId\": 55, \"status\": \"admitted\"}";
string updatePayload = "{\"status\": \"discharged\", \"dischargeDate\": \"2023-10-01\"}";

if (JsonUtility.IsValidJson(baseRecord) && JsonUtility.IsValidJson(updatePayload))
{
    string merged = JsonUtility.MergeJson(baseRecord, updatePayload);
    string formatted = JsonUtility.PrettyPrint(merged);
    
    // Log the readable final state
    System.Diagnostics.Debug.WriteLine(formatted);
}
```

## Notes

- **Thread Safety**: As all members are static and rely on local variables or immutable string inputs, `JsonUtility` is inherently thread-safe for read-only operations. However, care should be taken if the underlying serialization library utilizes static global state for configuration.
- **Null Handling**: Methods returning `object?` or `T?` explicitly handle JSON `null` values by returning C# `null`. Callers must perform null checks before dereferencing results.
- **Path Syntax**: `GetValueByPath` and `ValidateJsonStructure` utilize dot notation for navigation. Paths traversing through arrays (e.g., `items[0].id`) may require specific syntax support depending on the internal implementation; standard property access is guaranteed.
- **Exception Strategy**: With the exception of `IsValidJson`, most methods will throw exceptions upon encountering malformed JSON. It is recommended to wrap calls in try-catch blocks or pre-validate using `IsValidJson` when dealing with untrusted input sources.
- **Merge Behavior**: The `MergeJson` method performs a shallow to mid-level merge. Complex nested object conflicts are resolved by overwriting the target key with the source value; array concatenation is not performed by default.
