# ErrorHandlingMiddleware

A middleware component that intercepts unhandled exceptions during HTTP request processing, normalizes them into a consistent error response structure, and writes the result to the outgoing response. It captures diagnostic identifiers, the exception type, a sanitized message, and optional details, ensuring callers receive predictable, machine-readable error payloads regardless of the underlying failure.

## API

### `ErrorHandlingMiddleware`

The constructor. Accepts the next delegate in the request pipeline and any configuration required to control response formatting or logging behavior. Specific parameters are determined by the implementation; the constructor is not intended for direct public invocation outside of dependency injection or pipeline setup.

### `ProcessAsync`

```csharp
public async Task ProcessAsync(HttpContext context)
```

Invokes the next middleware in the pipeline within a try-catch boundary. When an unhandled exception propagates, it populates the public properties (`ErrorId`, `RequestId`, `StatusCode`, `Message`, `Details`, `ExceptionType`, `Timestamp`) and writes a structured error response to `context.Response`. If no exception occurs, the method completes without modifying the response.

- **Parameters:** `context` — the current `HttpContext` for the request.
- **Returns:** a `Task` representing the asynchronous operation.
- **Throws:** does not rethrow caught exceptions; exceptions from response writing (e.g., a closed connection) may propagate.

### `ErrorId`

```csharp
public string ErrorId { get; }
```

A unique identifier generated for each captured error, intended for correlation with server-side logs. This value is set only after an exception is caught by `ProcessAsync`.

### `RequestId`

```csharp
public string RequestId { get; }
```

The identifier of the HTTP request during which the error occurred, typically sourced from a trace or correlation header. Set only after an exception is caught.

### `StatusCode`

```csharp
public int StatusCode { get; }
```

The HTTP status code assigned to the error response. Derived from the exception type (e.g., 400 for validation errors, 500 for unhandled exceptions) or a default policy. Set only after an exception is caught.

### `Message`

```csharp
public string Message { get; }
```

A human-readable, sanitized description of the error. For production deployments, this omits sensitive internal details. Set only after an exception is caught.

### `Details`

```csharp
public string Details { get; }
```

Additional contextual information about the error, such as inner exception messages or parameter values. May be empty or suppressed in non-development environments. Set only after an exception is caught.

### `ExceptionType`

```csharp
public string? ExceptionType { get; }
```

The fully qualified name of the caught exception type, or `null` if no exception has been captured. Useful for diagnostics and client-side branching logic.

### `Timestamp`

```csharp
public DateTime Timestamp { get; }
```

The UTC time at which the exception was caught and the error response was constructed. Set only after an exception is caught.

## Usage

### Example 1: Basic pipeline registration

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapGet("/data", () =>
{
    throw new InvalidOperationException("Export failed due to missing configuration.");
});

app.Run();
```

A request to `/data` triggers the exception. The middleware catches it, sets `StatusCode` to 500, generates an `ErrorId`, and returns a JSON body containing all public property values.

### Example 2: Accessing error properties from a downstream service

```csharp
public class DiagnosticsReporter
{
    private readonly ErrorHandlingMiddleware _middleware;

    public DiagnosticsReporter(ErrorHandlingMiddleware middleware)
    {
        _middleware = middleware;
    }

    public void LogCurrentError(ILogger logger)
    {
        if (_middleware.ExceptionType is not null)
        {
            logger.LogError(
                "Error {ErrorId} for request {RequestId}: {ExceptionType} at {Timestamp}",
                _middleware.ErrorId,
                _middleware.RequestId,
                _middleware.ExceptionType,
                _middleware.Timestamp);
        }
    }
}
```

This service is resolved after the middleware processes a request. It reads the public properties to emit a structured log entry, relying on the fact that properties remain populated for the lifetime of the scoped middleware instance.

## Notes

- All public properties except `ExceptionType` are meaningful only after `ProcessAsync` catches an exception. Accessing them before an error occurs yields default or empty values.
- The middleware is typically registered as a scoped or singleton service. When scoped, property values are isolated per request; when singleton, properties reflect the most recent error across all requests and are not thread-safe for concurrent access.
- `ProcessAsync` does not rethrow. Downstream middleware or filters that depend on exception propagation will not see the original exception.
- If the response has already started (headers sent) when an exception occurs, the middleware may be unable to set the status code or write a body. In such cases, the properties are still populated, but the client may receive an incomplete or truncated response.
- `Details` may contain environment-specific information. Ensure it is cleared or restricted in production configurations to avoid information leakage.
