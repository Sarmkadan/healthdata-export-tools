// existing content ...

## ImportResultDto

The `ImportResultDto` class represents the result of an import operation, providing detailed information about the imported data, including the number of records imported, validated, and rejected, as well as any validation errors or warnings encountered during the process.

### Usage Example

```csharp
using HealthDataExportTools.DTOs;

// Create a new ImportResultDto instance
var importResult = new ImportResultDto
{
    ImportId = Guid.NewGuid().ToString(),
    Status = "Success",
    RecordsImported = 100,
    RecordsValidated = 90,
    RecordsRejected = 10,
    ImportSource = "File",
    DeviceType = "Smartwatch",
    DataTypes = new List<string> { "HeartRate", "Steps" },
    StartTime = DateTime.Now.AddHours(-1),
    EndTime = DateTime.Now,
    ValidationErrors = new List<ValidationError>
    {
        new ValidationError { RecordIndex = 5, ErrorType = "InvalidFormat", Message = "Invalid data format" },
        new ValidationError { RecordIndex = 10, ErrorType = "InvalidValue", Message = "Invalid value" }
    },
    Warnings = new List<string> { "Warning: Invalid data" },
    DateRange = new DateRangeInfo { StartDate = DateTime.Now.AddDays(-1), EndDate = DateTime.Now },
    Statistics = new ImportStatistics { SleepRecords = 50, HeartRateRecords = 100 }
};

// Get the success rate of the import operation
double successRate = importResult.GetSuccessRate();
Console.WriteLine($"Success rate: {successRate:P}");

// Check if there are any validation errors
bool hasErrors = importResult.HasErrors;
Console.WriteLine($"Has errors: {hasErrors}");

// Get the first validation error
ValidationError firstError = importResult.ValidationErrors.FirstOrDefault();
Console.WriteLine($"First error: {firstError?.Message}");
```

## ExportResultDto

The `ExportResultDto` class represents the result of an export operation, providing detailed information about exported data including the number of records exported and filtered, output file details, generated files, device types, data types, and any warnings or errors encountered during the export process.

### Usage Example

```csharp
using HealthDataExportTools.DTOs;
using System;
using System.Collections.Generic;

// Create a new ExportResultDto instance
var exportResult = new ExportResultDto
{
    ExportId = Guid.NewGuid().ToString(),
    Status = "Success",
    RecordsExported = 500,
    RecordsFiltered = 480,
    OutputPath = @"/exports/healthdata-2026-07-16.json",
    OutputSizeBytes = 1048576, // 1MB
    GeneratedFiles = new List<ExportedFile>
    {
        new ExportedFile
        {
            FileName = "healthdata-2026-07-16.json",
            FilePath = @"/exports/healthdata-2026-07-16.json",
            Format = "JSON",
            RecordCount = 500,
            FileSizeBytes = 1048576,
            CreatedAt = DateTime.Now
        },
        new ExportedFile
        {
            FileName = "healthdata-2026-07-16.csv",
            FilePath = @"/exports/healthdata-2026-07-16.csv",
            Format = "CSV",
            RecordCount = 480,
            FileSizeBytes = 819200, // 800KB
            CreatedAt = DateTime.Now
        }
    },
    ExportedFormats = new List<string> { "JSON", "CSV" },
    StartTime = DateTime.Now.AddMinutes(-30),
    EndTime = DateTime.Now,
    DeviceTypes = new List<string> { "Smartwatch", "FitnessBand", "BloodPressureMonitor" },
    DataTypes = new List<string> { "HeartRate", "Steps", "BloodPressure", "SleepData" },
    Warnings = new List<string> { "Warning: Some data points have missing timestamps" },
    Errors = new List<string>(),
    IsCompressed = true,
    CompressionRatio = 0.75
};

// Get human-readable size of the output
string readableSize = exportResult.GetHumanReadableSize();
Console.WriteLine($"Output size: {readableSize}");

// Check if there are any errors
bool hasErrors = exportResult.HasErrors;
Console.WriteLine($"Has errors: {hasErrors}");

// Check if there are any warnings
bool hasWarnings = exportResult.HasWarnings;
Console.WriteLine($"Has warnings: {hasWarnings}");

// Calculate export duration
double durationSeconds = exportResult.DurationSeconds;
Console.WriteLine($"Export took {durationSeconds:F2} seconds");
```

