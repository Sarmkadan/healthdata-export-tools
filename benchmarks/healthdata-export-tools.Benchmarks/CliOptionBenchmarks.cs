using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Engines;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace healthdata_export_tools.Benchmarks
{
    [MemoryDiagnoser]
    public class CliOptionsBenchmarks
    {
        [GlobalSetup]
        public void Setup()
        {
            // TODO: set up test data
        }

        [Params(10)]
        [Benchmark]
        public void Benchmark_CliOptions_Method1()
        {
            // TODO: implement benchmark for CliOptions.Method1
        }

        [Params(100)]
        [Benchmark]
        public void Benchmark_CliOptions_Method2()
        {
            // TODO: implement benchmark for CliOptions.Method2
        }

        [Params(1000)]
        [Benchmark]
        public void Benchmark_CliOptions_Method3()
        {
            // TODO: implement benchmark for CliOptions.Method3
        }
    }
}
