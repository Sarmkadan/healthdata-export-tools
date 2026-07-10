# HealthDataParserService

The `HealthDataParserService` is a core utility within the `healthdata-export-tools` project designed to ingest, normalize, and aggregate health telemetry from heterogeneous sources. It provides asynchronous parsing capabilities for both JSON and CSV formats, automatically detecting source device types to ensure data fidelity. The service consolidates parsed information into a unified `HealthDataCollection` while maintaining accessible, typed lists for specific metrics such as sleep, heart rate, SpO2, steps, and general activity, facilitating downstream analysis and export operations.

## API

### Constructors

#### `public HealthDataParserService()`
Initializes a new instance of the `HealthDataParserService` class. This constructor sets up the internal state required for parsing operations and initializes the specific data record lists.

### Methods

#### `public Task<HealthDataCollection> ParseJsonAsync(string jsonContent)`
Asynchronously parses a JSON string containing health data into a structured collection.
*   **Parameters**:
    *   `jsonContent`: The raw JSON string to be parsed.
*   **Returns**: A `Task` representing the asynchronous operation, containing a `HealthDataCollection` with the extracted data.
*   **Throws**: Throws an exception if the input string is null, empty, or contains malformed JSON that cannot be mapped to the expected health data schema.

#### `public async Task<HealthDataCollection> ParseCsvAsync(string csvContent, string delimiter = ",")`
Asynchronously parses a CSV formatted string into a structured collection.
*   **Parameters**:
    *   `csvContent`: The raw CSV string to be parsed.
    *   `delimiter`: (Optional) The character used to separate values in the CSV. Defaults to a comma.
*   **Returns**: A `Task` representing the asynchronous operation, containing a `HealthDataCollection` with the extracted data.
*   **Throws**: Throws an exception if the CSV structure is invalid, headers are missing, or data conversion fails for specific metric columns.

#### `public DeviceType DetectDeviceType(string rawData)`
Analyzes raw data content to identify the originating device manufacturer or model type.
*   **Parameters**:
    *   `rawData`: A snippet of the raw source data (JSON or CSV) used for signature detection.
*   **Returns**: A `DeviceType` enumeration value indicating the detected source. Returns `DeviceType.Unknown` if no matching signature is found.
*   **Throws**: Generally does not throw unless `rawData` is null.

#### `public HealthDataCollection MergeCollections(params HealthDataCollection[] collections)`
Combines multiple `HealthDataCollection` instances into a single aggregated collection.
*   **Parameters**:
    *   `collections`: A variable number of `HealthDataCollection` objects to merge.
*   **Returns**: A new `HealthDataCollection` containing the union of all records from the input collections.
*   **Throws**: Throws an exception if the `collections` argument is null or empty.

#### `public int GetTotalRecordCount()`
Calculates the total number of individual data records currently held across all specific data type lists within the service instance.
*   **Parameters**: None.
*   **Returns**: An integer representing the sum of records in `SleepRecords`, `HeartRateRecords`, `SpO2Records`, `StepsRecords`, and `ActivityRecords`.
*   **Throws**: None.

### Properties

#### `public List<SleepData> SleepRecords`
Gets the list of parsed sleep tracking entries. This list is populated during parsing operations and reflects the current state of sleep data held by the service.

#### `public List<HeartRateData> HeartRateRecords`
Gets the list of parsed heart rate measurements. This list is populated during parsing operations and reflects the current state of heart rate data held by the service.

#### `public List<SpO2Data> SpO2Records`
Gets the list of parsed blood oxygen saturation (SpO2) measurements. This list is populated during parsing operations and reflects the current state of SpO2 data held by the service.

#### `public List<StepsData> StepsRecords`
Gets the list of parsed step count entries. This list is populated during parsing operations and reflects the current state of step data held by the service.

#### `public List<ActivityData> ActivityRecords`
Gets the list of parsed general activity sessions (e.g., workouts, active minutes). This list is populated during parsing operations and reflects the current state of activity data held by the service.

#### `public List<HealthMetric> Metrics`
Gets a consolidated list of generic health metrics that may not fit into the specific strongly-typed lists above, or serves as an aggregate view depending on the parsing context.

## Usage

### Example 1: Parsing JSON and Detecting Device Source
This example demonstrates loading raw JSON data, identifying the source device, and accessing specific heart rate records.

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using HealthDataExportTools;

public class JsonIngestionExample
{
    public async Task RunAsync()
    {
        var service = new HealthDataParserService();
        string jsonPath = "data/export_sample.json";
        
        if (!File.Exists(jsonPath)) return;

        string rawJson = await File.ReadAllTextAsync(jsonPath);

        // Detect the device type before full parsing if needed for logging
        var deviceType = service.DetectDeviceType(rawJson);
        Console.WriteLine($"Detected Source: {deviceType}");

        // Parse the data asynchronously
        var collection = await service.ParseJsonAsync(rawJson);

        // Access specific typed records
        if (service.HeartRateRecords.Count > 0)
        {
            var latestHeartRate = service.HeartRateRecords[^1];
            Console.WriteLine($"Latest BPM: {latestHeartRate.BeatsPerMinute}");
        }

        Console.WriteLine($"Total records loaded: {service.GetTotalRecordCount()}");
    }
}
```

### Example 2: Merging Multiple CSV Exports
This example illustrates parsing multiple CSV files from different dates and merging them into a single comprehensive dataset.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HealthDataExportTools;

public class CsvMergeExample
{
    public async Task RunAsync()
    {
        var service = new HealthDataParserService();
        var files = new List<string> { "week1.csv", "week2.csv", "week3.csv" };
        var collections = new List<HealthDataCollection>();

        foreach (var file in files)
        {
            if (System.IO.File.Exists(file))
            {
                string csvContent = await System.IO.File.ReadAllTextAsync(file);
                var collection = await service.ParseCsvAsync(csvContent, ",");
                collections.Add(collection);
            }
        }

        // Merge all parsed collections into one
        if (collections.Count > 0)
        {
            var mergedData = service.MergeCollections(collections.ToArray());
            
            // The service internal lists are updated based on the last parse, 
            // but the mergedData object contains the aggregate result.
            Console.WriteLine($"Merged Sleep Records: {mergedData.SleepRecords.Count}");
            Console.WriteLine($"Merged Steps Records: {mergedData.StepsRecords.Count}");
        }
    }
}
```

## Notes

*   **Thread Safety**: The `HealthDataParserService` is not thread-safe. The public lists (`SleepRecords`, `HeartRateRecords`, etc.) are mutable `List<T>` instances. Concurrent calls to parsing methods or simultaneous modification of these lists from multiple threads may result in data corruption or `InvalidOperationException`. External synchronization is required when accessing or modifying these properties in a multi-threaded environment.
*   **State Management**: The specific data list properties (e.g., `SleepRecords`) reflect the state of the *last* successful parsing operation performed on that specific service instance. When using `MergeCollections`, the returned `HealthDataCollection` contains the aggregated data, but the service instance's internal lists are not automatically updated to reflect the merged result unless a subsequent parse overwrites them.
*   **Edge Cases**:
    *   **Empty Inputs**: Passing null or empty strings to `ParseJsonAsync` or `ParseCsvAsync` will result in an exception. Callers should validate input existence before invocation.
    *   **Schema Mismatches**: If the input data lacks expected columns (CSV) or properties (JSON), the parser may skip those specific records or throw a format exception depending on the severity of the mismatch.
    *   **Device Detection**: `DetectDeviceType` relies on heuristic analysis of the raw string. If the data format is highly obfuscated or minified without standard headers/signatures, it may return `DeviceType.Unknown`.
