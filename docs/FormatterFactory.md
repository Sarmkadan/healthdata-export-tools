# FormatterFactory
The `FormatterFactory` class is a central component in the `healthdata-export-tools` project, responsible for managing and providing access to various data formatters. It allows registration of custom formatters, retrieval of formatters by format or file extension, and conversion of data to multiple formats. This class serves as a factory and registry for data formatters, enabling flexible and extensible data formatting capabilities.

## API
* `public FormatterFactory`: The constructor for creating a new instance of the `FormatterFactory` class.
* `public void RegisterFormatter`: Registers a custom data formatter with the factory. This method takes no parameters and returns no value, but it throws if the registration fails.
* `public IDataFormatter? GetFormatter`: Retrieves a data formatter by format. It takes no parameters and returns an `IDataFormatter` instance or `null` if no formatter is found for the given format.
* `public List<IDataFormatter> GetAllFormatters`: Returns a list of all registered data formatters.
* `public List<string> GetSupportedFormats`: Retrieves a list of all supported data formats.
* `public bool IsSupportedFormat`: Checks if a given format is supported by any registered formatter.
* `public static FormatterFactory CreateDefault`: Creates a new instance of the `FormatterFactory` class with default settings and registrations.
* `public IDataFormatter? GetFormatterByExtension`: Retrieves a data formatter by file extension. It returns an `IDataFormatter` instance or `null` if no formatter is found for the given extension.
* `public async Task<Dictionary<string, string>> FormatToAllAsync`: Asynchronously formats data to all supported formats and returns a dictionary with format names as keys and formatted data as values.
* `public List<FormatterInfo> GetFormatterInfo`: Returns a list of information about all registered formatters.

## Usage
The following examples demonstrate how to use the `FormatterFactory` class:
```csharp
// Example 1: Registering a custom formatter and formatting data
var factory = new FormatterFactory();
factory.RegisterFormatter(); // Assuming a custom formatter is registered
var formatter = factory.GetFormatter();
if (formatter != null)
{
    var data = "Sample data to be formatted";
    var formattedData = formatter.Format(data);
    Console.WriteLine(formattedData);
}

// Example 2: Using the default factory to format data to all supported formats
var defaultFactory = FormatterFactory.CreateDefault();
var dataToFormat = "Data to be formatted to all supported formats";
var formattedDataDict = await defaultFactory.FormatToAllAsync(dataToFormat);
foreach (var kvp in formattedDataDict)
{
    Console.WriteLine($"Format: {kvp.Key}, Formatted Data: {kvp.Value}");
}
```

## Notes
When using the `FormatterFactory` class, consider the following edge cases and thread-safety remarks:
* The `RegisterFormatter` method may throw if the registration fails, so it's essential to handle potential exceptions.
* The `GetFormatter` and `GetFormatterByExtension` methods return `null` if no formatter is found, so null checks are necessary to avoid `NullReferenceException`.
* The `FormatToAllAsync` method is asynchronous, and its result should be awaited to ensure correct execution.
* The `FormatterFactory` class is not inherently thread-safe, so access to its instance members should be synchronized if used in a multi-threaded environment.
* The `CreateDefault` method creates a new instance with default settings, which may not include all available formatters. Additional formatters may need to be registered manually.
