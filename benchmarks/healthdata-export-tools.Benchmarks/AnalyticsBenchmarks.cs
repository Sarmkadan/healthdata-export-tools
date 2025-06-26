// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Services;

namespace HealthDataExportTools.Benchmarks;

/// <summary>
/// Benchmarks for the analytics engine — health score, sleep quality report,
/// and trend analysis over a realistic 30-day dataset.
/// </summary>
[MemoryDiagnoser]
public class AnalyticsBenchmarks
{
    private AnalyticsService _analytics = null!;
    private HealthDataCollection _collection = null!;
    private List<SleepData> _sleepRecords = null!;
    private List<int> _heartRateSeries = null!;

    [GlobalSetup]
    public void Setup()
    {
        _analytics = new AnalyticsService();

        var baseDate = DateTime.UtcNow.AddDays(-1);

        _sleepRecords = Enumerable.Range(0, 30).Select(i => new SleepData
        {
            RecordDate = baseDate.AddDays(-i),
            DeviceId = "zepp-smartwatch-001",
            SleepStart = baseDate.AddDays(-i - 1).AddHours(22),
            SleepEnd = baseDate.AddDays(-i).AddHours(6),
            DurationMinutes = 460 + (i % 60),
            DeepSleepMinutes = 85 + (i % 20),
            LightSleepMinutes = 235 + (i % 30),
            RemSleepMinutes = 110 + (i % 25),
            AwakeMinutes = 30,
            Quality = i % 4 == 0 ? SleepQuality.Excellent : SleepQuality.Good,
            Score = 76 + (i % 22)
        }).ToList();

        _collection = new HealthDataCollection
        {
            SleepRecords = _sleepRecords,
            HeartRateRecords = Enumerable.Range(0, 30).Select(i => new HeartRateData
            {
                RecordDate = baseDate.AddDays(-i),
                DeviceId = "zepp-smartwatch-001",
                MinimumBpm = 51 + (i % 5),
                MaximumBpm = 138 + (i % 12),
                AverageBpm = 67 + (i % 14),
                RestingBpm = 54,
                MeasurementCount = 1440
            }).ToList(),
            SpO2Records = Enumerable.Range(0, 30).Select(i => new SpO2Data
            {
                RecordDate = baseDate.AddDays(-i),
                DeviceId = "zepp-smartwatch-001",
                MinimumPercentage = 94,
                MaximumPercentage = 99,
                AveragePercentage = 96 + (i % 3),
                MeasurementCount = 8,
                LowSpO2Events = 0
            }).ToList(),
            StepsRecords = Enumerable.Range(0, 30).Select(i => new StepsData
            {
                RecordDate = baseDate.AddDays(-i),
                DeviceId = "zepp-smartwatch-001",
                TotalSteps = 9000 + (i * 80),
                DistanceKm = 7.1 + (i * 0.07),
                CaloriesBurned = 395 + (i % 40),
                DailyGoal = 10000,
                ActiveMinutes = 58 + (i % 25)
            }).ToList()
        };

        _heartRateSeries = Enumerable.Range(0, 30).Select(i => 68 + (i % 12)).ToList();
    }

    [Benchmark(Baseline = true)]
    public int CalculateHealthScore() => _analytics.CalculateHealthScore(_collection, 7);

    [Benchmark]
    public SleepQualityReport AnalyzeSleepQuality() => _analytics.AnalyzeSleepQuality(_sleepRecords, 30);

    [Benchmark]
    public TrendAnalysis AnalyzeHeartRateTrend() => _analytics.AnalyzeTrend(_heartRateSeries, 14);
}
