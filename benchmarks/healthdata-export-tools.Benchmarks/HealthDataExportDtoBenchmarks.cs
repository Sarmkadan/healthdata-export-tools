[Benchmark]
[Benchmark(MinTimeQuery = 100, MaxTimeQuery = 500)]
[MemoryDiagnoser]
public class HealthDataExportDtoBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Initialize test data here
    }

    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Test data preparation
        var testData = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            testData.Add("testData" + i);
        }

        // Method call with test data
        var healthDataExportDto = new HealthDataExportDto();
        healthDataExportDto.ExportData(testData);
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void BenchmarkMethod2(int inputSize)
    {
        // Test data preparation
        var testData = new List<string>();
        for (int i = 0; i < inputSize; i++)
        {
            testData.Add("testData" + i);
        }

        // Method call with test data
        var healthDataExportDto = new HealthDataExportDto();
        healthDataExportDto.ExportData(testData);
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Test data preparation
        var testData = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            testData.Add("testData" + i);
        }

        // Method call with test data
        var healthDataExportDto = new HealthDataExportDto();
        healthDataExportDto.ExportData(testData);
    }
}
