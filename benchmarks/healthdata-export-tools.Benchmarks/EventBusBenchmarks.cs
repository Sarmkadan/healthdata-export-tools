using BenchmarkDotNet.Core.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Engines;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using System.Globalization;

namespace healthdata_export_tools.Benchmarks
{
    [MemoryDiagnoser]
    public class EventBusBenchmarks
    {
        private readonly ILogger<EventBusBenchmarks> _logger;
        public EventBusBenchmarks(ILogger<EventBusBenchmarks> logger)
        {
            _logger = logger;
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void PublishEvent_Benchmark()
        {
            // Setup
            var ev = new Event();
            var evs = new List<Event>>();
            for (int i = 0; i < 1000; i++)
            {
                evs.Add(new Event());
            }

            // Benchmark
            var summary = new BenchmarkSummary(
                new Benchmark(Description: "PublishEvent", Job: Job.Medium),
                new[] { ev, evs }
            );

            summary.Run();
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void PublishEvent_Benchmark_Params()
        {
            // Setup
            var ev = new Event();
            var evs = new List<Event>>();
            for (int i = 0; i < 1000; i++)
            {
                evs.Add(new Event());
            }

            // Benchmark
            var summary = new BenchmarkSummary(
                new Benchmark(Description: "PublishEvent", Job: Job.Medium),
                new[] { ev, evs }
            );

            summary.Run();
        }
    }
}