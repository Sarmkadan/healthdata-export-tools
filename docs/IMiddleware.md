# IMiddleware
The `IMiddleware` type is designed to provide a standardized interface for middleware components in the `healthdata-export-tools` project. It allows for the encapsulation of request-specific data and metadata, enabling flexible and modular processing pipelines. By implementing this interface, developers can create reusable and composable middleware components that can be easily integrated into larger workflows.

## API
The `IMiddleware` interface exposes the following public members:
* `RequestId`: A string identifier for the current request.
* `StartTime`: A `DateTime` object representing the start time of the request.
* `Metadata`: A `Dictionary<string, object>` containing additional metadata associated with the request.
* `Data`: An optional object containing the request data.
* `Exception`: An optional `Exception` object representing any errors that occurred during processing.
* `ContinueProcessing`: A boolean flag indicating whether processing should continue.
* `Result`: An optional object containing the result of the processing operation.

## Usage
Here are two examples of using the `IMiddleware` interface in C#:
```csharp
// Example 1: Creating a simple logging middleware
public class LoggingMiddleware : IMiddleware
{
    public string RequestId { get; set; }
    public DateTime StartTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public object? Data { get; set; }
    public Exception? Exception { get; set; }
    public bool ContinueProcessing { get; set; }
    public object? Result { get; set; }

    public void ProcessRequest()
    {
        Console.WriteLine($"Request {RequestId} started at {StartTime}");
        // Log metadata and data
        foreach (var pair in Metadata)
        {
            Console.WriteLine($"{pair.Key}: {pair.Value}");
        }
        if (Data != null)
        {
            Console.WriteLine($"Data: {Data}");
        }
    }
}

// Example 2: Using middleware in a processing pipeline
public class ProcessingPipeline
{
    private readonly List<IMiddleware> _middlewareComponents;

    public ProcessingPipeline(List<IMiddleware> middlewareComponents)
    {
        _middlewareComponents = middlewareComponents;
    }

    public void ProcessRequest(string requestId, object data)
    {
        foreach (var middleware in _middlewareComponents)
        {
            middleware.RequestId = requestId;
            middleware.Data = data;
            middleware.ContinueProcessing = true;
            // Call the middleware's processing logic
            if (!middleware.ContinueProcessing)
            {
                break;
            }
        }
    }
}
```

## Notes
When implementing the `IMiddleware` interface, consider the following edge cases and thread-safety remarks:
* The `RequestId` and `StartTime` properties should be set before calling the middleware's processing logic.
* The `Metadata` dictionary should be initialized before adding any metadata.
* The `Data` and `Result` properties are optional and may be null.
* The `Exception` property should be set if an error occurs during processing.
* The `ContinueProcessing` flag should be set to false if processing should be halted.
* Middleware components should be designed to be thread-safe, as they may be executed concurrently in a multi-threaded environment.
* When using middleware components in a processing pipeline, ensure that each component is properly initialized and configured before calling its processing logic.
