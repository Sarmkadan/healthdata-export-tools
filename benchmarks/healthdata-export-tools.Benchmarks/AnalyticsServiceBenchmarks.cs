[MemoryDiagnoser]
public class AnalyticsServiceBenchmarks
{
    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Test data setup
        var testData = new object[] { /* test data */ };
        // Method call
        var result = AnalyticsService.Method1(testData);
        // Assert result
    }

    [Benchmark]
    public void BenchmarkMethod2([Params(10)] int inputSize)
    {
        // Test data setup
        var testData = new object[inputSize];
        // Method call
        var result = AnalyticsService.Method2(testData);
        // Assert result
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Test data setup
        var testData = new Dictionary<string, object>();
        // Method call
        var result = AnalyticsService.Method3(testData);
        // Assert result
    }
}