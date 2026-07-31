using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using HealthDataExportTools.Events;
using System;
using System.Collections.Generic;
using System.Linq;

[MemoryDiagnoser]
public class ExportCompletedEventBenchmarks
{
    private List<string> _warnings;
    private List<string> _generatedFiles;
    private ExportCompletedEvent _exportCompletedEvent;

    [GlobalSetup]
    public void Setup()
    {
        _warnings = Enumerable.Range(0, 100).Select(x => $"Warning {x}").ToList();
        _generatedFiles = Enumerable.Range(0, 100).Select(x => $"File {x}").ToList();
        _exportCompletedEvent = new ExportCompletedEvent(
            "ExportId",
            ExportFormat.Csv,
            100,
            "OutputPath",
            1024,
            DateTime.Now,
            DateTime.Now.AddSeconds(1),
            true,
            _generatedFiles,
            _warnings);
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void GetExportDuration(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            _exportCompletedEvent.GetExportDuration();
        }
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void GetThroughput(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            _exportCompletedEvent.GetThroughput();
        }
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void GetHumanReadableSize(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            _exportCompletedEvent.GetHumanReadableSize();
        }
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void HasWarnings(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            _exportCompletedEvent.HasWarnings;
        }
    }
}
