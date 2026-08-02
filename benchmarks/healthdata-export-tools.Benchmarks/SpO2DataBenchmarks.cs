using System;
using System.Collections.Generic;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using healthdata_export_tools.Domain.Models;

namespace healthdata_export_tools.Benchmarks
{
    [MemoryDiagnoser]
    public class SpO2DataBenchmarks
    {
        // Size of the test data set
        [Params(10, 100, 1000)]
        public int Size;

        // Test data
        private SpO2Data[] _data;
        private string[] _jsonStrings;

        // JSON serializer options (use default options)
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        [GlobalSetup]
        public void GlobalSetup()
        {
            // Create an array of SpO2Data objects
            _data = new SpO2Data[Size];
            for (int i = 0; i < Size; i++)
            {
                // Use Activator to create an instance (assumes a public parameterless constructor)
                _data[i] = Activator.CreateInstance<SpO2Data>();
            }

            // Pre-serialize the data to JSON strings for the FromJson benchmarks
            _jsonStrings = new string[Size];
            for (int i = 0; i < Size; i++)
            {
                _jsonStrings[i] = JsonSerializer.Serialize(_data[i], _jsonOptions);
            }
        }

        [Benchmark]
        public void Benchmark_ToJson()
        {
            // Serialize each SpO2Data instance to JSON
            for (int i = 0; i < Size; i++)
            {
                string json = JsonSerializer.Serialize(_data[i], _jsonOptions);
                // The result is not used; we just want to measure serialization cost
                if (json == null) throw new InvalidOperationException();
            }
        }

        [Benchmark]
        public void Benchmark_FromJson()
        {
            // Deserialize each JSON string back to a SpO2Data instance
            for (int i = 0; i < Size; i++)
            {
                SpO2Data? obj = JsonSerializer.Deserialize<SpO2Data>(_jsonStrings[i], _jsonOptions);
                if (obj == null) throw new InvalidOperationException();
            }
        }

        [Benchmark]
        public void Benchmark_TryFromJson()
        {
            // Use JsonSerializer.TryDeserialize (available in .NET 7+)
            // If not available, fallback to normal Deserialize
            for (int i = 0; i < Size; i++)
            {
                bool success = JsonSerializer.TryDeserialize(_jsonStrings[i], typeof(SpO2Data), _jsonOptions, out object? result);
                if (!success || result == null) throw new InvalidOperationException();
            }
        }
    }
}
