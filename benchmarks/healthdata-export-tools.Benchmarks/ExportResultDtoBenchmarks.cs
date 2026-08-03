using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using HealthDataExportTools.DTOs;

namespace healthdata_export_tools.Benchmarks
{
    [MemoryDiagnoser]
    public class ExportResultDtoBenchmarks
    {
        [Params(10, 100, 1000)]
        public int InputSize;

        private List<ExportResultDto> _results;

        [GlobalSetup]
        public void Setup()
        {
            _results = new List<ExportResultDto>(InputSize);
            for (int i = 0; i < InputSize; i++)
            {
                // Create a simple ExportResultDto instance.
                // Assuming a parameterless constructor exists.
                var dto = new ExportResultDto();
                _results.Add(dto);
            }
        }

        [Benchmark]
        public double Benchmark_GetSuccessRate()
        {
            double total = 0;
            foreach (var dto in _results)
            {
                total += dto.GetSuccessRate();
            }
            return total / _results.Count;
        }

        [Benchmark]
        public bool Benchmark_IsSuccessful()
        {
            bool all = true;
            foreach (var dto in _results)
            {
                all &= dto.IsSuccessful();
            }
            return all;
        }

        [Benchmark]
        public bool Benchmark_HasIssues()
        {
            bool any = false;
            foreach (var dto in _results)
            {
                any |= dto.HasIssues();
            }
            return any;
        }

        [Benchmark]
        public int Benchmark_CountSuccesses()
        {
            int count = 0;
            foreach (var dto in _results)
            {
                if (dto.IsSuccessful())
                {
                    count++;
                }
            }
            return count;
        }
    }
}
