[Benchmark]
public class RetryHandlerBenchmarks
{
    [MemoryDiagnoser]
    public RetryHandlerBenchmarks()
    {
    }

    [Benchmark]
    public void Benchmark_RetryHandler_WithValidRetryPolicy()
    {
        // setup test data
        var retryHandler = new RetryHandler();
        var validRetryPolicy = new ValidRetryPolicy();
        retryHandler.RetryPolicy = validRetryPolicy;

        // benchmark
        for (int i = 0; i < 100; i++)
        {
            retryHandler.Handle();
        }
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void Benchmark_RetryHandler_WithExponentialBackoffRetryPolicy()
    {
        // setup test data
        var retryHandler = new RetryHandler();
        var exponentialBackoffRetryPolicy = new ExponentialBackoffRetryPolicy();
        retryHandler.RetryPolicy = exponentialBackoffRetryPolicy;

        // benchmark
        for (int i = 0; i < 100; i++)
        {
            retryHandler.Handle();
        }
    }

    [Benchmark]
    public void Benchmark_RetryHandler_WithNoRetryPolicy()
    {
        // setup test data
        var retryHandler = new RetryHandler();
        retryHandler.RetryPolicy = null;

        // benchmark
        for (int i = 0; i < 100; i++)
        {
            retryHandler.Handle();
        }
    }
}
