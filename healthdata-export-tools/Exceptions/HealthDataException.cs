// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Exceptions;

/// <summary>
/// Base exception for all health data processing errors
/// </summary>
public class HealthDataException : Exception
{
    /// <summary>
    /// Error code for categorizing the exception
    /// </summary>
    public string ErrorCode { get; set; }

    /// <summary>
    /// Additional context data about the error
    /// </summary>
    public Dictionary<string, object>? ContextData { get; set; }

    /// <summary>
    /// Initialize a new instance of HealthDataException
    /// </summary>
    public HealthDataException(string message) : base(message)
    {
        ErrorCode = "UNKNOWN_ERROR";
    }

    /// <summary>
    /// Initialize with error code
    /// </summary>
    public HealthDataException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initialize with inner exception
    /// </summary>
    public HealthDataException(string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = "UNKNOWN_ERROR";
    }

    /// <summary>
    /// Initialize with error code and context data
    /// </summary>
    public HealthDataException(string message, string errorCode, Dictionary<string, object> contextData)
        : base(message)
    {
        ErrorCode = errorCode;
        ContextData = contextData;
    }
}

/// <summary>
/// Exception thrown when parsing health data files fails
/// </summary>
public class ParsingException : HealthDataException
{
    /// <summary>
    /// Path to the file being parsed
    /// </summary>
    public string? FilePath { get; }

    /// <summary>
    /// Line number where parsing failed
    /// </summary>
    public int? LineNumber { get; }

    public ParsingException(string message) : base(message, "PARSING_ERROR") { }

    public ParsingException(string message, string filePath)
        : base(message, "PARSING_ERROR", new() { { "FilePath", filePath } })
    {
        FilePath = filePath;
    }

    public ParsingException(string message, string filePath, int lineNumber, Exception inner)
        : base(message, inner)
    {
        ErrorCode = "PARSING_ERROR";
        FilePath = filePath;
        LineNumber = lineNumber;
        ContextData = new() { { "FilePath", filePath }, { "LineNumber", lineNumber } };
    }
}

/// <summary>
/// Exception thrown when data validation fails
/// </summary>
public class ValidationException : HealthDataException
{
    /// <summary>
    /// Field or property that failed validation
    /// </summary>
    public string? FieldName { get; }

    /// <summary>
    /// Validation rule that failed
    /// </summary>
    public string? ValidationRule { get; }

    public ValidationException(string message) : base(message, "VALIDATION_ERROR") { }

    public ValidationException(string message, string fieldName)
        : base(message, "VALIDATION_ERROR", new() { { "FieldName", fieldName } })
    {
        FieldName = fieldName;
    }

    public ValidationException(string message, string fieldName, string validationRule)
        : base(message, "VALIDATION_ERROR", new()
        {
            { "FieldName", fieldName },
            { "ValidationRule", validationRule }
        })
    {
        FieldName = fieldName;
        ValidationRule = validationRule;
    }
}

/// <summary>
/// Exception thrown when data export operation fails
/// </summary>
public class ExportException : HealthDataException
{
    /// <summary>
    /// Destination path where export was attempted
    /// </summary>
    public string? DestinationPath { get; }

    /// <summary>
    /// Export format that was used
    /// </summary>
    public string? ExportFormat { get; }

    public ExportException(string message) : base(message, "EXPORT_ERROR") { }

    public ExportException(string message, string destinationPath)
        : base(message, "EXPORT_ERROR", new() { { "DestinationPath", destinationPath } })
    {
        DestinationPath = destinationPath;
    }

    public ExportException(string message, string destinationPath, string format, Exception inner)
        : base(message, inner)
    {
        ErrorCode = "EXPORT_ERROR";
        DestinationPath = destinationPath;
        ExportFormat = format;
        ContextData = new() { { "DestinationPath", destinationPath }, { "Format", format } };
    }
}

/// <summary>
/// Exception thrown when database operation fails
/// </summary>
public class DataAccessException : HealthDataException
{
    /// <summary>
    /// Operation being performed when error occurred
    /// </summary>
    public string? Operation { get; }

    public DataAccessException(string message) : base(message, "DATA_ACCESS_ERROR") { }

    public DataAccessException(string message, string operation)
        : base(message, "DATA_ACCESS_ERROR", new() { { "Operation", operation } })
    {
        Operation = operation;
    }

    public DataAccessException(string message, string operation, Exception inner)
        : base(message, inner)
    {
        ErrorCode = "DATA_ACCESS_ERROR";
        Operation = operation;
    }
}
