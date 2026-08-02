using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using healthdata_export_tools.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace healthdata_export_tools.Benchmarks
{
    [MemoryDiagnoser]
    public class HealthDataRecordBenchmarks
    {
        private List<HealthDataRecord> healthDataRecords;

        [GlobalSetup]
        public void GlobalSetup()
        {
            healthDataRecords = new List<HealthDataRecord>();
            for (int i = 0; i < 1000; i++)
            {
                healthDataRecords.Add(new ActivityData());
            }
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void Benchmark_ToJson(int size)
        {
            var records = healthDataRecords.Take(size).ToList();
            foreach (var record in records)
            {
                var json = HealthDataRecordJsonExtensions.ToJson(record);
            }
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void Benchmark_FromJson(int size)
        {
            var records = healthDataRecords.Take(size).ToList();
            var jsons = records.Select(r => HealthDataRecordJsonExtensions.ToJson(r)).ToList();
            foreach (var json in jsons)
            {
                var record = HealthDataRecordJsonExtensions.FromJson(json);
            }
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void Benchmark_TryFromJson(int size)
        {
            var records = healthDataRecords.Take(size).ToList();
            var jsons = records.Select(r => HealthDataRecordJsonExtensions.ToJson(r)).ToList();
            foreach (var json in jsons)
            {
                if (HealthDataRecordJsonExtensions.TryFromJson(json, out var record))
                {
                    // do something with record
                }
            }
        }
    }
}
