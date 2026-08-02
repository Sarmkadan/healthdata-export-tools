using BenchmarkDotNet.Attributes;
using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Benchmarks;

[MemoryDiagnoser]
public class HealthMetricBenchmarks
{
    [Params(10, 100, 1000)]
    public int Size;

    private List<HealthMetric> _metrics = null!;

    [GlobalSetup]
    public void Setup()
    {
        _metrics = new List<HealthMetric>(Size);
        for (int i = 0; i < Size; i++)
        {
            _metrics.Add(new HealthMetric
            {
                MetricName = "HeartRate",
                Unit = "BPM",
                Value = 70 + (i % 20),
                NormalRangeLow = 60,
                NormalRangeHigh = 100,
                DataSources = new List<string> { "Watch", "Phone" }
            });
        }
    }

    [Benchmark]
    public void IsValid()
    {
        foreach (var metric in _metrics)
        {
            _ = metric.IsValid();
        }
    }

    [Benchmark]
    public void GetSummary()
    {
        foreach (var metric in _metrics)
        {
            _ = metric.GetSummary();
        }
    }

    [Benchmark]
    public void AssessHealthStatus()
    {
        foreach (var metric in _metrics)
        {
            metric.AssessHealthStatus();
        }
    }

    [Benchmark]
    public void UpdateValue()
    {
        foreach (var metric in _metrics)
        {
            metric.UpdateValue(80 + (metric.Value % 10));
        }
    }
}
