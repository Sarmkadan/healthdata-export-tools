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

