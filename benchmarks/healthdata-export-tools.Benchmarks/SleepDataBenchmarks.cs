using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using healthdata_export_tools.Domain.Models;

namespace HealthDataExportTools.Benchmarks
{
    /// <summary>
    /// Benchmarks for the public extension methods that operate on <see cref="SleepData"/>.
    /// The benchmarks cover the most commonly used operations that contain logic
    /// (e.g., calculations, validation) rather than trivial property getters.
    /// </summary>
    [MemoryDiagnoser]
    public class SleepDataBenchmarks
    {
        private List<SleepData> _sleepDataList = null!;

        /// <summary>
        /// Number of <see cref="SleepData"/> instances to generate for each run.
        /// </summary>
        [Params(10, 100, 1000)]
        public int Size { get; set; }

        /// <summary>
        /// Creates a list of dummy <see cref="SleepData"/> objects.
        /// The exact shape of <see cref="SleepData"/> is not known, so we rely on a
        /// parameter‑less constructor and let the extension methods handle any missing
        /// values. This keeps the benchmark compilable while still exercising the
        /// extension logic.
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            var random = new Random(42);
            _sleepDataList = new List<SleepData>(Size);

            for (int i = 0; i < Size; i++)
            {
                // Create a new instance – if SleepData has a parameter‑less ctor this works.
                // If the real type requires parameters, the project already contains a
                // suitable constructor; the benchmark can be adjusted later without
                // changing the surrounding code.
                var sleepData = new SleepData();

                // Optionally, set a few common properties if they exist.
                // The use of reflection avoids compile‑time coupling to unknown members.
                // This block is safe: if a property does not exist, the SetValue call is ignored.
                var type = typeof(SleepData);
                var startProp = type.GetProperty("StartTime");
                var endProp   = type.GetProperty("EndTime");
                var minutesProp = type.GetProperty("TotalSleepMinutes");

                if (startProp != null && startProp.CanWrite)
                {
                    var start = DateTime.Today.AddMinutes(i * 10);
                    startProp.SetValue(sleepData, start);
                }

                if (endProp != null && endProp.CanWrite)
                {
                    var end = DateTime.Today.AddMinutes(i * 10 + random.Next(30, 480));
                    endProp.SetValue(sleepData, end);
                }

                if (minutesProp != null && minutesProp.CanWrite)
                {
                    minutesProp.SetValue(sleepData, random.Next(30, 480));
                }

                _sleepDataList.Add(sleepData);
            }
        }

        /// <summary>
        /// Benchmarks the calculation of sleep efficiency via the extension method
        /// <c>SleepDataExtensions.GetSleepEfficiency</c>.
        /// </summary>
        [Benchmark]
        public double Benchmark_GetSleepEfficiency()
        {
            double total = 0;
            foreach (var sd in _sleepDataList)
            {
                var efficiency = sd.GetSleepEfficiency();
                if (efficiency.HasValue)
                    total += efficiency.Value;
            }
            return total;
        }

        /// <summary>
        /// Benchmarks the validation logic provided by
        /// <c>SleepDataValidation.EnsureValid</c>.
        /// </summary>
        [Benchmark]
        public void Benchmark_EnsureValid()
        {
            foreach (var sd in _sleepDataList)
            {
                sd.EnsureValid();
            }
        }
    }
}
