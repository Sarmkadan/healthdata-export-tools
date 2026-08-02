[MemoryDiagnoser]
public class ActivityDataBenchmarks
{
    [Benchmark]
    public void Benchmark_Method1()
    {
        // Setup and benchmark code here
    }
    [Benchmark]
    public void Benchmark_Method2([Params(10)] int inputSize)
    {
        // Setup and benchmark code here
    }
    [Benchmark]
    public void Benchmark_Method3()
    {
        // Setup and benchmark code here
    }
}