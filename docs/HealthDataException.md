# HealthDataException

A base exception type used within the `healthdata-export-tools` project to represent errors encountered during health data processing, parsing, validation, or export operations. It extends standard exception handling with structured error codes and contextual data for diagnostics and error recovery.

## API

### `public string ErrorCode`
A machine-readable identifier for the error type. Used to categorize exceptions programmatically. Set via constructor and immutable after creation.

### `public Dictionary<string, object>? ContextData`
Optional dictionary of additional diagnostic information related to the error. May be `null` if no context is provided. Accessed via properties or set during construction.

### `public HealthDataException(string message) : base(message)`
Constructs a `HealthDataException` with a human-readable error message. The `ErrorCode` defaults to `null`. Intended for generic health data processing failures.

### `public HealthDataException(string message, string errorCode) : base(message)`
Constructs a `HealthDataException` with a message and a specific error code. Useful when the error type must be programmatically identifiable.

### `public string? FilePath`
Optional file path associated with the error. Typically set when parsing or exporting a specific file. May be `null` if not applicable.

### `public int? LineNumber`
Optional line number within a file where the error occurred. Relevant for parsing or validation errors tied to file content. May be `null` if unknown or irrelevant.

### `public ParsingException(string message) : base(message, "PARSING_ERROR")`
Constructs a `ParsingException` with a message. Sets the `ErrorCode` to `"PARSING_ERROR"` to indicate a parsing-specific failure.

### `public ParsingException(string message, Dictionary<string, object>? contextData)`
Constructs a `ParsingException` with a message and additional context data. Sets the `ErrorCode` to `"PARSING_ERROR"`.

### `public ParsingException(string message, string filePath, int? lineNumber)`
Constructs a `ParsingException` with a message, file path, and line number. Sets the `ErrorCode` to `"PARSING_ERROR"`.

### `public string? FieldName`
Optional name of the field in the data that caused the validation error. Relevant only for validation exceptions. May be `null` if not applicable.

### `public string? ValidationRule`
Optional description of the validation rule that was violated. Relevant only for validation exceptions. May be `null` if not provided.

### `public ValidationException(string message) : base(message, "VALIDATION_ERROR")`
Constructs a `ValidationException` with a message. Sets the `ErrorCode` to `"VALIDATION_ERROR"` to indicate a validation-specific failure.

### `public ValidationException(string message, Dictionary<string, object>? contextData)`
Constructs a `ValidationException` with a message and additional context data. Sets the `ErrorCode` to `"VALIDATION_ERROR"`.

### `public ValidationException(string message, string fieldName, string? validationRule)`
Constructs a `ValidationException` with a message, field name, and optional validation rule description. Sets the `ErrorCode` to `"VALIDATION_ERROR"`.

### `public string? DestinationPath`
Optional destination path where the export failed. Relevant only for export exceptions. May be `null` if not applicable.

### `public string? ExportFormat`
Optional format of the export operation that failed. Relevant only for export exceptions. May be `null` if not provided.

### `public ExportException(string message) : base(message, "EXPORT_ERROR")`
Constructs an `ExportException` with a message. Sets the `ErrorCode` to `"EXPORT_ERROR"` to indicate an export-specific failure.

### `public ExportException(string message, Dictionary<string, object>? contextData)`
Constructs an `ExportException` with a message and additional context data. Sets the `ErrorCode` to `"EXPORT_ERROR"`.

## Usage
