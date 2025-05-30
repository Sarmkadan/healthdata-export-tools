// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Middleware;

/// <summary>
/// Middleware for structured logging of request/response cycles
/// Tracks timing, request data, and execution metrics
/// </summary>
public class LoggingMiddleware : IMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;
    private readonly Stopwatch _stopwatch;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stopwatch = new Stopwatch();
    }

    /// <summary>
    /// Log request start, delegate to next middleware, log completion
    /// </summary>
    public async Task ProcessAsync(MiddlewareContext context)
    {
        _stopwatch.Restart();

        try
        {
            LogRequestStart(context);

            // Continue to next middleware (simulated here)
            await Task.CompletedTask;

            LogRequestComplete(context);
        }
        catch (Exception ex)
        {
            LogRequestError(context, ex);
            throw;
        }
        finally
        {
            _stopwatch.Stop();
        }
    }

    /// <summary>
    /// Log details at the start of request processing
    /// </summary>
    private void LogRequestStart(MiddlewareContext context)
    {
        var logData = new
        {
            context.RequestId,
            Timestamp = context.StartTime,
            DataType = context.Data?.GetType().Name,
            MetadataCount = context.Metadata.Count
        };

        _logger.LogInformation(
            "Request started: {RequestId} at {Timestamp}. Data type: {DataType}, Metadata items: {Count}",
            context.RequestId,
            context.StartTime,
            logData.DataType,
            logData.MetadataCount);
    }

    /// <summary>
    /// Log details upon successful completion
    /// </summary>
    private void LogRequestComplete(MiddlewareContext context)
    {
        var duration = _stopwatch.ElapsedMilliseconds;
        var resultType = context.Result?.GetType().Name ?? "null";

        _logger.LogInformation(
            "Request completed: {RequestId} in {DurationMs}ms. Result type: {ResultType}",
            context.RequestId,
            duration,
            resultType);

        // Log slow requests as warnings
        if (duration > 5000)
        {
            _logger.LogWarning(
                "Slow request detected: {RequestId} took {DurationMs}ms",
                context.RequestId,
                duration);
        }
    }

    /// <summary>
    /// Log details when an error occurs
    /// </summary>
    private void LogRequestError(MiddlewareContext context, Exception ex)
    {
        _logger.LogError(ex,
            "Request failed: {RequestId}. Exception: {ExceptionType}: {Message}",
            context.RequestId,
            ex.GetType().Name,
            ex.Message);

        // Store exception in context for error handling middleware
        context.Exception = ex;
    }
}
