# HealthDataParserServiceTests

`HealthDataParserServiceTests` is a test suite designed to validate the functionality of the `HealthDataParserService` within the `healthdata-export-tools` project. It ensures that health data parsing from JSON, device type detection, and collection merging logic operate correctly under various conditions, including expected inputs, missing optional fields, and malformed data.

## API

*   **`ParseJsonAsync_ShouldParseAllHealthDataTypesSuccessfully`**
    *   **Purpose:** Verifies that a valid JSON input containing all supported health data types is correctly parsed into the corresponding object model.
    *   **Parameters:** None.
    *   **Return:** `Task`.
    *   **Throws:** N/A.

*   **`ParseJsonAsync_ShouldHandleMissingOptionalFields`**
    *   **Purpose:** Confirms that the parser gracefully handles JSON inputs where non-mandatory fields are omitted, populating the resulting objects with appropriate default values.
    *   **Parameters:** None.
    *   **Return:** `Task`.
    *   **Throws:** N/A.

*   **`ParseJsonAsync_ShouldThrowParsingExceptionForInvalidJson`**
    *   **Purpose:** Ensures that the service correctly identifies malformed JSON data and throws a specialized `ParsingException` instead of producing partial or corrupted data.
    *   **Parameters:** None.
    *   **Return:** `Task`.
    *   **Throws:** `ParsingException`.

*   **`ParseJsonAsync_ShouldReturnEmptyCollectionsForEmptyArrays`**
    *   **Purpose:** Validates that JSON payloads containing empty arrays for health record collections are mapped to empty collections rather than null references.
    *   **Parameters:** None.
    *   **Return:** `Task`.
    *   **Throws:** N/A.

*   **`DetectDeviceType_ShouldReturnCorrectDeviceTypeForKnownKeywords`**
    *   **Purpose:** Validates the device detection heuristic by checking against a set of known keywords associated with specific hardware manufacturers or device models.
    *   **Parameters:** None.
    *   **Return:** `void`.
    *   **Throws:** N/A.

*   **`DetectDeviceType_ShouldReturnUnknownForUnknownKeywords`**
    *   **Purpose:** Ensures that if a device string does not match any known keywords, the service returns an "Unknown" or default device type enumeration.
    *   **Parameters:** None.
    *   **Return:** `void`.
    *   **Throws:** N/A.

*   **`DetectDeviceType_ShouldBeCaseInsensitive`**
    *   **Purpose:** Confirms that the device detection logic handles variations in casing (e.g., "Apple", "apple", "APPLE") identically.
    *   **Parameters:** None.
    *   **Return:** `void`.
    *   **Throws:** N/A.

*   **`MergeCollections_ShouldCombineAllRecordsFromMultipleCollections`**
    *   **Purpose:** Verifies that the merging functionality correctly concatenates records from multiple distinct input collections into a single result set without data loss.
    *   **Parameters:** None.
    *   **Return:** `void`.
    *   **Throws:** N/A.

*   **`MergeCollections_ShouldHandleEmptyCollectionsGracefully`**
    *   **Purpose:** Ensures that merging operations involving empty or null input collections do not cause runtime exceptions and produce an empty or valid resulting collection.
    *   **Parameters:** None.
    *   **Return:** `void`.
    *   **Throws:** N/A.

## Usage

### Testing Parsing Logic
```csharp
[Fact]
public async Task Test_Parsing_InvalidJson()
{
    var service = new HealthDataParserService();
    // Act & Assert
    await Assert.ThrowsAsync<ParsingException>(() => 
        service.ParseJsonAsync("{\"invalid\": \"json\""));
}
```

### Testing Device Detection
```csharp
[Fact]
public void Test_DeviceType_CaseInsensitivity()
{
    var service = new HealthDataParserService();
    // Act
    var deviceType = service.DetectDeviceType("APPLEWATCH");
    // Assert
    Assert.Equal(DeviceType.AppleWatch, deviceType);
}
```

## Notes

*   **Edge Cases:** The `ParseJsonAsync` methods assume valid JSON structure; however, tests explicitly cover handling of empty arrays and missing optional fields. Ensure that large JSON payloads are tested if performance is a critical factor.
*   **Thread Safety:** The `HealthDataParserService` itself should be treated as thread-safe for reading operations, as it typically acts as a stateless parser. These tests do not explicitly test concurrent access, but the service design generally supports it.
