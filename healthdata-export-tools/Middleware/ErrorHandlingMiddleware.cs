// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Middleware;

/// <summary>
/// Middleware for centralized error handling and exception transformation
/// Converts exceptions into structured error responses
/// </summary>
public class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handle exceptions and transform them to error responses
    /// </summary>
    public async Task ProcessAsync(MiddlewareContext context)
    {
        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            context.ContinueProcessing = false;
            context.Result = HandleException(ex, context);
        }
    }

    /// <summary>
    /// Transform exception into structured error response
    /// Returns different error information based on exception type
    /// </summary>
    private object HandleException(Exception ex, MiddlewareContext context)
    {
        var errorId = Guid.NewGuid().ToString();

        _logger.LogError(ex,
            "Error {ErrorId} in request {RequestId}: {Message}",
            errorId,
            context.RequestId,
            ex.Message);

        return ex switch
        {
            HealthDataException healthEx => new ErrorResponse
            {
                ErrorId = errorId,
                RequestId = context.RequestId,
                StatusCode = 400,
                Message = "Health data processing error",
                Details = healthEx.Message,
                Timestamp = DateTime.UtcNow
            },

            ArgumentException argEx => new ErrorResponse
            {
                ErrorId = errorId,
                RequestId = context.RequestId,
                StatusCode = 400,
                Message = "Invalid argument provided",
                Details = argEx.Message,
                Timestamp = DateTime.UtcNow
            },

            FileNotFoundException fileEx => new ErrorResponse
            {
                ErrorId = errorId,
                RequestId = context.RequestId,
                StatusCode = 404,
                Message = "Required file not found",
                Details = fileEx.Message,
                Timestamp = DateTime.UtcNow
            },

            InvalidOperationException opEx => new ErrorResponse
            {
                ErrorId = errorId,
                RequestId = context.RequestId,
                StatusCode = 409,
                Message = "Invalid operation state",
                Details = opEx.Message,
                Timestamp = DateTime.UtcNow
            },

            // Default handling for any other exception
            _ => new ErrorResponse
            {
                ErrorId = errorId,
                RequestId = context.RequestId,
                StatusCode = 500,
                Message = "An unexpected error occurred",
                Details = ex.Message,
                ExceptionType = ex.GetType().Name,
                Timestamp = DateTime.UtcNow
            }
        };
    }
}

/// <summary>
/// Structured error response object
/// </summary>
public class ErrorResponse
{
    [JsonPropertyName("errorId")]
    public string ErrorId { get; set; } = string.Empty;

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = string.Empty;

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    public string Details { get; set; } = string.Empty;

    [JsonPropertyName("exceptionType")]
    public string? ExceptionType { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}
