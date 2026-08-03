[MemoryDiagnoser]
public class InMemoryCacheProviderBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Set up realistic test data here
    }

    [Benchmark]
    public void Benchmark_Method1()
    {
        // Benchmark the first public method
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void Benchmark_Method2(int inputSize)
    {
        // Benchmark the second public method with varying input size
    }

    [Benchmark]
    public void Benchmark_Method3()
    {
        // Benchmark the third public method
    }
}