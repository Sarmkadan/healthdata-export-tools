// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using BenchmarkDotNet.Attributes;
using HealthDataExportTools.Services;

namespace HealthDataExportTools.Benchmarks;

/// <summary>
/// Benchmarks for JSON health data parsing — covers small and large payloads
/// across all four metric types (sleep, heart rate, SpO2, steps).
/// </summary>
[MemoryDiagnoser]
public class JsonParsingBenchmarks
{
    private string _smallDataset = null!;   // 10 records per type (40 total)
    private string _largeDataset = null!;   // 50 records per type (200 total)
    private HealthDataParserService _parser = null!;

    [GlobalSetup]
    public void Setup()
    {
        _parser = new HealthDataParserService();
        _smallDataset = BuildHealthJson(10, 10, 10, 10);
        _largeDataset = BuildHealthJson(50, 50, 50, 50);
    }

    [Benchmark(Baseline = true)]
    public Task<HealthDataCollection> Parse40Records() => _parser.ParseJsonAsync(_smallDataset);

    [Benchmark]
    public Task<HealthDataCollection> Parse200Records() => _parser.ParseJsonAsync(_largeDataset);

    private static string BuildHealthJson(int sleepCount, int hrCount, int spo2Count, int stepsCount)
    {
        var sb = new StringBuilder(8192);
        sb.Append('{');

        sb.Append("\"sleep\":[");
        for (int i = 0; i < sleepCount; i++)
        {
            if (i > 0) sb.Append(',');
            var date = new DateTime(2025, 1, 1).AddDays(i);
            sb.Append('{')
              .Append($"\"recordDate\":\"{date:yyyy-MM-dd}\",")
              .Append("\"deviceId\":\"zepp-smartwatch-001\",")
              .Append($"\"sleepStart\":\"{date.AddDays(-1).AddHours(22):O}\",")
              .Append($"\"sleepEnd\":\"{date.AddHours(6):O}\",")
              .Append("\"durationMinutes\":480,")
              .Append("\"deepSleepMinutes\":90,")
              .Append("\"lightSleepMinutes\":240,")
              .Append("\"remSleepMinutes\":120,")
              .Append("\"awakeMinutes\":30,")
              .Append("\"score\":82")
              .Append('}');
        }
        sb.Append("],");

        sb.Append("\"heartRate\":[");
        for (int i = 0; i < hrCount; i++)
        {
            if (i > 0) sb.Append(',');
            var date = new DateTime(2025, 1, 1).AddDays(i);
            sb.Append('{')
              .Append($"\"recordDate\":\"{date:yyyy-MM-dd}\",")
              .Append("\"deviceId\":\"zepp-smartwatch-001\",")
              .Append("\"minimumBpm\":52,")
              .Append("\"maximumBpm\":142,")
              .Append("\"averageBpm\":72,")
              .Append("\"measurementCount\":1440,")
              .Append("\"restingBpm\":56,")
              .Append("\"stressLevel\":28")
              .Append('}');
        }
        sb.Append("],");

        sb.Append("\"spO2\":[");
        for (int i = 0; i < spo2Count; i++)
        {
            if (i > 0) sb.Append(',');
            var date = new DateTime(2025, 1, 1).AddDays(i);
            sb.Append('{')
              .Append($"\"recordDate\":\"{date:yyyy-MM-dd}\",")
              .Append("\"deviceId\":\"zepp-smartwatch-001\",")
              .Append("\"minimumPercentage\":94,")
              .Append("\"maximumPercentage\":99,")
              .Append("\"averagePercentage\":97,")
              .Append("\"measurementCount\":8,")
              .Append("\"restingPercentage\":98,")
              .Append("\"lowSpO2Events\":0")
              .Append('}');
        }
        sb.Append("],");

        sb.Append("\"steps\":[");
        for (int i = 0; i < stepsCount; i++)
        {
            if (i > 0) sb.Append(',');
            var date = new DateTime(2025, 1, 1).AddDays(i);
            sb.Append('{')
              .Append($"\"recordDate\":\"{date:yyyy-MM-dd}\",")
              .Append("\"deviceId\":\"zepp-smartwatch-001\",")
              .Append("\"totalSteps\":9847,")
              .Append("\"distanceKm\":7.4,")
              .Append("\"caloriesBurned\":412,")
              .Append("\"dailyGoal\":10000,")
              .Append("\"activeMinutes\":68")
              .Append('}');
        }
        sb.Append(']');

        sb.Append('}');
        return sb.ToString();
    }
}
