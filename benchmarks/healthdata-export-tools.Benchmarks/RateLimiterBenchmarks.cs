[MemoryDiagnoser]
public class RateLimiterBenchmarks
{
    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Test data setup in [GlobalSetup]
        // [Params] for input size where relevant
    }

    [Benchmark]
    public void BenchmarkMethod2([Params(10)])
    {
        // Test data setup in [GlobalSetup]
    }

    [Benchmark]
    public void BenchmarkMethod3([Params(100)])
    {
        // Test data setup in [GlobalSetup]
    }
}