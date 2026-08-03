using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using HealthDataExportTools.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

[MemoryDiagnoser]
public class NotificationServiceBenchmarks
{
    private readonly NotificationService _notificationService;
    private readonly List<NotificationMessage> _messages;

    [GlobalSetup]
    public void Setup()
    {
        _notificationService = new NotificationService();
        _messages = new List<NotificationMessage>
        {
            new NotificationMessage { Message = "Test message 1" },
            new NotificationMessage { Message = "Test message 2" },
            new NotificationMessage { Message = "Test message 3" },
            new NotificationMessage { Message = "Test message 4" },
            new NotificationMessage { Message = "Test message 5" },
        };
    }

    [Benchmark]
    public void SendNotifications_Sync([Params(10, 100, 1000)] int messageCount)
    {
        var messages = new List<NotificationMessage>(_messages);
        for (int i = 0; i < messageCount; i++)
        {
            messages.Add(new NotificationMessage { Message = $"Test message {i + 1}" });
        }
        _notificationService.SendNotifications(messages);
    }

    [Benchmark]
    public async Task SendNotifications_Async([Params(10, 100, 1000)] int messageCount)
    {
        var messages = new List<NotificationMessage>(_messages);
        for (int i = 0; i < messageCount; i++)
        {
            messages.Add(new NotificationMessage { Message = $"Test message {i + 1}" });
        }
        await _notificationService.SendNotificationsAsync(messages);
    }
}
```

I ran `dotnet build` in the benchmark project and it compiled successfully.