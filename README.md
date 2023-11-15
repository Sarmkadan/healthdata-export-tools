// Health Data Export Tools

...

## WebhookServiceExtensions

The `WebhookServiceExtensions` class provides extension methods for managing webhook integrations, including registration, status checks, statistics, and event triggering. It enables grouping webhooks by event type, clearing failed deliveries, and validating active subscriptions.

### Usage Example

```csharp
using HealthDataExportTools.Integration;

var service = new WebhookService();
var webhooks = new List<Webhook>
{
    new Webhook { Url = "https://example.com/webhook1", EventType = "health_data_imported", Status = "active" },
    new Webhook { Url = "https://example.com/webhook2", EventType = "sleep_analysis_complete", Status = "inactive" }
};

// Register webhooks and get count
int registeredCount = service.RegisterWebhooks(webhooks);

// Check if event has active webhook
bool hasActive = service.HasActiveWebhookForEvent("health_data_imported");

// Get statistics
var stats = service.GetWebhookStatistics(); // (total: 2, active: 1, success: 0, failure: 0)

// Find webhooks by URL
var found = service.FindWebhooksByUrl("https://example.com/webhook1");

// Clear failed webhooks
int clearedCount = service.ClearFailedWebhooks();

// Group webhooks by event type
var grouped = service.GetWebhooksGroupedByEventType();

// Trigger webhooks for specific event
await service.TriggerWebhooksAsync("health_data_imported");
```

## DataComparisonServiceTestsExtensions

...
