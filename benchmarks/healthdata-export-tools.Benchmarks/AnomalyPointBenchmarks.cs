[Benchmark]
[MemoryDiagnoser]
public class AnomalyPointBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // setup test data
    }

    [Benchmark]
    public void BenchmarkMethod1()
    {
        // benchmark code for BenchmarkMethod1
    }

    [Benchmark]
    [Params(10)]
    public void BenchmarkMethod2(int inputSize)
    {
        // benchmark code for BenchmarkMethod2
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // benchmark code for BenchmarkMethod3
    }
}