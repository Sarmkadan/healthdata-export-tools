// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.DTOs;

/// <summary>
/// Data Transfer Object for validation operation results
/// </summary>
public class ValidationResultDto
{
    [JsonPropertyName("validationId")]
    public string ValidationId { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }

    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; set; }

    [JsonPropertyName("validRecords")]
    public int ValidRecords { get; set; }

    [JsonPropertyName("invalidRecords")]
    public int InvalidRecords { get; set; }

    [JsonPropertyName("validationErrors")]
    public List<ValidationErrorDetail> ValidationErrors { get; set; } = new();

    [JsonPropertyName("warnings")]
    public List<ValidationWarning> Warnings { get; set; } = new();

    [JsonPropertyName("statistics")]
    public ValidationStatistics Statistics { get; set; } = new();

    [JsonPropertyName("duration")]
    public double DurationMs { get; set; }

    public double GetSuccessRate() =>
        TotalRecords > 0 ? (ValidRecords * 100.0) / TotalRecords : 100;

    public int GetErrorCount() => ValidationErrors.Count;
    public int GetWarningCount() => Warnings.Count;
}

/// <summary>
/// Individual validation error detail
/// </summary>
public class ValidationErrorDetail
{
    [JsonPropertyName("recordIndex")]
    public int RecordIndex { get; set; }

    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("severity")]
    public string Severity { get; set; } = "Error"; // Error, Critical

    public static ValidationErrorDetail Create(
        int recordIndex,
        string field,
        string message,
        string errorCode = "VALIDATION_ERROR",
        string? value = null)
    {
        return new ValidationErrorDetail
        {
            RecordIndex = recordIndex,
            Field = field,
            Value = value,
            Message = message,
            ErrorCode = errorCode
        };
    }
}

/// <summary>
/// Validation warning
/// </summary>
public class ValidationWarning
{
    [JsonPropertyName("recordIndex")]
    public int RecordIndex { get; set; }

    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("warningCode")]
    public string WarningCode { get; set; } = string.Empty;

    public static ValidationWarning Create(
        int recordIndex,
        string field,
        string message,
        string warningCode = "WARNING")
    {
        return new ValidationWarning
        {
            RecordIndex = recordIndex,
            Field = field,
            Message = message,
            WarningCode = warningCode
        };
    }
}

/// <summary>
/// Validation statistics
/// </summary>
public class ValidationStatistics
{
    [JsonPropertyName("dataTypeBreakdown")]
    public Dictionary<string, DataTypeValidationStats> DataTypeBreakdown { get; set; } = new();

    [JsonPropertyName("errorsByType")]
    public Dictionary<string, int> ErrorsByType { get; set; } = new();

    [JsonPropertyName("mostCommonErrors")]
    public List<CommonError> MostCommonErrors { get; set; } = new();

    [JsonPropertyName("affectedDevices")]
    public List<string> AffectedDevices { get; set; } = new();

    [JsonPropertyName("dateRangeValidated")]
    public DateRangeInfo DateRange { get; set; } = new();
}

/// <summary>
/// Data type validation statistics
/// </summary>
public class DataTypeValidationStats
{
    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; set; }

    [JsonPropertyName("validRecords")]
    public int ValidRecords { get; set; }

    [JsonPropertyName("invalidRecords")]
    public int InvalidRecords { get; set; }

    [JsonPropertyName("successRate")]
    public double SuccessRate => TotalRecords > 0 ? (ValidRecords * 100.0) / TotalRecords : 0;
}

/// <summary>
/// Common error information
/// </summary>
public class CommonError
{
    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("occurrences")]
    public int Occurrences { get; set; }

    [JsonPropertyName("affectedFields")]
    public List<string> AffectedFields { get; set; } = new();
}

