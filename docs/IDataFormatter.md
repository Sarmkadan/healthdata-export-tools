# IDataFormatter
The `IDataFormatter` type is designed to provide metadata and configuration options for formatting and exporting health data. It serves as a foundation for various data formatting and export operations, allowing for customization and flexibility in handling different types of health data.

## API
* `public string Name`: Gets the name of the data formatter. This property returns a string representing the formatter's name and does not take any parameters. It does not throw any exceptions.
* `public string Description`: Gets the description of the data formatter. This property returns a string representing the formatter's description and does not take any parameters. It does not throw any exceptions.
* `public string FileExtension`: Gets the file extension associated with the data formatter. This property returns a string representing the file extension and does not take any parameters. It does not throw any exceptions.
* `public bool IsCompressible`: Gets a value indicating whether the data formatter supports compression. This property returns a boolean value representing whether compression is supported and does not take any parameters. It does not throw any exceptions.
* `public int MaxRecordsPerFile`: Gets the maximum number of records that can be written to a single file. This property returns an integer value representing the maximum number of records and does not take any parameters. It does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `IDataFormatter` type:
```csharp
// Example 1: Creating a custom data formatter
public class CustomFormatter : IDataFormatter
{
    public string Name => "Custom Formatter";
    public string Description => "A custom data formatter";
    public string FileExtension => ".csv";
    public bool IsCompressible => true;
    public int MaxRecordsPerFile => 10000;
}

// Example 2: Using the IDataFormatter interface to configure data export
public void ExportData(IDataFormatter formatter, IEnumerable<HealthData> data)
{
    string filePath = $"export.{formatter.FileExtension}";
    using (var writer = new StreamWriter(filePath))
    {
        foreach (var record in data.Take(formatter.MaxRecordsPerFile))
        {
            writer.WriteLine(record.ToString());
        }
    }
}
```

## Notes
When implementing the `IDataFormatter` interface, consider the following edge cases:
* The `MaxRecordsPerFile` property should be set to a reasonable value to avoid excessive memory usage or file sizes.
* The `IsCompressible` property should be set to `true` only if the formatter supports compression, as this may impact performance.
* The `FileExtension` property should be set to a valid file extension to ensure correct file handling.
* The `IDataFormatter` interface is not thread-safe by default, so implementers should ensure proper synchronization when accessing or modifying its properties in a multithreaded environment.
