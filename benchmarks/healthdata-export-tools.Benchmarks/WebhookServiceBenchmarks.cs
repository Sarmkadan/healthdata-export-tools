using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using healthdata_export_tools.Integration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace healthdata_export_tools.Benchmarks
{
    [MemoryDiagnoser]
    public class WebhookServiceBenchmarks
    {
        private WebhookService _webhookService;
        private List<Webhook> _webhooks;

        [GlobalSetup]
        public void Setup()
        {
            _webhookService = new WebhookService();
            _webhooks = new List<Webhook>();
            for (int i = 0; i < 1000; i++)
            {
                _webhooks.Add(new Webhook { Id = i, Url = $"https://example.com/{i}" });
            }
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void GetRegisteredWebhooks_Benchmark(int count)
        {
            var webhooks = _webhookService.GetRegisteredWebhooks().Take(count).ToList();
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void RegisterWebhook_Benchmark(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _webhookService.RegisterWebhook(_webhooks[i]);
            }
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void UnregisterWebhook_Benchmark(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _webhookService.UnregisterWebhook(_webhooks[i].Id);
            }
        }
    }
}
