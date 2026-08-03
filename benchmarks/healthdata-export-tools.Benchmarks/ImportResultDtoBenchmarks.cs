[Benchmark]
[Benchmark(MinTimeQuery = 100, MaxTimeQuery = 500)]
[MemoryDiagnoser]
public class ImportResultDtoBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // setup test data
    }

    [Benchmark]
    public void Benchmark_Method1()
    {
        // benchmark method 1
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void Benchmark_Method2()
    {
        // benchmark method 2 with input size
    }

    [Benchmark]
    public void Benchmark_Method3()
    {
        // benchmark method 3
    }
}
