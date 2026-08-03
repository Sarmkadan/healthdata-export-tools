using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Diagnostics.Memory;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using healthdata_export_tools;
using healthdata_export_tools.Middleware;
using healthdata_export_tools.Middleware.ErrorHandling;

namespace healthdata_export_tools.Benchmarks
{
    [MemoryDiagnoser]
    public class ErrorHandlingMiddlewareBenchmarks
    {
        [GlobalSetup]
        public void Setup()
        {
            // Setup test data
        }

        [Params(10)]
        [Benchmark]
        public void Benchmark_Method1()
        {
            // Benchmark method 1
        }

        [Params(100)]
        [Benchmark]
        public void Benchmark_Method2()
        {
            // Benchmark method 2
        }

        [Params(1000)]
        [Benchmark]
        public void Benchmark_Method3()
        {
            // Benchmark method 3
        }
    }
}
