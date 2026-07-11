# CsvUtility
The `CsvUtility` class provides a set of methods for working with CSV files, including parsing, validation, and conversion to other formats. It offers a range of functionalities, from simple tasks like counting rows or escaping fields to more complex operations like merging files or generating statistics. This utility class is designed to simplify the process of handling CSV data in C# applications.

## API
### ParseCsvFileAsync
Parses a CSV file asynchronously and returns a list of dictionaries, where each dictionary represents a row in the file. The method takes no parameters and returns a `Task` that resolves to a `List<Dictionary<string, string>>`. It may throw exceptions if the file is not found, cannot be read, or if there is an error parsing the CSV data.

### ParseCsvLine
Parses a single CSV line and returns a list of strings, where each string represents a field in the line. The method takes a string parameter representing the CSV line and returns a `List<string>`. It does not throw exceptions.

### EscapeCsvField
Escapes a CSV field to ensure it can be safely included in a CSV file. The method takes a string parameter representing the field to escape and returns the escaped field as a string. It does not throw exceptions.

### ValidateCsvStructure
Validates the structure of a CSV file and returns a list of error messages if the structure is invalid. The method takes no parameters and returns a `List<string>`. It may throw exceptions if the file is not found or cannot be read.

### CountRowsAsync
Counts the number of rows in a CSV file asynchronously and returns the count as an integer. The method takes no parameters and returns a `Task` that resolves to an `int`. It may throw exceptions if the file is not found or cannot be read.

### MergeCsvFilesAsync
Merges multiple CSV files into a single file asynchronously and returns the path to the merged file as a string. The method takes no parameters and returns a `Task` that resolves to a `string`. It may throw exceptions if the files are not found, cannot be read, or if there is an error merging the files.

### CsvToJsonAsync
Converts a CSV file to JSON format asynchronously and returns the JSON data as a string. The method takes no parameters and returns a `Task` that resolves to a `string`. It may throw exceptions if the file is not found, cannot be read, or if there is an error converting the data.

### GetCsvStatisticsAsync
Generates statistics about a CSV file asynchronously and returns a `CsvStatistics` object containing the statistics. The method takes no parameters and returns a `Task` that resolves to a `CsvStatistics`. It may throw exceptions if the file is not found or cannot be read.

### RowCount
Gets the number of rows in the CSV file. This property returns an `int` and does not throw exceptions.

### HeaderCount
Gets the number of headers in the CSV file. This property returns an `int` and does not throw exceptions.

### FileSizeBytes
Gets the size of the CSV file in bytes. This property returns a `long` and does not throw exceptions.

### LastModified
Gets the date and time the CSV file was last modified. This property returns a `DateTime` and does not throw exceptions.

## Usage
The following examples demonstrate how to use the `CsvUtility` class:
```csharp
// Example 1: Parse a CSV file and print the contents
var csvData = await CsvUtility.ParseCsvFileAsync();
foreach (var row in csvData)
{
    foreach (var field in row)
    {
        Console.Write(field.Value + " ");
    }
    Console.WriteLine();
}

// Example 2: Validate the structure of a CSV file and print any error messages
var errors = CsvUtility.ValidateCsvStructure();
if (errors.Count > 0)
{
    Console.WriteLine("Errors found in CSV structure:");
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}
```

## Notes
The `CsvUtility` class is designed to be used in a variety of scenarios, from simple data import/export tasks to more complex data processing pipelines. However, there are some edge cases to consider:
* The `ParseCsvFileAsync` and `CountRowsAsync` methods may throw exceptions if the CSV file is very large, as they require reading the entire file into memory.
* The `MergeCsvFilesAsync` method may throw exceptions if the files being merged have different structures or if there are errors reading or writing the files.
* The `CsvToJsonAsync` method may throw exceptions if the CSV file contains data that cannot be represented in JSON format.
* The `GetCsvStatisticsAsync` method may throw exceptions if the CSV file is empty or if there are errors reading the file.
* The `CsvUtility` class is not thread-safe, as it uses static methods and properties to store state. If you need to use the class in a multi-threaded environment, you should create a new instance of the class for each thread or use synchronization mechanisms to protect access to the class's state.
