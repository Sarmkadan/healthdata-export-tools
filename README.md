// existing content ...

// IDataFormatter

The `IDataFormatter` interface defines a contract for data formatters that are responsible for transforming health data into a specific output format. A data formatter provides metadata about the format, such as its name, description, file extension, and whether it supports compression. 

### Usage Example

```csharp
using HealthDataExportTools.Formatters;

// Assume a concrete formatter class implementing IDataFormatter
public class JsonFormatter : IDataFormatter
{
    public string FileExtension => ".json";
    public string FormatName => "JSON";
    public string Description => "Formats health data in JSON.";
    public bool IsCompressible => true;
    public int MaxRecordsPerFile => 10000;

    // ... other IDataFormatter members ...
}

// Usage
IDataFormatter formatter = new JsonFormatter();
Console.WriteLine($"Formatter Name: {formatter.FormatName}");
Console.WriteLine($"File Extension: {formatter.FileExtension}");
Console.WriteLine($"Description: {formatter.Description}");
Console.WriteLine($"Is Compressible: {formatter.IsCompressible}");
Console.WriteLine($"Max Records Per File: {formatter.MaxRecordsPerFile}");
```
