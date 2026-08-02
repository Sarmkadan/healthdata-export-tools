[MemoryDiagnoser]
public class BackgroundTaskSchedulerBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Set up realistic test data here
    }

    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Benchmark the first public method
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void BenchmarkMethod2(int inputSize)
    {
        // Benchmark the second public method with varying input size
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Benchmark the third public method
    }
}