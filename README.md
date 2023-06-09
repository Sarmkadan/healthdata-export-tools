# Health Data Export Tools

A comprehensive .NET library for parsing, analyzing, and exporting health data from Zepp, Amazfit, and Garmin wearables. Extract sleep patterns, heart rate metrics, SpO2 levels, and step counts to multiple formats including CSV, JSON, and SQLite.

## Features

- **Multi-Format Support**: Parse data from Zepp, Amazfit, and Garmin health exports
- **Flexible Export**: Export processed data to CSV, JSON, or SQLite databases
- **Analytics Engine**: Calculate health metrics, trends, and insights
- **Data Validation**: Comprehensive validation rules for health data integrity
- **Async Operations**: Full async/await support for performance and scalability
- **Type-Safe**: Strongly typed domain models with automatic serialization
- **Dependency Injection**: Built-in DI support for testability and maintainability

## Supported Data Types

- Sleep data (duration, quality, REM/deep sleep phases)
- Heart rate measurements (minimum, maximum, average, resting)
- SpO2 (blood oxygen saturation) levels
- Step counts and activity tracking
- Activity summaries and performance metrics

## Installation

```bash
dotnet add package healthdata-export-tools
```

Or clone and build:

```bash
git clone https://github.com/vladyslav/healthdata-export-tools.git
cd healthdata-export-tools
dotnet build
```

## Quick Start

```csharp
using HealthDataExportTools;

var options = new HealthDataExportOptions 
{ 
    InputPath = "exports/", 
    OutputFormat = ExportFormat.Json 
};

var parser = new HealthDataParserService();
var exporter = new ExportService();

var data = await parser.ParseHealthDataAsync("export.zip");
await exporter.ExportAsync(data, options);
```

## Requirements

- .NET 10.0 or later
- C# 14 or later

## Project Structure

- **Domain Models**: Type-safe representations of health metrics
- **Services**: Business logic for parsing, validation, and analytics
- **Data Access**: Repository pattern with SQLite and in-memory support
- **Configuration**: Dependency injection and options setup
- **Utilities**: Extensions, validators, and helpers

## License

MIT License - See LICENSE file for details

## Author

Vladyslav Zaiets  
https://sarmkadan.com

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
