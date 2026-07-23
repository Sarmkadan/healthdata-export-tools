using System;
using HealthDataExportTools.Cli;

class Program
{
    static void Main()
    {
        Console.WriteLine("Testing CliArgumentParser improvements...\n");

        var parser = new CliArgumentParser();

        // Test 1: Unknown flag with suggestion
        Console.WriteLine("Test 1: Unknown flag with suggestion");
        var result1 = parser.Parse(new[] { "--unknwon", "test" });
        Console.WriteLine($"Success: {result1.Success}");
        Console.WriteLine($"Errors: {string.Join(", ", result1.Errors)}");
        Console.WriteLine();

        // Test 2: Invalid date format
        Console.WriteLine("Test 2: Invalid date format");
        var result2 = parser.Parse(new[] { "--start-date", "invalid" });
        Console.WriteLine($"Success: {result2.Success}");
        Console.WriteLine($"Errors: {string.Join(", ", result2.Errors)}");
        Console.WriteLine();

        // Test 3: Start date after end date
        Console.WriteLine("Test 3: Start date after end date");
        var result3 = parser.Parse(new[] { "--start-date", "2025-01-20", "--end-date", "2025-01-15" });
        Console.WriteLine($"Success: {result3.Success}");
        Console.WriteLine($"Errors: {string.Join(", ", result3.Errors)}");
        Console.WriteLine();

        // Test 4: Invalid format
        Console.WriteLine("Test 4: Invalid format");
        var result4 = parser.Parse(new[] { "--format", "xml" });
        Console.WriteLine($"Success: {result4.Success}");
        Console.WriteLine($"Errors: {string.Join(", ", result4.Errors)}");
        Console.WriteLine();

        // Test 5: Valid arguments
        Console.WriteLine("Test 5: Valid arguments");
        var result5 = parser.Parse(new[] { "--input", "./test", "--format", "csv", "--start-date", "2025-01-15" });
        Console.WriteLine($"Success: {result5.Success}");
        if (result5.Success)
        {
            Console.WriteLine($"Input: {result5.Options.InputPath}");
            Console.WriteLine($"Format: {result5.Options.Format}");
            Console.WriteLine($"Start Date: {result5.Options.StartDate}");
        }
        Console.WriteLine();

        // Test 6: Missing required value
        Console.WriteLine("Test 6: Missing required value");
        var result6 = parser.Parse(new[] { "--input" });
        Console.WriteLine($"Success: {result6.Success}");
        Console.WriteLine($"Errors: {string.Join(", ", result6.Errors)}");
        Console.WriteLine();

        // Test 7: TryParse extension method
        Console.WriteLine("Test 7: TryParse extension method");
        bool success = parser.TryParse(new[] { "--input", "./test" }, out var options);
        Console.WriteLine($"Success: {success}");
        Console.WriteLine($"Options: {(options != null ? "Not null" : "Null")}");
        Console.WriteLine();

        // Test 8: ParseWithValidation extension method
        Console.WriteLine("Test 8: ParseWithValidation extension method");
        var result8 = parser.ParseWithValidation(new[] { "--input", "./test", "--format", "jsonl" });
        Console.WriteLine($"Success: {result8.Success}");
        if (result8.Success)
        {
            Console.WriteLine("ParseWithValidation works correctly!");
        }

        Console.WriteLine("\nAll tests completed!");
    }
}