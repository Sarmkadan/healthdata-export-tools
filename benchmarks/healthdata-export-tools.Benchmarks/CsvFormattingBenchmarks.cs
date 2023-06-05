// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Formatters;
using Microsoft.Extensions.Logging.Abstractions;

namespace HealthDataExportTools.Benchmarks;

/// <summary>
/// Benchmarks for CSV formatting — simulates a typical 30-day export
/// for each health metric type.
/// </summary>
[MemoryDiagnoser]
public class CsvFormattingBenchmarks
{
    private CsvFormatter _formatter = null!;
    private List<SleepData> _sleepRecords = null!;
    private List<HeartRateData> _heartRateRecords = null!;
    private List<StepsData> _stepsRecords = null!;

    [GlobalSetup]
    public void Setup()
    {
        _formatter = new CsvFormatter(NullLogger<CsvFormatter>.Instance);

        _sleepRecords = Enumerable.Range(0, 30).Select(i =>
        {
            var date = DateTime.Today.AddDays(-i);
            return new SleepData
            {
                RecordDate = date,
                DeviceId = "zepp-smartwatch-001",
                SleepStart = date.AddDays(-1).AddHours(22),
                SleepEnd = date.AddHours(6),
                DurationMinutes = 480 + (i % 60) - 30,
                DeepSleepMinutes = 90,
                LightSleepMinutes = 240,
                RemSleepMinutes = 120,
                AwakeMinutes = 30,
                Quality = i % 4 == 0 ? SleepQuality.Excellent : SleepQuality.Good,
                Score = 78 + (i % 20)
            };
        }).ToList();

        _heartRateRecords = Enumerable.Range(0, 30).Select(i => new HeartRateData
        {
            RecordDate = DateTime.Today.AddDays(-i),
            DeviceId = "zepp-smartwatch-001",
            MinimumBpm = 50 + (i % 8),
            MaximumBpm = 138 + (i % 15),
            AverageBpm = 68 + (i % 12),
            RestingBpm = 54 + (i % 6),
            MeasurementCount = 1440,
            StressLevel = 22 + (i % 30)
        }).ToList();

        _stepsRecords = Enumerable.Range(0, 30).Select(i => new StepsData
        {
            RecordDate = DateTime.Today.AddDays(-i),
            DeviceId = "zepp-smartwatch-001",
            TotalSteps = 8500 + (i * 50),
            DistanceKm = 6.8 + (i * 0.04),
            CaloriesBurned = 390 + (i % 50),
            DailyGoal = 10000,
            ActiveMinutes = 55 + (i % 30)
        }).ToList();
    }

    [Benchmark(Baseline = true)]
    public Task<string> FormatSleepCsv() => _formatter.FormatSleepDataAsync(_sleepRecords);

    [Benchmark]
    public Task<string> FormatHeartRateCsv() => _formatter.FormatHeartRateDataAsync(_heartRateRecords);

    [Benchmark]
    public Task<string> FormatStepsCsv() => _formatter.FormatStepsDataAsync(_stepsRecords);
}
