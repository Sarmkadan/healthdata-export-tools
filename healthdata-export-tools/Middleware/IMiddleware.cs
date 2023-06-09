// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Middleware;

/// <summary>
/// Defines middleware pipeline interface for processing requests
/// </summary>
public interface IMiddleware
{
    /// <summary>
    /// Process the middleware logic asynchronously
    /// </summary>
    Task ProcessAsync(MiddlewareContext context);
}

/// <summary>
/// Context object passed through the middleware pipeline
/// </summary>
public class MiddlewareContext
{
    /// <summary>
    /// Unique request identifier for correlation
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp when request was initiated
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Custom metadata dictionary for passing data through pipeline
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// User-provided data to be processed
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Exception if one occurred during processing
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Indicates if the middleware pipeline should continue
    /// </summary>
    public bool ContinueProcessing { get; set; } = true;

    /// <summary>
    /// Result of the processing operation
    /// </summary>
    public object? Result { get; set; }
}
