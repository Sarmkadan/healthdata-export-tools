[MemoryDiagnoser]
public class DataComparisonServiceBenchmarks
{
    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Setup test data
        var testData = new[] { /* test data */ };
        // Benchmark code
    }

    [Benchmark]
    public void BenchmarkMethod2([Params(10)] int inputSize)
    {
        // Setup test data
        var testData = new[] { /* test data */ };
        // Benchmark code
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Setup test data
        var testData = new[] { /* test data */ };
        // Benchmark code
    }
}
