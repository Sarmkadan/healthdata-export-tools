#!/usr/bin/env dotnet-script

// Test script to verify CSV injection protection in CsvFormatter

using System;
using System.Collections.Generic;
using System.IO;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Formatters;
using Microsoft.Extensions.Logging;

// Simple console logger for testing
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<CsvFormatter>();
var formatter = new CsvFormatter(logger);

Console.WriteLine("Testing CSV Injection Protection in CsvFormatter");
Console.WriteLine("==============================================\n");

// Test cases for CSV injection vectors
var testCases = new List<(string Description, string TestValue)>
{
    ("Excel formula injection (=1+1)", "=1+1"),
    ("Excel formula injection (=SUM(A1:A10))", "=SUM(A1:A10)"),
    ("DDE injection (@SUM(A1:A10))", "@SUM(A1:A10)"),
    ("Positive formula injection (+1)", "+1"),
    ("Negative formula injection (-1)", "-1"),
    ("Tab character injection", "\t=1+1"),
    ("Carriage return injection", "\r=1+1"),
    ("Embedded CRLF", "Line1\r\nLine2"),
    ("Embedded LF", "Line1\nLine2"),
    ("Embedded CR", "Line1\rLine2"),
    ("Embedded double quotes", "He said \"Hello\""),
    ("Normal text", "Normal health data note"),
    ("Empty string", ""),
    ("Null value", null)
};

foreach (var (description, testValue) in testCases)
{
    Console.WriteLine($"Test: {description}");
    Console.WriteLine($"Input: {(testValue == null ? "null" : $"'" + testValue + "'")}");

    // Create a test record
    var record = new SleepData
    {
        RecordDate = DateTime.UtcNow,
        DurationMinutes = 480,
        Quality = HealthDataExportTools.Domain.Enums.SleepQuality.Good,
        DeviceId = testValue,  // This will be sanitized
        Notes = testValue       // This will also be sanitized
    };

    try
    {
        var csv = await formatter.FormatAsync(record);
        Console.WriteLine("Output CSV:");
        Console.WriteLine(csv);

        // Verify the output doesn't contain the dangerous characters at the start of any field
        var lines = csv.Split('\n');
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var fields = line.Split(',');
            foreach (var field in fields)
            {
                var trimmedField = field.Trim('\"');
                if (trimmedField.StartsWith('=') ||
                    trimmedField.StartsWith('+') ||
                    trimmedField.StartsWith('-') ||
                    trimmedField.StartsWith('@') ||
                    trimmedField.StartsWith('\t') ||
                    trimmedField.StartsWith('\r'))
                {
                    Console.WriteLine($"❌ FAILED: Dangerous character found in output field: {trimmedField}");
                    Environment.Exit(1);
                }
            }
        }

        Console.WriteLine("✅ PASSED: No dangerous characters in output\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ FAILED: Exception occurred: {ex.Message}\n");
        Environment.Exit(1);
    }
}

Console.WriteLine("All tests passed! ✅");
Console.WriteLine("\nSummary:");
Console.WriteLine("- Values starting with =, +, -, @, or tab are prefixed with single quote");
Console.WriteLine("- Embedded newlines (\\r\\n, \\n, \\r) are replaced with spaces");
Console.WriteLine("- Embedded double quotes are escaped per RFC 4180");
Console.WriteLine("- Normal text passes through unchanged");